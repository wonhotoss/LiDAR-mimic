using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ViewGizmo (Public Class)
    // Desc: Implements a view gizmo which can be displayed inside a camera's viewport.
    //       A view gizmo can be used to align the camera's view vector to different
    //       world axes, perform projection switches (i.e. perspective to ortho and
    //       vice versa) and it offers hints about the camera's rotation relative to
    //       the world axes.
    //-----------------------------------------------------------------------------
    public class ViewGizmo : Gizmo
    {
        #region Private Static Fields
        static Quaternion[] sAxisCapsPRotations = new Quaternion[3]
        { Quaternion.Euler(0.0f, 180.0f, 0.0f), Quaternion.Euler(0.0f, 0.0f, -90.0f), Quaternion.Euler(0.0f, 90.0f, 0.0f) };    // Rotations used for the positive axis caps
        static Quaternion[] sAxisCapsNRotations = new Quaternion[3]
        { Quaternion.identity, Quaternion.Euler(0.0f, 0.0f, 90.0f), Quaternion.Euler(0.0f, -90.0f, 0.0f) }; // Rotations used for the negative axis caps
        #endregion

        #region Private Fields
        GizmoHandleRenderArgs   mHRenderArgs    = new GizmoHandleRenderArgs();      // Gizmo handle render args
        GizmoCap[]              mAxisCapsP      = new GizmoCap[3]   { new GizmoCap(), new GizmoCap(), new GizmoCap() }; // Positive axis caps
        GizmoCap[]              mAxisCapsN      = new GizmoCap[3]   { new GizmoCap(), new GizmoCap(), new GizmoCap() }; // Negative axis caps
        GizmoCap                mCenterCap      = new GizmoCap();                   // The center cap which can be used to perform projection switches
        Camera                  mViewCamera;                                        // The view camera. The gizmo is displayed inside the viewport of this camera.
        Camera                  mRenderCamera;                                      // The camera that renders the gizmo inside the view camera's viewport

        // Used to animate the alpha of the gizmo axes that are aligned with the camera view vector
        Tween<float>[]          mAlphaTweens    = new Tween<float>[3] { new Tween<float>(), new Tween<float>(), new Tween<float>() };  // Array of alpha tweens, one for each pair of axes

        List<GizmoHandle>       mSortedHandles      = new List<GizmoHandle>();      // Used to sort handles
        Vector3[]               mGizmoAxes          = new Vector3[3];               // Used to collect the gizmo's local axes
        MaterialPropertyBlock   mMtrlPropertyBlock  = new MaterialPropertyBlock();  // Used for rendering tasks
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: style (Public Property)
        // Desc: Returns or sets the view gizmo style.
        //-----------------------------------------------------------------------------
        public ViewGizmoStyle   style           { get; set; } = null;

        //-----------------------------------------------------------------------------
        // Name: activeStyle (Public Property)
        // Desc: Returns the active style used by the gizmo. This is 'style' if it was set
        //       to a valid style. Otherwise, it is the active skin's view gizmo style.
        //-----------------------------------------------------------------------------
        public ViewGizmoStyle   activeStyle     { get { return style == null ? RTGizmos.get.skin.viewGizmoStyle : style; } }

        //-----------------------------------------------------------------------------
        // Name: viewCamera (Public Property)
        // Desc: Returns or sets the gizmo's view camera. The gizmo is displayed inside
        //       the viewport of this camera. Not to be confused with the render camera.
        //       the camera that renders the view gizmo is not the same as the view camera.
        //-----------------------------------------------------------------------------
        public Camera           viewCamera      { get { return mViewCamera; } set { if (value != null) mViewCamera = value; } }
        
        //-----------------------------------------------------------------------------
        // Name: renderCamera (Public Property)
        // Desc: Returns the gizmo's render camera. This is the camera that renders the
        //       gizmo inside the viewport of the gizmo's view camera.
        //-----------------------------------------------------------------------------
        public Camera           renderCamera    { get { return mRenderCamera; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: IsGUIHovered() (Public Function)
        // Desc: Checks if the gizmo's GUI is hovered. The gizmo GUI is whatever gets
        //       drawn inside 'OnGUI' that the user can interact with.
        // Rtrn: True if the gizmo's GUI is hovered and false otherwise.
        //-----------------------------------------------------------------------------
        public override bool IsGUIHovered()
        {
            // If the gizmo's view camera is disabled, the gizmo can't be hovered
            if (mViewCamera == null || !mViewCamera.enabled)
                return false;

            // Calculate gizmo GUI rect and the mouse cursor position in GUI space
            var guiRect = CalculateGizmoGUIRect();
            var p = CameraEx.ScreenToGUIPoint(RTInput.get.pointingInputDevice.position);
      
            // Check if the cursor position lies inside the projection switch labels
            if (CalculatePModeLabelGUIRect(guiRect, TextureManager.perspectiveCamModeIcon).Contains(p) ||
                CalculatePModeLabelGUIRect(guiRect, TextureManager.orthoCamModeIcon).Contains(p)) return true;

            // Not hovered
            return false;
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: IsReady() (Protected Function)
        // Desc: Checks if the gizmo is ready to use.
        // Rtrn: True if the gizmo is ready to use and false otherwise.
        //-----------------------------------------------------------------------------
        protected override bool IsReady()
        {
            // The gizmo is ready when its view camera is enabled.
            // Note: The render camera should be disabled when the gizmo is not ready.
            //       Otherwise, it might not get a chance to create its render target
            //       and it will just render on top of everything else.
            bool ready = mViewCamera != null && mViewCamera.enabled;
            mRenderCamera.enabled = ready;

            // Return ready state
            return ready;
        }

        //-----------------------------------------------------------------------------
        // Name: GetActiveGizmoStyle() (Protected Function)
        // Desc: Returns the active gizmo style.
        //-----------------------------------------------------------------------------
        protected override GizmoStyle GetActiveGizmoStyle()
        {
            return activeStyle;
        }

        //-----------------------------------------------------------------------------
        // Name: OnCreateHandles() (Protected Function)
        // Desc: Called during gizmo initialization to allow the gizmo to create all the
        //       handles that it needs. On function return, these handles will be made
        //       children of the gizmo transform.
        // Parm: handles - Returns the list of handles.
        //-----------------------------------------------------------------------------
        protected override void OnCreateHandles(List<GizmoHandle> handles)
        {
            // Store axis caps
            int count = mAxisCapsP.Length;
            for (int i = 0; i < count; ++i)
            {
                // Set tag
                mAxisCapsP[i].tag = (EGizmoHandleTag)i;
                mAxisCapsN[i].tag = (EGizmoHandleTag)i;

                // Store tween action for this axis pair
                var axisCapP = mAxisCapsP[i];
                var axisCapN = mAxisCapsN[i];
                mAlphaTweens[i].tweenUpdated = (val) => { axisCapP.alphaScale = val; axisCapN.alphaScale = val; };

                // Scale modes
                mAxisCapsP[i].zoomScaleMode         = EGizmoHandleZoomScaleMode.One;
                mAxisCapsP[i].transformScaleMode    = EGizmoHandleTransformScaleMode.One;
                mAxisCapsN[i].zoomScaleMode         = EGizmoHandleZoomScaleMode.One;
                mAxisCapsN[i].transformScaleMode    = EGizmoHandleTransformScaleMode.One;
               
                // Store handles
                handles.Add(mAxisCapsP[i]);
                handles.Add(mAxisCapsN[i]);
            }

            // Store mid cap
            mCenterCap.tag                 = EGizmoHandleTag.ProjectionSwitch;
            mCenterCap.zoomScaleMode       = EGizmoHandleZoomScaleMode.One;
            mCenterCap.transformScaleMode  = EGizmoHandleTransformScaleMode.One;
            handles.Add(mCenterCap);

            // Create the render camera
            mRenderCamera   = RTGizmos.get.Internal_CreateViewGizmoRenderCamera();

            // Let's also register any handlers. When the mouse hovers the projection
            // mode switch label, we don't want to allow other gizmos to be hovered.
            RTGizmos.get.hoverEnabledQuery += (answer) => 
            {
                // No-op?
                if (mViewCamera == null)
                {
                    answer.Yes();
                    return;
                }

                // Check if the mouse is inside the projection mode switch label.
                // Check both textures in case we are doing a transition.
                Rect gizmoGUIRect = CalculateGizmoGUIRect();
                Rect labelRect0   = CalculatePModeLabelGUIRect(gizmoGUIRect, TextureManager.perspectiveCamModeIcon);
                Rect labelRect1   = CalculatePModeLabelGUIRect(gizmoGUIRect, TextureManager.orthoCamModeIcon);
                Vector2 p = CameraEx.ScreenToGUIPoint(RTInput.get.pointingInputDevice.position);
                if (!labelRect0.Contains(p) && !labelRect1.Contains(p)) answer.Yes();
                else answer.No();
            };
        }

        //-----------------------------------------------------------------------------
        // Name: OnUpdateHandles() (Protected Function)
        // Desc: Called to allow the gizmo to update its handles. This could mean setting
        //       handle styles, updating their transforms etc. This function is called
        //       once for each camera that renders the gizmo. It is also called once during
        //       the update step to allow for correct interaction between the active camera
        //       and the gizmo handles.
        // Parm: updateReason  - The reason behind the update request.
        //       camera        - The camera that interacts with or renders the gizmo.
        //-----------------------------------------------------------------------------
        protected override void OnUpdateHandles(EGizmoHandleUpdateReason updateReason, Camera camera)
        {
            // Ignore OnGUI
            if (updateReason == EGizmoHandleUpdateReason.OnGUI)
                return;

            // Handle projection switch label interaction
            if (updateReason == EGizmoHandleUpdateReason.Update)
            {
                var gizmoRect = CalculateGizmoGUIRect();
                if (RTCamera.get.isSwitchingProjection && camera == RTCamera.get.settings.targetCamera)
                {
                    // Is the projection mode label visible?
                    if (activeStyle.camPModeLabelVisible)
                    {
                        // Calculate the 2 label rectangles
                        Rect orthoRect  = CalculatePModeLabelGUIRect(gizmoRect, TextureManager.orthoCamModeIcon);
                        Rect perspRect  = CalculatePModeLabelGUIRect(gizmoRect, TextureManager.perspectiveCamModeIcon);

                        // If we clicked on any of the 2 labels, do a projection switch
                        Vector2 cursorPos = CameraEx.ScreenToGUIPoint(RTInput.get.pointingInputDevice.position);
                        if (RTInput.get.pointingInputDevice.pickButtonWentUp && !RTGizmos.get.dragEndedThisFrame &&
                            (orthoRect.Contains(cursorPos) || 
                             perspRect.Contains(cursorPos)))
                        {
                            RTScene.get.CapturePointerInteraction();
                            SwitchProjection();
                        }
                    }
                }
                else
                {     
                    // Is the projection mode label visible?
                    if (activeStyle.camPModeLabelVisible)
                    {
                        // Place the label at the bottom of the gizmo rect
                        Texture2D labelTexture = mViewCamera.orthographic ? TextureManager.orthoCamModeIcon : TextureManager.perspectiveCamModeIcon;
                        Rect labelRect = CalculatePModeLabelGUIRect(gizmoRect, labelTexture);

                        // If we clicked on the label, do a projection switch
                        if (RTInput.get.pointingInputDevice.pickButtonWentUp && !RTGizmos.get.dragEndedThisFrame &&
                            labelRect.Contains(CameraEx.ScreenToGUIPoint(RTInput.get.pointingInputDevice.position)))
                        {
                            RTScene.get.CapturePointerInteraction();
                            SwitchProjection();
                        }
                    }
                }
            }

            // Keep the gizmo axes up to date
            transform.CollectAxes(mGizmoAxes);

            // When the axes become aligned with the camera view vector, we need to transition to an alpha scale of 0.
            // Also, if the axes were previously aligned but now they're not, we need to transition to an alpha scale of 1.
            int count = mAlphaTweens.Length;
            for (int i = 0; i < count; ++i)
            {
                // Tween duration in seconds
                const float tweenDuration = 0.35f;

                // Check alignment and start tweens
                float dot = Vector3Ex.AbsDot(camera.transform.forward, mGizmoAxes[i]);
                if (dot > 0.9f)
                {
                    // We need to fade the alpha scale to 0. Fade if the alpha scale if not already 0, and the
                    // tween is not already active OR it is active but it is fading in the other direction.
                    if (mAxisCapsP[i].alphaScale > 0.0f && (!mAlphaTweens[i].active || mAlphaTweens[i].endValue != 0.0f))
                        mAlphaTweens[i].Start(mAxisCapsP[i].alphaScale, 0.0f, tweenDuration);
                }
                else if (dot <= 0.9f)
                {
                    // We need to fade the alpha scale to 1. Fade if the alpha scale if not already 1, and the
                    // tween is not already active OR it is active but it is fading in the other direction.
                    if (mAxisCapsP[i].alphaScale < 1.0f && (!mAlphaTweens[i].active || mAlphaTweens[i].endValue != 1.0f)) 
                        mAlphaTweens[i].Start(mAxisCapsP[i].alphaScale, 1.0f, tweenDuration);
                }

                // Update tween
                mAlphaTweens[i].Update();

                // Update axis visibility. We want to make the handles invisible when their alpha
                // scale drops to 0 to disable hovering for those axes.
                if (mAxisCapsP[i].alphaScale == 0.0f)
                {
                    mAxisCapsP[i].visible = false;
                    mAxisCapsN[i].visible = false;
                }
                else
                {
                    mAxisCapsP[i].visible = true;
                    mAxisCapsN[i].visible = true;
                }
            }

            // Update axis caps
            float halfMidSize = activeStyle.GetCenterCapSize() / 2.0f;
            float axisLength  = activeStyle.GetAxisCapLength();
            GlobalGizmoStyle globalStyle = RTGizmos.get.skin.globalGizmoStyle;
            count = mAxisCapsP.Length;
            for (int i = 0; i < count; ++i)
            {
                // Attach cap styles
                mAxisCapsP[i].capStyle          = activeStyle.GetAxisCapStyle(i, true);
                mAxisCapsP[i].capStyle.color    = globalStyle.GetAxisColor(i);
                mAxisCapsN[i].capStyle          = activeStyle.GetAxisCapStyle(i, false);

                // Set rotations
                mAxisCapsP[i].transform.rotation = sAxisCapsPRotations[i];
                mAxisCapsN[i].transform.rotation = sAxisCapsNRotations[i];

                // Set positions
                mAxisCapsP[i].transform.position = mCenterCap.transform.position + mGizmoAxes[i] * mAxisCapsP[i].FVal((halfMidSize + axisLength), camera);
                mAxisCapsN[i].transform.position = mCenterCap.transform.position - mGizmoAxes[i] * mAxisCapsN[i].FVal((halfMidSize + axisLength), camera);
            }

            // Update mid cap
            mCenterCap.capStyle = activeStyle.centerCapStyle;

            // Sync camera with the view camera
            camera.orthographic         = mViewCamera.orthographic;
            camera.fieldOfView          = mViewCamera.fieldOfView;
            camera.transform.rotation   = mViewCamera.transform.rotation;

            // Focus the camera on the gizmo
            camera.Focus(camera.CalculateFocusData(CalculateSphere(camera)));

            // Update the camera target texture. We will render the gizmo to a render texture and draw the texture inside 'OnGUI'.
            RenderTexture targetTexture = camera.targetTexture;
            if (targetTexture == null || targetTexture.width != activeStyle.screenSize)
            {
                // Destroy the old texture and create a new one with the new specs
                if (targetTexture != null) { camera.targetTexture = null; RenderTexture.DestroyImmediate(targetTexture); }

                // Create the target texture.
                // Note: The 'GraphicsFormat.R32G32B32A32' format doesn't do alpha-blending correctly. It doesn't blend
                //       with the contents of the render texture for some reason.
                targetTexture = new RenderTexture(activeStyle.screenSize, activeStyle.screenSize, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D32_SFloat);
                camera.targetTexture = targetTexture;
            }

            // If the view camera is not the active camera, the handles should not be hoverable
            bool hoverable = mViewCamera == RTCamera.get.settings.targetCamera;
            count = internal_handles.Count;
            for (int i = 0; i < count; ++i)
                internal_handles[i].hoverable = hoverable;
        }

        //-----------------------------------------------------------------------------
        // Name: OnRender() (Protected Function)
        // Desc: Called when the gizmo must be rendered.
        // Parm: camera    - The camera which renders the gizmo.
        //       rasterCtx - The raster graph context.
        //-----------------------------------------------------------------------------
        protected override void OnRender(Camera camera, RasterGraphContext rasterCtx)
        {
            // Fill render args
            mHRenderArgs.camera    = camera;
            mHRenderArgs.rasterCtx = rasterCtx;

            // Sort handles
            GizmoHandle.ZSortHandles(internal_handles, mHRenderArgs, mSortedHandles);

            // Render sorted handles
            Material labelMaterial = MaterialManager.rtUnlit;
            int count = mSortedHandles.Count;
            for (int i = 0; i < count; ++i)
            {
                // Render handle
                mSortedHandles[i].Render(mHRenderArgs);

                // Render label for positive axes
                if (mSortedHandles[i] == mAxisCapsP[0] ||
                    mSortedHandles[i] == mAxisCapsP[1] ||
                    mSortedHandles[i] == mAxisCapsP[2]) RenderAxisLabel(mSortedHandles[i] as GizmoCap, camera, rasterCtx);
            }
        }
        
        //-----------------------------------------------------------------------------
        // Name: OnStartDrag() (Protected Function)
        // Desc: Called to notify the gizmo that a drag operation can start.
        // Parm: camera - The camera that interacts with the gizmo.
        // Rtrn: An instance 'GizmoDrag' if the drag operation can start and null otherwise.
        //-----------------------------------------------------------------------------
        protected override GizmoDrag OnStartDrag(Camera camera)
        {
            // A view gizmo doesn't support dragging
            return null;
        }

        //-----------------------------------------------------------------------------
        // Name: OnClickedHoveredHandle() (Protected Function)
        // Desc: Called when the user clicks/taps the hovered handle.
        //-----------------------------------------------------------------------------
        protected override void OnClickedHoveredHandle()
        {
            // Identify the hovered handle
            GizmoHandle hovHandle = hoverData.handle;
            if (hovHandle == mCenterCap)
            {
                // Do a projection switch
                SwitchProjection();
            }
            else SwitchRotation(hovHandle as GizmoCap); // Might be one of the axis caps, in which case we can switch rotation
        }

        //-----------------------------------------------------------------------------
        // Name: OnGUI() (Protected Function)
        // Desc: Called during the 'OnGUI' event to allow the gizmo to draw GUI elements.
        // Parm: camera - The camera that renders the GUI.
        //-----------------------------------------------------------------------------
        protected override void OnGUI(Camera camera)
        {
            // Only render the scene gizmo for its associated view camera
            if (camera != mViewCamera)
                return;

            // Draw gizmo
            var gizmoRect = CalculateGizmoGUIRect();
            if (mRenderCamera.targetTexture)
                GUI.DrawTexture(gizmoRect, mRenderCamera.targetTexture);    // Can be null if the gizmo hasn't had a chance to update during a frame, but OnGUI is called

            // Now we need to draw the camera mode label. If we are currently involved in a projection switch
            // we will draw 2 labels where one is fading in and the other is fading out based on the switch
            // progress. Otherwise, we can just draw a single label that corresponds to the current camera mode.
            Color oldColor = GUI.color;
            if (RTCamera.get.isSwitchingProjection && camera == RTCamera.get.settings.targetCamera)
            {
                // Calculate alpha scale values for both labels depending on what we're switching to
                float orthoAlphaScale = 1.0f, perspAlphaScale = 1.0f;
                if (RTCamera.get.isSwitchingToOrtho)
                {
                    orthoAlphaScale = RTCamera.get.projectionSwitchProgress;
                    perspAlphaScale = 1.0f - orthoAlphaScale;
                }
                else
                {
                    perspAlphaScale = RTCamera.get.projectionSwitchProgress;
                    orthoAlphaScale = 1.0f - perspAlphaScale;
                }

                // Is the projection mode label visible?
                if (activeStyle.camPModeLabelVisible)
                {
                    // Calculate the 2 label rectangles
                    Rect orthoRect  = CalculatePModeLabelGUIRect(gizmoRect, TextureManager.orthoCamModeIcon);
                    Rect perspRect  = CalculatePModeLabelGUIRect(gizmoRect, TextureManager.perspectiveCamModeIcon);

                    // Draw the 2 textures using the calculated alpha scale values
                    GUI.color = activeStyle.camPModeLabelColor.WithAlpha(orthoAlphaScale);
                    GUI.DrawTexture(orthoRect, TextureManager.orthoCamModeIcon);
                    GUI.color = activeStyle.camPModeLabelColor.WithAlpha(perspAlphaScale);
                    GUI.DrawTexture(perspRect, TextureManager.perspectiveCamModeIcon);
                }
            }
            else
            {     
                // Is the projection mode label visible?
                if (activeStyle.camPModeLabelVisible)
                {
                    // Place the label at the bottom of the gizmo rect
                    Texture2D labelTexture = mViewCamera.orthographic ? TextureManager.orthoCamModeIcon : TextureManager.perspectiveCamModeIcon;
                    Rect labelRect = CalculatePModeLabelGUIRect(gizmoRect, labelTexture);

                    // Draw
                    GUI.color = activeStyle.camPModeLabelColor;
                    GUI.DrawTexture(labelRect, labelTexture);
                }
            }

            // Restore GUI color
            GUI.color = oldColor;
        }

        //-----------------------------------------------------------------------------
        // Name: AcceptRenderCamera() (Protected Function)
        // Desc: Called before the gizmo is about to be rendered by the specified camera
        //       in order to check if the camera is allowed to render the gizmo.
        // Parm: renderCamera - The camera that wants to render the gizmo.
        // Rtrn: True if 'renderCamera' is allowed to render the gizmo and false otherwise.
        //-----------------------------------------------------------------------------
        protected override bool AcceptRenderCamera(Camera renderCamera) 
        {
            return renderCamera == mRenderCamera;
        }

        //-----------------------------------------------------------------------------
        // Name: GetInteractionCamera() (Protected Function)
        // Desc: Returns the camera that interacts with the gizmo.
        // Rtrn: The camera that interacts with the gizmo.
        //-----------------------------------------------------------------------------
        protected override Camera GetInteractionCamera()
        {
            return mRenderCamera;
        }

        //-----------------------------------------------------------------------------
        // Name: GetOnGUICamera() (Protected Function)
        // Desc: Returns the camera that is allowed to render the gizmo GUI during 'OnGUI'.
        //-----------------------------------------------------------------------------
        protected override Camera GetOnGUICamera()
        {
            return mViewCamera;
        }

        //-----------------------------------------------------------------------------
        // Name: GetPickRay() (Protected Virtual Function)
        // Desc: Calculates and returns a ray that can be used to pick gizmo handles.
        // Parm: camera - The camera that interacts with or renders the gizmo.
        // Rtrn: A ray that can be used to pick gizmo handles.
        //-----------------------------------------------------------------------------
        protected override Ray GetPickRay(Camera camera)
        {
            /* This also seems to work.
            Vector2 pt = RTInput.get.pointingInputDevice.position;
            Rect r = CalculateGizmoGUIRect();
            r = new Rect(r.x, Screen.height - 1 - (r.yMax - 1), r.width, r.height);
            pt.x -= r.x;
            pt.y -= r.y;
            return camera.ScreenPointToRay(new Vector3(pt.x, pt.y, camera.nearClipPlane));*/

            // Using 'ScreenPointToRay' doesn't work for the view gizmo because we are
            // rendering to a render texture. So we need to do this manually. First, we
            // need to calculate the GUI rectangle where the gizmo is displayed. This 
            // can be interpreted as a viewport rectangle for an imaginary camera that
            // renders in that area of the screen.
            Rect viewRect = CalculateGizmoGUIRect();

            // Calculate the ray using a custom-made function
            return camera.RenderTexture_GUIPointToRay(CameraEx.ScreenToGUIPoint(RTInput.get.pointingInputDevice.position), viewRect);
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: RenderAxisLabel() (Private Function)
        // Desc: Renders the label for the specified axis handle.
        // Parm: axisCap    - The axis handle.
        //       camera     - The render camera.
        //       rasterCtx  - The raster graph context.
        //-----------------------------------------------------------------------------
        void RenderAxisLabel(GizmoCap axisCap, Camera camera, RasterGraphContext rasterCtx)
        {
            // Cap not visible?
            if (!axisCap.canRender)
                return;

            // Get axis
            int axisIndex = (int)axisCap.tag;
            Vector3 axis  = mGizmoAxes[axisIndex];

            // Set material properties
            mMtrlPropertyBlock.Clear();
            mMtrlPropertyBlock.SetColor("_Color", activeStyle.axisLabelColor.WithAlpha(activeStyle.axisLabelColor.a * axisCap.alphaScale));
            mMtrlPropertyBlock.SetTexture("_MainTex", TextureManager.GetGizmoAxisLabel(axisIndex));

            // Calculate the label position. This is simply the cap position due to the way in which
            // they are rotated (i.e. they point towards the origin and the base is where the axis ends).
            Vector3 labelPos = axisCap.transform.position;
            float labelSize = CalculateAxisLabelSize(camera, labelPos);
            labelPos += axis * labelSize / 2.0f;

            // We need to offset the label position so it doesn't overlap the axis handle. We have to
            // do this in GUI space. Convert the current label position and the axis origin to
            // GUI space and offset the label in GUI space using this vector. Then convert back to
            // world space.
            Rect viewRect = CalculateGizmoGUIRect();
            Vector2 o = camera.RenderTexture_WorldToGUIPoint(mCenterCap.transform.position, viewRect); // Axis origin in GUI space
            Vector2 p = camera.RenderTexture_WorldToGUIPoint(labelPos, viewRect);                   // Label position in GUI space
            Vector2 v = (p - o).normalized;                                                         // Axis direction in GUI space
            
            // Offset label position in GUI space to avoid overlap with the axis handle when the axis is aligned with the view vector
            // and then convert this new point to the label world position.
            p += v * activeStyle.axisLabelSize * Vector3Ex.AbsDot(camera.transform.forward, axis);
            labelPos = camera.RenderTexture_GUIToWorldPoint(new Vector3(p.x, p.y, 
                Vector3.Dot(labelPos - camera.transform.position, camera.transform.forward)), viewRect);

            // Draw
            rasterCtx.cmd.DrawMesh(MeshManager.unitXYQuad,
                Matrix4x4.TRS(labelPos, 
                camera.transform.rotation, 
                Vector3Ex.FromValue(labelSize)), MaterialManager.rtUnlit, 0, 0, mMtrlPropertyBlock);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculatePModeLabelGUIRect() (Private Function)
        // Desc: Calculates and returns the GUI space rectangle of the projection mode
        //       switch label.
        // Parm: gizmoGUIRect - The GUI rectangle where the view gizmo is rendered.
        //       labelTexture - The label texture.
        // Rtrn: The GUI space rectangle of the projection mode switch label.
        //-----------------------------------------------------------------------------
        Rect CalculatePModeLabelGUIRect(Rect gizmoGUIRect, Texture2D labelTexture)
        {
            return new Rect(gizmoGUIRect.center.x - labelTexture.width / 2.0f, gizmoGUIRect.yMax, labelTexture.width, labelTexture.height);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateGizmoGUIRect() (Private Function)
        // Desc: Calculates and returns the coordinates of GUI rectangle where the gizmo
        //       is displayed.
        // Rtrn: A rectangle in GUI space where the gizmo can be displayed.
        //-----------------------------------------------------------------------------
        Rect CalculateGizmoGUIRect()
        {
            // Identify the cam mode label texture with the maximum height. When placing
            // the gizmo at the bottom we need to offset by this height to make room for
            // the label.
            float camModeHeight = TextureManager.orthoCamModeIcon.height;
            if (TextureManager.perspectiveCamModeIcon.height > camModeHeight) 
                camModeHeight = TextureManager.perspectiveCamModeIcon.height;

            // Calculate the screen space rectangle of the camera target texture that contains the gizmo.
            // The texture is drawn inside the view camera's viewport.;
            Rect viewRect = mViewCamera.pixelRect.ScreenToGUIRect();
            switch (activeStyle.alignment)
            {
                case EViewGizmoAlignment.TopLeft:

                    return new Rect(viewRect.xMin + activeStyle.screenPadding.x, viewRect.yMin + activeStyle.screenPadding.y, 
                        activeStyle.screenSize, activeStyle.screenSize);

                case EViewGizmoAlignment.TopRight:

                    return new Rect(viewRect.xMax - 1.0f - activeStyle.screenSize - activeStyle.screenPadding.x,
                        viewRect.yMin + activeStyle.screenPadding.y, activeStyle.screenSize, activeStyle.screenSize);

                case EViewGizmoAlignment.BottomLeft:

                    return new Rect(viewRect.xMin + activeStyle.screenPadding.x,
                        viewRect.yMax - 1.0f - activeStyle.screenSize - activeStyle.screenPadding.y - camModeHeight, activeStyle.screenSize, activeStyle.screenSize);

                case EViewGizmoAlignment.BottomRight:

                    return new Rect(viewRect.xMax - 1.0f - activeStyle.screenSize - activeStyle.screenPadding.x,
                        viewRect.yMax - 1.0f - activeStyle.screenSize - activeStyle.screenPadding.y - camModeHeight, activeStyle.screenSize, activeStyle.screenSize);

                default: return new Rect();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateSphere() (Private Function)
        // Desc: Calculates and returns the sphere that encloses the gizmo.
        // Parm: camera - The camera that interacts with or renders the gizmo.
        // Rtrn: The enclosing sphere.
        //-----------------------------------------------------------------------------
        Sphere CalculateSphere(Camera camera)
        {
            // Calculate the sphere radius
            float halfMidSize = activeStyle.GetCenterCapSize() * 0.5f;
            float radius = Mathf.Sqrt(3.0f * halfMidSize * halfMidSize);    // The size of the mid cap extent vector
            radius += activeStyle.GetAxisCapLength();
            radius += CalculateAxisLabelSize(camera, mCenterCap.transform.position);

            // Return sphere
            return new Sphere(transform.position, radius);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAxisLabelSize() (Private Function)
        // Desc: Calculates the axis label world size for a label that is positioned at 
        //       'labelWorldPos'.
        // Parm: camera         - The camera that interacts with or renders the gizmo.
        //       labelWorldPos  - The label world position.
        // Rtrn: The axis label world size.
        //-----------------------------------------------------------------------------
        float CalculateAxisLabelSize(Camera camera, Vector3 labelWorldPos)
        {
            // Note: Use a size of 'activeStyle.axisLabelSize' for a screen size equal to 'ViewGizmoStyle.defaultScreenSize' and scale from there.
            return camera.ScreenToWorldSize(labelWorldPos, activeStyle.scale * activeStyle.axisLabelSize * activeStyle.screenSize / ViewGizmoStyle.defaultScreenSize);
        }

        //-----------------------------------------------------------------------------
        // Name: SwitchProjection() (Private Function)
        // Desc: Performs a projection switch for the active camera and the view gizmo's
        //       render camera.
        // Rtrn: True if the projection switch can start and false otherwise.
        //-----------------------------------------------------------------------------
        bool SwitchProjection()
        {
            // Perform the projection switch on the active camera.
            // Note: We only do this if the active camera is the view camera attached to this gizmo. This
            //       is important when multiple viewports are used, each viewport with its own camera and
            //       view gizmo. We only want to affect the camera which is currently used to navigate the
            //       scene.
            if (mViewCamera == RTCamera.get.settings.targetCamera)
            {
                // Do the switch on the active camera
                return RTCamera.get.SwitchProjection();
            }

            // Can't start projection switch
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: SwitchRotation() (Private Function)
        // Desc: Performs a rotation switch for the active camera and the view gizmo's
        //       render camera.
        // Parm: axisCap - The axis cap which controls the new direction of the camera
        //                 view vector. For example, if this is the positive X axis cap,
        //                 the new camera look vector will be set to <-1, 0, 0>.
        //-----------------------------------------------------------------------------
        void SwitchRotation(GizmoCap axisCap)
        {
            // Perform the rotation switch on the active camera.
            // Note: We only do this if the active camera is the view camera attached to this gizmo. This
            //       is important when multiple viewports are used, each viewport with its own camera and
            //       view gizmo. We only want to affect the camera which is currently used to navigate the
            //       scene.
            if (mViewCamera == RTCamera.get.settings.targetCamera)
            {
                // Check what axis we're dealing with and create the target rotation
                Quaternion targetRotation = Quaternion.identity;
                if (axisCap == mAxisCapsP[0])       targetRotation = Quaternion.LookRotation(-Vector3.right, Vector3.up);
                else if (axisCap == mAxisCapsN[0])  targetRotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
                else if (axisCap == mAxisCapsP[1])  targetRotation = Quaternion.LookRotation(-Vector3.up, Vector3.forward);
                else if (axisCap == mAxisCapsN[1])  targetRotation = Quaternion.LookRotation(Vector3.up, -Vector3.forward);
                else if (axisCap == mAxisCapsP[2])  targetRotation = Quaternion.LookRotation(-Vector3.forward, Vector3.up);
                else if (axisCap == mAxisCapsN[2])  targetRotation = Quaternion.identity;
                else return;

                // Switch
                RTCamera.get.SwitchRotation(targetRotation);
            }
        }
        #endregion
    }
    #endregion
}