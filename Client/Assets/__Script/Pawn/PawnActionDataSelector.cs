using System;
using System.Collections.Generic;
using System.Linq;
using MainTable;
using UnityEngine;
using VInspector.Libs;

namespace Game
{
    public class PawnActionDataSelector : MonoBehaviour
    {   
        public class ActionSequence
        {
            public ActionSequence(PawnActionDataSelector selector, int hashCode, string sequenceName, MainTable.ActionData[] sequenceData, Dictionary<int, float> paddindTimeData)
            {
                HashCode = hashCode;
                SequenceName = sequenceName;
                __selector = selector;
                __sequenceData = sequenceData;
                __paddingTimeData = paddindTimeData;
                __currIndex = -1;
            }

            public MainTable.ActionData this[int index]
            {
                get => __sequenceData[index];
                set => __sequenceData[index] = value;
            }
            public int Size() => __sequenceData.Length;
            public MainTable.ActionData First() => __sequenceData[0];
            public MainTable.ActionData Last() => __sequenceData[__sequenceData.Length - 1];
            public MainTable.ActionData Curr() => __sequenceData[__currIndex];
            public MainTable.ActionData Next() => ++__currIndex < __sequenceData.Length ? __sequenceData[__currIndex] : null;
            public void Reset() { __currIndex = -1; }
            public float GetPaddingTime() => (__paddingTimeData?.ContainsKey(__currIndex) ?? false) ? __paddingTimeData[__currIndex] : 0f;
            public float GetMaxCoolTime() => __sequenceData.Max(p => __selector.GetCoolTime(p));
            public void SetCoolTime(int index = 0)
            {
                Debug.Assert(index < 0 || index >= __sequenceData.Length);
                __selector.SetCoolTime(__sequenceData[index]);
            }

            public void IncreaseProbability(float deltaProb, float maxProb)
            {
                currProb = Mathf.Min(currProb + deltaProb, maxProb);
            }
            public void ResetProbability(float resetProb = 0f)
            {
                currProb = resetProb;
            }

            public bool Evaluate(float probConstraint, float staminaConstraint, float intervalConstraint)
            {
                if ((Time.time - __evaluateTimeStamp) < intervalConstraint) return false;

                __evaluateTimeStamp = Time.time;

                if (currProb < probConstraint) 
                    return false;

                foreach (var d in __sequenceData)
                {
                    if (!__selector.EvaluateActionState(d, staminaConstraint))
                        return false;
                }

                return true;
            }

            readonly MainTable.ActionData[] __sequenceData;
            readonly Dictionary<int, float> __paddingTimeData;
            PawnActionDataSelector __selector;
            int __currIndex;
            float __evaluateTimeStamp;
            public float currProb;
            public int HashCode { get; private set; }
            public string SequenceName { get; private set; }
        }

        public ActionSequence ReserveSequence<T>(T alias, params object[] actionNames) where T : struct, Enum
        {
            var hashCode = alias.GetHashCode();
            if (__reservedSequences.ContainsKey(hashCode))
            {
                Debug.Assert(false);
                return null;
            }

            var actionData = new List<MainTable.ActionData>();
            var paddingTimeData = new Dictionary<int, float>();
            for (int i = 0; i < actionNames.Length; i++)
            {
                if (actionNames[i] is string s) 
                    actionData.Add(DatasheetManager.Instance.GetActionData(__pawnBrain.PawnBB.common.pawnId, s));
                else if (actionNames[i] is float f)
                    paddingTimeData.Add(actionData.Count, f);
                else
                    Debug.Assert(false);
            }

            __reservedSequences.Add(hashCode, new ActionSequence(this, alias.GetHashCode(), alias.ToString(), actionData.ToArray(), paddingTimeData.Count > 0 ? paddingTimeData : null));
            return __reservedSequences[hashCode];
        }

        public ActionSequence GetSequence<T>(T alias) where T : struct, Enum
        {
            if (__reservedSequences.TryGetValue(alias.GetHashCode(), out var ret)) 
                return ret;
            else
                return null;
        }

        public ActionSequence EnqueueSequence<T>(T alias) where T : struct, Enum
        { 
            var sequence = GetSequence<T>(alias);
            Debug.Assert(sequence != null);

            return EnqueueSequence(sequence);
        }
        public ActionSequence EnqueueSequence(ActionSequence sequence)
        { 
            if (!__sequenceQueue.Contains(sequence))
            {
                sequence.Reset();
                __sequenceQueue.Enqueue(sequence);
                return sequence;
            }
            else
            {
                __Logger.WarningR2(gameObject, nameof(EnqueueSequence), "__actionSequencePatternQueue.Contains() returns true.", "sequence", sequence.SequenceName);
                return null;
            }
        }
        

