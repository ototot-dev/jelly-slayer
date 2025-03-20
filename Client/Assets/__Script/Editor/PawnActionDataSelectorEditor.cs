using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Game
{
    [CustomEditor(typeof(PawnActionDataSelector))]
    public class PawnActionDataSelectorEditor : Editor
    {
        int __selectIndex;
        GUIStyle __currActionLabelStyle;
        GUIStyle __prevActionLabelStyle;
        List<Tuple<string,float>> __executedActionTable = new();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
             
            var pawnActionDataSelector = target as PawnActionDataSelector;

            GUILayout.BeginVertical();
            {
                var oldColor = GUI.backgroundColor;
                GUILayout.Label("Pattern");
                foreach (var s in pawnActionDataSelector.ReservedSequences)
                {
                    GUI.backgroundColor = s.Value.currProb > 0f && s.Value == pawnActionDataSelector.CurrSequence() ? new Color(0.8f, 0.8f, 1) : Color.white;
                    if (GUILayout.Button($"{s.Value.SequenceName} | Rate:{s.Value.currProb:F2} | CoolTime:{s.Value.GetMaxCoolTime():F1}"))
                        pawnActionDataSelector.EnqueueSequence(s.Value);
                }
                GUILayout.Label("Action");
                foreach (var s in pawnActionDataSelector.ActionDataStates)
                {
                    GUI.backgroundColor = s.Value.GetCoolTime() > 0f ? new Color(0.8f, 0.8f, 1) : Color.white;
                    if (GUILayout.Button($"{s.Value.actionData.actionName} | CoolTime:{s.Value.GetCoolTime():F1}"))
                        pawnActionDataSelector.GetComponent<PawnActionController>().SetPendingAction(s.Value.actionData.actionName);
                }
                GUI.backgroundColor = oldColor;
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                __currActionLabelStyle ??= new("U2D.createRect");
                __prevActionLabelStyle ??= new GUIStyle("RectangleToolSelection");

                if (pawnActionDataSelector.TryGetComponent<PawnActionController>(out var actionCtrler))
                {
                    if (__executedActionTable.Count == 0)
                    {
                        if (actionCtrler.CheckActionRunning())
                            __executedActionTable.Add(new(actionCtrler.CurrActionName, actionCtrler.currActionContext.startTimeStamp));

                        GUILayout.Button("Empty", __prevActionLabelStyle);
                    }
                    else
                    {                    
                        var lastActionStartTimeStamp = __executedActionTable.Last().Item2;
                        if (actionCtrler.CheckActionRunning() && lastActionStartTimeStamp != actionCtrler.currActionContext.startTimeStamp)
                        {
                            __executedActionTable.Add(new(actionCtrler.CurrActionName, actionCtrler.currActionContext.startTimeStamp));
                            lastActionStartTimeStamp = actionCtrler.currActionContext.startTimeStamp;
                        }

                        foreach (var a in __executedActionTable)
                            GUILayout.Button($"{a.Item1}", actionCtrler.CheckActionRunning() && a.Item2 == lastActionStartTimeStamp ? __currActionLabelStyle : __prevActionLabelStyle);

                        //* 시작 시간이 4초를 경과했으면 해당 액션은 __executedActionTable에서 제거함
                        __executedActionTable.RemoveAll(a => Time.time - a.Item2 > 4f);
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Down"))
                {
                    var damageConext = new PawnHeartPointDispatcher.DamageContext(null);
                    pawnActionDataSelector.GetComponent<PawnActionController>().StartOnKnockDownAction(ref damageConext);
                }
                if (GUILayout.Button("Groggy"))
                {
                    var damageConext = new PawnHeartPointDispatcher.DamageContext(null);
                    pawnActionDataSelector.GetComponent<PawnActionController>().StartOnGroogyAction(ref damageConext);
                }
                if (GUILayout.Button("Jump"))
                    (pawnActionDataSelector.GetComponent<PawnBrainController>() as IPawnMovable).StartJump(1f);
                if (GUILayout.Button("Land"))
                    (pawnActionDataSelector.GetComponent<PawnBrainController>() as IPawnMovable).FinishJump();
            }
            GUILayout.EndHorizontal();
        }
    }
}