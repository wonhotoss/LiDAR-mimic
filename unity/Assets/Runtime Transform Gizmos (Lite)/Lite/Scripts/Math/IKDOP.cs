using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Interfaces
    //-----------------------------------------------------------------------------
    // Name: IKDOP (Public Interface)
    // Desc: Interface which can be implemented for different types of DOPs. A DOP
    //       stands for discrete oriented polytope and it is used to approximate
    //       object volumes. The 'K' stands for the number of boundary planes that
    //       bound the DOP's volume.
    //-----------------------------------------------------------------------------
    public interface IKDOP
    {
        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: isValid (Public Property)
        // Desc: Returns true if the DOP is valid and false otherwise.
        //-----------------------------------------------------------------------------
        bool    isValid     { get; }

        //-----------------------------------------------------------------------------
        // Name: vertexCount (Public Property)
        // Desc: Returns the number of vertices.
        //-----------------------------------------------------------------------------
        int     vertexCount { get; }

        //-----------------------------------------------------------------------------
        // Name: planeCount (Public Property)
        // Desc: Returns the number of boundary planes.
        //-----------------------------------------------------------------------------
        int     planeCount  { get; }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: GetVertex() (Public Function)
        // Desc: Returns the vertex with the specified index.
        // Parm: index - The index of the vertex to be returned.
        // Rtrn: The DOP's vertex with the specified index.
        //-----------------------------------------------------------------------------
        Vector3 GetVertex       (int index);

        //-----------------------------------------------------------------------------
        // Name: CollectVerts() (Public Function)
        // Desc: Collects the DOP's vertices and stores them inside 'verts'.
        // Parm: verts - Returns the DOP's vertices.
        //-----------------------------------------------------------------------------
        void    CollectVerts    (List<Vector3> verts);

        //-----------------------------------------------------------------------------
        // Name: CollectVerts() (Public Function)
        // Desc: Collects the DOP's vertices and stores them inside 'verts'.
        // Parm: transformMtx - Transform matrix used to transform each vertex before
        //                      being stored in the output list.
        //       verts        - Returns the DOP's vertices transformed by 'transformMtx'.
        //-----------------------------------------------------------------------------
        void    CollectVerts    (Matrix4x4 transformMtx, List<Vector3> verts);
        #endregion
    }
    #endregion
}
