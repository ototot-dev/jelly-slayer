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
        GUIStyle __currActionLabelStyle;
        GUIStyle __prevActionLabelStyle;
        List<Tuple<string,float>> __executedActionTable = new();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
             
            var actionDataSelector = target as PawnActionDataSelector;

            GUILayout.BeginVertical();
            {
                var oldColor = GUI.backgroundColor;
                GUILayout.Label("Pattern");
                foreach (var s in actionDataSelector.ReservedSequences)
                {
                    GUI.backgroundColor = s.Value.currProb > 0f && s.Value == actionDataSelector.CurrSequence() ? new Color(0.8f, 0.8f, 1) : Color.white;
                    if (GUILayout.Button($"{s.Value.SequenceName} | Rate:{s.Value.currProb:F2} | CoolTime:{s.Value.GetRemainCoolTime():F1}"))
                        actionDataSelector.EnqueueSequence(s.Value);
                }
                GUILayout.Label("Action");
                foreach (var s in actionDataSelector.ActionDataStates)
                {
                    GUI.backgroundColor = Color.white;
                    if (GUILayout.Button($"{s.Value.actionData.actionName}"))
                        actionDataSelector.GetComponent<PawnActionController>().SetPendingAction(s.Value.actionData.actionName);
                }
                GUI.backgroundColor = oldColor;
            }
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            {
                __currActionLabelStyle ??= new("U2D.createRect");
                __prevActionLabelStyle ??= new GUIStyle("RectangleToolSelection");

                if (actionDataSelector.TryGetComponent<PawnActionController>(out var actionCtrler))
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
                if (GUILayout.Button("Groggy"))
                {
                    var damageConext = new PawnHeartPointDispatcher.DamageContext(null)
                    {
                        senderBrain = GameContext.Instance.playerCtrler.possessedBrain,
                        senderActionData = DatasheetManager.Instance.GetActionData(PawnId.Hero, "GuardParry")
                    };
                    actionDataSelector.GetComponent<PawnActionController>().StartOnGroogyAction(ref damageConext);
                }
                if (GUILayout.Button("Down"))
                {
                    var damageConext = new PawnHeartPointDispatcher.DamageContext(null);
                    actionDataSelector.GetComponent<PawnActionController>().StartOnKnockDownAction(ref damageConext);
                }
                if (GUILayout.Button("Jump"))
                    (actionDataSelector.GetComponent<PawnBrainController>() as IPawnMovable).StartJump(1f);
                if (GUILayout.Button("Land"))
                    (actionDataSelector.GetComponent<PawnBrainController>() as IPawnMovable).FinishJump();

                if (GUILayout.Button("Hold"))
                    actionDataSelector.GetComponent<PawnBrainController>().InvalidateDecision(4f);
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Label("Toggle");
            GUILayout.BeginVertical();
            {
                serializedObject.Update();
                serializedObject.FindProperty("debugActionSelectDisabled").boolValue = GUILayout.Toggle(serializedObject.FindProperty("debugActionSelectDisabled").boolValue, "Action Disabled");
                serializedObject.ApplyModifiedProperties();

                actionDataSelector.GetComponent<PawnMovement>().freezeMovement = GUILayout.Toggle(actionDataSelector.GetComponent<PawnMovement>().freezeMovement, "Movement Disabled");
            }
            GUILayout.EndHorizontal();
        }
    }
}