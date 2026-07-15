using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGameObjectType (Public Flags Enum)
    // Desc: Defines different object types that can be used as flags.
    //-----------------------------------------------------------------------------
    [Flags] public enum EGameObjectType
    {
        None            = 0,
        Mesh            = 1,
        Terrain         = 2,
        Sprite          = 4,
        Camera          = 8,
        Light           = 16,
        ParticleSystem  = 32,
        Empty           = 64,
        Other           = 128,
        All             = ~0
    }
    #endregion

    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: BoundsQueryConfig (Public Struct)
    // Desc: Stores information for a bounds query. This is used to configure the
    //       way in which bounds are calculated for objects and/or object hierarchies.
    //-----------------------------------------------------------------------------
    public struct BoundsQueryConfig
    {
        #region Public Fields
        public EGameObjectType  objectTypes;                    // Object type mask used to configure which object types are taken into account and which are ignored 
        public bool             includeInactive;                // Should inactive objects be included?
        public bool             includeInvisible;               // Should invisible objects be included?
        public bool             includeAddedObjectOverrides;    // Should added object overrides be included? (Unity Editor only)
        public float            directionalLightSize;           // Volume size for directional lights
        #endregion

        #region Public Static Readonly Fields
        public static readonly BoundsQueryConfig defaultConfig = new BoundsQueryConfig()    // Default bounds query config
        {
            objectTypes                 = EGameObjectType.All,  // Accept all object types
            includeInactive             = false,                // Ignore inactive objects
            includeInvisible            = false,                // Ignore invisible objects
            includeAddedObjectOverrides = true,                 // Allow added object overrides
            directionalLightSize        = 1.0f                  // Use a size of 1 for directional lights
        };
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GameObjectEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'GameObject' class.
    //-----------------------------------------------------------------------------
    public static class GameObjectEx
    {
        #region Private Static Fields
        // Note: The following buffers are used to optimize 'GetComponent'.
        static List<Camera>                         sCameraBuffer               = new List<Camera>();
        static List<SpriteRenderer>                 sSpriteRendererBuffer       = new List<SpriteRenderer>();
        static List<Renderer>                       sRendererBuffer             = new List<Renderer>();
        static List<MeshFilter>                     sMeshFilterBuffer           = new List<MeshFilter>();
        static List<Terrain>                        sTerrainBuffer              = new List<Terrain>();
        static List<TerrainCollider>                sTerrainColliderBuffer      = new List<TerrainCollider>();
        static List<BoxCollider>                    sBoxColliderBuffer          = new List<BoxCollider>();
        static List<BoxCollider2D>                  sBoxCollider2DBuffer        = new List<BoxCollider2D>();
        static List<SphereCollider>                 sSphereColliderBuffer       = new List<SphereCollider>();
        static List<CircleCollider2D>               sCircleColliderBuffer       = new List<CircleCollider2D>();
        static List<CapsuleCollider>                sCapsuleColliderBuffer      = new List<CapsuleCollider>();
        static List<CapsuleCollider2D>              sCapsuleCollider2DBuffer    = new List<CapsuleCollider2D>();
        static List<CharacterController>            sCharacterControllerBuffer  = new List<CharacterController>();
        static List<AudioReverbZone>                sAudioReverbZoneBuffer      = new List<AudioReverbZone>();
        static List<AudioSource>                    sAudioSourceBuffer          = new List<AudioSource>();
        static List<MeshRenderer>                   sMeshRendererBuffer         = new List<MeshRenderer>();
        static List<SkinnedMeshRenderer>            sSkinnedMeshRendererBuffer  = new List<SkinnedMeshRenderer>();
        static List<Light>                          sLightBuffer                = new List<Light>();
        static List<ParticleSystem>                 sParticleSystemBuffer       = new List<ParticleSystem>();
        static List<Tree>                           sTreeBuffer                 = new List<Tree>();
        static List<LODGroup>                       sLODGroupBuffer             = new List<LODGroup>();
        static List<Component>                      sComponentBuffer            = new List<Component>();
        static List<RTG>                            sPluginBuffer               = new List<RTG>();
        static List<IRTObjectTransformGizmoTarget>  sRTOTGTBuffer               = new List<IRTObjectTransformGizmoTarget>();

        // Buffers used to avoid memory allocations.
        static List<Transform>                      sTransformBuffer            = new List<Transform>();
        static List<GameObject>                     sParentObjectBuffer         = new List<GameObject>(128);
        static List<GameObject>                     sChildObjectBuffer          = new List<GameObject>(128);
        static List<GameObject>                     sObjectBuffer               = new List<GameObject>(128);
        static List<Vector3>                        sDOPVertexBuffer            = new List<Vector3>();
        static List<Vector3>                        sScreenPointBuffer          = new List<Vector3>();    

        // Maps a game objects to its type
        static Dictionary<GameObject, EGameObjectType> sObjectTypeMap           = new Dictionary<GameObject, EGameObjectType>();
        #endregion

        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: CalculateScreenRect() (Public Extension)
        // Desc: Calculates and returns the object's screen rectangle.
        // Parm: queryConfig - Bounds query configuration.
        //       camera      - The camera which sees the object.
        //       rect        - Returns the object's screen rectangle.
        // Rtrn: True on success and false on failure.
        //-----------------------------------------------------------------------------
        public static bool CalculateScreenRect(this GameObject gameObject, Camera camera, BoundsQueryConfig queryConfig, out Rect rect)
        {
            // Return empty rect by default
            rect = new Rect(0.0f, 0.0f, 0.0f, 0.0f);

            // Inactive objects not allowed?
            if (!queryConfig.includeInactive && !gameObject.activeInHierarchy)
                return false;

            // Invisible objects not allowed?
            if (!queryConfig.includeInvisible && !gameObject.IsVisible())
                return false;

            // If this is a mesh, we will use the DOP for a better approximation
            if (gameObject.GetGameObjectType() == EGameObjectType.Mesh)
            {
                // Get DOP verts in world space
                Mesh mesh = queryConfig.includeInactive ? gameObject.GetMesh() : gameObject.GetEnabledMesh();
                RTMesh rtMesh = RTMeshManager.GetRTMesh(mesh);
                if (rtMesh == null) return false;
                rtMesh.kdop.CollectVerts(gameObject.transform.localToWorldMatrix, sDOPVertexBuffer);

                // Convert world verts to screen verts and create rectangle
                camera.WorldToScreenPoints(sDOPVertexBuffer, sScreenPointBuffer);
                rect = RectEx.FromPoints(sScreenPointBuffer);
                if (!rect.HasPositiveSize()) return false;
            }
            else
            {
                // Calculate world OBB and return its screen rectangle
                OBox worldOBB = gameObject.CalculateWorldOBB(queryConfig);
                if (!worldOBB.isValid) return false;
                rect = worldOBB.CalculateScreenRect(camera);
                if (!rect.HasPositiveSize()) return false;
            }

            // Success!
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateSpriteWorldOBB() (Public Extension)
        // Desc: Calculates and returns the world OBB of a sprite object.
        // Rtrn: The sprite world OBB or an invalid OBB if not a valid sprite object.
        //-----------------------------------------------------------------------------
        public static OBox CalculateSpriteWorldOBB(this GameObject gameObject)
        {
            // Calculate model AABB
            Box modelAABB = gameObject.CalculateSpriteModelAABB();
            if (!modelAABB.isValid) return OBox.GetInvalid();

            // Return world OBB
            return new OBox(modelAABB, gameObject.transform);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateSpriteWorldAABB() (Public Extension)
        // Desc: Calculates and returns the world AABB of a sprite object.
        // Rtrn: The sprite world AABB or an invalid AABB if not a valid sprite object.
        //-----------------------------------------------------------------------------
        public static Box CalculateSpriteWorldAABB(this GameObject gameObject)
        {
            // Calculate model AABB
            Box modelAABB = gameObject.CalculateSpriteModelAABB();
            if (!modelAABB.isValid) return modelAABB;

            // Return world AABB
            modelAABB.Transform(gameObject.transform.localToWorldMatrix);
            return modelAABB;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateSpriteModelAABB() (Public Extension)
        // Desc: Calculates and returns the model AABB of a sprite object.
        // Rtrn: The sprite model AABB or an invalid AABB if not a valid sprite object.
        //-----------------------------------------------------------------------------
        public static Box CalculateSpriteModelAABB(this GameObject spriteObject)
        {
            // Calculate model AABB
            SpriteRenderer spriteRenderer = spriteObject.GetSpriteRenderer();
            if (spriteRenderer == null) return Box.GetInvalid();

            // Return model AABB
            return spriteRenderer.CalculateModelAABB();
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateMeshWorldOBB() (Public Extension)
        // Desc: Calculates and returns the world OBB of a mesh object.
        // Rtrn: The mesh world OBB or an invalid OBB if not a valid mesh object.
        //-----------------------------------------------------------------------------
        public static OBox CalculateMeshWorldOBB(this GameObject gameObject)
        {
            // Calculate model AABB
            Box modelAABB = gameObject.CalculateMeshModelAABB();
            if (!modelAABB.isValid) return OBox.GetInvalid();

            // Return world OBB
            return new OBox(modelAABB, gameObject.transform);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateMeshWorldAABB() (Public Extension)
        // Desc: Calculates and returns the world AABB of a mesh object.
        // Rtrn: The mesh world AABB or an invalid AABB if not a valid mesh object.
        //-----------------------------------------------------------------------------
        public static Box CalculateMeshWorldAABB(this GameObject gameObject)
        {
            // Calculate model AABB
            Box modelAABB = gameObject.CalculateMeshModelAABB();
            if (!modelAABB.isValid) return modelAABB;

            // Return world AABB
            modelAABB.Transform(gameObject.transform.localToWorldMatrix);
            return modelAABB;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateMeshModelAABB() (Public Extension)
        // Desc: Calculates and returns the model AABB of a mesh object.
        // Rtrn: The mesh model AABB or an invalid AABB if not a valid mesh object.
        //-----------------------------------------------------------------------------
        public static Box CalculateMeshModelAABB(this GameObject gameObject)
        {
            // Calculate model AABB
            Mesh mesh = gameObject.GetMesh();
            if (mesh == null) return Box.GetInvalid();

            // Return model AABB
            return new Box(mesh.bounds);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateHierarchyWorldOBB() (Public Extension)
        // Desc: Calculates and returns the world OBB of the object's hierarchy.
        // Parm: queryConfig - Bounds query configuration.
        // Rtrn: The world OBB or an invalid OBB if something goes wrong.
        //-----------------------------------------------------------------------------
        public static OBox CalculateHierarchyWorldOBB(this GameObject parent, BoundsQueryConfig queryConfig)
        {
            // Calculate model AABB
            Box modelAABB = parent.CalculateHierarchyModelAABB(queryConfig);
            if (!modelAABB.isValid) return OBox.GetInvalid();

            // Return world OBB.
            // Note: If the hierarchy root is a terrain, we don't want rotation.
            if (parent.GetTerrain() != null) return new OBox(modelAABB.center + parent.transform.position, modelAABB.size);
            else return new OBox(modelAABB, parent.transform);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateHierarchyWorldAABB() (Public Extension)
        // Desc: Calculates and returns the world AABB of the object's hierarchy.
        // Parm: queryConfig - Bounds query configuration.
        // Rtrn: The world AABB or an invalid AABB if something goes wrong.
        //-----------------------------------------------------------------------------
        public static Box CalculateHierarchyWorldAABB(this GameObject parent, BoundsQueryConfig queryConfig)
        {
            // Calculate model AABB
            Box modelAABB = parent.CalculateHierarchyModelAABB(queryConfig);
            if (!modelAABB.isValid) return Box.GetInvalid();

            // Return world AABB
            // Note: If the hierarchy root is a terrain, we don't want rotation.
            if (parent.GetTerrain() != null)
            {
                modelAABB.center += parent.transform.position;
                return modelAABB;
            }
            else
            {
                modelAABB.Transform(parent.transform.localToWorldMatrix);
                return modelAABB;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateWorldOBB() (Public Extension)
        // Desc: Calculates and returns the object's world OBB.
        // Parm: queryConfig - Bounds query configuration.
        // Rtrn: The object's world OBB or an invalid OBB if something goes wrong.
        //-----------------------------------------------------------------------------
        public static OBox CalculateWorldOBB(this GameObject gameObject, BoundsQueryConfig queryConfig)
        {
            // Calculate object model AABB
            EGameObjectType objectType = gameObject.GetGameObjectType();
            Box modelAABB = gameObject.CalculateModelAABB(objectType, queryConfig);
            if (!modelAABB.isValid) return OBox.GetInvalid();

            // Return world OBB.
            // Note: If the object is a terrain, we don't want rotation.
            if (objectType == EGameObjectType.Terrain) return new OBox(modelAABB.center + gameObject.transform.position, modelAABB.size);
            else return new OBox(modelAABB, gameObject.transform);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateWorldAABB() (Public Extension)
        // Desc: Calculates and returns the object's world AABB.
        // Parm: queryConfig - Bounds query configuration.
        // Rtrn: The object's world AABB or an invalid AABB if something goes wrong.
        //-----------------------------------------------------------------------------
        public static Box CalculateWorldAABB(this GameObject gameObject, BoundsQueryConfig queryConfig)
        {
            // If this is a skinned mesh renderer, we will use its 'bounds' property
            var skinnedRenderer = gameObject.GetSkinnedMeshRenderer();
            if (skinnedRenderer != null)
            {
                // Validate object states
                if ((!queryConfig.includeInactive && !skinnedRenderer.enabled) ||
                    (!queryConfig.includeInactive && !gameObject.activeInHierarchy))
                    return Box.GetInvalid();

                // Return the OBB
                return new Box(skinnedRenderer.bounds);
            }

            // Calculate object model AABB
            EGameObjectType objectType = gameObject.GetGameObjectType();
            Box modelAABB = gameObject.CalculateModelAABB(objectType, queryConfig);
            if (!modelAABB.isValid) return modelAABB;

            // Note: If the object is a terrain, we don't want rotation.
            if (objectType == EGameObjectType.Terrain)
            {
                modelAABB.center += gameObject.transform.position;
                return modelAABB;
            }
            else
            {
                modelAABB.Transform(gameObject.transform.localToWorldMatrix);
                return modelAABB;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateModelAABB() (Public Extension)
        // Desc: Calculates and returns the object's model AABB.
        // Parm: objectType  - The game object type.
        //       queryConfig - Bounds query configuration.
        // Rtrn: The object's model AABB or an invalid AABB if something goes wrong.
        //-----------------------------------------------------------------------------
        public static Box CalculateModelAABB(this GameObject gameObject, EGameObjectType objectType, BoundsQueryConfig queryConfig)
        {
            // If this is a scene object, we reject it if inactive objects are not allowed and the object is inactive
            if (gameObject.IsSceneObject() && !gameObject.activeInHierarchy && !queryConfig.includeInactive) return Box.GetInvalid();

            // We also reject the object if it's type is not allowed
            if ((objectType & queryConfig.objectTypes) == 0) 
                return Box.GetInvalid();

            // Reject invisible objects if necessary
            if (!queryConfig.includeInvisible && !gameObject.IsVisible())
                return Box.GetInvalid();

            // Continue based on the object's type
            if (objectType == EGameObjectType.Mesh)
            {
                // Check for a mesh filter
                MeshFilter meshFilter = gameObject.GetMeshFilter();
                if (meshFilter != null)
                {
                    // We have a mesh filter, check for a mesh
                    Mesh mesh = meshFilter.sharedMesh;
                    if (mesh != null)
                    {
                        // We have a mesh. Do we allow invisible objects?
                        if (!queryConfig.includeInactive)
                        {
                            // Invisible objects not allowed. If the mesh renderer is disabled, return an invalid AABB
                            MeshRenderer meshRenderer = gameObject.GetMeshRenderer();
                            if (meshRenderer == null || !meshRenderer.enabled) return Box.GetInvalid();
                        }

                        // We have a valid AABB
                        Box aabb = new Box(mesh.bounds);
                        aabb.size = aabb.size.FixFloatError();
                        return aabb;
                    }
                }
                else
                {
                    // Check for a skinned mesh renderer
                    SkinnedMeshRenderer skinnedRenderer = gameObject.GetSkinnedMeshRenderer();
                    if (skinnedRenderer != null)
                    {
                        // Check if the renderer has a mesh
                        Mesh mesh = skinnedRenderer.sharedMesh;
                        if (mesh != null)
                        {
                            // If the invisible objects are not allowed and the renderer is disabled, return an invalid AABB
                            if (!queryConfig.includeInactive && !skinnedRenderer.enabled) return Box.GetInvalid();

                            // We have a valid AABB
                            Box aabb = new Box(mesh.bounds);
                            aabb.size = aabb.size.FixFloatError();
                            return aabb;
                        }
                    }
                }

                // Nothing applies?
                return Box.GetInvalid();
            }
            else
            if (objectType == EGameObjectType.Sprite)
            {
                // Return the sprite renderer's AABB
                SpriteRenderer spriteRenderer = gameObject.GetSpriteRenderer();
                if (!queryConfig.includeInactive && !spriteRenderer.enabled) return Box.GetInvalid();
                return spriteRenderer.CalculateModelAABB();
            }
            else if (objectType == EGameObjectType.Terrain)
            {
                // Return the terrain's AABB
                Terrain terrain = gameObject.GetTerrain();
                if (!queryConfig.includeInactive && !terrain.enabled) return Box.GetInvalid();
                return terrain.CalculateModelAABB();
            }
            else if (objectType == EGameObjectType.Light)
            {
                // Point light?
                Light light = gameObject.GetLight();
                if (!queryConfig.includeInactive && !light.enabled) return Box.GetInvalid();
                if (light.type == LightType.Point)      return new Box(Vector3.zero, Vector3Ex.FromValue(light.range * 2.0f));
                else if (light.type == LightType.Spot)  return new Box(Vector3.zero, Vector3Ex.FromValue(light.range * 2.0f));
                else                                    return new Box(Vector3.zero, Vector3Ex.FromValue(queryConfig.directionalLightSize));
            }
            else
            if (objectType == EGameObjectType.Other)
            {
                // Box collider?
                BoxCollider boxCollider = gameObject.GetBoxCollider();
                if (boxCollider != null && (queryConfig.includeInactive || boxCollider.enabled))
                    return new Box(boxCollider.center, boxCollider.size);

                // Sphere collider?
                SphereCollider sphereCollider = gameObject.GetSphereCollider();
                if (sphereCollider != null && (queryConfig.includeInactive || sphereCollider.enabled)) 
                    return new Box(sphereCollider.center, Vector3Ex.FromValue(sphereCollider.radius * 2.0f));
                
                // Capsule collider?
                CapsuleCollider capsuleCollider = gameObject.GetCapsuleCollider();
                if (capsuleCollider != null && (queryConfig.includeInactive || capsuleCollider.enabled))
                {
                    // Assume Y direction by default and change if necessary
                    Vector3 size = new Vector3(capsuleCollider.radius * 2.0f, capsuleCollider.height, capsuleCollider.radius * 2.0f);
                    if (capsuleCollider.direction == 0)         size = new Vector3(capsuleCollider.height, capsuleCollider.radius * 2.0f, capsuleCollider.radius * 2.0f);
                    else if (capsuleCollider.direction == 2)    size = new Vector3(capsuleCollider.radius * 2.0f, capsuleCollider.radius * 2.0f, capsuleCollider.height);

                    // Calculate and return capsule collider model space box
                    return new Box(capsuleCollider.center, size);
                }

                // Character controller?
                CharacterController characterController = gameObject.GetCharacterController();
                if (characterController != null && (queryConfig.includeInactive || characterController.enabled))
                    return new Box(characterController.center, Vector3Ex.FromValue(characterController.height));

                // 2D box collider?
                BoxCollider2D boxCollider2D = gameObject.GetBoxCollider2D();
                if (boxCollider2D != null && (queryConfig.includeInactive || boxCollider2D.enabled))
                    return new Box(boxCollider2D.offset, boxCollider2D.size);

                // 2D circle collider?
                CircleCollider2D circleCollider2D = gameObject.GetCircleCollider2D();
                if (circleCollider2D != null && (queryConfig.includeInactive || circleCollider2D.enabled))
                    return new Box(circleCollider2D.offset, Vector3Ex.FromValue(circleCollider2D.radius * 2.0f));

                // 2D capsule collider?
                CapsuleCollider2D capsuleCollider2D = gameObject.GetCapsuleCollider2D();
                if (capsuleCollider2D != null && (queryConfig.includeInactive || capsuleCollider2D.enabled))
                    return new Box(capsuleCollider2D.offset, capsuleCollider2D.size);

                // Audio reverb zone?
                AudioReverbZone audioRZ = gameObject.GetAudioReverbZone();
                if (audioRZ != null && (queryConfig.includeInactive || audioRZ.enabled))
                    return new Box(Vector3.zero, Vector3Ex.FromValue(audioRZ.maxDistance * 2.0f));

                // Unknown object type
                return new Box(Vector3.zero, Vector3.zero);
            }
            else
            {
                // We are dealing with an object with no volume
                return new Box(Vector3.zero, Vector3.zero);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateHierarchyModelAABB() (Public Extension)
        // Desc: Calculates and returns the object hierarchy's model AABB.
        // Parm: queryConfig - Bounds query configuration.
        // Rtrn: The object hierarchy's model AABB or an invalid AABB if something goes wrong.
        //-----------------------------------------------------------------------------
        public static Box CalculateHierarchyModelAABB(this GameObject parent, BoundsQueryConfig queryConfig)
        {
            // Store root transform for easy access
            Matrix4x4 rootMtx = parent.transform.localToWorldMatrix;

            // Initialize the final AABB with the model AABB of the hierarchy root
            Box finalAABB = parent.CalculateModelAABB(parent.GetGameObjectType(), queryConfig);

            // Loop through each child in the hierarchy
            parent.CollectChildren(queryConfig.includeInactive, queryConfig.includeInvisible, sChildObjectBuffer);
            foreach (var child in sChildObjectBuffer)
            {
                // If this is an added override, ignore it if config says so
                #if UNITY_EDITOR
                if (!queryConfig.includeAddedObjectOverrides && 
                    PrefabUtility.IsAddedGameObjectOverride(child)) continue;
                #endif

                // Calculate model AABB for this child
                Box modelAABB = child.CalculateModelAABB(child.GetGameObjectType(), queryConfig);
                if (modelAABB.isValid)
                {
                    // Note: The child's model AABB exists in model space, isolated from its hierarchy.
                    //       However, we want to build a final AABB for the entire hierarchy. So we need
                    //       to convert the child AABB from model space to 'hierarchy space'. We do this
                    //       by calculating the child's transform relative to its parent. We can then use
                    //       this transform matrix to transform the child AABB.
                    Matrix4x4 relativeMtx = child.transform.localToWorldMatrix.CalcRelativeTransform(rootMtx);
                    modelAABB.Transform(relativeMtx);

                    // Init final AABB or enclose
                    if (finalAABB.isValid) finalAABB.EncloseBox(modelAABB);
                    else finalAABB = modelAABB;

                    // Note: Useful when the artist has created a hierarchy with unnecessary meshes.
                    //       Example: Root has valid mesh with renderer. Child has mesh collider and a duplicate
                    //       mesh and renderer which do not render. This kind of overlap can create rounding errors.
                    finalAABB.size = finalAABB.size.FixFloatError();
                }
            }

            // Return final AABB
            return finalAABB;
        }

        //-----------------------------------------------------------------------------
        // Name: GetGameObjectType() (Public Extension)
        // Desc: Returns the type of the specified game object.
        // Rtrn: The game objects' type.
        //-----------------------------------------------------------------------------
        public static EGameObjectType GetGameObjectType(this GameObject gameObject)
        {
            // Get type from map if available
            if (sObjectTypeMap.TryGetValue(gameObject, out EGameObjectType type)) return type;
            else
            {
                // Object not stored in map yet, get it manually
                Terrain terrain = gameObject.GetTerrain();
                if (terrain != null)
                {
                    sObjectTypeMap.Add(gameObject, EGameObjectType.Terrain);
                    return EGameObjectType.Terrain;
                }

                Mesh mesh = gameObject.GetMesh();
                if (mesh != null)
                {
                    if (gameObject.GetMeshOrSkinnedMeshRenderer() != null)
                    {
                        sObjectTypeMap.Add(gameObject, EGameObjectType.Mesh);
                        return EGameObjectType.Mesh;
                    }
                }

                Sprite sprite = gameObject.GetSprite();
                if (sprite != null)
                {
                    sObjectTypeMap.Add(gameObject, EGameObjectType.Sprite);
                    return EGameObjectType.Sprite;
                }

                Light light = gameObject.GetLight();
                if (light != null)
                {
                    sObjectTypeMap.Add(gameObject, EGameObjectType.Light);
                    return EGameObjectType.Light;
                }

                ParticleSystem particleSystem = gameObject.GetParticleSystem();
                if (particleSystem != null)
                {
                    sObjectTypeMap.Add(gameObject, EGameObjectType.ParticleSystem);
                    return EGameObjectType.ParticleSystem;
                }

                Camera camera = gameObject.GetCamera();
                if (camera != null)
                {
                    sObjectTypeMap.Add(gameObject, EGameObjectType.Camera);
                    return EGameObjectType.Camera;
                }

                // If the object has only a transform attached, it's an empty object.
                // Otherwise, we will treat it as 'Other'.
                gameObject.GetComponents(sComponentBuffer);
                if (sComponentBuffer.Count == 1)
                {
                    // Empty object
                    sObjectTypeMap.Add(gameObject, EGameObjectType.Empty);
                    return EGameObjectType.Empty;
                }
                else
                {
                    // Other
                    sObjectTypeMap.Add(gameObject, EGameObjectType.Other);
                    return EGameObjectType.Other;
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: OnObjectTypeDirty() (Public Extension)
        // Desc: Must be called whenever the type of an object changes. For example, this
        //       should be called when adding/removing components to/from a game object.
        //-----------------------------------------------------------------------------
        public static void OnObjectTypeDirty(this GameObject gameObject)
        {
            // Remove object from cache. The next call to 'GetGameObjectType' will
            // calculate and cache the updated type.
            sObjectTypeMap.Remove(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: IsSceneObject() (Public Extension)
        // Desc: Checks if the game object is a game object that belongs to a valid scene.
        // Rtrn: True if the game object belongs to a scene and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool IsSceneObject(this GameObject gameObject)
        {
            return gameObject.scene.IsValid();
        }

        //-----------------------------------------------------------------------------
        // Name: IgnoreInBuild() (Public Extension)
        // Desc: Call this function to ignore the specified object in the final build.
        //       This is accomplished by setting the object's tag to 'EditorOnly'.
        //-----------------------------------------------------------------------------
        public static void IgnoreInBuild(this GameObject gameObject)
        {
            gameObject.tag = "EditorOnly";
        }

        //-----------------------------------------------------------------------------
        // Name: GetPrefabAsset() (Public Extension)
        // Desc: Returns the prefab asset the object is part of.
        // Rtrn: The prefab asset the object is part of or null if the object is not
        //       an instance of a prefab.
        //-----------------------------------------------------------------------------
        #if UNITY_EDITOR
        public static GameObject GetPrefabAsset(this GameObject gameObject)
        {
            return PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
        }
        #endif

        //-----------------------------------------------------------------------------
        // Name: GetOutermostPrefabAsset() (Public Extension)
        // Desc: If 'gameObject' is part of a prefab instance, this function will find
        //       the root object of the prefab instance and return its associated prefab
        //       asset.
        // Rtrn: Prefab asset of the prefab instance root object or null of the object
        //       is not part of a prefab instance.
        //-----------------------------------------------------------------------------
        #if UNITY_EDITOR
        public static GameObject GetOutermostPrefabAsset(this GameObject gameObject)
        {
            // Get prefab instance root
            var root = gameObject.GetOutermostPrefabInstanceRoot();
            if (root == null) return null;

            // Return prefab asset associated with root
            return root.GetPrefabAsset();
        }
        #endif

        //-----------------------------------------------------------------------------
        // Name: GetOutermostPrefabInstanceRoot() (Public Extension)
        // Desc: If 'gameObject' is part of a prefab instance, this function will return
        //       the root object of the prefab instance.
        // Rtrn: The root object of the prefab instance the object is part of or null
        //       if the object is not part of a prefab instance.
        //-----------------------------------------------------------------------------
        #if UNITY_EDITOR
        public static GameObject GetOutermostPrefabInstanceRoot(this GameObject gameObject)
        {
            return PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
        }
        #endif

        //-----------------------------------------------------------------------------
        // Name: IsPartOfPrefabInstance() (Public Extension)
        // Desc: Checks if 'gameObject' is part of a prefab instance.
        // Rtrn: True if the object is part of a prefab instance and false otherwise.
        //-----------------------------------------------------------------------------
        #if UNITY_EDITOR
        public static bool IsPartOfPrefabInstance(this GameObject gameObject)
        {
            return PrefabUtility.IsPartOfPrefabInstance(gameObject);
        }
        #endif

        //-----------------------------------------------------------------------------
        // Name: HasVolume() (Public Extension)
        // Desc: Checks if the game object has a volume. An object has a volume if it
        //       is a mesh, sprite or a terrain.
        // Rtrn: True if the object has a volume and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool HasVolume(this GameObject gameObject)
        {
            // Get game object type
            EGameObjectType objectType = gameObject.GetGameObjectType();

            // Return result
            return  objectType == EGameObjectType.Mesh || objectType == EGameObjectType.Sprite ||
                    objectType == EGameObjectType.Terrain;
        }

        //-----------------------------------------------------------------------------
        // Name: IsVisible() (Public Extension)
        // Desc: Checks if the game object is visible. An object is visible if it is a
        //       mesh, sprite or terrain and the required renderer component is enabled.
        //       Objects with no volume (i.e. empty objects, lights etc) are considered
        //       visible by default.
        // Rtrn: True if the object is visible and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool IsVisible(this GameObject gameObject)
        {
            // Get game object type
            EGameObjectType objectType = gameObject.GetGameObjectType();

            // Return result
            if (objectType == EGameObjectType.Mesh) return gameObject.IsMeshOrSkinnedMeshRendererEnabled();
            else if (objectType == EGameObjectType.Sprite) return gameObject.IsSpriteRendererEnabled();
            else if (objectType == EGameObjectType.Terrain) return gameObject.IsTerrainEnabled();

            // Visible
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: CollectChildren() (Public Extension)
        // Desc: Returns a list of all children (direct and indirect). The 'this' object
        //       is not included in the list.
        // Parm: includeInactive  - If true, includes inactive objects in the result.
        //       includeInvisible - If true, includes invisible objects in the result.
        //       children         - Child objects will be stored here. The list is always
        //                          cleared before being populated.
        //       append           - If true, appends to the existing list contents;
        //                          otherwise, the list is cleared before storing.
        //-----------------------------------------------------------------------------
        public static void CollectChildren(this GameObject gameObject, bool includeInactive, bool includeInvisible, IList<GameObject> children, bool append = false)
        {
            // Clear child list and get child transforms
            if (!append) children.Clear();
            gameObject.GetComponentsInChildren(includeInactive, sTransformBuffer);

            // Are invisible objects allowed?
            if (includeInvisible)
            {
                // Loop through each child transform 
                foreach (var child in sTransformBuffer)
                {
                    // Add child if not the same as 'this' object
                    if (child.gameObject != gameObject) 
                        children.Add(child.gameObject);
                }
            }
            else
            {
                // Loop through each child transform 
                foreach (var child in sTransformBuffer)
                {
                    // Add child if not the same as 'this' object and if the object is visible
                    if (child.gameObject != gameObject &&
                        child.gameObject.IsVisible()) children.Add(child.gameObject);
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CollectMeAndChildren() (Public Extension)
        // Desc: Returns a list of all children (direct and indirect). The 'this' object
        //       is also included in the list.
        // Parm: includeInactive  - If true, includes inactive objects in the result.
        //       includeInvisible - If true, includes invisible objects in the result.
        //       children         - Child objects will be stored here. The list is always
        //                          cleared before being populated.
        //       append           - If true, appends to the existing list contents;
        //                          otherwise, the list is cleared before storing.
        //-----------------------------------------------------------------------------
        public static void CollectMeAndChildren(this GameObject gameObject, bool includeInactive, bool includeInvisible, IList<GameObject> children, bool append = false)
        {
            // Clear child list and get child transforms
            if (!append) children.Clear();
            gameObject.GetComponentsInChildren(includeInactive, sTransformBuffer);

            // Are invisible objects allowed?
            if (includeInvisible)
            {
                // Store child objects
                foreach (var child in sTransformBuffer)
                    children.Add(child.gameObject);
            }
            else
            {
                // Store visible child objects
                foreach (var child in sTransformBuffer)
                {
                    if (child.gameObject.IsVisible()) 
                        children.Add(child.gameObject);
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: GetMesh() (Public Extension)
        // Desc: Returns the mesh attached to this game object. The mesh could belong
        //       to a 'MeshFilter' or a 'SkinnedMeshRenderer'.
        // Rtrn: The mesh attached to the game object or null if the object has no mesh.
        //-----------------------------------------------------------------------------
        public static Mesh GetMesh(this GameObject gameObject)
        {
            // Get the mesh from the mesh filter if any
            sMeshFilterBuffer.Clear();
            gameObject.GetComponents(sMeshFilterBuffer);
            if (sMeshFilterBuffer.Count != 0 && sMeshFilterBuffer[0].sharedMesh != null) 
                return sMeshFilterBuffer[0].sharedMesh;

            // No mesh found. Return the skinned mesh renderer mesh.
            sSkinnedMeshRendererBuffer.Clear();
            gameObject.GetComponents(sSkinnedMeshRendererBuffer);
            if (sSkinnedMeshRendererBuffer.Count != 0 && sSkinnedMeshRendererBuffer[0].sharedMesh != null) 
                return sSkinnedMeshRendererBuffer[0].sharedMesh;

            // No mesh found
            return null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetEnabledMesh() (Public Extension)
        // Desc: Returns the mesh that is associated with an enabled component that is
        //       attached to this game object like a 'MeshFilter' or a 'SkinnedMeshRenderer'.
        // Rtrn: The mesh that is associated with an enabled component or null if no such
        //       mesh exists.
        //-----------------------------------------------------------------------------
        public static Mesh GetEnabledMesh(this GameObject gameObject)
        {
            // Get the mesh from the mesh filter if any
            sMeshFilterBuffer.Clear();
            gameObject.GetComponents(sMeshFilterBuffer);
            if (sMeshFilterBuffer.Count != 0 && sMeshFilterBuffer[0].sharedMesh != null)
                return sMeshFilterBuffer[0].sharedMesh;

            // No mesh found. Return the skinned mesh renderer mesh.
            sSkinnedMeshRendererBuffer.Clear();
            gameObject.GetComponents(sSkinnedMeshRendererBuffer);
            if (sSkinnedMeshRendererBuffer.Count != 0 && sSkinnedMeshRendererBuffer[0].sharedMesh != null && sSkinnedMeshRendererBuffer[0].enabled) 
                return sSkinnedMeshRendererBuffer[0].sharedMesh;

            // No mesh found
            return null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetMeshFilter() (Public Extension)
        // Desc: Returns the 'MeshFilter' component attached to this object.
        // Rtrn: The attached 'MeshFilter' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static MeshFilter GetMeshFilter(this GameObject gameObject)
        {
            sMeshFilterBuffer.Clear();
            gameObject.GetComponents(sMeshFilterBuffer);
            return sMeshFilterBuffer.Count != 0 ? sMeshFilterBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetMeshRenderer() (Public Extension)
        // Desc: Returns the 'MeshRenderer' component attached to this object.
        // Rtrn: The attached 'MeshRenderer' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static MeshRenderer GetMeshRenderer(this GameObject gameObject)
        {
            sMeshRendererBuffer.Clear();
            gameObject.GetComponents(sMeshRendererBuffer);
            return sMeshRendererBuffer.Count != 0 ? sMeshRendererBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetSkinnedMeshRenderer() (Public Extension)
        // Desc: Returns the 'SkinnedMeshRenderer' component attached to this object.
        // Rtrn: The attached 'SkinnedMeshRenderer' component or null if no such component
        //       exists.
        //-----------------------------------------------------------------------------
        public static SkinnedMeshRenderer GetSkinnedMeshRenderer(this GameObject gameObject)
        {
            sSkinnedMeshRendererBuffer.Clear();
            gameObject.GetComponents(sSkinnedMeshRendererBuffer);
            return sSkinnedMeshRendererBuffer.Count != 0 ? sSkinnedMeshRendererBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetMeshOrSkinnedMeshRenderer() (Public Extension)
        // Desc: Returns the 'MeshRenderer' or 'SkinnedMeshRenderer' component attached
        //       to this object.
        // Rtrn: The attached 'MeshRenderer' component. If missing, the function will
        //       check if a 'SkinnedMeshRenderer' exists and return that instead. Otherwise
        //       it will return null.
        //-----------------------------------------------------------------------------
        public static Renderer GetMeshOrSkinnedMeshRenderer(this GameObject gameObject)
        {
            // Check for a 'MeshRenderer' component
            sMeshRendererBuffer.Clear();
            gameObject.GetComponents(sMeshRendererBuffer);
            if (sMeshRendererBuffer.Count != 0) return sMeshRendererBuffer[0];

            // Check for a 'SkinnedMeshRenderer' component
            sSkinnedMeshRendererBuffer.Clear();
            gameObject.GetComponents(sSkinnedMeshRendererBuffer);
            return sSkinnedMeshRendererBuffer.Count != 0 ? sSkinnedMeshRendererBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: IsMeshOrSkinnedMeshRendererEnabled() (Public Extension)
        // Desc: Checks if the object's 'MeshRenderer' or 'SkinnedMeshRenderer' component
        //       is enabled.
        // Rtrn: True if the object has an enabled 'MeshRenderer' or 'SkinnedMeshRenderer'
        //       component.
        //-----------------------------------------------------------------------------
        public static bool IsMeshOrSkinnedMeshRendererEnabled(this GameObject gameObject)
        {
            // Check mesh renderer
            MeshRenderer meshRenderer = gameObject.GetMeshRenderer();
            if (meshRenderer != null) return meshRenderer.enabled;

            // Check skinned mesh renderer
            SkinnedMeshRenderer skinnedRenderer = gameObject.GetSkinnedMeshRenderer();
            if (skinnedRenderer != null) return skinnedRenderer.enabled;

            // No enabled renderer found
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: IsSpriteRendererEnabled() (Public Extension)
        // Desc: Checks if the object's 'SpriteRenderer' component is enabled.
        // Rtrn: True if the object has an enabled 'SpriteRenderer' component.
        //-----------------------------------------------------------------------------
        public static bool IsSpriteRendererEnabled(this GameObject gameObject)
        {
            // Is there a sprite rendered attached? Check if enabled.
            SpriteRenderer spriteRenderer = gameObject.GetSpriteRenderer();
            if (spriteRenderer != null) return spriteRenderer.enabled;

            // No enabled sprite renderer found
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: IsTerrainEnabled() (Public Extension)
        // Desc: Checks if the object's 'Terrain' component is enabled.
        // Rtrn: True if the object has an enabled 'Terrain' component.
        //-----------------------------------------------------------------------------
        public static bool IsTerrainEnabled(this GameObject gameObject)
        {
            // Is there a terrain component attached? Check if enabled.
            Terrain terrain = gameObject.GetTerrain();
            if (terrain != null) return terrain.enabled;

            // No enabled terrain found
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: GetPlugin() (Public Extension)
        // Desc: Returns the 'RTG' script component attached to this object.
        // Rtrn: The attached 'RTG' script component or null if no such component
        //       exists.
        //-----------------------------------------------------------------------------
        public static RTG GetPlugin(this GameObject gameObject)
        {
            sPluginBuffer.Clear();
            gameObject.GetComponents(sPluginBuffer);
            return sPluginBuffer.Count != 0 ? sPluginBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: IsPlugin() (Public Extension)
        // Desc: Checks if the game object is the RTG plugin object.
        // Rtrn: True if the game object is the RTG plugin object.
        //-----------------------------------------------------------------------------
        public static bool IsPlugin(this GameObject gameObject)
        {
            return gameObject.GetPlugin() != null;
        }

        //-----------------------------------------------------------------------------
        // Name: IsPluginOrPluginModule() (Public Extension)
        // Desc: Checks if the game object is the RTG plugin object or belongs to the
        //       RTG plugin hierarchy.
        // Rtrn: True if the game object is part of the RTG plugin hierarchy.
        //-----------------------------------------------------------------------------
        public static bool IsPluginOrPluginModule(this GameObject gameObject)
        {
            // Need plugin instance
            RTG plugin = RTG.get;
            if (plugin == null)
                return gameObject.IsPlugin();

            // Check plugin hierarchy
            Transform pluginTransform = plugin.transform;
            Transform objectTransform = gameObject.transform;
            return objectTransform == pluginTransform || objectTransform.IsChildOf(pluginTransform);
        }

        //-----------------------------------------------------------------------------
        // Name: GetRTObjectTransformGizmoTarget() (Public Extension)
        // Desc: Returns the 'IRTObjectTransformGizmoTarget' script component attached
        //       to this object.
        // Rtrn: The attached 'IRTObjectTransformGizmoTarget' script component or null
        //       if no such component exists.
        //-----------------------------------------------------------------------------
        public static IRTObjectTransformGizmoTarget GetRTObjectTransformGizmoTarget(this GameObject gameObject)
        {
            sRTOTGTBuffer.Clear();
            gameObject.GetComponents(sRTOTGTBuffer);
            return sRTOTGTBuffer.Count != 0 ? sRTOTGTBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetCamera() (Public Extension)
        // Desc: Returns the 'Camera' component attached to this object.
        // Rtrn: The attached 'Camera' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static Camera GetCamera(this GameObject gameObject)
        {
            sCameraBuffer.Clear();
            gameObject.GetComponents(sCameraBuffer);
            return sCameraBuffer.Count != 0 ? sCameraBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetLight() (Public Extension)
        // Desc: Returns the 'Light' component attached to this object.
        // Rtrn: The attached 'Light' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static Light GetLight(this GameObject gameObject)
        {
            sLightBuffer.Clear();
            gameObject.GetComponents(sLightBuffer);
            return sLightBuffer.Count != 0 ? sLightBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetParticleSystem() (Public Extension)
        // Desc: Returns the 'ParticleSystem' component attached to this object.
        // Rtrn: The attached 'ParticleSystem' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static ParticleSystem GetParticleSystem(this GameObject gameObject)
        {
            sParticleSystemBuffer.Clear();
            gameObject.GetComponents(sParticleSystemBuffer);
            return sParticleSystemBuffer.Count != 0 ? sParticleSystemBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetTree() (Public Extension)
        // Desc: Returns the 'Tree' component attached to this object.
        // Rtrn: The attached 'Tree' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static Tree GetTree(this GameObject gameObject)
        {
            sTreeBuffer.Clear();
            gameObject.GetComponents(sTreeBuffer);
            return sTreeBuffer.Count != 0 ? sTreeBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetLODGroup() (Public Extension)
        // Desc: Returns the 'LODGroup' component attached to this object.
        // Rtrn: The attached 'LODGroup' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static LODGroup GetLODGroup(this GameObject gameObject)
        {
            sLODGroupBuffer.Clear();
            gameObject.GetComponents(sLODGroupBuffer);
            return sLODGroupBuffer.Count != 0 ? sLODGroupBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: CollectLODGroups() (Public Extension)
        // Desc: Collects all LOD groups in the object's hierarchy and stores them inside
        //       'lodGroups'.
        // Parm: lodGroups - Returns the collected LOD groups.
        //-----------------------------------------------------------------------------
        public static void CollectLODGroups(this GameObject gameObject, List<LODGroup> lodGroups)
        {
            // Get LOD groups
            sLODGroupBuffer.Clear();
            gameObject.GetComponentsInChildren(sLODGroupBuffer);

            // Store LOD groups
            lodGroups.Clear();
            lodGroups.AddRange(sLODGroupBuffer);
        }

        //-----------------------------------------------------------------------------
        // Name: GetSprite() (Public Extension)
        // Desc: Returns the 'Sprite' component attached to this object.
        // Rtrn: The attached 'Sprite' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static Sprite GetSprite(this GameObject gameObject)
        {
            sSpriteRendererBuffer.Clear();
            gameObject.GetComponents(sSpriteRendererBuffer);
            return sSpriteRendererBuffer.Count != 0 ? sSpriteRendererBuffer[0].sprite : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetSpriteRenderer() (Public Extension)
        // Desc: Returns the 'SpriteRenderer' component attached to this object.
        // Rtrn: The attached 'SpriteRenderer' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static SpriteRenderer GetSpriteRenderer(this GameObject gameObject)
        {
            sSpriteRendererBuffer.Clear();
            gameObject.GetComponents(sSpriteRendererBuffer);
            return sSpriteRendererBuffer.Count != 0 ? sSpriteRendererBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetRenderer() (Public Extension)
        // Desc: Returns the 'Renderer' component attached to this object.
        // Rtrn: The attached 'Renderer' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static Renderer GetRenderer(this GameObject gameObject)
        {
            sRendererBuffer.Clear();
            gameObject.GetComponents(sRendererBuffer);
            return sRendererBuffer.Count != 0 ? sRendererBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetTerrain() (Public Extension)
        // Desc: Returns the 'Terrain' component attached to this object.
        // Rtrn: The attached 'Terrain' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static Terrain GetTerrain(this GameObject gameObject)
        {
            sTerrainBuffer.Clear();
            gameObject.GetComponents(sTerrainBuffer);
            return sTerrainBuffer.Count != 0 ? sTerrainBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetTerrainCollider() (Public Extension)
        // Desc: Returns the 'TerrainCollider' component attached to this object.
        // Rtrn: The attached 'TerrainCollider' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static TerrainCollider GetTerrainCollider(this GameObject gameObject)
        {
            sTerrainColliderBuffer.Clear();
            gameObject.GetComponents(sTerrainColliderBuffer);
            return sTerrainColliderBuffer.Count != 0 ? sTerrainColliderBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetBoxCollider() (Public Extension)
        // Desc: Returns the 'BoxCollider' component attached to this object.
        // Rtrn: The attached 'BoxCollider' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static BoxCollider GetBoxCollider(this GameObject gameObject)
        {
            sBoxColliderBuffer.Clear();
            gameObject.GetComponents(sBoxColliderBuffer);
            return sBoxColliderBuffer.Count != 0 ? sBoxColliderBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetBoxCollider2D() (Public Extension)
        // Desc: Returns the 'BoxCollider2D' component attached to this object.
        // Rtrn: The attached 'BoxCollider2D' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static BoxCollider2D GetBoxCollider2D(this GameObject gameObject)
        {
            sBoxCollider2DBuffer.Clear();
            gameObject.GetComponents(sBoxCollider2DBuffer);
            return sBoxCollider2DBuffer.Count != 0 ? sBoxCollider2DBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetSphereCollider() (Public Extension)
        // Desc: Returns the 'SphereCollider' component attached to this object.
        // Rtrn: The attached 'SphereCollider' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static SphereCollider GetSphereCollider(this GameObject gameObject)
        {
            sSphereColliderBuffer.Clear();
            gameObject.GetComponents(sSphereColliderBuffer);
            return sSphereColliderBuffer.Count != 0 ? sSphereColliderBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetCircleCollider2D() (Public Extension)
        // Desc: Returns the 'CircleCollider2D' component attached to this object.
        // Rtrn: The attached 'CircleCollider2D' component or null if no such component
        //       exists.
        //-----------------------------------------------------------------------------
        public static CircleCollider2D GetCircleCollider2D(this GameObject gameObject)
        {
            sCircleColliderBuffer.Clear();
            gameObject.GetComponents(sCircleColliderBuffer);
            return sCircleColliderBuffer.Count != 0 ? sCircleColliderBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetCapsuleCollider() (Public Extension)
        // Desc: Returns the 'CapsuleCollider' component attached to this object.
        // Rtrn: The attached 'CapsuleCollider' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static CapsuleCollider GetCapsuleCollider(this GameObject gameObject)
        {
            sCapsuleColliderBuffer.Clear();
            gameObject.GetComponents(sCapsuleColliderBuffer);
            return sCapsuleColliderBuffer.Count != 0 ? sCapsuleColliderBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetCapsuleCollider2D() (Public Extension)
        // Desc: Returns the 'CapsuleCollider2D' component attached to this object.
        // Rtrn: The attached 'CapsuleCollider2D' component or null if no such component
        //       exists.
        //-----------------------------------------------------------------------------
        public static CapsuleCollider2D GetCapsuleCollider2D(this GameObject gameObject)
        {
            sCapsuleCollider2DBuffer.Clear();
            gameObject.GetComponents(sCapsuleCollider2DBuffer);
            return sCapsuleCollider2DBuffer.Count != 0 ? sCapsuleCollider2DBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetCharacterController() (Public Extension)
        // Desc: Returns the 'CharacterController' component attached to this object.
        // Rtrn: The attached 'CharacterController' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static CharacterController GetCharacterController(this GameObject gameObject)
        {
            sCharacterControllerBuffer.Clear();
            gameObject.GetComponents(sCharacterControllerBuffer);
            return sCharacterControllerBuffer.Count != 0 ? sCharacterControllerBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetAudioReverbZone() (Public Extension)
        // Desc: Returns the 'AudioReverbZone' component attached to this object.
        // Rtrn: The attached 'AudioReverbZone' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static AudioReverbZone GetAudioReverbZone(this GameObject gameObject)
        {
            sAudioReverbZoneBuffer.Clear();
            gameObject.GetComponents(sAudioReverbZoneBuffer);
            return sAudioReverbZoneBuffer.Count != 0 ? sAudioReverbZoneBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: GetAudioSource() (Public Extension)
        // Desc: Returns the 'AudioSource' component attached to this object.
        // Rtrn: The attached 'AudioSource' component or null if no such component exists.
        //-----------------------------------------------------------------------------
        public static AudioSource GetAudioSource(this GameObject gameObject)
        {
            sAudioSourceBuffer.Clear();
            gameObject.GetComponents(sAudioSourceBuffer);
            return sAudioSourceBuffer.Count != 0 ? sAudioSourceBuffer[0] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: HierarchyHasTrees() (Public Extension)
        // Desc: Checks if the object hierarchy has at least one 'Tree' component.
        // Parm: includeInactive  - If true, includes inactive objects in the result.
        //       includeInvisible - If true, includes invisible objects in the result.
        // Rtrn: True if the hierarchy contains at least one 'Tree' component; false otherwise.
        //-----------------------------------------------------------------------------
        public static bool HierarchyHasTrees(this GameObject root, bool includeInactive, bool includeInvisible)
        {
            // Collect all objects in the hierarchy
            root.CollectMeAndChildren(includeInactive, includeInvisible, sObjectBuffer);

            // Loop through each object in the hierarchy
            int count = sObjectBuffer.Count;
            for (int i = 0; i < count; ++i)
            {
                // If this object has a tree, return true
                if (sObjectBuffer[i].GetTree())
                    return true;
            }

            // No tree
            return false;
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Reset() (Internal Static Function)
        // Desc: Clears all static extension state.
        //-----------------------------------------------------------------------------
        internal static void Internal_Reset()
        {
            // Clear component buffers
            sCameraBuffer.Clear();
            sSpriteRendererBuffer.Clear();
            sRendererBuffer.Clear();
            sMeshFilterBuffer.Clear();
            sTerrainBuffer.Clear();
            sTerrainColliderBuffer.Clear();
            sBoxColliderBuffer.Clear();
            sBoxCollider2DBuffer.Clear();
            sSphereColliderBuffer.Clear();
            sCircleColliderBuffer.Clear();
            sCapsuleColliderBuffer.Clear();
            sCapsuleCollider2DBuffer.Clear();
            sCharacterControllerBuffer.Clear();
            sAudioReverbZoneBuffer.Clear();
            sAudioSourceBuffer.Clear();
            sMeshRendererBuffer.Clear();
            sSkinnedMeshRendererBuffer.Clear();
            sLightBuffer.Clear();
            sParticleSystemBuffer.Clear();
            sTreeBuffer.Clear();
            sLODGroupBuffer.Clear();
            sComponentBuffer.Clear();
            sPluginBuffer.Clear();
            sRTOTGTBuffer.Clear();

            // Clear general buffers
            sTransformBuffer.Clear();
            sParentObjectBuffer.Clear();
            sChildObjectBuffer.Clear();
            sObjectBuffer.Clear();
            sDOPVertexBuffer.Clear();
            sScreenPointBuffer.Clear();

            // Clear object type cache
            sObjectTypeMap.Clear();
        }

        //-----------------------------------------------------------------------------
        // Name: CollectAllObjects() (Public Extension)
        // Desc: Collects all objects in 'gameObjects' along with their entire hierarchies
        //       and stores them in the 'collected' list.
        // Parm: gameObjects      - Source objects whose hierarchies will be collected.
        //       includeInactive  - If true, includes inactive objects in the result.
        //       includeInvisible - If true, includes invisible objects in the result.
        //       collected        - Destination list which receives all collected objects.
        //       append           - If true, appends to the existing list contents;
        //                          otherwise, the list is cleared before storing.
        //-----------------------------------------------------------------------------
        public static void CollectAllObjects(IList<GameObject> gameObjects, bool includeInactive, bool includeInvisible, IList<GameObject> collected, bool append = false)
        {
            // Clear?
            if (!append) collected.Clear();

            // Collect parents from object collection
            CollectParents(gameObjects, sParentObjectBuffer);

            // Now collect objects together with their hierarchies
            int count = sParentObjectBuffer.Count;
            for (int i = 0; i < count; ++i)
                sParentObjectBuffer[i].CollectMeAndChildren(includeInactive, includeInvisible, collected, true);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateObjectsWorldAABB() (Public Static Function)
        // Desc: Calculates and returns the world AABB for the specified game object
        //       collection.
        // Parm: gameObjects - The game object collection whose AABB is returned.
        //       queryConfig - Bounds query config.
        // Rtrn: The world AABB that encloses all objects in 'gameObjects'.
        //-----------------------------------------------------------------------------
        public static Box CalculateObjectsWorldAABB(IEnumerable<GameObject> gameObjects, BoundsQueryConfig queryConfig)
        {
            // Init AABB to invalid AABB
            Box aabb = Box.GetInvalid();

            // Loop through each game object
            foreach (var gameObject in gameObjects)
            {
                // Calculate object world AABB
                Box worldAABB = gameObject.CalculateWorldAABB(queryConfig);

                // If the AABB is valid, merge it with the final AABB
                if (worldAABB.isValid)
                {
                    if (aabb.isValid) aabb.EncloseBox(worldAABB);
                    else aabb = worldAABB;
                }
            }

            // Return final AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateHierarchiesWorldAABB() (Public Static Function)
        // Desc: Calculates and returns the world AABB that encloses the specified 
        //       collection of object hierarchies.
        // Parm: parents     - Each object in this collection is the parent of an object
        //                     hierarchy. 
        //       queryConfig - Bounds query config.
        // Rtrn: The world AABB that encloses all object hierarchies in 'parents'.
        //-----------------------------------------------------------------------------
        public static Box CalculateHierarchiesWorldAABB(IEnumerable<GameObject> parents, BoundsQueryConfig queryConfig)
        {
            // Init AABB to invalid AABB
            Box aabb = Box.GetInvalid();

            // Loop through each hierarchy parent
            foreach (var parent in parents)
            {
                // Calculate hierarchy AABB
                Box hierarchyAABB = parent.CalculateHierarchyWorldAABB(queryConfig);

                // If the AABB is valid, merge it with the final AABB
                if (hierarchyAABB.isValid)
                {
                    if (aabb.isValid) aabb.EncloseBox(hierarchyAABB);
                    else aabb = hierarchyAABB;
                }
            }

            // Return final AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateHierarchiesWorldOBB() (Public Static Function)
        // Desc: Calculates and returns the world OBB that encloses the specified 
        //       collection of object hierarchies.
        // Parm: parents     - Each object in this collection is the parent of an object
        //                     hierarchy. 
        //       queryConfig - Bounds query config.
        // Rtrn: The world OBB that encloses all object hierarchies in 'parents'.
        //-----------------------------------------------------------------------------
        public static OBox CalculateHierarchiesWorldOBB(IEnumerable<GameObject> parents, BoundsQueryConfig queryConfig)
        {
            // Init OBB to invalid AABB
            OBox obb = OBox.GetInvalid();

            // Loop through each hierarchy parent
            foreach (var parent in parents)
            {
                // Calculate hierarchy OBB
                OBox hierarchyOBB = parent.CalculateHierarchyWorldOBB(queryConfig);

                // If the OBB is valid, merge it with the final OBB
                if (hierarchyOBB.isValid)
                {
                    if (obb.isValid) obb.EncloseBox(hierarchyOBB);
                    else obb = hierarchyOBB;
                }
            }

            // Return final OBB
            return obb;
        }

        //-----------------------------------------------------------------------------
        // Name: FindFirstObjectByType() (Public Static Function)
        // Desc: Returns the first object of the specified type.
        // Parm: T - Object type. Must derive from 'UnityEngine.Object'.
        // Rtrn: The first object of the specified type or null if no such object exists.
        //-----------------------------------------------------------------------------
        public static T FindFirstObjectByType<T>() where T : UnityEngine.Object
        {
            return GameObject.FindFirstObjectByType<T>();
        }

        //-----------------------------------------------------------------------------
        // Name: FindObjectsByType() (Public Static Function)
        // Desc: Returns an array of objects of the specified type.
        // Parm: T - Object type. Must derive from 'UnityEngine.Object'.
        // Rtrn: An array of objects of the specified type.
        //-----------------------------------------------------------------------------
        public static T[] FindObjectsByType<T>() where T : UnityEngine.Object
        {
            return GameObject.FindObjectsByType<T>(FindObjectsSortMode.None);
        }

        //-----------------------------------------------------------------------------
        // Name: FindObjectsByType() (Public Static Function)
        // Desc: Returns an array of objects of the specified type.
        // Parm: T               - Object type. Must derive from 'UnityEngine.Object'.
        //       includeInactive - If true, inactive objects will also be returned.
        // Rtrn: An array of objects of the specified type.
        //-----------------------------------------------------------------------------
        public static T[] FindObjectsByType<T>(bool includeInactive) where T : UnityEngine.Object
        {
            return GameObject.FindObjectsByType<T>(includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        }

        //-----------------------------------------------------------------------------
        // Name: CollectGameObjects() (Public Static Function)
        // Desc: Given a collection of 'Transform' components, the function will extract
        //       the corresponding game objects and store them in the 'gameObjects' list. 
        // Parm: transform   - Collection of transform components whose game objects will be
        //                     stored in 'gameObjects'.
        //       gameObjects - The list which returns the transform game objects.
        //-----------------------------------------------------------------------------
        public static void CollectGameObjects(IEnumerable<Transform> transforms, List<GameObject> gameObjects)
        {
            // Clear object list
            gameObjects.Clear();

            // Loop through each transform and store its game object in the list
            foreach (var t in transforms)
                gameObjects.Add(t.gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: CollectParents() (Public Static Function)
        // Desc: Given a collection of game objects, the function will extract the parent
        //       objects and store them in 'parentObjects'.
        // Parm: gameObjects   - Collection of game objects being parsed.
        //       parentObjects - Returns the parents found in 'gameObjects'. These are
        //                       objects which don't have a parent that exists in 'gameObjects'.
        //-----------------------------------------------------------------------------
        public static void CollectParents(IEnumerable<GameObject> gameObjects, List<GameObject> parentObjects)
        {
            // Clear parent list
            parentObjects.Clear();

            // Loop through each object
            foreach (var currentObject in gameObjects)
            {             
                bool        foundParent     = false;                    // Used to check if this is a parent or not
                Transform   objectTransform = currentObject.transform;  // Cached object transform

                // Loop through each object in the list and check if it's a parent of the current game object
                foreach (var go in gameObjects)
                {
                    // Avoid self-check
                    if (go != currentObject)
                    {
                        // Is the current object a child of 'go'?
                        if (objectTransform.IsChildOf(go.transform))
                        {
                            // Yes it is, we found a parent. Exit loop.
                            foundParent = true;
                            break;
                        }
                    }
                }

                // If we didn't find a parent, it means the current object doesn't have
                // a parent in 'gameObjects'. So we store it inside 'parentObjects'.
                if (!foundParent) parentObjects.Add(currentObject);
            }
        }
        #endregion
    }
    #endregion
}