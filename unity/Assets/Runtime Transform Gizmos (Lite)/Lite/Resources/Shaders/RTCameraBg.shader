//-----------------------------------------------------------------------------
// Name: RTCameraBg (Surface Shader)
// Desc: Implements a simple camera background effect that draws a gradient.
//-----------------------------------------------------------------------------
Shader "Hidden/RTG/RTCameraBg"
{
	Properties
	{
		_FirstColor		("First color", Color)				= (0.5, 0.5, 0.5, 1.0)	// First gradient color
		_SecondColor	("Second color", Color)				= (0.1, 0.1, 0.1, 1.0)	// Second gradient color
		_FarPlaneHeight ("Far plane height", float)			= 0.0					// The height of the camera far plane in world units
		_GradientOffset ("Gradient offset", Range(-1, 1))	= 0.0					// Gradient offset (closer to -1 means more of _FirstColor, 1 means more of _SecondColor)
	}

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }

	    //-----------------------------------------------------------------------------
		// Passes
		//-----------------------------------------------------------------------------
		Pass
		{
			//-----------------------------------------------------------------------------
			// States
			//-----------------------------------------------------------------------------
			ZWrite	Off			// No need to write depth
			ZTest	LEqual		// The background is obscured by what is in front of it
			Cull	Off			// No need for culling

			//-----------------------------------------------------------------------------
			// Implementation
			//-----------------------------------------------------------------------------
			HLSLPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex		VSMain
			#pragma fragment	PSMain
			#pragma target 2.5

			//-----------------------------------------------------------------------------
			// Properties
			//-----------------------------------------------------------------------------
			float4	_FirstColor;
			float4	_SecondColor;
			float	_FarPlaneHeight;
			float	_GradientOffset;

			//-----------------------------------------------------------------------------
			// Structures
			//-----------------------------------------------------------------------------
			//-----------------------------------------------------------------------------
			// Name: VInput (Struct)
			// Desc: Vertex shader input.
			//-----------------------------------------------------------------------------
			struct VInput
			{
				float4 pos		: POSITION;		// Vertex position
			};

			//-----------------------------------------------------------------------------
			// Name: VOutput (Struct)
			// Desc: Vertex shader output and pixel shader input.
			//-----------------------------------------------------------------------------
			struct VOutput
			{
				float4 hClipPos	: SV_POSITION;	// Homogenous clip position
				float3 viewPos	: TEXCOORD0;	// View position
			};

			//-----------------------------------------------------------------------------
			// Functions
			//-----------------------------------------------------------------------------
			//-----------------------------------------------------------------------------
			// Name: VSMain() (Vertex Shader)
			// Desc: Vertex shader entry point.
			//-----------------------------------------------------------------------------
			VOutput VSMain(VInput input)
			{
				// Calculate output data
				VOutput output;
				output.hClipPos	= UnityObjectToClipPos(input.pos);
				output.viewPos	= UnityObjectToViewPos(input.pos);

				// Return output data
				return output;
			}

			//-----------------------------------------------------------------------------
			// Name: PSMain() (Pixel Shader)
			// Desc: Pixel shader entry point.
			//-----------------------------------------------------------------------------
			float4 PSMain(VOutput input) : COLOR
			{
				// Calculate interpolation factor
				float t = input.viewPos.y / (_FarPlaneHeight * 0.5f);	// t = [-1, 1]
				t *= -1.0f;												// Invert Y axis so we can have _FirstColor at t = 0 which is top of far plane in view space
				t = saturate(t * 0.5f + 0.5f + _GradientOffset);		// t = [0, 1] + _GradientOffset
	
				// Interpolate the gradient color based on the calculated t value
				return lerp(float4(_FirstColor.rgb, 1.0f), float4(_SecondColor.rgb, 1.0f), t);
			}
			ENDHLSL
		}
	}
}