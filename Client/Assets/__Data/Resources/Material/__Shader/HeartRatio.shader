Shader "Game/HeartRatio" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Tex (RGBA)", 2D) = "white" {}
		_Progress ("Progress", Range(0.0,1.0)) = 0.0
	}
	
	SubShader {
		Tags { "Queue"="Overlay+1" }
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			uniform sampler2D _MainTex;
			uniform float4 _Color;
			uniform float _Progress;
			
			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv = TRANSFORM_UV(0);
				
				return o;
			}
			
			half4 frag( v2f i ) : COLOR
			{
				half4 color = tex2D( _MainTex, i.uv);
				color.a *= i.uv.y < _Progress;
				return color*_Color;
			}
			
			ENDCG
			
		}
	}
	
}