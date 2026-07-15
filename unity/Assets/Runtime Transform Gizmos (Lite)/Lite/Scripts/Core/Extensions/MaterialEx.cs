using UnityEngine;
using UnityEngine.Rendering;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: MaterialEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Material' class.
    //-----------------------------------------------------------------------------
    public static class MaterialEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: IsCutout() (Public Extension)
        // Desc: Checks if the material uses alpha testing.
        // Rtrn: True if the material uses alpha testing and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool IsCutout(this Material material)
        {
            // Requires the '_AlphaClip' property
            if (!material.HasProperty("_AlphaClip")) return false;

            // Check '_AlphaClip' value. Must be 1.
            return (int)material.GetFloat("_AlphaClip") == 1;
        }

        //-----------------------------------------------------------------------------
        // Name: SetZTestAlways() (Public Extension)
        // Desc: Sets the ZTest to 'Always'. The material's shader must have a '_ZTest'
        //       integer property.
        //-----------------------------------------------------------------------------
        public static void SetZTestAlways(this Material material)
        {
            material.SetInt("_ZTest", (int)CompareFunction.Always);
        }

        //-----------------------------------------------------------------------------
        // Name: SetZTestLessEqual() (Public Extension)
        // Desc: Sets the ZTest to 'LessEqual'. The material's shader must have a '_ZTest'
        //       integer property.
        //-----------------------------------------------------------------------------
        public static void SetZTestLessEqual(this Material material)
        {
            material.SetInt("_ZTest", (int)CompareFunction.LessEqual);
        }

        //-----------------------------------------------------------------------------
        // Name: SetZTest() (Public Extension)
        // Desc: Sets the ZTest to the specified comparison function. The material's
        //       shader must have a '_ZTest' integer property.
        // Parm: compFunc - Comparison function.
        //-----------------------------------------------------------------------------
        public static void SetZTest(this Material material, CompareFunction compFunc)
        {
            material.SetInt("_ZTest", (int)compFunc);
        }

        //-----------------------------------------------------------------------------
        // Name: SetZWriteEnabled() (Public Extension)
        // Desc: Sets the ZWrite state to the specified value. The material's shader must
        //       have a '_ZWrite' integer property.
        // Parm: enabled - True if ZWrite should be enabled and false otherwise.
        //-----------------------------------------------------------------------------
        public static void SetZWriteEnabled(this Material material, bool enabled)
        {
            material.SetInt("_ZWrite", enabled ? 1 : 0);
        }

        //-----------------------------------------------------------------------------
        // Name: SetCullEnabled() (Public Extension)
        // Desc: Sets the Cull state to either 'Off' or 'Back' depending on the specified
        //       value. The material's shader must have a '_CullMode' integer property.
        // Parm: enabled - If true, the Cull state is set to 'Back'. Otherwise, it is set
        //                 to 'Off'.
        //-----------------------------------------------------------------------------
        public static void SetCullEnabled(this Material material, bool enabled)
        {
            material.SetInt("_CullMode", enabled ? (int)CullMode.Back : (int)CullMode.Off);
        }

        //-----------------------------------------------------------------------------
        // Name: SetZTest() (Public Extension)
        // Desc: Sets the Cull mode to the specified value. The material's shader must
        //       have a '_CullMode' integer property.
        // Parm: cullMode - Cull mode.
        //-----------------------------------------------------------------------------
        public static void SetCullMode(this Material material, CullMode cullMode)
        {
            material.SetInt("_CullMode", (int)cullMode);
        }
        #endregion
    }
    #endregion
}
