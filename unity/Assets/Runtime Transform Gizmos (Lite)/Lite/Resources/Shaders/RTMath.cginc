#ifndef RTMATH_CGINC
#define RTMATH_CGINC

//-----------------------------------------------------------------------------
// Macros
//-----------------------------------------------------------------------------
#define RTFLT_MAX 3.402823466e+38
#define RTFLT_MIN 1.175494351e-38
#define RTEPS 1e-4f
#define RTPI 3.14159265358979323846f

//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
// Name: Ray (Struct)
// Desc: Provides storage for a ray.
//-----------------------------------------------------------------------------
struct Ray
{
	float3 origin;		// Ray origin
	float3 direction;	// Ray direction
};

//-----------------------------------------------------------------------------
// Name: InsetOBox (Struct)
// Desc: Provides storage for oriented bounding boxes that have an inset.
//		 The inset is aligned with the box's Y axis.
//-----------------------------------------------------------------------------
struct InsetOBox
{
	float3  center;		// Box center
	float3	size;		// Box size
	float3	right;		// Right axis
	float3	up;			// Up axis
	float	thickness;	// Box thickness
};

//-----------------------------------------------------------------------------
// Name: InsetCircle (Struct)
// Desc: Provides storage for circles that have an inset.
//-----------------------------------------------------------------------------
struct InsetCircle
{
	float3	center;		// Circle center
	float3	right;		// Right axis
	float3	normal;		// Normal
	float	radius;		// Circle radius
	float   thickness;	// Circle thickness
};

//-----------------------------------------------------------------------------
// Name: Cylinder (Struct)
// Desc: Provides storage for cylinders.
//-----------------------------------------------------------------------------
struct Cylinder
{
	float	length;		// Cylinder length
	float	radius;		// Cylinder radius
	float3  baseCenter;	// Base center position
	float3  lengthAxis;	// Length axis
};

//-----------------------------------------------------------------------------
// Name: InsetCylinder (Struct)
// Desc: Provides storage for cylinders that have an inset.
//-----------------------------------------------------------------------------
struct InsetCylinder
{
    float	length;		// Cylinder length
	float	radius;		// Cylinder radius
	float3  baseCenter;	// Base center position
	float3  lengthAxis;	// Length axis
    float   thickness;  // CYlinder thickness
};

//-----------------------------------------------------------------------------
// Name: RayHit (Struct)
// Desc: Provides storage for a ray hit.
//-----------------------------------------------------------------------------
struct RayHit
{
	float3  normal;		// Hit normal
	float3	position;	// Hit position
	float	t;			// Hit distance from ray origin
};

//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------
//-----------------------------------------------------------------------------
// Name: GetCameraForward() (Function)
// Desc: Returns the camera forward vector.
// Rtrn: The camera forward vector.
//-----------------------------------------------------------------------------
float3 GetCameraForward()
{
    // https://discussions.unity.com/t/camera-forward-vector-in-shader/32664/3
    return mul((float3x3)unity_CameraToWorld, float3(0.0f, 0.0f, 1.0f));
}

//-----------------------------------------------------------------------------
// Name: GetCameraRay() (Function)
// Desc: Returns a camera ray that shoots through the specified point.
// Parm: pt - The ray passes through this world point.
// Rtrn: The camera ray that passes through this point.
//-----------------------------------------------------------------------------
Ray GetCameraRay(float3 pt)
{
    // Cache data
    float3 cameraForward = GetCameraForward();

    // Create ray
    Ray ray;
    ray.origin          = lerp(_WorldSpaceCameraPos, pt - cameraForward * dot(cameraForward, pt - _WorldSpaceCameraPos), unity_OrthoParams.w);
    ray.direction	    = lerp(normalize(pt - ray.origin), cameraForward, unity_OrthoParams.w);

    // Return ray
    return ray;
}

//-----------------------------------------------------------------------------
// Name: Max3() (Function)
// Desc: Returns the maximum component of the specified vector.
// Parm: v - Query vector.
// Rtrn: Max component of 'v'.
//-----------------------------------------------------------------------------
float Max3(float3 v)
{
	return max(max(v.x, v.y), v.z);
}

