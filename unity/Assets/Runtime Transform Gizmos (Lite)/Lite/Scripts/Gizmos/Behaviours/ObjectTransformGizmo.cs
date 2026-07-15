using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoPivot (Public Enum)
    // Desc: Defines different gizmo transform pivots. A pivot affects the gizmo
    //       position and in some cases how the object are transformed (e.g. rotation gizmo).
    //-----------------------------------------------------------------------------
    public enum EGizmoPivot
    {
        Pivot = 0,  // The gizmo is placed at the target's position
        Center,     // The gizmo is placed at the center of all targets
    }

    //-----------------------------------------------------------------------------
    // Name: EGizmoTransformSpace (Public Enum)
    // Desc: Defines different gizmo transform spaces. A transform space controls
    //       how the gizmo is oriented and affects both the gizmo rotation and the
    //       way in which objects are transformed.
    //-----------------------------------------------------------------------------
    public enum EGizmoTransformSpace
    {
        Local = 0,  // The gizmo's axes are aligned with the target's local axes
        Global      // The gizmo's axes are aligned with the world axes
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ObjectTransformGizmo (Public Class)
    // Desc: A gizmo behaviour which modifies object transforms.
    //-----------------------------------------------------------------------------
    public class ObjectTransformGizmo : GizmoBehaviour
    {
        #region Private Structures
        //-----------------------------------------------------------------------------
        // Name: ScaleData (Private Struct)
        // Desc: Provides storage for data that is used when scaling objects. When a 
        //       scale drag operation starts, each object will have an instance of
        //       this struct associated to it.
        //-----------------------------------------------------------------------------
        struct ScaleData
        {
            #region Public Fields
            public Vector3 pivot;           // The scale pivot
            public Vector3 pivotAnchor;     // Vector which goes from the pivot to the object's position
            public Vector3 invScale;        // The inverse of the object's scale at the beginning of the scale operation         
            #endregion
        }
        #endregion

        #region Private Static Fields
        static Dictionary<GameObject, Vector3>  sLocalPivotMap = new Dictionary<GameObject, Vector3>();  // Maps a game object to a pivot defined in the object local space

        // Buffers used to avoid memory allocations
        static List<ObjectTransformGizmo>       sObjectTransformGizmoBuffer = new List<ObjectTransformGizmo>();
        #endregion

        #region Private Fields
        IReadOnlyList<GameObject>   mTargets;                                   // Points to the target objects
        List<GameObject>            mTargetParents = new List<GameObject>();    // Contains all parents inside the target list. This is useful because when transforming the targets
                                                                                // we want to transform the parent only. The children will be transformed along with the parents.

        EGizmoPivot                 mPivot              = EGizmoPivot.Pivot;            // Pivot
        EGizmoTransformSpace        mTransformSpace     = EGizmoTransformSpace.Global;  // Transform space

        Dictionary<GameObject, ScaleData> 
                                    mScaleDataMap       = new Dictionary<GameObject, ScaleData>();  // Maps a game object to its scale data during a scale drag op
        MoveGizmo                   mMoveGizmo;         // If the behaviour is attached to a move gizmo, this will point to it
        TRSGizmo                    mTRSGizmo;          // If the behaviour is attached to a TRS  gizmo, this will point to it
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: targetCount (Public Property)
        // Desc: Returns the number of target objects.
        //-----------------------------------------------------------------------------
        public int  targetCount { get { return mTargets != null ? mTargets.Count : 0; } }

        //-----------------------------------------------------------------------------
        // Name: pivot (Public Property)
        // Desc: Returns or sets the gizmo's pivot.
        //-----------------------------------------------------------------------------
        public EGizmoPivot          pivot               { get { return mPivot; } set { if (mPivot != value) { mPivot = value; RefreshPosition(); } } }

        //-----------------------------------------------------------------------------
        // Name: transformSpace (Public Property)
        // Desc: Returns or sets the gizmo's transform space.
        //-----------------------------------------------------------------------------
        public EGizmoTransformSpace transformSpace      { get { return mTransformSpace; } set { if (mTransformSpace != value) { mTransformSpace = value; RefreshRotation(); } } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: SetTarget() (Public Function)
        // Desc: Sets the target object.
        // Parm: target - The game object which the gizmo will transform. You can set this
        //                to null if you wish to disconnect the gizmo from its target.
        //-----------------------------------------------------------------------------
        public void SetTarget(GameObject target)
        {
            // Set target
            if (target == null)
            {
                mTargetParents.Clear();     // Clear parents...
                mTargets = null;            // ... and set the targets to null

                // Clear vertex snap pivot objects
                if (mMoveGizmo != null) mMoveGizmo.vSnapPivotObjects = null;
                else if (mTRSGizmo != null) mTRSGizmo.vSnapPivotObjects = null;
            }
            else
            {
                // Store the target object in the target parents list and
                // make the targets collection point to the target parents.
                mTargetParents.Clear();
                mTargetParents.Add(target);
                mTargets = mTargetParents;

                // Set vertex snap pivot objects
                if (mMoveGizmo != null) mMoveGizmo.vSnapPivotObjects = mTargets;
                else if (mTRSGizmo != null) mTRSGizmo.vSnapPivotObjects = mTargets;
            }
   
            // Refresh
            Refresh();
        }

        //-----------------------------------------------------------------------------
        // Name: SetTargets() (Public Function)
        // Desc: Sets the target objects.
        // Parm: targets - Collection of target objects which implements the 'IReadOnlyList'
        //                 interface. You can set this to null if you wish to disconnect
        //                 the gizmo from its targets.
        //-----------------------------------------------------------------------------
        public void SetTargets(IReadOnlyList<GameObject> targets)
        {
            // Set targets
            if (targets == null)
            {
                mTargetParents.Clear();     // Clear parents...
                mTargets = null;            // ... and set the targets to null

                // Clear vertex snap pivot objects
                if (mMoveGizmo != null) mMoveGizmo.vSnapPivotObjects = null;
                else if (mTRSGizmo != null) mTRSGizmo.vSnapPivotObjects = null;
            }
            else
            {
                // Set targets and collect parents
                mTargets = targets;     
                GameObjectEx.CollectParents(targets, mTargetParents);

                // Set vertex snap pivot objects
                if (mMoveGizmo != null) mMoveGizmo.vSnapPivotObjects = mTargets;
                else if (mTRSGizmo != null) mTRSGizmo.vSnapPivotObjects = mTargets;
            }

            // Refresh
            Refresh();
        }

        //-----------------------------------------------------------------------------
        // Name: Refresh() (Public Function)
        // Desc: Should be called during any one of the following scenarios:
        //          a) the client has manually changed the target objects' transform.
        //          b) the client has added or removed objects to the target collection. 
        //       The function has no effect if the gizmo doesn't have any targets attached.
        //-----------------------------------------------------------------------------
        public void Refresh()
        {
            // No-op?
            if (targetCount == 0)
                return;

            // Collect parents
            if (mTargets != mTargetParents)
                GameObjectEx.CollectParents(mTargets, mTargetParents);

            // Refresh gizmo transform
            RefreshPosition();
            RefreshRotation();
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateTargetOBB() (Public Function)
        // Desc: Calculates the oriented box that encloses all target objects.
        // Rtrn: The 'OBox' that encloses all target objects. An invalid box is returned
        //       if no targets are available.
        //-----------------------------------------------------------------------------
        public OBox CalculateTargetOBB()
        {
            if (targetCount == 0) return OBox.GetInvalid();
            return GameObjectEx.CalculateHierarchiesWorldOBB(mTargetParents, BoundsQueryConfig.defaultConfig);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateTargetAABB() (Public Function)
        // Desc: Calculates the AABB that encloses all target objects.
        // Rtrn: The 'Box' that encloses all target objects. An invalid box is returned
        //       if no targets are available.
        //-----------------------------------------------------------------------------
        public Box CalculateTargetAABB()
        {
            if (targetCount == 0) return Box.GetInvalid();
            return GameObjectEx.CalculateHierarchiesWorldAABB(mTargetParents, BoundsQueryConfig.defaultConfig);
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: SetObjectLocalPivot() (Public Static Function)
        // Desc: Sets the specified object's local pivot. This is used when a gizmo's
        //       pivot is set to 'Pivot'. This function calls 'Refresh' on all
        //       'ObjectTransformGizmo' instances that currently exist.
        // Parm: gameObject - The game object whose pivot must be set.
        //       localPivot - The pivot of the object in its local/model space.
        //-----------------------------------------------------------------------------
        public static void SetObjectLocalPivot(GameObject gameObject, Vector3 localPivot)
        {
            // Does this game object already exist inside the pivot map or do we need to create a new entry?
            if (sLocalPivotMap.ContainsKey(gameObject))
                sLocalPivotMap[gameObject] = localPivot;
            else sLocalPivotMap.Add(gameObject, localPivot);

            // Refresh all object transform gizmos
            RTGizmos.get.CollectObjectTransformGizmos(sObjectTransformGizmoBuffer);
            int count = sObjectTransformGizmoBuffer.Count;
            for (int i = 0; i < count; ++i)
                sObjectTransformGizmoBuffer[i].Refresh();
        }

        //-----------------------------------------------------------------------------
        // Name: GetObjectLocalPivot() (Public Static Function)
        // Desc: Returns the local pivot assigned to the specified object via a previous
        //       call to 'SetObjectLocalPivot'.
        // Parm: gameObject - Query object.
        // Rtrn: The local pivot assigned to this object. If no pivot was assigned, the
        //       zero vector is returned which is the same as the object's position in
        //       the object's local space.
        //-----------------------------------------------------------------------------
        public static Vector3 GetObjectLocalPivot(GameObject gameObject)
        {
            // If the game object has a local pivot defined for it, return it
            if (sLocalPivotMap.TryGetValue(gameObject, out Vector3 pivot))
                return pivot;

            // Otherwise, just return the zero vector (i.e. the object position in the object's local space)
            return Vector3.zero;
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnInit() (Protected Function)
        // Desc: Called when the behaviour must initialize itself.
        //-----------------------------------------------------------------------------
        protected override void OnInit()
        {
            // Init data
            mMoveGizmo = gizmo as MoveGizmo;
            if (mMoveGizmo != null) mMoveGizmo.moveModeChanged += OnGizmoMoveModeChanged;

            mTRSGizmo = gizmo as TRSGizmo;
            if (mTRSGizmo != null) mTRSGizmo.moveModeChanged += OnGizmoMoveModeChanged;

            // Register handlers
            RTUndo.get.undoRedo += OnUndoRedo;
        }

        //-----------------------------------------------------------------------------
        // Name: OnDrag() (Protected Function)
        // Desc: Called when the gizmo is dragged.
        // Parm: drag - The gizmo drag operation.
        //-----------------------------------------------------------------------------
        protected override void OnDrag(GizmoDrag drag)
        {
            // No targets?
            if (mTargets == null)
                return;

            // See what kind of channel we are dealing with
            int count = mTargetParents.Count;
            switch (drag.dragChannel)
            {
                case EGizmoDragChannel.Position:

                    // Move objects
                    for (int i = 0; i < count; ++i)
                        MoveObject(mTargetParents[i], drag.dragDelta);

                    // Note: This is not really necessary, but if some of the objects use 'IRTObjectTransformGizmoTarget'
                    //       interface, the gizmo can become detached from the objects (e.g. if objects are constrained
                    //       to a move bounds).
                    // Note: Actually, don't do this. It ruins the drag values.
                    /*if (drag.desc.dragType != EGizmoDragType.VertexSnap)
                        RefreshPosition();*/
                    break;

                case EGizmoDragChannel.Rotation:

                    // Rotate objects
                    for (int i = 0; i < count; ++i)
                        RotateObject(mTargetParents[i], drag.rotationDelta);

                    break;

                case EGizmoDragChannel.Scale:

                    // Scale objects
                    for (int i = 0; i < count; ++i)
                        UpdateObjectScale(mTargetParents[i], drag);

                    break;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: OnDragStart() (Protected Function)
        // Desc: Called when the gizmo's drag operation starts.
        // Parm: drag - The gizmo's drag operation.
        //-----------------------------------------------------------------------------
        protected override void OnDragStart(GizmoDrag drag)
        {
            // No targets?
            if (mTargets == null)
                return;

            // If we are scaling, create the scale data instances
            if (drag.dragChannel == EGizmoDragChannel.Scale)
            {
                // Clear scale data map and repopulate
                mScaleDataMap.Clear();
                int count = mTargetParents.Count;
                for (int i = 0; i < count; ++i)
                {
                    // Create scale data instance and store it for this object
                    var scaleData = new ScaleData();
                    scaleData.pivot         = PivotToObjectPivot(mTargetParents[i]);
                    scaleData.pivotAnchor   = mTargetParents[i].transform.position - scaleData.pivot;
                    scaleData.invScale      = mTargetParents[i].transform.lossyScale.Inverse();
                    mScaleDataMap.Add(mTargetParents[i], scaleData);
                }
            }
            else mScaleDataMap.Clear(); // Just clear any stray data
        }

        //-----------------------------------------------------------------------------
        // Name: OnDragEnd() (Protected Function)
        // Desc: Called when the gizmo's drag operation ends.
        // Parm: drag - The drag operation that ended.
        //-----------------------------------------------------------------------------
        protected override void OnDragEnd(GizmoDrag drag)
        {
            // Refresh the position and rotation. For example, if we are dragging a move
            // gizmo and some objects have the 'IRTObjectTransformGizmoTarget' interface
            // attached as a script, they may not move along with the gizmo if the script's
            // logic doesn't allow it. This can lead to situations where the gizmo can 
            // appear to be detached from the target objects.
            RefreshPosition();
            RefreshRotation();
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: MoveObject() (Private Function)
        // Desc: Moves the specified object.
        // Parm: gameObject - Object to move.
        //       moveOffset - Move offset.
        //-----------------------------------------------------------------------------
        void MoveObject(GameObject gameObject, Vector3 moveOffset)
        {
            // Get transform target interface
            var rtTarget = gameObject.GetRTObjectTransformGizmoTarget();
            if (rtTarget != null)
            {
                // Are we allowed to change?
                if (!rtTarget.CanTransform(EGizmoDragChannel.Position, this))
                    return;

                // Update move offset
                moveOffset = rtTarget.UpdateMoveOffset(moveOffset, this);
            }

            // Transform object and notify
            RTUndo.get.Record(gameObject.transform);
            RTScene.get.MoveObject(gameObject, moveOffset);
            if (rtTarget != null) rtTarget.OnTransformed(EGizmoDragChannel.Position, this);
        }

        //-----------------------------------------------------------------------------
        // Name: RotateObject() (Private Function)
        // Desc: Rotates the specified object.
        // Parm: gameObject - Object to rotate.
        //       rotation   - Rotation to apply.
        //-----------------------------------------------------------------------------
        void RotateObject(GameObject gameObject, Quaternion rotation)
        {
            // Get transform target interface
            var rtTarget = gameObject.GetRTObjectTransformGizmoTarget();
            if (rtTarget != null)
            {
                // Are we allowed to change?
                if (!rtTarget.CanTransform(EGizmoDragChannel.Rotation, this))
                    return;
            }

            // Transform object and notify
            RTUndo.get.Record(gameObject.transform);
            RTScene.get.RotateObjectAroundPivot(gameObject, rotation, PivotToObjectPivot(gameObject));
            if (rtTarget != null) rtTarget.OnTransformed(EGizmoDragChannel.Rotation, this);
        }

        //-----------------------------------------------------------------------------
        // Name: UpdateObjectScale() (Private Function)
        // Desc: Updates the scale of the specified object.
        // Parm: gameObject  - The object whose scale must be updated.
        //       drag        - The gizmo drag operation.
        //-----------------------------------------------------------------------------
        void UpdateObjectScale(GameObject gameObject, GizmoDrag drag)
        {
            // Get transform target interface
            var rtTarget = gameObject.GetRTObjectTransformGizmoTarget();
            if (rtTarget != null)
            {
                // Are we allowed to change?
                if (!rtTarget.CanTransform(EGizmoDragChannel.Scale, this))
                    return;
            }

            // Apply scale offset
            RTUndo.get.Record(gameObject.transform);
            gameObject.transform.localScale += drag.dragDelta;
            gameObject.transform.localScale = gameObject.transform.localScale.FixFloatError();

            // Calculate scale factor used to scale the object position from the pivot
            var scaleData = mScaleDataMap[gameObject];
            Vector3 s = Vector3.Scale(gameObject.transform.lossyScale, scaleData.invScale);

            // Note: If the original scale contains 0 values, 'scaleData.invScale' contains
            //       invalid values and this can produce scale factors with NaN or Infinity
            //       values. In this case, we just replace those components with 1 meaning
            //       that no scaling will be applied in that direction.
            s = s.ReplaceNaN(1.0f).ReplaceInfinity(1.0f);

            // Scale object position from the pivot along the gizmo's transform axes
            Vector3 right   = gizmo.transform.right;           
            Vector3 up      = gizmo.transform.up;               
            Vector3 forward = gizmo.transform.forward;
            gameObject.transform.position = scaleData.pivot + 
                                            right   * Vector3.Dot(right, scaleData.pivotAnchor)     * s.x + 
                                            up      * Vector3.Dot(up, scaleData.pivotAnchor)        * s.y + 
                                            forward * Vector3.Dot(forward, scaleData.pivotAnchor)   * s.z;

            // Notify
            RTScene.get.OnObjectTransformChanged(gameObject);
            if (rtTarget != null) rtTarget.OnTransformed(EGizmoDragChannel.Scale, this);
        }

        //-----------------------------------------------------------------------------
        // Name: GetPivotObject() (Private Function)
        // Desc: Returns the pivot object. Used when the gizmo's transform pivot is
        //       set to one of the pivot identifiers.
        // Rtrn: The pivot object or null of the gizmo doesn't have any targets.
        //-----------------------------------------------------------------------------
        GameObject GetPivotObject()
        {
            return targetCount > 0 ? mTargets[mTargets.Count - 1] : null;
        }

        //-----------------------------------------------------------------------------
        // Name: RefreshPosition() (Private Function)
        // Desc: Refreshes the gizmo's position. This must be called when the gizmo's
        //       pivot changes or when the target objects change.
        //-----------------------------------------------------------------------------
        void RefreshPosition()
        {
            // No-op?
            if (targetCount == 0)
                return;

            // Set the gizmo position to the current pivot
            gizmo.transform.position = PivotToGizmoPosition();
        }

        //-----------------------------------------------------------------------------
        // Name: RefreshRotation() (Private Function)
        // Desc: Refreshes the gizmo's position. This must be called when the gizmo's
        //       transform space changes or when the target objects change.
        //-----------------------------------------------------------------------------
        void RefreshRotation()
        {
            // No-op?
            if (targetCount == 0)
                return;

            // Check transform space
            if (transformSpace == EGizmoTransformSpace.Global)
                gizmo.transform.rotation = Quaternion.identity;
            else gizmo.transform.rotation = GetPivotObject().transform.rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: PivotToGizmoPosition() (Private Function)
        // Desc: Converts the current pivot to a gizmo position that reflects the selected
        //       pivot.
        // Rtrn: The gizmo position that reflects the selected pivot.
        //-----------------------------------------------------------------------------
        Vector3 PivotToGizmoPosition()
        {
            // Check pivot
            switch (pivot)
            {
                case EGizmoPivot.Center:    return CalculateTargetOBB().center;
                case EGizmoPivot.Pivot:     return GetPivotObject().transform.TransformPoint(GetObjectLocalPivot(GetPivotObject()));
                default:                    return Vector3.zero;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: PivotToObjectPivot() (Private Function)
        // Desc: Converts the current pivot to a 3D pivot used to transform the specified
        //       target object.
        // Parm: targetObject - The target object about to be transformed.
        // Rtrn: 3D pivot used to transform the specified target object.
        //-----------------------------------------------------------------------------
        Vector3 PivotToObjectPivot(GameObject targetObject)
        {
            // Check pivot
            switch (pivot)
            {
                case EGizmoPivot.Center:    return gizmo.transform.position;    //  Avoid calling 'CalculateTargetOBox' for each object. We can use the gizmo position.
                case EGizmoPivot.Pivot:     return targetObject.transform.TransformPoint(GetObjectLocalPivot(targetObject));
                default:                    return Vector3.zero;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: OnGizmoMoveModeChanged() (Private Function)
        // Desc: Event handler for the gizmo move mode changed event.
        // Parm: oldMoveMode    - Old move mode.
        //       gizmo          - The gizmo that fires the event.
        //-----------------------------------------------------------------------------
        void OnGizmoMoveModeChanged(EGizmoMoveMode oldMoveMode, Gizmo gizmo)
        {
            // If we are getting out of vertex snapping, refresh the position. This is necessary
            // because of the fact that not all target objects may have been affected by the
            // vertex snap in the same manner (due to 'IRTObjectTransformGizmoTarget) and we
            // want the gizmo to be positioned correctly based on the selected pivot.
            if (oldMoveMode == EGizmoMoveMode.VertexSnap)
                RefreshPosition();
        }

        //-----------------------------------------------------------------------------
        // Name: OnUndoRedo() (Private Function)
        // Desc: Event handler for the Undo/Redo event.
        // Parm: redo - True on Redo. False on Undo.
        //-----------------------------------------------------------------------------
        void OnUndoRedo(bool redo)
        {
            RefreshPosition();
            RefreshRotation();
        }
        #endregion
    }
    #endregion
}