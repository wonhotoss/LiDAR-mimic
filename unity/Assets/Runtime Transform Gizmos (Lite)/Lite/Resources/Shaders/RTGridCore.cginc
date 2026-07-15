#ifndef RTGRID_CORE_CGINC
#define RTGRID_CORE_CGINC

//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
// Name: CalcGridAlphaScale() (Function)
// Desc: Calculates an alpha scale value for a point on a grid. Meant to be used
//       with RTGrid and related shaders.
// Parm: viewPosition   - Point on the grid expressed in view space coords.
//       farPlane       - Camera far plane distance.
//       cameraPosition - Camera position.
// Rtrn: A scale value in the [0, 1] interval.
//-----------------------------------------------------------------------------
float CalcGridAlphaScale(float3 viewPosition, float farPlane, float3 cameraPosition)
{
    return saturate(1.0f - abs(viewPosition.z) / farPlane);
}
#endif