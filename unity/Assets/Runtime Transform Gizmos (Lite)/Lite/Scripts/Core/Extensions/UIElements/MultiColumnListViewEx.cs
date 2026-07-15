using UnityEngine;
using UnityEngine.UIElements;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: MultiColumnListViewEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'MultiColumnListView'
    //       class.
    //-----------------------------------------------------------------------------
    public static class MultiColumnListViewEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: GetRowFromCell() (Public Extension)
        // Desc: Given a 'VisualElement' which represents a cell inside the multi-column
        //       list view, the function returns the 'VisualElement' which is the parent
        //       of all cells on the same row as 'cell'.
        // Parm: cell - 'VisualElement' which represents a cell inside the list view.
        // Rtrn: The 'VisualElement' which is the parent of all cells on the same row
        //       as 'cell'.
        //-----------------------------------------------------------------------------
        public static VisualElement GetRowFromCell(this MultiColumnListView list, VisualElement cell)
        {
            return cell.parent.parent;
        }
        #endregion
    }
    #endregion
}
