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
            struct v_out { float4 pos : SV_POSITION; float3 world : TEXCOORD0; };

            v_out vert(v_in v) {
                v_out o;
                o.world = TransformObjectToWorld(v.pos.xyz);
                o.pos = TransformWorldToHClip(o.world);
                return o;
            }

            // R = originating object id, G = range (m) from the sensor. The face camera sits at the sensor origin,
            // so _WorldSpaceCameraPos is the sensor position.
            float2 frag(v_out i) : SV_Target {
                return float2(_LidarID, distance(i.world, _WorldSpaceCameraPos));
            }
            ENDHLSL
        }
    }
}
