//-----------------------------------------------------------------------------
// Name: RTGrid (Surface Shader)
// Desc: Implements a grid shader. The effect is inspired from here:
//		 https://madebyevan.com/shaders/grid/ Many thanks to the author for
//		 writing the article!
//-----------------------------------------------------------------------------
Shader "Hidden/RTG/RTGrid"
{
    Properties
    {
        _CellLineColor		("Cell line color", Color)		= (1, 1, 1, 1)		// Grid cell line color
		_GridAxisColor		("Grid axis color", Color)		= (1, 1, 1, 1)		// The color of the grid's local axis (used in the second pass)
        _CellSizeX			("Cell size X", float)			= 1					// Cell size along the grid's X axis
        _CellSizeZ			("Cell size Z", float)			= 1					// Cell size along the grid's Z axis
		_GridOrigin			("Grid origin", Vector)			= (0, 0, 0, 0)		// Grid origin
		_GridRight			("Grid right", Vector)			= (1, 0, 0, 0)		// The grid's right axis
		_GridForward		("Grid forward", Vector)		= (0, 0, 1, 0)		// The grid's forward axis
		_FadeZoom			("Fade zoom", float)			= 1					// The positive distance between the camera position and the grid plane	
		_ZoomFadeEnabled	("Zoom fade enabled", int)		= 1					// 1 - zoom fading enabled; 0 - zoom fading disabled
    }

    SubShader
    {
		Tags { "RenderPipeline" = "UniversalPipeline" }

		//-----------------------------------------------------------------------------
		// States
		//-----------------------------------------------------------------------------
        Blend   SrcAlpha OneMinusSrcAlpha	// Enable alpha blending
        Cull    Off							// No culling. We want to see the grid from any side.

		//-----------------------------------------------------------------------------
		// Passes
		//-----------------------------------------------------------------------------
		//-----------------------------------------------------------------------------
		// Coordinate System Pass - Draws one of the grid's coordinate system lines.
		//-----------------------------------------------------------------------------
		Pass
		{
			//-----------------------------------------------------------------------------
			// States
			//-----------------------------------------------------------------------------
			// Stencil states used to avoid Z-wars. When drawing the grid plane, only the
			// pixels which don't overlap the grid axes will be drawn.
			Stencil
			{
				Ref		1
				Comp	Always
				Pass	Replace
				ZFail	Keep
				Fail	Keep
			}

			//-----------------------------------------------------------------------------
			// Implementation
			//-----------------------------------------------------------------------------
            HLSLPROGRAM
            #include "UnityCG.cginc"
			#include "RTGridCore.cginc"
            #pragma vertex		VSMain
			#pragma fragment	PSMain
            #pragma target 2.5	

			//-----------------------------------------------------------------------------
			// Properties
			//-----------------------------------------------------------------------------
			float4	_GridAxisColor;

			//-----------------------------------------------------------------------------
			// Structures
			//-----------------------------------------------------------------------------
			//-----------------------------------------------------------------------------
			// Name: VInput (Struct)
			// Desc: Vertex shader input.
			//-----------------------------------------------------------------------------
			struct VInput
			{
				float4 pos	: POSITION;	// Vertex position
			};

			//-----------------------------------------------------------------------------
			// Name: VOutput (Struct)
			// Desc: Vertex shader output and pixel shader input.
			//-----------------------------------------------------------------------------
			struct VOutput
			{
				float3 worldPos	: TEXCOORD0;	// World position
				float3 viewPos	: TEXCOORD1;	// View position
				float4 hClipPos	: SV_POSITION;	// Homogenous clip position
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
				VOutput output;

				// Calculate vertex position in different spaces
				output.worldPos = mul(unity_ObjectToWorld, input.pos);
				output.hClipPos	= UnityObjectToClipPos(input.pos);
				output.viewPos	= UnityObjectToViewPos(input.pos);

				// Return output
				return output;
			}

			//-----------------------------------------------------------------------------
			// Name: PSMain() (Pixel Shader)
			// Desc: Pixel shader entry point.
			//-----------------------------------------------------------------------------
			float4 PSMain(VOutput input) : COLOR
			{
				// Calculate alpha scale factor based on view space position
				float farPlane		= _ProjectionParams.z;
				float alphaScale	= CalcGridAlphaScale(input.viewPos, farPlane, _WorldSpaceCameraPos);

				// Return final color
				return float4(_GridAxisColor.xyz, _GridAxisColor.a * alphaScale);
			}
			ENDHLSL
		}

		//-----------------------------------------------------------------------------
		// Grid Pass - Draws the grid.
		//-----------------------------------------------------------------------------
        Pass
        {
			//-----------------------------------------------------------------------------
			// States
			//-----------------------------------------------------------------------------
			// Stencil states used to avoid Z-wars. When drawing the grid plane, only the
			// pixels which don't overlap the grid axes will be drawn.
			Stencil
			{
				Ref		1
				Comp	NotEqual
				Pass	Keep
				ZFail	Keep
				Fail	Keep
			}

			//-----------------------------------------------------------------------------
			// Implementation
			//-----------------------------------------------------------------------------
            HLSLPROGRAM
            #include "UnityCG.cginc"
			#include "RTGridCore.cginc"
            #pragma vertex		VSMain
			#pragma fragment	PSMain
            #pragma target 2.5	

			//-----------------------------------------------------------------------------
			// Properties
			//-----------------------------------------------------------------------------
			float4	_CellLineColor;
			float	_CellSizeX;
			float	_CellSizeZ;
			float3	_GridOrigin;
			float3	_GridRight;
			float3	_GridForward;
			float	_FadeZoom;
			int		_ZoomFadeEnabled;

			//-----------------------------------------------------------------------------
			// Structures
			//-----------------------------------------------------------------------------
			//-----------------------------------------------------------------------------
			// Name: VInput (Struct)
			// Desc: Vertex shader input.
			//-----------------------------------------------------------------------------
			struct VInput
			{
				float4 pos	: POSITION;	// Vertex position
			};

			//-----------------------------------------------------------------------------
			// Name: VOutput (Struct)
			// Desc: Vertex shader output and pixel shader input.
			//-----------------------------------------------------------------------------
			struct VOutput
			{
				float3 worldPos	: TEXCOORD0;	// World position
				float3 viewPos	: TEXCOORD1;	// View position
				float4 hClipPos	: SV_POSITION;	// Homogenous clip position
			};

			//-----------------------------------------------------------------------------
			// Functions
			//-----------------------------------------------------------------------------
			//-----------------------------------------------------------------------------
			// Name: GetGridEgde() (Function)
			// Desc: Checks it the specified world coordinates fall on the grid cell edge.
			// Parm: worldCoords - Query world coordinates.
			//		 cellSizeX   - Grid cell size X.
			//		 cellSizeZ	 - Grid cell size Z.
			// Rtrn: A float in the [0, 1] range where 0 means not on edge, 1 means on edge.
			//		 This value could be used to scale the alpha value of a pixel so that
			//		 only pixels which lie really close to the cell edge actually contribute
			//		 to the final color.
			//-----------------------------------------------------------------------------
			float GetGridEgde(float3 worldCoords, float cellSizeX, float cellSizeZ)
			{
				// Create a vector going from the grid origin to the world coords
				float3 coords	= (worldCoords - _GridOrigin);

				// Project the coordinates along the grid's local axes and 
				// divide by the cell size to get the number of cells along
				// each axis. Y position doesn't matter, so we set it to 0.
				coords			= float3(dot(coords, _GridRight) / cellSizeX, 0.0f, dot(coords, _GridForward) / cellSizeZ);

				// Many thanks to https://madebyevan.com/shaders/grid/
				float3 f		= abs(frac(coords - 0.5f) - 0.5f) / fwidth(coords);
				return 1.0f - saturate(min(f.x, f.z));
			}
			
			//-----------------------------------------------------------------------------
			// Name: GetDigitCount() (Function)
			// Desc: Returns the number of digits in the specified integer value.
			// Parm: val - Query int value. Can be both positive and negative.
			// Rtrn: The number of digits in 'val'.
			//-----------------------------------------------------------------------------
			int GetDigitCount(int val)
			{
				// If val == 0 => 1 digit
				// Otherwise, take the floor of base 10 log of absolute value and add 1.
				// Ex: val = 23		=> floor(log10(23)) + 1		= floor(1.36) + 1	= 2	(Correct)
				//	   val = 1000	=> floor(log10(1000)) + 1	= floor(3) + 1		= 4	(Correct)
				//	   val = 1      => floor(log10(1)) + 1		= floor(0) + 1		= 1 (Correct)
				return val == 0 ? 1 : (int)floor(log10(abs(val)) + 1);
			}

			//-----------------------------------------------------------------------------
			// Name: VSMain() (Vertex Shader)
			// Desc: Vertex shader entry point.
			//-----------------------------------------------------------------------------
			VOutput VSMain(VInput input)
			{
				VOutput output;

				// Calculate vertex position in different spaces
				output.worldPos = mul(unity_ObjectToWorld, input.pos);
				output.hClipPos	= UnityObjectToClipPos(input.pos);
				output.viewPos	= UnityObjectToViewPos(input.pos);

				// Return output
				return output;
			}

			//-----------------------------------------------------------------------------
			// Name: PSMain() (Pixel Shader)
			// Desc: Pixel shader entry point.
			//-----------------------------------------------------------------------------
			float4 PSMain(VOutput input) : COLOR
			{
				// Is zoom fade enabled?
				if (_ZoomFadeEnabled)
				{
					// Calculate the number of digits in the fade zoom variable.
					// This is the number of digits in the distance between the 
					// camera position and the grid plane.
					int digitCount		= GetDigitCount((int)_FadeZoom);

					// We will draw 2 grids. One grid which is fading in and another one which is
					// fading out. Each grid's cell size will be scaled by 10 ^ N, where N is:
					//	a) digitCount - 1		
					//	b) digitCount
					float csScale0		= pow(10.0f, digitCount - 1);	// When digitCount = 1, this will yield a value of 1, leaving the original cell size unchanged
					float csScale1		= pow(10.0f, digitCount);		// Scale the other grid's cell size using the next power of 10. This produces an effect where each
																		// cell in this grid contains 10 cells from the other grid.

					// Calculate alpha scale factors for each grid. The grid with the smaller
					// cells will fade out the more we move away from the grid plane, while 
					// the other grid will fade in.
					float a0			= (csScale1 - _FadeZoom) / (csScale1 - csScale0);	// For grid with smaller cells
					float a1			= 1.0 - a0;											// For grid with big cells

					// Calculate the onEdge factors and scale them using the alpha factors calculated earlier
					float onEdge0		= GetGridEgde(input.worldPos, _CellSizeX * csScale0, _CellSizeZ * csScale0) * a0;
					float onEdge1		= GetGridEgde(input.worldPos, _CellSizeX * csScale1, _CellSizeZ * csScale1) * a1;

					// Calculate alpha scale factor based on view space position
					float farPlane		= _ProjectionParams.z;
					float alphaScale	= CalcGridAlphaScale(input.viewPos, farPlane, _WorldSpaceCameraPos);

					// Calculate the final pixel color. This is the cell line color where its alpha value is a value
					// in the [0, 1] range. This alpha value is a combination of more values:
					//	a) GetGridEgde						- decides if the pixel lies on the grid cell edge
					//	b) GetGridEgde * a0					- scale again based on power of 10 rule
					//	c) GetGridEgde * a0 * alphaScale	- scale once again based on relationship to the camera
					return float4(_CellLineColor.r, _CellLineColor.g, _CellLineColor.b, _CellLineColor.a * (onEdge0 + onEdge1) * alphaScale);
				}
				else
				{
					// Calculate the onEdge factor
					float onEdge	= GetGridEgde(input.worldPos, _CellSizeX, _CellSizeZ);

					// Calculate alpha scale factor based on view space position
					float farPlane		= _ProjectionParams.z;
					float alphaScale	= CalcGridAlphaScale(input.viewPos, farPlane, _WorldSpaceCameraPos);

					// Calculate the final pixel color
					return float4(_CellLineColor.r, _CellLineColor.g, _CellLineColor.b, _CellLineColor.a * onEdge * alphaScale);
				}
			}
			ENDHLSL
        }
    }
}