//-----------------------------------------------------------------------------
// Name: Min3() (Function)
// Desc: Returns the minimum component of the specified vector.
// Parm: v - Query vector.
// Rtrn: Min component of 'v'.
//-----------------------------------------------------------------------------
float Min3(float3 v)
{
	return min(min(v.x, v.y), v.z);
}

//-----------------------------------------------------------------------------
// Name: SolveQuadratic() (Function)
// Desc: Solves the quadratic equation whose coefficients are a, b and c.
// Parm: a, b, c    - Coefficients.
//       t0         - Returns the first solution. 
//       t1         - Returns the second solution.
// Rtrn: True if the equation has a solution and false otherwise. If false is
//       returned the values in t0 and t1 should be ignored. If true is returned,
//       the 2 solutions will always be sorted (t0 <= t1).
//-----------------------------------------------------------------------------
bool SolveQuadratic(float a, float b, float c, out float t0, out float t1)
{
	// Reset output values
    t0 = t1 = 0.0f;

    // Calculate delta = b^2 - 4ac
    float delta = b * b - 4.0f * a * c;
    if (delta < 0.0f) return false;     // If delta < 0, the equation doesn't have any real solutions

    // Store denominator used to calculate solutions. If 0, there are no solutions.
    float denom = 2.0f * a;
    if (denom == 0.0f) return false;

    // If delta is 0, simplify (both solutions have the same value)
    if (delta == 0.0f)
    {
        t0 = t1 = -b / denom;
        return true;
    }
    else
    {
        // t = (-b +/- sqrt(delta))/ (2 * a)
        float sqrtDelta = sqrt(delta);
        t0 = (-b + sqrtDelta) / denom;
        t1 = (-b - sqrtDelta) / denom;

        // Order solutions
        if (t0 > t1)
        {
            float tSwap = t0;
            t0 = t1;
            t1 = tSwap;
        }

        // Success!
        return true;
    }
}

//-----------------------------------------------------------------------------
// Name : RotationMatrixFromAngleAxis() (Function)
// Desc : Builds a 3x3 rotation matrix from the specified angle-axis representation.
// Parm : angle - Rotation angle in radians.
//        axis  - Rotation axis.       
// Rtrn : A 3x3 rotation matrix representing the rotation.
//-----------------------------------------------------------------------------
float3x3 RotationMatrixFromAngleAxis(float angle, float3 axis)
{
    float s = sin(angle);
    float c = cos(angle);
    float t = 1.0f - c;

    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3
    (
        t*x*x + c,     t*x*y - s*z,   t*x*z + s*y,
        t*x*y + s*z,   t*y*y + c,     t*y*z - s*x,
        t*x*z - s*y,   t*y*z + s*x,   t*z*z + c
    );
}

//-----------------------------------------------------------------------------
// Name: RaycastPlane() (Function)
// Desc: Raycasts the specified plane.
// Parm: ray		 - Query ray.
//		 ptOnPlane   - A point on the query plane.
//       planeNormal - Query plane normal.
//		 hit	     - Returns the hit information. Should be ignored if the function
//		               returns false.
// Rtrn: True if the ray intersects the plane and false otherwise.
//-----------------------------------------------------------------------------
bool RaycastPlane(Ray ray, float3 ptOnPlane, float3 planeNormal, out RayHit hit)
{
    // Clear output
    hit = (RayHit)0;

    // Project ray direction into the plane normal
    planeNormal = normalize(planeNormal);
    float v = dot(ray.direction, planeNormal);

    // Calculate moment of intersection as the ratio between the ray origin's distance from plane and the negated projected velocity
    float o = dot((ray.origin - ptOnPlane), planeNormal);
    hit.t = o / -v;

    // If we are intersecting from behind, reject intersection
    if (hit.t < 0.0f)
        return false;

	// Store hit info
	hit.normal		= planeNormal;
	hit.position	= ray.origin + ray.direction * hit.t;

    // Success!
    return true;
}

