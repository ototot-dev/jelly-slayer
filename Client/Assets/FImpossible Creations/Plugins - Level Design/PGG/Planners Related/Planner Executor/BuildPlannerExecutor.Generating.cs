﻿#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using FIMSpace.Generating.Planning;
using FIMSpace.Generating.Planning.GeneratingLogics;
using System.Collections;

namespace FIMSpace.Generating
{
    public partial class BuildPlannerExecutor : MonoBehaviour
    {

        public bool IsGenerating { get; private set; } = false;
        public bool GeneratingCompleted { get { return GeneratingProgress >= 1; } }
        public float GeneratingProgress { get { if (instantiationProgress != null) return instantiationProgress.Value * _progressMul; if (generatingInstance == null) return 0f; return generatingInstance.GeneratingProgress * _progressMul; } }
        public float GeneratingProgressSmooth { get { if (instantiationProgress != null) return instantiationProgress.Value * _progressMul; if (generatingInstance == null) return 0f; return generatingInstance.GeneratingProgressSmooth * _progressMul; } }
        private bool willInstantiateInCoroutine = false;
        private float _progressMul { get { if (instantiationProgress != null) return 1f; if (willInstantiateInCoroutine) return 0.5f; else return 1f; } }
        private float? instantiationProgress = null;

        /// <summary> Generated Painters or Flexible Painters </summary>
        public List<PGGGeneratorRoot> GeneratedGenerators { get { return generatedGenerators; } }
        [SerializeField, HideInInspector] private List<PGGGeneratorRoot> generatedGenerators = new List<PGGGeneratorRoot>();

        public PlanGenerationPrint GeneratedPreview { get; private set; } = null;
        [SerializeField][HideInInspector] private List<GridPainter> generatedPainters = new List<GridPainter>();

        public List<GridPainter> GeneratedPainters { get { return generatedPainters; } }
        [SerializeField][HideInInspector] private List<FlexibleGenerator> generatedFlexiblePainters = new List<FlexibleGenerator>();
        public List<FlexibleGenerator> GeneratedFlexiblePainters { get { return generatedFlexiblePainters; } }

        BuildPlannerPreset generatingInstance = null;

        public System.Random GeneratingRandom { get; private set; } = null;

        public void ClearGenerated()
        {
            if (generatingInstance) generatingInstance.ClearGeneration();

            for (int i = _generated.Count - 1; i >= 0; i--)
            {
                if (_generated[i] == null) continue;
                FGenerators.DestroyObject(_generated[i]);
            }

            if (generatedGenerators == null) generatedGenerators = new List<PGGGeneratorRoot>();

            _generated.Clear();
            GeneratedPreview = null;
            GeneratedGenerators.Clear();
            GeneratedPainters.Clear();
            GeneratedFlexiblePainters.Clear();
            if (BuildPlannerReferences != null) BuildPlannerReferences.Clear();
            latestGeneratedPreviewSeed = null;
        }




