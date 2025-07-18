// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;
using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Matrix2X2", "Constants And Properties", "Matrix2X2 property" )]
	public sealed class Matrix2X2Node : MatrixParentNode
	{
		private string[,] m_fieldText = new string[ 2, 2 ] { { "0", "0" }, { "0", "0" } };
		public Matrix2X2Node() : base() { }
		public Matrix2X2Node( int uniqueId, float x, float y, float width, float height ) : base( uniqueId, x, y, width, height ) { }
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			GlobalTypeWarningText = string.Format( GlobalTypeWarningText, "Matrix" );
			AddOutputPort( WirePortDataType.FLOAT2x2, Constants.EmptyPortValue );
			m_insideSize.Set( Constants.FLOAT_DRAW_WIDTH_FIELD_SIZE * 2 + Constants.FLOAT_WIDTH_SPACING * 2, Constants.FLOAT_DRAW_HEIGHT_FIELD_SIZE * 2 + Constants.FLOAT_WIDTH_SPACING * 2 + Constants.OUTSIDE_WIRE_MARGIN );
			//m_defaultValue = new Matrix4x4();
			//m_materialValue = new Matrix4x4();
			m_drawPreview = false;
		}

		public override void CopyDefaultsToMaterial()
		{
			m_materialValue = m_defaultValue;
		}

		public override void DrawSubProperties()
		{
			EditorGUILayout.LabelField( Constants.DefaultValueLabel );
			for( int row = 0; row < 2; row++ )
			{
				EditorGUILayout.BeginHorizontal();
				for( int column = 0; column < 2; column++ )
				{
					m_defaultValue[ row, column ] = EditorGUILayoutFloatField( string.Empty, m_defaultValue[ row, column ], GUILayout.MaxWidth( 76 ) );
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		public override void DrawMaterialProperties()
		{
			if( m_materialMode )
				EditorGUI.BeginChangeCheck();

			EditorGUILayout.LabelField( Constants.MaterialValueLabel );
			for( int row = 0; row < 2; row++ )
			{
				EditorGUILayout.BeginHorizontal();
				for( int column = 0; column < 2; column++ )
				{
					m_materialValue[ row, column ] = EditorGUILayoutFloatField( string.Empty, m_materialValue[ row, column ], GUILayout.MaxWidth( 76 ) );
				}
				EditorGUILayout.EndHorizontal();
			}

			if( m_materialMode && EditorGUI.EndChangeCheck() )
				m_requireMaterialUpdate = true;
		}

		public override void OnNodeLayout( DrawInfo drawInfo )
		{
			base.OnNodeLayout( drawInfo );

			m_propertyDrawPos.position = m_remainingBox.position;
			m_propertyDrawPos.width = drawInfo.InvertedZoom * Constants.FLOAT_DRAW_WIDTH_FIELD_SIZE;
			m_propertyDrawPos.height = drawInfo.InvertedZoom * Constants.FLOAT_DRAW_HEIGHT_FIELD_SIZE;
		}

		public override void DrawGUIControls( DrawInfo drawInfo )
		{
			base.DrawGUIControls( drawInfo );

			if( drawInfo.CurrentEventType != EventType.MouseDown )
				return;

			Rect hitBox = m_remainingBox;
			hitBox.height = m_insideSize.y * drawInfo.InvertedZoom;
			bool insideBox = hitBox.Contains( drawInfo.MousePosition );

			if( insideBox )
			{
				GUI.FocusControl( null );
				m_isEditingFields = true;
			}
			else if( m_isEditingFields && !insideBox )
			{
				GUI.FocusControl( null );
				m_isEditingFields = false;
			}
		}


		public override void Draw( DrawInfo drawInfo )
		{
			base.Draw( drawInfo );

			if( !m_isVisible )
				return;

			if( m_isEditingFields && m_currentParameterType != PropertyType.Global )
			{
				bool currMode = m_materialMode && m_currentParameterType != PropertyType.Constant;
				Matrix4x4 value = currMode ? m_materialValue : m_defaultValue;

				EditorGUI.BeginChangeCheck();
				for( int row = 0; row < 2; row++ )
				{
					for( int column = 0; column < 2; column++ )
					{
						m_propertyDrawPos.position = m_remainingBox.position + Vector2.Scale( m_propertyDrawPos.size, new Vector2( column, row ) ) + new Vector2( Constants.FLOAT_WIDTH_SPACING * drawInfo.InvertedZoom * column, Constants.FLOAT_WIDTH_SPACING * drawInfo.InvertedZoom * row );
						value[ row, column ] = EditorGUIFloatField( m_propertyDrawPos, string.Empty, value[ row, column ], UIUtils.MainSkin.textField );
					}
				}

				if( currMode )
				{
					m_materialValue = value;
				}
				else
				{
					m_defaultValue = value;
				}

				if( EditorGUI.EndChangeCheck() )
				{
					m_requireMaterialUpdate = m_materialMode;
					BeginDelayedDirtyProperty();
				}
			}
			else if( drawInfo.CurrentEventType == EventType.Repaint )
			{
				bool guiEnabled = GUI.enabled;
				//redundant ternary conditional but this makes easier to read that only when m_showCuffer is on that we need to take PropertyType.Property into account
				GUI.enabled = ( m_showCBuffer )? m_currentParameterType != PropertyType.Global&& m_currentParameterType != PropertyType.Property : m_currentParameterType != PropertyType.Global;

				bool currMode = m_materialMode && m_currentParameterType != PropertyType.Constant;
				Matrix4x4 value = currMode ? m_materialValue : m_defaultValue;
				for( int row = 0; row < 2; row++ )
				{
					for( int column = 0; column < 2; column++ )
					{
						Rect fakeField = m_propertyDrawPos;
						fakeField.position = m_remainingBox.position + Vector2.Scale( m_propertyDrawPos.size, new Vector2( column, row ) ) + new Vector2( Constants.FLOAT_WIDTH_SPACING * drawInfo.InvertedZoom * column, Constants.FLOAT_WIDTH_SPACING * drawInfo.InvertedZoom * row );
						if( GUI.enabled )
							EditorGUIUtility.AddCursorRect( fakeField, MouseCursor.Text );

						if( m_previousValue[ row, column ] != value[ row, column ] )
						{
							m_previousValue[ row, column ] = value[ row, column ];
							m_fieldText[ row, column ] = value[ row, column ].ToString();
						}

						GUI.Label( fakeField, m_fieldText[ row, column ], UIUtils.MainSkin.textField );
					}
				}
				GUI.enabled = guiEnabled;
			}
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			base.GenerateShaderForOutput( outputId, ref dataCollector, ignoreLocalvar );
			m_precisionString = UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, m_outputPorts[ 0 ].DataType );
			if( m_currentParameterType != PropertyType.Constant )
			{
				if( m_outputPorts[ 0 ].IsLocalValue( dataCollector.PortCategory ) )
					return m_outputPorts[ 0 ].LocalValue( dataCollector.PortCategory );
				string localVarName = PropertyData( dataCollector.PortCategory ) + "Local2x2";
				string localVarValue = string.Format( "float2x2( {0}._m00, {0}._m01, {0}._m10, {0}._m11 )", PropertyData( dataCollector.PortCategory ) );
				RegisterLocalVariable( 0, localVarValue, ref dataCollector, localVarName );
				return localVarName;
			}

			Matrix4x4 value = m_defaultValue;

			return m_precisionString + "( " + value[ 0, 0 ] + ", " + value[ 0, 1 ] + ", " + value[ 1, 0 ] + ", " + value[ 1, 1 ] + " )";

		}


		public override void UpdateMaterial( Material mat )
		{
			base.UpdateMaterial( mat );
			if( UIUtils.IsProperty( m_currentParameterType ) && !InsideShaderFunction )
			{
				Shader.SetGlobalMatrix( m_propertyName, m_materialValue );
				//mat.SetMatrix( m_propertyName, m_materialValue );
			}
		}

		public override bool GetUniformData( out string dataType, out string dataName, ref bool fullValue )
		{
			dataType = UIUtils.PrecisionWirePortToCgType( CurrentPrecisionType, WirePortDataType.FLOAT4x4 );
			dataName = m_propertyName;
			return true;
		}

		public override void SetMaterialMode( Material mat, bool fetchMaterialValues )
		{
			base.SetMaterialMode( mat, fetchMaterialValues );
			if( fetchMaterialValues && m_materialMode && UIUtils.IsProperty( m_currentParameterType ) && mat.HasProperty( m_propertyName ) )
			{
				m_materialValue = mat.GetMatrix( m_propertyName );
			}
		}

		public override void ForceUpdateFromMaterial( Material material )
		{
			if( UIUtils.IsProperty( m_currentParameterType ) && material.HasProperty( m_propertyName ) )
			{
				m_materialValue = material.GetMatrix( m_propertyName );
				PreviewIsDirty = true;
			}
		}


		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			m_defaultValue = IOUtils.StringToMatrix2x2( GetCurrentParam( ref nodeParams ) );
		}

		public override void WriteToString( ref string nodeInfo, ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo, ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, IOUtils.Matrix2x2ToString( m_defaultValue ) );
		}

		public override void ReadAdditionalClipboardData( ref string[] nodeParams )
		{
			base.ReadAdditionalClipboardData( ref nodeParams );
			m_materialValue = IOUtils.StringToMatrix2x2( GetCurrentParam( ref nodeParams ) );
		}

		public override void WriteAdditionalClipboardData( ref string nodeInfo )
		{
			base.WriteAdditionalClipboardData( ref nodeInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo, IOUtils.Matrix2x2ToString( m_materialValue ) );
		}

		public override string GetPropertyValStr()
		{

			return ( m_materialMode && m_currentParameterType != PropertyType.Constant ) ?  m_materialValue[ 0, 0 ].ToString( Mathf.Abs( m_materialValue[ 0, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_materialValue[ 0, 1 ].ToString( Mathf.Abs( m_materialValue[ 0, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR +
																							m_materialValue[ 1, 0 ].ToString( Mathf.Abs( m_materialValue[ 1, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_materialValue[ 1, 1 ].ToString( Mathf.Abs( m_materialValue[ 1, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) :

																							m_defaultValue[ 0, 0 ].ToString( Mathf.Abs( m_defaultValue[ 0, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 0, 1 ].ToString( Mathf.Abs( m_defaultValue[ 0, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.MATRIX_DATA_SEPARATOR +
																							m_defaultValue[ 1, 0 ].ToString( Mathf.Abs( m_defaultValue[ 1, 0 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel ) + IOUtils.VECTOR_SEPARATOR + m_defaultValue[ 1, 1 ].ToString( Mathf.Abs( m_defaultValue[ 1, 1 ] ) > 1000 ? Constants.PropertyBigMatrixFormatLabel : Constants.PropertyMatrixFormatLabel );
		}

	}
}
