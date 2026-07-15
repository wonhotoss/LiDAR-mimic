using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //----------------------------------------------------------------------------- 
    // Name: BVHRaycastHit (Public Interface)
    // Desc: Stores raycast hit information during BVH raycast queries.
    //----------------------------------------------------------------------------- 
    public struct BVHRaycastHit
    {
        #region Public Fields
        public Vector3 normal;  // Hit normal
        public float   t;       // Hit distance
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: BVHNodeRayHit<T> (Public Struct)
    // Desc: Stores information about a BVH node hit, including the hit node data
    //       and the corresponding BVH ray hit information. Used when collecting
    //       multiple ray hits during 'RaycastAll()' queries.
    // Parm: T - Type of data stored in the BVH leaf nodes.
    //-----------------------------------------------------------------------------
    public struct BVHNodeRayHit<T>
        where T : class, new()
    {
        #region Public Fields
        public T                data;   // Node data which was hit
        public BVHRaycastHit    hit;    // BVH ray hit information (normal, t)
        #endregion
    }
    #endregion

    #region Public Interfaces
    //----------------------------------------------------------------------------- 
    // Name: IBVHQueryCollector<T> (Public Interface)
    // Desc: Collector interface used to collect node data during BVH tree queries.
    // Parm: T - Type of data stored in the BVH nodes.
    //----------------------------------------------------------------------------- 
    public interface IBVHQueryCollector<T>
        where T : class, new()
    {
        #region Public Functons
        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Determines whether the ray hits the node's data during a raycast query.
        // Parm: ray    - Query ray.
        //       data   - Node data.
        //       bvhHit - Returns the BVH hit data.
        // Rtrn: True if the ray hits the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool Raycast(Ray ray, T data, out BVHRaycastHit bvhHit);

        //-----------------------------------------------------------------------------
        // Name: BoxOverlap() (Public Function)
        // Desc: Determines whether the box overlaps with the node's data.
        // Parm: box  - Query box.
        //       data - Node data.
        // Rtrn: True if the box overlaps with the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool BoxOverlap(OBox box, T data);

        //-----------------------------------------------------------------------------
        // Name: SphereOverlap() (Public Function)
        // Desc: Determines whether the sphere overlaps with the node's data.
        // Parm: sphere  - Query sphere.
        //       data    - Node data.
        // Rtrn: True if the sphere overlaps with the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool SphereOverlap(Sphere sphere, T data);

        //-----------------------------------------------------------------------------
        // Name: PolyhedronOverlap() (Public Function)
        // Desc: Determines whether the polyhedron overlaps with the node's data.
        // Parm: polyhedron  - Query polyhedron.
        //       data        - Node data.
        // Rtrn: True if the polyhedron overlaps with the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool PolyhedronOverlap(Polyhedron polyhedron, T data);
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: BinaryAABBTreeNodePool (Public Class)
    // Desc: Expandable free list-based memory pool for 'BinaryAABBTreeNode<T>'.
    //       Supports efficient allocation and reuse of nodes without GC.
    //-----------------------------------------------------------------------------
    public class BinaryAABBTreeNodePool<T>
        where T : class, new()
    {
        #region Private Fields
        List<BinaryAABBTreeNode<T>> mAllNodes = new List<BinaryAABBTreeNode<T>>();  // All allocated nodes (tracking only)
        BinaryAABBTreeNode<T>       mNextFree;                                      // Next free node
        int                         mInitialCapacity;                               // Number of nodes to allocate when expanding
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: BinaryAABBTreeNodePool() (Public Constructor)
        // Desc: Creates the pool with the specified initial capacity.
        // Parm: initialCapacity - Initial number of nodes to preallocate.
        //-----------------------------------------------------------------------------
        public BinaryAABBTreeNodePool(int initialCapacity = 1024)
        {
            mInitialCapacity = initialCapacity;
            GrowPool(mInitialCapacity);
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: AllocNode() (Public Function)
        // Desc: Retrieves a free node from the pool, expanding if necessary.
        // Rtrn: The allocated 'BinaryAABBTreeNode<T>'.
        //-----------------------------------------------------------------------------
        public BinaryAABBTreeNode<T> AllocNode()
        {
            // Grow the pool if needed
            if (mNextFree == null)
                GrowPool(mInitialCapacity);

            // Allocate new node
            BinaryAABBTreeNode<T> node = mNextFree;
            mNextFree = node.nextFree;

            // Reset node state
            ResetNode(node);
            return node;
        }

        //-----------------------------------------------------------------------------
        // Name: FreeNode() (Public Function)
        // Desc: Clears and returns a node to the pool for future reuse.
        // Parm: node - The node to recycle.
        //-----------------------------------------------------------------------------
        public void FreeNode(BinaryAABBTreeNode<T> node)
        {
            // Clear node state before recycling
            ResetNode(node);

            // Add node to free list
            node.nextFree = mNextFree;
            mNextFree = node;
        }

        //-----------------------------------------------------------------------------
        // Name: Clear() (Public Function)
        // Desc: Clears all node state and rebuilds the free list while retaining the
        //       pool's allocated node capacity.
        //-----------------------------------------------------------------------------
        public void Clear()
        {
            // Reset free list
            mNextFree = null;

            // Reset and recycle all allocated nodes
            int numNodes = mAllNodes.Count;
            for (int i = 0; i < numNodes; ++i)
            {
                BinaryAABBTreeNode<T> node = mAllNodes[i];
                ResetNode(node);

                node.nextFree = mNextFree;
                mNextFree = node;
            }
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: GrowPool() (Private Function)
        // Desc: Allocates and links a batch of new nodes into the free list.
        // Parm: count - Number of new nodes to create.
        //-----------------------------------------------------------------------------
        void GrowPool(int count)
        {
            // Grow
            for (int i = 0; i < count; ++i)
            {
                // Create the node and add it to the list
                BinaryAABBTreeNode<T> node = new BinaryAABBTreeNode<T>();
                node.nextFree = mNextFree;
                mNextFree = node;
                mAllNodes.Add(node);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: ResetNode() (Private Function)
        // Desc: Clears all node state so recycled nodes do not retain references to
        //       old tree data or tree structure.
        // Parm: node - The node to reset.
        //-----------------------------------------------------------------------------
        void ResetNode(BinaryAABBTreeNode<T> node)
        {
            node.bounds             = default;
            node.data               = null;
            node.parent             = null;
            node.child0             = null;
            node.child1             = null;
            node.nextFree           = null;
            node.pendingIntegration = false;
            node.pendingDeletion    = false;
        }
        #endregion
    }

    //----------------------------------------------------------------------------- 
    // Name: BinaryAABBTreeNode<T> (Public Class)
    // Desc: Represents an AABB node in a binary AABB tree.
    // Parm: T - The type of data stored in leaf nodes.
    //----------------------------------------------------------------------------- 
    public class BinaryAABBTreeNode<T>
        where T : class, new()
    {
        #region Public Fields
        public Bounds                       bounds;             // AABB bounds
        public T                            data;               // Data (leaf nodes only)
        public BinaryAABBTreeNode<T>        parent;             // Parent node
        public BinaryAABBTreeNode<T>        child0;             // First child node
        public BinaryAABBTreeNode<T>        child1;             // Second child node
        public BinaryAABBTreeNode<T>        nextFree;           // Free list support
        public bool                         pendingIntegration; // Is this node pending integration?
        public bool                         pendingDeletion;    // Is this node pending deletion?
        #endregion

        #region Public Functions
        //----------------------------------------------------------------------------- 
        // Name: Refit() (Public Function)
        // Desc: Must be called for non-leaf nodes in order to ensure that their AABB
        //       volume encloses the 2 child nodes.
        //----------------------------------------------------------------------------- 
        public void Refit()
        {
            // Combine the child AABBs to form the new AABB
            bounds = child0.bounds;
            bounds.Encapsulate(child1.bounds);
        }

        //----------------------------------------------------------------------------- 
        // Name: ContainsNode() (Public Function)
        // Desc: Checks whether the given node is fully enclosed within this node's AABB.
        // Parm: node       - The node to test.
        // Parm: excessOut  - The amount by which the child exceeds the parent's bounds.
        // Rtrn: True if the node is fully enclosed, false otherwise.
        //----------------------------------------------------------------------------- 
        public bool ContainsNode(BinaryAABBTreeNode<T> node, out float excessOut)
        {
            excessOut = 0.0f;
            if (bounds.Contains(node.bounds.min) && bounds.Contains(node.bounds.max))
                return true;

            // Expand current bounds to include the child node's bounds
            Bounds combined = bounds;
            combined.Encapsulate(node.bounds);

            // Compute how much larger the new bounds are
            excessOut = (combined.size - bounds.size).magnitude;
            return false;
        }
        #endregion
    }

    //----------------------------------------------------------------------------- 
    // Name: BinaryAABBTree<T> (Public Class)
    // Desc: Implements a binary AABB tree which can be used to speed up queries
    //       such as raycasts and overlap tests.
    // Parm: T - The type of data stored in leaf nodes.
    //----------------------------------------------------------------------------- 
    public class BinaryAABBTree<T>
        where T : class, new()
    {
        #region Private Fields
        Vector3                         mPadding;                       // Padding value added to the leaf node bounds
        BinaryAABBTreeNode<T>[]         mNodeStack;                     // Node stack used for manual traversal
        int                             mNodeStackLength;               // Current stack size
        BinaryAABBTreeNode<T>           mRoot;                          // Root node
        BinaryAABBTreeNodePool<T>       mNodePool           = new();    // Node pool
        BinaryAABBTreeNode<T>[]         mIntegrationLeaves  = new BinaryAABBTreeNode<T>[2048];  // List of leaves waiting to be integrated
        int                             mIntegrationLeafCount;                                  // Number of leaves waiting to be integrated        
        #endregion

        #region Public Constructors
        //----------------------------------------------------------------------------- 
        // Name: BinaryAABBTree() (Public Constructor)
        // Desc: Create a binary AABB tree with the specified bounds padding.
        // Parm: padding - Value to expand leaf AABB size to avoid frequent reinsertion.
        //----------------------------------------------------------------------------- 
        public BinaryAABBTree(float padding)
        {
            mPadding            = Vector3Ex.FromValue(Mathf.Max(0.0f, padding));
            mNodeStack          = new BinaryAABBTreeNode<T>[4096];
            mNodeStackLength    = 0;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Clear() (Public Function)
        // Desc: Clears all tree nodes and transient query state while retaining the
        //       internal allocation capacity for future reuse.
        //-----------------------------------------------------------------------------
        public void Clear()
        {
            // Detach tree root
            mRoot = null;

            // Reset all pooled nodes
            mNodePool.Clear();

            // Clear pending integration references
            Array.Clear(mIntegrationLeaves, 0, mIntegrationLeaves.Length);
            mIntegrationLeafCount = 0;

            // Clear traversal stack references
            Array.Clear(mNodeStack, 0, mNodeStack.Length);
            mNodeStackLength = 0;
        }

        //----------------------------------------------------------------------------- 
        // Name: CreateLeaf() (Public Function)
        // Desc: Creates and inserts a new leaf node into the tree.
        // Parm: data   - The data to store in the leaf.
        //       bounds - The bounds of the leaf node.
        // Rtrn: The new leaf node.
        //----------------------------------------------------------------------------- 
        public BinaryAABBTreeNode<T> CreateLeaf(T data, Bounds bounds)
        {
            // Validate call
            if (data == null) return null;

            // Create and configure the leaf node
            BinaryAABBTreeNode<T> leaf = mNodePool.AllocNode();
            leaf.data   = data;
            leaf.bounds = bounds;

            // Insert the new leaf into the tree
            InsertLeaf(leaf);
            return leaf;
        }

        //----------------------------------------------------------------------------- 
        // Name: DestroyLeaf() (Public Function)
        // Desc: Removes the specified leaf node from the tree.
        // Parm: leaf - The leaf node to remove.
        //----------------------------------------------------------------------------- 
        public void DestroyLeaf(BinaryAABBTreeNode<T> leaf)
        {
            // No-op?
            if (leaf == null) return;

            // If it's pending integration, don't try to remove it again
            if (leaf.pendingIntegration)
                leaf.pendingDeletion = true;
            else
            {
                RemoveLeaf(leaf);
                mNodePool.FreeNode(leaf);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Performs a raycast and returns the closest hit node data.
        // Parm: ray       - Query ray.
        //       collector - Used to check if the ray intersects node data. Must be valid.
        //       data      - Returns the closest hit node data.
        //       bvhHit    - Returns the BVH hit data.
        // Rtrn: True if the ray hits any node data; false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, IBVHQueryCollector<T> collector, out T data, out BVHRaycastHit bvhHit)
        {
            // Clear output
            data        = null;
            bvhHit      = new BVHRaycastHit();
            bvhHit.t    = float.MaxValue;

            // Validate args
            if (collector == null)
                return false;

            // Make sure the tree is ready
            ProcessIntegrationLeaves();
            if (mRoot == null)
                return false;

            // Push the root node onto the stack
            mNodeStack[mNodeStackLength++] = mRoot;

            // While there are still nodes onto the stack ...
            while (mNodeStackLength != 0)
            {
                // Pop node off the stack
                BinaryAABBTreeNode<T> node = mNodeStack[--mNodeStackLength];

                // Skip if node AABB does not intersect the ray
                if (!node.bounds.IntersectRay(ray))
                    continue;

                // Check if node is a leaf
                if (node.data != null)
                {
                    // Raycast leaf
                    if (collector.Raycast(ray, node.data, out BVHRaycastHit rayHit))
                    {
                        // Is this hit closer?
                        if (rayHit.t < bvhHit.t)
                        {
                            bvhHit = rayHit;
                            data = node.data;
                        }
                    }
                    continue;
                }

                // Ensure enough space for 2 children
                if (mNodeStackLength + 2 > mNodeStack.Length)
                {
                    var newStack = new BinaryAABBTreeNode<T>[mNodeStack.Length * 2];
                    Array.Copy(mNodeStack, newStack, mNodeStackLength);
                    mNodeStack = newStack;
                }

                // Push both children
                mNodeStack[mNodeStackLength++] = node.child0;
                mNodeStack[mNodeStackLength++] = node.child1;
            }

            // Return result
            return data != null;
        }

        //-----------------------------------------------------------------------------
        // Name: RaycastAll() (Public Function)
        // Desc: Performs a raycast against the entire tree and collects all leaf node
        //       data that are hit by the specified ray. Unlike 'Raycast()', this function
        //       does not stop at the closest hit, but instead gathers every valid hit.
        // Parm: ray       - Query ray.
        //       collector - Collector used to test ray intersection against leaf data.
        //                   Must be valid.
        //       hits      - List which receives all ray hits. The list is cleared
        //                   before being populated.
        // Rtrn: True if at least one node data was hit; false otherwise.
        //-----------------------------------------------------------------------------
        public bool RaycastAll(Ray ray, IBVHQueryCollector<T> collector, List<BVHNodeRayHit<T>> hits)
        {
            // Clear output list
            hits.Clear();

            // Validate args
            if (collector == null)
                return false;

            // Ensure the tree is up-to-date (integrate pending leaves)
            ProcessIntegrationLeaves();

            // If tree is empty, nothing to do
            if (mRoot == null)
                return false;

            // Push root node onto traversal stack
            mNodeStack[mNodeStackLength++] = mRoot;

            // Traverse tree using manual stack (non-recursive)
            while (mNodeStackLength != 0)
            {
                // Pop node from stack
                BinaryAABBTreeNode<T> node = mNodeStack[--mNodeStackLength];

                // Early reject: if ray does not intersect node AABB, skip entire subtree
                if (!node.bounds.IntersectRay(ray))
                    continue;

                // Leaf node?
                if (node.data != null)
                {
                    // Perform precise raycast against leaf data
                    if (collector.Raycast(ray, node.data, out BVHRaycastHit rayHit))
                    {
                        // Store hit
                        BVHNodeRayHit<T> nodeHit;
                        nodeHit.data = node.data;
                        nodeHit.hit  = rayHit;

                        hits.Add(nodeHit);
                    }

                    // Continue traversal
                    continue;
                }

                // Ensure enough stack space
                if (mNodeStackLength + 2 > mNodeStack.Length)
                {
                    var newStack = new BinaryAABBTreeNode<T>[mNodeStack.Length * 2];
                    Array.Copy(mNodeStack, newStack, mNodeStackLength);
                    mNodeStack = newStack;
                }

                // Push both children for further traversal
                mNodeStack[mNodeStackLength++] = node.child0;
                mNodeStack[mNodeStackLength++] = node.child1;
            }

            // Return true if at least one hit was collected
            return hits.Count != 0;
        }

        //-----------------------------------------------------------------------------
        // Name: BoxCollect() (Public Function)
        // Desc: Collects the data from all nodes that intersect or are fully contained
        //       within the specified box.
        // Parm: box        - Query box.
        //       collector  - Used to check if the box overlaps with the node data.
        //                    Must be valid.
        //       data       - Returns the collected node data.
        // Rtrn: True if node data was collected.
        //-----------------------------------------------------------------------------
        public bool BoxCollect(OBox box, IBVHQueryCollector<T> collector, List<T> data)
        {
            // Clear output
            data.Clear();
            
            // Validate args
            if (collector == null || !box.isValid)
                return false;

            // Make sure the tree is ready
            ProcessIntegrationLeaves();
            if (mRoot == null)
                return false;

            // Push the root node onto the stack
            mNodeStack[mNodeStackLength++] = mRoot;

            // While there are still nodes onto the stack ...
            while (mNodeStackLength != 0)
            {
                // Pop node off the stack
                BinaryAABBTreeNode<T> node = mNodeStack[--mNodeStackLength];

                // Skip if node AABB does not intersect the box
                if (!box.TestBox(node.bounds))
                    continue;

                // Check if node is a leaf
                if (node.data != null)
                {
                    // Overlap leaf
                    if (collector.BoxOverlap(box, node.data))
                        data.Add(node.data);
                    continue;
                }

                // Ensure enough space for 2 children
                if (mNodeStackLength + 2 > mNodeStack.Length)
                {
                    var newStack = new BinaryAABBTreeNode<T>[mNodeStack.Length * 2];
                    Array.Copy(mNodeStack, newStack, mNodeStackLength);
                    mNodeStack = newStack;
                }

                // Push both children
                mNodeStack[mNodeStackLength++] = node.child0;
                mNodeStack[mNodeStackLength++] = node.child1;
            }

            // Return result
            return data.Count != 0;
        }

        //-----------------------------------------------------------------------------
        // Name: BoxOverlap() (Public Function)
        // Desc: Checks if the specified box overlaps with the node data.
        // Parm: box        - Query box.
        //       collector  - Used to check if the box overlaps with the node data.
        //                    Must be valid.
        // Rtrn: True if node data was collected.
        //-----------------------------------------------------------------------------
        public bool BoxOverlap(OBox box, IBVHQueryCollector<T> collector)
        {
            // Is the tree empty?
            if (mRoot == null)
                return false;

            // Validate args
            if (collector == null || !box.isValid)
                return false;

            // Make sure the tree is ready
            ProcessIntegrationLeaves();
            if (mRoot == null)
                return false;

            // Push the root node onto the stack
            mNodeStack[mNodeStackLength++] = mRoot;

            // While there are still nodes onto the stack ...
            while (mNodeStackLength != 0)
            {
                // Pop node off the stack
                BinaryAABBTreeNode<T> node = mNodeStack[--mNodeStackLength];

                // Skip if node AABB does not intersect the box
                if (!box.TestBox(node.bounds))
                    continue;

                // Check if node is a leaf
                if (node.data != null)
                {
                    // Overlap leaf
                    if (collector.BoxOverlap(box, node.data))
                        return true;

                    // Move on
                    continue;
                }

                // Ensure enough space for 2 children
                if (mNodeStackLength + 2 > mNodeStack.Length)
                {
                    var newStack = new BinaryAABBTreeNode<T>[mNodeStack.Length * 2];
                    Array.Copy(mNodeStack, newStack, mNodeStackLength);
                    mNodeStack = newStack;
                }

                // Push both children
                mNodeStack[mNodeStackLength++] = node.child0;
                mNodeStack[mNodeStackLength++] = node.child1;
            }

            // No overlap
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: SphereCollect() (Public Function)
        // Desc: Collects the data from all nodes that intersect or are fully contained
        //       within the specified sphere.
        // Parm: sphere     - Query sphere.
        //       collector  - Used to check if the sphere overlaps with the node data.
        //                    Must be valid.
        //       data       - Returns the collected node data.
        // Rtrn: True if node data was collected.
        //-----------------------------------------------------------------------------
        public bool SphereCollect(Sphere sphere, IBVHQueryCollector<T> collector, List<T> data)
        {
            // Clear output
            data.Clear();
    
            // Validate args
            if (collector == null)
                return false;

            // Make sure the tree is ready
            ProcessIntegrationLeaves();
            if (mRoot == null)
                return false;

            // Push the root node onto the stack
            mNodeStack[mNodeStackLength++] = mRoot;

            // While there are still nodes onto the stack ...
            while (mNodeStackLength != 0)
            {
                // Pop node off the stack
                BinaryAABBTreeNode<T> node = mNodeStack[--mNodeStackLength];

                // Skip if node AABB does not intersect the sphere
                if (!sphere.TestBox(node.bounds))
                    continue;

                // Check if node is a leaf
                if (node.data != null)
                {
                    // Overlap leaf
                    if (collector.SphereOverlap(sphere, node.data))
                        data.Add(node.data);
                    continue;
                }

                // Ensure enough space for 2 children
                if (mNodeStackLength + 2 > mNodeStack.Length)
                {
                    var newStack = new BinaryAABBTreeNode<T>[mNodeStack.Length * 2];
                    Array.Copy(mNodeStack, newStack, mNodeStackLength);
                    mNodeStack = newStack;
                }

                // Push both children
                mNodeStack[mNodeStackLength++] = node.child0;
                mNodeStack[mNodeStackLength++] = node.child1;
            }

            // Return result
            return data.Count != 0;
        }

        //-----------------------------------------------------------------------------
        // Name: SphereOverlap() (Public Function)
        // Desc: Checks if the specified sphere overlaps with the node data.
        // Parm: sphere     - Query sphere.
        //       collector  - Used to check if the sphere overlaps with the node data.
        //                    Must be valid.
        // Rtrn: True if node data was collected.
        //-----------------------------------------------------------------------------
        public bool SphereOverlap(Sphere sphere, IBVHQueryCollector<T> collector)
        {
            // Validate args
            if (collector == null)
                return false;

            // Make sure the tree is ready
            ProcessIntegrationLeaves();
            if (mRoot == null)
                return false;

            // Push the root node onto the stack
            mNodeStack[mNodeStackLength++] = mRoot;

            // While there are still nodes onto the stack ...
            while (mNodeStackLength != 0)
            {
                // Pop node off the stack
                BinaryAABBTreeNode<T> node = mNodeStack[--mNodeStackLength];

                // Skip if node AABB does not intersect the sphere
                if (!sphere.TestBox(node.bounds))
                    continue;

                // Check if node is a leaf
                if (node.data != null)
                {
                    // Overlap leaf
                    if (collector.SphereOverlap(sphere, node.data))
                        return true;

                    // Move on
                    continue;
                }

                // Ensure enough space for 2 children
                if (mNodeStackLength + 2 > mNodeStack.Length)
                {
                    var newStack = new BinaryAABBTreeNode<T>[mNodeStack.Length * 2];
                    Array.Copy(mNodeStack, newStack, mNodeStackLength);
                    mNodeStack = newStack;
                }

                // Push both children
                mNodeStack[mNodeStackLength++] = node.child0;
                mNodeStack[mNodeStackLength++] = node.child1;
            }

            // No overlap
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: PolyhedronCollect() (Public Function)
        // Desc: Collects the data from all nodes that intersect or are fully contained
        //       within the specified polyhedron.
        // Parm: polyhedron - Query polyhedron.
        //       collector  - Used to check if the polyhedron overlaps with the node data.
        //                    Must be valid.
        //       data       - Returns the collected node data.
        // Rtrn: True if node data was collected.
        //-----------------------------------------------------------------------------
        public bool PolyhedronCollect(Polyhedron polyhedron, IBVHQueryCollector<T> collector, List<T> data)
        {
            // Clear output
            data.Clear();
         
            // Validate args
            if (!polyhedron.isValid || collector == null)
                return false;

            // Make sure the tree is ready
            ProcessIntegrationLeaves();
            if (mRoot == null)
                return false;

            // Push the root node onto the stack
            mNodeStack[mNodeStackLength++] = mRoot;

            // While there are still nodes onto the stack ...
            while (mNodeStackLength != 0)
            {
                // Pop node off the stack
                BinaryAABBTreeNode<T> node = mNodeStack[--mNodeStackLength];

                // Skip if node AABB does not intersect the polyhedron
                if (!polyhedron.TestBox(node.bounds))
                    continue;

                // Check if node is a leaf
                if (node.data != null)
                {
                    // Overlap leaf
                    if (collector.PolyhedronOverlap(polyhedron, node.data))
                        data.Add(node.data);
                    continue;
                }

                // Ensure enough space for 2 children
                if (mNodeStackLength + 2 > mNodeStack.Length)
                {
                    var newStack = new BinaryAABBTreeNode<T>[mNodeStack.Length * 2];
                    Array.Copy(mNodeStack, newStack, mNodeStackLength);
                    mNodeStack = newStack;
                }

                // Push both children
                mNodeStack[mNodeStackLength++] = node.child0;
                mNodeStack[mNodeStackLength++] = node.child1;
            }

            // Return result
            return data.Count != 0;
        }

        //-----------------------------------------------------------------------------
        // Name: OnLeafUpdated() (Public Function)
        // Desc: Must be called when a leaf node's volume has changed.
        // Parm: leaf   - The leaf whose bounds were updated.
        //       bounds - The new AABB.
        //-----------------------------------------------------------------------------
        public void OnLeafUpdated(BinaryAABBTreeNode<T> leaf, Bounds bounds)
        {
            // Validate call
            if (leaf == null) return;

            // Update leaf bounds
            leaf.bounds = bounds;

            // If we're already pending integration, there's nothing to do
            if (leaf.pendingIntegration)
                return;

            // Compute new AABB min/max
            Vector3 newCenter = bounds.center;
            Vector3 newExt    = bounds.extents;
            float newMinX     = newCenter.x - newExt.x;
            float newMinY     = newCenter.y - newExt.y;
            float newMinZ     = newCenter.z - newExt.z;
            float newMaxX     = newCenter.x + newExt.x;
            float newMaxY     = newCenter.y + newExt.y;
            float newMaxZ     = newCenter.z + newExt.z;

            // Check against parent's bounds
            BinaryAABBTreeNode<T> parent = leaf.parent;
            if (parent != null)
            {
                Vector3 pCenter = parent.bounds.center;
                Vector3 pExt    = parent.bounds.extents;
                float pMinX     = pCenter.x - pExt.x;
                float pMinY     = pCenter.y - pExt.y;
                float pMinZ     = pCenter.z - pExt.z;
                float pMaxX     = pCenter.x + pExt.x;
                float pMaxY     = pCenter.y + pExt.y;
                float pMaxZ     = pCenter.z + pExt.z;

                // Early out if new bounds are still within parent's bounds
                if (newMinX >= pMinX && newMinY >= pMinY && newMinZ >= pMinZ &&
                    newMaxX <= pMaxX && newMaxY <= pMaxY && newMaxZ <= pMaxZ)
                    return;
            }

            // Remove the leaf
            RemoveLeaf(leaf);

            // Resize integration array if needed
            if (mIntegrationLeafCount == mIntegrationLeaves.Length)
                Array.Resize(ref mIntegrationLeaves, mIntegrationLeaves.Length * 2);

            // Mark for integration
            leaf.pendingIntegration = true;
            mIntegrationLeaves[mIntegrationLeafCount++] = leaf;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: ProcessIntegrationLeaves() (Private Function)
        // Desc: Reinserts all previously removed leaf nodes into the tree.
        //       Called before spatial queries to ensure the tree is up-to-date.
        //-----------------------------------------------------------------------------
        void ProcessIntegrationLeaves()
        {
            // Loop through each integration node and integrate it
            for (int i = 0; i < mIntegrationLeafCount; ++i)
            {
                // Retrieve and release array reference immediately
                BinaryAABBTreeNode<T> leaf = mIntegrationLeaves[i];
                mIntegrationLeaves[i] = null;

                // If the leaf is pending deletion, recycle it
                if (leaf.pendingDeletion)
                {
                    leaf.pendingDeletion = false;
                    mNodePool.FreeNode(leaf);
                    continue;
                }

                // Insert leaf
                InsertLeaf(leaf);
            }

            // Reset counter
            mIntegrationLeafCount = 0;
        }

        //----------------------------------------------------------------------------- 
        // Name: InsertLeaf() (Private Function)
        // Desc: Inserts a leaf node into the binary tree.
        // Parm: leaf - The leaf node to insert.
        //----------------------------------------------------------------------------- 
        void InsertLeaf(BinaryAABBTreeNode<T> leaf)
        {
            // No longer pending integration
            leaf.pendingIntegration = false;

            // If the tree is empty, assign the leaf as root
            if (mRoot == null)
            {
                mRoot = leaf;
                return;
            }

            // Cache leaf center and extents
            Vector3 lc = leaf.bounds.center;
            Vector3 le = leaf.bounds.extents;
            float lxMin = lc.x - le.x;
            float lyMin = lc.y - le.y;
            float lzMin = lc.z - le.z;
            float lxMax = lc.x + le.x;
            float lyMax = lc.y + le.y;
            float lzMax = lc.z + le.z;

            BinaryAABBTreeNode<T> sibling = mRoot;
            while (sibling.data == null)
            {
                BinaryAABBTreeNode<T> c0 = sibling.child0;
                BinaryAABBTreeNode<T> c1 = sibling.child1;

                // Child 0 min/max
                Vector3 c0c = c0.bounds.center;
                Vector3 c0e = c0.bounds.extents;
                float c0xMin = c0c.x - c0e.x;
                float c0yMin = c0c.y - c0e.y;
                float c0zMin = c0c.z - c0e.z;
                float c0xMax = c0c.x + c0e.x;
                float c0yMax = c0c.y + c0e.y;
                float c0zMax = c0c.z + c0e.z;

                // Child 1 min/max
                Vector3 c1c = c1.bounds.center;
                Vector3 c1e = c1.bounds.extents;
                float c1xMin = c1c.x - c1e.x;
                float c1yMin = c1c.y - c1e.y;
                float c1zMin = c1c.z - c1e.z;
                float c1xMax = c1c.x + c1e.x;
                float c1yMax = c1c.y + c1e.y;
                float c1zMax = c1c.z + c1e.z;

                // Check full containment
                if (c0xMin <= lxMin && c0yMin <= lyMin && c0zMin <= lzMin &&
                    c0xMax >= lxMax && c0yMax >= lyMax && c0zMax >= lzMax)
                {
                    sibling = c0;
                    continue;
                }

                if (c1xMin <= lxMin && c1yMin <= lyMin && c1zMin <= lzMin &&
                    c1xMax >= lxMax && c1yMax >= lyMax && c1zMax >= lzMax)
                {
                    sibling = c1;
                    continue;
                }

                // --- Compute SAH cost for c0 ---
                float minX0 = (c0xMin < lxMin) ? c0xMin : lxMin;
                float minY0 = (c0yMin < lyMin) ? c0yMin : lyMin;
                float minZ0 = (c0zMin < lzMin) ? c0zMin : lzMin;
                float maxX0 = (c0xMax > lxMax) ? c0xMax : lxMax;
                float maxY0 = (c0yMax > lyMax) ? c0yMax : lyMax;
                float maxZ0 = (c0zMax > lzMax) ? c0zMax : lzMax;
                float dx0 = maxX0 - minX0;
                float dy0 = maxY0 - minY0;
                float dz0 = maxZ0 - minZ0;
                float area0 = 2.0f * (dx0 * dy0 + dy0 * dz0 + dz0 * dx0);

                // --- Compute SAH cost for c1 ---
                float minX1 = (c1xMin < lxMin) ? c1xMin : lxMin;
                float minY1 = (c1yMin < lyMin) ? c1yMin : lyMin;
                float minZ1 = (c1zMin < lzMin) ? c1zMin : lzMin;
                float maxX1 = (c1xMax > lxMax) ? c1xMax : lxMax;
                float maxY1 = (c1yMax > lyMax) ? c1yMax : lyMax;
                float maxZ1 = (c1zMax > lzMax) ? c1zMax : lzMax;
                float dx1 = maxX1 - minX1;
                float dy1 = maxY1 - minY1;
                float dz1 = maxZ1 - minZ1;
                float area1 = 2.0f * (dx1 * dy1 + dy1 * dz1 + dz1 * dx1);

                // Choose the better sibling
                sibling = (area0 < area1) ? c0 : c1;
            }

            // Create new parent and connect to tree
            BinaryAABBTreeNode<T> newParent = mNodePool.AllocNode();
            newParent.child0 = sibling;
            newParent.child1 = leaf;

            // Compute bounds of new parent
            newParent.bounds = sibling.bounds;
            newParent.bounds.Encapsulate(leaf.bounds);
            newParent.bounds.size += mPadding * 0.5f;

            // Link the new parent into the tree
            BinaryAABBTreeNode<T> oldParent = sibling.parent;
            if (oldParent != null)
            {
                newParent.parent = oldParent;
                if (oldParent.child0 == sibling) oldParent.child0 = newParent;
                else oldParent.child1 = newParent;
            }
            else
            {
                // Sibling was the root, so newParent becomes the new root
                mRoot = newParent;
            }

            sibling.parent = newParent;
            leaf.parent = newParent;

            // Refit and balance upward
            BinaryAABBTreeNode<T> current = newParent.parent;
            while (current != null)
            {
                current.Refit();
                current = current.parent;
            }
        }

        //----------------------------------------------------------------------------- 
        // Name: RemoveLeaf() (Private Function)
        // Desc: Removes a leaf node from the binary tree.
        // Parm: leaf - The leaf node to remove.
        //----------------------------------------------------------------------------- 
        void RemoveLeaf(BinaryAABBTreeNode<T> leaf)
        {
            // If the leaf is pending integration, we can't remove it
            if (leaf.pendingIntegration)
                return;

            // Special case: removing the root node
            if (leaf == mRoot)
            {
                mRoot       = null;
                leaf.parent = null;
                return;
            }

            // Cache data
            BinaryAABBTreeNode<T> parent   = leaf.parent;
            BinaryAABBTreeNode<T> grandpa  = parent.parent;
            BinaryAABBTreeNode<T> sibling  = (parent.child0 == leaf) ? parent.child1 : parent.child0;

            // Do we have a grandpa node?
            if (grandpa != null)
            {
                // Link the sibling to the grandpa
                sibling.parent = grandpa;
                if (grandpa.child0 == parent) grandpa.child0 = sibling;
                else grandpa.child1 = sibling;

                // Recalculate grandpa's bounds and apply padding (it's the new immediate parent)
                grandpa.bounds = grandpa.child0.bounds;
                grandpa.bounds.Encapsulate(grandpa.child1.bounds);
                grandpa.bounds.size += mPadding * 0.5f;

                // Refit upwards
                BinaryAABBTreeNode<T> current = grandpa.parent;
                while (current != null)
                {
                    current.Refit();
                    current = current.parent;
                }
            }
            else
            {
                // No grandparent means sibling becomes new root
                mRoot = sibling;
                mRoot.parent = null;
            }

            // Recycle the unlinked parent node (super node)
            leaf.parent = null;
            mNodePool.FreeNode(parent);
        }
        #endregion
    }
    #endregion
}
