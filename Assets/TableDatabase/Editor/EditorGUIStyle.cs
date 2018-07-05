//
//   		Copyright 2017 KeyleXiao.
//     		Contact : Keyle_xiao@hotmail.com 
//
//     		Licensed under the Apache License, Version 2.0 (the "License");
//     		you may not use this file except in compliance with the License.
//     		You may obtain a copy of the License at
//
//     		http://www.apache.org/licenses/LICENSE-2.0
//
//     		Unless required by applicable law or agreed to in writing, software
//     		distributed under the License is distributed on an "AS IS" BASIS,
//     		WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     		See the License for the specific language governing permissions and
//     		limitations under the License.
//
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class EditorGUIStyle
{
    private static Texture2D MainBg { get; set; }
    private static Texture2D TagBg { get; set; }



    private static GUIStyle tagButtonStyle;
    private static GUIStyle groupBoxStyle;
    private static GUIStyle horizontalScrollbarStyle;
    private static GUIStyle pageLabelGuiStyle;
    private static GUIStyle tableGroupBoxStyle;
    private static GUIStyle textGuiStyle;
    private static GUIStyle jumpButtonGuiStyle;
    private static GUIStyle ToggleStyle;
    private static Dictionary<string, GUIStyle> _titleButtonStyleDic = new Dictionary<string, GUIStyle>();
    private static GUIStyle MiddleButton;
    private static GUIStyle _titleButton;
    private static GUIStyle _minusButton;
    public static T LoadEditorResource<T>(string file_name_with_extension) where T : UnityEngine.Object
    {
        //            string path = string.Format("{0}/EditorResources/", PathMapping.GetSmartDataViewEditorPath());
        string path = TableDatabaseUtils.EditorPath + "/EditorResources/" + file_name_with_extension;
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
    public static GUIStyle GetMinusButton
    {
        get
        {
            if (_minusButton == null)
            {
                _minusButton = new GUIStyle("OL Minus");
                _minusButton.alignment = TextAnchor.MiddleCenter;
            }
            return _minusButton;
        }
    }
    public static GUIStyle GetMiddleButton
    {
        get
        {
            if (MiddleButton == null)
            {
                MiddleButton = new GUIStyle("OL Title");
                MiddleButton.alignment = TextAnchor.MiddleCenter;
                MiddleButton.fixedHeight = 0;
                MiddleButton.margin = new RectOffset(0, 0, 0, 0);
                MiddleButton.padding = new RectOffset(0, 0, 0, 0);
            }
            return MiddleButton;
        }
    }
    public static GUIStyle GetTitleButton
    {
        get
        {
            if (_titleButton == null)
            {
                _titleButton = new GUIStyle("OL Title");
                _titleButton.alignment = TextAnchor.MiddleCenter;
                _titleButton.fixedHeight = 0;
                _titleButton.margin = new RectOffset(0, 0, 0, 0);
                _titleButton.padding = new RectOffset(0, 0, 0, 0);
                _titleButton.normal.background = null;
                Texture2D texture2D = new Texture2D(64, 64);
                for (int i = 0; i < texture2D.width; i++)
                {
                    for (int j = 0; j < texture2D.height; j++)
                    {
                        texture2D.SetPixel(i, j, new Color(0.77f, 0.77f, 0.77f));
                    }
                }
                _titleButton.active.background = texture2D;
            }
            return _titleButton;
        }
    }


    public static GUIStyle GetTogleStyle()
    {
        if (ToggleStyle == null)
        {
            ToggleStyle = new GUIStyle(GUI.skin.toggle);
            ToggleStyle.margin = new RectOffset(0, 0, 3, 0);
            ToggleStyle.padding = new RectOffset(0, 0, 0, 0);
        }

        return ToggleStyle;
    }


    public static GUIStyle GetJumpButtonGuiStyle()
    {
        if (jumpButtonGuiStyle == null)
        {
            jumpButtonGuiStyle = new GUIStyle(GUI.skin.button);
            jumpButtonGuiStyle.margin = new RectOffset(0, 0, 3, 0);
            jumpButtonGuiStyle.padding = new RectOffset(0, 0, 0, 0);
        }

        return jumpButtonGuiStyle;
    }


    public static GUIStyle GetPageLabelGuiStyle()
    {
        if (pageLabelGuiStyle == null)
        {
            pageLabelGuiStyle = new GUIStyle(GUI.skin.box);
            pageLabelGuiStyle.margin = new RectOffset(0, 0, 0, 0);
            pageLabelGuiStyle.padding = new RectOffset(5, 5, 5, 5);
        }
        return pageLabelGuiStyle;
    }

    public static GUIStyle GetFieldBoxStyle()
    {
        if (groupBoxStyle == null)
        {
            groupBoxStyle = new GUIStyle(GUI.skin.label);
            groupBoxStyle.margin = new RectOffset(0, 0, 0, 0);

            groupBoxStyle.padding = new RectOffset(0, 0, 0, 0);
        }

        return groupBoxStyle;
    }


    public static GUIStyle GetGroupBoxStyle()
    {
        if (groupBoxStyle == null)
        {
            groupBoxStyle = new GUIStyle(GUI.skin.box);
            groupBoxStyle.margin = new RectOffset(0, 0, 0, 0);
            groupBoxStyle.padding = new RectOffset(0, 0, 0, 0);
        }

        return groupBoxStyle;
    }


    public static GUIStyle GetTableGroupBoxStyle()
    {
        if (tableGroupBoxStyle == null)
        {
            tableGroupBoxStyle = new GUIStyle(GUI.skin.box);
            tableGroupBoxStyle.margin = new RectOffset(0, 0, 0, 0);
            tableGroupBoxStyle.padding = new RectOffset(0, 0, 0, 0);
        }

        return tableGroupBoxStyle;
    }


    public static GUIStyle GetHorizontalScrollbarStyle()
    {
        if (horizontalScrollbarStyle == null)
        {
            horizontalScrollbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar);
            horizontalScrollbarStyle.margin = new RectOffset(0, 0, 0, 0);
            horizontalScrollbarStyle.padding = new RectOffset(0, 0, 0, 0);
        }

        return horizontalScrollbarStyle;
    }


    public static GUIStyle GetTextGuiStyle()
    {
        if (textGuiStyle == null)
        {
            textGuiStyle = new GUIStyle(GUI.skin.textField);
            textGuiStyle.margin = new RectOffset(0, 0, 0, 0);
            textGuiStyle.padding = new RectOffset(0, 0, 0, 0);
            textGuiStyle.wordWrap = false;
        }

        return textGuiStyle;
    }

    public static void DrawLine(GUIStyle rLineStyle)
    {
        GUILayout.BeginHorizontal(rLineStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

}
