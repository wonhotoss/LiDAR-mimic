using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: SphereColliderEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'SphereCollider'
    //       class.
    //-----------------------------------------------------------------------------
    public static class SphereColliderEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: GetWorldRadius (Public Extension)
        // Desc: Returns the collider's world radius.
        // Rtrn: The collider's world radius.
        //-----------------------------------------------------------------------------
        public static float GetWorldRadius(this SphereCollider c)
        {
            Vector3 scale = c.transform.lossyScale;
            return c.radius * scale.MaxAbsComp();
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldRadius (Public Extension)
        // Desc: Sets the collider's world radius. This function also takes care of
        //       clipping the collider's radius so that it doesn't take on invalid values.
        // Parm: worldRadius - World radius.
        //-----------------------------------------------------------------------------
        public static void SetWorldRadius(this SphereCollider c, float worldRadius)
        {
            Vector3 scale = c.transform.lossyScale;
            c.radius = worldRadius / scale.MaxAbsComp();
            c.EnsureValidData();
        }

        //-----------------------------------------------------------------------------
        // Name: GetWorldCenter (Public Extension)
        // Desc: Returns the collider's world center.
        // Rtrn: The collider's world center.
        //-----------------------------------------------------------------------------
        public static Vector3 GetWorldCenter(this SphereCollider c)
        {
            return c.transform.TransformPoint(c.center);
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldCenter (Public Extension)
        // Desc: Sets the collider's world center.
        // Parm: worldCenter - World center.
        //-----------------------------------------------------------------------------
        public static void SetWorldCenter(this SphereCollider c, Vector3 worldCenter)
        {
            c.center = c.transform.InverseTransformPoint(worldCenter);
        }

        //-----------------------------------------------------------------------------
        // Name: EnsureValidData (Public Extension)
        // Desc: Ensures the collider's data is valid (i.e. no invalid radius etc).
        //-----------------------------------------------------------------------------
        public static void EnsureValidData(this SphereCollider c)
        {
            c.radius = Mathf.Abs(c.radius);
        }
        #endregion
    }
    #endregion
}
