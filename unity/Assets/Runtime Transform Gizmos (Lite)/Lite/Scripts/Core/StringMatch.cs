namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: StringMatch (Public Static Class)
    // Desc: Implements string matching utility functions.
    //-----------------------------------------------------------------------------
    public static class StringMatch
    {
        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: Match() (Public Static Function)
        // Desc: Checks if 'str' matches 'matchTarget'. A match occurs when the 2 strings
        //       are equal or when 'str' contains 'matchTarget'.
        // Parm: str           - The string to match against 'matchTarget'.
        //       matchTarget   - Match target.
        //       caseSensitive - If true, a case sensitive comparison is performed.
        // Rtrn: True if 'str' is equal to 'matchTarget' or if 'str' contains 'matchTarget'.
        //-----------------------------------------------------------------------------
        public static bool Match(string str, string matchTarget, bool caseSensitive)
        {
            // We have a match if the 2 strings are equal
            if (str == matchTarget) return true;

            // Case sensitive?
            if (caseSensitive)
            {
                // If the string contains 'stringToMatch', we have a match
                if (str.Contains(matchTarget))
                    return true;
            }
            else
            {
                // Convert both strings to lower case
                str             = str.ToLower();
                matchTarget     = matchTarget.ToLower();

                // If the string contains 'stringToMatch', we have a match
                if (str.Contains(matchTarget))
                    return true;
            }

            // No match
            return false;
        }
        #endregion
    }
    #endregion
}
