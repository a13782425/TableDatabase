using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CreateTableEditor : EditorWindow
{

    [MenuItem("Table/CreateTable")]
    static void CreateTable()
    {
        EditorWindow.GetWindowWithRect<CreateTableEditor>(new Rect(100, 100, 800, 500), false, "创建表格").Init();
    }

    private CreateTableEditor createTableEditor;
    private static string _configPath;

    private bool _isCreate = false;

    private static TableConfig _tempTableConfig;
    private static TableConfigSerializeData _tableConfigSerializeData;

    private string[] _baseType = new string[] { "int", "float", "string", "bool", "Vector2", "Vector3", "Quaternion", "Sprite", "Texture", "GameObject", "List" };

    private string[] _collectionType = new string[] { "int", "float", "string", "bool", "Vector2", "Vector3", "Quaternion", "Sprite", "Texture", "GameObject" };

    private const int INFO_LABEL_WIDTH = 40;
    private const int INFO_TEXT_WIDTH = 60;
    private const int INFO_POPUP_WIDTH = 70;

    private bool _isGenerateCode = false;

    private int _selectConfigIndex = -1;

    private Vector2 _listScollViewVec;
    private Vector2 _infoScollViewVec;

    private void OnGUI()
    {
        if (_isGenerateCode)
        {
            return;
        }
        GUILayout.BeginHorizontal("sv_iconselector_back");
        GUILayout.BeginVertical();
        ShowTypeList();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        ShowTypeInfo();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 显示类型列表
    /// </summary>
    private void ShowTypeList()
    {
        GUILayout.BeginArea(new Rect(10, 10, 200, 480), "", "box");
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal("OL Title");
        GUI.color = Color.white;
        GUILayout.Label("Table列表");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("添加", "OL Plus"))
        {
            _tempTableConfig = new TableConfig();
            _tableConfigSerializeData.TableConfigList.Add(_tempTableConfig);
            _selectConfigIndex = _tableConfigSerializeData.TableConfigList.Count - 1;
        }
        GUILayout.EndHorizontal();
        List<TableConfig> removeList = new List<TableConfig>();
        GUILayout.Space(5);
        _listScollViewVec = GUILayout.BeginScrollView(_listScollViewVec, false, true);
        for (int i = 0; i < _tableConfigSerializeData.TableConfigList.Count; i++)
        {
            if (_tempTableConfig == _tableConfigSerializeData.TableConfigList[i])
            {
                GUI.color = Color.yellow;
            }
            GUILayout.BeginHorizontal("GroupBox", GUILayout.Height(30));
            GUI.color = Color.white;
            GUILayout.Space(5);
            if (GUILayout.Button(_tableConfigSerializeData.TableConfigList[i].TableName, "OL Title", GUILayout.Width(90)))
            {
                _selectConfigIndex = i;
                _tempTableConfig = _tableConfigSerializeData.TableConfigList[i];
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("删除", "OL Minus"))
            {
                removeList.Add(_tableConfigSerializeData.TableConfigList[i]);
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        GUILayout.EndScrollView();
        if (removeList.Count > 0)
        {
            for (int i = 0; i < removeList.Count; i++)
            {
                _tableConfigSerializeData.TableConfigList.Remove(removeList[i]);
                //todo 删除一个表（同时删除数据和代码）
            }
            WriteConfig();
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// 显示类型具体信息
    /// </summary>
    private void ShowTypeInfo()
    {
        if (_tempTableConfig != null)
        {
            GUILayout.BeginArea(new Rect(220, 10, 570, 480), "", "box");
            GUI.color = new Color(0.8f, 0.8f, 0.8f); ;
            GUILayout.BeginHorizontal("OL Title");
            GUI.color = Color.white;
            GUILayout.Label("Table信息");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("添加字段", "OL Plus"))
            {
                _tempTableConfig.FieldList.Add(new FieldConfig() { FieldIndex = 0, FieldType = "int" });
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginArea(new Rect(10, 30, 550, 440), "", "box");
            GUILayout.BeginHorizontal();
            GUILayout.Label("表名：", GUILayout.Width(INFO_LABEL_WIDTH));
            _tempTableConfig.TableName = GUILayout.TextField(_tempTableConfig.TableName, GUILayout.Width(INFO_TEXT_WIDTH));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("保存配置"))
            {
                WriteConfig();
            }
            GUILayout.Space(30);
            GUILayout.EndHorizontal();
            ShowConfigInfo();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生成脚本"))
            {
                GenerateCode();
            }
            if (GUILayout.Button("另存脚本"))
            {
                _tempTableConfig.CodePath = "";
                _tempTableConfig.DataPath = "";
                GenerateCode();
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
            GUILayout.EndVertical();

            GUILayout.EndArea();

        }
    }

    private void ShowConfigInfo()
    {
        _infoScollViewVec = GUILayout.BeginScrollView(_infoScollViewVec, false, false);
        List<FieldConfig> removeList = new List<FieldConfig>();
        for (int i = 0; i < _tempTableConfig.FieldList.Count; i++)
        {
            FieldConfig fieldConfig = _tempTableConfig.FieldList[i];
            GUILayout.BeginHorizontal();
            GUILayout.Label("列名：", GUILayout.Width(INFO_LABEL_WIDTH));
            fieldConfig.Name = GUILayout.TextField(fieldConfig.Name, GUILayout.Width(INFO_TEXT_WIDTH));
            GUILayout.Label("别名：", GUILayout.Width(INFO_LABEL_WIDTH));
            fieldConfig.Name = GUILayout.TextField(fieldConfig.Name, GUILayout.Width(INFO_TEXT_WIDTH));
            bool isShow = GUILayout.Toggle(true, "默认显示");
            EditorGUILayout
            GUILayout.Label("类型：", GUILayout.Width(INFO_LABEL_WIDTH));
            int baseIndex = EditorGUILayout.Popup(fieldConfig.FieldIndex, _baseType, "MiniToolbarPopup", GUILayout.Width(INFO_POPUP_WIDTH));
            if (baseIndex != fieldConfig.FieldIndex)
            {
                fieldConfig.FieldIndex = baseIndex;
                fieldConfig.FieldType = _baseType[baseIndex];
            }
            if (baseIndex == _baseType.Length - 1)
            {
                GUILayout.Label("子类型：", GUILayout.Width(INFO_LABEL_WIDTH));
                int collectionIndex = EditorGUILayout.Popup(fieldConfig.GenericIndex, _collectionType, "MiniToolbarPopup", GUILayout.Width(INFO_POPUP_WIDTH));
                if (collectionIndex != fieldConfig.GenericIndex)
                {
                    fieldConfig.GenericIndex = collectionIndex;
                    fieldConfig.GenericType = _collectionType[collectionIndex];
                }
            }
            else
            {
                fieldConfig.GenericIndex = 0;
                fieldConfig.GenericType = "";
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("删除", "OL Minus"))
            {
                removeList.Add(fieldConfig);
                //removeList.Add(_tableConfigSerializeData.TableConfigList[i]);
            }
            GUILayout.Space(30);
            GUILayout.EndHorizontal();
        }
        if (removeList.Count > 0)
        {
            for (int i = 0; i < removeList.Count; i++)
            {
                _tempTableConfig.FieldList.Remove(removeList[i]);
                //todo 删除一个表（同时删除数据和代码）
            }
            WriteConfig();
        }
        GUILayout.EndScrollView();
    }

    #region 初始化和功能方法

    void OnEnable()
    {
        GetConfigPath();
        ReadConfig();
        if (_selectConfigIndex >= 0)
        {
            _tempTableConfig = _tableConfigSerializeData.TableConfigList[_selectConfigIndex];
        }
    }

    private void Init()
    {
        if (_isCreate)
        {
            return;
        }
        _isCreate = true;
    }

    private void GenerateCode()
    {
        _isGenerateCode = true;
        string csName = _tempTableConfig.TableName + "SerializeData.cs";
        if (string.IsNullOrEmpty(_tempTableConfig.CodePath))
        {
            string path = EditorUtility.SaveFilePanelInProject("保存文件", _tempTableConfig.TableName + "SerializeData.cs", "cs", "");
            if (string.IsNullOrEmpty(path))
            {
                if (EditorUtility.DisplayDialog("保存失败", "路径为空!!!", "OK"))
                {
                    _isGenerateCode = false;
                    return;
                }
            }
            _tempTableConfig.CodePath = path;
            _tempTableConfig.CodePath = Path.GetDirectoryName(Path.GetFullPath(_tempTableConfig.CodePath)) + "/" + csName;
        }
        else
        {
            if (Path.GetFileName(_tempTableConfig.CodePath) != csName)
            {
                _tempTableConfig.CodePath = Path.GetDirectoryName(_tempTableConfig.CodePath) + "/" + csName;
            }
        }
        string tableName = _tempTableConfig.TableName;
        StringBuilder codeSb = new StringBuilder();
        codeSb.AppendLine("//------------------------------------------------------------------------------------------------------------");
        codeSb.AppendLine("//-----------------------------------generate file " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "----------------------------------------");
        codeSb.AppendLine("//------------------------------------------------------------------------------------------------------------");
        codeSb.AppendLine("");
        codeSb.AppendLine("using System;");
        codeSb.AppendLine("using System.Collections.Generic;");
        codeSb.AppendLine("using UnityEngine;");
        codeSb.AppendLine("");
        //生成SerializeData
        codeSb.AppendLine("public class " + tableName + "SerializeData : ScriptableObject");
        codeSb.AppendLine("{");
        codeSb.AppendLine("    public List<" + tableName + "> DataList = new List<" + tableName + ">();");
        codeSb.AppendLine("}");

        codeSb.AppendLine("");
        codeSb.AppendLine("[System.Serializable]");
        codeSb.AppendLine("public class " + tableName + " : ScriptableObject");
        codeSb.AppendLine("{");
        List<string> fieldNameList = new List<string>();
        for (int i = 0; i < _tempTableConfig.FieldList.Count; i++)
        {
            FieldConfig fieldConfig = _tempTableConfig.FieldList[i];
            if (tableName == fieldConfig.Name)
            {
                if (EditorUtility.DisplayDialog("生成失败", "变量名和类名重复!!!", "OK"))
                {
                    _isGenerateCode = false;
                    return;
                }
            }
            if (string.IsNullOrEmpty(fieldConfig.Name))
            {
                if (EditorUtility.DisplayDialog("生成失败", "变量名为空!!!", "OK"))
                {
                    _isGenerateCode = false;
                    return;
                }
            }
            if (fieldNameList.Contains(fieldConfig.Name))
            {
                if (EditorUtility.DisplayDialog("生成失败", "变量名重复!!!", "OK"))
                {
                    _isGenerateCode = false;
                    return;
                }
            }
            fieldNameList.Add(fieldConfig.Name);
            if (string.IsNullOrEmpty(fieldConfig.GenericType))
            {
                //单一类型
                codeSb.AppendLine("    public " + fieldConfig.FieldType + " " + fieldConfig.Name + ";");
            }
            else
            {
                //集合类型
                codeSb.AppendLine("    public List<" + fieldConfig.GenericType + "> " + fieldConfig.Name + " = new List<" + fieldConfig.GenericType + ">();");
            }
        }
        codeSb.AppendLine("}");

        EditorApplication.LockReloadAssemblies();
        try
        {
            if (!File.Exists(_tempTableConfig.CodePath))
            {
                File.Create(_tempTableConfig.CodePath).Dispose();
            }
            File.WriteAllText(_tempTableConfig.CodePath, codeSb.ToString());
            EditorApplication.UnlockReloadAssemblies();
            _isGenerateCode = false;
            EditorUtility.DisplayDialog("生成成功", "生成成功", "OK");
        }
        catch (Exception ex)
        {
            EditorApplication.UnlockReloadAssemblies();
            _isGenerateCode = false;
            EditorUtility.DisplayDialog("生成失败", ex.Message, "OK");
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
        if (!File.Exists(_configPath))
        {
            _tableConfigSerializeData = ScriptableObject.CreateInstance<TableConfigSerializeData>();
            AssetDatabase.CreateAsset(_tableConfigSerializeData, _configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    #endregion

}
