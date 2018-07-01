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
        string templateText = File.ReadAllText(_templatePath, encoding);
        templateText = templateText.Replace("$(TableShowName)", string.IsNullOrEmpty(_currentConfig.ShowName) ? _currentConfig.TableName : _currentConfig.ShowName);
        templateText = templateText.Replace("$(SerializeClassName)", _currentConfig.TableName + "SerializeData");
        templateText = templateText.Replace("$(DataClassName)", _currentConfig.TableName);
        templateText = templateText.Replace("$(Primary)", _currentConfig.PrimaryKey);
        templateText = templateText.Replace("$(PrimaryType)", _currentConfig.PrimaryType);
        if (_currentConfig.HasDescription)
        {
            if (!string.IsNullOrEmpty(_currentConfig.Description))
            {
                //object.Equals(data.PlayerId, null) ? "null" : data.PlayerId.ToString()
                templateText = templateText.Replace("$(SelectName)", "object.Equals(data." + _currentConfig.Description + ", null) ? \"null\" : data." + _currentConfig.Description + ".ToString()");
            }
            else
            {
                templateText = templateText.Replace("$(SelectName)", "\"选择\"");
            }
        }
        else
        {
            templateText = templateText.Replace("$(SelectName)", "\"选择\"");
        }
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
                sb.AppendLine("        if (_primaryKeyDataDic.ContainsKey(_tempData." + fieldConfig.Name + "))");
                sb.AppendLine("        {");
                sb.AppendLine("            if (_primaryKeyDataDic[_tempData." + fieldConfig.Name + "] > 1)");
                sb.AppendLine("            {");
                sb.AppendLine("                GUI.color = Color.red;");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }
            else if (fieldConfig.HasForeignKey)
            {
                sb.AppendLine("        if (!_foreignKeyDic[\"" + fieldConfig.ForeignKey + "\"].Contains(_tempData." + fieldConfig.Name + "))");
                sb.AppendLine("        {");
                sb.AppendLine("            GUI.color = new Color(1, 0.5f, 0);");
                sb.AppendLine("        }");

            }
            if (fieldConfig.FieldType == "Sprite")
            {
                sb.AppendLine("        GUILayout.BeginHorizontal();");
                sb.AppendLine("        EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative(\"" + fieldConfig.Name + "\"), new GUIContent(\"" + fieldName + "\"), true);");
                sb.AppendLine("        if (_tempData." + fieldConfig.Name + " != null)");
                sb.AppendLine("        {");
                sb.AppendLine("            GUILayout.Label(new GUIContent(image: _tempData." + fieldConfig.Name + ".texture), GUILayout.Width(65), GUILayout.Height(64));");
                sb.AppendLine("        }");
                sb.AppendLine("        GUILayout.EndHorizontal();");
            }
            else if (fieldConfig.FieldType == "Texture")
            {
                sb.AppendLine("        GUILayout.BeginHorizontal();");
                sb.AppendLine("        EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative(\"" + fieldConfig.Name + "\"), new GUIContent(\"" + fieldName + "\"), true);");
                sb.AppendLine("        if (_tempData." + fieldConfig.Name + " != null)");
                sb.AppendLine("        {");
                sb.AppendLine("            GUILayout.Label(new GUIContent(image: _tempData." + fieldConfig.Name + "), GUILayout.Width(65), GUILayout.Height(64));");
                sb.AppendLine("        }");
                sb.AppendLine("        GUILayout.EndHorizontal();");
            }
            else
            {
                sb.AppendLine("        EditorGUILayout.PropertyField(_tempSerializedProperty.FindPropertyRelative(\"" + fieldConfig.Name + "\"), new GUIContent(\"" + fieldName + "\"), true);");
            }
            sb.AppendLine("        GUI.color = Color.white;");
            sb.AppendLine("");
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