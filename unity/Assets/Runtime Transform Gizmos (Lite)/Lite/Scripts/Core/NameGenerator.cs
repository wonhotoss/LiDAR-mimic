using System.Collections.Generic;
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: NameGenerator (Public Static Class)
    // Desc: Utility class used for generating names.
    //-----------------------------------------------------------------------------
    public static class NameGenerator
    {
        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: GenerateUnique() (Public Static Function)
        // Desc: Generates a name that is as close as possible to 'desiredName' and doesn't
        //       exist in the 'existingNames' list.
        // Parm: desiredName    - Desired name.
        //       existingNames  - List of existing names. Can be null or empty.
        // Rtrn: If 'desiredName' doesn't exist in 'existingNames' it will be returned
        //       unmodified. If it does exist, the function will return a modified version
        //       with a number suffix.
        //-----------------------------------------------------------------------------
        public static string GenerateUnique(string desiredName, List<string> existingNames)
        {
            // No names?
            if (existingNames == null || existingNames.Count == 0)
                return desiredName;

            //-----------------------------------------------------------------------------
            // Return value examples for different values of 'desiredName':
            // 1) 'desiredName' = Cube:
            //      a) Cube if 'existingNames' doesn't contain it.
            //      b) Cube (0) if 'existingNames' contains Cube.
            //      c) Cube (1) if 'existingNames' contains Cube and Cube (0).
            // 2) 'desiredName' = Cube (2)':
            //      a) Cube (2) if 'existingNames' doesn't contain it.
            //      b) Cube (3) if 'existingNames' contains Cube (2).
            //      c) Cube (4) if 'existingNames' contains Cube (2) and Cube (3).
            // This is how Unity seems to handle duplicate object names in the hierarchy view.
            //-----------------------------------------------------------------------------
            string  baseName  = desiredName;    // E.g. Cube (0) -> baseName is Cube
            string  finalName = desiredName;    // Final name returned to the caller
            int     intSuffix = 0;              // E.g, Cube (0) -> intSuffix is 0 

            // Normally names such as Cube, Props, Item, Library etc will be passed in, but
            // it is possible to receive an input name which already uses the bracket-suffix
            // format: Cube (1), Props (4) etc. In that case, what we want to do is to extract
            // the int suffix and build a base name which can be used to generate new names
            // in the format: baseName + "(" + intSuffix + "); 
            int openIndex = desiredName.LastIndexOf('(');
            if (openIndex >= 0)
            {
                // Get the closing bracket index
                int closeIndex = desiredName.LastIndexOf(")");

                // If we have a closing bracket and it doesn't immediately follow the opening
                // bracket, we have to extract the text in the middle and see that's there.
                if (closeIndex >= 0 && closeIndex - 1 != openIndex)
                {
                    // Extract the text between brackets and turn it into an int
                    string text = desiredName.Substring(openIndex + 1, closeIndex - (openIndex + 1));
                    if (Int32.TryParse(text, out intSuffix))
                    {
                        // If successful, we have to establish a base name to which we can easily add a number suffix later on
                        baseName = desiredName.Substring(0, openIndex).Trim();
                    }
                }
            }
            
            // Keep going until the generated name doesn't exist.
            while (existingNames.Contains(finalName))
            {
                // Generate a new name
                finalName = baseName + " (" + intSuffix + ")";

                // Increase int suffix
                ++intSuffix;
            }

            // Return final name
            return finalName;
        }
        #endregion
    }
    #endregion
}
