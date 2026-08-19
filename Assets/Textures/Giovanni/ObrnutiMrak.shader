Shader "Custom/ObrnutiMrak"
{
    Properties
    {
        _Color ("Boja Mraka", Color) = (0,0,0,1)
        _InnerRadius ("Krug oko lika", Float) = 2.5
        _FlashAngle ("Kut svjetiljke", Range(0,1)) = 0.85
        _FlashlightOn ("Flashlight On", Float) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off 
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _Color;
            float _InnerRadius;
            float _FlashAngle;
            float _FlashlightOn;
            float3 _PlayerPos;
            float3 _FlashDir;

            struct v2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float d = distance(i.worldPos, _PlayerPos);
                
                // Osnovna maska (krug oko lika)
                float mask = smoothstep(_InnerRadius, _InnerRadius + 1.0, d);

                // Maska za svjetiljku - radi samo ako je _FlashlightOn == 1
                if (_FlashlightOn > 0.5) {
                    float3 dirToPixel = normalize(i.worldPos - _PlayerPos);
                    float dotProd = dot(dirToPixel, _FlashDir);
                    
                    if (dotProd > _FlashAngle && d < 15.0) {
                        mask = 0;
                    }
                }

                return fixed4(_Color.rgb, mask * _Color.a);
            }
            ENDCG
        }
    }
}