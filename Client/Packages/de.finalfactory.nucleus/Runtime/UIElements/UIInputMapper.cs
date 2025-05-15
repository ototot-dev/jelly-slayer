using System;
using FinalFactory.Logging;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace FinalFactory.UIElements
{
    public class UIInputMapper : MonoBehaviour
    {
        private static readonly Log Log = LogManager.GetLogger(typeof(UIInputMapper));
        
        [Tooltip("[Required] The UIDocument to map input to.")]
        public UIDocument Document;
        [Tooltip("[Optional] The camera to use for mapping input to the UIDocument. If not set, the main camera will be used.")]
        public Camera Camera;

        [Header("Cursor Settings")]
        [Tooltip("Whether to enable the cursor when the UIDocument is active. The cursor must be a VisualElement with the name 'cursor'.")]
        public bool EnableCursor = false;
        
        [Header("Raycast Settings")] 
        [Tooltip("The distance to use for ray-casting from the camera to the UI")]
        public float Distance = 100;

        [Tooltip("The layer mask to use for ray-casting from the camera to the UI")]
        public LayerMask LayerMask = -1;
        
        [Header("Debug")]
        [Tooltip("Whether to draw the ray used for ray-casting from the camera to the UI. Enable this only for debugging. Editor only.")]
        public bool DrawRay = false;
        [Tooltip("Whether to suppress the warning that the cursor is enabled but no element with the name 'cursor' was found. This may be useful if you want to enable the cursor, but the cursor is not always visible.")]
        public bool SuppressWarning = false;
        private void OnEnable()
        {
            if (Document == null)
            {
                Document = GetComponent<UIDocument>();
            }
            
            if (Document == null)
            {
                Log.Error("UIDocument is missing");
                return;
            }

            if (Camera == null)
            {
                Camera = Camera.main;
            }
            
            if (Camera == null)
            {
                Log.Error("Camera is missing");
                return;
            }

            Document.panelSettings.SetScreenToPanelSpaceFunction((Vector2 screenPosition) =>
            {
                var invalidPosition = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
                
                var cameraRay = Camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
#if UNITY_EDITOR
                if (DrawRay)
                {
                    Debug.DrawRay(cameraRay.origin, cameraRay.direction * Distance, Color.magenta, 1);
                }
#endif
                
                if (!Physics.Raycast(cameraRay, out var hit, Distance, LayerMask))
                {
                    return invalidPosition;
                }
                
                var uv = hit.textureCoord;
                uv.y = 1 - uv.y;
                uv.x *= Document.panelSettings.targetTexture.width;
                uv.y *= Document.panelSettings.targetTexture.height;

                if (EnableCursor)
                {
                    var cursor = Document.rootVisualElement.Q("cursor");
                    if (cursor != null)
                    {
                        cursor.style.left = uv.x;
                        cursor.style.top = uv.y;
                    }
                    else if(!SuppressWarning)
                    {
                        Log.Warn("Cursor is enabled but no element with the name 'cursor' was found");
                    }
                }
                
                return uv;
            });
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (LayerMask == -1)
            {
                LayerMask = LayerMask.GetMask("UI");
            }
        }
#endif
    }           
}
