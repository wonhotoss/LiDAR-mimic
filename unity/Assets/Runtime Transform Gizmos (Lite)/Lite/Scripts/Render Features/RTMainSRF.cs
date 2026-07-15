using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTMainSRF (Public Class)
    // Desc: A 'ScriptableRendererFeature' that implements common rendering passes
    //       used to render different entities such as grid, gizmos etc.
    //-----------------------------------------------------------------------------
    public class RTMainSRF : ScriptableRendererFeature
    {
        #region Private Fields
        RTCameraBgSRP   mCamBgPass;     // Camera background pass
        RTMainSRP       mMainPass;      // Main pass
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Create() (Public Function)
        // Desc: Creates the render feature.
        //-----------------------------------------------------------------------------
        public override void Create()
        {
            // Create passes
            mCamBgPass = new RTCameraBgSRP();
            mCamBgPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

            mMainPass = new RTMainSRP();
            mMainPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        //-----------------------------------------------------------------------------
        // Name: AddRenderPasses() (Public Function)
        // Desc: This is called before rendering takes place in order to inject one or
        //       more render passes into the renderer.
        // Parm: renderer       - The renderer that receives the render passes.
        //       renderingData  - Rendering state which can be used to setup render passes.
        //-----------------------------------------------------------------------------
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // We don't want to do anything in the editor or if the plugin is not initialized
            if (!Application.isPlaying ||
                RTG.get == null) return;

            // Add passes
            renderer.EnqueuePass(mCamBgPass);
            renderer.EnqueuePass(mMainPass);
        }
        #endregion
    }
    #endregion
}