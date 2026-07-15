using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ShaderManager (Public Static Class)
    // Desc: Provides shared shaders used throughout the plugin.
    //-----------------------------------------------------------------------------
    public static class ShaderManager
    {
        #region Private Static Fields
        static Shader mRTGizmoHandle;          // Gizmo handle shader
        static Shader mRTGridShader;           // Grid shader
        static Shader mRTCameraBg;             // Camera background shader
        static Shader mRTUnlit;                // Unlit shader
        static Shader mRT2D;                   // 2D rendering shader
        static Shader mRTOutlineSilhouette;    // Outline silhouette shader
        static Shader mRTOutlineEffect;        // Outline effect shader
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: rtGizmoHandle (Public Static Property)
        // Desc: Returns the shader which renders gizmo handles.
        //-----------------------------------------------------------------------------
        public static Shader rtGizmoHandle
        {
            get
            {
                if (mRTGizmoHandle == null) mRTGizmoHandle = Shader.Find("Hidden/RTG/RTGizmoHandle");
                return mRTGizmoHandle;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtGrid (Public Static Property)
        // Desc: Returns the shader which renders the 'RTGrid'.
        //-----------------------------------------------------------------------------
        public static Shader rtGrid
        {
            get
            {
                if (mRTGridShader == null) mRTGridShader = Shader.Find("Hidden/RTG/RTGrid");
                return mRTGridShader;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtCameraBg (Public Static Property)
        // Desc: Returns the shader which renders the 'RTCamera' background.
        //-----------------------------------------------------------------------------
        public static Shader rtCameraBg
        {
            get
            {
                if (mRTCameraBg == null) mRTCameraBg = Shader.Find("Hidden/RTG/RTCameraBg");
                return mRTCameraBg;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtUnlit (Public Static Property)
        // Desc: Returns the shader that renders unlit geometry.
        //-----------------------------------------------------------------------------
        public static Shader rtUnlit
        {
            get
            {
                if (mRTUnlit == null) mRTUnlit = Shader.Find("Hidden/RTG/RTUnlit");
                return mRTUnlit;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rt2D (Public Static Property)
        // Desc: Returns the shader that renders 2D geometry.
        //-----------------------------------------------------------------------------
        public static Shader rt2D
        {
            get
            {
                if (mRT2D == null) mRT2D = Shader.Find("Hidden/RTG/RT2D");
                return mRT2D;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtOutlineSilhouette (Public Static Property)
        // Desc: Returns the shader that renders object silhouettes which can be
        //       consumed by the outline effect.
        //-----------------------------------------------------------------------------
        public static Shader rtOutlineSilhouette
        {
            get
            {
                if (mRTOutlineSilhouette == null) mRTOutlineSilhouette = Shader.Find("Hidden/RTG/RTOutlineSilhouette");
                return mRTOutlineSilhouette;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtOutlineEffect (Public Static Property)
        // Desc: Returns the shader that renders outline effects.
        //-----------------------------------------------------------------------------
        public static Shader rtOutlineEffect
        {
            get
            {
                if (mRTOutlineEffect == null) mRTOutlineEffect = Shader.Find("Hidden/RTG/RTOutlineEffect");
                return mRTOutlineEffect;
            }
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Reset() (Public Static Function)
        // Desc: Resets all cached shader references.
        //-----------------------------------------------------------------------------
        public static void Internal_Reset()
        {
            // Reset shaders
            mRTGizmoHandle        = null;
            mRTGridShader         = null;
            mRTCameraBg           = null;
            mRTUnlit              = null;
            mRT2D                 = null;
            mRTOutlineSilhouette  = null;
            mRTOutlineEffect      = null;
        }
        #endregion
    }
    #endregion
}