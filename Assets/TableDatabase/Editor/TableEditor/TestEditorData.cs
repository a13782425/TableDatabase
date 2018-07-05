using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TestDataEditor : EditorWindow
{
    [MenuItem("Table/Tables/Test", priority = 30)]
    static void CreateTable()
    {
        EditorWindow.GetWindow<TestDataEditor>(false, "Test数据").minSize = new Vector2(600, 500);
    }

    private TestConfig _excelConfig;

    private TableConfig _tableConfig;

    private TestSerializeData _serializeData;

    private DivisionSlider _divisionSlider;

    private PrimaryKeyInfo _primaryKeyInfo = null;

    private string[] _searchFieldArray;

    private int _searchFieldIndex = 0;

    private string _searchValue;

    private bool _isAdvancedSearch = false;

    private float _areaWidht = 0;

    private Vector2 _tableScrollView;

    private bool _isMouseInSearch;

    private Event currentEvent = null;

    private int _pageNum = 1;

    private int _pageMaxNum = 1;

    private bool _isSort = false;

    private List<Test> _dataList = null;

    private string[] _showCountArray = new string[] { "10", "30", "50", "80", "100" };

    private int _showCountIndex = 0;

    private List<Rect> rectList;

    void OnGUI()
    {
        if (_serializeData == null)
        {
            return;
        }
        currentEvent = Event.current;

        GUI.SetNextControlName("GUIArea");
        GUILayout.BeginArea(new Rect(5, 5, position.width - 10, position.height - 10), "", "box");
        GUILayout.BeginVertical();
        GUITitleInfo();
        GUISearchInfo();
        GUIShowTableHead();
        GUIShowTableBody();
        GUIFooterInfo();
        GUILayout.EndVertical();
        GUILayout.EndArea();
        if (GUI.enabled)
        {
            switch (currentEvent.type)
            {
                case EventType.Used:
                    _isMouseInSearch = GUI.GetNameOfFocusedControl() == "SearchText";
                    break;
                case EventType.mouseUp:
                    if (0 == GUIUtility.hotControl)//0为面板
                    {
                        if (GUI.GetNameOfFocusedControl() == "SearchText" && _isMouseInSearch)
                        {
                            GUI.FocusControl("GUIArea");
                            _isMouseInSearch = false;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        if (currentEvent.keyCode == KeyCode.Escape || currentEvent.keyCode == KeyCode.Return)
        {
            GUI.FocusControl("GUIArea");
        }
    }
    private void Update()
    {
        //Debug.LogError(EditorWindow.focusedWindow);
    }


    /// <summary>
    /// 显示Title
    /// </summary>
    private void GUITitleInfo()
    {
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal("OL Title");
        GUI.color = Color.white;
        GUILayout.Space(-5);
        if (GUILayout.Button("保存", EditorGUIStyle.GetMiddleButton, GUILayout.ExpandHeight(true), GUILayout.Width(100)))
        {
            this.ShowNotification(new GUIContent("正在保存。。。"));
            _primaryKeyInfo.Values.Clear();
            for (int i = 0; i < _serializeData.DataList.Count; i++)
            {
                string value = _serializeData.DataList[i].T.ToString();
                if (!_primaryKeyInfo.Values.ContainsKey(value))
                {
                    _primaryKeyInfo.Values.Add(value, 1);
                }
                else
                {
                    _primaryKeyInfo.Values[value]++;
                }
            }
            TableDatabaseUtils.SavaGlobalData();
            EditorUtility.SetDirty(_serializeData);
            AssetDatabase.SaveAssets();
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("添加", EditorGUIStyle.GetMiddleButton, GUILayout.ExpandHeight(true), GUILayout.Width(100)))
        {
            Test data = new Test();
            _serializeData.DataList.Add(data);
            if (_primaryKeyInfo.Values.ContainsKey(data.T.ToString()))
            {
                _primaryKeyInfo.Values[data.T.ToString()]++;
            }
            else
            {
                _primaryKeyInfo.Values.Add(data.T.ToString(), 1);
            }
            _pageMaxNum = (_serializeData.DataList.Count / _excelConfig.ShowCount);
            if (_serializeData.DataList.Count % _excelConfig.ShowCount > 0)
            {
                _pageMaxNum++;
            }
            _pageNum = 1;
        }
        GUILayout.Space(-5);

        GUILayout.EndHorizontal();

    }

    /// <summary>
    /// 显示搜索
    /// </summary>
    private void GUISearchInfo()
    {
        if (_searchFieldArray.Length <= 0)
        {
            return;
        }
        GUILayout.BeginHorizontal(EditorGUIStyle.GetPageLabelGuiStyle(), GUILayout.Height(25));
        GUI.color = new Color(1, 1, 1, 0);
        _searchFieldIndex = EditorGUI.Popup(new Rect(15, 25, 10, 14), _searchFieldIndex, _searchFieldArray);
        GUI.color = Color.white;
        GUI.SetNextControlName("SearchText");
        string str = GUILayout.TextField(_searchValue, "ToolbarSeachTextFieldPopup");
        if (string.IsNullOrEmpty(str) && !_isMouseInSearch)
        {
            GUI.color = new Color(0.8f, 0.8f, 0.8f);
            GUI.Label(new Rect(25, 28, 90, 12), _searchFieldArray[_searchFieldIndex], "RL Element");
            GUI.color = Color.white;
        }
        if (str != _searchValue)
        {
            _searchValue = str;
            if (!_isAdvancedSearch)
            {
                _dataList.Clear();
                _isSort = false;
                if (!string.IsNullOrEmpty(str))
                {
                    _dataList = SearchTableData.Search<Test>(_searchFieldArray[_searchFieldIndex], _searchValue, _serializeData.DataList);
                }
                else
                {
                    _dataList.Clear();
                    _dataList.AddRange(_serializeData.DataList);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_searchValue))
                {
                    SearchTableData.Hide();
                }
                else
                {
                    SearchTableData.Show(_searchValue, (a) =>
                    {
                        _searchValue = a + ":";
                        SearchTableData.Hide();
                        this.Focus();
                        GUI.FocusControl("SearchText");
                        this.Repaint();
                    });
                }
            }
        }
        else if (string.IsNullOrEmpty(str) && !_isSort)
        {
            _searchValue = str;
            _dataList.Clear();
            _dataList.AddRange(_serializeData.DataList);
        }

        if (string.IsNullOrEmpty(_searchValue))
        {
            GUILayout.Button("", "ToolbarSeachCancelButtonEmpty", GUILayout.Width(20));
        }
        else
        {
            if (GUILayout.Button("", "ToolbarSeachCancelButton", GUILayout.Width(20)))
            {
                _searchValue = "";
                SearchTableData.Hide();
                GUI.FocusControl("GUIArea");
            }
        }
        _isAdvancedSearch = GUILayout.Toggle(_isAdvancedSearch, "", GUILayout.Height(10), GUILayout.Width(10));
        if (GUILayout.Button("高级搜索", EditorGUIStyle.GetMiddleButton, GUILayout.Width(60)))
        {
            if (_isAdvancedSearch)
            {
                _dataList = SearchTableData.AdvancedSearch<Test>(this, _searchFieldArray[_searchFieldIndex], _searchValue, _serializeData.DataList);
            }
        }
        GUILayout.EndHorizontal();
    }

    private void GUIShowTableHead()
    {
        _areaWidht = 0;
        for (int i = 0; i < _divisionSlider.Count; i++)
        {
            _areaWidht += _excelConfig.ColumnsWidth[i];
        }
        Rect areaRect = new Rect(35, 55, _areaWidht, 30);
        rectList = new List<Rect>(_divisionSlider.HorizontalLayoutRects(areaRect));
        GUILayout.BeginArea(new Rect(3, 50, position.width - 20, 50));
        GUILayout.BeginScrollView(new Vector2(_tableScrollView.x, 0), GUIStyle.none, GUIStyle.none);
        GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(_areaWidht), GUILayout.Height(30));
        int index = 0;

        GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(30), GUILayout.MaxHeight(30));
        GUILayout.Space(2);
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        if (GUILayout.Button("R", EditorGUIStyle.GetTitleButton, GUILayout.Height(20), GUILayout.ExpandWidth(true)))
        {
            _searchValue = "";
            SearchTableData.Hide();
            GUI.FocusControl("GUIArea");
            _isSort = false;
        }
        GUILayout.EndVertical();
        GUILayout.Space(2);
        GUILayout.EndHorizontal();

        for (index = 0; index < _tableConfig.FieldList.Count; index++)
        {
            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(rectList[index].width), GUILayout.MaxHeight(30));
            string fieldName = _tableConfig.FieldList[index].Name;
            if (!string.IsNullOrEmpty(_tableConfig.FieldList[index].ShowName))
            {
                fieldName += "\r\n" + _tableConfig.FieldList[index].ShowName;
            }
            GUILayout.Space(5);
            GUILayout.BeginVertical();
            GUILayout.Space(2);
            if (GUILayout.Button(fieldName, EditorGUIStyle.GetTitleButton, GUILayout.Height(25)))
            {
                //_dataList.Clear();
                //_dataList.AddRange(_serializeData.DataList.OrderBy(a => a.T));
                //_isSort = true;
            }
            _excelConfig.ColumnsWidth[index] = rectList[index].width;
            GUILayout.EndVertical();
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(rectList[index].width), GUILayout.MaxHeight(30));
        GUILayout.Space(5);
        GUILayout.BeginVertical();
        GUILayout.Space(5);
        GUILayout.Button("", EditorGUIStyle.GetTitleButton, GUILayout.Height(20));
        GUILayout.Space(5);
        GUILayout.EndVertical();
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        float f = 0;
        if (_areaWidht > position.width)
        {
            float diff = _areaWidht - position.width + 20;
            f = diff * Mathf.Clamp(_tableScrollView.x / diff, 0f, 1f);
        }

        _divisionSlider.DoHorizontalSliders(areaRect, f);
        _divisionSlider.Resize(areaRect.width, DivisionSlider.ResizeMode.PrioritizeOuter);
    }

    private void GUIShowTableBody()
    {
        GUILayout.BeginArea(new Rect(3, 85, position.width - 20, position.height - 150));
        _tableScrollView = GUILayout.BeginScrollView(_tableScrollView, false, false, GUI.skin.horizontalScrollbar, GUIStyle.none);// );
        GUILayout.BeginVertical();
        Test removeData = null;
        int begin = (_pageNum - 1) * _excelConfig.ShowCount;
        int end = Mathf.Min(_pageNum * _excelConfig.ShowCount, _dataList.Count);
        for (int i = begin; i < end; i++)
        {

            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(_areaWidht + 30), GUILayout.MinHeight(30));

            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(30), GUILayout.ExpandHeight(true));
            GUILayout.Space(5);
            if (GUILayout.Button("", "OL Minus"))
            {
                removeData = _dataList[i];
            }
            GUILayout.EndHorizontal();

            float columnsWidth = 0;


            if (_primaryKeyInfo.Values.ContainsKey(_dataList[i].T.ToString()))
            {
                if (_primaryKeyInfo.Values[_dataList[i].T.ToString()] > 1)
                {
                    GUI.color = Color.red;
                }
            }
            columnsWidth = _excelConfig.ColumnsWidth[0];
            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));
            int key = (int)TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[0].FieldType, _dataList[i].T);
            if (key != _dataList[i].T)
            {
                _primaryKeyInfo.Values[_dataList[i].T.ToString()]--;
                if (_primaryKeyInfo.Values.ContainsKey(key.ToString()))
                {
                    _primaryKeyInfo.Values[key.ToString()]++;
                }
                else
                {
                    _primaryKeyInfo.Values.Add(key.ToString(), 1);
                }
                _dataList[i].T = key;
            }
            GUILayout.EndHorizontal();
            GUI.color = Color.white;

            columnsWidth = _excelConfig.ColumnsWidth[1];
            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));
            _dataList[i].a = (List<int>)TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[1].FieldType, _dataList[i].a);
            GUILayout.EndHorizontal();

            if (TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic.ContainsKey("Test2"))
            {
                if (TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic["Test2"].Values.ContainsKey(_dataList[i].b.ToString()))
                {
                    if (TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic["Test2"].Values[_dataList[i].b.ToString()] < 1)
                    {
                        GUI.color = new Color(1, 0.5f, 0);
                    }
                }
                else
                {
                    GUI.color = new Color(1, 0.5f, 0);
                }
            }
            else
            {
                GUI.color = new Color(1, 0.5f, 0);
            }
            columnsWidth = _excelConfig.ColumnsWidth[2];
            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));
            _dataList[i].b = (int)TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[2].FieldType, _dataList[i].b);
            GUILayout.EndHorizontal();
            GUI.color = Color.white;

            columnsWidth = _excelConfig.ColumnsWidth[3];
            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));
            _dataList[i].c = (int)TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[3].FieldType, _dataList[i].c);
            GUILayout.EndHorizontal();

            columnsWidth = _excelConfig.ColumnsWidth[4];
            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));
            _dataList[i].d = (int)TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[4].FieldType, _dataList[i].d);
            GUILayout.EndHorizontal();

            columnsWidth = _excelConfig.ColumnsWidth[5];
            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));
            _dataList[i].e = (int)TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[5].FieldType, _dataList[i].e);
            GUILayout.EndHorizontal();




            //GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(_excelConfig.ColumnsWidth[8]), GUILayout.ExpandHeight(true));
            //GUILayout.Label("");
            //GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }
        if (removeData != null)
        {
            _dataList.Remove(removeData);
            if (_serializeData.DataList.Contains(removeData))
            {
                _serializeData.DataList.Remove(removeData);
            }
            _primaryKeyInfo.Values[removeData.T.ToString()]--;
        }
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }


    private void GUIFooterInfo()
    {
        GUILayout.BeginArea(new Rect(3, position.height - 55, position.width - 20, 50));
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("显示个数:");
        int showCountIndex = GUILayout.Toolbar(_showCountIndex, _showCountArray);
        if (_showCountIndex != showCountIndex)
        {
            _showCountIndex = showCountIndex;
            _excelConfig.ShowCount = Convert.ToInt32(_showCountArray[_showCountIndex]);
            _pageMaxNum = (_serializeData.DataList.Count / _excelConfig.ShowCount);
            if (_serializeData.DataList.Count % _excelConfig.ShowCount > 0)
            {
                _pageMaxNum++;
            }
            _pageNum = 1;
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("◀"))
        {
            if (_pageNum != 1)
            {
                _pageNum--;
            }
        }
        GUI.color = Color.black;
        GUILayout.Label(_pageNum + "/" + _pageMaxNum);
        GUI.color = Color.white;
        if (GUILayout.Button("▶"))
        {
            if (_pageNum < _pageMaxNum)
            {
                _pageNum++;
            }
        }
        //GUILayout.FlexibleSpace();
        GUILayout.Space((position.width) / 2 - 150);
        GUILayout.Label("数据列表:");
        GUILayout.Label(_serializeData.DataList.Count + "条");

        GUILayout.EndHorizontal();
        GUILayout.Space(1);
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal("OL Title");
        GUI.color = Color.white;
        GUILayout.Label("Version 2.0.0 Beta");
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    #region 初始化和功能方法

    private void OnEnable()
    {
        CheckPlayerData();
        CheckPlayerConfig();
        CheckSearch();
        InitDivisionSlider();
    }

    private void OnDestroy()
    {
        _excelConfig.ColumnsWidth[_excelConfig.ColumnsWidth.Count - 1] = TableDatabaseUtils.TableConfigSerializeData.Setting.ColumnWidth;
        EditorUtility.SetDirty(_excelConfig);
        TableDatabaseUtils.SavaGlobalData();
    }


    private void CheckSearch()
    {
        List<string> tempList = new List<string>();
        for (int i = 0; i < _tableConfig.FieldList.Count; i++)
        {
            if (_tableConfig.FieldList[i].FieldType != "List" && _tableConfig.FieldList[i].FieldType != "Vector2" && _tableConfig.FieldList[i].FieldType != "Vector3" && _tableConfig.FieldList[i].FieldType != "Quaternion")
            {
                tempList.Add(_tableConfig.FieldList[i].Name);
            }
        }
        _searchFieldArray = tempList.ToArray();
    }

    private void InitDivisionSlider()
    {
        if (_divisionSlider == null)
        {
            _divisionSlider = new DivisionSlider(5, false, _excelConfig.ColumnsWidth.ToArray());
        }
    }

    private void CheckPlayerData()
    {
        for (int i = 0; i < TableDatabaseUtils.TableConfigSerializeData.TableConfigList.Count; i++)
        {
            if (TableDatabaseUtils.TableConfigSerializeData.TableConfigList[i].TableName == "Test")
            {
                _tableConfig = TableDatabaseUtils.TableConfigSerializeData.TableConfigList[i];
                break;
            }
        }
        if (string.IsNullOrEmpty(_tableConfig.DataPath))
        {
            string path = EditorUtility.SaveFilePanelInProject("保存文件", _tableConfig.TableName + ".asset", "asset", "");
            if (string.IsNullOrEmpty(path))
            {
                if (EditorUtility.DisplayDialog("保存失败", "路径为空!!!", "OK"))
                {
                    return;
                }
            }
            _tableConfig.DataPath = path;
            _serializeData = ScriptableObject.CreateInstance<TestSerializeData>();
            TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic.Add(_tableConfig.TableName, new PrimaryKeyInfo());
            _primaryKeyInfo = TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[_tableConfig.TableName];
            _primaryKeyInfo.TableName = _tableConfig.TableName;
            _primaryKeyInfo.PrimaryKey = _tableConfig.PrimaryKey;
            _primaryKeyInfo.PrimaryType = _tableConfig.PrimaryType;
            _primaryKeyInfo.Values = new Dictionary<string, int>();
            TableDatabaseUtils.SavaGlobalData();
            AssetDatabase.CreateAsset(_serializeData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            _serializeData = AssetDatabase.LoadAssetAtPath<TestSerializeData>(_tableConfig.DataPath);
            _primaryKeyInfo = TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[_tableConfig.TableName];
            if (_tableConfig.PrimaryKey != _primaryKeyInfo.PrimaryKey)
            {
                //更换主键
                _primaryKeyInfo.TableName = _tableConfig.TableName;
                _primaryKeyInfo.PrimaryKey = _tableConfig.PrimaryKey;
                _primaryKeyInfo.PrimaryType = _tableConfig.PrimaryType;
                _primaryKeyInfo.Values = new Dictionary<string, int>();
                for (int i = 0; i < _serializeData.DataList.Count; i++)
                {
                    string value = _serializeData.DataList[i].T.ToString();
                    if (!_primaryKeyInfo.Values.ContainsKey(value))
                    {
                        _primaryKeyInfo.Values.Add(value, 1);
                    }
                    else
                    {
                        _primaryKeyInfo.Values[value]++;
                    }
                }
                TableDatabaseUtils.SavaGlobalData();
            }
        }
        _dataList = new List<Test>();
        _dataList.AddRange(_serializeData.DataList);
    }

    private void CheckPlayerConfig()
    {
        if (_excelConfig == null)
        {
            string path = TableDatabaseUtils.EditorPath + "/Config/Table";
            if (!Directory.Exists(Path.GetFullPath(path)))
            {
                Directory.CreateDirectory(Path.GetFullPath(path));
            }
            path += "/TestConfig.asset";
            if (!File.Exists(Path.GetFullPath(path)))
            {
                _excelConfig = ScriptableObject.CreateInstance<TestConfig>();
                for (int i = 0; i < _tableConfig.FieldList.Count; i++)
                {
                    _excelConfig.ColumnsWidth.Add(TableDatabaseUtils.TableConfigSerializeData.Setting.ColumnWidth);
                }
                _excelConfig.ColumnsWidth.Add(TableDatabaseUtils.TableConfigSerializeData.Setting.ColumnWidth);
                AssetDatabase.CreateAsset(_excelConfig, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                _excelConfig = AssetDatabase.LoadAssetAtPath<TestConfig>(path);
                int diff = _excelConfig.ColumnsWidth.Count - _tableConfig.FieldList.Count;
                if (diff > 1)
                {
                    while (_excelConfig.ColumnsWidth.Count > _tableConfig.FieldList.Count + 1)
                    {
                        _excelConfig.ColumnsWidth.RemoveAt(0);
                    }
                }
                else
                {
                    while (_excelConfig.ColumnsWidth.Count <= _tableConfig.FieldList.Count)
                    {
                        _excelConfig.ColumnsWidth.Add(TableDatabaseUtils.TableConfigSerializeData.Setting.ColumnWidth);
                    }
                }
            }
        }
    }
    #endregion

}
