using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: Segment (Public Struct)
    // Desc: Provides storage for segment primitives and implements relevant functions.
    //-----------------------------------------------------------------------------
    public struct Segment
    {
        #region Private Fields
        Vector3 mStart;     // Start point
        Vector3 mEnd;       // End point
        float   mLength;    // Segment length
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: start (Public Property)
        // Desc: Returns or sets the segment start.
        //-----------------------------------------------------------------------------
        public Vector3 start        { get { return mStart; } set { mStart = value; mLength = (mEnd - mStart).magnitude; } }

        //-----------------------------------------------------------------------------
        // Name: end (Public Property)
        // Desc: Returns or sets the segment end.
        //-----------------------------------------------------------------------------
        public Vector3 end          { get { return mEnd; } set { mEnd = value; mLength = (mEnd - mStart).magnitude; } }

        //-----------------------------------------------------------------------------
        // Name: direction (Public Property)
        // Desc: Returns the segment direction. This is a normalized vector that points
        //       from the start to the end of the segment.
        //-----------------------------------------------------------------------------
        public Vector3 direction    { get { return (mEnd - mStart).normalized; } }

        //-----------------------------------------------------------------------------
        // Name: center (Public Property)
        // Desc: Returns the segment center point.
        //-----------------------------------------------------------------------------
        public Vector3 center       { get { return mStart + (mEnd - mStart) * 0.5f; } }

        //-----------------------------------------------------------------------------
        // Name: length (Public Property)
        // Desc: Returns the segment length.
        //-----------------------------------------------------------------------------
        public float   length       { get { return mLength; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: Segment() (Public Constructor)
        // Desc: Creates a segment from the specified points.
        // Parm: start - Segment start.
        //       end   - Segment end.
        //-----------------------------------------------------------------------------
        public Segment(Vector3 start, Vector3 end)
        {
            mStart      = start;
            mEnd        = end;
            mLength     = (mEnd - mStart).magnitude;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the segment's AABB.
        // Rtrn: The segment's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Enclose segment points
            Box aabb = new Box(mStart, Vector3.zero);
            aabb.EnclosePoint(mEnd);

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: Normalize()
        // Desc: Normalizes the segment. The function preserve the segments start point
        //       and direction, but moves the end point at 1 unit distance from the
        //       start.
        //-----------------------------------------------------------------------------
        public void Normalize()
        {
            mEnd    = mStart + direction;
            mLength = 1.0f;
        }
        #endregion
    }
    #endregion
}
