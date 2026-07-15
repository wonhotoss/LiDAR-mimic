//-----------------------------------------------------------------------------
// Name: RTUnlit (Surface Shader)
// Desc: Implements a simple unlit surface shader that outputs a color modulated 
//       with a color sampled from a texture.
//-----------------------------------------------------------------------------
Shader "Hidden/RTG/RTUnlit"
{
    Properties
    {
        _MainTex    ("Texture", 2D)     = "white" {}    // Texture whose sampled color is modulated with the color porperty
        _Color	    ("Color", Color)	= (1, 1, 1, 1)	// Color modulated with the texture color

		_CullMode	("Cull mode", int)	= 2				// Cull mode		(Default = Back)
		_ZTest		("ZTest", int)		= 8				// Z Test toggle	(Default = Always)
		_ZWrite		("ZWrite", int)		= 0				// Z Write toggle	(Default = Off)
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
            Blend   SrcAlpha OneMinusSrcAlpha	// Enable alpha blending
			Cull	[_CullMode]					// Set cull mode from property
			ZTest	[_ZTest]					// ZTest state
			ZWrite	[_ZWrite]					// ZWrite state

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
			float4		_Color;
			sampler2D	_MainTex;

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
				float2 uv		: TEXCOORD0;	// Texture coords
			};

			//-----------------------------------------------------------------------------
			// Name: VOutput (Struct)
			// Desc: Vertex shader output and pixel shader input.
			//-----------------------------------------------------------------------------
			struct VOutput
			{
				float2 uv		: TEXCOORD0;	// Texture coords
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
				// Calculate output data
				VOutput output;
				output.uv		= input.uv;
				output.hClipPos	= UnityObjectToClipPos(input.pos);

				// Return output data
				return output;
			}

			//-----------------------------------------------------------------------------
			// Name: PSMain() (Pixel Shader)
			// Desc: Pixel shader entry point.
			//-----------------------------------------------------------------------------
			float4 PSMain(VOutput input) : COLOR
			{
				// Return color modulated with texture color
				return tex2D(_MainTex, input.uv) * _Color;
			}
			ENDHLSL
        }
    }
}
