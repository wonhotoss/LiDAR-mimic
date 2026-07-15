using UnityEngine;
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Tween (Public Class)
    // Desc: Implements tween effects for animating different kinds of data types
    //       like 'float', 'Color', 'Vector3'.
    // Parm: T - The type to animate.
    //-----------------------------------------------------------------------------
    public class Tween<T>
    {
        #region Private Fields
        bool    mActive;        // Is the tween effect active?
        float   mDuration;      // The tween's duration in seconds
        float   mElapsedTime;   // The amount of time that has passed since the beginning of the effect
        T       mStartVal;      // Start value
        T       mEndVal;        // End value
        T       mVal;           // The interpolated value
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: active (Public Property)
        // Desc: Returns whether or not the effect is currently active.
        //-----------------------------------------------------------------------------
        public bool         active          { get { return mActive; } }

        //-----------------------------------------------------------------------------
        // Name: startValue (Public Property)
        // Desc: Returns the tween's start value.
        //-----------------------------------------------------------------------------
        public T            startValue      { get { return mStartVal; } }

        //-----------------------------------------------------------------------------
        // Name: endValue (Public Property)
        // Desc: Returns the tween's end value.
        //-----------------------------------------------------------------------------
        public T            endValue        { get { return mEndVal; } }

        //-----------------------------------------------------------------------------
        // Name: value (Public Property)
        // Desc: Returns the interpolated value.
        //-----------------------------------------------------------------------------
        public T            value           { get { return mVal; } }

        //-----------------------------------------------------------------------------
        // Name: tweenUpdated (Public Property)
        // Desc: Returns or sets the action to execute when the tween is updated. The
        //       single parameter represents the value that was produced during the 
        //       update step.
        //-----------------------------------------------------------------------------
        public Action<T>    tweenUpdated    { get; set; }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Start() (Public Function)
        // Desc: Starts the tween effect.
        // Parm: startVal - Start value.
        //       endVal   - End value.
        //       duration - The amount of time in seconds it takes for the animation
        //                  to complete.
        //-----------------------------------------------------------------------------
        public void Start(T startVal, T endVal, float duration)
        {
            // End in case we're already active
            End();

            // Store data
            mStartVal   = startVal;
            mEndVal     = endVal;
            mDuration   = duration;

            // We're now active
            mActive = true;
        }

        //-----------------------------------------------------------------------------
        // Name: Update() (Public Function)
        // Desc: Updates the tween effect. If the effect is not active, the function
        //       exits immediately.
        //-----------------------------------------------------------------------------
        public void Update()
        {
            // Are we active?
            if (!mActive) return;

            // Update elapsed time and calculate interpolant
            mElapsedTime += Time.smoothDeltaTime;
            float t = Mathf.Clamp01(mElapsedTime / mDuration);

            // Interpolate
            mVal = Interpolate(t);
            if (tweenUpdated != null)
                tweenUpdated(mVal);

            // End if we're done
            if (mElapsedTime >= mDuration)
                End();
        }

        //-----------------------------------------------------------------------------
        // Name: End() (Public Function)
        // Desc: Ends the tween effect.
        //-----------------------------------------------------------------------------
        public void End()
        {
            mActive         = false;
            mElapsedTime    = 0.0f;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: Interpolate() (Private Function)
        // Desc: Interpolates between the start and end values.
        // Parm: t - Interpolant in the [0, 1] interval.
        // Rtrn: Interpolated value.
        //-----------------------------------------------------------------------------
        T Interpolate(float t)
        {
            if (mStartVal is float startFloat && mEndVal is float endFloat)
                return (T)(object)Mathf.LerpUnclamped(startFloat, endFloat, t);

            if (mStartVal is Color startColor && mEndVal is Color endColor)
                return (T)(object)Color.LerpUnclamped(startColor, endColor, t);

            if (mStartVal is Vector3 startVec3 && mEndVal is Vector3 endVec3)
                return (T)(object)Vector3.LerpUnclamped(startVec3, endVec3, t);

            return default(T);
        }
        #endregion
    }
    #endregion
}
