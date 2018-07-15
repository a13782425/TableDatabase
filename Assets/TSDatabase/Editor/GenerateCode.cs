using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GenerateCode
{
    private TableConfig _currentConfig;

    private string _csPath;

    private string _templatePath;

    private Encoding encoding = new UTF8Encoding();

    public GenerateCode(TableConfig config)
    {
        if (config == null)
        {
            throw new Exception("表配置异常");
        }
        _currentConfig = config;
        string[] guids = AssetDatabase.FindAssets(typeof(GenerateCode).Name);
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
    public bool GenerateTable()
    {
        if (!GenerateDataCode())
            return false;
        GenerateConfig();

        GenerateEditor();
        return true;
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
        //if (!string.IsNullOrEmpty(_tempTableConfig.Description))
        //{
        //    codeSb.AppendLine("[Description(\"" + _tempTableConfig.Description + "\")]");
        //}
        codeSb.AppendLine("[System.Serializable]");
        codeSb.AppendLine("public class " + tableName);
        codeSb.AppendLine("{");
        List<string> fieldNameList = new List<string>();
        for (int i = 0; i < _currentConfig.FieldList.Count; i++)
        {
            FieldConfig fieldConfig = _currentConfig.FieldList[i];
            if (tableName == fieldConfig.FieldName)
            {
                if (EditorUtility.DisplayDialog("生成失败", "变量名和类名重复!!!", "OK"))
                {
                    return false;
                }
            }
            if (string.IsNullOrEmpty(fieldConfig.FieldName))
            {
                if (EditorUtility.DisplayDialog("生成失败", "变量名为空!!!", "OK"))
                {
                    return false;
                }
            }
            if (fieldNameList.Contains(fieldConfig.FieldName))
            {
                if (EditorUtility.DisplayDialog("生成失败", "变量名重复!!!", "OK"))
                {
                    return false;
                }
            }
            fieldNameList.Add(fieldConfig.FieldName);
            //if (!string.IsNullOrEmpty(fieldConfig.Description))
            //{
            //    codeSb.AppendLine("    [Description(\"" + fieldConfig.Description + "\")]");
            //}
            switch (fieldConfig.FieldType)
            {
                case "List":
                    switch (fieldConfig.GenericType)
                    {
                        case "enum":
                            codeSb.AppendLine("    public List<" + fieldConfig.EnumName + "> " + fieldConfig.FieldName + " = new List<" + fieldConfig.EnumName + ">();");
                            break;
                        default:
                            codeSb.AppendLine("    public List<" + fieldConfig.GenericType + "> " + fieldConfig.FieldName + " = new List<" + fieldConfig.GenericType + ">();");
                            break;
                    }

                    break;
                case "enum":
                    codeSb.AppendLine("    public " + fieldConfig.EnumName + " " + fieldConfig.FieldName + ";");
                    break;
                default:
                    codeSb.AppendLine("    public " + fieldConfig.FieldType + " " + fieldConfig.FieldName + ";");
                    break;
            }

        }
        codeSb.AppendLine("}");
        if (!File.Exists(Path.GetFullPath(_currentConfig.CodePath)))
        {
            File.Create(Path.GetFullPath(_currentConfig.CodePath)).Dispose();
        }
        File.WriteAllText(Path.GetFullPath(_currentConfig.CodePath), codeSb.ToString());
        return true;
    }

    private void GenerateConfig()
    {
        string directory = TSDatabaseUtils.EditorFullPath + "/Data/Table";
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
        sb.AppendLine("//------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("//-----------------------------------generate file " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "----------------------------------------");
        sb.AppendLine("//------------------------------------------------------------------------------------------------------------");
        sb.AppendLine("");
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
        templateText = templateText.Replace("$(GenerateTime)", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        templateText = templateText.Replace("$(TableDescription)", string.IsNullOrEmpty(_currentConfig.Description) ? _currentConfig.TableName : _currentConfig.Description);
        templateText = templateText.Replace("$(TableName)", _currentConfig.TableName);
        templateText = templateText.Replace("$(Primary)", _currentConfig.PrimaryKey);
        StringBuilder sb = new StringBuilder();
        GenerateSort(sb);
        templateText = templateText.Replace("$(SortColumn)", sb.ToString());
        sb = new StringBuilder();
        GenerateDataInfo(sb);
        templateText = templateText.Replace("$(ShowDataInfo)", sb.ToString());
        File.WriteAllText(_csPath, templateText, encoding);
    }

    private void GenerateSort(StringBuilder sb)
    {
        for (int i = 0; i < _currentConfig.FieldList.Count; i++)
        {
            if (_currentConfig.FieldList[i].FieldType == "List")
            {
                continue;
            }
            sb.AppendLine("                    case \"" + _currentConfig.FieldList[i].FieldName + "\":");
            sb.AppendLine("                        if (_sortFieldDic[fieldName] == -1)");
            sb.AppendLine("                        {");
            sb.AppendLine("                            _sortFieldDic[fieldName] = 1;");
            sb.AppendLine("                            _dataList = _dataList.OrderBy(a => a." + _currentConfig.FieldList[i].FieldName + ").ToList();");
            sb.AppendLine("                        }");
            sb.AppendLine("                        else");
            sb.AppendLine("                        {");
            sb.AppendLine("                            _sortFieldDic[fieldName] = -1;");
            sb.AppendLine("                            _dataList = _dataList.OrderByDescending(a => a." + _currentConfig.FieldList[i].FieldName + ").ToList();");
            sb.AppendLine("                        }");
            sb.AppendLine("                        break;");
        }
    }

    private void GenerateDataInfo(StringBuilder sb)
    {
        for (int i = 0; i < _currentConfig.FieldList.Count; i++)
        {
            FieldConfig fieldConfig = _currentConfig.FieldList[i];
            if (fieldConfig.FieldName == _currentConfig.PrimaryKey)
            {
                sb.AppendLine("            if (_primaryKeyInfo.Values.ContainsKey(_dataList[i]." + fieldConfig.FieldName + ".ToString()))");
                sb.AppendLine("            {");
                sb.AppendLine("                if (_primaryKeyInfo.Values[_dataList[i]." + fieldConfig.FieldName + ".ToString()] > 1)");
                sb.AppendLine("                {");
                sb.AppendLine("                    GUI.color = Color.red;");
                sb.AppendLine("                }");
                sb.AppendLine("            }");
                sb.AppendLine("            columnsWidth = _excelConfig.ColumnsWidth[" + i + "];");
                sb.AppendLine("            GUILayout.BeginHorizontal(EditorGUIStyle.GroupBoxStyle, GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));");
                sb.AppendLine("            " + _currentConfig.PrimaryType + " key = (" + fieldConfig.FieldType + ")TSDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.FieldName + ");");
                sb.AppendLine("            if (key != _dataList[i]." + fieldConfig.FieldName + ")");
                sb.AppendLine("            {");
                sb.AppendLine("                _primaryKeyInfo.Values[_dataList[i]." + fieldConfig.FieldName + ".ToString()]--;");
                sb.AppendLine("                if (_primaryKeyInfo.Values.ContainsKey(key.ToString()))");
                sb.AppendLine("                {");
                sb.AppendLine("                    _primaryKeyInfo.Values[key.ToString()]++;");
                sb.AppendLine("                }");
                sb.AppendLine("                else");
                sb.AppendLine("                {");
                sb.AppendLine("                    _primaryKeyInfo.Values.Add(key.ToString(), 1);");
                sb.AppendLine("                }");
                sb.AppendLine("                _dataList[i]." + fieldConfig.FieldName + " = key;");
                sb.AppendLine("            }");
                sb.AppendLine("            GUILayout.EndHorizontal();");
                sb.AppendLine("            GUI.color = Color.white;");
                sb.AppendLine();
            }
            else if (fieldConfig.HasForeignKey)
            {
                if (fieldConfig.FieldType == "List")
                {
                    sb.AppendLine("            columnsWidth = _excelConfig.ColumnsWidth[" + i + "];");
                    sb.AppendLine("            GUILayout.BeginHorizontal(EditorGUIStyle.GroupBoxStyle, GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));");
                    if (fieldConfig.GenericType == "enum")
                    {
                        sb.AppendLine("            _dataList[i]." + fieldConfig.FieldName + " = (" + fieldConfig.FieldType + "<" + fieldConfig.EnumName + ">)TSDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.FieldName + ",_tableConfig.FieldList[" + i + "].ForeignKey);");
                    }
                    else
                    {
                        sb.AppendLine("            _dataList[i]." + fieldConfig.FieldName + " = (" + fieldConfig.FieldType + "<" + fieldConfig.GenericType + ">)TSDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.FieldName + ",_tableConfig.FieldList[" + i + "].ForeignKey);");
                    }
                }
                else
                {
                    sb.AppendLine("            if (TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic.ContainsKey(\"" + fieldConfig.ForeignKey + "\"))");
                    sb.AppendLine("            {");
                    sb.AppendLine("                if (TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[\"" + fieldConfig.ForeignKey + "\"].Values.ContainsKey(_dataList[i]." + fieldConfig.FieldName + ".ToString()))");
                    sb.AppendLine("                {");
                    sb.AppendLine("                    if (TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[\"" + fieldConfig.ForeignKey + "\"].Values[_dataList[i]." + fieldConfig.FieldName + ".ToString()] < 1)");
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
                    sb.AppendLine("            GUILayout.BeginHorizontal(EditorGUIStyle.GroupBoxStyle, GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));");
                    sb.AppendLine("            _dataList[i]." + fieldConfig.FieldName + " = (" + fieldConfig.FieldType + ")TSDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.FieldName + ");");
                }
                sb.AppendLine("            GUILayout.EndHorizontal();");
                sb.AppendLine("            GUI.color = Color.white;");
                sb.AppendLine("");
            }
            else
            {
                sb.AppendLine("            columnsWidth = _excelConfig.ColumnsWidth[" + i + "];");
                sb.AppendLine("            GUILayout.BeginHorizontal(EditorGUIStyle.GroupBoxStyle, GUILayout.Width(columnsWidth), GUILayout.MaxWidth(columnsWidth), GUILayout.ExpandHeight(true));");
                if (fieldConfig.FieldType == "List")
                {
                    if (fieldConfig.GenericType == "enum")
                    {
                        sb.AppendLine("            _dataList[i]." + fieldConfig.FieldName + " = (" + fieldConfig.FieldType + "<" + fieldConfig.EnumName + ">)TSDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.FieldName + ");");
                    }
                    else
                    {
                        sb.AppendLine("            _dataList[i]." + fieldConfig.FieldName + " = (" + fieldConfig.FieldType + "<" + fieldConfig.GenericType + ">)TSDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.FieldName + ");");
                    }
                }
                else if (fieldConfig.FieldType == "enum")
                {
                    sb.AppendLine("            _dataList[i]." + fieldConfig.FieldName + " = (" + fieldConfig.EnumName + ")TSDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.FieldName + ");");
                }
                else
                {
                    sb.AppendLine("            _dataList[i]." + fieldConfig.FieldName + " = (" + fieldConfig.FieldType + ")TSDatabaseUtils.RenderFieldInfoControl(columnsWidth, _tableConfig.FieldList[" + i + "].FieldType, _dataList[i]." + fieldConfig.FieldName + ");");
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