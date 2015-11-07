Shader "Colour/GradientCutout" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
	    _Fade ("Fade", Float) = 0.4
	    _FadeSpeed ("Fade speed", Float) = 10
	    _TextureWidth ("Texture width", int) = 64
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
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alphatest:_Cutoff

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
		};
		
		float _Fade;
		int _FadeSpeed;
		int _TextureWidth;
		sampler2D _MainTex;
		fixed4 _ReplacementColour;
		fixed4 _TopColour;
		fixed4 _BottomColour;
		float _RangeStart;
		float _RangeEnd;
		float _BrightnessAdjust;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			
			int fadeValue = round(_Fade * _FadeSpeed);
		    int xRound = round(IN.uv_MainTex.x * _TextureWidth);
		    
			float r = c.r - _ReplacementColour.r;
			float g = c.g - _ReplacementColour.g;
		 	float b = c.b - _ReplacementColour.b;
		 	float v = 0.01;
		 	if (r < v && r > -v && r < v && r > -v &&
		 		g < v && g > -v && g < v && g > -v &&
		 		b < v && b > -v && b < v && b > -v) {
			 	float top = (IN.uv_MainTex.y - _RangeStart) / (_RangeEnd - _RangeStart);
			 	float bottom = 1 - top; 
			 	fixed4 g = ((_TopColour * top) + (_BottomColour * bottom)) * ((1 + _BrightnessAdjust)); 
			 	g.a = 1;
			 	
			 	o.Albedo = g.rgb;
		 	} else {
		 		o.Albedo = c.rgb;
			}
			o.Alpha = (xRound % 2) == 0 ? c.a : 0;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