        public void GeneratePreview()
        {
            latestGeneratedPreviewSeed = null;
            generatingSet = EGenerating.JustPreview;

            if (GeneratedPreview != null) ClearGenerated();

            IsGenerating = true;

            if (RandomSeed) Seed = Random.Range(-9999, 9999);
            GeneratingRandom = new System.Random(Seed);

            if (BuildPlannerReferences != null) BuildPlannerReferences.Clear();
            else BuildPlannerReferences = new List<BuildPlannerReference>();


            RefreshVariablesReferences();
            generatingInstance = BuildPlannerPreset.DeepCopy();
            generatingInstance.AsyncGenerating = Async;


            for (int i = 0; i < generatingInstance.BuildVariables.Count; i++)
            {
                generatingInstance.BuildVariables[i].SetValue(_plannerPrepare.PlannerVariablesOverrides[i].GetValue());
            }

            AdjustDuplicatesCounts();

            for (int p = 0; p < generatingInstance.BasePlanners.Count; p++) AdjustTargetDuplicatesCount(p);


            for (int p = 0; p < generatingInstance.BasePlanners.Count; p++)
            {
                var plannerInst = generatingInstance.BasePlanners[p];

                if (plannerInst.FieldType == FieldPlanner.EFieldType.InternalField) continue;

                var tgtCompos = _plannerPrepare.FieldSetupCompositions[p];

                plannerInst.DefaultFieldSetup = tgtCompos.Setup;

                //plannerInst.CheckerScale = tgtCompos.GetCellSize();
                plannerInst.PreviewCellSize = tgtCompos.GetCellSize();

                if (tgtCompos.GenType == EPGGGenType.Prefab)
                {
                    tgtCompos.PrefabFieldHandler.PreparePlannerInstance(plannerInst);
                }
                else if (plannerInst.FieldType == FieldPlanner.EFieldType.Prefab) plannerInst.FieldType = FieldPlanner.EFieldType.FieldPlanner;

                //plannerInst.UseCheckerScale = true;
                plannerInst.DisableWholePlanner = !tgtCompos.OverrideEnabled;
                plannerInst.ShapeGenerator = tgtCompos.InitShapes[0];

                // Get override shape if enabled
                var newShape = GetShapeFor(plannerInst);
                if (newShape != null) plannerInst.ShapeGenerator = newShape;

                plannerInst.Instances = tgtCompos.Instances;

                for (int v = 0; v < plannerInst.Variables.Count; v++)
                {
                    plannerInst.Variables[v].SetValue(tgtCompos.PlannerVariablesOverrides[v]);
                }
            }


            var manager = generatingInstance.RunProceduresAndGeneratePrint(Seed);

            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                if (_plannerPrepare.UseDuplicatesOverrides[i] == false) continue;
                var overr = _plannerPrepare.DuplicatesOverrides[i];

                // Duplicate instances are generated during planner execution
                // so we can get reference to them through the event
                manager.GetAsyncInstances[i].OnDuplicatesGenerated = (FieldPlanner p) =>
                {
                    var dups = p.GetDuplicatesPlannersList();

                    if (dups != null)
                    {
                        for (int d = 0; d < dups.Count; d++)
                        {
                            #region Supporting duplicates overrides compositions

                            var overrComp = overr.DuplicatesCompositions[d];

                            if (overrComp.OverrideCellSize)
                            {
                                var dupPlan = dups[d];
                                dupPlan.PreviewCellSize = overrComp.GetCellSize();
                                //dupPlan.CheckerScale = overrComp.GetCellSize();
                                //UnityEngine.Debug.Log("set scale on " + p + " to " + overrComp.OverridingCellSize);
                            }

                            var shapeGen = GetShapeFor(dups[d]);
                            if (shapeGen != null) dups[d].ShapeGenerator = shapeGen;

                            #endregion
                        }
                    }
                };
            }


            #region Playmode Generating Progress Invoke

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                updateGenerating = true;
            }

