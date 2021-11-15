/*
 *	Created by:  Peter @sHTiF Stefcek
 */

using UnityEngine;

namespace Dash.Editor
{
    public class ShortcutsHandler
    {
        public static void Handle()
        {
            if (!Event.current.control || Event.current.type != EventType.KeyDown)
                return;
            
            switch (Event.current.keyCode)
            {
                case KeyCode.D:
                    SelectionManager.DuplicateSelectedNodes(DashEditorCore.Graph);
                    break;
                case KeyCode.X:
                    SelectionManager.DeleteSelectedNodes(DashEditorCore.Graph);
                    break;
            }
        }
    }
}