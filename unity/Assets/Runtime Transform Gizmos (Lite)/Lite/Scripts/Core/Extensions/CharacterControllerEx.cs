using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: CharacterControllerEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'CharacterController'
    //       class.
    //-----------------------------------------------------------------------------
    public static class CharacterControllerEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: GetWorldRadius (Public Extension)
        // Desc: Returns the controller's world radius.
        // Rtrn: The controller's world radius.
        //-----------------------------------------------------------------------------
        public static float GetWorldRadius(this CharacterController c)
        {
            return c.radius * c.transform.lossyScale.MaxAbsComp();
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldRadius (Public Extension)
        // Desc: Sets the controller's world radius.
        // Parm: worldRadius - World radius.
        //-----------------------------------------------------------------------------
        public static void SetWorldRadius(this CharacterController c, float worldRadius)
        {
            c.radius = worldRadius / c.transform.lossyScale.MaxAbsComp();
            c.EnsureValidData();
        }

        //-----------------------------------------------------------------------------
        // Name: GetWorldHeight (Public Extension)
        // Desc: Returns the controller's world height.
        // Rtrn: The controller's world height.
        //-----------------------------------------------------------------------------
        public static float GetWorldHeight(this CharacterController c)
        {
            return c.height * MathEx.FastAbs(c.transform.lossyScale[1]);
        }

        //-----------------------------------------------------------------------------
        // Name: GetPartialWorldHeight (Public Extension)
        // Desc: Returns the controller's partial world height. This is the world height
        //       minus 2 times the world radius.
        // Rtrn: The controller's partial world height.
        //-----------------------------------------------------------------------------
        public static float GetPartialWorldHeight(this CharacterController c)
        {
            return Mathf.Max(c.GetWorldHeight() - 2.0f * c.GetWorldRadius(), 0.0f);
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldHeight (Public Extension)
        // Desc: Sets the controller's world height.
        // Parm: worldHeight - World height.
        //-----------------------------------------------------------------------------
        public static void SetWorldHeight(this CharacterController c, float worldHeight)
        {
            c.height = worldHeight / MathEx.FastAbs(c.transform.lossyScale[1]);
            c.EnsureValidData();
        }

        //-----------------------------------------------------------------------------
        // Name: SetPartialWorldHeight (Public Extension)
        // Desc: Sets the controller's partial world height. This is the height of the
        //       controller minus 2 times the radius.
        // Parm: partialWorldHeight - Partial world height.
        //-----------------------------------------------------------------------------
        public static void SetPartialWorldHeight(this CharacterController c, float partialWorldHeight)
        {
            float worldHeight = partialWorldHeight + 2.0f * c.GetWorldRadius();
            c.SetWorldHeight(worldHeight);
        }

        //-----------------------------------------------------------------------------
        // Name: GetWorldCenter (Public Extension)
        // Desc: Returns the controller's world center.
        // Rtrn: The controller's world center.
        //-----------------------------------------------------------------------------
        public static Vector3 GetWorldCenter(this CharacterController c)
        {
            return c.transform.TransformPoint(c.center);
        }

        //-----------------------------------------------------------------------------
        // Name: SetWorldCenter (Public Extension)
        // Desc: Sets the controller's world center.
        // Parm: worldCenter - World center.
        //-----------------------------------------------------------------------------
        public static void SetWorldCenter(this CharacterController c, Vector3 worldCenter)
        {
            c.center = c.transform.InverseTransformPoint(worldCenter);
        }

        //-----------------------------------------------------------------------------
        // Name: GetWorldHeightAxis (Public Extension)
        // Desc: Returns the controller's height axis in world space.
        // Rtrn: The controller's height axis in world space.
        //-----------------------------------------------------------------------------
        public static Vector3 GetWorldHeightAxis(this CharacterController c)
        {
            return Vector3.up;
        }

        //-----------------------------------------------------------------------------
        // Name: EnsureValidData (Public Extension)
        // Desc: Ensures the controller's data is valid (e.g. radius, height etc are valid).
        //-----------------------------------------------------------------------------
        public static void EnsureValidData(this CharacterController c)
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
