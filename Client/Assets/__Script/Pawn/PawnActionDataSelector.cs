using System.Collections.Generic;
using System.Linq;
using FIMSpace.Generating;
using UnityEngine;

namespace Game
{
    public class PawnActionDataSelector : MonoBehaviour
    {
        public class SelectionState
        {
            public MainTable.ActionData actionData;
            public float currRate;
            public float currCoolTime;

            public SelectionState(MainTable.ActionData actionData)
            {
                this.actionData = actionData;

                ResetRate();
                ResetCoolTime();
            }

            public void IncreaseRate(float deltaRate)
            {
                currRate += deltaRate;
            }

            public void ResetRate()
            {
                currRate = actionData.selectionRate;
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

        /// <summary>
        /// Item1: Weight, Item2: CoolTime
        /// </summary>
        public Dictionary<MainTable.ActionData, SelectionState> SelectionStates { get; private set; } = new();
        HashSet<MainTable.ActionData> __executables = new();
        PawnBrainController __pawnBrain;

        void Awake()
        {
#if UNITY_EDITOR
            DatasheetManager.Instance.Load();
#endif
            var pawnBB = GetComponent<PawnBlackboard>();
            if (pawnBB != null)
            {
                var data = DatasheetManager.Instance.GetActionData(pawnBB.common.pawnId);
                if (data != null)
                {
                    foreach (var d in data)
                        SelectionStates.Add(d, new SelectionState(d));
                }
            }

            __pawnBrain = GetComponent<PawnBrainController>();
        }

        public void UpdateSelection(float deltaTime)
        {            
            foreach (var s in SelectionStates)
            {
                if (s.Key.selectionRate > 0 && (s.Key.coolTime < 0f || (s.Key.coolTime > 0 && s.Value.DecreaseCoolTime(deltaTime) <= 0)))
                    __executables.Add(s.Key);
            }
        }

        public void BoostSelection(string actionName, float deltaRate)
        {
            var actionData = DatasheetManager.Instance.GetActionData(__pawnBrain.PawnBB.common.pawnId, actionName);
            if (actionData == null)
            {
                __Logger.WarningF(gameObject, nameof(BoostSelection), "DatasheetManager.Instance.GetActionData() return false", "pawnId", __pawnBrain.PawnBB.common.pawnId, "actionName", actionData.actionName);
                return;
            }

            BoostSelection(actionData, deltaRate);
        }

        public void BoostSelection(MainTable.ActionData actionData, float deltaRate)
        {   
            Debug.Assert(actionData != null);

            if (SelectionStates.TryGetValue(actionData, out var state))
                state.currRate += deltaRate;
            else
                __Logger.WarningF(gameObject, nameof(BoostSelection), "SourceActionStates.TryGetValue() return false", "actionName", actionData.actionName);
        }
        
        public void ResetSelection(string actionName)
        {
            var actionData = DatasheetManager.Instance.GetActionData(__pawnBrain.PawnBB.common.pawnId, actionName);
            if (actionData == null)
            {
                __Logger.WarningF(gameObject, nameof(ResetSelection), "DatasheetManager.Instance.GetActionData() return false", "pawnId", __pawnBrain.PawnBB.common.pawnId, "actionName", actionData.actionName);
                return;
            }

            ResetSelection(actionData);
        }

        public void ResetSelection(MainTable.ActionData actionData)
        {   
            Debug.Assert(actionData != null);

            if (SelectionStates.TryGetValue(actionData, out var state))
            {
                state.ResetRate();
                state.ResetCoolTime();
            }
            else
            {
                __Logger.WarningF(gameObject, nameof(ResetSelection), "SourceActionStates.TryGetValue() return false", "actionName", actionData.actionName);
            }
        }

        public bool EvaluateSelection(string actionName, float distanceConstraint, float staminaConstraint)
        {
            var actionData = DatasheetManager.Instance.GetActionData(__pawnBrain.PawnBB.common.pawnId, actionName);
            if (actionData == null)
            {
                __Logger.WarningF(gameObject, nameof(EvaluateSelection), "DatasheetManager.Instance.GetActionData() return false", "pawnId", __pawnBrain.PawnBB.common.pawnId, "actionName", actionData.actionName);
                return false;
            }

            return EvaluateSelection(actionData, distanceConstraint, staminaConstraint);
        }

        public bool EvaluateSelection(MainTable.ActionData actionData, float distanceConstraint, float staminaConstraint)
        {
            Debug.Assert(actionData != null);

            if (!__executables.Contains(actionData))
                return false;

            if (SelectionStates.TryGetValue(actionData, out var actionState))
            {
                if (actionData.actionRange < distanceConstraint)
                    return false;
                else if (actionData.staminaCost > staminaConstraint)
                    return false;
                else
                    return actionState.currRate > UnityEngine.Random.Range(0f, 1f);
            }
            else
            {
                __Logger.WarningF(gameObject, nameof(EvaluateSelection), "SourceActionStates.TryGetValue() return false", "actionName", actionData.actionName);
                return false;
            }
        }
        
        public bool TryRandomSelection(float distanceConstraint, float staminaConstraint, bool resetRate, out MainTable.ActionData result)
        {
            result = RandomSelection(distanceConstraint, staminaConstraint, resetRate);
            return result != null;
        }

        public MainTable.ActionData RandomSelection(float distanceConstraint, float staminaConstraint, bool resetSelection)
        {
            var selector = UnityEngine.Random.Range(0, __executables.Where(d => d.actionRange >= distanceConstraint && d.staminaCost <= staminaConstraint).Sum(d => SelectionStates[d].currRate));
            var curr = 0f;
            foreach (var d in __executables)
            {
                if (distanceConstraint > d.actionRange || staminaConstraint < d.staminaCost)
                    continue;

                if (selector >= curr && selector < curr + SelectionStates[d].currRate)
                {
                    if (resetSelection)
                    {
                        SelectionStates[d].ResetRate();
                        SelectionStates[d].ResetCoolTime();
                    }
                 
                    __executables.Remove(d);       
                    return d;
                }

                curr += SelectionStates[d].currRate;
            }

            return null;
        }
    }
}