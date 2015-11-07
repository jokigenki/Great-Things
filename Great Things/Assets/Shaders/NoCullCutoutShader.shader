Shader "GreatThings/NoCullCutout/Diffuse" {
	 Properties {
	     _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	     _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	     _Fade ("Fade", Float) = 0.4
	     _FadeSpeed ("Fade speed", Float) = 10
	     _TextureWidth ("Texture width", int) = 64
	 }
	 
	 SubShader {
	     Tags {
	     	"Queue"="AlphaTest"
	     	"IgnoreProjector"="True"
	     	"RenderType"="TransparentCutout"
	     }
	     LOD 200
	     Cull Off
	     
		 CGPROGRAM
		 #pragma surface surf Lambert alphatest:_Cutoff
		 
		 sampler2D _MainTex;
		 float _Fade;
		 int _FadeSpeed;
		 int _TextureWidth;
		 
		 struct Input {
		     float2 uv_MainTex;
		     float3 worldPos;
		 };
		 
		 void surf (Input IN, inout SurfaceOutput o) {
		     fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		     
		     int fadeValue = round(_Fade * _FadeSpeed);
		     int xRound = round(IN.uv_MainTex.x * _TextureWidth);
		     o.Alpha = (xRound % 2) == 0 ? c.a : 0;
		     o.Albedo = c.rgb;
		 }
		 
		 ENDCG
	 }
 
 	Fallback "Transparent/Cutout/VertexLit"
 }