using System;
using System.Collections.Generic;
using System.Linq;
using MainTable;
using UnityEngine;

namespace Game
{
    public class PawnActionDataSelector : MonoBehaviour
    {   
#region ActionSequence 구현
        public class ActionSequence
        {
            public ActionSequence(int hashCode, string sequenceName, params MainTable.ActionData[] sequenceData)
            {
                HashCode = hashCode;
                SequenceName = sequenceName;
                __sequenceData = sequenceData;
                __currIndex = -1;
            }

            public MainTable.ActionData this[int index]
            {
                get => __sequenceData[index];  // 값 가져오기
                set => __sequenceData[index] = value;  // 값 설정하기
            }
            public MainTable.ActionData First() => __sequenceData[0];
            public MainTable.ActionData Last() => __sequenceData[__sequenceData.Length - 1];
            public MainTable.ActionData Curr() => __sequenceData[__currIndex];
            public MainTable.ActionData Next() => ++__currIndex < __sequenceData.Length ? __sequenceData[__currIndex] : null;
            public void Reset() { __currIndex = 0; }
            
            readonly MainTable.ActionData[] __sequenceData;
            int __currIndex;
            public int HashCode { get; private set; }
            public string SequenceName { get; private set; }
        }

        public ActionSequence ReserveSequence<T>(T alias, params string[] actionNames) where T : Enum
        {
            var hashCode = alias.GetHashCode();
            if (__reservedSequences.ContainsKey(hashCode))
            {
                Debug.Assert(false);
                return null;
            }
            __reservedSequences.Add(hashCode, new ActionSequence(alias.GetHashCode(), alias.ToString(), actionNames.Select(n => DatasheetManager.Instance.GetActionData(__pawnBrain.PawnBB.common.pawnId, n)).ToArray()));
            return __reservedSequences[hashCode];
        }

        public ActionSequence GetSequence<T>(T alias) where T : Enum
        {
            if (__reservedSequences.TryGetValue(alias.GetHashCode(), out var ret)) 
                return ret;
            else
                return null;
        }

        public MainTable.ActionData GetSequenceData<T>(T alias, int dataIndex = 0) where T : Enum
        {
            if (__reservedSequences.TryGetValue(alias.GetHashCode(), out var ret)) 
                return ret[dataIndex];
            else
                return null;
        }

        public ActionSequence EnqueueSequence<T>(T alias) where T : Enum
        { 
            var sequence = GetSequence<T>(alias);
            Debug.Assert(sequence != null);

            if (!__sequenceQueue.Contains(sequence))
            {
                sequence.Reset();
                __sequenceQueue.Enqueue(sequence);
                return sequence;
            }
            else
            {
                __Logger.WarningR2(gameObject, nameof(EnqueueSequence), "__actionSequencePatternQueue.Contains() returns true.", "patternAlias", alias);
                return null;
            }
        }

        public ActionData AdvanceSequence()
        {
            if (__sequenceQueue.TryPeek(out var currSequence))
                return currSequence.Next() != null ? currSequence.Curr() : (NextSequence()?.Curr() ?? null);
            else
                return null;
        }

        public ActionSequence CurrSequence() => __sequenceQueue.TryPeek(out var ret) ? ret : null;
        public ActionSequence NextSequence()
        {
            __sequenceQueue.Dequeue();
            return CurrSequence();
        }
        public void ClearSequences() { __sequenceQueue.Clear(); }

        readonly Queue<ActionSequence> __sequenceQueue = new();
        readonly Dictionary<int, ActionSequence> __reservedSequences = new();
#endregion

        public class ActionDataState
        {
            public MainTable.ActionData actionData;
            public float currProb;
            public float currCoolTime;

            public ActionDataState(MainTable.ActionData actionData)
            {
                this.actionData = actionData;

                ResetProbability();
                ResetCoolTime();
            }

            public void IncreaseProbability(float deltaProb)
            {
                currProb += deltaProb;
            }

            public void ResetProbability(float resetProb = 0f)
            {
                currProb = resetProb;
            }

            public float DecreaseCoolTime(float deltaCoolTime)
            {
                Debug.Assert(deltaCoolTime > 0);
                return currCoolTime = Mathf.Max(0, currCoolTime - deltaCoolTime);
            }

            public void ResetCoolTime()
            {
                currCoolTime = actionData.coolTime;
            }
        };

#if UNITY_EDITOR
        public Dictionary<MainTable.ActionData, ActionDataState> ActionDataStates => __actionDataStates;
#endif

        readonly Dictionary<MainTable.ActionData, ActionDataState> __actionDataStates = new();
        readonly HashSet<MainTable.ActionData> __executables = new();
        PawnBrainController __pawnBrain;

