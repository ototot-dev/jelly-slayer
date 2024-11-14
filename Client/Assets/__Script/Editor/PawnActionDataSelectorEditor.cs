using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
using FlowCanvas.Nodes;

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

            GUILayout.BeginHorizontal();
            {
                var options = (target as PawnActionDataSelector).SelectionStates.Keys.Select(k => k.actionName).ToArray();
                __selectIndex = EditorGUILayout.Popup(__selectIndex, options);

                if (GUILayout.Button("Execute Action", GUILayout.MaxWidth(120)))
                    (target as PawnActionDataSelector).GetComponent<PawnActionController>().SetPendingAction(options[__selectIndex]);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                __currActionLabelStyle ??= new("U2D.createRect");
                __prevActionLabelStyle ??= new GUIStyle("RectangleToolSelection");
                if ((target as PawnActionDataSelector).TryGetComponent<PawnActionController>(out var actionCtrler))
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
        }
    }
}