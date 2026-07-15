using System.Collections;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: CameraProjectionSwitch (Public Class)
    // Desc: Implements a camera projection switch. This is useful for implementing
    //       effects such as the one that occurs when the user clicks on the scene
    //       gizmo inside the Unity Editor to switch from perspective to ortho or
    //       vice versa.
    //-----------------------------------------------------------------------------
    public class CameraProjectionSwitch
    {
        #region Private Fields
        Camera      mTargetCamera;  // The target camera
        Coroutine   mCrtn;          // The coroutine that implements the switch
        bool        mToOrtho;       // If true, we're switching from perspective to ortho. False, means ortho to perspective.
        float       mZoom;          // Camera zoom. This is the distance in front of the camera where the zoom point resides.
        float       mDuration;      // The amount of time in seconds it takes to complete the switch
        float       mFOV;           // Original camera FOV. We need this because the camera's 'fieldOfView' property will change during the effect.
        float       mStartFOV;      // The starting field of view
        float       mTargetFOV;     // The target FOV that must be achieved to complete the switch
        float       mProgress;      // Switch progress between [0, 1]
        Vector3     mOriginalPos;   // Original camera position. The switch effect also changes the camera position, but it needs to be restored when we're done.
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: active (Public Property)
        // Desc: Returns whether or not the switch operation is active (i.e. in progress).
        //-----------------------------------------------------------------------------
        public bool     active      { get { return mCrtn != null; } }

        //-----------------------------------------------------------------------------
        // Name: progress (Public Property)
        // Desc: Returns a value between [0, 1] which represents the switch progress.
        //       0 means just started, 1 means completed.
        //-----------------------------------------------------------------------------
        public float    progress    { get { return mProgress; } }

        //-----------------------------------------------------------------------------
        // Name: toOrtho (Public Property)
        // Desc: Returns true when switching to an ortho projection and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     toOrtho     { get { return active && mToOrtho; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Switch() (Public Function)
        // Desc: Performs a projection switch for the specified camera. This works even
        //       if a switch is already in progress.
        // Parm: targetCamera - The camera whose projection mode is affected.
        //       fov          - Camera field of view. This is the field of view the camera
        //                      is using to display the scene when in perspective mode.
        //       zoom         - The distance in front of the camera where the zoom point
        //                      resides.
        //       duration     - The amount of time in seconds it takes to complete the
        //                      switch.
        //-----------------------------------------------------------------------------
        public void Switch(Camera targetCamera, float fov, float zoom, float duration)
        {
            // Validate args
            if (targetCamera == null)
                return;

            // Store data
            mTargetCamera   = targetCamera;
            mFOV            = fov;
            mZoom           = zoom;
            mDuration       = duration;

            // Regardless of what we're switching to (i.e. perspective or ortho), the effect
            // is achieved using the camera field of view. So, we have 2 possible scenarios:
            //  1. Ortho to Perspective:
            //      Start FOV  = Ortho FOV
            //      Target FOV = Camera FOV
            //  2. Perspective to Ortho:
            //      Start FOV  = Camera FOV
            //      Target FOV = Ortho FOV
            // The Ortho FOV is simply the FOV of the ortho frustum treated as a perspective frustum
            // where the camera rays shoot out from the camera position.
            // We also want to be able to do a switch event if we are already in the process of switching.
            if (active)
            {
                // We're already switching. Start from where we're at.
                if (mToOrtho)
                {
                    // We were switching to ortho, but now we want perspective
                    mStartFOV   = mTargetCamera.fieldOfView;    // Start from where we're at...
                    mTargetFOV  = mFOV;                         // ... and stop at the desired field of view
                    mToOrtho    = false;
                }
                else
                {
                    // We were switching to perspective, but now we want ortho
                    mStartFOV   = mTargetCamera.fieldOfView;            // Start from where we're at...
                    mTargetFOV  = mTargetCamera.CalculateOrthoFOV();    // ... and stop when the field of view reaches the ortho FOV
                    mToOrtho    = true;
                }

                // Stop the current coroutine
                RTG.get.StopCoroutine(mCrtn);
                mCrtn = null;
                mTargetCamera.transform.position = mOriginalPos; // Restore camera position to what it was before the switch started. We will start anew.
            }
            else
            {
                // We're not currently engaged in a switch. What kind of camera are we?
                if (mTargetCamera.orthographic)
                {
                    mStartFOV  = mTargetCamera.CalculateOrthoFOV();
                    mTargetFOV = mTargetCamera.fieldOfView;
                    mToOrtho   = false;
                }
                else
                {
                    mStartFOV   = mTargetCamera.fieldOfView;
                    mTargetFOV  = mTargetCamera.CalculateOrthoFOV();
                    mToOrtho    = true;
                }
            }

            // Start the coroutine
            mProgress = 0.0f;
            mCrtn = RTG.get.StartCoroutine(Coroutine_Switch());
        }
        #endregion

        #region Coroutines
        //-----------------------------------------------------------------------------
        // Name: Coroutine_Switch() (Private Coroutine)
        // Desc: Coroutine which implements the projection switch effect.
        //-----------------------------------------------------------------------------
        IEnumerator Coroutine_Switch()
        {
            // Compute needed data
            float zoomHeight    = mTargetCamera.orthographicSize * 2.0f;    // The height of the camera frustum at the zoom point
            Vector3 zoomPt      = mTargetCamera.transform.position + mTargetCamera.transform.forward * mZoom;

            // Setup camera
            mTargetCamera.orthographic = false;                 // We require a perspective camera to achieve the effect
            mTargetCamera.fieldOfView  = mStartFOV;             // Start from this FOV and move towards the target FOV
            mOriginalPos = mTargetCamera.transform.position;    // Camera position restore. We will be changing the camera position during the switch
                                                                // and this is needed to restore the camera position at the end.

            // Keep going until we're done
            while (true)
            {
                // Update the camera field of view
                mTargetCamera.fieldOfView = Mathf.Lerp(mStartFOV, mTargetFOV, mProgress);

                // We want to maintain a constant frustum size at the zoom point, so we need to position
                // the camera in front of the zoom point where the frustum height is the same as it was
                // before the switch operation started.
                mTargetCamera.transform.position = zoomPt - mTargetCamera.transform.forward * mTargetCamera.FrustumHeightToZ(zoomHeight);

                // Update progress and check if we are done
                mProgress += Time.smoothDeltaTime / mDuration;
                if (mProgress >= 1.0f)
                {
                    // Snap to final values
                    mProgress                           = 1.0f;
                    mTargetCamera.fieldOfView           = mFOV;
                    mTargetCamera.orthographic          = mToOrtho;
                    mTargetCamera.transform.position    = mOriginalPos;
                    break;
                }

                // Wait for next turn
                yield return null;
            }

            // Reset values
            mCrtn   = null;
        }
        #endregion
    }
    #endregion
}
