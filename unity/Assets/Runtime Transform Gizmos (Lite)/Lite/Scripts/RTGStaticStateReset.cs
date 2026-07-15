using System.Reflection;
using UnityEngine;

namespace RTGLite
{
    //-----------------------------------------------------------------------------
    // Name: RTGStaticStateReset (Public Static Class)
    // Desc: Ensures that all static runtime state is reset when Unity initializes
    //       subsystems. This is required to support domain reload disabled mode.
    // Note: Based on a community-provided solution discussed on the Unity forums:
    //       https://shorturl.at/uEazq
    //-----------------------------------------------------------------------------
    public static class RTGStaticStateReset
    {
        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: ResetStaticState() (Public Static Function)
        // Desc: Ensures that all static runtime state is reset when Unity initializes
        //       subsystems. This is required to support domain reload disabled mode.
        //-----------------------------------------------------------------------------
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticState()
        {
            // Reset static resource managers
            TextureManager.Internal_Reset();
            MaterialManager.Internal_Reset();
            ShaderManager.Internal_Reset();
            MeshManager.Internal_Reset();
            RTMeshManager.Internal_Reset();

            // Reset static extension data
            GameObjectEx.Internal_Reset();

            // Reset static fields in RTGizmos
            var rtGizmosType = typeof(RTGizmos);
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;

            // Reset sMtrlPropertyBlock
            var mtrlPropertyBlockField = rtGizmosType.GetField("sMtrlPropertyBlock", bindingFlags);
            if (mtrlPropertyBlockField != null)
                mtrlPropertyBlockField.SetValue(null, null);

            // Reset sMaterial
            var materialField = rtGizmosType.GetField("sMaterial", bindingFlags);
            if (materialField != null)
                materialField.SetValue(null, null);

            // Reset sGRS
            var grsField = rtGizmosType.GetField("sGRS", bindingFlags);
            if (grsField != null)
                grsField.SetValue(null, new GizmoRenderStates());
        }
        #endregion
    }
}