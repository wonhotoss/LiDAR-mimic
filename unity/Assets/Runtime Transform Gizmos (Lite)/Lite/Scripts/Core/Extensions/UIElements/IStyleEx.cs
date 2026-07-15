using UnityEngine;
using UnityEngine.UIElements;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: IStyleEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'IStyle' interface.
    //-----------------------------------------------------------------------------
    public static class IStyleEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: SetDisplayVisible() (Public Extension)
        // Desc: Sets the style's display visibility.
        // Parm: visible - If true, the control will be visible. If false, the control
        //                 will be hidden and it won't take up any space in the control
        //                 layout.
        //-----------------------------------------------------------------------------
        public static void SetDisplayVisible(this IStyle style, bool visible)
        {
            style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        //-----------------------------------------------------------------------------
        // Name: IsDisplayVisible() (Public Extension)
        // Desc: Checks if the style's display is visible.
        // Rtrn: True if the style's display is visible and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool IsDisplayVisible(this IStyle style)
        {
            return style.display == DisplayStyle.Flex;
        }

        //-----------------------------------------------------------------------------
        // Name: SetBorderColor() (Public Extension)
        // Desc: Sets the left/right/bottom/top border color.
        // Parm: color - Left/right/bottom/top border color.
        //-----------------------------------------------------------------------------
        public static void SetBorderColor(this IStyle style, Color color)
        {
            style.borderLeftColor     = color;
            style.borderRightColor    = color;
            style.borderBottomColor   = color;
            style.borderTopColor      = color;
        }

        //-----------------------------------------------------------------------------
        // Name: SetBorderWidth() (Public Extension)
        // Desc: Sets the left/right/bottom/top border width.
        // Parm: width - Left/right/bottom/top border width.
        //-----------------------------------------------------------------------------
        public static void SetBorderWidth(this IStyle style, float width)
        {
            style.borderLeftWidth     = width;
            style.borderRightWidth    = width;
            style.borderBottomWidth   = width;
            style.borderTopWidth      = width;
        }

        //-----------------------------------------------------------------------------
        // Name: SetBorderRadius() (Public Extension)
        // Desc: Sets the border radius for all corners.
        // Parm: radius - Border radius for all corners.
        //-----------------------------------------------------------------------------
        public static void SetBorderRadius(this IStyle style, float radius)
        {
            style.borderBottomLeftRadius    = radius;
            style.borderBottomRightRadius   = radius;
            style.borderTopLeftRadius       = radius;
            style.borderTopRightRadius      = radius;
        }

        //-----------------------------------------------------------------------------
        // Name: SetMargin() (Public Extension)
        // Desc: Sets the left/right/bottom/top margin.
        // Parm: margin - Left/right/bottom/top margin.
        //-----------------------------------------------------------------------------
        public static void SetMargin(this IStyle style, float margin)
        {
            style.marginLeft      = margin;
            style.marginRight     = margin;
            style.marginBottom    = margin;
            style.marginTop       = margin;
        }

        //-----------------------------------------------------------------------------
        // Name: EnableRoundBorder() (Public Extension)
        // Desc: Changes the style so that it draws round borders. The current border
        //       color is preserved.
        //-----------------------------------------------------------------------------
        public static void EnableRoundBorder(this IStyle style)
        {
            style.SetBorderWidth(1.0f);
            style.SetBorderRadius(3.0f);
        }

        //-----------------------------------------------------------------------------
        // Name: SetBackgroundImage() (Public Extension)
        // Desc: Sets the background image and also updates the fixed width and height
        //       to be the same as the image's width and height.
        // Parm: image - Background image. Can be null.
        //-----------------------------------------------------------------------------
        public static void SetBackgroundImage(this IStyle style, Texture2D image)
        {
            // Set background image
            style.backgroundImage = image;

            // If the image is valid, set width and height
            if (image != null)
            {
                style.width = image.width;
                style.height = image.height;
            }
        }
        #endregion
    }
    #endregion
}
