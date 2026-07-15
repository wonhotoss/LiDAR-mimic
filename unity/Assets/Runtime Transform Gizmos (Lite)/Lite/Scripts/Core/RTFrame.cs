namespace RTGLite
{
    //-----------------------------------------------------------------------------
    // Name: RTFrame (Public Static Class)
    // Desc: Provides a deterministic, 64-bit frame index for the runtime.
    //       This index is incremented once per frame by the root runtime module
    //       and serves as a stable temporal reference for systems such as Undo,
    //       input edge grouping, and other frame-bound logic.
    // Note: Unlike Unity's Time.frameCount (which exposes a 32-bit value),
    //       this counter uses 64 bits and will not wrap in any practical
    //       runtime scenario.
    //-----------------------------------------------------------------------------
    public static class RTFrame
    {
        #region Private Fields
        static ulong sFrameIndex;       // Current frame index
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: index (Public Property)
        // Desc: Returns the current 64-bit frame index.
        //-----------------------------------------------------------------------------
        public static ulong index
        {
            get { return sFrameIndex; }
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Tick() (Public Function)
        // Desc: Advances the frame index by one. Must be called exactly once per
        //       frame by the root runtime module before other frame-dependent
        //       systems execute.
        //-----------------------------------------------------------------------------
        public static void Tick()
        {
            ++sFrameIndex;
        }

        //-----------------------------------------------------------------------------
        // Name: Reset() (Public Function)
        // Desc: Resets the frame index to zero. Intended for runtime reinitialization
        //       scenarios such as scene reload or full system restart.
        //-----------------------------------------------------------------------------
        public static void Reset()
        {
            sFrameIndex = 0;
        }
        #endregion
    }
}