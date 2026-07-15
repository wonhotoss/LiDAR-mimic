using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoProxy (Public Abstract Class)
    // Desc: A gizmo proxy is used to implement a gizmo and allows for code reuse
    //       across different gizmo types. For example, the 'MoveGizmo' and the 
    //       'TRSGizmo' use the 'MoveGizmoProxy' class to implement the move functionality.
    //-----------------------------------------------------------------------------
    public abstract class GizmoProxy
    {
        #region Protected Fields
        protected Gizmo                     mGizmo;                                         // The proxy implements this gizmo
        protected GizmoDrag                 mDrag           = new GizmoDrag();              // Drag operation
        protected GizmoHandleRenderArgs     mHRenderArgs    = new GizmoHandleRenderArgs();  // Gizmo handle render args
        protected int                       mBaseHoverPriority;                             // Base hover priority for the gizmo handles. If a gizmo uses more
                                                                                            // than one proxy, this can be handy to control hover priorities across proxies.
        protected List<GizmoHandle>         mHandles        = new List<GizmoHandle>();      // Stores all handles for this proxy
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: handles (Public Property)
        // Desc: Returns the list of handles that belong to this proxy.
        //-----------------------------------------------------------------------------
        public List<GizmoHandle>    handles     { get { return mHandles; } }

        //-----------------------------------------------------------------------------
        // Name: drag (Public Property)
        // Desc: Returns the proxy's drag operation. This is the drag operation that
        //       is activated when the user starts dragging one of the proxy handles.
        //-----------------------------------------------------------------------------
        public GizmoDrag            drag        { get { return mDrag; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------  
        // Name: GizmoProxy() (Public Constructor)
        // Desc: Creates a gizmo proxy for the specified gizmo.
        // Parm: gizmo             - The gizmo.
        //       baseHoverPriority - All handles created by the proxy will have this
        //                           base hover priority. Useful when a gizmo uses more
        //                           than one proxy because it allows us to control the
        //                           hover priority across proxies.
        //----------------------------------------------------------------------------- 
        public GizmoProxy(Gizmo gizmo, int baseHoverPriority)
        {
            // Assign gizmo and validate it
            mGizmo = gizmo;
            if (mGizmo == null)
                RTG.Exception(nameof(GizmoProxy), nameof(GizmoProxy), "The gizmo was null.");

            // Set data
            mBaseHoverPriority = baseHoverPriority;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnCreateHandles() (Public Function)
        // Desc: Called during gizmo initialization to allow the gizmo to create all the
        //       handles that it needs. On function return, these handles will be made
        //       children of the gizmo transform.
        // Parm: handles - Returns the list of handles.
        //-----------------------------------------------------------------------------
        public void OnCreateHandles(List<GizmoHandle> handles)
        {
            // Allow derived class to create the handles in the proxy's handle list.
            // It's useful to have a list of handles per proxy.
            OnCreateProxyHandles(mHandles);

            // Now store the proxy handles inside the passed handles list
            handles.AddRange(mHandles);
        }
        #endregion

        #region Protected Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: OnCreateProxyHandles() (Protected Abstract Function)
        // Desc: Called in order to allow the proxy to create the gizmo handles.
        // Parm: handles - Returns the list of handles.
        //-----------------------------------------------------------------------------
        protected abstract void OnCreateProxyHandles(List<GizmoHandle> handles);
        #endregion
    }
    #endregion
}