//-----------------------------------------------------------------------------
// Name: RaycastSphere() (Function)
// Desc: Raycasts the specified sphere.
// Parm: ray	- Query ray.
//		 center - Sphere center.
//       radius - Sphere radius.
//		 hit	- Returns the hit information. Should be ignored if the function
//		          returns false.
// Rtrn: True if the ray intersects the sphere and false otherwise.
//-----------------------------------------------------------------------------
bool RaycastSphere(Ray ray, float3 center, float radius, out RayHit hit)
{
	// Clear output
    hit = (RayHit)0;

    // Store values for easy and fast access
    float3 cToO    = ray.origin - center;
    float3 rayDir  = ray.direction;

    // Calculate coefficients
    float a = rayDir.x * rayDir.x + rayDir.y * rayDir.y + rayDir.z * rayDir.z;
    float b = 2.0f * (rayDir.x * cToO.x + rayDir.y * cToO.y + rayDir.z * cToO.z);
    float c = (cToO.x * cToO.x + cToO.y * cToO.y +
        cToO.z * cToO.z) - radius * radius;

    // Solve quadratic equation
	float t0, t1;
    if (SolveQuadratic(a, b, c, t0, t1))
    {
        // Only accept solutions which move us forward along the original ray direction
        if (t0 < 0.0f && t1 < 0.0f) return false;

        // Return the first non-negative solution
        hit.t			= t0 >= 0.0f ? t0 : t1;
		hit.position	= ray.origin + ray.direction * hit.t;
		hit.normal		= normalize(hit.position - center);
        return true;
    }

    // No hit
    return false;
}

//-----------------------------------------------------------------------------
// Name: RaycastBox() (Function)
// Desc: Raycasts the specified box/AABB.
// Parm: ray	- Query ray.
//		 minPt  - Box min point.
//		 maxPt  - Box max point.
//		 hit	- Returns the hit information. Should be ignored if the function
//		          returns false.
// Rtrn: True if the ray intersects the box and false otherwise.
// Cred: Modified version of https://jcgt.org/published/0007/03/04/
//-----------------------------------------------------------------------------
bool RaycastBox(Ray ray, float3 minPt, float3 maxPt, out RayHit hit)
{
    // Clear output
    hit = (RayHit)0;

	// Calculate inverse ray direction
	float3 invRayDir = float3(1.0f / ray.direction.x, 1.0f / ray.direction.y, 1.0f / ray.direction.z);

	// Calculate t values
	float3 t0   = (minPt - ray.origin) * invRayDir;
	float3 t1   = (maxPt - ray.origin) * invRayDir;
	float3 tMin = min(t0, t1), tMax = max(t0, t1);

	// Did we hit anything?
	hit.t = Max3(tMin);
	if (hit.t > Min3(tMax)) return false;
	
	// Calculate intersection point
	hit.position = ray.origin + ray.direction * hit.t;

	// Calculate normal
	float3 absMin = abs(hit.position - minPt);
	float3 absMax = abs(hit.position - maxPt);

	if      (absMin.x <= RTEPS) hit.normal = float3(-1.0f,  0.0f,  0.0f);
	else if (absMin.y <= RTEPS) hit.normal = float3( 0.0f, -1.0f,  0.0f);
	else if (absMin.z <= RTEPS) hit.normal = float3( 0.0f,  0.0f, -1.0f);
	else if (absMax.x <= RTEPS) hit.normal = float3( 1.0f,  0.0f,  0.0f);
	else if (absMax.y <= RTEPS) hit.normal = float3( 0.0f,  1.0f,  0.0f);
	else if (absMax.z <= RTEPS) hit.normal = float3( 0.0f,  0.0f,  1.0f);
	
	// We have an intersection
	return true;
}

