#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if ENABLE_DOTWEEN_SEQUENCE
using DG.Tweening;
#endif

namespace Retween.Rx
{
    [CustomPropertyDrawer(typeof(TweenSelectorQuery))]
    public class TweenSelectorQueryDrawer : PropertyDrawer
    {
        /// <summary>
        /// ctor.
        /// </summary>
        public TweenSelectorQueryDrawer()
        {
            __checkStyle.alignment = TextAnchor.MiddleCenter;
            __checkStyle.fontStyle = FontStyle.Bold;
            __uncheckStyle.alignment = TextAnchor.MiddleCenter;
            __runningTweenStyle.alignment = TextAnchor.MiddleLeft;
            __runningTweenStyle.fontStyle = FontStyle.Bold;
            __unrunningTweenStyle.alignment = TextAnchor.MiddleLeft;
            __unrunningTweenStyle.fontStyle = FontStyle.Italic;
            __emptyListStyle.alignment = TextAnchor.MiddleLeft;
            __emptyListStyle.fontStyle = FontStyle.Italic;
            __initValueStyle.fontStyle = FontStyle.BoldAndItalic;
            __initValueEmptyStyle.fontStyle = FontStyle.Italic;
        }

        GUIStyle __checkStyle = new GUIStyle("U2D.createRect");
        GUIStyle __uncheckStyle = new GUIStyle("RectangleToolSelection");
        GUIStyle __runningTweenStyle = new GUIStyle("U2D.createRect");
        GUIStyle __unrunningTweenStyle = new GUIStyle("RectangleToolSelection");
        GUIStyle __emptyListStyle = new GUIStyle("RectangleToolSelection");
        GUIStyle __initValueStyle = new GUIStyle();
        GUIStyle __initValueEmptyStyle = new GUIStyle();
        bool __classListFoldOut = true;
        bool __stateListFoldOut = true;
        bool __runningTweensFoldOut = true;
        readonly float __lineHeight = 18;
        readonly float __charWidth = 9;
        float __propertyHeight = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (fieldInfo.GetValue(property.serializedObject.targetObject) is not TweenSelectorQuery query)
                return;

            var tweensProperty = property.FindPropertyRelative("sources");
            EditorGUI.PropertyField(position, tweensProperty, new GUIContent("Tween Sources"), true);

            var basePositionY = position.y; 
            if (tweensProperty.isExpanded)
                position.y += __lineHeight * (tweensProperty.arraySize + 1) + __lineHeight;

            position.y += __lineHeight;

            var drawRect = position;
            drawRect.height = __lineHeight;

            if (GUI.Button(drawRect, "Update Tween Sources"))
            {
                if (query.TargetSelector != null)
                    UpdateTweenSources(query.TargetSelector.transform, query, tweensProperty);

                query.BuildSelectables();
            }

            drawRect.y += __lineHeight + 3;

            var usingTagProperty = property.FindPropertyRelative("usingTag");
            EditorGUI.PropertyField(drawRect, usingTagProperty);

            drawRect.y += __lineHeight;
            if (usingTagProperty.boolValue)
            {
                var tagProperty = property.FindPropertyRelative("tag");
                EditorGUI.PropertyField(drawRect, tagProperty);

                drawRect.y += __lineHeight;
            }

            var enableInitTweensProperty = property.FindPropertyRelative("enableInitTweens");
            EditorGUI.PropertyField(drawRect, enableInitTweensProperty);

