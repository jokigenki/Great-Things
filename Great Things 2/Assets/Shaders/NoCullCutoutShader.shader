Shader "GreatThings/NoCullCutout/Diffuse" {
 Properties {
     _Color ("Main Color", Color) = (1,1,1,1)
     _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
     _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
     _Fade ("Fade", Float) = 0
     _FadeSpeed ("Fade speed", Float) = 10
     _TextureWidth ("Texture width", int) = 64
 }
 
 SubShader {
     Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
     LOD 200
     Cull Off
     
 CGPROGRAM
 #pragma surface surf Lambert alphatest:_Cutoff
 
 sampler2D _MainTex;
 fixed4 _Color;
 float _Fade;
 int _FadeSpeed;
 int _TextureWidth;
 int _Thing;
 
 struct Input {
     float2 uv_MainTex;
 };
 
 void surf (Input IN, inout SurfaceOutput o) {
     fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
     
     int fadeValue = round(_Fade * _FadeSpeed);
     int xRound = round(IN.uv_MainTex.x * _TextureWidth);
     if (_Fade == 0 || (xRound % fadeValue) == 0) {
	     o.Alpha = c.a;
     } else {
          o.Alpha = 0;
	 }
     o.Albedo = c.rgb;
 }
 
 ENDCG
 }
 
 Fallback "Transparent/Cutout/VertexLit"
 }