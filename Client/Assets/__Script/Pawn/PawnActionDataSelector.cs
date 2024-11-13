using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class PawnActionDataSelector : MonoBehaviour
    {
        public bool boostSelectWeight;

        public class ActionState
        {
            public MainTable.ActionData source;
            public float currRate;
            public float currCoolTime;

            public ActionState(MainTable.ActionData actionData)
            {
                source = actionData;

                ResetRate();
                ResetCoolTime();
            }

            public void IncreaseRate(float deltaRate)
            {
                currRate += deltaRate;
            }

            public void ResetRate()
            {
                currRate = source.selectionRate;
            }

            public float DecreaseCoolTime(float deltaCoolTime)
            {
                Debug.Assert(deltaCoolTime > 0);
                return currCoolTime = Mathf.Max(0, currCoolTime - deltaCoolTime);
            }

            public void ResetCoolTime()
            {
                currCoolTime = source.coolTime;
            }
        };

        /// <summary>
        /// Item1: Weight, Item2: CoolTime
        /// </summary>
        public Dictionary<MainTable.ActionData, ActionState> SourceActionStates { get; private set; } = new();
        HashSet<MainTable.ActionData> __executableActionData = new();
        PawnBrainController __pawnBrain;

        void Awake()
        {
#if UNITY_EDITOR
            DatasheetManager.Instance.Load();
#endif
            var board = GetComponent<PawnBlackboard>();
            if (board != null)
            {
                var data = DatasheetManager.Instance.GetActionData(board.common.pawnId);
                if (data != null)
                {
                    foreach (var d in data)
                        SourceActionStates.Add(d, new ActionState(d));
                }
            }
            __pawnBrain = GetComponent<PawnBrainController>();
        }

        public void UpdateSelection(float deltaTime)
        {            
            foreach (var s in SourceActionStates)
            {
                if (s.Key.selectionRate > 0 && (s.Key.coolTime < 0f || (s.Key.coolTime > 0 && s.Value.DecreaseCoolTime(deltaTime) <= 0)))
                    __executableActionData.Add(s.Key);
            }
        }

        public bool TryPickSelection(float distanceConstraint, float staminaConstraint, out MainTable.ActionData ret)
        {
            ret = PickSelection(distanceConstraint, staminaConstraint);

            if (boostSelectWeight && __executableActionData.Count > 0)
            {
                foreach (var e in __executableActionData)
                    SourceActionStates[e].IncreaseRate(1f);
            }

            return ret != null;
        }

        public MainTable.ActionData PickSelection(float distanceConstraint, float staminaConstraint)
        {
            var selector = UnityEngine.Random.Range(0, __executableActionData.Where(d => d.actionRange >= distanceConstraint && d.staminaCost <= staminaConstraint).Sum(d => SourceActionStates[d].currRate));
            var curr = 0f;
            foreach (var d in __executableActionData)
            {
                if (distanceConstraint > d.actionRange || staminaConstraint < d.staminaCost)
                    continue;

                if (selector >= curr && selector < curr + SourceActionStates[d].currRate)
                {
                    SourceActionStates[d].currRate = d.selectionRate;
                    __executableActionData.Remove(d);
                    return d;
                }

                curr += SourceActionStates[d].currRate;
            }

            return null;
        }

        public MainTable.ActionData EvaluateSelection(string actionName, float distanceConstraint, float staminaConstraint)
        {
            return null;
            // var actionData = __executableActionData.FirstOrDefault(d => d.actionName == actionName);
            // if (actionData == null)
            //     return null;

            // if (actionData.actionRange )

            

        }
    }
}