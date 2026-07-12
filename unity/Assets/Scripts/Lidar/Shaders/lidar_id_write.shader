Shader "lidar/id_write" {
    Properties { _LidarID ("id", Float) = 0 }

    SubShader {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass {
            // Overlay onto the depth already written by the camera's opaque pass: keep only the nearest surface.
            ZTest LEqual
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _LidarID;
            CBUFFER_END

            struct v_in { float4 pos : POSITION; };
            struct v_out { float4 pos : SV_POSITION; };

            v_out vert(v_in v) {
                v_out o;
                o.pos = TransformObjectToHClip(v.pos.xyz);
                return o;
            }

            // R = originating object id, G = NDC depth (for world reconstruction in the compute pass).
            float2 frag(v_out i) : SV_Target {
                return float2(_LidarID, i.pos.z);
            }
            ENDHLSL
        }
    }
}
