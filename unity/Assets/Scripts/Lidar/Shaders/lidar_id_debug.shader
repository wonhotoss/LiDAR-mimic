Shader "lidar/id_debug" {
    // Visualizes the LiDAR map: scene as depth grayscale, receivers (id > 0) tinted by a per-id hue. Verification aid.
    Properties { _MainTex ("tex", 2D) = "black" {} }

    SubShader {
        Tags { "RenderType" = "Opaque" }

        Pass {
            ZTest Always
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct v_out { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v_out vert(appdata_img v) {
                v_out o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag(v_out i) : SV_Target {
                float2 s = tex2D(_MainTex, i.uv).rg; // r = id, g = NDC depth
                float h = frac(s.r * 0.61803399);
                float3 hue = saturate(abs(frac(h + float3(0, 2.0 / 3.0, 1.0 / 3.0)) * 6 - 3) - 1);
                float3 gray = s.g.xxx * 0.6; // scene context (near surfaces brighter under reversed-Z)
                return fixed4(s.r < 0.5 ? gray : hue, 1);
            }
            ENDCG
        }
    }
}
