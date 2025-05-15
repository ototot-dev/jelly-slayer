using System;
using System.Collections.Generic;
using System.Linq;
using Finalfactory.Tagger;
using UnityEngine;

namespace FinalFactory.Tagger.Examples
{
    public class ListTaggedGameObjectScript : MonoBehaviour
    {
        private Vector2 _vec;
        private TaggerSearchMode _mode;
        private string _text;

        private void OnGUI()
        {
            //Just GUI stuff
            GUILayout.Space(250);
            GUILayout.Label("Search Example 2:");
            GUILayout.Label("Enter Tags like 'Red' 'Green' 'Small' and select the search mode.");
            GUILayout.Label("You can view the search code under:");
            GUILayout.Label("Plugins/HeiKyu/Tagger/Examples/ListTaggedGameObjectScript.cs");
            
            GUILayout.BeginHorizontal();
            ExampleGUI.SelectSearchMode(ref _mode);
            GUILayout.EndHorizontal();
            GUILayout.Label("Filtered Tags (separate with space):");
            _text = GUILayout.TextField(_text);
            List<string> tagList = _text?.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries).ToList();

            //Checking if the tag exists to avoid Unknown tag Logs.
            if (tagList != null)
            {
                for (int i = tagList.Count - 1; i >= 0; i--)
                {
                    string tag = tagList[i];
                    if (!TaggerSystem.Data.TagExists(tag))
                    {
                        tagList.RemoveAt(i);
                    }
                }
            }
            //If the tag list is null, initialize an empty list. So we get all tagged GameObjects.
            string[] tagArray = tagList == null ? new string[0] : tagList.ToArray();
            
            
            //Just GUI stuff
            GUILayout.Label("Invalid Tags are ignored.");
            GUILayout.Label("Valid filter Tags: " + String.Join(" ", tagArray));
            GUILayout.Label("Search result (GameObjects with Tags): ");
            _vec = GUILayout.BeginScrollView(_vec);
            
            
            //Here is the real search method for multiple tags.
            //There is also one for single tag search
            // TaggerSystem.FindGameObjectWithTag() 
            // TaggerSystem.FindGameObjectsWithTag()
            HashSet<GameObject> gameObjectsWithTags = TaggerSystem.FindGameObjectsWithTags(_mode, tagArray);
            foreach (GameObject taggedGameObject in gameObjectsWithTags)
            {
                //Get all tags of this gameobject
                HashSet<string> hashSet = taggedGameObject.GetTags();
                GUILayout.Label(taggedGameObject.name + ": " + String.Join(" ", hashSet.ToArray()));
            }


            //Just GUI stuff
            GUILayout.EndScrollView();
        }
    }
}
