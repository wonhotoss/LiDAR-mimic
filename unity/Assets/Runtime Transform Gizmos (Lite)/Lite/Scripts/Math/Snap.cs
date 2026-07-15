using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: GridSnapDesc (Public Struct)
    // Desc: Describes a grid snap operation.
    //-----------------------------------------------------------------------------
    public struct GridSnapDesc
    {
        #region Public Fields
        public Vector3 cellSize;    // Grid cell size
        public Vector3 origin;      // Grid origin
        public Vector3 right;       // Grid right vector
        public Vector3 up;          // Grid up vector
        public Vector3 forward;     // Grid forward vector
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Snap (Public Static Class)
    // Desc: Implements useful snap utility functions.
    //-----------------------------------------------------------------------------
    public static class Snap
    {
        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: SnapValue() (Public Static Function)
        // Desc: Snaps 'value' to the closest multiple of 'snap'. Snap must be positive.
        // Parm: value - Value to snap.
        //       snap  - The snap increment. Must be positive.
        // Rtrn: 'value' rounded to the closest multiple of 'snap'.
        //-----------------------------------------------------------------------------
        public static float SnapValue(float value, float snap)
        {
            return Mathf.Round(value / snap) * snap;
        }

        //-----------------------------------------------------------------------------
        // Name: SnapValue() (Public Static Function)
        // Desc: Snaps 'value' to the closest multiple of 'snap'. Snap must be positive.
        // Parm: value      - Value to snap.
        //       snap       - The snap increment. Must be positive.
        //       stepCount  - The number of snap steps in value. For example, if 'value'
        //                    is 2 and 'snap' is 0.5, 'stepCount' is 4.
        // Rtrn: 'value' rounded to the closest multiple of 'snap'.
        //-----------------------------------------------------------------------------
        public static float SnapValue(float value, float snap, out int stepCount)
        {
            stepCount = Mathf.RoundToInt(value / snap);
            float res = stepCount * snap;
            stepCount = MathEx.FastAbs(stepCount);
            return res;
        }

        //-----------------------------------------------------------------------------
        // Name: GridSnapAxis() (Public Static Function)
        // Desc: Snaps 'point' to a grid axis.
        // Parm: point      - The point to snap.
        //       snapDesc   - Describes the grid snap operation.
        //       axis       - Grid axis to snap to: (0 = X, 1 = Y, 2 = Z).
        // Rtrn: The snapped point.
        //-----------------------------------------------------------------------------
        public static Vector3 GridSnapAxis(Vector3 point, GridSnapDesc snapDesc, int axis)
        {
            // Calculate the point's coordinates in grid space (i.e. along each grid axis)
            Vector3 toPoint = point - snapDesc.origin;
            float dotX      = Vector3.Dot(toPoint, snapDesc.right);
            float dotY      = Vector3.Dot(toPoint, snapDesc.up);
            float dotZ      = Vector3.Dot(toPoint, snapDesc.forward);

            // Snap the point to the specified axis
            if (axis == 0) dotX = SnapValue(dotX, snapDesc.cellSize.x);
            else if (axis == 1) dotY = SnapValue(dotY, snapDesc.cellSize.y);
            else dotZ = SnapValue(dotZ, snapDesc.cellSize.z);

            // Move the snapped point back to world space
            return (snapDesc.origin + snapDesc.right * dotX + snapDesc.up * dotY + snapDesc.forward * dotZ).FixFloatError();
        }

        //-----------------------------------------------------------------------------
        // Name: GridSnapAxes() (Public Static Function)
        // Desc: Snaps 'point' to grid axes.
        // Parm: point      - The point to snap.
        //       snapDesc   - Describes the grid snap operation.
        //       axisMask   - Grid axis mask. A value of 0, means the axis is ignored.
        // Rtrn: The snapped point.
        //-----------------------------------------------------------------------------
        public static Vector3 GridSnapAxes(Vector3 point, GridSnapDesc snapDesc, Vector3Int axisMask)
        {
            // Calculate the point's coordinates in grid space (i.e. along each grid axis)
            Vector3 toPoint = point - snapDesc.origin;
            float dotX      = Vector3.Dot(toPoint, snapDesc.right);
            float dotY      = Vector3.Dot(toPoint, snapDesc.up);
            float dotZ      = Vector3.Dot(toPoint, snapDesc.forward);

            // Snap the point
            if (axisMask.x != 0) dotX = SnapValue(dotX, snapDesc.cellSize.x);
            if (axisMask.y != 0) dotY = SnapValue(dotY, snapDesc.cellSize.y);
            if (axisMask.z != 0) dotZ = SnapValue(dotZ, snapDesc.cellSize.z);

            // Move the snapped point back to world space
            return (snapDesc.origin + snapDesc.right * dotX + snapDesc.up * dotY + snapDesc.forward * dotZ).FixFloatError();
        }

        //-----------------------------------------------------------------------------
        // Name: GridSnapAllAxes() (Public Static Function)
        // Desc: Snaps 'point' to all grid axes.
        // Parm: point      - The point to snap.
        //       snapDesc   - Describes the grid snap operation.
        // Rtrn: The snapped point.
        //-----------------------------------------------------------------------------
        public static Vector3 GridSnapAllAxes(Vector3 point, GridSnapDesc snapDesc)
        {
            // Bring point relative to the grid origin
            Vector3 toPoint = point - snapDesc.origin;
            Vector3 snapped = snapDesc.origin;

            // Snap along X
            float dot   = Vector3.Dot(toPoint, snapDesc.right);
            snapped     += snapDesc.right * SnapValue(dot, snapDesc.cellSize.x);

            // Snap along Y
            dot         = Vector3.Dot(toPoint, snapDesc.up);
            snapped     += snapDesc.up * SnapValue(dot, snapDesc.cellSize.y);

            // Snap along Z
            dot         = Vector3.Dot(toPoint, snapDesc.forward);
            snapped     += snapDesc.forward * SnapValue(dot, snapDesc.cellSize.z);

            // Return snapped point
            return snapped.FixFloatError();
        }
        #endregion
    }
    #endregion
}
