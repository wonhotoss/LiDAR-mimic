using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: ObjectRayHit (Public Struct)
    // Desc: Stores information for an object ray hit.
    //-----------------------------------------------------------------------------
    public struct ObjectRayHit
    {
        #region Public Fields
        public GameObject       gameObject;         // The object which was hit
        public RTMeshRayHit     rtMeshHit;          // If a mesh is hit, stores information about the mesh hit. Should be ignored if the 'rtMesh' field is null.
        public SpriteRenderer   spriteRenderer;     // The sprite renderer if the object is a sprite
        public TerrainCollider  terrainCollider;    // The terrain collider if the object is a terrain

        public Vector3          normal;             // Hit normal
        public Vector3          point;              // Hit point
        public float            t;                  // Hit distance from ray origin
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ObjectFilter (Public Class)
    // Desc: Implements an object filter which can be configured to reject objects
    //       that don't meet a specified filter criteria. This can be useful when
    //       performing raycasts or overlap checks etc.
    //-----------------------------------------------------------------------------
    public class ObjectFilter
    {
        #region Private Fields
        HashSet<GameObject>     mIgnoredObjects = new HashSet<GameObject>(128);     // Ignored objects set
        Func<GameObject, bool>  mCustomFilter   = null;                             // Custom filter
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: layerMask (Public Property)
        // Desc: Returns or sets the layer mask that specifies the object layers that
        //       can pass the filter.
        //-----------------------------------------------------------------------------
        public int                      layerMask   { get; set; } = ~0;     
        
        //-----------------------------------------------------------------------------
        // Name: objectTypes (Public Property)
        // Desc: Returns or sets the allowed object types.
        //-----------------------------------------------------------------------------
        public EGameObjectType          objectTypes { get; set; } = EGameObjectType.All;

        //-----------------------------------------------------------------------------
        // Name: customFilter (Public Property)
        // Desc: Returns or sets the custom filter function. The function must return
        //       true if the object is accepted.
        //-----------------------------------------------------------------------------
        public Func<GameObject, bool>   customFilter { get { return mCustomFilter; } set { mCustomFilter = value; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: SetIgnoredObjects() (Public Function)
        // Desc: Use this to let the filter know about any game objects that must be 
        //       ignored.
        // Parm: ignoredObjects - List of game objects that must be ignored.
        //-----------------------------------------------------------------------------
        public void SetIgnoredObjects(IReadOnlyList<GameObject> ignoredObjects)
        {
            // Clear ignored objects set
            mIgnoredObjects.Clear();

            // If null was passed, nothing left to do
            if (ignoredObjects == null) return;

            // Add each game object to the ignored object set
            int count = ignoredObjects.Count;
            for (int i = 0; i < count; ++i)
                mIgnoredObjects.Add(ignoredObjects[i]);
        }

        //-----------------------------------------------------------------------------
        // Name: Pass() (Public Function)
        // Desc: Checks if the specified object can pass the filter.
        // Rtrn: True if the object passes and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Pass(GameObject gameObject)
        {
            // Check layer
            if (!LayerEx.CheckLayerBit(gameObject.layer, layerMask))
                return false;

            // If not all object types are accepted, check if the object's type is valid
            if (objectTypes != EGameObjectType.All &&
                (objectTypes & gameObject.GetGameObjectType()) == 0)
                return false;

            // Is the object ignored?
            if (mIgnoredObjects.Count != 0 && mIgnoredObjects.Contains(gameObject))
                return false;

            // Custom filter?
            if (mCustomFilter != null && !mCustomFilter(gameObject))
                return false;

            // The object passed
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: Pass() (Public Function)
        // Desc: Checks if the specified object can pass the filter.
        // Parm: objectType - Object type. Can improve performance if known beforehand.
        // Rtrn: True if the object passes and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Pass(GameObject gameObject,  EGameObjectType objectType)
        {
            // Check layer
            if ((layerMask & (1 << gameObject.layer)) == 0)
                return false;

            // Check object type (no HasFlag to avoid GC)
            if ((objectTypes & objectType) == 0)
                return false;

            // Is the object ignored?
            if (mIgnoredObjects.Contains(gameObject))
                return false;

            // Custom filter?
            if (mCustomFilter != null && !mCustomFilter(gameObject))
                return false;

            // The object passed
            return true;
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: SceneObjectTree (Public Class)
    // Desc: Implements a tree data structures that allows for fast raycasts and
    //       overlap tests against scene objects.
    //-----------------------------------------------------------------------------
    public class SceneObjectTree : IBVHQueryCollector<SceneObjectTree.SceneObject>
    {
        #region Private Classes
        //-----------------------------------------------------------------------------
        // Name: SceneObject (Private Class)
        // Desc: Stores useful information for a scene object.
        //-----------------------------------------------------------------------------
        class SceneObject
        {
            #region Public Fields
            public GameObject       gameObject;         // The actual game object
            public Transform        transform;          // Object transform
            public EGameObjectType  objectType;         // Object type
            public Bounds           worldBounds;        // World bounds (mesh, sprite or terrain bounds)
            public Mesh             mesh;               // Mesh
            public RTMesh           rtMesh;             // Used to avoid look-up when raycasting
            public Renderer         renderer;           // Renderer component
            public SpriteRenderer   spriteRenderer;     // Sprite renderer component
            public Terrain          terrain;            // Terrain component
            public TerrainCollider  terrainCollider;    // Terrain collider used for raycasts

            public RTMeshRayHit     rtMeshHit;          // Set inside the collector's 'Raycast' function
            public OBox             worldOBB;           // Cached world OBB
            public bool             worldOBBDirty;      // Is the world OBB dirty?
            #endregion
        }
        #endregion

        #region Private Fields
        Dictionary<GameObject, BinaryAABBTreeNode<SceneObject>> mObjectToNodeMap  = new ();   // Maps a game object to its node inside the tree
        BinaryAABBTree<SceneObject>                             mObjectTree;                  // The object tree

        
        Polyhedron      mRectPolyhedron         = new Polyhedron();     // Used for creating polyhedrons from screen rectangles
        ObjectFilter    mQueryFilter;                                   // Used during queries to set the current filter. This is used inside the explicit interface function implementation.
        bool            mQueryIgnoreBackFaces   = true;                 // Specifies whether mesh back faces are ignored during raycast queries
        Rect            mQueryScreenRect;                               // Used during rect related queries
        Camera          mQueryCamera;                                   // Used during queries which require a camera

        // Buffers used to avoid memory allocations
        List<GameObject>                        mObjectBuffer       = new(1024);
        List<SceneObject>                       mSceneObjectBuffer  = new(4096);
        List<BinaryAABBTreeNode<SceneObject>>   mObjectNodeBuffer   = new(4096);
        List<Vector3>                           mDOPVertexBuffer    = new(128);
        List<Vector2>                           mScreenPtBuffer     = new(128);
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: SceneObjectTree() (Public Constructor)
        // Desc: Default constructor. Creates a tree using a default node padding value.
        //-----------------------------------------------------------------------------
        public SceneObjectTree()
        {
            mObjectTree = new BinaryAABBTree<SceneObject>(0.5f);
        }

        //-----------------------------------------------------------------------------
        // Name: SceneObjectTree() (Public Constructor)
        // Desc: Creates a new scene object tree and initializes the internal tree
        //       with the specified node padding. A small padding value helps reduce
        //       tree rebuilds when objects move slightly.
        // Parm: nodePadding - Extra padding added to leaf nodes to reduce rebuild frequency
        //                     when scene objects move.
        //-----------------------------------------------------------------------------
        public SceneObjectTree(float nodePadding)
        {
            mObjectTree = new BinaryAABBTree<SceneObject>(nodePadding);
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Clear() (Public Function)
        // Desc: Releases all registered scene object state and clears the tree.
        //-----------------------------------------------------------------------------
        public void Clear()
        {
            // Release all scene object resources and cached type entries
            foreach (var pair in mObjectToNodeMap)
                ReleaseSceneObject(pair.Value.data);

            // Clear object map and tree
            mObjectToNodeMap.Clear();
            mObjectTree.Clear();

            // Clear query state
            mQueryFilter        = null;
            mQueryCamera        = null;
            mQueryScreenRect    = default;

            // Clear internal buffers
            mObjectBuffer.Clear();
            mSceneObjectBuffer.Clear();
            mObjectNodeBuffer.Clear();
            mDOPVertexBuffer.Clear();
            mScreenPtBuffer.Clear();
        }

        //-----------------------------------------------------------------------------
        // Name: BoxCollect() (Public Function)
        // Desc: Collects all scene objects whose bounding volumes intersect or are 
        //       fully contained within the specified oriented box.
        // Parm: box         - The oriented box used for intersection testing.
        //       filter      - Optional object filter. Can be null to include all objects.
        //       gameObjects - Receives the collected objects.
        // Rtrn: True if at least one object was collected; false otherwise.
        //-----------------------------------------------------------------------------
        public bool BoxCollect(OBox box, ObjectFilter filter, List<GameObject> gameObjects)
        {
            // Clear output
            gameObjects.Clear();

            // Set query data
            mQueryFilter = filter;

            // Collect scene objects
            if (mObjectTree.BoxCollect(box, this, mSceneObjectBuffer))
            {
                // Store game objects
                int count = mSceneObjectBuffer.Count;
                for (int i = 0; i < count; ++i)
                    gameObjects.Add(mSceneObjectBuffer[i].gameObject);

                // We've collected
                return true;
            }

            // Nothing collected
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: BoxOverlap() (Public Function)
        // Desc: Checks if the specified box overlaps with at least one scene object.
        // Parm: box     - The oriented box used for intersection testing.
        //       filter  - Optional filter used to include/exclude objects. Can be null.
        // Rtrn: True if at least one object is overlapped; false otherwise.
        //-----------------------------------------------------------------------------
        public bool BoxOverlap(OBox box, ObjectFilter filter)
        {
            mQueryFilter = filter;
            return mObjectTree.BoxOverlap(box, this);
        }

        //-----------------------------------------------------------------------------
        // Name: SphereCollect() (Public Function)
        // Desc: Collects all scene objects whose bounding volumes intersect or are 
        //       fully contained within the specified sphere.
        // Parm: sphere      - The sphere used for intersection testing.
        //       filter      - Optional object filter. Can be null to include all objects.
        //       gameObjects - Receives the collected objects.
        // Rtrn: True if at least one object was collected; false otherwise.
        //-----------------------------------------------------------------------------
        public bool SphereCollect(Sphere sphere, ObjectFilter filter, List<GameObject> gameObjects)
        {
            // Clear output
            gameObjects.Clear();

            // Set query data
            mQueryFilter = filter;

            // Collect scene objects
            if (mObjectTree.SphereCollect(sphere, this, mSceneObjectBuffer))
            {
                // Store game objects
                int count = mSceneObjectBuffer.Count;
                for (int i = 0; i < count; ++i)
                    gameObjects.Add(mSceneObjectBuffer[i].gameObject);

                // We've collected
                return true;
            }

            // Nothing collected
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: SphereOverlap() (Public Function)
        // Desc: Checks if the specified sphere overlaps with at least one scene object.
        // Parm: sphere  - The sphere used for intersection testing.
        //       filter  - Optional filter used to include/exclude objects. Can be null.
        // Rtrn: True if at least one object is overlapped; false otherwise.
        //-----------------------------------------------------------------------------
        public bool SphereOverlap(Sphere sphere, ObjectFilter filter)
        {
            mQueryFilter = filter;
            return mObjectTree.SphereOverlap(sphere, this);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Performs a raycast and returns the closest object hit.
        // Parm: ray        - Query ray.
        //       filter     - Object filter. Can be null if no filtering is needed.
        //       objectHit  - Returns the closest object hit.
        // Rtrn: True of there is a hit and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, ObjectFilter filter, out ObjectRayHit objectHit)
        {
            return Raycast(ray, filter, true, out objectHit);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Performs a raycast and returns the closest object hit.
        // Parm: ray             - Query ray.
        //       filter          - Object filter. Can be null if no filtering is needed.
        //       ignoreBackFaces - Specifies whether mesh back faces should be ignored.
        //       objectHit       - Returns the closest object hit.
        // Rtrn: True of there is a hit and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, ObjectFilter filter, bool ignoreBackFaces, out ObjectRayHit objectHit)
        {
            // Init hit
            objectHit     = new ObjectRayHit();
            objectHit.t   = float.MaxValue;

            // Set query data
            mQueryFilter          = filter;
            mQueryIgnoreBackFaces = ignoreBackFaces;

            // Raycast
            if (mObjectTree.Raycast(ray, this, out SceneObject data, out BVHRaycastHit bvhHit))
            {
                // Store object hit based on object type
                switch (data.objectType)
                {
                    case EGameObjectType.Mesh:

                        // Store hit information
                        objectHit.gameObject            = data.gameObject;
                        objectHit.rtMeshHit             = data.rtMeshHit;
                        objectHit.terrainCollider       = null;
                        objectHit.spriteRenderer        = null;
                        objectHit.normal                = bvhHit.normal;
                        objectHit.point                 = data.rtMeshHit.point;
                        objectHit.t                     = bvhHit.t;
                        break;

                    case EGameObjectType.Sprite:

                        // Store hit info
                        objectHit.gameObject            = data.gameObject;
                        objectHit.rtMeshHit.rtMesh      = null;
                        objectHit.terrainCollider       = null;
                        objectHit.spriteRenderer        = data.spriteRenderer;
                        objectHit.normal                = bvhHit.normal;
                        objectHit.point                 = ray.GetPoint(bvhHit.t);
                        objectHit.t                     = bvhHit.t;
                        break;

                    case EGameObjectType.Terrain:

                        // Store hit into
                        objectHit.gameObject            = data.gameObject;
                        objectHit.rtMeshHit.rtMesh      = null;
                        objectHit.spriteRenderer        = null;
                        objectHit.normal                = bvhHit.normal;
                        objectHit.point                 = ray.GetPoint(bvhHit.t);
                        objectHit.t                     = bvhHit.t;
                        break;
                }
            }

            // Return true if a game object was hit
            return objectHit.gameObject != null;
        }

        //-----------------------------------------------------------------------------
        // Name: ScreenRectCollectInside() (Public Function)
        // Desc: Collects all objects that lie completely within the specified screen
        //       rectangle.
        // Parm: screenRect  - The query screen rectangle.
        //       camera      - The camera that sees the rectangle.
        //       filter      - Object filter. Can be null if no filtering is needed.
        //       gameObjects - Returns all objects that lie completely within the rectangle.
        // Rtrn: True if at least one object was collected and false otherwise.
        //-----------------------------------------------------------------------------
        public bool ScreenRectCollectInside(Rect screenRect, Camera camera, ObjectFilter filter, List<GameObject> gameObjects)
        {
            // Clear object list
            gameObjects.Clear();

            // Create a polyhedron from the screen rectangle
            if (!mRectPolyhedron.FromScreenRect(screenRect, camera))
                return false;

            // Set query data
            mQueryFilter        = filter;
            mQueryScreenRect    = screenRect;
            mQueryCamera        = camera;

            // Collect
            if (mObjectTree.PolyhedronCollect(mRectPolyhedron, this, mSceneObjectBuffer))
            {
                // Store game objects
                int count = mSceneObjectBuffer.Count;
                for (int i = 0; i < count; ++i)
                    gameObjects.Add(mSceneObjectBuffer[i].gameObject);

                // We've collected
                return true;
            }

            // Nothing collected
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: OnObjectComponentsChanged() (Public Function)
        // Desc: Must be called whenever adding or removing components to/from a game object.
        // Parm: gameObject - The game object whose components have changed.
        //-----------------------------------------------------------------------------
        public void OnObjectComponentsChanged(GameObject gameObject)
        {
            // Unregister the object (e.g. maybe it was a mesh object previously, but now it's an empty object). 
            UnregisterObject(gameObject);

            // Register object (e.g. maybe it was an empty object previously and now it's a mesh object).
            RegisterObject(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: RegisterObject() (Public Function)
        // Desc: Registers the specified object with the tree. If the object is already
        //       registered, the function has no effect.
        // Parm: gameObject - The object which must be registered with the tree.
        //-----------------------------------------------------------------------------
        public void RegisterObject(GameObject gameObject)
        {
            // No-op?
            if (mObjectToNodeMap.ContainsKey(gameObject))
                return;

            // Create a scene object and insert it into the tree
            var sceneObject = CreateSceneObject(gameObject);
            if (sceneObject != null)
            {
                var objectNode  = mObjectTree.CreateLeaf(sceneObject, sceneObject.worldBounds);
                mObjectToNodeMap.Add(gameObject, objectNode);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: UnregisterObject() (Public Function)
        // Desc: Removes the specified object from the tree.
        // Parm: gameObject - The game object which must be removed from the tree.
        //-----------------------------------------------------------------------------
        public void UnregisterObject(GameObject gameObject)
        {
            // Try getting the object's node
            if (mObjectToNodeMap.TryGetValue(gameObject, out var objectNode))
            {
                // Cache scene object before destroying its node
                SceneObject sceneObject = objectNode.data;

                // Destroy leaf node and remove node from map
                mObjectTree.DestroyLeaf(objectNode);
                mObjectToNodeMap.Remove(gameObject);

                // Release scene object resources
                ReleaseSceneObject(sceneObject);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: OnObjectTransformChanged() (Public Function)
        // Desc: Must be called when the transform of an object changes.
        // Parm: gameObject - The game object whose transform changed.
        //-----------------------------------------------------------------------------
        public void OnObjectTransformChanged(GameObject gameObject)
        {
            // Collect all objects in the hierarchy
            gameObject.CollectMeAndChildren(true, true, mObjectBuffer);

            // Update object leaf nodes
            int count = mObjectBuffer.Count;
            for (int i = 0; i < count; ++i)
            {
                // Get scene object leaf node
                if (mObjectToNodeMap.TryGetValue(mObjectBuffer[i], out BinaryAABBTreeNode<SceneObject> leaf))
                {
                    // World OBB is now dirty
                    leaf.data.worldOBBDirty = true;

                    // Calculate object bounds and update the leaf
                    leaf.data.worldBounds = CalclculateWorldBounds(leaf.data);
                    mObjectTree.OnLeafUpdated(leaf, leaf.data.worldBounds);
                }
            }
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: ReleaseSceneObject() (Private Function)
        // Desc: Releases resources owned by the specified scene object and clears all
        //       cached references.
        // Parm: sceneObject - The scene object whose resources must be released.
        //-----------------------------------------------------------------------------
        void ReleaseSceneObject(SceneObject sceneObject)
        {
            // Invalid scene object?
            if (sceneObject == null)
                return;

            // Clear cached references
            sceneObject.gameObject      = null;
            sceneObject.transform       = null;
            sceneObject.mesh            = null;
            sceneObject.rtMesh          = null;
            sceneObject.renderer        = null;
            sceneObject.spriteRenderer  = null;
            sceneObject.terrain         = null;
            sceneObject.terrainCollider = null;

            // Clear cached value data
            sceneObject.objectType      = default;
            sceneObject.worldBounds     = default;
            sceneObject.rtMeshHit       = default;
            sceneObject.worldOBB        = default;
            sceneObject.worldOBBDirty   = false;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateSceneObject() (Public Function)
        // Desc: Creates an 'SceneObject' from the specified 'GameObject'.
        // Parm: gameObject - The 'GameObject' which must be turned into a 'SceneObject'.
        // Rtrn: 'SceneObject' instance for the specified 'GameObject'.
        //-----------------------------------------------------------------------------
        SceneObject CreateSceneObject(GameObject gameObject)
        {
            // Get object type and validate it
            EGameObjectType objectType = gameObject.GetGameObjectType();
            if (objectType != EGameObjectType.Mesh &&
                objectType != EGameObjectType.Sprite &&
                objectType != EGameObjectType.Terrain)
                return null;

            // Create the scene object
            SceneObject sceneObject     = new SceneObject();
            sceneObject.gameObject      = gameObject;
            sceneObject.objectType      = objectType;
            sceneObject.transform       = gameObject.transform;
            sceneObject.mesh            = gameObject.GetMesh();
            sceneObject.renderer        = gameObject.GetRenderer();
            sceneObject.spriteRenderer  = sceneObject.renderer as SpriteRenderer;
            sceneObject.terrain         = gameObject.GetTerrain();
            sceneObject.worldBounds     = CalclculateWorldBounds(sceneObject);;
    
            // Acquire RTMesh
            if (sceneObject.mesh)
                sceneObject.rtMesh = RTMeshManager.GetRTMesh(sceneObject.mesh);

            // Cache terrain collider
            if (sceneObject.terrain)
                sceneObject.terrainCollider = sceneObject.gameObject.GetTerrainCollider();

            // Calculate world OBB
            sceneObject.worldOBB        = CalculateWorldOBB(sceneObject);

            // Return scene object
            return sceneObject;
        }

        //-----------------------------------------------------------------------------
        // Name: CalclculateWorldBounds() (Private Function)
        // Desc: Calculates the world-space bounds for the specified 'SceneObject'.
        // Parm: sceneObject - Query 'SceneObject'.
        // Rtrn: The scene object's world-space bounds.
        //-----------------------------------------------------------------------------
        Bounds CalclculateWorldBounds(SceneObject sceneObject)
        {
            Box box;

            // Calculate bounds based on object type
            switch (sceneObject.objectType)
            {
                case EGameObjectType.Sprite:

                    box = sceneObject.spriteRenderer.CalculateModelAABB();
                    box.Transform(sceneObject.transform.localToWorldMatrix);
                    return box.ToBounds();

                case EGameObjectType.Mesh:

                    box = new Box(sceneObject.mesh.bounds);
                    box.Transform(sceneObject.transform.localToWorldMatrix);
                    return box.ToBounds();

                case EGameObjectType.Terrain:

                    var terrainData = sceneObject.terrain.terrainData;
                    return new Bounds(sceneObject.transform.position + terrainData.size / 2.0f, terrainData.size);

                default:

                    return new Bounds();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateWorldOBB() (Private Function)
        // Desc: Calculates the world-space OBB for the specified 'SceneObject'.
        // Parm: sceneObject - The 'SceneObject' to process.
        // Rtrn: The OBB representing the object's world-space bounds or an invalid OBB
        //       if the object's key component (renderer, terrain etc) is not enabled.
        //-----------------------------------------------------------------------------
        OBox CalculateWorldOBB(SceneObject sceneObject)
        {
            // Prepare model-space bounds
            Bounds modelAABB;
            switch (sceneObject.objectType)
            {
                case EGameObjectType.Mesh:

                    // Is the component enabled?
                    if (!sceneObject.renderer.enabled)
                        return OBox.GetInvalid();

                    // Calculate model AABB
                    modelAABB = sceneObject.mesh.bounds;
                    break;

                case EGameObjectType.Sprite:

                    // Is the component enabled?
                    var spriteRenderer = sceneObject.renderer as SpriteRenderer;
                    if (!spriteRenderer.enabled) return OBox.GetInvalid();

                    // Calculate model AABB
                    modelAABB = spriteRenderer.CalculateModelAABB().ToBounds();

                    // Apply small Z padding for sprites
                    if (modelAABB.size.z <= 0.0001f)
                    {
                        Vector3 paddedSize = modelAABB.size;
                        paddedSize.z = 0.01f;
                        modelAABB.size = paddedSize;
                    }

                    break;

                case EGameObjectType.Terrain:

                    // Is the component enabled?
                    var terrain = sceneObject.terrain;
                    if (!terrain.enabled) return OBox.GetInvalid();

                    // Calculate model AABB
                    var size    = terrain.terrainData.size;
                    modelAABB = new Bounds(size * 0.5f, size);
                    break;

                default:

                    return default;
            }

            // Construct world-space OBB
            return new OBox(modelAABB, sceneObject.transform);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Private Function)
        // Desc: Determines whether the ray hits the node's data during a raycast query.
        // Parm: ray    - Query ray.
        //       data   - Node data.
        //       bvhHit - Returns the BVH hit data.
        // Rtrn: True if the ray hits the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool IBVHQueryCollector<SceneObject>.Raycast(Ray ray, SceneObject data, out BVHRaycastHit bvhHit)
        {
            // Clear output
            bvhHit = new BVHRaycastHit();

            // Ignore inactive objects
            GameObject gameObject   = data.gameObject;
            if (!gameObject.activeInHierarchy) 
                return false;

            // If we have a filter, use it
            if (mQueryFilter != null && !mQueryFilter.Pass(gameObject, data.objectType))
                return false;

            // What kind of object are we dealing with?
            switch (data.objectType)
            {
                case EGameObjectType.Mesh:

                    // Is the renderer enabled?
                    if (data.renderer.enabled && data.rtMesh != null)
                    {
                        // Raycast 'RTMesh' 
                        if (data.rtMesh.Raycast(ray, gameObject.transform, mQueryIgnoreBackFaces, out data.rtMeshHit))
                        {
                            // Store hit information
                            bvhHit.normal   = data.rtMeshHit.normal;
                            bvhHit.t        = data.rtMeshHit.t;

                            // We have a hit!
                            return true;
                        }
                    }
                    break;

                case EGameObjectType.Sprite:

                    // Is the renderer enabled?
                    var sr = data.spriteRenderer;
                    if (sr.enabled && sr.sprite && sr.sprite.texture)
                    {
                        // Raycast against the sprite's plane
                        Vector3 spriteExtents   = Vector3.Scale(gameObject.transform.lossyScale, sr.sprite.bounds.extents);
                        Plane spritePlane       = new Plane(gameObject.transform.forward, gameObject.transform.position);
                        if (spritePlane.Raycast(ray, out float t))
                        {
                            // Get intersection point and store direction from object position to intersection point
                            Vector3 pt      = ray.origin + ray.direction * t;
                            Vector3 toPt    = (pt - gameObject.transform.position);

                            // Check if the intersection point lies within the sprite's area
                            // Check X axis
                            Vector3 right   = gameObject.transform.right;
                            float dot       = (right.x * toPt.x + right.y * toPt.y + right.z * toPt.z);
                            if (dot > spriteExtents.x || dot < -spriteExtents.x)
                                return false;

                            // Check Y axis
                            Vector3 up      = gameObject.transform.up;
                            dot             = (up.x * toPt.x + up.y * toPt.y + up.z * toPt.z);
                            if (dot > spriteExtents.y || dot < -spriteExtents.y)
                                return false;

                            // Store hit information
                            bvhHit.normal   = spritePlane.normal;
                            bvhHit.t        = t;

                            // We have a hit!
                            return true;
                        }
                    }
                    break;

                case EGameObjectType.Terrain:

                    // Raycast using the terrain collider
                    TerrainCollider terrainCollider = data.terrainCollider;
                    if (terrainCollider != null)
                    {
                        // Raycast terrain collider and update hit info if hit is closer than what we have so far
                        if (terrainCollider.Raycast(ray, out RaycastHit rayHit, float.MaxValue) && 
                            rayHit.distance < bvhHit.t)
                        {
                            // Store hit information
                            bvhHit.normal   = rayHit.normal;
                            bvhHit.t        = rayHit.distance;

                            // We have a hit!
                            return true;
                        }
                    }
                    break;
            }

            // No hit
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: BoxOverlap() (Private Function)
        // Desc: Determines whether the box overlaps with the node's data.
        // Parm: box  - Query box.
        //       data - Node data.
        // Rtrn: True if the box overlaps with the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool IBVHQueryCollector<SceneObject>.BoxOverlap(OBox box, SceneObject data)
        {
            // Ignore inactive objects
            GameObject gameObject = data.gameObject;
            if (!gameObject.activeInHierarchy)
                return false;

            // If we have a filter, use it
            if (mQueryFilter != null && !mQueryFilter.Pass(gameObject, data.objectType))
                return false;

            // Compute OBB if necessary
            if (data.worldOBBDirty)
            {
                data.worldOBB = CalculateWorldOBB(data);
                data.worldOBBDirty = false;
            }

            // Return intersection result
            return data.worldOBB.isValid && data.worldOBB.TestBox(box);
        }

        //-----------------------------------------------------------------------------
        // Name: SphereOverlap() (Private Function)
        // Desc: Determines whether the sphere overlaps with the node's data.
        // Parm: sphere  - Query sphere.
        //       data    - Node data.
        // Rtrn: True if the sphere overlaps with the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool IBVHQueryCollector<SceneObject>.SphereOverlap(Sphere sphere, SceneObject data)
        {
            // Ignore inactive objects
            GameObject gameObject = data.gameObject;
            if (!gameObject.activeInHierarchy)
                return false;

            // If we have a filter, use it
            if (mQueryFilter != null && !mQueryFilter.Pass(gameObject, data.objectType))
                return false;

            // Compute OBB if necessary
            if (data.worldOBBDirty)
            {
                data.worldOBB = CalculateWorldOBB(data);
                data.worldOBBDirty = false;
            }

            // Return intersection result
            return data.worldOBB.isValid && data.worldOBB.TestSphere(sphere.center, sphere.radius);
        }

        //-----------------------------------------------------------------------------
        // Name: PolyhedronOverlap() (Private Function)
        // Desc: Determines whether the polyhedron overlaps with the node's data.
        // Parm: polyhedron  - Query polyhedron.
        //       data        - Node data.
        // Rtrn: True if the polyhedron overlaps with the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool IBVHQueryCollector<SceneObject>.PolyhedronOverlap(Polyhedron polyhedron, SceneObject data)
        {
            // Ignore inactive objects
            GameObject gameObject = data.gameObject;
            if (!gameObject.activeInHierarchy)
                return false;

            // If we have a filter, use it
            if (mQueryFilter != null && !mQueryFilter.Pass(gameObject, data.objectType))
                return false;

            // Is this a mesh?
            if (data.rtMesh != null)
            {
                // Get DOP verts in world space
                data.rtMesh.kdop.CollectVerts(gameObject.transform.localToWorldMatrix, mDOPVertexBuffer);

                // Convert world verts to screen verts and create rectangle
                mQueryCamera.WorldToScreenPoints(mDOPVertexBuffer, mScreenPtBuffer);
                Rect objectRect = RectEx.FromPoints(mScreenPtBuffer);
                if (!objectRect.HasPositiveSize()) return false;

                // Does the screen rect contain the object rect?
                return mQueryScreenRect.TestRectInside(objectRect);               
            }
            else
            {
                // Not a mesh. Use world OBB and calculate its screen rectangle.
                if (data.worldOBBDirty)
                {
                    data.worldOBB = CalculateWorldOBB(data);
                    data.worldOBBDirty = false;
                }
                if (data.worldOBB.isValid)
                {
                    // Calculate object rect
                    Rect objectRect = data.worldOBB.CalculateScreenRect(mQueryCamera);
                    if (!objectRect.HasPositiveSize()) 
                        return false;

                    // Does the screen rect contain the object rect?
                    return mQueryScreenRect.TestRectInside(objectRect);
                }
            }

            // Not overlapped
            return false;
        }
        #endregion
    }
    #endregion
}