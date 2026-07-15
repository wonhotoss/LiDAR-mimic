using UnityEngine;
using System.Text.RegularExpressions;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: StringEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'string' class.
    //-----------------------------------------------------------------------------
    public static class StringEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: RemoveWhiteSpace() (Public Extension)
        // Desc: Removes all whitespace from the string and returns the new string with
        //       whitespace removed.
        // Rtrn: The new string with whitespace removed.
        //-----------------------------------------------------------------------------
        public static string RemoveWhiteSpace(this string s)
        {
            return Regex.Replace(s, @"\s", string.Empty);
        }

        //-----------------------------------------------------------------------------
        // Name: IsPrecededByAn() (Public Extension)
        // Desc: The function makes the assumption the string is a noun and returns true
        //       if this is the kind of noun that should be preceded by 'an' instead of 'a'.
        // Rtrn: True if the string is the kind of noun that should be preceded by 'an'
        //       instead of 'a' and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool IsPrecededByAn(this string s)
        {
            // Null or empty?
            if (string.IsNullOrEmpty(s)) return false;

            // Nouns that start with 'a, o, i' should be preceded by 'an'
            return s[0] == 'a' || s[0] == 'o' || s[0] == 'i';
        }
        #endregion
    }
    #endregion
}
