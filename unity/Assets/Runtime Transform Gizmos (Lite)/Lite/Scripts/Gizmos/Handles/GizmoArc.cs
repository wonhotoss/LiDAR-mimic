using UnityEngine;
using UnityEngine.Rendering;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoArc (Public Class)
    // Desc: Implements a gizmo arc. This can be useful when implementing indicator
    //       handles like the rotation gizmo's rotation indicator which appears when
    //       the user is rotation the gizmo.
    // Note: The arc exists in the XY plane in model space. The Z axis represents the
    //       arc normal. Additionally, the arc will always use a zoom scale of 1,
    //       regardless of the selected zoom scale.
    //-----------------------------------------------------------------------------
    public class GizmoArc : GizmoHandle
    {
        #region Private Static Readonly Fields
        static readonly int sNormalAxis = 2;   // Index of the arc's transform axis that represents the arc normal
        #endregion

        #region Private Fields
        GizmoArcStyle   mArcStyle   = new GizmoArcStyle();      // Default style
        float           mArcRadius  = 1.0f;                     // Arc radius
        Vector3         mArcStart;                              // Arc starting point. The arc angle is measured relative to this point.
        float           mArcAngle;                              // Arc angle in degrees measures relative to the starting point
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: arcStyle (Public Property)
        // Desc: Returns or sets the arc style.
        //-----------------------------------------------------------------------------
        public GizmoArcStyle                arcStyle    { get { return mArcStyle; } set { if (value != null) mArcStyle = value; } }

        //-----------------------------------------------------------------------------
        // Name: handleStyle (Public Property)
        // Desc: Returns the current style assigned to the handle.
        //-----------------------------------------------------------------------------
        public override GizmoHandleStyle    handleStyle { get { return mArcStyle; } }

        //-----------------------------------------------------------------------------
        // Name: arcRadius (Public Property)
        // Desc: Returns or the arc radius.
        //-----------------------------------------------------------------------------
        public float    arcRadius           { get { return mArcRadius; } }

        //-----------------------------------------------------------------------------
        // Name: arcPlane (Public Property)
        // Desc: Returns the arc plane.
        //-----------------------------------------------------------------------------
        public Plane    arcPlane            { get { return new Plane(arcNormal, transform.position); } }

        //-----------------------------------------------------------------------------
        // Name: arcNormal (Public Property)
        // Desc: Returns the arc normal.
        //-----------------------------------------------------------------------------
        public Vector3  arcNormal           { get { return transform.GetAxis(sNormalAxis); } }

        //-----------------------------------------------------------------------------
        // Name: arcStart (Public Property)
        // Desc: Returns the arc starting point. 
        //-----------------------------------------------------------------------------
        public Vector3  arcStart            { get { return mArcStart; } }

        //-----------------------------------------------------------------------------
        // Name: arcAngle (Public Property)
        // Desc: Returns the arc angle in degrees measured relative to the starting point.
        //-----------------------------------------------------------------------------
        public float    arcAngle            { get { return mArcAngle; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set (Public Function)
        // Desc: Sets the arc details.
        // Parm: start  - Arc starting point. The function will project this point on the
        //                arc plane and ensure that it sits at 'radius' units away from 
        //                the arc center. 
        //       radius - Arc radius. This must be a finalized value with zoom scale and
        //                any other scale values properly applied. You can achieve this by
        //                calling 'FVal' on the radius value before passing it into this function.
        //       angle  - Arc angle measured relative to the starting point.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 start, float radius, float angle)
        {
            // Set data
            mArcRadius      = Mathf.Max(0, radius);
            mArcAngle       = angle;

            // Project starting point on the arc plane and satisfy arc radius
            mArcStart       = arcPlane.ProjectPoint(start);
            mArcStart       = transform.position + (mArcStart - transform.position).normalized * mArcRadius;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB (Public Function)
        // Desc: Calculates and returns the handle's AABB.
        // Parm: camera - The camera that interacts with or renders the handle.
        // Rtrn: The handle's AABB.
        //-----------------------------------------------------------------------------
        public override Box CalculateAABB(Camera camera)
        {
            // Create a circle that describes the arc
            var circle = new Circle();
            circle.Set(transform.position, mArcRadius, transform.right, transform.forward);

            // Return the circle AABB
            return circle.CalculateAABB();
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: Raycast (Protected Function)
        // Desc: Performs a raycast check against the gizmo handle. This function always
        //       returns false for this type of handle.
        // Parm: args   - Raycast arguments.
        //       t      - Returns the distance from the ray origin where the intersection
        //                happens.
        // Rtrn: True if the ray intersects the handle and false otherwise.
        //-----------------------------------------------------------------------------
        protected override bool Raycast(GizmoHoverRaycastArgs args, out float t)
        {
            t = 0.0f;
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: OnRender (Protected Function)
        // Desc: Called when the handle must render itself.
        // Parm: args - Render arguments.
        //-----------------------------------------------------------------------------
        protected override void OnRender(GizmoHandleRenderArgs args)
        {
            RTGizmos.DrawArc(this, args);
        }
        #endregion
    }
    #endregion
}
