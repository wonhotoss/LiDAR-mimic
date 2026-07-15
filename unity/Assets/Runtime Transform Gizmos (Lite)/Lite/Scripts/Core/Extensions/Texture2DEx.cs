using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Texture2DEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Texture2D' class.
    //-----------------------------------------------------------------------------
    public static class Texture2DEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: GetHalfSize() (Public Extension)
        // Desc: Returns the texture's half size.
        // Rtrn: Texture half size.
        //-----------------------------------------------------------------------------
        public static Vector2 GetHalfSize(this Texture2D texture)
        {
            return new Vector2(texture.width / 2.0f, texture.height / 2.0f);
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: CreateFromSource() (Public Static Function)
        // Desc: Creates a 2D texture from the specified source texture and pixel block.
        // Parm: source         - Source texture.
        //       blockX         - Starting X coordinate of the source pixel block.
        //       blockY         - Starting Y coordinate of the source pixel block.
        //       blockWidth     - Width of the source pixel block.
        //       blockHeight    - Height of the source pixel block.
        // Rtrn: The created 2D texture.
        //-----------------------------------------------------------------------------
        public static Texture2D CreateFromSource(Texture2D source, int blockX, int blockY, int blockWidth, int blockHeight)
        {
            // Create texture
            Texture2D texture = new Texture2D(blockWidth, blockHeight, source.format, true);
            texture.filterMode = source.filterMode;

            // Copy pixels from the source texture
            Color[] pixels = source.GetPixels(blockX, blockY, blockWidth, blockHeight);
            texture.SetPixels(pixels);
            texture.Apply(true);

            // Return the texture
            return texture;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateFillTexture() (Public Static Function)
        // Desc: Creates a texture filled with the specified color.
        // Parm: color  - Texture fill color.
        //       width  - Texture width.
        //       height - Texture height.
        // Rtrn: The created texture.
        //-----------------------------------------------------------------------------
        public static Texture2D CreateFillTexture(Color color, int width, int height)
        {
            // Create an RGBA texture
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);

            // Fill texture
            Color[] fillColor = ColorEx.CreateColorArray(color, width * height);
            texture.SetPixels(fillColor);

            // Apply changes
            texture.Apply(false, true);

            // Prevent the texture from being saved or destroyed unintentionally
            texture.hideFlags = HideFlags.HideAndDontSave;

            // Return texture
            return texture;
        }
        #endregion
    }
    #endregion
}
