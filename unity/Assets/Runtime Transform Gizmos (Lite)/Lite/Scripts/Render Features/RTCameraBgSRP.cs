using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTCameraBgSRP (Public Class)
    // Desc: Implements the camera background SRP.
    //-----------------------------------------------------------------------------
    public class RTCameraBgSRP : ScriptableRenderPass
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
            using (var builder = renderGraph.AddRasterRenderPass("RTCameraBgSRP", out PassData passData))
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
                builder.SetRenderFunc<PassData>((passData, rasterCtx) => 
                { 
                    // Get camera render config
                    RTCamera.get.GetCameraRenderConfig(passData.camera, out RTCameraRenderConfig renderConfig);

                    // Can we render the camera background?
                    if ((renderConfig.renderFlags & ECameraRenderFlags.CameraBg) != 0)
                        RTCamera.get.Internal_Render(passData.camera, rasterCtx);
                });
            }
        }
        #endregion
    }
    #endregion
}
