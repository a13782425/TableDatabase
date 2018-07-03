using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SplitEditor : EditorWindow
{
    GUIStyle styleLeftView;
    GUIStyle styleRightView;
    float splitterPos;
    Rect splitterRect;
    bool dragging;
    float splitterWidth = 2;

    // Add menu named "My Window" to the Window menu
    [MenuItem("GUI/GUISplitter")]
    static void Init()
    {
        SplitEditor window = (SplitEditor)EditorWindow.GetWindow(
            typeof(SplitEditor));
        window.position = new Rect(200, 200, 200, 200);
        window.splitterPos = 100;
    }

    void OnGUI()
    {
        if (styleLeftView == null)
            styleLeftView = new GUIStyle(GUI.skin.box);
        if (styleRightView == null)
            styleRightView = new GUIStyle(GUI.skin.button);

        GUILayout.BeginHorizontal();

        // Left view
        GUILayout.Box("Left View",
                styleLeftView, GUILayout.Width(splitterPos), GUILayout.ExpandHeight(true));
        //GUILayout.ExpandWidth(true),
        //GUILayout.ExpandHeight(true));

        // Splitter
        GUILayout.Box("", "dragtab",
            GUILayout.Width(splitterWidth),
            GUILayout.ExpandHeight(true));
        splitterRect = GUILayoutUtility.GetLastRect();
        EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.SplitResizeLeftRight);

        // Right view
        GUILayout.Box("Right View",
        styleRightView,
       GUILayout.Width(this.maxSize.x - splitterPos - splitterWidth),
        GUILayout.ExpandHeight(true));

        GUILayout.EndHorizontal();

        // Splitter events
        if (Event.current != null)
        {
            switch (Event.current.rawType)
            {
                case EventType.MouseDown:
                    if (splitterRect.Contains(Event.current.mousePosition))
                    {
                        dragging = true;
                    }
                    break;
                case EventType.MouseDrag:
                    if (dragging)
                    {
                        splitterPos += Event.current.delta.x;
                        Repaint();
                    }
                    break;
                case EventType.MouseUp:
                    if (dragging)
                    {
                        this.ShowNotification(new GUIContent("This is a Notification"));
                        dragging = false;
                    }
                    break;
            }
        }
    }
}
