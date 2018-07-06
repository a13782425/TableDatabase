using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class SearchPanel : EditorWindow
{
    public string SelectText = "";

    public Action<string> CallBack = null;

    private void OnGUI()
    {
        if (SearchTableData.ShowList.Count > 0)
        {
            GUILayout.BeginVertical();
            for (int i = 0; i < SearchTableData.ShowList.Count; i++)
            {
                if (GUILayout.Button(SearchTableData.ShowList[i], EditorGUIStyle.MiddleButtonStyle, GUILayout.Height(20)))
                {
                    if (CallBack != null)
                    {
                        CallBack.Invoke(SearchTableData.ShowList[i]);
                    }
                }
                GUILayout.Space(5);
            }
            GUILayout.EndVertical();
        }

    }
}


public class SearchTableData
{
    private static string[] KeywordArray = new string[] { "default", "findprimary", "a", "b", "c" };

    private static SearchPanel _searchPanel = null;
    private static bool _isShowPanel = false;
    public static List<string> ShowList = new List<string>();
    public static List<T> Search<T>(string fieldName, string param, List<T> dataList)
    {
        List<T> tempList = new List<T>();
        for (int i = 0; i < dataList.Count; i++)
        {
            T data = dataList[i];
            string temp = data.GetType().GetField(fieldName).GetValue(data).ToString();
            if (temp.Contains(param))
            {
                tempList.Add(data);
            }
        }
        return tempList.ToList();
    }

    public static void Show(string searchValue, Action<string> setValueCallBack)
    {
        if (_searchPanel == null)
        {
            _searchPanel = ScriptableObject.CreateInstance<SearchPanel>();
            _searchPanel.position = new Rect(GUIUtility.GUIToScreenPoint(new Vector2(40, 40)), new Vector2(150, 125));
        }
        string str = searchValue;
        int index = str.IndexOf(":");
        ShowList.Clear();
        if (index < 0)
        {
            for (int i = 0; i < KeywordArray.Length; i++)
            {
                if (KeywordArray[i].StartsWith(searchValue.ToLower()))
                {
                    ShowList.Add(KeywordArray[i]);
                }
            }
        }

        if (ShowList.Count > 0)
        {
            if (!_isShowPanel)
            {
                _isShowPanel = true;
                _searchPanel.ShowPopup();
                _searchPanel.CallBack = setValueCallBack;
            }
        }
    }

    public static void Hide()
    {
        ShowList.Clear();
        if (_searchPanel != null && _isShowPanel)
        {
            _isShowPanel = false;
            _searchPanel.Close();
        }
    }

    public static List<T> AdvancedSearch<T>(EditorWindow window, string fieldName, string param, List<T> dataList)
    {
        string keyWord = "";
        bool isHaveKey = false;
        List<string> values = new List<string>();
        string temp = null;
        for (int i = 0; i < param.Length; i++)
        {
            if (param[i] != ':' && !isHaveKey)
            {
                keyWord += param[i];
            }
            else if (param[i] == ':')
            {
                isHaveKey = true;
            }
            else if (param[i] == ' ')
            {
                if (!isHaveKey)
                {
                    window.ShowNotification(new GUIContent("高级搜索有误!!!"));
                    return dataList.ToList();
                }
                else
                {
                    if (keyWord == "default")
                    {
                        temp += param[i];
                    }
                    else
                    {
                        values.Add(temp);
                    }
                }
            }
            else
            {
                temp += param[i];
            }
        }

        switch (keyWord)
        {
            case "default":
                return Search<T>(fieldName, temp, dataList);
            default:
                break;
        }
        return new List<T>();
    }
}