//-----------------------------------------------------------------------------
// Name: RaycastInsetOBox() (Function)
// Desc: Raycasts the specified inset box.
// Parm: ray - Query ray.
//		 box - Box.
//		 hit - Returns the hit information. Should be ignored if the function
//		       returns false.
// Rtrn: True if the ray intersects the box and false otherwise.
//-----------------------------------------------------------------------------
bool RaycastInsetOBox(Ray ray, InsetOBox box, out RayHit hit)
{
    // Clear output
    hit = (RayHit)0;

	// Calculate the box's forward vector
	float3 forward = normalize(cross(box.right, box.up));

	// Transform the ray into box model space
	Ray r;
	r.origin	= ray.origin - box.center;
	r.origin	= float3(dot(box.right, r.origin), dot(box.up, r.origin), dot(forward, r.origin));
	r.direction	= normalize(float3(dot(box.right, ray.direction), dot(box.up, ray.direction), dot(forward, ray.direction)));
	
	// Raycast box
	float3 minPt = -box.size / 2.0f;
	float3 maxPt =  box.size / 2.0f;
	if (RaycastBox(r, minPt, maxPt, hit))
	{
		// If we hit the top or bottom, check if we hit the inset area
		if (abs(hit.position.y - maxPt.y) <= RTEPS || abs(hit.position.y - minPt.y) <= RTEPS)
		{
			// Are we in the inset area?
			float hix = (box.size.x - box.thickness * 2.0f) / 2.0f;	// Half inset size along X
			float hiz = (box.size.z - box.thickness * 2.0f) / 2.0f;	// half inset size along Z
			if (abs(hit.position.x) < hix &&
				abs(hit.position.z) < hiz) 
			{
				// We're in the inset area. We need to raycast again to see if the ray hits the inner faces.
				// Divide the box into 4 sections, raycast all of them and pick smallest t.
				RayHit hit0, hit1, hit2, hit3;
				float tMin = RTFLT_MAX;

				// Front face <0, 0, -1>
				float3 iMinPt = float3(minPt.x + box.thickness, minPt.y, maxPt.z - box.thickness);
				float3 iMaxPt = float3(maxPt.x - box.thickness, maxPt.y, maxPt.z);
				if (RaycastBox(r, iMinPt, iMaxPt, hit0) && hit0.t < tMin) { tMin = hit0.t; hit = hit0; };

				// Back face <0, 0, 1>
				iMinPt = float3(minPt.x + box.thickness, minPt.y, minPt.z);
				iMaxPt = float3(maxPt.x - box.thickness, maxPt.y, minPt.z + box.thickness);
				if (RaycastBox(r, iMinPt, iMaxPt, hit1) && hit1.t < tMin) { tMin = hit1.t; hit = hit1; };

				// Left face <0, 0, 1>
				iMinPt = float3(minPt.x, minPt.y, minPt.z + box.thickness);
				iMaxPt = float3(minPt.x + box.thickness, maxPt.y, maxPt.z - box.thickness);
				if (RaycastBox(r, iMinPt, iMaxPt, hit2) && hit2.t < tMin) { tMin = hit2.t; hit = hit2; };

				// Right face <0, 0, -1>
				iMinPt = float3(maxPt.x - box.thickness, minPt.y, minPt.z + box.thickness);
				iMaxPt = float3(maxPt.x, maxPt.y, maxPt.z - box.thickness);
				if (RaycastBox(r, iMinPt, iMaxPt, hit3) && hit3.t < tMin) { tMin = hit3.t; hit = hit3; };
						
				// Have we hit anything?
				if (tMin == RTFLT_MAX) return false;				
			}
		}

		// Convert intersection point and normal back into world space
		hit.position = hit.position.x * box.right + hit.position.y * box.up + hit.position.z * forward + box.center;
		hit.normal   = normalize(hit.normal.x * box.right + hit.normal.y * box.up + hit.normal.z * forward);

		// We have an intersection
		return true;
	}

	// No intersection
	return false;
}

//-----------------------------------------------------------------------------
// Name: RaycastInsetCircle() (Function)
// Desc: Raycasts the specified inset circle.
// Parm: ray	- Query ray.
//		 circle - Circle.
//		 hit	- Returns the hit information. Should be ignored if the function
//		          returns false.
// Rtrn: True if the ray intersects the circle and false otherwise.
//-----------------------------------------------------------------------------
bool RaycastInsetCircle(Ray ray, InsetCircle circle, out RayHit hit)
{
    // Clear output
    hit = (RayHit)0;

	// If the ray doesn't intersect the circle plane, it can't possibly intersect the circle
	if (!RaycastPlane(ray, circle.center, circle.normal, hit))
        return false;
	
	// The ray intersects the plane. The intersection point must lie inside the circle, but outside the inset area.
    float d = length(hit.position - circle.center);
    if (d <= circle.radius && d > (circle.radius - circle.thickness))
		return true;

    // No intersection
    return false;
}

