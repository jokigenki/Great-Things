Shader "Colour/GradientReplacementShader" {
	Properties {
	     _MainTex ("Sprite Texture", 2D) = "white" {}
	     _ReplacementColour ("Replacement Colour", Color) = (1,1,1,1)
	     _TopColour ("Top Color", Color) = (1,1,1,1)
	     _BottomColour ("Bottom Color", Color) = (1,1,1,1)
	     _RangeStart("Range Start", Range(0,1)) = 0
	     _RangeEnd("Range End", Range(0,1)) = 1
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
		fixed4 _ReplacementColour;
		fixed4 _TopColour;
		fixed4 _BottomColour;
		float _RangeStart;
		float _RangeEnd;
		float _BrightnessAdjust;
		 
		struct Input {
		    float2 uv_MainTex;
		};
		 
		void surf (Input IN, inout SurfaceOutput o) {
		 
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			int r = (int)((c.r - _ReplacementColour.r) * 100);
			int g = (int)((c.g - _ReplacementColour.g) * 100);
		 	int b = (int)((c.b - _ReplacementColour.b) * 100);
		 	// test whether colour is within bounds
		 	if (r == 0 && g == 0 && b == 0) {
			 	float top = (IN.uv_MainTex.y - _RangeStart) / (_RangeEnd - _RangeStart);
			 	float bottom = 1 - top; 
			 	fixed4 g = ((_TopColour * top) + (_BottomColour * bottom)) * ((1 + _BrightnessAdjust)); 
			 	g.a = 1;
			 	
			 	o.Albedo = g.rgb;
		 	} else {
		 		o.Albedo = c.rgb;
			}
			o.Alpha = c.a;
		 }
		 
		 ENDCG
	 }
 
 	Fallback "Transparent/Cutout/VertexLit"
}
