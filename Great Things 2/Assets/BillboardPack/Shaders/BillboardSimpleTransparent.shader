Shader "Billboard/SimpleTransparent" {
	Properties {
		_MainTex ("Texture Image", 2D) = "white" {}
	}
	SubShader {
		Tags {"Queue" = "Transparent"} 
		Pass {   
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
 
			#pragma vertex vert  
			#pragma fragment frag 
         // User-specified uniforms            
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
 
			struct vertexInput {
				float4 vertex : POSITION;
				float4 tex : TEXCOORD0;
			};
			struct vertexOutput {
				float4 pos : SV_POSITION;
				float4 tex : TEXCOORD0;
			};
 
			vertexOutput vert(vertexInput input) 
			{
				vertexOutput output;
				
				float2 billboardDirection_local_xz = normalize(_WorldSpaceCameraPos.xz + float2(_World2Object[0].w, _World2Object[2].w));
				float2x2 billboardRotation = float2x2(
					billboardDirection_local_xz[1], billboardDirection_local_xz.x,
    				billboardDirection_local_xz.x, billboardDirection_local_xz[1]);
				output.pos.xz = mul(billboardRotation, input.vertex.xz);
				output.pos.yw = input.vertex.yw;
				output.pos = mul(UNITY_MATRIX_MVP, output.pos);
				
				//output.pos = mul(UNITY_MATRIX_MVP, input.vertex);
				
				//output.pos = mul(UNITY_MATRIX_P, 
			//		mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
			//		+ float4(input.vertex.x, input.vertex.y, 0.0, 0.0));
 
 			output.tex = input.tex;
//				output.tex = float4(input.vertex.x + 0.5,
 //              input.vertex.y + 0.5, 0.0, 0.0);
 
				return output;
			}
 
			float4 frag(vertexOutput input) : COLOR
			{
				return tex2D(_MainTex, float2(input.tex));   
			}
			ENDCG
		}
	}
}