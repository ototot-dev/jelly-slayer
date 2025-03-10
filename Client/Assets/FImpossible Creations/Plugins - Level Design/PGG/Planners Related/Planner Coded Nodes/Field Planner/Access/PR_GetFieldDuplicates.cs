﻿using FIMSpace.Graph;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.Generating.Planning.PlannerNodes.Field.Access
{

    public class PR_GetFieldDuplicates : PlannerRuleBase
    {
        public override string GetDisplayName(float maxWidth = 120) { return wasCreated ? "Get Field Instances" : "Get Field Planner Instances \\ Duplicates"; }
        public override string GetNodeTooltipDescription { get { return "Get all duplicate fields of provided field"; } }
        public override EPlannerNodeType NodeType { get { return EPlannerNodeType.ReadData; } }
        public override Color GetNodeColor() { return new Color(1.0f, 0.75f, 0.25f, 0.9f); }
        public override Vector2 NodeSize { get { return new Vector2(186, 101); } }
        public override bool DrawInputConnector { get { return false; } }
        public override bool DrawOutputConnector { get { return false; } }

        [HideInInspector][Port(EPortPinType.Input, EPortNameDisplay.Default, 1)] public PGGPlannerPort Planner;
        [HideInInspector][Port(EPortPinType.Output, true)] public PGGPlannerPort MultiplePlanners;
        [Tooltip("Including root instance of the child instances")]
        public bool IncludeSelf = true;
        public override void OnStartReadingNode()
        {
            if (CurrentExecutingPlanner == null) return;
            MultiplePlanners.Clear();

            FieldPlanner rootPlanner = null;

                rootPlanner = GetPlannerFromPort(Planner);
                if (rootPlanner.Available == false) rootPlanner = null;

            if (rootPlanner == null) return;

            List<FieldPlanner> planners = new List<FieldPlanner>();
            if (IncludeSelf) planners.Add(rootPlanner);

            var dupl = rootPlanner.GetDuplicatesPlannersList();

            if (dupl != null)
                for (int p = 0; p < dupl.Count; p++)
                {
                    var duplicate = dupl[p];
                    if (duplicate == null) continue;
                    if (!duplicate.Available) continue;
                    planners.Add(duplicate);
                }

            MultiplePlanners.Output_Provide_PlannersList(planners);
        }


#if UNITY_EDITOR
        SerializedProperty sp = null;
        public override void Editor_OnNodeBodyGUI(ScriptableObject setup)
        {
            base.Editor_OnNodeBodyGUI(setup);
            Planner.Editor_DisplayVariableName = false;

            if (sp == null) sp = baseSerializedObject.FindProperty("Planner");
            SerializedProperty s = sp.Copy();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(s, GUILayout.Width(NodeSize.x - 80));
            GUILayout.EndHorizontal();

            s.Next(false);
            GUILayout.Space(-21);
            GUILayout.BeginHorizontal();
            GUILayout.Space(-27);
            EditorGUILayout.PropertyField(s, GUIContent.none);
            GUILayout.EndHorizontal();
        }
#endif

    }
}