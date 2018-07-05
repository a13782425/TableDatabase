using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GenerateEditorCode
{
    private TableConfig _currentConfig;

    private string _csPath;

    private string _templatePath;

    private Encoding encoding = new UTF8Encoding();

    public GenerateEditorCode(TableConfig config)
    {
        if (config == null)
        {
            throw new Exception("表配置异常");
        }
        _currentConfig = config;
        string[] guids = AssetDatabase.FindAssets(typeof(GenerateEditorCode).Name);
        if (guids.Length != 1)
        {
            Debug.LogError("guids存在多个");
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        path = Path.GetDirectoryName(Path.GetFullPath(path));
        _templatePath = Path.Combine(path, "Template") + "/DataEditorTemplate";
        if (!File.Exists(_templatePath))
        {
            Debug.LogError(_templatePath);
            throw new Exception("生成模板被删除!");
        }
        _csPath = Path.Combine(path, "TableEditor") + "/" + _currentConfig.TableName + "EditorData.cs";
        if (!Directory.Exists(Path.GetDirectoryName(_csPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_csPath));
        }
        if (!File.Exists(_csPath))
        {
            File.Create(_csPath).Dispose();
        }
    }
    public void GenerateCode()
    {
        if (!GenerateDataCode())
            return;
        GenerateConfig();

        GenerateEditor();
    }

    private bool GenerateDataCode()
    {
        string tableName = _currentConfig.TableName;
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
        for (int i = 0; i < _currentConfig.FieldList.Count; i++)
        {
            FieldConfig fieldConfig = _currentConfig.FieldList[i];
            if (tableName == fieldConfig.Name)
            {
                if (EditorUtility.DisplayDialog("生成失败", "变量名和类名重复!!!", "OK"))
                {
                    return false;
                }
            }
            if (string.IsNullOrEmpty(fieldConfig.Name))
            {
                if (EditorUtility.DisplayDialog("生成失败", "变量名为空!!!", "OK"))
                {
                    return false;
                }
            }
            if (fieldNameList.Contains(fieldConfig.Name))
            {
                if (EditorUtility.DisplayDialog("生成失败", "变量名重复!!!", "OK"))
                {
                    return false;
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
        if (!File.Exists(_currentConfig.CodePath))
        {
            File.Create(_currentConfig.CodePath).Dispose();
        }
        File.WriteAllText(_currentConfig.CodePath, codeSb.ToString());
        return true;
    }

    private void GenerateConfig()
    {
        string directory = TableDatabaseUtils.EditorFullPath + "/Data/Table";
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        string filePath = directory + "/" + _currentConfig.TableName + "Config.cs";
        if (!File.Exists(filePath))
        {
            File.Create(filePath).Dispose();
        }
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();
        sb.AppendLine("public class " + _currentConfig.TableName + "Config : ScriptableObject");
        sb.AppendLine("{");
        sb.AppendLine("    public List<float> ColumnsWidth = new List<float>();");
        sb.AppendLine("    public int ShowCount = 10;");
        sb.AppendLine("}");
        File.WriteAllText(filePath, sb.ToString());
    }

    private void GenerateEditor()
    {
        string templateText = File.ReadAllText(_templatePath, encoding);
        templateText = templateText.Replace("$(TableShowName)", string.IsNullOrEmpty(_currentConfig.ShowName) ? _currentConfig.TableName : _currentConfig.ShowName);
        templateText = templateText.Replace("$(DataClassName)", _currentConfig.TableName);
        templateText = templateText.Replace("$(Primary)", _currentConfig.PrimaryKey);
        //if (_currentConfig.HasDescription)
        //{
        //    if (!string.IsNullOrEmpty(_currentConfig.Description))
        //    {
        //        //object.Equals(data.PlayerId, null) ? "null" : data.PlayerId.ToString()
        //        templateText = templateText.Replace("$(SelectName)", "object.Equals(data." + _currentConfig.Description + ", null) ? \"null\" : data." + _currentConfig.Description + ".ToString()");
        //    }
        //    else
        //    {
        //        templateText = templateText.Replace("$(SelectName)", "\"选择\"");
        //    }
        //}
        //else
        //{
        //    templateText = templateText.Replace("$(SelectName)", "\"选择\"");
        //}
        StringBuilder sb = new StringBuilder();
        GenerateDataInfo(sb);
        templateText = templateText.Replace("$(ShowDataInfo)", sb.ToString());
        File.WriteAllText(_csPath, templateText, encoding);
    }

    private void GenerateDataInfo(StringBuilder sb)
    {
        for (int i = 0; i < _currentConfig.FieldList.Count; i++)
        {
            FieldConfig fieldConfig = _currentConfig.FieldList[i];
            string fieldName = string.IsNullOrEmpty(fieldConfig.ShowName) ? fieldConfig.Name : fieldConfig.ShowName;
            if (fieldConfig.Name == _currentConfig.PrimaryKey)
            {
                sb.AppendLine("            if (_primaryKeyInfo.Values.ContainsKey(_dataList[i]." + fieldConfig.Name + ".ToString()))");
                sb.AppendLine("            {");
                sb.AppendLine("                if (_primaryKeyInfo.Values[_dataList[i]." + fieldConfig.Name + ".ToString()] > 1)");
                sb.AppendLine("                {");
                sb.AppendLine("                    GUI.color = Color.red;");
                sb.AppendLine("                }");
                sb.AppendLine("            }");
                sb.AppendLine("            columnsWidth = _excelConfig.ColumnsWidth[" + i + "];");
                sb.AppendLine("            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));");
                sb.AppendLine("            " + _currentConfig.PrimaryType + " key = (" + fieldConfig.FieldType + ")TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.Name + ");");
                sb.AppendLine("            if (key != _dataList[i]." + fieldConfig.Name + ")");
                sb.AppendLine("            {");
                sb.AppendLine("                _primaryKeyInfo.Values[_dataList[i]." + fieldConfig.Name + ".ToString()]--;");
                sb.AppendLine("                if (_primaryKeyInfo.Values.ContainsKey(key.ToString()))");
                sb.AppendLine("                {");
                sb.AppendLine("                    _primaryKeyInfo.Values[key.ToString()]++;");
                sb.AppendLine("                }");
                sb.AppendLine("                else");
                sb.AppendLine("                {");
                sb.AppendLine("                    _primaryKeyInfo.Values.Add(key.ToString(), 1);");
                sb.AppendLine("                }");
                sb.AppendLine("                _dataList[i]." + fieldConfig.Name + " = key;");
                sb.AppendLine("            }");
                sb.AppendLine("            GUILayout.EndHorizontal();");
                sb.AppendLine("            GUI.color = Color.white;");
                sb.AppendLine();
            }
            else if (fieldConfig.HasForeignKey)
            {
                sb.AppendLine("            if (TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic.ContainsKey(\"" + fieldConfig.ForeignKey + "\"))");
                sb.AppendLine("            {");
                sb.AppendLine("                if (TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[\"" + fieldConfig.ForeignKey + "\"].Values.ContainsKey(_dataList[i]." + fieldConfig.Name + ".ToString()))");
                sb.AppendLine("                {");
                sb.AppendLine("                    if (TableDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[\"" + fieldConfig.ForeignKey + "\"].Values[_dataList[i]." + fieldConfig.Name + ".ToString()] < 1)");
                sb.AppendLine("                    {");
                sb.AppendLine("                        GUI.color = new Color(1, 0.5f, 0);");
                sb.AppendLine("                    }");
                sb.AppendLine("                }");
                sb.AppendLine("                else");
                sb.AppendLine("                {");
                sb.AppendLine("                    GUI.color = new Color(1, 0.5f, 0);");
                sb.AppendLine("                }");
                sb.AppendLine("            }");
                sb.AppendLine("            else");
                sb.AppendLine("            {");
                sb.AppendLine("                GUI.color = new Color(1, 0.5f, 0);");
                sb.AppendLine("            }");
                sb.AppendLine("            columnsWidth = _excelConfig.ColumnsWidth[" + i + "];");
                sb.AppendLine("            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));");
                if (fieldConfig.FieldType == "List")
                {
                    sb.AppendLine("            _dataList[i]." + fieldConfig.Name + " = (" + fieldConfig.FieldType + "<" + fieldConfig.GenericType + ">)TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.Name + ");");
                }
                else
                {
                    sb.AppendLine("            _dataList[i]." + fieldConfig.Name + " = (" + fieldConfig.FieldType + ")TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.Name + ");");
                }
                sb.AppendLine("            GUILayout.EndHorizontal();");
                sb.AppendLine("            GUI.color = Color.white;");
                sb.AppendLine("");
            }
            else
            {
                sb.AppendLine("            columnsWidth = _excelConfig.ColumnsWidth[" + i + "];");
                sb.AppendLine("            GUILayout.BeginHorizontal(EditorGUIStyle.GetGroupBoxStyle(), GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));");
                if (fieldConfig.FieldType == "List")
                {
                    sb.AppendLine("            _dataList[i]." + fieldConfig.Name + " = (" + fieldConfig.FieldType + "<" + fieldConfig.GenericType + ">)TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.Name + ");");
                }
                else
                {
                    sb.AppendLine("            _dataList[i]." + fieldConfig.Name + " = (" + fieldConfig.FieldType + ")TableDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.Name + ");");
                }
                sb.AppendLine("            GUILayout.EndHorizontal();");
                sb.AppendLine();
            }
        }
    }
}

//if (!_foreignKeyDic["MyTest"].Contains(_tempData.ccc))
//     {
//         GUI.color = new Color(1, 0.5f, 0);
//     }
//     EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative("ccc"), new GUIContent("ccc"), true);
//     GUI.color = Color.white;

//     if (_primaryKeyDataDic.ContainsKey(_tempData.cccc))
//     {
//         if (_primaryKeyDataDic[_tempData.cccc] > 1)
//         {
//             GUI.color = Color.red;
//         }
//     }
//     EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative("cccc"), new GUIContent("玩家ID"), true);
//     GUI.color = Color.white;


//     EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative("enumTest"), new GUIContent("enumTest"), true);

//     EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative("ListTest"), new GUIContent("ListTest"), true);

//     GUILayout.BeginHorizontal();
//     EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative("sprit"), new GUIContent("sprit"), true);
//     if (_tempData.sprit != null)
//     {
//         GUILayout.Label(new GUIContent(image: _tempData.sprit.texture), GUILayout.Width(65), GUILayout.Height(64));
//     }
//     GUILayout.EndHorizontal();

//     GUILayout.BeginHorizontal();
//     EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative("text"), new GUIContent("text"), true);
//     if (_tempData.sprit != null)
//     {
//         GUILayout.Label(new GUIContent(image: _tempData.text), GUILayout.Width(65), GUILayout.Height(64));
//     }
//     GUILayout.EndHorizontal();

//     EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative("spritList"), new GUIContent("spritList"), true);