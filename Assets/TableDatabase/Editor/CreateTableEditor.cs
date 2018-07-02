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

    [MenuItem("Table/CreateTable", priority = 50)]
    static void CreateTable()
    {
        EditorWindow.GetWindowWithRect<CreateTableEditor>(new Rect(100, 100, 800, 500), false, "创建表格");
    }

    private CreateTableEditor createTableEditor;

    private static TableConfig _tempTableConfig;

    //private List<string> _primaryKeyList = new List<string>();

    private string[] _enumArray;

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
        GUI.SetNextControlName("DataList");
        GUILayout.BeginArea(new Rect(10, 10, 200, 480), "", "box");
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal("OL Title");
        GUI.color = Color.white;
        GUILayout.Label("Table列表");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("添加", "OL Plus"))
        {
            _tempTableConfig = new TableConfig();
            _tempTableConfig.HasPrimaryKey = true;
            TableDatabaseUtils.TableConfigSerializeData.TableConfigList.Add(_tempTableConfig);
            _selectConfigIndex = TableDatabaseUtils.TableConfigSerializeData.TableConfigList.Count - 1;
            GUI.FocusControl("DataList");
        }
        GUILayout.EndHorizontal();
        int removeIndex = -1;
        GUILayout.Space(5);
        _listScollViewVec = GUILayout.BeginScrollView(_listScollViewVec, false, true);
        for (int i = 0; i < TableDatabaseUtils.TableConfigSerializeData.TableConfigList.Count; i++)
        {
            if (_tempTableConfig == TableDatabaseUtils.TableConfigSerializeData.TableConfigList[i])
            {
                GUI.color = Color.yellow;
            }
            GUILayout.BeginHorizontal("GroupBox", GUILayout.Height(30));
            GUI.color = Color.white;
            GUILayout.Space(5);
            if (GUILayout.Button(TableDatabaseUtils.TableConfigSerializeData.TableConfigList[i].TableName, "OL Title", GUILayout.Width(90)))
            {
                _selectConfigIndex = i;
                _tempTableConfig = TableDatabaseUtils.TableConfigSerializeData.TableConfigList[i];
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("删除", "OL Minus"))
            {
                removeIndex = i;
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        GUILayout.EndScrollView();
        if (removeIndex > -1)
        {

            EditorApplication.LockReloadAssemblies();
            TableConfig config = TableDatabaseUtils.TableConfigSerializeData.TableConfigList[removeIndex];
            TableDatabaseUtils.TableConfigSerializeData.TableConfigList.RemoveAt(removeIndex);
            if (!string.IsNullOrEmpty(config.DataPath))
            {
                File.Delete(Path.GetFullPath(config.DataPath));
                string editorPath = TableDatabaseUtils.EditorPath + "/TableEditor/" + config.TableName + "EditorData.cs";
                if (File.Exists(editorPath))
                {
                    File.Delete(editorPath);
                }
            }
            if (!string.IsNullOrEmpty(config.CodePath))
            {
                File.Delete(Path.GetFullPath(config.CodePath));
            }
            EditorApplication.UnlockReloadAssemblies();
            WriteConfig();
            AssetDatabase.Refresh();
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
                _tempTableConfig.FieldList.Add(new FieldConfig() { FieldIndex = 0, FieldType = "int", GenericIndex = 0, GenericType = "int" });
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginArea(new Rect(10, 30, 550, 440), "", "box");
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("表名：", GUILayout.Width(INFO_LABEL_WIDTH));
            _tempTableConfig.TableName = GUILayout.TextField(_tempTableConfig.TableName, GUILayout.Width(INFO_TEXT_WIDTH));
            GUILayout.Label("别名：", GUILayout.Width(INFO_LABEL_WIDTH));
            _tempTableConfig.ShowName = GUILayout.TextField(_tempTableConfig.ShowName, GUILayout.Width(INFO_TEXT_WIDTH));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("修饰：", GUILayout.Width(INFO_LABEL_WIDTH));
            _tempTableConfig.HasDescription = GUILayout.Toggle(_tempTableConfig.HasDescription, "", GUILayout.Width(20));
            if (_tempTableConfig.HasDescription)
            {
                string[] desArray = TableDatabaseUtils.GetDescription(_tempTableConfig);
                if (desArray.Length == 0)
                {
                    GUILayout.Label("没有合适的字段", GUILayout.Width(INFO_POPUP_WIDTH));
                    _tempTableConfig.Description = "";
                }
                else
                {
                    _tempTableConfig.DescriptionIndex = EditorGUILayout.Popup(_tempTableConfig.DescriptionIndex, desArray, GUILayout.Width(INFO_POPUP_WIDTH));
                    _tempTableConfig.Description = desArray[_tempTableConfig.DescriptionIndex];
                }
            }
            GUILayout.Space(5);
            GUILayout.Label("主键：", GUILayout.Width(INFO_LABEL_WIDTH));
            string[] keys = TableDatabaseUtils.GetPrimaryKey(_tempTableConfig);
            if (keys.Length == 1)
            {
                EditorGUILayout.Popup(0, new string[0], GUILayout.Width(INFO_POPUP_WIDTH));
            }
            else
            {
                _tempTableConfig.PrimaryIndex = EditorGUILayout.Popup(_tempTableConfig.PrimaryIndex, keys, GUILayout.Width(INFO_POPUP_WIDTH));

                _tempTableConfig.PrimaryKey = keys[_tempTableConfig.PrimaryIndex];
                for (int i = 0; i < _tempTableConfig.FieldList.Count; i++)
                {
                    if (_tempTableConfig.FieldList[i].Name == _tempTableConfig.PrimaryKey)
                    {
                        _tempTableConfig.PrimaryType = _tempTableConfig.FieldList[i].FieldType;
                        break;
                    }
                }
            }
            //}
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("保存配置"))
            {
                WriteConfig();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
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
            GUILayout.BeginHorizontal("GroupBox");
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("列名：", GUILayout.Width(INFO_LABEL_WIDTH));
            fieldConfig.Name = GUILayout.TextField(fieldConfig.Name, GUILayout.Width(INFO_TEXT_WIDTH));

            GUILayout.Label("类型：", GUILayout.Width(INFO_LABEL_WIDTH));
            int baseIndex = EditorGUILayout.Popup(fieldConfig.FieldIndex, TableDatabaseUtils.BaseType, GUILayout.Width(INFO_POPUP_WIDTH));
            if (baseIndex != fieldConfig.FieldIndex)
            {
                fieldConfig.FieldIndex = baseIndex;
                fieldConfig.FieldType = TableDatabaseUtils.BaseType[baseIndex];
                if (fieldConfig.FieldType == "enum" && !string.IsNullOrEmpty(fieldConfig.EnumName))
                {
                    for (int j = 0; j < _enumArray.Length; j++)
                    {
                        if (fieldConfig.EnumName == _enumArray[j])
                        {
                            fieldConfig.EnumIndex = j;
                            break;
                        }
                    }
                }
            }
            if (fieldConfig.FieldType == "List")
            {
                GUILayout.Label("集合类型:", GUILayout.Width(50));
                int collectionIndex = EditorGUILayout.Popup(fieldConfig.GenericIndex, TableDatabaseUtils.GenericType, GUILayout.Width(INFO_POPUP_WIDTH));
                if (collectionIndex != fieldConfig.GenericIndex)
                {
                    fieldConfig.GenericIndex = collectionIndex;
                    fieldConfig.GenericType = TableDatabaseUtils.GenericType[collectionIndex];
                }
            }
            if (fieldConfig.FieldType == "enum")
            {
                GUILayout.Label("枚举类型:", GUILayout.Width(50));
                fieldConfig.EnumIndex = EditorGUILayout.Popup(fieldConfig.EnumIndex, _enumArray, GUILayout.Width(INFO_POPUP_WIDTH));
                fieldConfig.EnumName = _enumArray[fieldConfig.EnumIndex];
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("删除", "OL Minus"))
            {
                removeList.Add(fieldConfig);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("别名：", GUILayout.Width(INFO_LABEL_WIDTH));
            fieldConfig.ShowName = GUILayout.TextField(fieldConfig.ShowName, GUILayout.Width(INFO_TEXT_WIDTH));
            if (fieldConfig.FieldType == "int" || fieldConfig.FieldType == "string")
            {
                GUILayout.Label("外键：", GUILayout.Width(INFO_LABEL_WIDTH));
                fieldConfig.HasForeignKey = GUILayout.Toggle(fieldConfig.HasForeignKey, "", GUILayout.Width(20));//, "MiniToolbarPopup"
                if (fieldConfig.HasForeignKey)
                {
                    //GUILayout.Label("外键：", GUILayout.Width(INFO_LABEL_WIDTH));
                    string[] keys = TableDatabaseUtils.GetForeignKey(_tempTableConfig, fieldConfig.FieldType);
                    if (keys.Length == 1)
                    {
                        fieldConfig.ForeignKeyIndex = 0;
                        fieldConfig.ForeignKey = "";
                        keys = new string[0];
                        EditorGUILayout.Popup(fieldConfig.ForeignKeyIndex, keys, GUILayout.Width(INFO_POPUP_WIDTH));
                    }
                    else
                    {
                        fieldConfig.ForeignKeyIndex = EditorGUILayout.Popup(fieldConfig.ForeignKeyIndex, keys, GUILayout.Width(INFO_POPUP_WIDTH));
                        fieldConfig.ForeignKey = keys[fieldConfig.ForeignKeyIndex];
                    }
                }
            }
            GUILayout.Space(10);
            GUILayout.Label("是否导出：", GUILayout.Width(INFO_LABEL_WIDTH + 20));
            fieldConfig.IsExport = GUILayout.Toggle(fieldConfig.IsExport, "", GUILayout.Width(20));//, "MiniToolbarPopup"
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
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
        //GetConfigPath();
        //ReadConfig();
        if (_selectConfigIndex >= 0)
        {
            if (_selectConfigIndex >= TableDatabaseUtils.TableConfigSerializeData.TableConfigList.Count)
            {
                _selectConfigIndex = 0;
            }
            else
            {
                _tempTableConfig = TableDatabaseUtils.TableConfigSerializeData.TableConfigList[_selectConfigIndex];
            }
        }
        _enumArray = TableDatabaseUtils.GetEnums();
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
            _tempTableConfig.CodePath = Path.GetDirectoryName(Path.GetFullPath(path)) + "/" + csName;
        }
        else
        {
            if (Path.GetFileName(_tempTableConfig.CodePath) != csName)
            {
                _tempTableConfig.CodePath = Path.GetDirectoryName(_tempTableConfig.CodePath) + "/" + csName;
            }
        }
        if (string.IsNullOrEmpty(_tempTableConfig.PrimaryKey))
        {
            if (EditorUtility.DisplayDialog("保存失败", "主键为空!!!", "OK"))
            {
                _isGenerateCode = false;
                return;
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
        //if (!string.IsNullOrEmpty(_tempTableConfig.ShowName))
        //{
        //    codeSb.AppendLine("[ShowName(\"" + _tempTableConfig.ShowName + "\")]");
        //}
        codeSb.AppendLine("[System.Serializable]");
        codeSb.AppendLine("public class " + tableName);
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
            //if (!string.IsNullOrEmpty(fieldConfig.ShowName))
            //{
            //    codeSb.AppendLine("    [ShowName(\"" + fieldConfig.ShowName + "\")]");
            //}
            switch (fieldConfig.FieldType)
            {
                case "List":
                    codeSb.AppendLine("    public List<" + fieldConfig.GenericType + "> " + fieldConfig.Name + " = new List<" + fieldConfig.GenericType + ">();");
                    break;
                case "enum":
                    codeSb.AppendLine("    public " + fieldConfig.EnumName + " " + fieldConfig.Name + ";");
                    break;
                default:
                    codeSb.AppendLine("    public " + fieldConfig.FieldType + " " + fieldConfig.Name + ";");
                    break;
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
            GenerateEditorCode code = new GenerateEditorCode(_tempTableConfig);
            code.GenerateCode();
            EditorApplication.UnlockReloadAssemblies();

            _isGenerateCode = false;
            if (EditorUtility.DisplayDialog("生成成功", "生成成功", "OK"))
            {
                AssetDatabase.Refresh();
            }
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
        EditorUtility.SetDirty(TableDatabaseUtils.TableConfigSerializeData);
        AssetDatabase.SaveAssets();
    }

    #endregion

}
