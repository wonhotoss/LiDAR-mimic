Shader "lidar/point" {
    // Draws pc_buffer points as fixed screen-size, axis-aligned squares, colored per id. id 0 -> degenerate.
    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass {
            ZTest LEqual
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct pc_point {
                float3 world;
                uint id;
            };

            StructuredBuffer<pc_point> pc;
            StructuredBuffer<float4> style; // per id: rgb = color, w = size (screen pixels)
            float depth_bias; // clip-space z nudge toward camera (sign is platform-dependent; tune in inspector)

            struct v_out {
                float4 pos : SV_POSITION;
                float3 color : TEXCOORD0;
            };

            static const float2 corners[6] = {
                float2(-1, -1), float2(1, -1), float2(1, 1),
                float2(-1, -1), float2(1, 1), float2(-1, 1)
            };

            v_out vert(uint vid : SV_VertexID) {
                uint pi = vid / 6;
                uint corner = vid % 6;
                pc_point p = pc[pi];
                float4 s = style[p.id];

                float4 clip = TransformWorldToHClip(p.world);
                clip.xy += corners[corner] * (s.w * 0.5) * 2.0 / _ScreenParams.xy * clip.w;
                clip.z += depth_bias * clip.w;

                v_out o;
                o.pos = (p.id == 0) ? (float4) 0 : clip; // degenerate for background / non-receiver
                o.color = s.xyz;
                return o;
            }

            float4 frag(v_out i) : SV_Target {
                return float4(i.color, 1);
            }
            ENDHLSL
        }
    }
}
