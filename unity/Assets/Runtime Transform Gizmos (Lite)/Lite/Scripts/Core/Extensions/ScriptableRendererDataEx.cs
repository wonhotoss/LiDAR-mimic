using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering.Universal;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ScriptableRendererDataEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'ScriptableRendererData'
    //       class.
    //-----------------------------------------------------------------------------
    public static class ScriptableRendererDataEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: ContainsRenderFeature() (Public Extension)
        // Desc: Checks if the renderer data contains a render feature of the specified
        //       type.
        // Parm: featureType - Render feature type to check for.
        // Rtrn: True if the renderer data contains a feature of the specified type and
        //       false otherwise.
        //-----------------------------------------------------------------------------
        public static bool ContainsRenderFeature(this ScriptableRendererData rendererData, System.Type featureType)
        {
            // Loop through each feature and check if it matches the specified type
            var features = rendererData.rendererFeatures;
            foreach (var f in features)
            {
                // Do we have a match?
                if (f.GetType().Equals(featureType))
                    return true;
            }

            // No match found
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: AddRenderFeature() (Public Extension)
        // Desc: Adds a render feature of the specified type to the renderer data if
        //       a feature of the same type doesn't already exist.
        // Parm: T - Render feature type. Must derive from 'ScriptableRendererFeature'.
        //-----------------------------------------------------------------------------
        #if UNITY_EDITOR
        public static void AddRenderFeature<T>(this ScriptableRendererData rendererData) where T : ScriptableRendererFeature
        {
            // Only add if the feature is not already present
            if (rendererData.ContainsRenderFeature(typeof(T)))
                return;

            // Create the render feature and attach it to the renderer data asset
            var feature = ScriptableObject.CreateInstance<T>();
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            rendererData.rendererFeatures.Add(feature);
            EditorUtility.SetDirty(rendererData);

            // For future reference in case it's needed:
            // Thanks to https://forum.unity.com/threads/urp-adding-a-renderfeature-from-script.1117060/
            /*ScriptableRendererData[] rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(UniversalRenderPipeline.asset);
            int defaultRendererIndex = (int)typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GraphicsSettings.currentRenderPipeline);*/
        }
        #endif
        #endregion
    }
    #endregion
}
