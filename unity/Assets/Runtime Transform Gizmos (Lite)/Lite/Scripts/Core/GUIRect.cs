using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: GUIRect (Public Struct)
    // Desc: Represents a semantic GUI rectangle. This struct wraps Unity's 'Rect' and
    //       encodes GUI-space intent, enabling fluent, chainable layout and alignment
    //       operations while remaining interoperable with the underlying 'Rect' type.
    //-----------------------------------------------------------------------------
    public struct GUIRect
    {
        #region Private Fields
        Rect mRect; // Unity rectangle
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: position (Public Property)
        // Desc: Returns or sets the rectangle's position.
        //-----------------------------------------------------------------------------
        public Vector2  position    { get { return mRect.position; } set { mRect.position = value; } }

        //-----------------------------------------------------------------------------
        // Name: x (Public Property)
        // Desc: Returns or sets the rectangle's X position. Doesn't affect size.
        //-----------------------------------------------------------------------------
        public float    x           { get { return mRect.x; } set { mRect.x = value; } }

        //-----------------------------------------------------------------------------
        // Name: y (Public Property)
        // Desc: Returns or sets the rectangle's Y position. Doesn't affect size.
        //-----------------------------------------------------------------------------
        public float    y           { get { return mRect.y; } set { mRect.y = value; } }

        //-----------------------------------------------------------------------------
        // Name: center (Public Property)
        // Desc: Returns or sets the rectangle's center.
        //-----------------------------------------------------------------------------
        public Vector2  center      { get { return mRect.center; } set { mRect.center = value; } }

        //-----------------------------------------------------------------------------
        // Name: centerX (Public Property)
        // Desc: Returns or sets the rectangle's center along X.
        //-----------------------------------------------------------------------------
        public float    centerX     { get { return mRect.center.x; } set { mRect.x = value - width / 2.0f; } }

        //-----------------------------------------------------------------------------
        // Name: centerY (Public Property)
        // Desc: Returns or sets the rectangle's center along Y.
        //-----------------------------------------------------------------------------
        public float    centerY     { get { return mRect.center.y; } set { mRect.y = value - height / 2.0f; } }

        //-----------------------------------------------------------------------------
        // Name: size (Public Property)
        // Desc: Returns or sets the rectangle's size.
        //-----------------------------------------------------------------------------
        public Vector2  size        { get { return mRect.size; } set { mRect.size = value.Abs(); } }

        //-----------------------------------------------------------------------------
        // Name: width (Public Property)
        // Desc: Returns or sets the rectangle's width.
        //-----------------------------------------------------------------------------
        public float    width       { get { return mRect.width; } set { mRect.width = Mathf.Abs(value); } }
        
        //-----------------------------------------------------------------------------
        // Name: height (Public Property)
        // Desc: Returns or sets the rectangle's height.
        //-----------------------------------------------------------------------------
        public float    height      { get { return mRect.height; } set { mRect.height = Mathf.Abs(value); } }

        //-----------------------------------------------------------------------------
        // Name: xMin (Public Property)
        // Desc: Returns or sets the rectangle's minimum X coordinate. The setter affects
        //       the size.
        //-----------------------------------------------------------------------------
        public float    xMin        { get { return mRect.xMin; } set { mRect.xMin = value; mRect.PositiveSize(); } }

        //-----------------------------------------------------------------------------
        // Name: xMax (Public Property)
        // Desc: Returns or sets the rectangle's maximum X coordinate. The setter affects
        //       the size.
        //-----------------------------------------------------------------------------
        public float    xMax        { get { return mRect.xMax; } set { mRect.xMax = value; mRect.PositiveSize(); } }

        //-----------------------------------------------------------------------------
        // Name: yMin (Public Property)
        // Desc: Returns or sets the rectangle's minimum Y coordinate. The setter affects
        //       the size.
        //-----------------------------------------------------------------------------
        public float    yMin        { get { return mRect.yMin; } set { mRect.yMin = value; mRect.PositiveSize(); } }

        //-----------------------------------------------------------------------------
        // Name: yMax (Public Property)
        // Desc: Returns or sets the rectangle's maximum Y coordinate. The setter affects
        //       the size.
        //-----------------------------------------------------------------------------
        public float    yMax        { get { return mRect.yMax; } set { mRect.yMax = value; mRect.PositiveSize(); } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: RTGUIRect() (Public Constructor)
        // Desc: Creates an RTGUI rectangle form the given Unity rectangle.
        // Parm: rect - Unity rectangle to create the 'RTGUIRect' from.
        //-----------------------------------------------------------------------------
        public GUIRect(Rect rect)
        {
            mRect = rect.PositiveSize();
        }

        //-----------------------------------------------------------------------------
        // Name: RTGUIRect() (Public Constructor)
        // Desc: Creates an RTGUI rectangle form the given position and dimensions.
        // Parm: x      - Rectangle X position.
        //       y      - Rectangle Y position.
        //       width  - Rectangle width. 
        //       height - Rectangle height.
        //-----------------------------------------------------------------------------
        public GUIRect(float x, float y, float width, float height)
        {
            mRect = new Rect(x, y, width, height).PositiveSize();
        }

        //-----------------------------------------------------------------------------
        // Name: RTGUIRect() (Public Constructor)
        // Desc: Creates an RTGUI rectangle form the given position and size.
        // Parm: x      - Rectangle X position.
        //       y      - Rectangle Y position.
        //       size   - Rectangle width and height.
        //-----------------------------------------------------------------------------
        public GUIRect(float x, float y, float size)
        {
            mRect = new Rect(x, y, size, size).PositiveSize();
        }
        #endregion

        #region Public Operators
        public static implicit operator Rect(GUIRect guiRect) => guiRect.mRect;
        public static explicit operator GUIRect(Rect rect)    => new GUIRect(rect);
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Contains() (Public Function)
        // Desc: Checks whether the rectangle contains the specified point.
        // Parm: pt - Point to test for containment.
        // Rtrn: True if the point is inside the rectangle; false otherwise.
        //-----------------------------------------------------------------------------
        public bool Contains(Vector2 pt)
        {
            return mRect.Contains(pt);
        }

        //-----------------------------------------------------------------------------
        // Name: Below() (Public Function)
        // Desc: Clones the rectangle and places the clone below the given reference
        //       rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting below 'refRect'.
        //-----------------------------------------------------------------------------
        public GUIRect Below(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.y = refRect.yMax;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: Above() (Public Function)
        // Desc: Clones the rectangle and places the clone above the given reference
        //       rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting above 'refRect'.
        //-----------------------------------------------------------------------------
        public GUIRect Above(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.y = refRect.yMin - rc.height;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: LeftOf() (Public Function)
        // Desc: Clones the rectangle and places the clone to the left of the given reference
        //       rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting to the left of 'refRect'.
        //-----------------------------------------------------------------------------
        public GUIRect LeftOf(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.x = refRect.xMin - rc.width;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: RightOf() (Public Function)
        // Desc: Clones the rectangle and places the clone to the right of the given reference
        //       rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting to the right of 'refRect'.
        //-----------------------------------------------------------------------------
        public GUIRect RightOf(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.x = refRect.xMax;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: TopLeftOf() (Public Function)
        // Desc: Clones the rectangle and places the clone in the top left corner of the
        //       given reference rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting in the top left corner of 'refRect'.
        //-----------------------------------------------------------------------------
        public GUIRect TopLeftOf(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.x = refRect.xMin;
            rc.y = refRect.yMin;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: TopRightOf() (Public Function)
        // Desc: Clones the rectangle and places the clone in the top right corner of the
        //       given reference rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting in the top right corner of 'refRect'.
        //-----------------------------------------------------------------------------
        public GUIRect TopRightOf(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.x = refRect.xMax - rc.width;
            rc.y = refRect.yMin;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: BottomLeftOf() (Public Function)
        // Desc: Clones the rectangle and places the clone in the bottom left corner of the
        //       given reference rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting in the bottom left corner of 'refRect'.
        //-----------------------------------------------------------------------------
        public GUIRect BottomLeftOf(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.x = refRect.xMin;
            rc.y = refRect.yMax - rc.height;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: BottomRightOf() (Public Function)
        // Desc: Clones the rectangle and places the clone in the bottom right corner of the
        //       given reference rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting in the bottom right corner of 'refRect'.
        //-----------------------------------------------------------------------------
        public GUIRect BottomRightOf(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.x = refRect.xMax - rc.width;
            rc.y = refRect.yMax - rc.height;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: CenterOf() (Public Function)
        // Desc: Clones the rectangle by placing it in the center of the specified
        //       reference rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting in the center of the reference rectangle.
        //-----------------------------------------------------------------------------
        public GUIRect CenterOf(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.center = refRect.center;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: CenterXOf() (Public Function)
        // Desc: Clones the rectangle by placing it in the center of the specified
        //       reference rectangle along the X axis.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting in the center of the reference rectangle along
        //       the X axis.
        //-----------------------------------------------------------------------------
        public GUIRect CenterXOf(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.centerX = refRect.centerX;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: CenterYOf() (Public Function)
        // Desc: Clones the rectangle by placing it in the center of the specified
        //       reference rectangle along the Y axis.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle sitting in the center of the reference rectangle along
        //       the Y axis.
        //-----------------------------------------------------------------------------
        public GUIRect CenterYOf(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.centerY = refRect.centerY;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: AlignLeft() (Public Function)
        // Desc: Clones the rectangle by aligning it to the left edge of the specified
        //       reference rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle aligned to the left edge of the reference rectangle.
        //-----------------------------------------------------------------------------
        public GUIRect AlignLeft(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.x = refRect.xMin;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: AlignRight() (Public Function)
        // Desc: Clones the rectangle by aligning it to the right edge of the specified
        //       reference rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle aligned to the right edge of the reference rectangle.
        //-----------------------------------------------------------------------------
        public GUIRect AlignRight(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.x = refRect.xMax - rc.width;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: AlignTop() (Public Function)
        // Desc: Clones the rectangle by aligning it to the top edge of the specified
        //       reference rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle aligned to the top edge of the reference rectangle.
        //-----------------------------------------------------------------------------
        public GUIRect AlignTop(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.y = refRect.yMin;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: AlignBottom() (Public Function)
        // Desc: Clones the rectangle by aligning it to the bottom edge of the specified
        //       reference rectangle.
        // Parm: refRect - Reference rectangle.
        // Rtrn: The new rectangle aligned to the bottom edge of the reference rectangle.
        //-----------------------------------------------------------------------------
        public GUIRect AlignBottom(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.y = refRect.yMax - rc.height;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: WithPosition() (Public Function)
        // Desc: Clones the rectangle with the specified position.
        // Parm: pos - The position of the cloned rectangle.
        // Rtrn: The new rectangle with its position set to 'pos'.
        //-----------------------------------------------------------------------------
        public GUIRect WithPosition(Vector2 pos)
        {
            GUIRect rc = this;
            rc.position = pos;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: WithSize() (Public Function)
        // Desc: Clones the rectangle with the specified size.
        // Parm: size - The size of the cloned rectangle.
        // Rtrn: The new rectangle with its size set to 'size'.
        //-----------------------------------------------------------------------------
        public GUIRect WithSize(Vector2 size)
        {
            GUIRect rc = this;
            rc.size = size;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: WithSize() (Public Function)
        // Desc: Clones the rectangle with the specified size.
        // Parm: width  - The width of the cloned rectangle.
        //       height - The height of the cloned rectangle.
        // Rtrn: The new rectangle with its dimensions set to 'width' and 'height'.
        //-----------------------------------------------------------------------------
        public GUIRect WithSize(float width, float height)
        {
            GUIRect rc = this;
            rc.width    = width;
            rc.height   = height;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: WithWidth() (Public Function)
        // Desc: Clones the rectangle with the specified width.
        // Parm: width - The width of the cloned rectangle.
        // Rtrn: The new rectangle with its width set to 'width'.
        //-----------------------------------------------------------------------------
        public GUIRect WithWidth(float width)
        {
            GUIRect rc    = this;
            rc.width        = width;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: WithHeight() (Public Function)
        // Desc: Clones the rectangle with the specified height.
        // Parm: height - The height of the cloned rectangle.
        // Rtrn: The new rectangle with its height set to 'height'.
        //-----------------------------------------------------------------------------
        public GUIRect WithHeight(float height)
        {
            GUIRect rc = this;
            rc.height    = height;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: Offset() (Public Function)
        // Desc: Clones the rectangle and offsets its position by the specified amount.
        // Parm: offset - Position offset.
        // Rtrn: The new rectangle with its position offset by the specified amount.
        //-----------------------------------------------------------------------------
        public GUIRect Offset(Vector2 offset)
        {
            GUIRect rc = this;
            rc.position += offset;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: Offset() (Public Function)
        // Desc: Clones the rectangle and offsets its position by the specified amount.
        // Parm: offsetX - X position offset.
        //       offsetY - Y position offset.
        // Rtrn: The new rectangle with its position offset by the specified amount.
        //-----------------------------------------------------------------------------
        public GUIRect Offset(float offsetX, float offsetY)
        {
            GUIRect rc = this;
            rc.x += offsetX;
            rc.y += offsetY;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: OffsetX() (Public Function)
        // Desc: Clones the rectangle and offsets its X position by the specified amount.
        // Parm: offsetX - X position offset.
        // Rtrn: The new rectangle with its X position offset by the specified amount.
        //-----------------------------------------------------------------------------
        public GUIRect OffsetX(float offsetX)
        {
            GUIRect rc = this;
            rc.x += offsetX;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: OffsetY() (Public Function)
        // Desc: Clones the rectangle and offsets its Y position by the specified amount.
        // Parm: offsetX - Y position offset.
        // Rtrn: The new rectangle with its Y position offset by the specified amount.
        //-----------------------------------------------------------------------------
        public GUIRect OffsetY(float offsetY)
        {
            GUIRect rc = this;
            rc.y += offsetY;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: RelativeTo (Public Function)
        // Desc: Clones the rectangle with its position expressed relative to the specified
        //       reference rectangle's position.
        // Parm: refRect - The reference rectangle.
        // Rtrn: The new rectangle with its position expressed relative to 'refRect'.
        //-----------------------------------------------------------------------------
        public GUIRect RelativeTo(GUIRect refRect)
        {
            GUIRect rc = this;
            rc.x = rc.x - refRect.x;
            rc.y = rc.y - refRect.y;
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: CenterOnScreen() (Public Function)
        // Desc: Clones the rectangle by centering it on the screen.
        // Rtrn: The cloned rectangle centered on the screen.
        //-----------------------------------------------------------------------------
        public GUIRect CenterOnScreen()
        {
            // Calculate the screen middle
            float midX = Screen.width   / 2.0f;
            float midY = Screen.height  / 2.0f;

            // Calculate the rectangle position
            GUIRect rc = this;
            rc.position = new Vector2(midX - rc.width / 2.0f, midY - rc.height / 2.0f);

            // Return cloned rectangle
            return rc;
        }

        //-----------------------------------------------------------------------------
        // Name: Clip() (Public Function)
        // Desc: Clips the rectangle to the specified clip rectangle and returns the
        //       clipped rectangle.
        // Parm: clipRect - Clipping area rectangle.
        // Rtrn: The clipped rectangle. If the original rectangle lies completely outside
        //       the clipping area, a rectangle with a size of 0 is returned.
        //-----------------------------------------------------------------------------
        public GUIRect Clip(GUIRect clipRect)
        {
            // Start from the original rectangle
            GUIRect rc = this;

            // Is the rectangle completely outside the clip rect?
            if (rc.xMax <= clipRect.xMin ||
                rc.xMin >= clipRect.xMax ||
                rc.yMax <= clipRect.yMin ||
                rc.yMin >= clipRect.yMax) return new GUIRect(0.0f, 0.0f, 0.0f);

            // Clip rectangle
            if (rc.xMin < clipRect.xMin) rc.xMin = clipRect.xMin;
            if (rc.xMax > clipRect.xMax) rc.xMax = clipRect.xMax;
            if (rc.yMin < clipRect.yMin) rc.yMin = clipRect.yMin;
            if (rc.yMax > clipRect.yMax) rc.yMax = clipRect.yMax;

            // Return new rectangle
            return rc;
        }

        //-----------------------------------------------------------------------------
	    // Name: KeepInside() (Public Function)
	    // Desc: Keeps the rectangle within the specified bounds.
	    // Parm: bounds - Defines the area where the rectangle is allowed to exist.
	    // Rtrn: The new rectangle.
	    //-----------------------------------------------------------------------------
        public GUIRect KeepInside(GUIRect bounds)
        {
            GUIRect rc = this;

            // Push coordinates inside
			if (rc.xMin < bounds.xMin)
			{
				float dx = bounds.xMin - rc.xMin;
                rc.x += dx;
			}
			if (rc.yMin < bounds.yMin)
			{
				float dy = bounds.yMin - rc.yMin;
				rc.y += dy;
			}
			if (rc.xMax > bounds.xMax)
			{
                float dx = bounds.xMax - rc.xMax;
				rc.x += dx;
			}
			if (rc.yMax > bounds.yMax)
			{
                float dy = bounds.yMax - rc.yMax;
				rc.y += dy;
			}

            // Return new rect
            return rc;
        }
        #endregion
    }
    #endregion
}