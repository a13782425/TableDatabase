using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TableDatabaseUtils
{
    public static string[] BaseType = new string[] { "int", "float", "string", "bool", "enum", "Vector2", "Vector3", "Quaternion", "Sprite", "Texture", "GameObject", "List" };

    public static string[] GenericType = new string[] { "int", "float", "string", "bool", "Vector2", "Vector3", "Quaternion", "Sprite", "Texture", "GameObject" };

    private static string _editorPath;
    public static string EditorPath
    {
        get
        {
            if (string.IsNullOrEmpty(_editorPath))
            {
                GetTableConfigSerializeData();
            }
            return _editorPath;
        }
    }

    private static TableConfigSerializeData _tableConfigSerializeData = null;

    public static TableConfigSerializeData TableConfigSerializeData
    {
        get
        {
            if (_tableConfigSerializeData == null)
            {
                GetTableConfigSerializeData();
            }
            return _tableConfigSerializeData;
        }
    }

    public static string[] GetPrimaryKey(TableConfig config)
    {
        List<string> keys = new List<string>();
        keys.Add("");
        for (int i = 0; i < config.FieldList.Count; i++)
        {
            if (string.IsNullOrEmpty(config.FieldList[i].Name))
            {
                continue;
            }
            if (config.FieldList[i].FieldType == "int" || config.FieldList[i].FieldType == "string")
            {
                keys.Add(config.FieldList[i].Name);
            }
        }
        return keys.ToArray();
    }

    public static string[] GetDescription(TableConfig config)
    {
        List<string> keys = new List<string>();
        for (int i = 0; i < config.FieldList.Count; i++)
        {
            if (string.IsNullOrEmpty(config.FieldList[i].Name))
            {
                continue;
            }
            if (config.FieldList[i].FieldType != "List" && config.FieldList[i].FieldType != "Vector2" && config.FieldList[i].FieldType != "Vector3" && config.FieldList[i].FieldType != "Quaternion")
            {
                keys.Add(config.FieldList[i].Name);
            }
        }
        return keys.ToArray();
    }

    public static string[] GetForeignKey(TableConfig config, string typeName)
    {
        List<string> keys = new List<string>();
        keys.Add("");
        for (int i = 0; i < TableConfigSerializeData.TableConfigList.Count; i++)
        {
            string name = TableConfigSerializeData.TableConfigList[i].TableName;
            if (name != config.TableName && TableConfigSerializeData.TableConfigList[i].HasPrimaryKey && TableConfigSerializeData.TableConfigList[i].PrimaryType == typeName)
            {
                keys.Add(name);
            }

        }
        return keys.ToArray();
    }

    public static string[] GetEnums()
    {
        List<string> keys = new List<string>();
        foreach (Assembly item in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (item.GetName().Name == "Assembly-CSharp")
            {
                Type[] types = item.GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].IsEnum && (types[i].IsPublic || types[i].IsNestedPublic))
                    {
                        keys.Add(types[i].FullName.Replace('+', '.'));
                    }
                }
                break;
            }
        }
        return keys.ToArray();
    }

    public static void ShowIntField()
    {

    }


    private static void GetTableConfigSerializeData()
    {
        string[] guids = AssetDatabase.FindAssets(typeof(CreateTableEditor).Name);
        if (guids.Length != 1)
        {
            Debug.LogError("guids存在多个");
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        path = Path.GetDirectoryName(path);
        _editorPath = Path.GetFullPath(path);
        path = Path.Combine(path, "Config") + "/TableConfig.asset";
        if (!Directory.Exists(_editorPath + "/Config"))
        {
            Directory.CreateDirectory(_editorPath + "/Config");
        }
        if (!File.Exists(path))
        {
            _tableConfigSerializeData = ScriptableObject.CreateInstance<TableConfigSerializeData>();
            AssetDatabase.CreateAsset(_tableConfigSerializeData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            _tableConfigSerializeData = AssetDatabase.LoadAssetAtPath<TableConfigSerializeData>(path);
        }
    }


}
