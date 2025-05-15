using Finalfactory.Tagger;
using UnityEngine;

namespace Plugins.HeiKyu.Tagger.Examples
{
    [ExecuteInEditMode]
    public class FilterExample : MonoBehaviour
    {
        public bool FilterMatching;
        public TaggerFilter Filter;

        private void Awake()
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
            
            gameObject.AddTag("Blue");
        }

        private void Update()
        {
            FilterMatching = Filter.Match(gameObject);
        }

        private void OnValidate()
        {
            FilterMatching = Filter.Match(gameObject);
        }
    }
}
