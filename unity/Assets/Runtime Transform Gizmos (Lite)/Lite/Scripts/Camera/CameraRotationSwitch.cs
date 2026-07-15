using UnityEngine;
using System.Collections;
using System.Threading;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: CameraRotationSwitch (Public Class)
    // Desc: Implements a camera rotation switch. This is useful for implementing
    //       effects such as the one that occurs when the user clicks on the scene
    //       gizmo inside the Unity Editor to align the camera view vector to the
    //       clicked gizmo axis. A rotation switch is simply a transition from one
    //       rotation to another.
    //-----------------------------------------------------------------------------
    public class CameraRotationSwitch
    {
        #region Private Fields
        Camera      mTargetCamera;      // The target camera
        Coroutine   mCrtn;              // The coroutine that implements the switch
        Quaternion  mTargetRotation;    // The target rotation
        float       mZoom;              // Camera zoom. This is the distance in front of the camera where the zoom point resides.
        float       mSmoothSpeed;       // The bigger the value, the faster the switch
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: active (Public Property)
        // Desc: Returns whether or not the switch operation is active (i.e. in progress).
        //-----------------------------------------------------------------------------
        public bool     active      { get { return mCrtn != null; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: InstantSwitch() (Public Function)
        // Desc: Performs a rotation switch for the specified camera. This works even if
        //       a switch is already in progress.
        // Parm: targetCamera   - The camera whose rotation is affected.
        //       targetRotation - The rotation to switch to.
        //       zoom           - The distance in front of the camera where the zoom point
        //                        resides.
        //       smoothSpeed    - The bigger the value, the faster the switch.
        //-----------------------------------------------------------------------------
        public void InstantSwitch(Camera targetCamera, Quaternion targetRotation, float zoom)
        {
            // Stop the coroutine if already active
            if (active)
            {
                RTG.get.StopCoroutine(mCrtn);
                mCrtn = null;
            }

            // Calculate zoom point
            Vector3 zoomPt = targetCamera.transform.position + targetCamera.transform.forward * zoom;

            // Set the camera rotation to the target rotation
            targetCamera.transform.rotation = targetRotation;

            // Orbit around the zoom point
            targetCamera.transform.position = zoomPt - targetCamera.transform.forward * zoom;
        }

        //-----------------------------------------------------------------------------
        // Name: StartSwitch() (Public Function)
        // Desc: Starts a rotation switch transition for the specified camera. This
        //       works even if a switch is already in progress.
        // Parm: targetCamera   - The camera whose rotation is affected.
        //       targetRotation - The rotation to switch to.
        //       zoom           - The distance in front of the camera where the zoom point
        //                        resides.
        //       smoothSpeed    - The bigger the value, the faster the switch.
        //-----------------------------------------------------------------------------
        public void StartSwitch(Camera targetCamera, Quaternion targetRotation, float zoom, float smoothSpeed)
        {
            // Validate args
            if (targetCamera == null)
                return;

            // Store data
            mTargetCamera   = targetCamera;
            mTargetRotation = targetRotation;
            mZoom           = zoom;
            mSmoothSpeed    = smoothSpeed;

            // Stop the coroutine if already active
            if (active)
            {
                RTG.get.StopCoroutine(mCrtn);
                mCrtn = null;
            }

            // Start coroutine
            mCrtn = RTG.get.StartCoroutine(Coroutine_Switch());
        }
        #endregion

        #region Coroutines
        //-----------------------------------------------------------------------------
        // Name: Coroutine_Switch() (Private Coroutine)
        // Desc: Coroutine which implements the rotation switch effect.
        //-----------------------------------------------------------------------------
        IEnumerator Coroutine_Switch()
        {
            // Cache data
            Vector3 zoomPt = mTargetCamera.transform.position + mTargetCamera.transform.forward * mZoom;

            // Keep going until we're done
            while (Quaternion.Angle(mTargetCamera.transform.rotation, mTargetRotation) > 1e-3f)
            {
                // Interpolate rotation and orbit around zoom point
                mTargetCamera.transform.rotation = Quaternion.Lerp(mTargetCamera.transform.rotation, mTargetRotation, Time.smoothDeltaTime * mSmoothSpeed);
                mTargetCamera.transform.position = zoomPt - mTargetCamera.transform.forward * mZoom;

                // Wait for next turn
                yield return null;
            }

            // Snap to final rotation
            mTargetCamera.transform.rotation = mTargetRotation;

            // Reset values
            mCrtn   = null;
        }
        #endregion
    }
    #endregion
}
