Shader "Colour/BrightnessAdjust" {
	Properties {
	     _MainTex ("Sprite Texture", 2D) = "white" {}
	     _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	     _BrightnessAdjust ("Brightness Adjust", Range(-1,1)) = 0
	 }
	 
	 SubShader {
	     Tags {
	     	"Queue" = "Transparent+1"
	     	"RenderType"="TransparentCutout"
	     }
	     
	     Blend SrcAlpha OneMinusSrcAlpha 
	     
		 CGPROGRAM
		 #pragma surface surf Lambert alphatest:_Cutoff
		 
		 sampler2D _MainTex;
		 float _BrightnessAdjust;
		 
		 struct Input {
		     float2 uv_MainTex;
		 };
		 
		 void surf (Input IN, inout SurfaceOutput o) {
		 
		 	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		 	o.Albedo = c * ((1 + _BrightnessAdjust));
			o.Alpha = c.a;
		 }
		 
		 ENDCG
	 }
 
 	Fallback "Transparent/Cutout/VertexLit"
}
