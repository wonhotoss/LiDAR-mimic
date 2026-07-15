using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ColorEx (Public Static Class)
    // Desc: Implements useful extensions and utility functions for the 'Color' struct.      
    //-----------------------------------------------------------------------------
    public static class ColorEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: WithAlpha() (Public Extension)
        // Desc: Creates a new color with the same RGB values, but replaces the alpha
        //       value with 'alpha'.
        // Parm: alpha - The new alpha value.
        // Rtrn: A new 'Color' instance with the same RGB components as the original
        //       color but with its alpha value set to 'alpha'.
        //-----------------------------------------------------------------------------
        public static Color WithAlpha(this Color color, float alpha) 
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        //-----------------------------------------------------------------------------
        // Name: ScaleAlpha() (Public Extension)
        // Desc: Creates a new color with the same RGB values, but scales the alpha
        //       using 'alphaScale'.
        // Parm: alphaScale - The alpha scale value used to produce a new alpha.
        // Rtrn: A new 'Color' instance with the same RGB components as the original
        //       color but with its alpha value scaled by 'alphaScale'.
        //-----------------------------------------------------------------------------
        public static Color ScaleAlpha(this Color color, float alphaScale)
        {
            return new Color(color.r, color.g, color.b, color.a * alphaScale);
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: FromRGBBytes() (Public Static Function)
        // Desc: Creates an opaque color from the specified RGB byte values.
        // Parm: r, g, b - RGB byte values.
        // Rtrn: An opaque color with the specified RGB values.
        //-----------------------------------------------------------------------------
        public static Color FromRGBBytes(byte r, byte g, byte b)
        {
            return new Color(r / 255.0f, g / 255.0f, b / 255.0f, 1.0f);
        }

        //-----------------------------------------------------------------------------
        // Name: FromRGBABytes() (Public Static Function)
        // Desc: Creates a color from the specified RGBA byte values.
        // Parm: r, g, b, a - RGBA byte values.
        // Rtrn: A color with the specified RGBA values.
        //-----------------------------------------------------------------------------
        public static Color FromRGBABytes(byte r, byte g, byte b, byte a)
        {
            return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        //-----------------------------------------------------------------------------
        // Name: CreateColorArray() (Public Static Function)
        // Desc: Creates a color array of length 'arrayLength' where each element is set
        //       to 'color'.
        // Parm: color       - The color value of each element in the array.
        //       arrayLength - The number of elements in the array.
        // Rtrn: An array of length 'arrayLength' where each element is set to 'color'.
        //-----------------------------------------------------------------------------
        public static Color[] CreateColorArray(Color color, int arrayLength)
        {
            // Allocate array and set each item to the specified color
            Color[] colors = new Color[arrayLength];
            for (int i = 0; i < arrayLength; ++i)
                colors[i] = color;

            // Return array
            return colors;
        }
        #endregion
    }
    #endregion
}
