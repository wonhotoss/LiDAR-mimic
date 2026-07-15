using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: BoxColliderEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'BoxCollider'
    //       class.
    //-----------------------------------------------------------------------------
    public static class BoxColliderEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: GetWorldSize (Public Extension)
        // Desc: Returns the collider's world size.
        // Rtrn: The collider's world size.
        //-----------------------------------------------------------------------------
        public static Vector3 GetWorldSize(this BoxCollider c)
        {
            return Vector3Ex.Abs(Vector3.Scale(c.size, c.transform.lossyScale));
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldSize (Public Extension)
        // Desc: Sets the collider's world size. This function also takes care of
        //       clipping the collider's size so that it doesn't take on invalid values.
        // Parm: worldSize - The collider's world size.
        //-----------------------------------------------------------------------------
        public static void SetWorldSize(this BoxCollider c, Vector3 worldSize)
        {
            Vector3 invScale = c.transform.lossyScale.Inverse();
            c.size = Vector3.Scale(worldSize, invScale);
            c.EnsureValidData();
        }

        //-----------------------------------------------------------------------------
        // Name: GetWorldCenter (Public Extension)
        // Desc: Returns the collider's world center.
        // Rtrn: The collider's world center.
        //-----------------------------------------------------------------------------
        public static Vector3 GetWorldCenter(this BoxCollider c)
        {
            return c.transform.TransformPoint(c.center);
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldCenter (Public Extension)
        // Desc: Sets the collider's world center.
        // Parm: worldCenter - World center.
        //-----------------------------------------------------------------------------
        public static void SetWorldCenter(this BoxCollider c, Vector3 worldCenter)
        {
            c.center = c.transform.InverseTransformPoint(worldCenter);
        }

        //-----------------------------------------------------------------------------
        // Name: EnsureValidData (Public Extension)
        // Desc: Ensures the collider's data is valid (i.e. no invalid size etc).
        //-----------------------------------------------------------------------------
        public static void EnsureValidData(this BoxCollider c)
        {
            c.size = Vector3Ex.Abs(c.size);
        }
        #endregion
    }
    #endregion
}
