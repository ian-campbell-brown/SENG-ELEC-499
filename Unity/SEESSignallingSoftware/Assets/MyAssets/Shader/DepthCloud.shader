Shader "Custom/DepthImage" {
	Properties {
		[MaterialToggle]
		_ProjectionOn ("Project", float) = 0.0
		_ProjectionFOV ("Projection FOV", Range(1, 89)) = 45
		
		_ProjectScale ("Project Scaling", float) = 2.0
		_DepthScale ("Depth Scaling", float) = 2.0
		
		_PointSize ("Point Size", float) = 1.0
	}
    SubShader {
    Pass {
        LOD 200
         
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
		
		#include "UnityCG.cginc"
		
		uniform sampler2D _MainTex;
		uniform float _ProjectionOn;
		uniform float _ProjectionFOV;
		uniform float _DepthScale;
		uniform float _ProjectScale;
		uniform float _PointSize;
 
        struct VertexInput {
            float4 pos : POSITION;
			float2 uv : TEXCOORD0;
        };
         
        struct VertexOutput {
            float4 pos : SV_POSITION;
			float4 col : COLOR;
			float size : PSIZE;
        };
         
        VertexOutput vert(VertexInput v) {
         
            VertexOutput o;
			float4 pos = v.pos;
			float2 texCol = tex2Dlod(_MainTex, float4(v.uv, 0, 0));
			
			pos.z = texCol.x * _DepthScale;
			
			if (texCol.x == 0)
				pos.z = 1.0f;
			
			if (_ProjectionOn != 0.0f)
			{
				float tfov = tan(radians(_ProjectionFOV));
				pos.x *= pos.z * tfov * _ProjectScale;
				pos.y *= pos.z * tfov * _ProjectScale;
			}
			
			pos.z -= 0.5f;
			
            o.pos = mul(UNITY_MATRIX_MVP, pos);
			o.size = _PointSize;
			o.col = float4(1, 1, 1, 1) * (texCol.x > 0 ? 1 : 0);
             
            return o;
        }
         
        float4 frag(VertexOutput o) : COLOR {
            return o.col;
        }
 
        ENDCG
        } 
    }
 
}