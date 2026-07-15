using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: SpriteRendererEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'SpriteRenderer'
    //       class.
    //-----------------------------------------------------------------------------
    public static class SpriteRendererEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: CalculateModelAABB() (Public Extension)
        // Desc: Calculates and returns the sprite renderer's model space AABB.
        // Rtrn: The sprite renderer's model space AABB. If the renderer doesn't have
        //       a sprite attached, an invalid AABB will be returned.
        //-----------------------------------------------------------------------------
        public static Box CalculateModelAABB(this SpriteRenderer spriteRenderer)
        {
            // No sprite attached?
            Sprite sprite = spriteRenderer.sprite;
            if (sprite == null) return Box.GetInvalid();

            // Return the model AABB based on the sprite's vertices
            return new Box(sprite.vertices);
        }
        #endregion
    }
    #endregion
}