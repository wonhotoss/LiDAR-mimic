//-----------------------------------------------------------------------------
// Name: RTGizmoHandle (Surface Shader)
// Desc: Implements a shader which is used for rendering gizmo handles.
//-----------------------------------------------------------------------------
Shader "Hidden/RTG/RTGizmoHandle"
{
	Properties
	{
		// Standard properties
		_Color					("Color", Color)			= (1, 1, 1, 1)	// Handle color
		_ZoomScale				("Zoom scale", float)		= 1				// Handle zoom scale calculated using the camera that renders the handle
		_AlphaScale				("Alpha scale", float)		= 1				// Alpha scale value used to scale the handle alpha
		_Lit					("Lit", int)				= 1				// Is the handle affected by lighting?					
		_LightDirection			("Light direction", Vector)	= (1, 1, 1, 0)	// Light direction	
		_LightIntensity			("Light intensity", float)	= 1.5			// Light intensity
		_CullMode				("Cull mode", int)			= 2				// Cull mode		(Default = Back)
		_ZTest					("ZTest", int)				= 8				// Z Test toggle	(Default = Always)
		_ZWrite					("ZWrite", int)				= 0				// Z Write toggle	(Default = Off)

		// Handle shapes. Those are used for some types of primitives which can't easily
		// be stored inside a mesh. For example, although an inset box can be stored as a mesh, 
		// it's thickness can't be scaled without affecting the entire box size. But we can just
		// render a regular box instead and use raycasting inside the shader to carve the inset.
		_HandleShape			("Handle shape", int)		= 0				// 1 = inset box, 2 = inset circle, 3 = inset cylinder, 
																			// 4 = torus, 5 = arc, 6 = sphere border, any other value means standard mesh primitive.
		
		// Handle shape args:
		//  InsetBox:		xyz = size,				w = thickness
		//  InsetCircle:	x	= radius,			y = thickness
		//  InsetCylinder:	x	= radius,			y = length,		z = thickness
		//  Torus:			x	= radius,			y = tube radius
		//  Arc:			xyz = start,			w = angle		
		//  SphereBorder:	xyz = sphere center,	w = sphere radius
		// Note: When drawing a sphere border, the mesh should be a circle surrounding the 
		//		 sphere in the plane defined by the camera right and up axes. The circle
		//		 center must be set to the sphere center.
		_HandleShapeArgs		("Handle shape args", Vector)	= (0, 0, 0, 0)	// Handle shape args. See above.
		_ArcNormal				("Arc normal", Vector)			= (0, 0, 1, 0)	// Arc normal used when '_HandleShape' is 'Arc'

		// Cull sphere. A cull sphere is used to 'cull' pixels which lie behind the sphere from
		// the perspective of the camera. When a pixel is culled, it may be rejected or its alpha
		// value could be scaled down to indicate that the pixel is not visible (e.g. rotate gizmo
		// circles are culled by the arc-ball).
		_CullSphereEnabled		("Cull sphere enabled", int)	= 0				// Is the cull sphere enabled? (1 = enabled, 0 = disabled)
		_CullSphereMode			("Cull sphere mode", int)		= 0				// The cull sphere mode (0 = Full, 1 = Behind)
		_CullSphere				("Cull sphere center", Vector)	= (0, 0, 0, 0)	// Cull sphere	(xyz = center, w = radius)
		_CullAlphaScale			("Cull alpha scale", float)		= 0				// When a pixel is culled, its alpha will be scaled by this value

		_ClipPlaneEnabled		("Clip plane enabled", int)		= 0				// Is the clip plane enabled? (1 = enabled, 0 = disabled)
		_ClipPlane				("Clip plane", Vector)			= (0, 0, 0, 0)	// Clipping plane (xyz = normal, w = distance from origin which is < 0 when origin is behind the plane: ax + by + cz + d = 0)
																				// Points that exist behind the clipping plane are clipped.
	}

	Subshader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" }

		//-----------------------------------------------------------------------------
		// States
		//-----------------------------------------------------------------------------
		Blend	SrcAlpha OneMinusSrcAlpha	// Enable alpha blending	
		ZTest	[_ZTest]					// ZTest state
		ZWrite	[_ZWrite]					// ZWrite state
		Cull	[_CullMode]					// Cull state

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
			#include "RTMath.cginc"
			#pragma vertex		VSMain
			#pragma fragment	PSMain

			//-----------------------------------------------------------------------------
			// Macros
			//-----------------------------------------------------------------------------
			#define RTGIZMO_HANDLE_INSET_BOX		1
			#define RTGIZMO_HANDLE_INSET_CIRCLE		2
			#define RTGIZMO_HANDLE_INSET_CYLINDER   3
			#define RTGIZMO_HANDLE_TORUS			4
			#define RTGIZMO_HANDLE_ARC				5
			#define RTGIZMO_HANDLE_SPHERE_BORDER	6

			#define RTCULL_SPHERE_MODE_FULL			0
			#define RTCULL_SPHERE_MODE_BEHIND		1

			//-----------------------------------------------------------------------------
			// Properties
			//-----------------------------------------------------------------------------
			float4	_Color;
			float	_ZoomScale;
			float	_AlphaScale;
			int		_Lit;
			float4	_LightDirection;
			float	_LightIntensity;

			int		_HandleShape;
			float4	_HandleShapeArgs;
			float3	_ArcNormal;

			int		_CullSphereEnabled;
			int		_CullSphereMode;
			float4	_CullSphere;
			float	_CullAlphaScale;

			int		_ClipPlaneEnabled;
			float4	_ClipPlane;

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
				float3 normal   : NORMAL;		// Normal,
			};

			//-----------------------------------------------------------------------------
			// Name: VOutput (Struct)
			// Desc: Vertex shader output and pixel shader input.
			//-----------------------------------------------------------------------------
			struct VOutput
			{
				float3 worldPos		: TEXCOORD0;		// World position
				float3 worldNormal	: TEXCOORD1;		// World normal
				float4 hClipPos		: SV_POSITION;		// Homogenous clip position
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

				// If we're drawing a torus, we need to process the vertices to ensure the
				// core and tube radii are applied correctly.
				// Note: The torus must mesh satisfy the following conditions:
				//		1. main axis aligned with the Z axis.
				//		2. its center is <0, 0, 0>
				//		3. radius = 1
				//		4. tube radius = 1
				if (_HandleShape == RTGIZMO_HANDLE_TORUS)
				{				
					// We need to calculate the center of the cross section the vertex belongs to
					float3 crossDir		= normalize(float3(input.pos.x, input.pos.y, 0.0f));
					float3 crossCenter	= crossDir * _HandleShapeArgs.x;

					// Calculate final position and normal
					float3 v		= normalize(input.pos - crossDir) * _HandleShapeArgs.y;
					input.pos		= float4(crossCenter + v, input.pos.w);
					input.normal	= normalize(input.pos - crossCenter);
				}

				// If we're drawing a sphere border, we need to update the world position.
				// Otherwise, just continue as usual.
				if (_HandleShape == RTGIZMO_HANDLE_SPHERE_BORDER)
				{
					// Calculate world position
					output.worldPos		= mul(unity_ObjectToWorld, input.pos);

					// Calculate camera ray
					Ray ray = GetCameraRay(output.worldPos);

					// Calculate final output data
					output.worldPos		= SphereExtentToTangentPoint(_HandleShapeArgs.xyz, _HandleShapeArgs.w, output.worldPos, GetCameraForward(), ray.direction);
					output.worldNormal	= normalize(output.worldPos - _HandleShapeArgs.xyz);
					output.hClipPos		= UnityWorldToClipPos(output.worldPos);
				}
				else
				{
					// Calculate output data
					output.worldPos		= mul(unity_ObjectToWorld, input.pos);
					output.worldNormal	= UnityObjectToWorldNormal(input.normal);
					output.hClipPos		= UnityObjectToClipPos(input.pos);
				}

				// Return output
				return output;
			}

			//-----------------------------------------------------------------------------
			// Name: PSMain() (Pixel Shader)
			// Desc: Pixel shader entry point.
			//-----------------------------------------------------------------------------
			float4 PSMain(VOutput input) : COLOR
			{
				// Use clip plane?
				if (_ClipPlaneEnabled == 1)
				{
					float d = dot(input.worldPos, _ClipPlane.xyz) + _ClipPlane.w;
					clip(d + 1e-4f);
				}

				// Create the ray used for raycasting (in case we need it)
				Ray ray = GetCameraRay(input.worldPos);

				// Convert input data if special primitives are used
				if (_HandleShape == RTGIZMO_HANDLE_INSET_BOX)
				{
					// Create the inset box
					InsetOBox box;
					box.center	  = unity_ObjectToWorld._m03_m13_m23;
					box.right	  = normalize(unity_ObjectToWorld._m00_m10_m20);
					box.up		  = normalize(unity_ObjectToWorld._m01_m11_m21);
					box.size	  = _HandleShapeArgs.xyz;
					box.thickness = _HandleShapeArgs.w;

					// Raycast
					RayHit hit;
					if (RaycastInsetOBox(ray, box, hit)) 
					{
						// Change the input normal with the real point that needs to be rendered.
						// Note: The pixel depth value should also be changed since we are essentially
						//		 shading a different world position right now. However, since most of
						//		 the times Z testing and even Z writing will be disabled when drawing
						//		 handles, this hardly matters.
						input.worldNormal = hit.normal;
					}
					else clip(-1);
				}
				else
				if (_HandleShape == RTGIZMO_HANDLE_INSET_CIRCLE)
				{
					// Create the inset circle (assume the circle mesh is defined in the XY plane in model space)
					InsetCircle circle;
					circle.center		= unity_ObjectToWorld._m03_m13_m23;
					circle.radius		= _HandleShapeArgs.x;
					circle.thickness	= _HandleShapeArgs.y;
					circle.right		= normalize(unity_ObjectToWorld._m00_m10_m20);
					circle.normal		= normalize(unity_ObjectToWorld._m02_m12_m22);

					// Raycast
					RayHit hit;
					if (!RaycastInsetCircle(ray, circle, hit)) 
						clip(-1);
				}
				else
				if (_HandleShape == RTGIZMO_HANDLE_INSET_CYLINDER)
				{
					// Create the inset cylinder (assume the cylinder mesh is defined such that the inset runs through the local Z axis)
					InsetCylinder cylinder;
					cylinder.baseCenter = unity_ObjectToWorld._m03_m13_m23;
					cylinder.radius     = _HandleShapeArgs.x;
					cylinder.length     = _HandleShapeArgs.y;
					cylinder.thickness  = _HandleShapeArgs.z;
					cylinder.lengthAxis = normalize(unity_ObjectToWorld._m02_m12_m22);

					// Raycast
					RayHit hit;
					if (!RaycastInsetCylinder(ray, cylinder, hit)) 
						clip(-1);

					// Update pixel normal
					input.worldNormal = hit.normal;
				}
				else
				if (_HandleShape == RTGIZMO_HANDLE_ARC)
				{
					// Reject pixel if it doesn't lie inside the arc
					clip(0.5 - (float)!PointInArc(input.worldPos, _HandleShapeArgs.xyz, unity_ObjectToWorld._m03_m13_m23, _ArcNormal, _HandleShapeArgs.w));
				}

				// Is the cull sphere enabled?
				float cullAlphaScale = 1.0f;
				if (_CullSphereEnabled == 1)
				{
					// The pixel is culled when the pixel normal is facing away from the camera. This essentially treats the
					// sphere as if it has an infinite radius. If this check fails, we will do a raycast to see if the pixel
					// is actually occluded.
					if (dot(input.worldPos - _CullSphere.xyz, ray.direction) >= 0.0f) 
					{
						// Is the pixel actually occluded?
						RayHit hit;
						if (RaycastSphere(ray, _CullSphere.xyz, _CullSphere.w, hit))
							cullAlphaScale = _CullAlphaScale;
					}
					else
					if (_CullSphereMode == RTCULL_SPHERE_MODE_FULL)	// Only do the inside check if we are allowed to do so
					{
						// The pixel normal is facing the camera but, the pixel may be inside the sphere
						if (length(input.worldPos - _CullSphere.xyz) - _CullSphere.w < -1e-2f * _ZoomScale)
							cullAlphaScale = _CullAlphaScale;
					}
				}

				// If this isn't a lit handle, just return its color
				if(_Lit == 0) return float4(_Color.rgb, _Color.a * cullAlphaScale * _AlphaScale);
				else
				{
					// Normalize normal
					input.worldNormal = normalize(input.worldNormal);

					// Calculate light contribution.
					float nDotL = saturate(dot(-_LightDirection.xyz, input.worldNormal));

					// Return final color scaled by lighting contribution and light intensity and add in some ambient
					float scale = nDotL * _LightIntensity;
					return float4(_Color.rgb * scale + _Color.rgb * 0.1f, _Color.a * cullAlphaScale * _AlphaScale);
				}
			}
			ENDHLSL
		}
	}
}
