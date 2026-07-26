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

            float point_mode;   // 0 = per-object color/size, 1 = depth colormap
            float global_size;  // point size (px) in depth-map mode
            float3 lidar_pos;   // sensor position, for range-based coloring
            float depth_min;    // range (m) mapped to colormap start
            float depth_max;    // range (m) mapped to colormap end
            float depth_emission; // multiplies the colormap color (>1 -> bloom)
            float depth_offset;   // colormap phase (cycles); the ramp is periodic, so scrolling it is seamless
            Texture2D colormap;
            SamplerState sampler_colormap;

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

                bool depth = point_mode > 0.5;
                float size = depth ? global_size : style[p.id].w;
                float3 color;
                if (depth) {
                    float d = distance(p.world, lidar_pos);
                    float t = frac(saturate((d - depth_min) / max(depth_max - depth_min, 1e-5)) + depth_offset);
                    color = colormap.SampleLevel(sampler_colormap, float2(t, 0.5), 0).rgb * depth_emission;
                } else {
                    color = style[p.id].xyz;
                }

                float4 clip = TransformWorldToHClip(p.world);
                clip.xy += corners[corner] * (size * 0.5) * 2.0 / _ScreenParams.xy * clip.w;
                clip.z += depth_bias * clip.w;

                v_out o;
                o.pos = (p.id == 0) ? (float4) 0 : clip; // degenerate for background / non-receiver
                o.color = color;
                return o;
            }

            float4 frag(v_out i) : SV_Target {
                return float4(i.color, 1);
            }
            ENDHLSL
        }
    }
}
