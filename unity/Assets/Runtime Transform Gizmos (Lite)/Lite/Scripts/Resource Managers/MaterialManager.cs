using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: MaterialRenderStates (Public Struct)
    // Desc: Provides storage for a combination of material render states.
    //-----------------------------------------------------------------------------
    public struct MaterialRenderStates
    {
        public CompareFunction  zTest;      // Z test function
        public bool             zWrite;     // Z write toggle
        public CullMode         cullMode;   // Cull mode
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: MaterialManager (Public Static Class)
    // Desc: Provides shared materials used throughout the plugin and utility
    //       functions for creating material variants.
    //-----------------------------------------------------------------------------
    public static class MaterialManager
    {
        #region Private Static Fields
        static Dictionary<MaterialRenderStates, Material>
                        mRTGizmoHandleMaterialMap = new Dictionary<MaterialRenderStates, Material>();    // Maps gizmo handle material states to a gizmo handle material

        static Material mRTGrid;                       // Grid material
        static Material mRTCameraBg;                   // Camera background material
        static Material mRTUnlit;                      // Unlit material
        static Material mRT2D;                         // 2D rendering material

        static Material mRTOutlineSilhouette;          // Outline silhouette material
        static Material mRTOutlineSilhouette_Cutout;   // Outline silhouette material with alpha testing
        static Material mRTOutlineEffect;              // Outline effect material
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: rtGrid (Public Static Property)
        // Desc: Returns the material which renders the 'RTGrid'.
        //-----------------------------------------------------------------------------
        public static Material rtGrid
        {
            get
            {
                if (mRTGrid == null) mRTGrid = new Material(ShaderManager.rtGrid);
                return mRTGrid;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtCameraBg (Public Static Property)
        // Desc: Returns the material which renders the 'RTCamera' background.
        //-----------------------------------------------------------------------------
        public static Material rtCameraBg
        {
            get
            {
                if (mRTCameraBg == null) mRTCameraBg = new Material(ShaderManager.rtCameraBg);
                return mRTCameraBg;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtUnlit (Public Static Property)
        // Desc: Returns the material that renders unlit geometry.
        //-----------------------------------------------------------------------------
        public static Material rtUnlit
        {
            get
            {
                if (mRTUnlit == null) mRTUnlit = new Material(ShaderManager.rtUnlit);
                return mRTUnlit;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rt2D (Public Static Property)
        // Desc: Returns the material that renders 2D geometry.
        //-----------------------------------------------------------------------------
        public static Material rt2D
        {
            get
            {
                if (mRT2D == null)
                {
                    mRT2D = new Material(ShaderManager.rt2D);
                    mRT2D.DisableKeyword("RT_SAMPLE_TEXTURE");
                }

                return mRT2D;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtOutlineSilhouette (Public Static Property)
        // Desc: Returns the material that renders object silhouettes which can be
        //       consumed by the outline effect. This material is used for objects
        //       that are rendered with alpha-testing disabled.
        //-----------------------------------------------------------------------------
        public static Material rtOutlineSilhouette
        {
            get
            {
                if (mRTOutlineSilhouette == null)
                    mRTOutlineSilhouette = CreateOutlineSilhouetteMaterial(false);

                return mRTOutlineSilhouette;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtOutlineSilhouette_Cutout (Public Static Property)
        // Desc: Returns the material that renders object silhouettes which can be
        //       consumed by the outline effect. This material is used for objects
        //       that are rendered with alpha-testing enabled.
        //-----------------------------------------------------------------------------
        public static Material rtOutlineSilhouette_Cutout
        {
            get
            {
                if (mRTOutlineSilhouette_Cutout == null)
                    mRTOutlineSilhouette_Cutout = CreateOutlineSilhouetteMaterial(true);

                return mRTOutlineSilhouette_Cutout;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: rtOutlineEffect (Public Static Property)
        // Desc: Returns the material that renders outline effects.
        //-----------------------------------------------------------------------------
        public static Material rtOutlineEffect
        {
            get
            {
                if (mRTOutlineEffect == null) mRTOutlineEffect = new Material(ShaderManager.rtOutlineEffect);
                return mRTOutlineEffect;
            }
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: GetRTGizmoHandleMaterial() (Public Static Function)
        // Desc: Returns the gizmo handle material that uses the specified render states.
        // Parm: renderStates - Render state combo.
        // Rtrn: The material that uses the specified render state combo.
        //-----------------------------------------------------------------------------
        public static Material GetRTGizmoHandleMaterial(MaterialRenderStates renderStates)
        {
            // Do we have an entry for this state combo?
            Material material = null;
            if (mRTGizmoHandleMaterialMap.TryGetValue(renderStates, out material))
                return material;

            // Create a new entry
            material = new Material(ShaderManager.rtGizmoHandle);
            material.SetCullMode(renderStates.cullMode);
            material.SetZTest(renderStates.zTest);
            material.SetZWriteEnabled(renderStates.zWrite);

            // Store entry and return it
            mRTGizmoHandleMaterialMap.Add(renderStates, material);
            return material;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateOutlineSilhouetteMaterial() (Public Static Function)
        // Desc: Creates and returns a material used to render silhouettes for an
        //       object outline effect.
        // Parm: cutout - True if the material is needed for objects whose renderer
        //       used an alpha cutout shader.
        // Rtrn: The material used to render silhouettes for an object outline effect.
        //       The material pool doesn't manage the returned material so the client
        //       should destroy the material when no longer needed.
        //-----------------------------------------------------------------------------
        public static Material CreateOutlineSilhouetteMaterial(bool cutout)
        {
            // Create the material and enable/disable keywords
            Material material = new Material(ShaderManager.rtOutlineSilhouette);
            if (cutout) material.EnableKeyword("RT_ALPHA_TEST_ENABLED");
            else        material.DisableKeyword("RT_ALPHA_TEST_ENABLED");

            // Return material
            return material;
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Reset() (Public Static Function)
        // Desc: Resets all cached material references.
        //-----------------------------------------------------------------------------
        public static void Internal_Reset()
        {
            // Reset material map
            mRTGizmoHandleMaterialMap.Clear();

            // Reset materials
            mRTGrid                     = null;
            mRTCameraBg                 = null;
            mRTUnlit                    = null;
            mRT2D                       = null;
            mRTOutlineSilhouette        = null;
            mRTOutlineSilhouette_Cutout = null;
            mRTOutlineEffect            = null;
        }
        #endregion
    }
    #endregion
}