using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTMainSRP (Public Class)
    // Desc: Implements rendering for different kinds of entities, such as gizmos,
    //       grid etc.
    //-----------------------------------------------------------------------------
    public class RTMainSRP : ScriptableRenderPass
    {
        #region Private Classes
        class PassData { public Camera camera; }    // Used to send data to the render pass
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: RecordRenderGraph() (Public Function)
        // Desc: Records the render graph. This is where rendering occurs.
        // Parm: renderGraph - The render graph.
        //       frameData   - Stores rendering data and resources.
        //-----------------------------------------------------------------------------
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Add a new raster pass
            using (var builder = renderGraph.AddRasterRenderPass("RTMainSRP", out PassData passData))
            {
                // Store camera
                passData.camera = frameData.Get<UniversalCameraData>().camera;

                // Get resource data
                UniversalResourceData resources = frameData.Get<UniversalResourceData>();

                // Use the camera's render target for rendering
                builder.SetRenderAttachment(resources.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resources.activeDepthTexture);
                builder.AllowPassCulling(false);

                // Set render function
                builder.SetRenderFunc<PassData>(Render);
            }
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: Render() (Private Function)
        // Desc: Performs all the necessary rendering.
        // Parm: passData   - Pass data.
        //       rasterCtx  - The raster graph context.
        //-----------------------------------------------------------------------------
        void Render(PassData passData, RasterGraphContext rasterCtx)
        {
            // Get camera render config
            RTCamera.get.GetCameraRenderConfig(passData.camera, out RTCameraRenderConfig renderConfig);

            // Can we render the scene grid?
            if ((renderConfig.renderFlags & ECameraRenderFlags.SceneGrid) != 0)
                RTGrid.get.Internal_Render(passData.camera, rasterCtx);

            // Can we render the gizmos?
            if ((renderConfig.renderFlags & ECameraRenderFlags.Gizmos) != 0)
                RTGizmos.get.Internal_Render(passData.camera, rasterCtx);
        }
        #endregion
    }
    #endregion
}
