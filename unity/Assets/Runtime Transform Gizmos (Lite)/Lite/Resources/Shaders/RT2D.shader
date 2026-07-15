//-----------------------------------------------------------------------------
// Name: RT2D (Surface Shader)
// Desc: Implements a shader that can be used to draw 2D geometry. The shader
//		 expects vertices whose coordinates are expressed in screen space.
//-----------------------------------------------------------------------------
Shader "Hidden/RTG/RT2D"
{
	Properties
	{
		_MainTex        ("Texture", 2D)			= "white" {}    // Main color texture (modulated with the color property)
		_Color			("Color", Color)		= (1, 1, 1, 1)	// Color (modulated with the texture property)
		_Position		("Position", Vector)	= (0, 0, 0, 0)	// Position
		_Rotation		("Rotation", float)		= 0.0			// Rotation in degrees
		_Scale			("Scale", Vector)		= (1, 1, 1, 1)	// Scale
		_ViewStart		("View start", Vector)	= (0, 0, 0, 0)	// Viewport starting coordinate
		_ViewSize		("View size", Vector)	= (0, 0, 0, 0)	// Viewport size
	}

	SubShader
    {
		Tags { "RenderPipeline" = "UniversalPipeline" }

		//-----------------------------------------------------------------------------
		// States
		//-----------------------------------------------------------------------------
        Blend   SrcAlpha OneMinusSrcAlpha	// Enable alpha blending
        Cull    Off							// No culling needed for 2D
		ZWrite  Off							// Not writing any depth
		ZTest	Always						// Always passing the depth test in 2D

		//-----------------------------------------------------------------------------
		// Passes
		//-----------------------------------------------------------------------------
		Pass
		{
			//-----------------------------------------------------------------------------
			// Implementation
			//-----------------------------------------------------------------------------
            HLSLPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex		VSMain
			#pragma fragment	PSMain
            #pragma target 2.5
			#pragma multi_compile _ RT_SAMPLE_TEXTURE

			//-----------------------------------------------------------------------------
			// Properties
			//-----------------------------------------------------------------------------
			sampler2D	_MainTex;	
			float4		_Color;
			float3		_Position;
			float		_Rotation;
			float3		_Scale;
			float2		_ViewStart;
			float2		_ViewSize;

			//-----------------------------------------------------------------------------
			// Structures
			//-----------------------------------------------------------------------------
			//-----------------------------------------------------------------------------
			// Name: VInput (Struct)
			// Desc: Vertex shader input.
			//-----------------------------------------------------------------------------
			struct VInput
			{
				float4 pos	: POSITION;		// Vertex position
				float2 uv   : TEXCOORD0;	// UV coords
			};

			//-----------------------------------------------------------------------------
			// Name: VOutput (Struct)
			// Desc: Vertex shader output and pixel shader input.
			//-----------------------------------------------------------------------------
			struct VOutput
			{
				float2 uv		: TEXCOORD0;	// UV coords
				float4 hClipPos	: SV_POSITION;	// Homogenous clip position
			};

			//-----------------------------------------------------------------------------
			// Functions
			//-----------------------------------------------------------------------------
			//-----------------------------------------------------------------------------
			// Name: Transform() 
			// Desc: Transforms the specified 2D position using the position, rotation and
			//		 scale properties.
			// Parm: pos - 2D position which must be transformed.
			// Rtrn: The transformed position.
			//-----------------------------------------------------------------------------
			float2 Transform(float2 pos)
			{
				// Convert rotation angle to radians
				const float PI	= 3.14159265358979323846f;
				float radians	= (_Rotation / 180.0f) * PI;

				// Cache sine and cosine values
				float s			= sin(radians);
				float c         = cos(radians);
				
				// Calculate final position (i.e. apply SRT transform)
				float2 p = pos * _Scale;
				return p.x * float2(c, s) + p.y * float2(-s, c) + _Position;
			}

			//-----------------------------------------------------------------------------
			// Name: ToNDC() 
			// Desc: Converts the specified 2D coordinate to NDC space.
			// Parm: pos - 2D coordinate to be transformed to NDC space.
			// Rtrn: NDC space coordinate with z = 0 and w = 1.
			//-----------------------------------------------------------------------------
			float4 ToNDC(float2 pos)
			{
				return float4( (pos.x - _ViewStart.x) / _ViewSize.x * 2.0f - 1.0f,
							  -((pos.y - _ViewStart.y) / _ViewSize.y * 2.0f - 1.0f),
							   0.0f, 1.0f);
			}

			//-----------------------------------------------------------------------------
			// Name: VSMain() (Vertex Shader)
			// Desc: Vertex shader entry point.
			//-----------------------------------------------------------------------------
			VOutput VSMain(VInput input)
			{
				// Calculate output data
				VOutput output;
				output.uv		= input.uv;
				output.hClipPos	= ToNDC(Transform(input.pos.xy));

				// Return output data
				return output;
			}

			//-----------------------------------------------------------------------------
			// Name: PSMain() (Pixel Shader)
			// Desc: Pixel shader entry point.
			//-----------------------------------------------------------------------------
			float4 PSMain(VOutput input) : COLOR
			{
				// Return color modulated with texture, or just the color if texture sampling is disabled
				#ifdef RT_SAMPLE_TEXTURE
				return _Color * tex2D(_MainTex, input.uv);
				#else
				return _Color;
				#endif
			}
			ENDHLSL
		}
	}
}