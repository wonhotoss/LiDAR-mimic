using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: BinaryAnswer (Public Class)
    // Desc: Stores a yes or no answer. Useful with event handlers that ask for permission
    //       to do something.
    //-----------------------------------------------------------------------------
    public class BinaryAnswer
    {
        #region Private Fields
        int mYesCount = 0;  // The number of times 'Yes' has been answered
        int mNoCount  = 0;  // The number of times 'No' has been answered
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: yesCount (Public Property)
        // Desc: Returns the number of 'Yes' answers.
        //-----------------------------------------------------------------------------
        public int  yesCount    { get { return mYesCount; } }

        //-----------------------------------------------------------------------------
        // Name: noCount (Public Property)
        // Desc: Returns the number of 'No' answers.
        //-----------------------------------------------------------------------------
        public int  noCount     { get { return mNoCount; } }

        //-----------------------------------------------------------------------------
        // Name: onlyYes (Public Property)
        // Desc: Returns true if there are only 'Yes' answers.
        //-----------------------------------------------------------------------------
        public bool onlyYes     { get { return mYesCount != 0 && mNoCount == 0; } }

        //-----------------------------------------------------------------------------
        // Name: onlyNo (Public Property)
        // Desc: Returns true if there are only 'No' answers.
        //-----------------------------------------------------------------------------
        public bool onlyNo      { get { return mYesCount == 0 && mNoCount != 0; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Clear() (Public Function)
        // Desc: Clears the yes and no counters.
        //-----------------------------------------------------------------------------
        public void Clear()
        {
            mYesCount = 0;
            mNoCount  = 0;
        }

        //-----------------------------------------------------------------------------
        // Name: Yes() (Public Function)
        // Desc: Answers 'Yes' to a question.
        //-----------------------------------------------------------------------------
        public void Yes()
        {
            ++mYesCount;
        }

        //-----------------------------------------------------------------------------
        // Name: No() (Public Function)
        // Desc: Answers 'No' to a question.
        //-----------------------------------------------------------------------------
        public void No()
        {
            ++mNoCount;
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: MonoSingleton (Public Abstract Class)
    // Desc: Abstract class which can be derived by MonoBehaviour singleton classes.
    // Parm: T - Singleton type.
    //-----------------------------------------------------------------------------
    public abstract class MonoSingleton<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        #region Private Static Fields
        static T sInstance = null;      // Singleton instance
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: get (Public Static Property)
        // Desc: Returns the singleton instance.
        //-----------------------------------------------------------------------------
        public static T @get
        {
            get
            {
                if (sInstance == null) sInstance = GameObjectEx.FindFirstObjectByType<T>();
                return sInstance;
            }
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: Singleton (Public Abstract Class)
    // Desc: Abstract class which can be derived by singleton classes.
    // Parm: T - Singleton type.
    //-----------------------------------------------------------------------------
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        #region Private Static Fields
        static T sInstance    = new T();    // Singleton instance
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: get (Public Static Property)
        // Desc: Returns the singleton instance.
        //-----------------------------------------------------------------------------
        public  static T @get    { get { return sInstance; } }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: Core (Public Static Class)
    // Desc: Implements core utility properties and functions.
    //-----------------------------------------------------------------------------
    public static class Core
    {
        #region Public Static Readonly Fields
        public static readonly Vector3[]    axes        = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };
        public static readonly string[]     axisNames   = new string[] { "X", "Y", "Z" };                               
        public static readonly string[]     planeNames  = new string[] { "XY", "YZ", "ZX" };                           
        public static readonly Color        xAxisColor  = new Color(0.8588235f, 0.2431373f, 0.1137255f, 0.9294118f);
        public static readonly Color        yAxisColor  = new Color(0.6039216f, 0.9529412f, 0.282353f, 0.9294118f);
        public static readonly Color        zAxisColor  = new Color(0.227451f, 0.4784314f, 0.972549f, 0.9294118f);
        #endregion
    }
    #endregion
}