//-----------------------------------------------------------------------------
// Name: RaycastCylinder() (Function)
// Desc: Raycasts the specified cylinder.
// Parm: ray        - Query ray.
//		 cylinder   - Cylinder.
//       ignoreCaps - If true, the ray can't hit the cylinder caps.
//		 hit        - Returns the hit information. Should be ignored if the function
//		              returns false.
// Rtrn: True if the ray intersects the cylinder and false otherwise.
//-----------------------------------------------------------------------------
bool RaycastCylinder(Ray ray, Cylinder cylinder, bool ignoreCaps, out RayHit hit)
{
	// Clear output
    hit = (RayHit)0;

    // Precompute needed data
    float3 dCl = cross(ray.direction, cylinder.lengthAxis);
    float3 oCl = cross((ray.origin - cylinder.baseCenter), cylinder.lengthAxis);

    // Calculate the quadratic coefficients.
    // Note: Use 'dot' for sqr length.
    float a = dot(dCl, dCl);
    float b = 2.0f * dot(dCl, oCl);
    float c = dot(oCl, oCl) - cylinder.radius * cylinder.radius;
            
    // Solve the quadratic equation
    float t0, t1;
    bool wasHit = false;
    if (SolveQuadratic(a, b, c, t0, t1))
    {
        // Make sure the ray doesn't intersect the cylinder only from behind
        if (t0 >= 0.0f || t1 >= 0.0f) 
        {
            // Are we ignoring caps?
            if (!ignoreCaps)
            {
                // Make sure we are using the smallest positive t value
                if (t0 < 0.0f)
                {
                    float temp = t0;
                    t0 = t1;
                    t1 = temp;
                }
                hit.t = t0;

                // Now make sure the intersection point lies between the cylinder's bottom and top points
                hit.position = ray.origin + ray.direction * hit.t;
                float d = dot(cylinder.lengthAxis, (hit.position - cylinder.baseCenter));
                if (d >= 0.0f && d <= cylinder.length) 
                {
                    // Push the hit point onto the cylinder base plane and calculate normal
                    hit.normal = normalize(hit.position - cylinder.lengthAxis * d - cylinder.baseCenter);
                    wasHit = true;
                }
            }
            else
            {
                // When ignoring caps, we could have 2 hit points. One on the exterior and one on the interior.
                // Pick the closest point that lies between the cylinder caps.
                bool valid0 = false, valid1 = false; float d;
                float3 iPt0 = ray.origin + ray.direction * t0;
                float3 iPt1 = ray.origin + ray.direction * t1;
                float d0 = dot(cylinder.lengthAxis, (iPt0 - cylinder.baseCenter));
                float d1 = dot(cylinder.lengthAxis, (iPt1 - cylinder.baseCenter));
                if (d0 >= 0.0f && d0 <= cylinder.length) valid0 = true;
                if (d1 >= 0.0f && d1 <= cylinder.length) valid1 = true;
                if (valid0 && valid1) 
                {
                    if (t0 < t1)
                    {
                        hit.t = t0;
                        d = d0;
                    }
                    else
                    {
                        hit.t = t1;
                        d = d1;
                    }
                }
                else if (valid0) { hit.t = t0; d = d0; }
                else if (valid1) { hit.t = t1; d = d1; }

                // Do we have a hit?
                if (valid0 || valid1)
                {
                    hit.position = ray.origin + ray.direction * hit.t;
                    hit.normal = normalize(hit.position - cylinder.lengthAxis * d - cylinder.baseCenter);
                    return true;
                }

                // No hit
                return false;
            }
        }
    }

    // Test bottom and top caps?
    if (!ignoreCaps)
    {
        // Raycast base plane
        RayHit capHit;
        if (RaycastPlane(ray, cylinder.baseCenter, -cylinder.lengthAxis, capHit) && (!wasHit || capHit.t < hit.t))
        {
            // The ray intersects the plane, but we must also check if the intersection point is within radius
            if (length(capHit.position - cylinder.baseCenter) <= cylinder.radius)
                { hit = capHit; wasHit = true; }
        }
       
        // Raycast top plane
        float3 topCenterPt = cylinder.baseCenter + cylinder.lengthAxis * cylinder.length;
        if (RaycastPlane(ray, topCenterPt, cylinder.lengthAxis, capHit) && (!wasHit || capHit.t < hit.t))
        {
            // The ray intersects the plane, but we must also check if the intersection point is within radius
            if (length(capHit.position - topCenterPt) <= cylinder.radius)
                 { hit = capHit; wasHit = true; }
        }
    }

    // Return result
    return wasHit;
}

