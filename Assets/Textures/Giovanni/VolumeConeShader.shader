Shader "Custom/SoftVolumetricBeam"
{
    Properties
    {
        _Color ("Boja Snopa", Color) = (0.1, 0.3, 0.5, 0.2)
        _Brightness ("Intenzitet", Range(0, 5)) = 1.0
        _FadeExp ("Gustoća rubova", Range(1, 10)) = 4
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        
        // Additive blending: snop dodaje svjetlost, ne prekriva maglu
        Blend One One 
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD3;
            };

            float4 _Color;
            float _Brightness;
            float _FadeExp;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 1. Fresnel efekt (čini snop prozirnim kad gledaš direktno kroz njega)
                float dotProd = 1.0 - saturate(dot(i.normal, i.viewDir));
                float alpha = pow(dotProd, _FadeExp);

                // 2. Distance Fade (gasi snop ako je preblizu kameri da ne blješti)
                float distToCam = distance(i.worldPos, _WorldSpaceCameraPos);
                float cameraFade = saturate((distToCam - 0.5) * 2.0);

                return _Color * _Brightness * alpha * cameraFade;
            }
            ENDCG
        }
    }
}