            #endregion

        }

        /// <summary> I was using invoke repeating, but no matter what, it was influenced by time.timeScale </summary>
        bool updateGenerating = false;

        private void Update()
        {
            if (!updateGenerating) return;
            UpdateGeneratingProgress();
        }

        PlannerDuplicatesSupport GetSupportFor(FieldPlanner planner)
        {
            if (planner.IndexOnPreset >= _plannerPrepare.DuplicatesOverrides.Count) return null;
            return _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset];
        }


        ShapeGeneratorBase GetShapeFor(FieldPlanner planner)
        {
            if (planner.IndexOnPreset < 0 || planner.IndexOnPreset >= _plannerPrepare.FieldSetupCompositions.Count)
            {
                return null;
            }

            if (_plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset].InitShapes == null || _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset].InitShapes.Count == 0)
            {
                _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset].RefreshPlannerShapesSupport(planner);
            }

            ShapeGeneratorBase deflt = _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset].InitShapes[0];

            if (planner.IndexOfDuplicate < 0 || planner.IndexOfDuplicate >= _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesShapes.Count)
            {
                return deflt;
            }

            ShapeGeneratorBase gen = _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesShapes[planner.IndexOfDuplicate];
            if (gen == null)
            {
                return deflt;
            }

            return gen;
        }


        FieldSetupComposition GetCompositionFor(FieldPlanner planner)
        {
            if (planner.IndexOnPreset >= _plannerPrepare.DuplicatesOverrides.Count) return null;
            FieldSetupComposition deflt = _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset];

            if (planner.IndexOfDuplicate < 0 || planner.IndexOfDuplicate >= _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions.Count) return deflt;

            FieldSetupComposition gen = _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions[planner.IndexOfDuplicate];
            if (gen == null) return deflt;
            return gen;
        }




        #region Generating Field Setups

        enum EGenerating { JustPreview, Base, GridPainters, FlexiblePainters }
        private EGenerating generatingSet = EGenerating.Base;
        private int? latestGeneratedPreviewSeed = null;
        public void Generate()
        {
            EnsureFieldsInitialization();

            ClearGenerated();
            GeneratePreview();

            if (FlexibleGen == false)
                generatingSet = EGenerating.GridPainters;
            else
            {
                FlexiblePaintersGeneratorsDone = false;
                generatingSet = EGenerating.FlexiblePainters;
            }

            willInstantiateInCoroutine = false;
            if (generatingSet == EGenerating.FlexiblePainters) willInstantiateInCoroutine = true;
        }

        /// <summary> Added to prevent wrong generating for prefabed executors </summary>
        public void EnsureFieldsInitialization()
        {
            ValidateSetups();
            ResetPlannerComposition();
            RefreshVariablesReferences();

            if (BuildPlannerPreset == null) return;

            for (int i = 0; i < PlannerPrepare.FieldSetupCompositions.Count; i++)
            {
                var compos = PlannerPrepare.FieldSetupCompositions[i];
                if (compos.Prepared) continue;
                compos.PrepareWithCurrentlyChoosed(this, compos.ParentFieldPlanner, false);
            }
        }

        /// <summary>
        /// Mainly for refreshing variable value with scene transform Vector3 position feature
        /// </summary>
        public void RefreshVariablesReferences()
        {
            for (int i = 0; i < _plannerPrepare.PlannerVariablesOverrides.Count; i++)
            {
                Transform trs = null;
                if (_plannerPrepare.PlannerVariablesOverrides[i].allowTransformFollow)
                    if (_plannerPrepare.PlannerVariablesOverrides[i].additionalHelperRef)
                    {
                        trs = _plannerPrepare.PlannerVariablesOverrides[i].additionalHelperRef as Transform;
                        if (trs)
                        {
                            _plannerPrepare.PlannerVariablesOverrides[i].SetValue(transform.InverseTransformPoint(trs.position));
                        }
                    }
            }

            for (int p = 0; p < _plannerPrepare.FieldSetupCompositions.Count; p++)
            {
                var selected = _plannerPrepare.FieldSetupCompositions[p];

                for (int i = 0; i < selected.PlannerVariablesOverrides.Count; i++)
                {
                    Transform trs = null;
                    if (selected.PlannerVariablesOverrides[i].allowTransformFollow)
                        if (selected.PlannerVariablesOverrides[i].additionalHelperRef)
                        {
                            trs = selected.PlannerVariablesOverrides[i].additionalHelperRef as Transform;
                            if (trs)
                            {
                                selected.PlannerVariablesOverrides[i].SetValue(transform.InverseTransformPoint(trs.position));
                            }
                        }
                }
            }
        }

        public void SwitchFromPreviewGen()
        {
            if (generatingSet == EGenerating.JustPreview) generatingSet = EGenerating.GridPainters;
        }

        public void GenerateGridPainters()
        {
            if (latestGeneratedPreviewSeed == null || latestGeneratedPreviewSeed.Value != Seed)
            {
                GeneratePreview();

                if (FlexibleGen == false)
                    generatingSet = EGenerating.GridPainters;
                else
                    generatingSet = EGenerating.FlexiblePainters;
            }
            else
            {
                RunGeneratePainters();
                if (generatingInstance) generatingInstance.CallOperations_OnExecutorComplete(this);
            }
        }


        private void RunGeneratePainters()
        {
            ValidateSetups(); // Validate compositions

            if (generatingSet == EGenerating.GridPainters)
                ConvertGeneratedSchemeToGridPainters();
            else if (generatingSet == EGenerating.FlexiblePainters)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    StartCoroutine(IConvertGeneratedSchemeToFlexiblePainters());
                else
                    ConvertGeneratedSchemeToFlexiblePainters();
#else
                        StartCoroutine(IConvertGeneratedSchemeToFlexiblePainters());
#endif
            }

            // Generate prefabs if not using grids
            if (generatingSet != EGenerating.JustPreview)
                GeneratedSchemesToPrefabs();
        }

        public void GenerateFieldSetupsOnGeneratedScheme()
        {
            seedIteration = 0;
            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];

                GenerateWithPlanner(planner);

                var duplicates = planner.GetDuplicatesPlannersList();

                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        GenerateWithPlanner(duplicates[d]);
                    }
            }
        }

        void GenerateWithPlanner(FieldPlanner planner)
        {

            seedIteration += 1;
        }

        int seedIteration;

        public void ConvertGeneratedSchemeToGridPainters()
        {
            seedIteration = 0;

            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];
                if (planner.DontGenerateIt) continue;
                if (planner.FieldType == FieldPlanner.EFieldType.Prefab) continue;

                GenerateGridPainterWithPlanner(planner);

                var duplicates = planner.GetDuplicatesPlannersList();

                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        if (duplicates[d].DontGenerateIt) continue;
                        GenerateGridPainterWithPlanner(duplicates[d]);
                    }

                for (int s = 0; s < planner.GetSubFieldsCount; s++)
                {
                    var pl = planner.GetSubField(s);
                    if (pl.DontGenerateIt) continue;
                    if (pl.DefaultFieldSetup == null) pl.DefaultFieldSetup = planner.DefaultFieldSetup;
                    GenerateGridPainterWithPlanner(pl);
                }
            }
        }

        public IEnumerator IConvertGeneratedSchemeToFlexiblePainters()
        {
            willInstantiateInCoroutine = true;
            instantiationProgress = 0.5f;

            float instatiationTotal = 0f;

            #region Compute count

            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];
                if (planner.DontGenerateIt) continue;
                instatiationTotal += 1f;

                var duplicates = planner.GetDuplicatesPlannersList();
                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        if (duplicates[d].DontGenerateIt) continue;
                        instatiationTotal += 1f;
                    }

                for (int d = 0; d < planner.GetSubFieldsCount; d++)
                {
                    if (planner.GetSubField(d).DontGenerateIt) continue;
                    instatiationTotal += 1f;
                }
            }

            if (instatiationTotal == 0f) instatiationTotal = 1f;

            #endregion

            float done = 0f;

            seedIteration = 0;
            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];
                if (planner.DontGenerateIt) continue;

                FlexibleGenerator painter = GenerateFlexiblePainterWithPlanner(planner);

                if (painter != null)
                {
                    while (painter.FinishedGenerating == false)
                    {
                        yield return null;
                        instantiationProgress = 0.5f + 0.5f * ((done + painter.GeneratingProgress) / instatiationTotal);
                        generatingInstance.OverrideProgressDisplay(painter.GeneratingProgress);
                    }
                }

                done += 1f;

                var duplicates = planner.GetDuplicatesPlannersList();

                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        if (duplicates[d].DontGenerateIt) continue;

                        painter = GenerateFlexiblePainterWithPlanner(duplicates[d]);

                        if (painter != null)
                        {
                            while (painter.FinishedGenerating == false)
                            {
                                yield return null;
                                instantiationProgress = 0.5f + 0.5f * ((done + painter.GeneratingProgress) / instatiationTotal);
                                generatingInstance.OverrideProgressDisplay(painter.GeneratingProgress);
                            }
                        }

                        done += 1f;
                    }


                for (int d = 0; d < planner.GetSubFieldsCount; d++)
                {
                    var subF = planner.GetSubField(d);
                    if (subF.DontGenerateIt) continue;
                    if (subF.DefaultFieldSetup == null) subF.DefaultFieldSetup = planner.DefaultFieldSetup;

                    painter = GenerateFlexiblePainterWithPlanner(subF);

                    if (painter != null)
                    {
                        while (painter.FinishedGenerating == false)
                        {
                            yield return null;
                            instantiationProgress = 0.5f + 0.5f * ((done + painter.GeneratingProgress) / instatiationTotal);
                            generatingInstance.OverrideProgressDisplay(painter.GeneratingProgress);
                        }
                    }

                    done += 1f;
                }

            }


            instantiationProgress = 1f;
        }

        public void ConvertGeneratedSchemeToFlexiblePainters()
        {
            seedIteration = 0;
            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];
                if (planner.DontGenerateIt) continue;
                if (planner.FieldType == FieldPlanner.EFieldType.Prefab) continue;

                FlexibleGenerator painter = GenerateFlexiblePainterWithPlanner(planner);
                var duplicates = planner.GetDuplicatesPlannersList();

                if (duplicates != null)
                    for (int d = 0; d < duplicates.Count; d++)
                    {
                        if (duplicates[d].DontGenerateIt) continue;

                        painter = GenerateFlexiblePainterWithPlanner(duplicates[d]);
                        if (painter != null) generatingInstance.OverrideProgressDisplay(painter.GeneratingProgress);
                    }
            }
        }

        void GenerateGridPainterWithPlanner(FieldPlanner planner)
        {
            string name = planner.name;
            name = name.Replace("(Clone)", "");
            if (planner.IndexOfDuplicate >= 0) name += "[" + planner.IndexOfDuplicate + "]";

            GameObject painterObj = new GameObject(name);

            GridPainter painter = painterObj.AddComponent<GridPainter>();

            painter.RandomSeed = false;
            painter.Seed = Seed + seedIteration;
            painter.FieldPreset = planner.DefaultFieldSetup;
            painter.GenerateOnStart = false;

            painter.grid = planner.LatestResult.Checker.Grid;
            painter.CellsInstructions = FGenerators.CopyList(planner.LatestResult.CellsInstructions);

            painter.SaveCells();
            GeneratedPainters.Add(painter);
            generatedGenerators.Add(painter);

            FieldSetupComposition baseComposition = _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset];

            if (baseComposition.GenType == EPGGGenType.Modificator || baseComposition.GenType == EPGGGenType.ModPack)
            {
                if (baseComposition.UseComposition == false || baseComposition.Prepared == false)
                { baseComposition = baseComposition.Copy(); if (baseComposition.GenType == EPGGGenType.Modificator) baseComposition.RefreshModSetup(); else baseComposition.RefreshModPackSetup(); }
                baseComposition.UseComposition = true;
                baseComposition.Prepared = true;
                //UnityEngine.Debug.Log("prepared true");
            }

            if (baseComposition.UseComposition && baseComposition.Prepared)
            {
                painter.Composition = baseComposition;
            }

            if (planner.IndexOnPreset < _plannerPrepare.UseDuplicatesOverrides.Count)
                if (_plannerPrepare.UseDuplicatesOverrides[planner.IndexOnPreset])
                {
                    if (planner.IsDuplicate)
                        if (planner.IndexOfDuplicate < _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions.Count)
                        {
                            var compo = _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions[planner.IndexOfDuplicate];

                            if (compo.UseComposition && compo.Prepared)
                            {
                                if (compo.Setup != null) painter.FieldPreset = compo.Setup;
                                painter.Composition = compo;
                            }
                        }
                }

            painterObj.transform.SetParent(transform, true);
            painterObj.transform.position = transform.TransformPoint(planner.LatestResult.Checker.RootPosition);
            painterObj.transform.rotation = transform.rotation * planner.LatestResult.Checker.RootRotation;

            _generated.Add(painterObj);
            seedIteration += 1;

            /*var genSetup = */
            painter.GetTargetGeneratingSetup();

            if (planner.OnGeneratingEvents != null)
            {
                for (int g = 0; g < planner.OnGeneratingEvents.Count; g++)
                {
                    planner.OnGeneratingEvents[g].Invoke(painter);
                }
            }

            painter.GenerateObjects();
            painterObj.transform.localScale = Vector3.one;

            AddPlannerReference(planner, painter);
            planner.CallOperations_OnSpawningComplete(this, painter);
        }

        FlexibleGenerator GenerateFlexiblePainterWithPlanner(FieldPlanner planner)
        {
            string name = planner.name;
            name = name.Replace("(Clone)", "");
            if (planner.IndexOfDuplicate >= 0) name += "[" + planner.IndexOfDuplicate + "]";

            GameObject painterObj = new GameObject(name);

            FlexibleGenerator painter = painterObj.AddComponent<FlexibleGenerator>();

            painter.CodedUsage = true;
            painter.RandomSeed = false;
            painter.GenerateOnStart = false;
            painter.Seed = Seed + seedIteration;
            painter.DataSetup.FieldPreset = planner.DefaultFieldSetup;
            //painter.AsyncComputing = true;
            //painter.InstantiationMaxSecondsDelay = 0.0001f;
            painter.TestGridSize = Vector2Int.zero;

            GeneratedFlexiblePainters.Add(painter);
            GeneratedGenerators.Add(painter);

            FieldSetupComposition baseComposition = _plannerPrepare.FieldSetupCompositions[planner.IndexOnPreset];

            if (baseComposition.GenType == EPGGGenType.Modificator || baseComposition.GenType == EPGGGenType.ModPack)
            {
                if (baseComposition.UseComposition == false || baseComposition.Prepared == false)
                { baseComposition = baseComposition.Copy(); if (baseComposition.GenType == EPGGGenType.Modificator) baseComposition.RefreshModSetup(); else baseComposition.RefreshModPackSetup(); }
                baseComposition.UseComposition = true; baseComposition.Prepared = true;
                //UnityEngine.Debug.Log("prepared true");
            }

            if (baseComposition.UseComposition && baseComposition.Prepared)
            {
                painter.Composition = baseComposition;
            }

            if (painter.Composition != null)
            {
                if (painter.Composition.Prepared && painter.Composition.OverrideEnabled)
                    painter.DataSetup.FieldPreset = painter.Composition.GetSetup;
            }

            if (planner.IndexOnPreset < _plannerPrepare.UseDuplicatesOverrides.Count)
                if (_plannerPrepare.UseDuplicatesOverrides[planner.IndexOnPreset])
                {
                    if (planner.IsDuplicate)
                        if (planner.IndexOfDuplicate < _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions.Count)
                        {
                            var compo = _plannerPrepare.DuplicatesOverrides[planner.IndexOnPreset].DuplicatesCompositions[planner.IndexOfDuplicate];

                            if (compo.UseComposition && compo.Prepared)
                            {
                                if (compo.Setup != null) painter.DataSetup.FieldPreset = compo.Setup;
                                painter.Composition = compo;
                            }
                        }
                }


            painter.CheckIfInitialized();
            painter.DataSetup.RefreshReferences(painter);
            painter.Cells.Initialize(painter.DataSetup);

            painter.PrepareWithoutGridChanges();
            painter.Cells.SetWithGrid(planner.LatestResult.Checker.Grid);
            painter.Preparation.CellInstructions = FlexiblePainter.CellInstructionsToSpawnInstructions(FGenerators.CopyList(planner.LatestResult.CellsInstructions), painter.DataSetup.FieldPreset, painter.transform);

            painterObj.transform.SetParent(transform, true);
            painterObj.transform.position = transform.TransformPoint(planner.LatestResult.Checker.RootPosition);
            painterObj.transform.rotation = transform.rotation * planner.LatestResult.Checker.RootRotation;


            _generated.Add(painterObj);
            seedIteration += 1;

            painter.GenerateObjects();
            painterObj.transform.localScale = Vector3.one;

            AddPlannerReference(planner, painter);
            planner.CallOperations_OnSpawningComplete(this, painter);

            return painter;
        }


        public void GeneratedSchemesToPrefabs()
        {
            for (int i = 0; i < generatingInstance.BasePlanners.Count; i++)
            {
                var planner = generatingInstance.BasePlanners[i];
                if (planner.DontGenerateIt) continue;
                if (planner.FieldType != FieldPlanner.EFieldType.Prefab) continue;

                InstantiatePrefabField(planner);
                for (int d = 0; d < planner.Duplicates; d++) InstantiatePrefabField(planner.GetDuplicatesPlannersList()[d]);
                for (int s = 0; s < planner.GetSubFieldsCount; s++) InstantiatePrefabField(planner.GetSubField(s));
            }
        }

        void InstantiatePrefabField(FieldPlanner planner)
        {
            if (planner == null) return;
            if (planner.DefaultPrefab == null) return;

            GameObject created = FGenerators.InstantiateObject(planner.DefaultPrefab);
            created.transform.SetParent(transform, true);
            created.transform.localPosition = (planner.LatestResult.Checker.RootPosition);
            created.transform.localRotation = planner.LatestResult.Checker.RootRotation;
            Generated.Add(created);

            planner.CallOperations_OnSpawningComplete(this, null);
        }


        public List<BuildPlannerReference> BuildPlannerReferences { get; private set; }
        void AddPlannerReference(FieldPlanner planner, PGGGeneratorRoot root)
        {
            BuildPlannerReference pRef = root.gameObject.AddComponent<BuildPlannerReference>();
            pRef.ParentExecutor = this;
            pRef.Generator = root;
            pRef.Planner = planner;
            pRef.PlannerName = planner.name.Replace("(Clone)", "");
            pRef.BuildPlannerIndex = planner.IndexOnPreset;
            pRef.BuildPlannerInstanceID = planner.InstanceIndex;
            pRef.GridSpaceBounds = PGG_MinimapUtilities.ComputeGridCellSpaceBounds(root);
            pRef.GridSpaceBounds.center += new Vector3(0f, 0.5f, 0f);
            if (root.PGG_Setup == null)
            { UnityEngine.Debug.Log("null setup"); return; }
            pRef.GridSpaceBounds = PGG_MinimapUtilities.ScaleBoundsWithSetup(pRef.GridSpaceBounds, root.PGG_Setup);

            if (BuildPlannerReferences != null) BuildPlannerReferences.Add(pRef);
        }

        public BuildPlannerReference GetPlannerReferenceByID(Vector3 id)
        {
            if (BuildPlannerReferences == null) return null;
            if (BuildPlannerReferences.Count == 0) return null;

            for (int p = 0; p < BuildPlannerReferences.Count; p++)
            {
                var pRef = BuildPlannerReferences[p];
                if (pRef == null) continue;
                if (pRef.Planner == null) continue;
                if (pRef.Planner.ArrayNameIDVector == id) return pRef;
            }

            return null;
        }


        public BuildPlannerReference GetPlannerReferenceByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            if (id.Contains("[") == false) return null;

            return GetPlannerReferenceByID(StringToPlannerID(id));
        }

        public Vector3 StringToPlannerID(string id)
        {
            Vector3 result = new Vector3(-1, -1, -1);

            int brace = 0;
            string num = "";

            for (int i = 0; i < id.Length; i++)
            {
                if (id[i] == ' ') continue;

                if (id[i] == '[')
                {
                    brace += 1;
                }
                else if (id[i] == ']')
                {
                    int idInt = -1;

                    if (int.TryParse(num, out idInt))
                    {
                        if (brace == 1) result.x = idInt;
                        else if (brace == 2) result.y = idInt;
                        else if (brace == 3) result.z = idInt;
                    }

                    num = "";
                }
                else
                {
                    num += id[i];
                }
            }

            return result;
        }


        #endregion



        #region Generating Progress

        public void UpdateGeneratingProgress()
        {
            if (generatingInstance == null)
                IsGenerating = false;
            else
                generatingInstance.UpdateGenerating();


            if (IsGenerating)
            {
                GeneratedPreview = generatingInstance.LatestGenerated;

                #region Repaint
#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif
                #endregion

                if (generatingInstance.IsGeneratingDone) IsGenerating = false;

            }


            #region Finishing Generating

            if (IsGenerating == false)
            {

                #region Repaint
#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif
                #endregion

                updateGenerating = false;

                latestGeneratedPreviewSeed = Seed;

                if (generatingSet == EGenerating.Base)
                    GenerateFieldSetupsOnGeneratedScheme();
                else
                {
                    RunGeneratePainters();
                }

                if (!FlexibleGen)
                {
                    if (RunAfterGenerating != null) RunAfterGenerating.Invoke();
                    if (generatingInstance) generatingInstance.CallOperations_OnExecutorComplete(this);
                }
                else
                {
                    StartCoroutine(IEWaitForFlexAfterGenerating());
                }
            }

            #endregion

        }

        public bool FlexiblePaintersGeneratorsDone { get; private set; }
        IEnumerator IEWaitForFlexAfterGenerating()
        {
            while (FlexiblePaintersGeneratorsDone == false)
            {
                bool anyNotDone = false;

                for (int f = 0; f < GeneratedFlexiblePainters.Count; f++)
                {
                    var flex = GeneratedFlexiblePainters[f];
                    if (flex == null) continue;
                    if (flex.FinishedGenerating == false) { anyNotDone = true; break; }
                }

                if (anyNotDone)
                    yield return null;
                else
                    FlexiblePaintersGeneratorsDone = true;
            }

            if (generatingInstance) generatingInstance.CallOperations_OnExecutorComplete(this);
            if (RunAfterGenerating != null) RunAfterGenerating.Invoke();
        }


        /// <summary>
        /// Returns the field generator which was generated by the build planner.
        /// </summary>
        public PGGGeneratorRoot GetGeneratedGenerator(string name)
        {
            for (int i = 0; i < generatedGenerators.Count; i++)
            {
                if (generatedGenerators[i].name == name) return generatedGenerators[i];
            }

            return null;
        }

        /// <summary>
        /// Returns first field planner preparation composition with provided field planner name
        /// </summary>
        public FieldSetupComposition GetFieldPlannerSetup(string name)
        {
            for (int p = 0; p < PlannerPrepare.FieldSetupCompositions.Count; p++)
            {
                if (PlannerPrepare.FieldSetupCompositions[p].ParentFieldPlanner.name == name)
                {
                    return PlannerPrepare.FieldSetupCompositions[p];
                }
            }

            return null;
        }

        #endregion


    }

}