//-----------------------------------------------------------------------------
// Name: RaycastInsetCylinder() (Function)
// Desc: Raycasts the specified inset cylinder.
// Parm: ray        - Query ray.
//		 cylinder   - Cylinder.
//		 hit        - Returns the hit information. Should be ignored if the function
//		              returns false.
// Rtrn: True if the ray intersects the cylinder and false otherwise.
//-----------------------------------------------------------------------------
bool RaycastInsetCylinder(Ray ray, InsetCylinder cylinder, out RayHit hit)
{
    // Clear output
    hit = (RayHit)0;

    // Raycast standard cylinder
    Cylinder stdCylinder;
    stdCylinder.length      = cylinder.length;
    stdCylinder.radius      = cylinder.radius;
    stdCylinder.lengthAxis  = cylinder.lengthAxis;
    stdCylinder.baseCenter  = cylinder.baseCenter;
    if (!RaycastCylinder(ray, stdCylinder, false, hit))
        return false;

    // If we've hit one of the caps, we might have hit the inset area
    float d = dot(hit.position - stdCylinder.baseCenter, cylinder.lengthAxis);
    if (d < 1e-4f || abs(d - stdCylinder.length) < 1e-4f)
    {
        // We've hit one of the caps, are we inside the inset area?
        float halfInset = cylinder.radius - cylinder.thickness;
        float3 iPt = hit.position - cylinder.lengthAxis * d;        // Hit point projected into the base plane
        if (length(iPt - cylinder.baseCenter) < halfInset)
        {
            // We're in the inset area. Raycast against the inner cylinder. If we don't hit it, it means the ray doesn't intersect the cylinder.
            stdCylinder.radius  = halfInset;
            if (!RaycastCylinder(ray, stdCylinder, true, hit))
                return false;

            // The inner cylinder was hit. Reverse normal because the inner cylinder is pointing on the inside.
            hit.normal = -hit.normal;
        }
    }

    // We have a hit!
    return true;
}

//-----------------------------------------------------------------------------
// Name: DegreesToRadians() (Function)
// Desc: Converts the specified angle in degrees to radians.
// Parm: degrees - Angle in degrees.
// Rtrn: The angle in radians.
//-----------------------------------------------------------------------------
float DegreesToRadians(float degrees)
{
    return (degrees / 180.0f) * RTPI;
}

//-----------------------------------------------------------------------------
// Name: RadiansToDegrees() (Function)
// Desc: Converts the specified angle in radians to degrees.
// Parm: radians - Angle in radians.
// Rtrn: The angle in degrees.
//-----------------------------------------------------------------------------
float RadiansToDegrees(float radians)
{
    return (radians / RTPI) * 180.0f;
}

//-----------------------------------------------------------------------------
// Name: SignedAngle() (Function)
// Desc: Returns the signed angle in degrees between the 2 vectors.
// Parm: v0   - First vector.
//       v1   - Second vector.
//       axis - The rotation axis which can be used to rotate from v0 to v1 using
//              a positive rotation angle.
// Rtrn: The signed angle between v0 and v1 expressed in degrees.
//-----------------------------------------------------------------------------
float SignedAngle(float3 v0, float3 v1, float3 axis)
{
    // Are the vectors aligned?
    float d = dot(v0, v1);
    if (1.0f - d < 1e-8f) return 0.0f;
    if (1.0f + d < 1e-8f) return 180.0f;

    // Calculate the angle between the 2 vectors using the dot product
    float angle = RadiansToDegrees(acos(d));

    // If we cross the 2 vectors and the resulting axes runs in the opposite direction
    // of 'axis', we have to change the sign of the angle.
    if (dot(normalize(cross(v0, v1)), axis) < 0.0f) angle = -angle;

    // Done!
    return angle;
}

