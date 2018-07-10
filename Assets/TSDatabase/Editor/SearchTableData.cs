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
    private static string[] KeywordArray = new string[] { "default", "checkprimary", "range", "multiple", "ignore", "ignoremultiple" };

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
                _searchPanel.position = new Rect(GUIUtility.GUIToScreenPoint(new Vector2(40, 40)), new Vector2(150, 25 * ShowList.Count));
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

    public static List<T> AdvancedSearch<T>(EditorWindow window, TableConfig tableConfig, string fieldName, string param, List<T> dataList)
    {
        window.ShowNotification(new GUIContent("正在搜索..."));
        string keyWord = "";
        bool isHaveKey = false;
        List<T> tempList = new List<T>();
        List<string> values = new List<string>();
        string temp = null;
        for (int i = 0; i < param.Length; i++)
        {
            if (param[i] == ':' && !isHaveKey)
            {
                keyWord = temp;
                temp = "";
                isHaveKey = true;
            }
            else if (param[i] == ' ')
            {
                if (!isHaveKey)
                {
                    window.ShowNotification(new GUIContent("高级搜索有误!!!"));
                    goto End;
                }
                else
                {
                    values.Add(temp);
                    temp = "";
                }
            }
            else
            {
                temp += param[i];
            }
        }
        values.Add(temp);
        if (!isHaveKey)
        {
            window.ShowNotification(new GUIContent("关键字不能解析..."));
            goto End;
        }

        switch (keyWord)
        {
            case "default":
                if (values.Count < 1)
                {
                    window.ShowNotification(new GUIContent("高级搜索有误!!!"));
                    goto End;
                }
                tempList = DefaultSearch<T>(window, fieldName, values[0], dataList);
                break;
            case "checkprimary":
                tempList = CheckPrimarySearch<T>(window, tableConfig, dataList);
                break;
            case "range":
                if (values.Count != 2)
                {
                    window.ShowNotification(new GUIContent("范围搜索参数有误!!!"));
                    goto End;
                }
                for (int i = 0; i < tableConfig.FieldList.Count; i++)
                {
                    if (fieldName == tableConfig.FieldList[i].FieldName)
                    {
                        if (tableConfig.FieldList[i].FieldType != "int" && tableConfig.FieldList[i].FieldType != "float")
                        {
                            window.ShowNotification(new GUIContent("范围搜索类型有误!!!"));
                            goto End;
                        }
                        break;
                    }
                }
                float rangeMinNum = 0;
                float rangeMaxNum = 0;
                if (!float.TryParse(values[0], out rangeMinNum))
                {
                    window.ShowNotification(new GUIContent("范围搜索参数有误!!!"));
                    goto End;
                }
                if (!float.TryParse(values[1], out rangeMaxNum))
                {
                    window.ShowNotification(new GUIContent("范围搜索参数有误!!!"));
                    goto End;
                }
                tempList = RangeSearch<T>(window, fieldName, rangeMinNum, rangeMaxNum, dataList);
                break;
            case "multiple":
                tempList = MultipleSearch<T>(window, fieldName, values, dataList);
                break;
            case "ignore":
                if (values.Count < 1)
                {
                    window.ShowNotification(new GUIContent("高级搜索有误!!!"));
                    goto End;
                }
                tempList = IgnoreSearch<T>(window, fieldName, values[0], dataList);
                break;
            case "ignoremultiple":
                tempList = IgnoreMultipleSearch<T>(window, fieldName, values, dataList);
                break;
            default:
                goto End;
        }
        window.RemoveNotification();
        End: return tempList;
    }

    private static List<T> CheckPrimarySearch<T>(EditorWindow window, TableConfig tableConfig, List<T> dataList)
    {
        List<T> tempList = new List<T>();
        PrimaryKeyInfo primaryKeyInfo = TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[tableConfig.TableName];
        for (int i = 0; i < dataList.Count; i++)
        {
            window.ShowNotification(new GUIContent("正在搜索..."));
            T data = dataList[i];
            string temp = data.GetType().GetField(tableConfig.PrimaryKey).GetValue(data).ToString();
            if (primaryKeyInfo.Values.ContainsKey(temp))
            {
                if (primaryKeyInfo.Values[temp] != 1)
                {
                    tempList.Add(data);
                }
            }
            else
            {
                tempList.Add(data);
            }
        }
        return tempList.ToList();
    }

    private static List<T> RangeSearch<T>(EditorWindow window, string fieldName, float rangeMinNum, float rangeMaxNum, List<T> dataList)
    {
        List<T> tempList = new List<T>();
        for (int i = 0; i < dataList.Count; i++)
        {
            window.ShowNotification(new GUIContent("正在搜索..."));
            T data = dataList[i];
            float temp = Convert.ToSingle(data.GetType().GetField(fieldName).GetValue(data));
            if (temp >= rangeMinNum && temp <= rangeMaxNum)
            {
                tempList.Add(data);
            }
        }
        return tempList.ToList();
    }

    private static List<T> DefaultSearch<T>(EditorWindow window, string fieldName, string param, List<T> dataList)
    {
        List<T> tempList = new List<T>();
        for (int i = 0; i < dataList.Count; i++)
        {
            window.ShowNotification(new GUIContent("正在搜索..."));
            T data = dataList[i];
            string temp = data.GetType().GetField(fieldName).GetValue(data).ToString();
            if (temp.Contains(param))
            {
                tempList.Add(data);
            }
        }
        return tempList.ToList();
    }

    private static List<T> IgnoreSearch<T>(EditorWindow window, string fieldName, string param, List<T> dataList)
    {
        param = param.ToLower();
        List<T> tempList = new List<T>();
        for (int i = 0; i < dataList.Count; i++)
        {
            window.ShowNotification(new GUIContent("正在搜索..."));
            T data = dataList[i];
            string temp = data.GetType().GetField(fieldName).GetValue(data).ToString().ToLower();
            if (temp.Contains(param))
            {
                tempList.Add(data);
            }
        }
        return tempList.ToList();
    }

    private static List<T> IgnoreMultipleSearch<T>(EditorWindow window, string fieldName, List<string> values, List<T> dataList)
    {
        for (int i = 0; i < values.Count; i++)
        {
            values[i] = values[i].ToLower();
        }
        List<T> tempList = new List<T>();
        for (int i = 0; i < dataList.Count; i++)
        {
            window.ShowNotification(new GUIContent("正在搜索..."));
            T data = dataList[i];
            string temp = data.GetType().GetField(fieldName).GetValue(data).ToString().ToLower();
            for (int j = 0; j < values.Count; j++)
            {
                if (temp.Contains(values[j]))
                {
                    tempList.Add(data);
                    break;
                }
            }
        }
        return tempList.ToList();
    }

    private static List<T> MultipleSearch<T>(EditorWindow window, string fieldName, List<string> values, List<T> dataList)
    {
        List<T> tempList = new List<T>();
        for (int i = 0; i < dataList.Count; i++)
        {
            window.ShowNotification(new GUIContent("正在搜索..."));
            T data = dataList[i];
            string temp = data.GetType().GetField(fieldName).GetValue(data).ToString();
            for (int j = 0; j < values.Count; j++)
            {
                if (temp.Contains(values[j]))
                {
                    tempList.Add(data);
                    break;
                }
            }
        }
        return tempList.ToList();
    }
}

