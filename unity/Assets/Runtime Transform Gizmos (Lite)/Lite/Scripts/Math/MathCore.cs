using UnityEngine;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EPlane (Public Enum)
    // Desc: Defines different planes in a coordinate system.
    //-----------------------------------------------------------------------------
    public enum EPlane
    {
        XY = 0,
        YZ,
        ZX
    }

    //-----------------------------------------------------------------------------
    // Name: EPlaneClassify (Public Enum)
    // Desc: Defines different type of results which can be obtained when classifying
    //       entities against a plane.
    //-----------------------------------------------------------------------------
    public enum EPlaneClassify
    {
        InFront = 0,    // All polygon points are in front of the plane
        Behind,         // All polygon points are behind the plane
        Spanning,       // The polygon is spanning the plane (i.e. some points in front, others behind)
        OnPlane         // All polygon points are on the plane
    }
    #endregion
}