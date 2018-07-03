using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TestEditor : EditorWindow
{
    [MenuItem("Table/NewTables/玩家信息", priority = 30)]
    static void CreateTable()
    {
        EditorWindow.GetWindow<TestEditor>(false, "玩家信息数据").minSize = new Vector2(400, 200);
    }

    private PlayerInfoConfig _playerInfoConfig;

    private TableConfig _tableConfig;

    private PlayerInfoSerializeData _serializeData;

    private DivisionSlider _divisionSlider;


    private string[] _searchFieldArray;

    private int _searchFieldIndex = 0;

    private string _searchValue;

    private Dictionary<string, List<object>> _foreignKeyDic;

    private float _areaWidht = 0;

    void OnGUI()
    {
        if (_serializeData == null)
        {
            return;
        }
        //GUILayout.BeginHorizontal("sv_iconselector_back");
        GUI.SetNextControlName("GUIArea");
        GUILayout.BeginArea(new Rect(5, 5, position.width - 10, position.height - 10), "", "box");
        GUILayout.BeginVertical();
        GUITitleInfo();
        GUISearchInfo();
        GUIShowTableHead();
        GUIShowTableBody();

        //ShowDataList();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        //ShowDataInfo();
        GUILayout.EndVertical();
        GUILayout.EndArea();
        //GUILayout.EndHorizontal();
        if (Event.current.keyCode == KeyCode.Escape)
        {
            GUI.FocusControl("GUIArea");
        }
    }

    private void GUIShowTableBody()
    {
        GUILayout.BeginArea(new Rect(5, 100, position.width - 20, position.height - 180), "", "box");
        _tableScrollView = GUILayout.BeginScrollView(_tableScrollView, false, false, GUI.skin.horizontalScrollbar, GUIStyle.none);
        GUILayout.BeginVertical();

        for (int i = 0; i < 100; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("dasdasdasdasdasdas" + i, GUILayout.Width(_areaWidht));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private Vector2 _tableScrollView;
    private void GUIShowTableHead()
    {
        _areaWidht = 0;
        for (int i = 0; i < _divisionSlider.Count; i++)
        {
            _areaWidht += _divisionSlider.GetSize(i);
        }

        Rect areaRect = new Rect(5, 55, _areaWidht, 30);
        GUI.BeginScrollView(new Rect(5, 55, position.width - 20, 30), new Vector2(_tableScrollView.x, 0), areaRect, false, false, GUIStyle.none, GUIStyle.none);
        GUILayout.BeginHorizontal();
        foreach (Rect rect in _divisionSlider.HorizontalLayoutRects(areaRect))
        {
            GUILayout.BeginArea(rect, "", "Box");
            GUILayout.Button("rect", "OL Title");
            GUILayout.EndArea();
        }
        GUILayout.EndHorizontal();
        _divisionSlider.DoHorizontalSliders(areaRect);
        _divisionSlider.Resize(areaRect.width, DivisionSlider.ResizeMode.PrioritizeOuter);
        GUI.EndScrollView();
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
        GUILayout.BeginHorizontal("box");
        GUI.color = new Color(1, 1, 1, 0);
        _searchFieldIndex = EditorGUI.Popup(new Rect(10, 25, 10, 14), _searchFieldIndex, _searchFieldArray);
        GUI.color = Color.white;
        GUI.SetNextControlName("SeachText");
        string str = GUILayout.TextField(_searchValue, "ToolbarSeachTextFieldPopup");
        if (string.IsNullOrEmpty(str) && GUI.GetNameOfFocusedControl() != "SeachText")
        {
            GUI.color = new Color(0.8f, 0.8f, 0.8f);
            GUI.Label(new Rect(25, 30, 90, 12), _searchFieldArray[_searchFieldIndex], "RL Element");
            GUI.color = Color.white;
        }
        if (str != _searchValue)
        {
            _searchValue = str;
            //_searchDataDic.Clear();
            //if (!string.IsNullOrEmpty(str))
            //{
            //    for (int i = 0; i < _serializeData.DataList.Count; i++)
            //    {
            //        PlayerInfo data = _serializeData.DataList[i];
            //        string temp = data.GetType().GetField(_searchFieldArray[_searchFieldIndex]).GetValue(data).ToString();
            //        if (temp.Contains(str))
            //        {
            //            _searchDataDic.Add(i, data);
            //        }
            //    }
            //    _isSearch = true;
            //}
            //else
            //{
            //    _isSearch = false;
            //}
            //_searchValue = str;
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
                GUI.FocusControl("GUIArea");
                //_isSearch = false;
                //_searchDataDic.Clear();
            }
        }

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 显示Title
    /// </summary>
    private void GUITitleInfo()
    {
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal("OL Title");
        GUI.color = Color.white;
        GUILayout.Label("数据列表:");
        GUILayout.Label(_serializeData.DataList.Count + "条");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("添加", "OL Plus"))
        {
        }
        GUILayout.EndHorizontal();

    }



    private void OnEnable()
    {
        CheckPlayerConfig();
        CheckPlayerData();
        CheckSearch();
        InitDivisionSlider();
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
            if (_tableConfig.FieldList[i].HasForeignKey)
            {
                _foreignKeyDic.Add(_tableConfig.FieldList[i].ForeignKey, new List<object>());
            }
        }
        _searchFieldArray = tempList.ToArray();
    }

    private void InitDivisionSlider()
    {
        if (_divisionSlider == null)
        {
            List<float> sizes = new List<float>() {
                _playerInfoConfig.IdColumnWidth,
                _playerInfoConfig.NameColumnWidth,
                _playerInfoConfig.IconColumnWidth,
                _playerInfoConfig.BackinfoColumnWidth,
                _playerInfoConfig.PosColumnWidth,
                _playerInfoConfig.RotColumnWidth,
                _playerInfoConfig.ClothsColumnWidth
            };
            sizes.Add(TableDatabaseUtils.TableConfigSerializeData.Setting.ColumnWidth);

            _divisionSlider = new DivisionSlider(5, false, sizes.ToArray());
        }
    }

    private void CheckPlayerData()
    {
        for (int i = 0; i < TableDatabaseUtils.TableConfigSerializeData.TableConfigList.Count; i++)
        {
            if (TableDatabaseUtils.TableConfigSerializeData.TableConfigList[i].TableName == "PlayerInfo")
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
            _serializeData = ScriptableObject.CreateInstance<PlayerInfoSerializeData>();
            TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyInfoList.Add(new PrimaryKeyInfo() { TableName = "PlayerInfo", PrimaryKey = "Id", PrimaryType = "int" });
            EditorUtility.SetDirty(TableDatabaseUtils.TableConfigSerializeData);
            AssetDatabase.CreateAsset(_serializeData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            _serializeData = AssetDatabase.LoadAssetAtPath<PlayerInfoSerializeData>(_tableConfig.DataPath);
        }
    }

    private void CheckPlayerConfig()
    {
        if (_playerInfoConfig == null)
        {
            string path = TableDatabaseUtils.EditorPath + "/Config/Table";
            if (!Directory.Exists(Path.GetFullPath(path)))
            {
                Directory.CreateDirectory(Path.GetFullPath(path));
            }
            path += "/PlayerInfoConfig.asset";
            if (!File.Exists(Path.GetFullPath(path)))
            {
                _playerInfoConfig = ScriptableObject.CreateInstance<PlayerInfoConfig>();
                AssetDatabase.CreateAsset(_playerInfoConfig, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                _playerInfoConfig = AssetDatabase.LoadAssetAtPath<PlayerInfoConfig>(path);
            }
        }
    }

}
