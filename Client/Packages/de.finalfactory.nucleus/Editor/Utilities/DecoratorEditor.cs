
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FinalFactory.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalFactory.Editor.Utilities
{
	/// <summary>
	///     A base class for creating editors that decorate Unity's built-in editor types.
	///     Source: https://gist.github.com/liortal53/352fda2d01d339306e03
	/// </summary>
	public abstract class DecoratorEditor : UnityEditor.Editor
	{
		#region Editor Fields

		/// <summary>
		/// Type object for the internally used (decorated) editor.
		/// </summary>
		private System.Type decoratedEditorType;

		/// <summary>
		/// Type object for the object that is edited by this editor.
		/// </summary>
		private System.Type editedObjectType;

		private UnityEditor.Editor editorInstance;

		#endregion

		private static Dictionary<string, MethodInfo> decoratedMethods = new Dictionary<string, MethodInfo>();

		private static Assembly editorAssembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));

		protected UnityEditor.Editor EditorInstance
		{
			get
			{
				if (editorInstance == null && targets != null && targets.Length > 0)
				{
					editorInstance = CreateEditor(targets, decoratedEditorType);
				}

				if (editorInstance == null)
				{
					Debug.LogError("Could not create editor !");
				}

				return editorInstance;
			}
		}

		public DecoratorEditor(string editorTypeName)
		{
			this.decoratedEditorType = editorAssembly.GetTypes().FirstOrDefault(t => t.Name == editorTypeName);

			if (decoratedEditorType == null)
			{
				throw new System.ArgumentException("Could not find internal editor type with name " + editorTypeName);
			}

			Init();

			// Check CustomEditor types.
			var originalEditedType = GetCustomEditorType(decoratedEditorType);

			if (originalEditedType != editedObjectType)
			{
				throw new System.ArgumentException(
					string.Format("Type {0} does not match the editor {1} type {2}",
						editedObjectType, editorTypeName, originalEditedType));
			}
		}

		private System.Type GetCustomEditorType(System.Type type)
		{
			var flags = BindingFlags.NonPublic | BindingFlags.Instance;

			var attributes = type.GetCustomAttributes(typeof(CustomEditor), true) as CustomEditor[];
			var field = attributes.Select(editor => editor.GetType().GetField("m_InspectedType", flags)).First();

			return field.GetValue(attributes[0]) as System.Type;
		}

		private void Init()
		{
			var flags = BindingFlags.NonPublic | BindingFlags.Instance;

			var attributes = this.GetType().GetCustomAttributes(typeof(CustomEditor), true) as CustomEditor[];
			var field = attributes.Select(editor => editor.GetType().GetField("m_InspectedType", flags)).First();

			editedObjectType = field.GetValue(attributes[0]) as System.Type;
		}

		private void OnEnable()
		{
			CallInspectorMethod("OnEnable");
		}

		private void OnDisable()
		{
			if (editorInstance != null)
			{
				CallInspectorMethod("OnDisable");
				DestroyImmediate(editorInstance);
			}
		}
		
		

		/// <summary>
		/// Delegates a method call with the given name to the decorated editor instance.
		/// </summary>
		protected void CallInspectorMethod(string methodName, object[] parameters = null)
		{
			MethodInfo method = null;
			if (parameters == null)
			{
				parameters = Array.Empty<object>();
			}

			// Add MethodInfo to cache
			if (!decoratedMethods.TryGetValue(methodName, out var decoratedMethod))
			{
				const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

				method = decoratedEditorType.GetMethod(methodName, flags);

				if (method != null)
				{
					decoratedMethods[methodName] = method;
				}
				else
				{
					return;
				}
			}
			else
			{
				method = decoratedMethod;
			}

			if (method != null)
			{
				method.Invoke(EditorInstance, parameters);
			}
		}

		public void OnSceneGUI()
		{
			CallInspectorMethod("OnSceneGUI");
		}

		protected override void OnHeaderGUI()
		{
			CallInspectorMethod("OnHeaderGUI");
		}

		public override void OnInspectorGUI()
		{
			EditorInstance.OnInspectorGUI();
		}

		public override void DrawPreview(Rect previewArea)
		{
			EditorInstance.DrawPreview(previewArea);
		}

		public override string GetInfoString()
		{
			return EditorInstance.GetInfoString();
		}

		public override GUIContent GetPreviewTitle()
		{
			return EditorInstance.GetPreviewTitle();
		}

		private void OnSceneDrag(SceneView sceneView, int index)
		{
			CallInspectorMethod("OnSceneDrag", new object[] {sceneView, index});
		}

		public override bool HasPreviewGUI()
		{
			return EditorInstance.HasPreviewGUI();
		}

		public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
		{
			EditorInstance.OnInteractivePreviewGUI(r, background);
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			EditorInstance.OnPreviewGUI(r, background);
		}

		public override void OnPreviewSettings()
		{
			EditorInstance.OnPreviewSettings();
		}

		public override void ReloadPreviewInstances()
		{
			EditorInstance.ReloadPreviewInstances();
		}

		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			return EditorInstance.RenderStaticPreview(assetPath, subAssets, width, height);
		}

		public override bool RequiresConstantRepaint()
		{
			return EditorInstance.RequiresConstantRepaint();
		}

		public override bool UseDefaultMargins()
		{
			return EditorInstance.UseDefaultMargins();
		}
	}
}