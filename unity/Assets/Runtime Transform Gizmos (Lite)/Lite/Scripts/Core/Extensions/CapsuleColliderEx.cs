using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: CapsuleColliderEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'CapsuleCollider'
    //       class.
    //-----------------------------------------------------------------------------
    public static class CapsuleColliderEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: GetWorldRadius (Public Extension)
        // Desc: Returns the collider's world radius.
        // Rtrn: The collider's world radius.
        //-----------------------------------------------------------------------------
        public static float GetWorldRadius(this CapsuleCollider c)
        {
            // The radius is scaled by the maximum scale component ignoring the height axis
            return c.radius * c.transform.lossyScale.MaxAbsComp(c.direction);
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldRadius (Public Extension)
        // Desc: Sets the collider's world radius.
        // Parm: worldRadius - World radius.
        //-----------------------------------------------------------------------------
        public static void SetWorldRadius(this CapsuleCollider c, float worldRadius)
        {
            // The radius is divided by the maximum scale component ignoring the height axis
            c.radius = worldRadius / c.transform.lossyScale.MaxAbsComp(c.direction);
            c.EnsureValidData();
        }

        //-----------------------------------------------------------------------------
        // Name: GetWorldHeight (Public Extension)
        // Desc: Returns the collider's world height.
        // Rtrn: The collider's world height.
        //-----------------------------------------------------------------------------
        public static float GetWorldHeight(this CapsuleCollider c)
        {
            return c.height * MathEx.FastAbs(c.transform.lossyScale[c.direction]);
        }

        //-----------------------------------------------------------------------------
        // Name: GetPartialWorldHeight (Public Extension)
        // Desc: Returns the collider's partial world height. This is the world height
        //       minus 2 times the world radius.
        // Rtrn: The collider's partial world height.
        //-----------------------------------------------------------------------------
        public static float GetPartialWorldHeight(this CapsuleCollider c)
        {
            return Mathf.Max(c.GetWorldHeight() - 2.0f * c.GetWorldRadius(), 0.0f);
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldHeight (Public Extension)
        // Desc: Sets the collider's world height.
        // Parm: worldHeight - World height.
        //-----------------------------------------------------------------------------
        public static void SetWorldHeight(this CapsuleCollider c, float worldHeight)
        {
            c.height = worldHeight / MathEx.FastAbs(c.transform.lossyScale[c.direction]);
            c.EnsureValidData();
        }

        //-----------------------------------------------------------------------------
        // Name: SetPartialWorldHeight (Public Extension)
        // Desc: Sets the collider's partial world height. This is the height of the
        //       collider minus 2 times the radius.
        // Parm: partialWorldHeight - Partial world height.
        //-----------------------------------------------------------------------------
        public static void SetPartialWorldHeight(this CapsuleCollider c, float partialWorldHeight)
        {
            float worldHeight = partialWorldHeight + 2.0f * c.GetWorldRadius();
            c.SetWorldHeight(worldHeight);
        }

        //-----------------------------------------------------------------------------
        // Name: GetWorldCenter (Public Extension)
        // Desc: Returns the collider's world center.
        // Rtrn: The collider's world center.
        //-----------------------------------------------------------------------------
        public static Vector3 GetWorldCenter(this CapsuleCollider c)
        {
            return c.transform.TransformPoint(c.center);
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldCenter (Public Extension)
        // Desc: Sets the collider's world center.
        // Parm: worldCenter - World center.
        //-----------------------------------------------------------------------------
        public static void SetWorldCenter(this CapsuleCollider c, Vector3 worldCenter)
        {
            c.center = c.transform.InverseTransformPoint(worldCenter);
        }

        //-----------------------------------------------------------------------------
        // Name: GetWorldHeightAxis (Public Extension)
        // Desc: Returns the collider's height axis in world space.
        // Rtrn: The collider's height axis in world space.
        //-----------------------------------------------------------------------------
        public static Vector3 GetWorldHeightAxis(this CapsuleCollider c)
        {
            switch(c.direction)
            {
                case 0: return c.transform.TransformDirection(Vector3.right);
                case 1: return c.transform.TransformDirection(Vector3.up);
                case 2: return c.transform.TransformDirection(Vector3.forward);
                default: return Vector3.zero;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: EnsureValidData (Public Extension)
        // Desc: Ensures the collider's data is valid (e.g. radius, height etc are valid).
        //-----------------------------------------------------------------------------
        public static void EnsureValidData(this CapsuleCollider c)
        {
            // Clip radius
            float worldRadius = c.GetWorldRadius();
            if (worldRadius < 0.0f) c.SetWorldRadius(0.0f);

            // Clip height. It must be no less than 2 times the radius.
            float h = c.GetWorldHeight();
            float r = c.GetWorldRadius();
            if (h < 2.0f * r) c.SetWorldHeight(2.0f * r);
        }
        #endregion
    }
    #endregion
}
