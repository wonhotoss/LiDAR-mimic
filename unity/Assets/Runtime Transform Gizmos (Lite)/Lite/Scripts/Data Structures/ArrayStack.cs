using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ArrayStack (Public Class)
    // Desc: Implements a stack in which elements are stored in an array.
    // Parm: T - The type of data stored in the stack.
    // Note: References are released on 'Pop()' and 'Clear()' when required by T.
    //       Intended for internal engine state stacks where correct push / pop
    //       pairing is enforced by design and misuse is treated as a logic error.
    //-----------------------------------------------------------------------------
    public class ArrayStack<T>
    {
        #region Private Fields
        T[]     mStack;         // The stack array
        int     mCount;         // Number of elements in the stack
        bool    mClearRefs;     // True if T may contain references and requires clearing for GC
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: count (Public Property)
        // Desc: Returns the number of items in the stack.
        //-----------------------------------------------------------------------------
        public int count    { get { return mCount; } }

        //-----------------------------------------------------------------------------
        // Name: top (Public Property)
        // Desc: Returns the index of the last item in the stack. If the stack is empty
        //       this will return -1.
        //-----------------------------------------------------------------------------
        public int top      { get { return mCount - 1; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: ArrayStack() (Public Constructor)
        // Desc: Default constructor.
        //-----------------------------------------------------------------------------
        public ArrayStack()
        {
            mStack      = new T[1];
            mClearRefs  = System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<T>();
        }

        //-----------------------------------------------------------------------------
        // Name: ArrayStack() (Public Constructor)
        // Desc: Creates an array stack with the specified initial capacity.
        // Parm: capacity - Initial stack capacity.
        //-----------------------------------------------------------------------------
        public ArrayStack(int capacity)
        {
            mStack      = new T[capacity];
            mClearRefs  = System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<T>();
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Clear() (Public Function)
        // Desc: Clears the stack.
        //-----------------------------------------------------------------------------
        public void Clear()
        {
            // Clear references if necessary
            if (mClearRefs)
            {
                for (int i = 0; i < mCount; ++i)
                    mStack[i] = default;
            }

            // Reset item count
            mCount = 0;
        }

        //-----------------------------------------------------------------------------
        // Name: Push() (Public Function)
        // Desc: Pushes the specified item onto the stack.
        // Parm: item - The item to push onto the stack.
        //-----------------------------------------------------------------------------
        public void Push(T item)
        {
            // Grow the stack if needed and add item
            if (mCount == mStack.Length) Grow();
            mStack[mCount++] = item;
        }

        //-----------------------------------------------------------------------------
        // Name: Pop() (Public Function)
        // Desc: Pops an item from the stack and returns it. The function assumes the
        //       stack has at least one item.
        // Rtrn: The item that was popped off the stack.
        //-----------------------------------------------------------------------------
        public T Pop()
        {
            // Pop the item and clear ref if necessary
            T item = mStack[--mCount];
            if (mClearRefs)
                mStack[mCount] = default;

            // Return item
            return item;
        }

        //-----------------------------------------------------------------------------
        // Name: Peek() (Public Function)
        // Desc: Returns the last item on the stack without popping it. The function assumes
        //       the stack has at least one item.
        // Rtrn: the last item on the stack.
        //-----------------------------------------------------------------------------
        public T Peek()
        {
            return mStack[mCount - 1];
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: Grow() (Private Function)
        // Desc: Grows the stack's capacity.
        //-----------------------------------------------------------------------------
        void Grow()
        {
            Array.Resize(ref mStack, mStack.Length * 2);
        }
        #endregion
    }
	#endregion
}
