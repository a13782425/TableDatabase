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

    private static GUIStyle _minusTableButtonStyle = null;
    public static GUIStyle TableMinusButtonStyle
    {
        get
        {
            if (_minusTableButtonStyle == null)
            {
                _minusTableButtonStyle = new GUIStyle("OL Minus");
                _minusTableButtonStyle.alignment = TextAnchor.MiddleCenter;
                _minusTableButtonStyle.margin = new RectOffset(15, 0, 2, 0);
                _minusTableButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            }
            return _minusTableButtonStyle;
        }
    }
    private static GUIStyle _getTableNumberStyle = null;
    public static GUIStyle TableNumberStyle
    {
        get
        {
            if (_getTableNumberStyle == null)
            {
                _getTableNumberStyle = new GUIStyle(GUI.skin.label);
                _getTableNumberStyle.alignment = TextAnchor.MiddleCenter;
                _getTableNumberStyle.margin = new RectOffset(0, 0, 0, 2);
                _getTableNumberStyle.padding = new RectOffset(0, 0, 0, 0);
            }
            return _getTableNumberStyle;
        }
    }
    private static GUIStyle _middleButtonStyle = null;
    public static GUIStyle MiddleButtonStyle
    {
        get
        {
            if (_middleButtonStyle == null)
            {
                _middleButtonStyle = new GUIStyle("OL Title");
                _middleButtonStyle.alignment = TextAnchor.MiddleCenter;
                _middleButtonStyle.fixedHeight = 0;
                _middleButtonStyle.margin = new RectOffset(0, 0, 0, 0);
                _middleButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            }
            return _middleButtonStyle;
        }
    }

    private static GUIStyle _titleButtonStyle = null;
    public static GUIStyle TitleButtonStyle
    {
        get
        {
            if (_titleButtonStyle == null)
            {
                _titleButtonStyle = new GUIStyle("OL Title");
                _titleButtonStyle.alignment = TextAnchor.MiddleCenter;
                _titleButtonStyle.fixedHeight = 0;
                _titleButtonStyle.margin = new RectOffset(0, 0, 0, 0);
                _titleButtonStyle.padding = new RectOffset(0, 0, 0, 0);
                _titleButtonStyle.normal.background = null;
                Texture2D texture2D = new Texture2D(64, 64);
                for (int i = 0; i < texture2D.width; i++)
                {
                    for (int j = 0; j < texture2D.height; j++)
                    {
                        texture2D.SetPixel(i, j, new Color(0.77f, 0.77f, 0.77f));
                    }
                }
                _titleButtonStyle.active.background = texture2D;
            }
            return _titleButtonStyle;
        }
    }



    private static GUIStyle _searchPanelStyle = null;
    public static GUIStyle SearchPanelStyle
    {
        get
        {
            if (_searchPanelStyle == null)
            {
                _searchPanelStyle = new GUIStyle(GUI.skin.box);
                _searchPanelStyle.margin = new RectOffset(0, 0, 0, 0);
                _searchPanelStyle.padding = new RectOffset(5, 5, 5, 5);
            }
            return _searchPanelStyle;
        }
    }

    private static GUIStyle _listBoxStyle = null;

    public static GUIStyle ListBoxStyle
    {
        get
        {
            if (_listBoxStyle == null)
            {
                _listBoxStyle = new GUIStyle(GUI.skin.label);
                _listBoxStyle.margin = new RectOffset(0, 0, 0, 0);

                _listBoxStyle.padding = new RectOffset(0, 0, 0, 0);
            }
            return _listBoxStyle;
        }
    }

    private static GUIStyle _groupBoxStyle = null;
    public static GUIStyle GroupBoxStyle
    {
        get
        {
            if (_groupBoxStyle == null)
            {
                _groupBoxStyle = new GUIStyle(GUI.skin.box);
                _groupBoxStyle.margin = new RectOffset(0, 0, 0, 0);
                _groupBoxStyle.padding = new RectOffset(0, 0, 0, 0);
                _groupBoxStyle.fixedHeight = 0;
                _groupBoxStyle.stretchHeight = true;
            }
            return _groupBoxStyle;
        }
    }
}
