Shader "Custom/SnowShader" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
 		_Bump ("Bump", 2D) = "bump" {}
        _Snow ("Snow Level", Range(0,1) ) = 0
	    _SnowColor ("Snow Color", Color) = (1.0,1.0,1.0,1.0)
    	_SnowDirection ("Snow Direction", Vector) = (0,1,0)
    	_SnowDepth ("Snow Depth", Range(0,2)) = 0.1
    	_Wetness ("Wetness", Range(0, 0.5)) = 0.3
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
 
        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
 
        sampler2D _MainTex;
        sampler2D _Bump;
        float _Snow;
		float4 _SnowColor;
		float4 _SnowDirection;
		float _SnowDepth;
 		float _Wetness;
 
        struct Input {
            float2 uv_MainTex;
            float2 uv_Bump;
            float3 worldNormal;
        	INTERNAL_DATA
        };
 
        void surf (Input IN, inout SurfaceOutput o) {
            //Normal color of a pixel
    		half4 c = tex2D (_MainTex, IN.uv_MainTex);
 
    		//Get the normal from the bump map
    		o.Normal = UnpackNormal (tex2D (_Bump, IN.uv_Bump));
 
    		float difference = dot(WorldNormalVector(IN, o.Normal), _SnowDirection.xyz) - lerp(1,-1,_Snow);
            difference = saturate(difference / _Wetness);
            o.Albedo = difference*_SnowColor.rgb + (1-difference) *c; 
            o.Alpha = c.a;
        }
        
        void vert (inout appdata_full v) {
      		//in vert
			fixed3 sn = normalize(_SnowDirection.xyz);
			//normalize let us change direction of snow properly from inspector
			fixed3 normalWorld = normalize(mul(v.normal,float3x3(_World2Object)));
			fixed NdotS = dot(normalWorld, sn);//computed once for reuse
			if(NdotS >= lerp(1,-1,(1-_Wetness)*(_Snow*2)/3))
			v.vertex.xyz +=NdotS*v.normal*_SnowDepth*_Snow;//NdotS for smoother vertex transitions
        }
        ENDCG
    } 
    FallBack "Diffuse"
}