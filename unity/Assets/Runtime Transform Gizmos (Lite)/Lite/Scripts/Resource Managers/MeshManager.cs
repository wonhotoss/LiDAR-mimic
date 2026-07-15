using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: MeshManager (Public Static Class)
    // Desc: Provides shared meshes used throughout the plugin.
    // Defs: Unit Box           - box with width, height and depth = 1.
    //       Unit Quad          - quad with width and height = 1.
    //       Unit Cylinder      - cylinder with length and radius = 1.
    //       Unit Cone          - cone with length and radius = 1.
    //       Unit Segment       - segment with length = 1.
    //       Unit Sphere        - sphere with radius = 1.
    //       Unit Pyramid       - pyramid with a base size = 1 and height = 1.
    //       Unit Torus         - torus with a tube radius = 1 and radius = 1.
    //       Unit RATriangle    - right-angled triangle with both adjacent sides = 1.
    //-----------------------------------------------------------------------------
    public static class MeshManager
    {
        #region Private Static Fields
        static Mesh mUnitXYRATriangle;         // Unit XY right-angled triangle
        static Mesh mUnitWireXYRATriangle;     // Unit wire XY right-angled triangle

        static Mesh mUnitBox;                  // Unit box
        static Mesh mUnitWireBox;              // Unit wire box
        static Mesh mUnitBoxXSegment;          // Unit box segment aligned with the X axis
        static Mesh mUnitWireBoxXSegment;      // Unit wire box segment aligned with the X axis

        static Mesh mUnitCylinderXCap;         // Unit cylinder that caps the X axis
        static Mesh mUnitZCylinder;            // Unit cylinder whose length axis is aligned with the Z axis
        static Mesh mUnitConeXCap;             // Unit cone that caps the X axis

        static Mesh mUnitSphere;               // Unit sphere
        static Mesh mUnitZTorus;               // Unit torus whose main axis is aligned with the Z axis

        static Mesh mUnitSPyramidXCap;         // Unit square pyramid that caps the X axis
        static Mesh mUnitWireSPyramidXCap;     // Unit wire square pyramid that caps the X axis

        static Mesh mUnitXYCircle;             // Unit XY circle
        static Mesh mUnitWireXYCircle;         // Unit wire XY circle
        static Mesh mUnitWireYZCircle;         // Unit wire YZ circle
        static Mesh mUnitWireZXCircle;         // Unit wire ZX circle

        static Mesh mUnitXYQuad;               // Unit XY quad
        static Mesh mUnitWireXYQuad;           // Unit wire XY quad
        static Mesh mUnitZXQuad;               // Unit ZX quad
        static Mesh mSpriteQuad;               // Sprite quad
        static Mesh mBlitQuad;                 // Blit quad mesh used for blit operations

        static Mesh mUnitXSegment;             // Unit segment aligned with the X axis
        static Mesh mUnitYSegment;             // Unit segment aligned with the Y axis
        static Mesh mUnitZSegment;             // Unit segment aligned with the Z axis
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: unitXYRATriangle (Public Static Property)
        // Desc: Returns a unit right-angled triangle that sits in the XY plane.
        //-----------------------------------------------------------------------------
        public static Mesh unitXYRATriangle
        {
            get
            {
                if (mUnitXYRATriangle == null) 
                    mUnitXYRATriangle = MeshEx.CreateRATriangle(new RATriangleMeshDesc { trianglePlane = EFlatMeshPlane.XY, aeSize0 = 1.0f, aeSize1 = 1.0f, color = Color.white });

                return mUnitXYRATriangle;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitWireXYRATriangle (Public Static Property)
        // Desc: Returns a wire unit right-angled triangle that sits in the XY plane.
        //-----------------------------------------------------------------------------
        public static Mesh unitWireXYRATriangle
        {
            get
            {
                if (mUnitWireXYRATriangle == null) 
                    mUnitWireXYRATriangle = MeshEx.CreateWireRATriangle(new RATriangleMeshDesc { trianglePlane = EFlatMeshPlane.XY, aeSize0 = 1.0f, aeSize1 = 1.0f, color = Color.white });

                return mUnitWireXYRATriangle;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitBox (Public Static Property)
        // Desc: Returns a unit box mesh.
        //-----------------------------------------------------------------------------
        public static Mesh unitBox
        {
            get
            {
                if (mUnitBox == null) mUnitBox = MeshEx.CreateBox(new BoxMeshDesc { width = 1.0f, height = 1.0f, depth = 1.0f, color = Color.white });
                return mUnitBox;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitWireBox (Public Static Property)
        // Desc: Returns a unit wire box mesh.
        //-----------------------------------------------------------------------------
        public static Mesh unitWireBox
        {
            get
            {
                if (mUnitWireBox == null) mUnitWireBox = MeshEx.CreateWireBox(new BoxMeshDesc { width = 1.0f, height = 1.0f, depth = 1.0f, color = Color.white });
                return mUnitWireBox;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitBoxXSegment (Public Static Property)
        // Desc: Returns a unit box that can be used as a unit segment that starts
        //       from the origin and is aligned with the X axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitBoxXSegment
        {
            get
            {
                if (mUnitBoxXSegment == null) mUnitBoxXSegment = MeshEx.CreateBox(new BoxMeshDesc { width = 1.0f, height = 1.0f, depth = 1.0f, color = Color.white, center = new Vector3(0.5f, 0.0f, 0.0f) });
                return mUnitBoxXSegment;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitWireBoxXSegment (Public Static Property)
        // Desc: Returns a unit wire box that can be used as a unit segment that starts
        //       from the origin and is aligned with the X axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitWireBoxXSegment
        {
            get
            {
                if (mUnitWireBoxXSegment == null) mUnitWireBoxXSegment = MeshEx.CreateWireBox(new BoxMeshDesc { width = 1.0f, height = 1.0f, depth = 1.0f, color = Color.white, center = new Vector3(0.5f, 0.0f, 0.0f) });
                return mUnitWireBoxXSegment;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitCylinderXCap (Public Static Property)
        // Desc: Returns a unit cylinder that caps the X axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitCylinderXCap
        {
            get
            {
                if (mUnitCylinderXCap == null)
                {
                    // Fill descriptor
                    CylinderMeshDesc desc = new CylinderMeshDesc();
                    desc.length         = 1.0f;
                    desc.radius         = 1.0f;
                    desc.lengthAxis     = 0;
                    desc.sliceCount     = 30;
                    desc.stackCount     = 1;
                    desc.capRingCount0  = 1;
                    desc.capRingCount1  = 1;
                    desc.color          = Color.white;

                    // Create mesh
                    mUnitCylinderXCap = MeshEx.CreateCylinder(desc);
                }

                return mUnitCylinderXCap;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitZCylinder (Public Static Property)
        // Desc: Returns a unit cylinder whose length axis is aligned with the Z axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitZCylinder
        {
            get
            {
                if (mUnitZCylinder == null)
                {
                    // Fill descriptor
                    CylinderMeshDesc desc = new CylinderMeshDesc();
                    desc.length         = 1.0f;
                    desc.radius         = 1.0f;
                    desc.lengthAxis     = 2;
                    desc.sliceCount     = 30;
                    desc.stackCount     = 1;
                    desc.capRingCount0  = 1;
                    desc.capRingCount1  = 1;
                    desc.color          = Color.white;

                    // Create mesh
                    mUnitZCylinder = MeshEx.CreateCylinder(desc);
                }

                return mUnitZCylinder;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitConeXCap (Public Static Property)
        // Desc: Returns a unit cone that caps the X axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitConeXCap
        {
            get
            {
                if (mUnitConeXCap == null)
                {
                    // Fill descriptor
                    ConeMeshDesc desc = new ConeMeshDesc();
                    desc.length         = 1.0f;
                    desc.radius         = 1.0f;
                    desc.lengthAxis     = 0;
                    desc.sliceCount     = 20;
                    desc.stackCount     = 1;
                    desc.capRingCount   = 1;
                    desc.color          = Color.white;

                    // Create mesh
                    mUnitConeXCap = MeshEx.CreateCone(desc);
                }

                return mUnitConeXCap;
            }
        }
        
        //-----------------------------------------------------------------------------
        // Name: unitSphere (Public Static Property)
        // Desc: Returns a unit sphere.
        //-----------------------------------------------------------------------------
        public static Mesh unitSphere
        {
            get
            {
                if (mUnitSphere == null)
                {
                    // Fill descriptor
                    SphereMeshDesc desc = new SphereMeshDesc();
                    desc.radius         = 1.0f;
                    desc.sliceCount     = 30;
                    desc.stackCount     = 30;
                    desc.color          = Color.white;

                    // Create mesh
                    mUnitSphere = MeshEx.CreateSphere(desc);
                }

                return mUnitSphere;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitZTorus (Public Static Property)
        // Desc: Returns a unit torus whose main axis is aligned with the Z axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitZTorus
        {
            get
            {
                if (mUnitZTorus == null)
                {
                    // Fill descriptor
                    TorusMeshDesc desc      = new TorusMeshDesc();
                    desc.radius             = 1.0f;
                    desc.tubeRadius         = 1.0f;
                    desc.sliceCount         = 80;
                    desc.crossSliceCount    = 80;
                    desc.mainAxis           = 2;

                    // Create mesh
                    mUnitZTorus = MeshEx.CreateTorus(desc);
                }

                return mUnitZTorus;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitSPyramidXCap (Public Static Property)
        // Desc: Returns a unit square pyramid that caps the X axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitSPyramidXCap
        {
            get
            {
                if (mUnitSPyramidXCap == null)
                {
                    // Fill descriptor
                    SquarePyramidMeshDesc desc = new SquarePyramidMeshDesc();
                    desc.length         = 1.0f;
                    desc.baseSize       = 1.0f;
                    desc.lengthAxis     = 0;
                    desc.color          = Color.white;

                    // Create mesh
                    mUnitSPyramidXCap = MeshEx.CreateSquarePyramid(desc);
                }

                return mUnitSPyramidXCap;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitWireSPyramidXCap (Public Static Property)
        // Desc: Returns a unit wire square pyramid that caps the X axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitWireSPyramidXCap
        {
            get
            {
                if (mUnitWireSPyramidXCap == null)
                {
                    // Fill descriptor
                    SquarePyramidMeshDesc desc = new SquarePyramidMeshDesc();
                    desc.length         = 1.0f;
                    desc.baseSize       = 1.0f;
                    desc.lengthAxis     = 0;
                    desc.color          = Color.white;

                    // Create mesh
                    mUnitWireSPyramidXCap = MeshEx.CreateWireSquarePyramid(desc);
                }

                return mUnitWireSPyramidXCap;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitXYCircle (Public Static Property)
        // Desc: Returns a unit XY circle.
        //-----------------------------------------------------------------------------
        public static Mesh unitXYCircle
        {
            get
            {
                if (mUnitXYCircle == null) mUnitXYCircle = MeshEx.CreateCircle(new CircleMeshDesc { circlePlane = EFlatMeshPlane.XY, radius = 1.0f, sliceCount = 80, color = Color.white });
                return mUnitXYCircle;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitWireXYCircle (Public Static Property)
        // Desc: Returns a unit wire XY circle.
        //-----------------------------------------------------------------------------
        public static Mesh unitWireXYCircle
        {
            get
            {
                if (mUnitWireXYCircle == null) mUnitWireXYCircle = MeshEx.CreateWireCircle(new CircleMeshDesc { circlePlane = EFlatMeshPlane.XY, radius = 1.0f, sliceCount = 80, color = Color.white });
                return mUnitWireXYCircle;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitWireYZCircle (Public Static Property)
        // Desc: Returns a unit wire YZ circle.
        //-----------------------------------------------------------------------------
        public static Mesh unitWireYZCircle
        {
            get
            {
                if (mUnitWireYZCircle == null) mUnitWireYZCircle = MeshEx.CreateWireCircle(new CircleMeshDesc { circlePlane = EFlatMeshPlane.YZ, radius = 1.0f, sliceCount = 80, color = Color.white });
                return mUnitWireYZCircle;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitWireZXCircle (Public Static Property)
        // Desc: Returns a unit wire ZX circle.
        //-----------------------------------------------------------------------------
        public static Mesh unitWireZXCircle
        {
            get
            {
                if (mUnitWireZXCircle == null) mUnitWireZXCircle = MeshEx.CreateWireCircle(new CircleMeshDesc { circlePlane = EFlatMeshPlane.ZX, radius = 1.0f, sliceCount = 80, color = Color.white });
                return mUnitWireZXCircle;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitXYQuad (Public Static Property)
        // Desc: Returns a unit XY quad.
        //-----------------------------------------------------------------------------
        public static Mesh unitXYQuad
        {
            get
            {
                if (mUnitXYQuad == null) mUnitXYQuad = MeshEx.CreateQuad(new QuadMeshDesc { quadPlane = EFlatMeshPlane.XY, width = 1.0f, height = 1.0f, color = Color.white });
                return mUnitXYQuad;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitWireXYQuad (Public Static Property)
        // Desc: Returns a unit wire XY quad.
        //-----------------------------------------------------------------------------
        public static Mesh unitWireXYQuad
        {
            get
            {
                if (mUnitWireXYQuad == null) mUnitWireXYQuad = MeshEx.CreateWireQuad(new QuadMeshDesc { quadPlane = EFlatMeshPlane.XY, width = 1.0f, height = 1.0f, color = Color.white });
                return mUnitWireXYQuad;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitZXQuad (Public Static Property)
        // Desc: Returns a unit ZX quad.
        //-----------------------------------------------------------------------------
        public static Mesh unitZXQuad
        {
            get 
            {
                if (mUnitZXQuad == null) mUnitZXQuad = MeshEx.CreateQuad(new QuadMeshDesc(){ quadPlane = EFlatMeshPlane.ZX, width = 1.0f, height = 1.0f, color = Color.white });
                return mUnitZXQuad;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: spriteQuad (Public Static Property)
        // Desc: Returns a mesh that can be used to render sprite quads.
        //-----------------------------------------------------------------------------
        public static Mesh spriteQuad
        {
            get
            {
                if (mSpriteQuad == null) mSpriteQuad = MeshEx.CreateQuad(new QuadMeshDesc(){ quadPlane = EFlatMeshPlane.XY, width = 1.0f, height = 1.0f, color = Color.white });
                return mSpriteQuad;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: blitQuad (Public Static Property)
        // Desc: Returns a mesh that can be used to perform blit operations. This is
        //       a quad whose vertices cover the entire viewport area.
        //-----------------------------------------------------------------------------
        public static Mesh blitQuad
        {
            get
            {
                if (mBlitQuad == null) mBlitQuad = MeshEx.CreateBlitQuad();
                return mBlitQuad;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitXSegment (Public Static Property)
        // Desc: Returns a unit length segment aligned with the X axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitXSegment
        {
            get
            {
                if (mUnitXSegment == null) mUnitXSegment = MeshEx.CreateSegment(Vector3.zero, Vector3.right, Color.white);
                return mUnitXSegment;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitYSegment (Public Static Property)
        // Desc: Returns a unit length segment aligned with the Y axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitYSegment
        {
            get
            {
                if (mUnitYSegment == null) mUnitYSegment = MeshEx.CreateSegment(Vector3.zero, Vector3.up, Color.white);
                return mUnitYSegment;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: unitZSegment (Public Static Property)
        // Desc: Returns a unit length segment aligned with the Z axis.
        //-----------------------------------------------------------------------------
        public static Mesh unitZSegment
        {
            get
            {
                if (mUnitZSegment == null) mUnitZSegment = MeshEx.CreateSegment(Vector3.zero, Vector3.forward, Color.white);
                return mUnitZSegment;
            }
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Reset() (Public Static Function)
        // Desc: Resets all cached mesh references.
        //-----------------------------------------------------------------------------
        public static void Internal_Reset()
        {
            // Reset triangle meshes
            mUnitXYRATriangle      = null;
            mUnitWireXYRATriangle  = null;

            // Reset box meshes
            mUnitBox               = null;
            mUnitWireBox           = null;
            mUnitBoxXSegment       = null;
            mUnitWireBoxXSegment   = null;

            // Reset cylinder and cone meshes
            mUnitCylinderXCap      = null;
            mUnitZCylinder         = null;
            mUnitConeXCap          = null;

            // Reset sphere and torus meshes
            mUnitSphere            = null;
            mUnitZTorus            = null;

            // Reset pyramid meshes
            mUnitSPyramidXCap      = null;
            mUnitWireSPyramidXCap  = null;

            // Reset circle meshes
            mUnitXYCircle          = null;
            mUnitWireXYCircle      = null;
            mUnitWireYZCircle      = null;
            mUnitWireZXCircle      = null;

            // Reset quad meshes
            mUnitXYQuad            = null;
            mUnitWireXYQuad        = null;
            mUnitZXQuad            = null;
            mSpriteQuad            = null;
            mBlitQuad              = null;

            // Reset segment meshes
            mUnitXSegment          = null;
            mUnitYSegment          = null;
            mUnitZSegment          = null;
        }
        #endregion
    }
    #endregion
}