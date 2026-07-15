#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: UndoEx (Public Static Class)
    // Desc: Contains utility functions for working with the Undo/Redo functionality.
    //-----------------------------------------------------------------------------
    public static class UndoEx
    {
        #region Private Static Fields
        static ArrayStack<bool>     sEnabledStack       = new();    // Allows nesting of enabled state changes
        static ArrayStack<string>   sActionNameStack    = new();    // Allows nesting of action names
        #endregion

        #region Private Static Properties
        //-----------------------------------------------------------------------------
        // Name: enabled (Private Static Property)
        // Desc: Returns whether or not the Undo/Redo functionality is enabled.
        //-----------------------------------------------------------------------------
        static bool     enabled     { get { return sEnabledStack.count == 0 || sEnabledStack.Peek(); } }

        //-----------------------------------------------------------------------------
        // Name: actionName (Private Static Property)
        // Desc: Returns the current action name. This is the last action name that was
        //       pushed via the last call to 'PushActionName'. If the action name stack
        //       is empty, the property returns a default action name.
        //-----------------------------------------------------------------------------
        static string   actionName  { get { return sActionNameStack.count == 0 ? RTG.shortPluginName : sActionNameStack.Peek(); } }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: PushEnabled() (Public Static Function)
        // Desc: Changes the enabled state of the Undo/Redo functionality to the specified
        //       value. Every call to 'PushEnabled' should be matched by a call to 'PopEnabled'.
        // Parm: enabled - The new enabled state of the Undo/Redo functionality.
        //-----------------------------------------------------------------------------
        public static void PushEnabled(bool enabled)
        {
            sEnabledStack.Push(enabled);
        }

        //-----------------------------------------------------------------------------
        // Name: PopEnabled() (Public Static Function)
        // Desc: Restores the enabled state to what it was before the last call to 
        //       'PushEnabled'. Should be matched with a previous call to 'PushEnabled'.
        //-----------------------------------------------------------------------------
        public static void PopEnabled()
        {
            // No-op?
            if (sEnabledStack.count == 0) return;

            // Pop
            sEnabledStack.Pop();
        }

        //-----------------------------------------------------------------------------
        // Name: PushActionName() (Public Static Function)
        // Desc: Changes the current action name to the specified value. Every call to
        //       'PushActionName' should be matched by a call to 'PopActionName'.
        // Parm: actionName - The action name.
        //-----------------------------------------------------------------------------
        public static void PushActionName(string actionName)
        {
            sActionNameStack.Push(actionName);
        }

        //-----------------------------------------------------------------------------
        // Name: PopActionName() (Public Static Function)
        // Desc: Restores the action name to what it was before the last call to 
        //       'PushActionName'. Should be matched with a previous call to 'PushActionName'.
        //-----------------------------------------------------------------------------
        public static void PopActionName()
        {
            // No-op?
            if (sActionNameStack.count == 0) return;

            // Pop
            sActionNameStack.Pop();
        }

        //-----------------------------------------------------------------------------
        // Name: Record() (Public Static Function)
        // Desc: Records the specified object's state. The function has no effect if
        //       Undo/Redo is disabled.
        // Parm: targetObject - The object to be recorded.
        //-----------------------------------------------------------------------------
        public static void Record(UnityEngine.Object targetObject)
        {
            if (!enabled) return;
            Undo.RecordObject(targetObject, actionName);
        }

        //-----------------------------------------------------------------------------
        // Name: AddComponent() (Public Static Function)
        // Desc: Adds a component of type T to the specified game object. If Undo/Redo
        //       is disabled, the component is added without undo/redo support.
        // Parm: T          - The type of component to add. Must derive from 'Component'.
        //       gameObject - The game object that receives the new component.
        // Rtrn: The added component.
        //-----------------------------------------------------------------------------
        public static T AddComponent<T>(GameObject gameObject) where T : Component
        {
            if (enabled) return Undo.AddComponent<T>(gameObject);
            else return gameObject.AddComponent<T>();
        }

        //-----------------------------------------------------------------------------
        // Name: CreateScriptableObject() (Public Static Function)
        // Desc: Creates a scriptable objects of type T. If Undo/Redo is disabled, the
        //       scriptable object is created without undo/redo support. 
        // Parm: T          - The type of scriptable object to create. Must derive from
        //                    'ScriptableObject'.
        //       gameObject - The game object that receives the new component.
        // Rtrn: The created scriptable objects.
        //-----------------------------------------------------------------------------
        public static T CreateScriptableObject<T>() where T : ScriptableObject
        {
            // Is Undo/Redo enabled?
            if (enabled)
            {
                // Create SO and record for Undo/Redo
                var scriptableObject = ScriptableObject.CreateInstance<T>();
                Undo.RegisterCreatedObjectUndo(scriptableObject, actionName);

                // Return SO
                return scriptableObject;
            }
            else return ScriptableObject.CreateInstance<T>();   // Just create and return SO
        }

        //-----------------------------------------------------------------------------
        // Name: RecordGameObjects() (Public Static Function)
        // Desc: Records the the state of the game objects stored in 'gameObjects. The
        //       function has no effect if Undo/Redo is disabled.
        // Parm: gameObjects - The collection of objects to be recorded.
        //-----------------------------------------------------------------------------
        public static void RecordGameObjects(IEnumerable<GameObject> gameObjects)
        {
            // No-op?
            if (!enabled) return;

            // Loop through each game object and record it
            foreach (var gameObject in gameObjects)
                Undo.RecordObject(gameObject, actionName);
        }

        //-----------------------------------------------------------------------------
        // Name: SetParentTransform() (Public Static Function)
        // Desc: Changes the parent of 'transform'. If Undo/Redo is disabled, the parent
        //       change happens without undo/redo support.
        // Parm: transform          - The transform whose parent is changed.
        //       newParentTransfor  - The new parent transform.
        //-----------------------------------------------------------------------------
        public static void SetParentTransform(Transform transform, Transform newParentTransform)
        {
            if (enabled) Undo.SetTransformParent(transform, newParentTransform, actionName);
            else transform.parent = newParentTransform;
        }

        //-----------------------------------------------------------------------------
        // Name: SetParentTransform() (Public Static Function)
        // Desc: Changes the parent transform of all objects stored in 'gameObjects'.
        //       If Undo/Redo is disabled, the parent change happens without undo/redo
        //       support.
        // Parm: gameObjects        - The game objects whose parent transforms are changed.
        //       newParentTransform - The new parent transform.
        //-----------------------------------------------------------------------------
        public static void SetParentTransform(IEnumerable<GameObject> gameObjects, Transform newParentTransform)
        {
            // Is Undo/Redo enabled?
            if (enabled)
            {
                // Change transform with Undo/Redo support
                foreach (var gameObject in gameObjects)
                    Undo.SetTransformParent(gameObject.transform, newParentTransform, actionName);
            }
            else
            {
                // Change transform without Undo/Redo support
                foreach (var gameObject in gameObjects)
                    gameObject.transform.parent = newParentTransform;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: RegisterCompleteObjectUndo() (Public Static Function)
        // Desc: https://docs.unity3d.com/ScriptReference/Undo.RegisterCompleteObjectUndo.html
        //       The function has no effect if Undo/Redo is disabled.
        // Parm: targetObject - The object to be recorded.
        //-----------------------------------------------------------------------------
        public static void RegisterCompleteObjectUndo(UnityEngine.Object targetObject)
        {
            if (!enabled) return;
            Undo.RegisterCompleteObjectUndo(targetObject, actionName);
        }

        //-----------------------------------------------------------------------------
        // Name: RegisterChildrenOrderUndo() (Public Static Function)
        // Desc: https://docs.unity3d.com/ScriptReference/Undo.RegisterChildrenOrderUndo.html
        //       The function has no effect if Undo/Redo is disabled.
        // Parm: targetObject - The object to be recorded.
        //-----------------------------------------------------------------------------
        public static void RegisterChildrenOrderUndo(UnityEngine.Object targetObject)
        {
            if (!enabled) return;
            Undo.RegisterChildrenOrderUndo(targetObject, actionName);
        }

        //-----------------------------------------------------------------------------
        // Name: RecordGameObjectTransforms() (Public Static Function)
        // Desc: Records the state of the transforms associated with the game objects
        //       stored in 'gameObjects'. The function has no effect if Undo/Redo is disabled.
        // Parm: gameObjects - The list of objects whose transforms will be recorded.
        //-----------------------------------------------------------------------------
        public static void RecordGameObjectTransforms(IEnumerable<GameObject> gameObjects)
        {
            if (!enabled) return;
            foreach (var gameObject in gameObjects)
                Undo.RecordObject(gameObject.transform, actionName);
        }

        //-----------------------------------------------------------------------------
        // Name: RecordTransforms() (Public Static Function)
        // Desc: Records the state of the transforms stores in 'transforms'. The function
        //       has no effect if Undo/Redo is disabled.
        // Parm: transforms - The list of transforms to be recorded.
        //-----------------------------------------------------------------------------
        public static void RecordTransforms(IEnumerable<Transform> transforms)
        {
            if (!enabled) return;
            foreach (var transform in transforms)
                Undo.RecordObject(transform, actionName);
        }

        //-----------------------------------------------------------------------------
        // Name: RegisterCreatedObject() (Public Static Function) 
        // Desc: Registers the creation of a new object of type T. The function has no
        //       effect if Undo/Redo is disabled.
        // Parm: T             - The object type. Must derive from 'UnityEngine.Object'.
        //       createdObject - The created object.
        //-----------------------------------------------------------------------------
        public static void RegisterCreatedObject<T>(T createdObject) where T : UnityEngine.Object
        {
            if (!enabled) return;
            Undo.RegisterCreatedObjectUndo(createdObject, actionName);
        }

        //-----------------------------------------------------------------------------
        // Name: DestroyObjectImmediate() (Public Static Function) 
        // Desc: Destroys the specified game object immediately. If Undo/Redo is disabled,
        //       the objects is destroyed without undo/redo support.
        // Parm: targetObject - The object to be destroyed.
        //-----------------------------------------------------------------------------
        public static void DestroyObjectImmediate(UnityEngine.Object targetObject)
        {
            if (enabled) Undo.DestroyObjectImmediate(targetObject);
            else UnityEngine.Object.DestroyImmediate(targetObject, true);
        }

        //-----------------------------------------------------------------------------
        // Name: DestroyGameObjectsImmediate() (Public Static Function) 
        // Desc: Destroys the specified game objects immediately. If Undo/Redo is disabled,
        //       the game objects are destroyed without undo/redo support.
        // Parm: gameObjects - The list of objects to be destroyed. Can contain null refs
        //                     and can also contain a mix of parents and children.
        //-----------------------------------------------------------------------------
        public static void DestroyGameObjectsImmediate(List<GameObject> gameObjects)
        {
            // Undo/Redo enabled?
            if (enabled)
            {
                // Loop through each game object
                foreach (var gameObj in gameObjects)
                {
                    // Destroy object if valid (it might have been destroyed indirectly
                    // if its parent was destroyed on an earlier run)
                    if (gameObj != null)
                        Undo.DestroyObjectImmediate(gameObj);
                }
            }
            else
            {
                // Loop through each game object
                foreach (var gameObj in gameObjects)
                {
                    // Destroy object if valid (it might have been destroyed indirectly
                    // if its parent was destroyed on an earlier run)
                    if (gameObj != null)
                        GameObject.DestroyImmediate(gameObj);
                }
            }
        }
        #endregion
    }
    #endregion
}
#endif