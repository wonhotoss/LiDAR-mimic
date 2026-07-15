using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: LayerEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for working with layer
    //       masks.
    //-----------------------------------------------------------------------------
    public static class LayerEx
    {
        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: minLayerIndex (Public Static Property)
        // Desc: Returns the minimum layer index.
        //-----------------------------------------------------------------------------
        public static int minLayerIndex { get { return 0; } }

        //-----------------------------------------------------------------------------
        // Name: maxLayerIndex (Public Static Property)
        // Desc: Returns the maximum layer index.
        //-----------------------------------------------------------------------------
        public static int maxLayerIndex { get { return 31; } }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: CheckLayerBit() (Public Static Function)
        // Desc: Checks if a layer's bit is set inside the specified layer mask.
        // Parm: layerIndex - Layer index.
        //       layerMask  - Layer mask.
        // Rtrn: True if the layer's bit is set inside the specified layer mask and false
        //       otherwise.
        //-----------------------------------------------------------------------------
        public static bool CheckLayerBit( int layerIndex, int layerMask)
        {
            return (layerMask & (1 << layerIndex)) != 0;
        }

        //-----------------------------------------------------------------------------
        // Name: SetLayerBit() (Public Static Function)
        // Desc: Sets the specified layer's bit inside the specified layer mask.
        // Parm: layerIndex - Layer index.
        //       layerMask  - Layer mask.
        // Rtrn: The new mask with the specified layer's bit set to 1.
        //-----------------------------------------------------------------------------
        public static int SetLayerBit(int layerIndex, int layerMask)
        {
            return layerMask | (1 << layerIndex);
        }

        //-----------------------------------------------------------------------------
        // Name: ClearLayerBit() (Public Static Function)
        // Desc: Clears the specified layer's bit inside the specified layer mask.
        // Parm: layerIndex - Layer index.
        //       layerMask  - Layer mask.
        // Rtrn: The new mask with the specified layer's bit set to 0.
        //-----------------------------------------------------------------------------
        public static int ClearLayerBit(int layerIndex, int layerMask)
        {
            return layerMask & (~(1 << layerIndex));
        }

        //-----------------------------------------------------------------------------
        // Name: IsLayerValid() (Public Static Function)
        // Desc: Checks if the specified layer index is valid.
        // Parm: layerIndex - The index of the layer which must be validated.
        //-----------------------------------------------------------------------------
        public static bool IsLayerValid(int layerIndex)
        {
            return layerIndex >= minLayerIndex && layerIndex <= maxLayerIndex;
        }

        //-----------------------------------------------------------------------------
        // Name: CollectLayerNames() (Public Static Function)
        // Desc: Collects the names of all existing layers and stores them inside 'layerNames'.
        // Parm: layerNames - Returns the layer names.
        //-----------------------------------------------------------------------------
        public static void CollectLayerNames(List<string> layerNames)
        {
            // Clear names list
            layerNames.Clear();

            // Loop through each layer
            for (int layerIndex = minLayerIndex; layerIndex <= maxLayerIndex; ++layerIndex)
            {
                // Get layer name and if it's valid, add it to the list
                string layerName = LayerMask.LayerToName(layerIndex);
                if (!string.IsNullOrEmpty(layerName)) layerNames.Add(layerName);
            }
        }
        #endregion
    }
    #endregion
}