//-----------------------------------------------------------------------------
// Name: PointInArc() (Function)
// Desc: Checks if 'pt' lies inside the specified arc. The function assumes 'pt'
//       lies on the arc plane.
// Parm: pt         - Query point.
//       arcStart   - Arc starting point.
//       arcCenter  - Arc center.
//       arcNormal  - Arc normal.
//       arcAngle   - Arc angle.
// Rtrn: True if 'pt' lies inside the arc and false otherwise.
//-----------------------------------------------------------------------------
bool PointInArc(float3 pt, float3 arcStart, float3 arcCenter, float3 arcNormal, float arcAngle)
{
    // Bail if the angle is really close to 0
    if (abs(arcAngle) <= 1e-5f)
        return false;

	// Arc angle wrap-around.
	// Note: The % operator is defined only in cases where either both sides are positive or both sides are negative.
	//		 https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-operators
	arcAngle = arcAngle % (sign(arcAngle) * 360.0f);

	// Calculate the angle between the starting point and the world point we are shading
	float3 v0		= normalize(arcStart - arcCenter);
	float3 v1		= normalize(pt - arcCenter);
	float  angle	= SignedAngle(v0, v1, arcNormal);
					
	// Are we dealing with an arc angle <= 180?
	if (abs(arcAngle) <= 180.0f)
	{
        if ((arcAngle < 0.0f && angle < arcAngle) ||            // Out of range when arc angle is negative
            (arcAngle > 0.0f && angle > arcAngle) ||            // Out of range when arc angle is positive
            (sign(arcAngle) != sign(angle))) return false;      // Going in the other direction
	}
	else 
	{
		// Calculate the gap angle. Pixels lying in this gap are rejected.
		float gapAngle = sign(arcAngle) * 360.0f - arcAngle;
        if ((gapAngle < 0.0f && angle > 0.0f && angle < abs(gapAngle)) ||
            (gapAngle > 0.0f && angle < 0.0f && angle > -gapAngle)) return false;
	}

    // The point is inside the arc
    return true;
}

//-----------------------------------------------------------------------------
// Name: SphereExtentToTangentPoint() (Function)
// Desc: Calculates the point where a ray from the camera position, tangent to
//       the sphere, intersects the sphere surface. The input point 'exPt' is
//       one of the sphere's extent points, obtained by projecting the sphere's
//       center along the camera's right, left, up, or down vectors.
// Parm: center - Sphere center.
//       radius - Sphere radius.
//       exPt   - A sphere extent point projected from the sphere center along
//                a camera axis (right, left, up, or down).
//       cameraForward  - Camera forward vector.
//       rayDir         - Camera ray fired from the camera position.
// Rtrn: The point of tangency where the ray from the camera position touches
//       the sphere surface.
//-----------------------------------------------------------------------------
float3 SphereExtentToTangentPoint(float3 center, float radius, float3 exPt, float3 cameraForward, float3 rayDir)
{
    // Precompute data
    float3 normal  = normalize(exPt - center);  // Sphere point normal

    // If the ray is shooting behind the camera, return the original point
    if (dot(cameraForward, rayDir) < 0.0f)
        return exPt;

    // Is the point's normal perpendicular to the camera ray direction?
    float vDot = dot(normal, rayDir);
    if (abs(vDot) > 1e-5f)
    {
        // We need to rotate the point normal so that it becomes perpendicular to the camera ray
        float  angle            = RadiansToDegrees(acos(vDot));
        float3 rotationAxis     = normalize(cross(rayDir, normal));
        float3x3 rotMtx         = RotationMatrixFromAngleAxis(DegreesToRadians(90.0f - angle), rotationAxis);

        // Rotate normal and recalculate point
        normal  = normalize(mul(rotMtx, normal));
        exPt    = center + normal * radius;
    }

    // Return new point (or the original one if no adjustment was needed)
    return exPt;
}
#endif