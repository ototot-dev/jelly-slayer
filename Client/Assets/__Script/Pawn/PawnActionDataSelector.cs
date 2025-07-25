using System;
using System.Collections.Generic;
using System.Linq;
using MainTable;
using UnityEngine;
using ZLinq;

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
            public void Reset() { __currIndex = -1; }
            public MainTable.ActionData First() => __sequenceData[0];
            public MainTable.ActionData Last() => __sequenceData[__sequenceData.Length - 1];
            public MainTable.ActionData Curr() => __sequenceData[__currIndex];
            public MainTable.ActionData Next() => ++__currIndex < __sequenceData.Length ? __sequenceData[__currIndex] : null;
            public float GetPaddingTime() => (__paddingTimeData?.ContainsKey(__currIndex) ?? false) ? __paddingTimeData[__currIndex] : 0f;
            public float GetRemainCoolTime() => maxCoolTime > 0f ? Mathf.Max(0f, maxCoolTime + __beginTimeStamp - Time.time) : 0f;
            public ActionSequence BeginCoolTimeInternal(float coolTime)
            {
                __beginTimeStamp = Time.time;
                maxCoolTime = coolTime;
                return this;
            }
            public ActionSequence CancelCoolTimeInternal()
            {
                __beginTimeStamp = 0f;
                return this;
            }
            public bool Evaluate(float probConstraint, float staminaConstraint)
            {
                if (GetRemainCoolTime() > 0f)
                    return false;

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
            float __beginTimeStamp;
            public float maxCoolTime;
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
            if (debugActionSelectDisabled)
            {
                __Logger.LogR2(gameObject, "EnqueueSequence()", "debugEnqueueDisabled", debugActionSelectDisabled);
                return null;
            }

            if (!__sequenceQueue.Contains(sequence))
            {
                __sequenceQueue.Enqueue(sequence);
                sequence.Reset();
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

        public void CancelSequences() { __sequenceQueue.Clear(); }

        readonly Queue<ActionSequence> __sequenceQueue = new();
        readonly Dictionary<int, ActionSequence> __reservedSequences = new();

        public class ActionDataState
        {
            public MainTable.ActionData actionData;

            public ActionDataState(MainTable.ActionData actionData)
            {
                this.actionData = actionData;
            }
        };

#if UNITY_EDITOR
        [HideInInspector, SerializeField]
        public bool debugActionSelectDisabled;
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

        public ActionSequence BoostProbability<T>(T alias, float deltaProb, float maxProb = 1f) where T : struct, Enum
        {
            var seq = GetSequence(alias);
            seq.currProb = Mathf.Min(seq.currProb + deltaProb, maxProb);
            return seq;
        }
        public ActionSequence ResetProbability<T>(T alias, float resetProb = 0f) where T : struct, Enum
        {
            var seq = GetSequence(alias);
            seq.currProb = resetProb;
            return seq;
        }
        public ActionSequence BeginCoolTime<T>(T alias, float coolTime) where T : struct, Enum
        {
            var seq = GetSequence(alias);
            seq.BeginCoolTimeInternal(coolTime);
            return seq;
        }
        public ActionSequence ReduceCoolTime<T>(T alias, float deltaTime) where T : struct, Enum
        {
            var seq = GetSequence(alias);
            seq.maxCoolTime = Mathf.Max(0f, seq.maxCoolTime - deltaTime);
            return seq;
        }
        public ActionSequence CancelCoolTime<T>(T alias) where T : struct, Enum
        {
            var seq = GetSequence(alias);
            seq.maxCoolTime = 0f;
            return seq;
        }
        public float GetRemainCoolTime<T>(T alias) where T : struct, Enum
        {
            return GetSequence(alias)?.GetRemainCoolTime() ?? 0f;
        }

        public bool EvaluateSequence<T>(T alias, float probConstraint = -1f, float staminaConstraint = -1f) where T : struct, Enum
        {
            if (debugActionSelectDisabled)
            {
                __Logger.LogR2(gameObject, "EvaluateSequence()", "debugEnqueueDisabled", debugActionSelectDisabled);
                return false;
            }

            return GetSequence(alias).Evaluate(probConstraint, staminaConstraint);
        }
        public bool EvaluateActionState(string actionName, float staminaConstraint = -1f) => EvaluateActionState(GetActionData(actionName), staminaConstraint);
        public bool EvaluateActionState(MainTable.ActionData actionData, float staminaConstraint = -1f)
        {
            if (debugActionSelectDisabled)
            {
                __Logger.LogR2(gameObject, "EvaluateActionState()", "debugEnqueueDisabled", debugActionSelectDisabled);
                return false;
            }

            Debug.Assert(actionData != null);

            if (!__actionDataStates.TryGetValue(actionData, out var actionState))
            {
                __Logger.WarningR2(gameObject, nameof(EvaluateActionState), "SourceActionStates.TryGetValue() return false", "actionName", actionData.actionName);
                return false;
            }

            return staminaConstraint < 0f || actionState.actionData.staminaCost <= staminaConstraint;
        }

        public T TryRandomPick<T>(float probConstraint = 0f, float staminaConstraint = -1f) where T : struct, Enum
        {
            if (debugActionSelectDisabled)
            {
                __Logger.LogR2(gameObject, "TryRandomPick()", "debugActionSelectDisabled", debugActionSelectDisabled);
                return Enum.Parse<T>("None");
            }

            var executables = __reservedSequences.AsValueEnumerable().Select(p => p.Value).Where(s => s.currProb > 0f && s.Evaluate(probConstraint, staminaConstraint));
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