        public ActionSequence CurrSequence() => __sequenceQueue.TryPeek(out var ret) ? ret : null;
        public ActionSequence NextSequence()
        {
            __sequenceQueue.Dequeue();
            return CurrSequence();
        }
        public ActionData AdvanceSequence()
        {
            if (__sequenceQueue.TryPeek(out var currSequence))
                return currSequence.Next() != null ? currSequence.Curr() : (NextSequence()?.Next() ?? null);
            else
                return null;
        }

        public void ClearSequences() { __sequenceQueue.Clear(); }

        readonly Queue<ActionSequence> __sequenceQueue = new();
        readonly Dictionary<int, ActionSequence> __reservedSequences = new();

        public class ActionDataState
        {
            public MainTable.ActionData actionData;
            float __executedTimeStamp;
            public float GetCoolTime() => actionData.coolTime <= 0f ? 0f : Mathf.Max(0f, actionData.coolTime + __executedTimeStamp - Time.time);
            public void ResetCoolTime() { __executedTimeStamp = Time.time; }

            public ActionDataState(MainTable.ActionData actionData)
            {
                this.actionData = actionData;
            }
        };

#if UNITY_EDITOR
        public Dictionary<int, ActionSequence> ReservedSequences => __reservedSequences;
        public Dictionary<MainTable.ActionData, ActionDataState> ActionDataStates => __actionDataStates;
#endif

        readonly Dictionary<MainTable.ActionData, ActionDataState> __actionDataStates = new();
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
                __Logger.WarningR2(gameObject, nameof(GetActionData), "DatasheetManager.Instance.GetActionData() return false", "pawnId", __pawnBrain.PawnBB.common.pawnId, "actionName", actionName);

            return ret;
        }

        public void BoostProbability<T>(T alias, float deltaProb, float maxProb = 1f) where T : struct, Enum
        {
            GetSequence(alias)?.IncreaseProbability(deltaProb, maxProb);
        }
        public void ResetProbability<T>(T alias, float resetProb = 0f) where T : struct, Enum
        {
            GetSequence(alias)?.ResetProbability(resetProb);
        }

        public void SetCoolTime(string actionName)
        {
            SetCoolTime(GetActionData(actionName));
        }
        public void SetCoolTime(MainTable.ActionData actionData)
        {   
            Debug.Assert(actionData != null);

            if (__actionDataStates.TryGetValue(actionData, out var state))
                state.ResetCoolTime();
            else
                __Logger.WarningR2(gameObject, nameof(SetCoolTime), "__actionDataStates.TryGetValue() return false", "actionName", actionData.actionName);
        }

        public float GetCoolTime(string actionName) => GetCoolTime(GetActionData(actionName));
        public float GetCoolTime(ActionData actionData)
        {
            Debug.Assert(actionData != null);

            if (__actionDataStates.TryGetValue(actionData, out var state))
            {
                return state.GetCoolTime();
            }
            else
            {
                __Logger.WarningR2(gameObject, nameof(SetCoolTime), "__actionDataStates.TryGetValue() return false", "actionName", actionData.actionName);
                return 0f;
            }
        }

        public bool CheckCoolTime(string actionName) => CheckCoolTime(GetActionData(actionName));
        public bool CheckCoolTime(ActionData actionData) => GetCoolTime(actionData) <= 0f;

        public bool EvaluateSequence<T>(T alias, float probConstraint = -1f, float staminaConstraint = -1f, float intervalConstraint = -1f) where T : struct, Enum
        {
            return GetSequence(alias)?.Evaluate(probConstraint, staminaConstraint, intervalConstraint) ?? false;
        }
        public bool EvaluateActionState(string actionName, float staminaConstraint = -1f) => EvaluateActionState(GetActionData(actionName), staminaConstraint);
        public bool EvaluateActionState(MainTable.ActionData actionData, float staminaConstraint = -1f)
        {
            Debug.Assert(actionData != null);

            if (!__actionDataStates.TryGetValue(actionData, out var actionState))
            {
                __Logger.WarningR2(gameObject, nameof(EvaluateActionState), "SourceActionStates.TryGetValue() return false", "actionName", actionData.actionName);
                return false;
            }

            return actionState.GetCoolTime() <= 0f && (staminaConstraint < 0f || actionState.actionData.staminaCost <= staminaConstraint);
        }

        public T TryRandomPick<T>(float probConstraint, float staminaConstraint = -1f, float intervalConstraint = -1f) where T : struct, Enum
        {
            var executables = __reservedSequences.Select(p => p.Value).Where(s => s.currProb > 0f && s.Evaluate(probConstraint, staminaConstraint, intervalConstraint)).ToArray();
            var selectRate = UnityEngine.Random.Range(0, executables.Sum(e => e.currProb));
            var accumRate = 0f;
            foreach (var e in executables)
            {
                if (selectRate >= accumRate && selectRate < (accumRate + e.currProb))
                    return Enum.Parse<T>(e.SequenceName);

                accumRate += e.currProb;
            }

            //* Enum 기본값에 'None'이 존재함을 가정함
            return Enum.Parse<T>("None");
        }
    }
}