        void Awake()
        {
#if UNITY_EDITOR
            DatasheetManager.Instance.Load();
#endif
            if (TryGetComponent<PawnBlackboard>(out var pawnBB))
            {
                var actionData = DatasheetManager.Instance.GetActionData(pawnBB.common.pawnId);
                if (actionData != null)
                {
                    foreach (var d in actionData)
                        __actionDataStates.Add(d, new ActionDataState(d));
                }
            }

            __pawnBrain = GetComponent<PawnBrainController>();
        }

        public MainTable.ActionData GetActionData(string actionName)
        {
            var ret = DatasheetManager.Instance.GetActionData(__pawnBrain.PawnBB.common.pawnId, actionName);
            if (ret == null)
                __Logger.WarningR2(gameObject, nameof(BoostSelection), "DatasheetManager.Instance.GetActionData() return false", "pawnId", __pawnBrain.PawnBB.common.pawnId, "actionName", actionName);

            return ret;
        }

        public bool CheckExecutable(string actionName) => CheckExecutable(GetActionData(actionName));
        public bool CheckExecutable(ActionData actionData) => __executables.Contains(actionData);

        public void UpdateSelection(float deltaTime)
        {            
            foreach (var s in __actionDataStates)
            {
                if (s.Key.coolTime < 0f || (s.Key.coolTime > 0 && s.Value.DecreaseCoolTime(deltaTime) <= 0f))
                    __executables.Add(s.Key);
            }
        }

        public void BoostSelection(string actionName, float boostProb) => BoostSelection(GetActionData(actionName), boostProb);
        public void BoostSelection(MainTable.ActionData actionData, float boostProb)
        {   
            Debug.Assert(actionData != null);

            if (__actionDataStates.TryGetValue(actionData, out var state))
                state.currProb += boostProb;
            else
                __Logger.WarningR2(gameObject, nameof(BoostSelection), "SourceActionStates.TryGetValue() return false", "actionName", actionData.actionName);
        }
        
        public void ResetSelection(string actionName, float resetProb = 0f) => ResetSelection(GetActionData(actionName), resetProb);
        public void ResetSelection(MainTable.ActionData actionData, float resetProb = 0f)
        {   
            Debug.Assert(actionData != null);

            if (__actionDataStates.TryGetValue(actionData, out var state))
            {
                state.ResetProbability(resetProb);
                state.ResetCoolTime();
                __executables.Remove(actionData);
            }
            else
            {
                __Logger.WarningR2(gameObject, nameof(ResetSelection), "SourceActionStates.TryGetValue() return false", "actionName", actionData.actionName);
            }
        }

        public bool EvaluateSelection<T>(T alias, float probConstraint = -1f, float staminaConstraint = -1f) where T : Enum
        {
            return EvaluateSelection(GetSequenceData(alias), probConstraint, staminaConstraint);
        }
        public bool EvaluateSelection<T>(T alias, int dataIndex, float probConstraint = -1f, float staminaConstraint = -1f) where T : Enum
        {
            return EvaluateSelection(GetSequenceData(alias, dataIndex), probConstraint, staminaConstraint);
        }
        
        public bool EvaluateSelection(string actionName, float probConstraint = -1f, float staminaConstraint = -1f) => EvaluateSelection(GetActionData(actionName), probConstraint, staminaConstraint);
        public bool EvaluateSelection(MainTable.ActionData actionData, float probConstraint = -1f, float staminaConstraint = -1f)
        {
            Debug.Assert(actionData != null);

            if (!__executables.Contains(actionData))
                return false;

            if (__actionDataStates.TryGetValue(actionData, out var actionState))
            {
                return actionState.currProb >= probConstraint && (actionData.staminaCost <= staminaConstraint || staminaConstraint < 0f);
            }
            else
            {
                __Logger.WarningR2(gameObject, nameof(EvaluateSelection), "SourceActionStates.TryGetValue() return false", "actionName", actionData.actionName);
                return false;
            }
        }

        public bool TryPickRandomSelection(float probConstraint, float staminaConstraint, out MainTable.ActionData actionData)
        {
            actionData = PickRandomSelection(probConstraint, staminaConstraint);
            return actionData != null;
        }
        public MainTable.ActionData PickRandomSelection(float probConstraint = -1f, float staminaConstraint = -1f)
        {
            var selectRate = UnityEngine.Random.Range(0, __executables.Where(e => __actionDataStates[e].currProb >= probConstraint && (e.staminaCost <= staminaConstraint || staminaConstraint < 0f)).Sum(e => __actionDataStates[e].currProb));
            var accumRate = 0f;
            foreach (var e in __executables)
            {
                if (__actionDataStates[e].currProb < probConstraint)
                    continue;
                if (staminaConstraint >= 0f && e.staminaCost > staminaConstraint)
                    continue;
                if (selectRate >= accumRate && selectRate < accumRate + __actionDataStates[e].currProb)
                    return e;

                accumRate += __actionDataStates[e].currProb;
            }

            return null;
        }
    }
}