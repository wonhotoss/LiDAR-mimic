using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RectEx (Public Static Class)
    // Desc: Implements useful extensions and utility functions for the 'Rect' struct.
    //-----------------------------------------------------------------------------
    public static class RectEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: HasPositiveSize() (Public Extension)
        // Desc: Checks if both the rectangle width and height are non-negative.
        // Rtrn: True if the rect width and height are >= 0; false otherwise.
        //-----------------------------------------------------------------------------
        public static bool HasPositiveSize(this Rect rect)
        {
            return rect.width >= 0.0f && rect.height >= 0.0f;
        }

        //-----------------------------------------------------------------------------
        // Name: ScreenToGUIRect() (Public Extension)
        // Desc: Converts a rectangle from screen space to GUI space, where the Y axis
        //       grows downwards.
        // Rtrn: The GUI space rectangle.
        //-----------------------------------------------------------------------------
        public static Rect ScreenToGUIRect(this Rect rect)
        {
            // Note: Don't subtract 1 from 'Screen.height'. 'rect.yMax' is exclusive so it
            //       already contains that extra one pixel we would need to subtract.
            return new Rect(rect.x, Screen.height - rect.yMax, rect.width, rect.height);
        }

        //-----------------------------------------------------------------------------
        // Name: Inflate() (Public Extension)
        // Desc: Inflates the rectangle symmetrically by the specified amount both
        //       horizontally and vertically.
        // Parm: amount - Amount to inflate. Negative amounts will deflate the rectangle.
        // Rtrn: The updated rectangle.
        //-----------------------------------------------------------------------------
        public static Rect Inflate(this Rect rect, float amount)
        {
            // Inflate in all directions
            float half = amount / 2.0f;
            rect.xMin -= half;
            rect.xMax += half;
            rect.yMin -= half;
            rect.yMax += half;

            // Return new rectangle
            return rect;
        }

        //-----------------------------------------------------------------------------
        // Name: PositiveSize() (Public Extension)
        // Desc: Clones the rectangle and ensures that the clone has a positive width
        //       and height.
        // Rtrn: The cloned rectangle but with its min/max coords reordered so that it
        //       has a positive width and height.
        //-----------------------------------------------------------------------------
        public static Rect PositiveSize(this Rect rect) 
        {
            // Use rect coords and size by default and adjust if necessary
            float xStart    = rect.xMin;
            float yStart    = rect.yMin;
            float width     = rect.width;
            float height    = rect.height;

            // Adjust if necessary
            if (width < 0.0f)
            {
                width   = -width;
                xStart  = rect.xMax;
            }
            if (height < 0.0f)
            {
                height = -height;
                yStart = rect.yMax;
            }

            // Return a new rectangle
            return new Rect(xStart, yStart, width, height);
        }   

        //-----------------------------------------------------------------------------
        // Name: RelativeTo (Public Extension)
        // Desc: Returns a rectangle whose position is expressed relative to the position
        //       of the specified origin rectangle.
        // Parm: origin - The rectangle that represents the coordinate system origin.
        // Rtrn: The new rectangle with the same size but its position expressed relative
        //       to the position of 'origin' rectangle.
        //-----------------------------------------------------------------------------
        public static Rect RelativeTo(this Rect rect, Rect origin)
        {
            rect.x = rect.x - origin.x;
            rect.y = rect.y - origin.y;
            return rect;
        }

        //-----------------------------------------------------------------------------
        // Name: TestRectInside() (Public Extension)
        // Desc: Determines whether the specified rectangle is fully contained within
        //       this rectangle.
        // Parm: other - The rectangle which must be tested.
        // Rtrn: True if 'other' lies completely inside of 'this' rectangle; false otherwise.
        //-----------------------------------------------------------------------------
        public static bool TestRectInside(this Rect rect, Rect other)
        {
            // Sort rects
            rect  = rect.PositiveSize();
            other = other.PositiveSize();

            // All corner points must reside inside 'this' rect.
            if (other.xMin < rect.xMin || other.xMin > rect.xMax) return false;
            if (other.xMax < rect.xMin || other.xMax > rect.xMax) return false;

            if (other.yMin < rect.yMin || other.yMin > rect.yMax) return false;
            if (other.yMax < rect.yMin || other.yMax > rect.yMax) return false;

            // The other rect is completely inside
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: Clip() (Public Extension)
        // Desc: Clips the rectangle to the specified clipping rectangle.
        // Parm: clipRect    - Clipping area rectangle.
        //       clippedRect - Receives the clipped rectangle.
        // Rtrn: True if the rectangle is at least partially inside the clipping area.
        //       False if it lies completely outside or becomes degenerate after clipping.
        //-----------------------------------------------------------------------------
        public static bool Clip(this Rect rect, Rect clipRect, out Rect clippedRect)
        {
            // Default to original rect by default
            clippedRect = rect;

            // Force the rectangles to have positive size. It's easier this way.
            clippedRect = rect.PositiveSize();
            clipRect    = clipRect.PositiveSize();

            // Is the rectangle completely outside the clip rect?
            if (clippedRect.xMax <= clipRect.xMin) return false;
            if (clippedRect.xMin >= clipRect.xMax) return false;
            if (clippedRect.yMax <= clipRect.yMin) return false;
            if (clippedRect.yMin >= clipRect.yMax) return false;

            // Clip rectangle
            if (clippedRect.xMin < clipRect.xMin) clippedRect.xMin = clipRect.xMin;
            if (clippedRect.xMax > clipRect.xMax) clippedRect.xMax = clipRect.xMax;
            if (clippedRect.yMin < clipRect.yMin) clippedRect.yMin = clipRect.yMin;
            if (clippedRect.yMax > clipRect.yMax) clippedRect.yMax = clipRect.yMax;

            // Clipping can cause the rectangle to become degenerate
            if (clippedRect.width < 1e-5f || clippedRect.height < 1e-5f)
                return false;

            // Success!
            return true;
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: FromPoints() (Public Static Function)
        // Desc: Creates and returns a rectangle that encloses all the specified points.
        // Parm: points - Indexable collection of points which must be enclosed by the
        //                calculated rectangle.
        // Rtrn: A rectangle which encloses all points inside 'points'.
        //-----------------------------------------------------------------------------
        public static Rect FromPoints(IList<Vector2> points)
        {
            // Keep track of min/max coords
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;

            // Loop through each point
            int pointCount = points.Count();
            for (int i = 0; i < pointCount; ++i)
            {
                // Update min/max coords
                if (points[i].x < xMin) xMin = points[i].x;
                if (points[i].x > xMax) xMax = points[i].x;
                if (points[i].y < yMin) yMin = points[i].y;
                if (points[i].y > yMax) yMax = points[i].y;
            }

            // Return the rectangle
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        //-----------------------------------------------------------------------------
        // Name: FromPoints() (Public Static Function)
        // Desc: Creates and returns a rectangle that encloses all the specified points.
        // Parm: points - Indexable collection of points which must be enclosed by the
        //                calculated rectangle. Only points with a Z coord >= 0 are taken
        //                into account.
        // Rtrn: A rectangle which encloses all points inside 'points'. If all points
        //       are behind the camera, the returned rectangle has a negative width
        //       and height.
        //-----------------------------------------------------------------------------
        public static Rect FromPoints(IList<Vector3> points)
        {
            // Keep track of min/max coords
            bool found = false;
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMin = float.MaxValue;
            float yMax = float.MinValue;

            // Loop through each point
            int pointCount = points.Count();
            for (int i = 0; i < pointCount; ++i)
            {
                // Update min/max coords
                if (points[i].z >= 0.0f)
                {
                    if (points[i].x < xMin) xMin = points[i].x;
                    if (points[i].x > xMax) xMax = points[i].x;
                    if (points[i].y < yMin) yMin = points[i].y;
                    if (points[i].y > yMax) yMax = points[i].y;
                    found = true;
                }
            }

            // Return the rectangle
            return found ? new Rect(xMin, yMin, xMax - xMin, yMax - yMin) : new Rect(0.0f, 0.0f, -1.0f, -1.0f);
        }
        #endregion
    }
    #endregion
}