            drawRect.y += __lineHeight;
            if (enableInitTweensProperty.boolValue)
            {
                var skipInitTweensProperty = property.FindPropertyRelative("skipInitTweens");
                EditorGUI.PropertyField(drawRect, skipInitTweensProperty);

                drawRect.y += __lineHeight;

                GUI.Label(drawRect,
                    "- Classes = " + (query.initActiveClasses.Count == 0 ? "Empty" : query.initActiveClasses.Aggregate((concat, str) => $"{concat}, {str}")),
                    query.initActiveClasses.Count == 0 ? __initValueEmptyStyle : __initValueStyle
                    );

                drawRect.y += __lineHeight;

                GUI.Label(drawRect,
                    "- States = " + (query.initActiveStates.Count == 0 ? "Empty" : query.initActiveStates.Aggregate((concat, str) => $"{concat}, {str}")),
                    query.initActiveStates.Count == 0 ? __initValueEmptyStyle : __initValueStyle
                    );

                drawRect.y += __lineHeight;

                if (GUI.Button(drawRect, "Save Init Tweens"))
                {
                    var initActiveClassesProperty = property.FindPropertyRelative("initActiveClasses");
                    var initActiveStatesProperty = property.FindPropertyRelative("initActiveStates");

                    initActiveClassesProperty.ClearArray();
                    initActiveStatesProperty.ClearArray();

                    var tempList = new List<string>(query.activeClasses);
                    // tempList.AddRange(query.activeClasses);
                    for (int i = tempList.Count - 1; i >= 0; --i)
                    {
                        initActiveClassesProperty.InsertArrayElementAtIndex(0);
                        initActiveClassesProperty.GetArrayElementAtIndex(0).stringValue = tempList[i];
                    }

                    tempList.Clear();
                    tempList.AddRange(query.activeStates);

                    for (int i = tempList.Count - 1; i >= 0; --i)
                    {
                        initActiveStatesProperty.InsertArrayElementAtIndex(0);
                        initActiveStatesProperty.GetArrayElementAtIndex(0).stringValue = tempList[i];
                    }
                }

                drawRect.x = position.x;
                drawRect.y += __lineHeight;
            }

            drawRect.width = __charWidth;
            drawRect.height = __lineHeight;

            __classListFoldOut = EditorGUI.Foldout(drawRect, __classListFoldOut, "Class List");

            drawRect.y += __lineHeight;

            var hasDirty = false;
            if (__classListFoldOut)
            {
                if (query.selectableClasses.Count > 0)
                {
                    foreach (var c in query.selectableClasses)
                    {
                        if (query.activeClasses.Contains(c))
                        {
                            drawRect.width = __checkStyle.CalcSize(new GUIContent(c)).x;
                            if (drawRect.x + drawRect.width > EditorGUIUtility.currentViewWidth)
                            {
                                drawRect.x = position.x;
                                drawRect.y += __lineHeight;
                            }

                            if (GUI.Button(drawRect, c, __checkStyle))
                            {
                                query.activeClasses.Remove(c);
                                hasDirty = true;
                            }

                            drawRect.x += drawRect.width;
                        }
                        else
                        {
                            drawRect.width = __uncheckStyle.CalcSize(new GUIContent(c)).x;
                            if (drawRect.x + drawRect.width > EditorGUIUtility.currentViewWidth)
                            {
                                drawRect.x = position.x;
                                drawRect.y += __lineHeight;
                            }

                            if (GUI.Button(drawRect, c, __uncheckStyle))
                            {
                                query.activeClasses.Add(c);
                                hasDirty = true;
                            }

                            drawRect.x += drawRect.width;
                        }
                    }
                }
                else
                {
                    drawRect.width = position.width;
                    GUI.Label(drawRect, "Empty", __emptyListStyle);
                }

                drawRect.y += __lineHeight;
            }

            drawRect.x = position.x;
            drawRect.width = __charWidth;

            __stateListFoldOut = EditorGUI.Foldout(drawRect, __stateListFoldOut, "State List");

            drawRect.y += __lineHeight;

            if (__stateListFoldOut)
            {
                if (query.selectableStates.Count > 0)
                {
                    foreach (var s in query.selectableStates)
                    {
                        if (query.activeStates.Contains(s))
                        {
                            drawRect.width = __checkStyle.CalcSize(new GUIContent(s)).x;
                            if (drawRect.x + drawRect.width > EditorGUIUtility.currentViewWidth)
                            {
                                drawRect.x = position.x;
                                drawRect.y += __lineHeight;
                            }

                            if (GUI.Button(drawRect, s, __checkStyle))
                            {
                                query.activeStates.Remove(s);
                                hasDirty = true;
                            }

                            drawRect.x += drawRect.width;
                        }
                        else
                        {
                            drawRect.width = __uncheckStyle.CalcSize(new GUIContent(s)).x;
                            if (drawRect.x + drawRect.width > EditorGUIUtility.currentViewWidth)
                            {
                                drawRect.x = position.x;
                                drawRect.y += __lineHeight;
                            }

                            if (GUI.Button(drawRect, s, __uncheckStyle))
                            {
                                query.activeStates.Add(s);
                                hasDirty = true;
                            }

                            drawRect.x += drawRect.width;
                        }
                    }
                }
                else
                {
                    drawRect.width = position.width;
                    GUI.Label(drawRect, "Empty", __emptyListStyle);
                }

                drawRect.y += __lineHeight;
            }

