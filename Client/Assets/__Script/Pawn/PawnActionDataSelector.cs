using System.Collections.Generic;
using System.Linq;
using MainTable;
using NodeCanvas.Framework;
using UnityEngine;

namespace Game
{
    public class PawnActionDataSelector : MonoBehaviour
    {   
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