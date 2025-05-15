// // ***********************************************************************
// // Assembly         : Tagger.Plugins
// // Author           : HeiaSamahi
// // Created          : 14.05.2017 : 16:19
// //
// // Last Modified By : Heia Samahi
// // Last Modified On : 26.05.2017 : 10:40
// // ***********************************************************************
// // <copyright file="ExampleGUI.cs" company="Heikyu">
// //     Copyright (c) Heikyu. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using Finalfactory.Tagger;

namespace FinalFactory.Tagger.Examples
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using UnityEngine;

    // ReSharper disable once UnusedMember.Global
    public class ExampleGUI : MonoBehaviour
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedField.Global
        public Transform Target;

        private readonly List<TaggerAdvancedSearch> _searches = new List<TaggerAdvancedSearch>();

        private TaggerSearchMode _mode = TaggerSearchMode.And;

        private void Start()
        {
            AddTestTags();
        }

        public static void AddTestTags()
        {
            TaggerSystem.Data.AddTag("Red", "Color");
            TaggerSystem.Data.AddTag("Blue", "Color");
            TaggerSystem.Data.AddTag("Green", "Color");

            TaggerSystem.Data.SetColor("Red", Color.red);
            TaggerSystem.Data.SetColor("Blue", Color.blue);
            TaggerSystem.Data.SetColor("Green", Color.green);

            TaggerSystem.Data.AddTag("Small", "Size");
            TaggerSystem.Data.AddTag("Normal", "Size");
            TaggerSystem.Data.AddTag("Large", "Size");

            TaggerSystem.Data.AddTag("Cylinder", "Shape");
            TaggerSystem.Data.AddTag("Cube", "Shape");
            TaggerSystem.Data.AddTag("Sphere", "Shape");
            TaggerSystem.Data.AddTag("Capsule", "Shape");
        }

        // ReSharper disable once UnusedMember.Global
        public void OnGUI()
        {
            if (_searches.Count == 0 && _mode == TaggerSearchMode.Or)
            {
                _mode = TaggerSearchMode.And;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("None"))
            {
                _searches.Clear();
                HighlightObjects();
            }

            GUILayout.Space(10f);

            DrawTagGroup("Color Tags", "Red", "Blue", "Green");
            DrawTagGroup("Size Tags", "Small", "Normal", "Large");
            DrawTagGroup("Shape Tags", "Cylinder", "Cube", "Sphere", "Capsule");

            GUILayout.Space(10f);
            GUILayout.BeginVertical();
            GUILayout.Label("Operators");
            
            SelectSearchMode(ref _mode, _searches.Count > 0);

            GUILayout.EndVertical();
            GUILayout.Space(10f);
            GUI.enabled = _searches.Count > 0;
            if (GUILayout.Button("Remove Last"))
            {
                _searches.RemoveAt(_searches.Count - 1);
                if (_searches.Count > 0)
                {
                    _searches[_searches.Count - 1].SetChild(TaggerSearchMode.And, null);
                }

                HighlightObjects();
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();
            GUILayout.Label(GetVisualString());
            GUILayout.Label(GetCasesString());
            GUILayout.Label(GetTagArrayString());
        }

        public static void SelectSearchMode(ref TaggerSearchMode mode, bool canSelectOr = true)
        {
            GUI.color = Color.grey;
            if (mode == TaggerSearchMode.And)
            {
                GUI.color = Color.white;
            }

            if (GUILayout.Button("And"))
            {
                mode = TaggerSearchMode.And;
            }

            GUI.color = Color.grey;
            GUI.enabled = canSelectOr;

            if (mode == TaggerSearchMode.Or || !GUI.enabled)
            {
                GUI.color = Color.white;
            }

            if (GUILayout.Button("Or"))
            {
                mode = TaggerSearchMode.Or;
            }

            GUI.enabled = true;
            GUI.color = Color.grey;
            if (mode == TaggerSearchMode.Not)
            {
                GUI.color = Color.white;
            }

            if (GUILayout.Button("Not"))
            {
                mode = TaggerSearchMode.Not;
            }

            GUI.color = Color.white;
        }

        private void DrawTag(string tag)
        {
            GUI.color = TaggerSystem.Data.GetColor(tag);
            if (GUILayout.Button(tag))
            {
                SetTag(tag);
            }

            GUI.color = Color.white;
        }

        private void DrawTagGroup(string label, params string[] tags)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label);
            foreach (string s in tags)
            {
                DrawTag(s);
            }

            GUILayout.EndVertical();

            GUILayout.Space(10f);
        }

        private string GetCasesString()
        {
            string[] allTags = TaggerSystem.GetAllTags();
            if (_searches.Count > 0)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Possible tag compositions that are searched:");
                foreach (TagArray casee in _searches[0].Cases)
                {
                    sb.Append("Case # ");
                    for (int i = 0; i < TagArray.Length; i++)
                    {
                        if (casee.Get(i))
                        {
                            sb.Append(allTags[i] + " ; ");
                        }
                    }

                    sb.Append(" Excluded: ");

                    for (int i = 0; i < TagArray.Length; i++)
                    {
                        if (!casee.GetMask(i))
                        {
                            sb.Append(allTags[i] + " ; ");
                        }
                    }

                    sb.AppendLine();
                }

                return sb.ToString();
            }

            return "No tag cases";
        }

        private string GetTagArrayString()
        {
            if (_searches.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Possible tag compositions that fit: ");
                sb.AppendLine(_searches[0].TagArrayIDs.Count.ToString());

                return sb.ToString();
            }

            return "No tagarray ids";
        }

        private string GetVisualString()
        {
            if (_searches.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Logical visualization of the search:");
                ProcessString(sb, _searches[0]);

                return sb.ToString();
            }

            return "No selection";
        }

        private void HighlightObjects()
        {
            foreach (SelectionHighlighter selectionHighlighter in FindObjectsOfType<SelectionHighlighter>())
            {
                selectionHighlighter.TurnOff();
            }

            foreach (SelectionRunner runner in FindObjectsOfType<SelectionRunner>())
            {
                runner.Return();
            }

            if (_searches.Count > 0)
            {
                HashSet<GameObject> gameObjects = _searches[0].MatchedGameObjects();
                foreach (GameObject o in gameObjects)
                {
                    SelectionHighlighter selectionHighlighter = o.GetComponent<SelectionHighlighter>();
                    if (selectionHighlighter != null) selectionHighlighter.TurnOn();
                    SelectionRunner selectionRunner = o.GetComponent<SelectionRunner>();
                    if (selectionRunner != null) selectionRunner.SetTargetPos(Target.position);
                }
            }
        }

        private void ProcessString(StringBuilder sb, TaggerAdvancedSearch s)
        {
            switch (s.Mode)
            {
                case TaggerSearchMode.And:
                    sb.Append(" & ");
                    break;
                case TaggerSearchMode.Or:
                    sb.Append(" | ");
                    break;
                case TaggerSearchMode.Not:
                    sb.Append(" ! ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            sb.Append(" ( ");
            if (s.Tags != null)
            {
                sb.Append(" [ ");
                foreach (string sTag in s.Tags)
                {
                    sb.Append(sTag + "; ");
                }

                sb.Append(" ] ");
            }

            if (s.Child != null)
            {
                ProcessString(sb, s.Child);
            }

            sb.Append(" ) ");
        }

        private void SetTag(string tag)
        {
            TaggerAdvancedSearch search = new TaggerAdvancedSearch(_mode == TaggerSearchMode.And);
            search.SetTags(tag);
            if (_searches.Count > 0)
            {
                _searches[_searches.Count - 1].SetChild(_mode, search);
            }

            _searches.Add(search);

            HighlightObjects();
        }
    }
}