            if (hasDirty && Application.isPlaying)
                query.Apply();

            drawRect.x = position.x;
            drawRect.width = position.width;
            drawRect.height = __lineHeight;

            __runningTweensFoldOut = EditorGUI.Foldout(drawRect, __runningTweensFoldOut, "Running Tweens");

            drawRect.y += __lineHeight;
            if (__runningTweensFoldOut)
            {
                var drawAny = false;
                if (query.TargetSelector != null && query.TargetSelector.Player != null && query.TargetSelector.Player.tweenStates.Count > 0)
                {
                    foreach (var v in query.TargetSelector.Player.tweenStates.Values)
                    {
                        if (!v.IsRunning && !v.IsRewinding && !v.IsRollBack && v.elapsed <= TweenPlayer.MIN_ELAPSED_TIME)
                            continue;

                        var buttonText = v.source.name;
                        if (v.animEnabled)
                        {
                            if (v.IsRewinding)
                                buttonText += " (Rewinding...)";
                            else if (v.IsRollBack)
                                buttonText += " (Rollbacking...)";
                            else
                                buttonText += " (Running...)";

                            if (GUI.Button(drawRect, buttonText, __runningTweenStyle))
                                EditorGUIUtility.PingObject(v.source.gameObject);

                            drawRect.y += __lineHeight;
                            drawAny = true;
                        }
                        else if (!v.source.transition.IsLooping && (v.source.rewindOnCancelled || v.source.runRollback))
                        {
                            buttonText += " (EOT)";

                            if (GUI.Button(drawRect, buttonText, __unrunningTweenStyle))
                                EditorGUIUtility.PingObject(v.source.gameObject);

                            drawRect.y += __lineHeight;
                            drawAny = true;
                        }
                    }
                }

#if ENABLE_DOTWEEN_SEQUENCE
            if (query.Target != null && query.Target.Player != null && query.Target.Player.dotweeenSeqRunnings.Count > 0) 
            {
                foreach (var v in query.Target.Player.dotweeenSeqRunnings.Values) 
                {
                    if (v.dotweenSeq == null)
                        continue;
                    if (!v.dotweenSeq.IsPlaying() && !v.isRewinding && v.dotweenSeq.position <= TweenPlayer.minElapsed)
                        continue;

                    var buttonText = v.source.name;
                    if (v.dotweenSeq.IsPlaying()) {
                        if (v.isRewinding)
                            buttonText += " (Rewinding...)";
                        else
                            buttonText += " (Running...)";

                        if (GUI.Button(drawRect, buttonText, __runningTweenStyle))
                            EditorGUIUtility.PingObject(v.source.gameObject);

                        drawRect.y += __lineHeight;
                        drawAny = true;
                    }
                    else if (v.source.rewindOnCancelled || v.source.rewindOnRollback) 
                    {
                        buttonText += " (EOT)";

                        if (GUI.Button(drawRect, buttonText, __unrunningTweenStyle))
                            EditorGUIUtility.PingObject(v.source.gameObject);

                        drawRect.y += __lineHeight;
                        drawAny = true;
                    }
                }
            }   
#endif

                if (!drawAny)
                {
                    GUI.Label(drawRect, "Empty", __emptyListStyle);
                    drawRect.y += __lineHeight;
                }
            }

            EditorGUI.EndProperty();

            position.y = drawRect.y;
            __propertyHeight = position.y - basePositionY;
        }

        void UpdateTweenSources(Transform target, TweenSelectorQuery query, SerializedProperty tweensProperty)
        {
            for (int i = target.childCount - 1; i >= 0; --i)
            {
                var tweenName = target.GetChild(i).GetComponent<TweenName>();
                if (tweenName != null && !query.sources.Contains(tweenName))
                {
                    tweensProperty.InsertArrayElementAtIndex(0);
                    tweensProperty.GetArrayElementAtIndex(0).objectReferenceValue = tweenName;
                }
            }

            if (target.childCount > 0)
            {
                for (int i = target.childCount - 1; i >= 0; --i)
                    UpdateTweenSources(target.GetChild(i), query, tweensProperty);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return __propertyHeight <= 0 ? base.GetPropertyHeight(property, label) : __propertyHeight;
        }

    }
}
#endif