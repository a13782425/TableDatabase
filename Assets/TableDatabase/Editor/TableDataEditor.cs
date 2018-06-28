using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Reflection;

public class TableDataEditor : EditorWindow
{
    [MenuItem("Table/TableView")]
    static void CreateTable()
    {
        EditorWindow.GetWindowWithRect<TableDataEditor>(new Rect(100, 100, 800, 500), false, "数据操作").Init();
    }

    private static TableConfig _tempTableConfig;
    private static TableConfigSerializeData _tableConfigSerializeData;
    private static string _configPath;

    private int _selectTableConfigIndex = 0;


    private ScriptableObject _dataSerializeScriptData = null;

    private SerializedObject _tempSerializeddata = null;

    private List<ScriptableObject> _tempSerializedDataList = null;

    private string[] _tableNameArray;

    private Assembly _assembly;
    private Assembly _editorAssembly;
    private Type _dataType;
    private Type _serializedType;


    void OnGUI()
    {
        GUILayout.BeginHorizontal("sv_iconselector_back");
        GUILayout.BeginVertical();
        ShowDataList();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        ShowDataInfo();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

    }

    /// <summary>
    /// 显示数据详情
    /// </summary>
    private void ShowDataInfo()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 480), "", "box");
        GUILayout.BeginHorizontal();
        GUILayout.Space(5);
        GUILayout.Label("选择表：");
        int index = EditorGUILayout.Popup(_selectTableConfigIndex, _tableNameArray, "MiniToolbarPopup", GUILayout.Width(70));
        if (index != _selectTableConfigIndex)
        {
            _tempTableConfig = _tableConfigSerializeData.TableConfigList[index];
            _selectTableConfigIndex = index;

            _dataType = _assembly.GetType(_tempTableConfig.TableName);
            _serializedType = _assembly.GetType(_tempTableConfig.TableName + "SerializeData");
            if (_dataType == null)
            {
                _dataType = _editorAssembly.GetType(_tempTableConfig.TableName);
                _serializedType = _editorAssembly.GetType(_tempTableConfig.TableName + "SerializeData");
                if (_dataType == null)
                {
                    Debug.LogError("没有找到：" + _tempTableConfig.TableName + "类");
                    return;
                }
            }
            _dataSerializeScriptData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(_configPath);
        }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
        if (_tempTableConfig != null)
        {
            //显示数据列表
            if (string.IsNullOrEmpty(_tempTableConfig.DataPath))
            {
                //没有数据
                if (GUILayout.Button("创建数据"))
                {
                    SaveDataFile();
                }
            }
            else
            {
                //有数据
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("添加数据", "OL Plus"))
                {

                }
                GUILayout.EndHorizontal();
                GUILayout.BeginArea(new Rect(10, 40, 180, 400), "", "box");
                GUILayout.EndArea();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                if (GUILayout.Button("保存"))
                {
                    SaveDataFile();
                }
                GUILayout.Space(5);
                if (GUILayout.Button("另存为"))
                {
                    _tempTableConfig.DataPath = "";
                    SaveDataFile();
                }
                GUILayout.Space(5);
                GUILayout.EndHorizontal();

            }
        }
        GUILayout.EndArea();
    }



    /// <summary>
    /// 显示数据列表
    /// </summary>
    private void ShowDataList()
    {
        if (_tempSerializeddata != null)
        {
            GUILayout.BeginArea(new Rect(220, 10, 570, 480), "", "box");
            GUILayout.EndArea();
        }
    }



    #region 初始化和功能方法

    private void Init()
    {

    }

    void OnEnable()
    {
        GetConfigPath();
        ReadConfig();
        _tableNameArray = new string[_tableConfigSerializeData.TableConfigList.Count];
        for (int i = 0; i < _tableConfigSerializeData.TableConfigList.Count; i++)
        {
            _tableNameArray[i] = _tableConfigSerializeData.TableConfigList[i].TableName;
        }
        if (_selectTableConfigIndex >= 0)
        {
            _tempTableConfig = _tableConfigSerializeData.TableConfigList[_selectTableConfigIndex];
            _dataType = _assembly.GetType(_tempTableConfig.TableName);
            _serializedType = _assembly.GetType(_tempTableConfig.TableName + "SerializeData");
            if (_dataType == null)
            {
                _dataType = _editorAssembly.GetType(_tempTableConfig.TableName);
                _serializedType = _editorAssembly.GetType(_tempTableConfig.TableName + "SerializeData");
                if (_dataType == null)
                {
                    Debug.LogError("没有找到：" + _tempTableConfig.TableName + "类");
                    return;
                }
            }
            _dataSerializeScriptData = AssetDatabase.LoadAssetAtPath(_tempTableConfig.DataPath, _serializedType) as ScriptableObject;
            List<Test> test = _dataSerializeScriptData.GetType().GetField("DataList").GetValue(_dataSerializeScriptData) as List<Test>;
            Debug.LogError(test);
        }


    }

    private void WriteConfig()
    {
        EditorUtility.SetDirty(_tableConfigSerializeData);
        AssetDatabase.SaveAssets();
    }

    private void ReadConfig()
    {
        _tableConfigSerializeData = AssetDatabase.LoadAssetAtPath<TableConfigSerializeData>(_configPath);
    }

    /// <summary>
    /// 获取配置文件路径
    /// </summary>
    private void GetConfigPath()
    {
        string[] guids = AssetDatabase.FindAssets(typeof(CreateTableEditor).Name);
        if (guids.Length != 1)
        {
            Debug.LogError("guids存在多个");
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        path = Path.GetDirectoryName(path);
        _configPath = Path.Combine(path, "Config") + "/TableConfig.asset";
        _assembly = typeof(TableConfigSerializeData).Assembly;
        _editorAssembly = typeof(CreateTableEditor).Assembly;
        if (!File.Exists(_configPath))
        {
            _tableConfigSerializeData = ScriptableObject.CreateInstance<TableConfigSerializeData>();
            AssetDatabase.CreateAsset(_tableConfigSerializeData, _configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    private void SaveDataFile()
    {
        if (string.IsNullOrEmpty(_tempTableConfig.DataPath))
        {
            string path = EditorUtility.SaveFilePanelInProject("保存数据", _tempTableConfig.TableName + "SerializeData.asset", "asset", "");
            _tempTableConfig.DataPath = path;
            _dataSerializeScriptData = ScriptableObject.CreateInstance(_tempTableConfig.TableName + "SerializeData");
            EditorUtility.SetDirty(_tableConfigSerializeData);
            AssetDatabase.CreateAsset(_dataSerializeScriptData, _tempTableConfig.DataPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            EditorUtility.SetDirty(_dataSerializeScriptData);
            AssetDatabase.SaveAssets();
        }


    }

    #endregion
}
