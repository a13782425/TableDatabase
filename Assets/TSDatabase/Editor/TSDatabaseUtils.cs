using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TSDatabaseUtils
{
    public static string[] BaseType = new string[] { "int", "float", "string", "bool", "enum", "Vector2", "Vector3", "Rect", "Vector4", "Quaternion", "Color", "Color32", "Sprite", "Texture", "GameObject", "List" };

    public static string[] GenericType = new string[] { "int", "float", "string", "bool", "enum", "Vector2", "Vector3", "Vector4", "Quaternion", "Color", "Color32", "Sprite", "Texture", "GameObject" };

    private static string _editorPullPath;

    public static string EditorFullPath
    {
        get
        {
            if (string.IsNullOrEmpty(_editorPullPath))
            {
                GetGlobalSerializeData();
            }
            return _editorPullPath;
        }
    }

    private static string _editorPath;
    public static string EditorPath
    {
        get
        {
            if (string.IsNullOrEmpty(_editorPath))
            {
                GetGlobalSerializeData();
            }
            return _editorPath;
        }
    }
    private static string _tableConfigPath = "";
    private static TableConfigSerializeData _tableConfigSerializeData = null;

    public static TableConfigSerializeData TableConfigSerializeData
    {
        get
        {
            if (_tableConfigSerializeData == null)
            {
                GetGlobalSerializeData();
            }
            return _tableConfigSerializeData;
        }
    }

    private static string _primaryKeyPath = "";

    private static PrimaryKeySerializeData _primaryKeySerializeData = null;

    public static PrimaryKeySerializeData PrimaryKeySerializeData
    {
        get
        {
            if (_primaryKeySerializeData == null)
            {
                GetGlobalSerializeData();
            }
            return _primaryKeySerializeData;
        }
    }

    public static string[] GetPrimaryKey(TableConfig config)
    {
        List<string> keys = new List<string>();
        keys.Add("");
        for (int i = 0; i < config.FieldList.Count; i++)
        {
            if (string.IsNullOrEmpty(config.FieldList[i].FieldName))
            {
                continue;
            }
            if (config.FieldList[i].FieldType == "int" || config.FieldList[i].FieldType == "string")
            {
                keys.Add(config.FieldList[i].FieldName);
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


    public static object RenderFieldInfoControl(float width, string fieldType, object value, string foreignKey = "")
    {
        object result = null;
        switch (fieldType)
        {
            case "int":
            case "Int32":
                result = EditorGUILayout.IntField((int)value, GUILayout.MaxWidth(width - 10));
                break;
            case "String":
            case "string":
                result = EditorGUILayout.TextArea(value == null ? "" : value.ToString(), GUILayout.MaxWidth(width - 10));
                break;
            case "Boolean":
            case "bool":
                result = EditorGUILayout.Toggle((bool)value, GUILayout.MaxWidth(width - 10));
                break;
            case "Single":
            case "float":
                result = EditorGUILayout.FloatField((float)value, GUILayout.MaxWidth(width - 10));
                break;
            case "Rect":
                result = EditorGUILayout.RectField((Rect)value, GUILayout.MaxWidth(width - 10));
                break;
            case "Vector2":
                result = EditorGUILayout.Vector3Field("", (Vector2)value, GUILayout.MaxWidth(width - 10));
                break;
            case "Vector3":
                result = EditorGUILayout.Vector3Field("", (Vector3)value, GUILayout.MaxWidth(width - 10));
                break;
            case "Quaternion":
                Quaternion quaternion = (Quaternion)value;
                Vector4 vector4 = new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                vector4 = EditorGUILayout.Vector4Field("", vector4, GUILayout.MaxWidth(width - 10));
                result = new Quaternion(vector4.x, vector4.y, vector4.z, vector4.w);
                break;
            case "Color":
                result = EditorGUILayout.ColorField((Color)value, GUILayout.MaxWidth(width - 10));
                break;
            case "Color32":
                result = (Color32)EditorGUILayout.ColorField((Color32)value, GUILayout.MaxWidth(width - 10));
                break;
            case "Sprite":
                result = EditorGUILayout.ObjectField((Sprite)value, typeof(Sprite), false, GUILayout.MaxWidth(width - 10));
                if (((Sprite)value) != null)
                {
                    GUILayout.Label(new GUIContent(image: ((Sprite)value).texture), GUILayout.Width(32), GUILayout.Height(32));
                }
                break;
            case "Texture":
                result = EditorGUILayout.ObjectField((Texture)value, typeof(Texture), false, GUILayout.MaxWidth(width - 10));
                if (((Texture)value) != null)
                {
                    GUILayout.Label(new GUIContent(image: ((Texture)value)), GUILayout.Width(32), GUILayout.Height(32));
                }
                break;
            case "Texture2D":
                result = EditorGUILayout.ObjectField((Texture2D)value, typeof(Texture2D), false, GUILayout.MaxWidth(width - 10));
                if (((Texture2D)value) != null)
                {
                    GUILayout.Label(new GUIContent(image: ((Texture2D)value)), GUILayout.Width(32), GUILayout.Height(32));
                }
                break;
            case "GameObject":
                result = EditorGUILayout.ObjectField((GameObject)value, typeof(GameObject), false, GUILayout.MaxWidth(width - 10));
                break;

            case "enum":
                result = EditorGUILayout.EnumPopup((Enum)value, GUILayout.MaxWidth(width - 10));
                break;
            case "List":
                GUILayout.BeginVertical(EditorGUIStyle.ListBoxStyle, GUILayout.Width(width), GUILayout.MaxWidth(width), GUILayout.ExpandHeight(true));
                Type listType = value.GetType();
                Type elementType = listType.GetGenericArguments()[0];
                int count = (int)listType.GetProperty("Count").GetValue(value, null);
                PropertyInfo itemPropertyInfo = value.GetType().GetProperty("Item");
                int removeAt = -1;
                for (int i = 0; i < count; i++)
                {
                    object item = itemPropertyInfo.GetValue(value, new object[] { i });
                    if (!string.IsNullOrEmpty(foreignKey))
                    {
                        if (TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic.ContainsKey(foreignKey))
                        {
                            if (TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[foreignKey].Values.ContainsKey(item.ToString()))
                            {
                                if (TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[foreignKey].Values[item.ToString()] < 1)
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
                    }
                    GUILayout.BeginHorizontal();
                    if (elementType.IsEnum)
                    {
                        item = RenderFieldInfoControl(width - 20, "enum", item);
                    }
                    else
                    {
                        item = RenderFieldInfoControl(width - 20, elementType.Name, item);
                    }
                    GUI.color = Color.white;
                    itemPropertyInfo.SetValue(value, item, new object[] { i });
                    if (GUILayout.Button("", "OL Minus"))
                    {
                        removeAt = i;
                    }
                    GUILayout.EndHorizontal();
                }
                if (removeAt >= 0)
                {
                    listType.GetMethod("RemoveAt").Invoke(value, new object[] { removeAt });
                }
                if (GUILayout.Button("Add"))
                {
                    object o = elementType.IsValueType ? Activator.CreateInstance(elementType) : null;
                    value.GetType().GetMethod("Add").Invoke(value, new object[] { o });
                }
                result = value;
                GUILayout.EndVertical();
                break;
            default:
                EditorGUILayout.LabelField(value == null ? "" : value.ToString(), GUILayout.MaxWidth(width - 10));
                result = null;
                break;
        }
        //GUILayout.EndHorizontal();

        return result;
    }


    public static void SavaGlobalData()
    {
        byte[] bytes = SerializeToBinary(PrimaryKeySerializeData);
        File.WriteAllBytes(_primaryKeyPath, bytes);
        EditorUtility.SetDirty(TableConfigSerializeData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// 获取全局
    /// </summary>
    private static void GetGlobalSerializeData()
    {
        string[] guids = AssetDatabase.FindAssets(typeof(TSDatabaseUtils).Name);
        if (guids.Length != 1)
        {
            Debug.LogError("guids存在多个");
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        path = Path.GetDirectoryName(path);
        _editorPath = path;
        _editorPullPath = Path.GetFullPath(path);
        _tableConfigPath = Path.Combine(_editorPath, "Config/Global") + "/TableConfig.asset";
        _primaryKeyPath = Path.Combine(_editorPullPath, "Config/Global") + "/PrimaryKey";
        if (!Directory.Exists(_editorPullPath + "/Config/Global"))
        {
            Directory.CreateDirectory(_editorPullPath + "/Config/Global");
        }
        if (!File.Exists(_tableConfigPath))
        {
            _tableConfigSerializeData = ScriptableObject.CreateInstance<TableConfigSerializeData>();
            AssetDatabase.CreateAsset(_tableConfigSerializeData, _tableConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            _tableConfigSerializeData = AssetDatabase.LoadAssetAtPath<TableConfigSerializeData>(_tableConfigPath);
        }
        if (!File.Exists(_primaryKeyPath))
        {
            File.Create(Path.GetFullPath(_primaryKeyPath)).Dispose();
            _primaryKeySerializeData = new PrimaryKeySerializeData();
        }
        else
        {
            _primaryKeySerializeData = (PrimaryKeySerializeData)DeserializeWithBinary(File.ReadAllBytes(Path.GetFullPath(_primaryKeyPath)));
        }

    }

    /// <summary>
    /// 将对象序列化为二进制数据 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static byte[] SerializeToBinary(object obj)
    {
        MemoryStream stream = new MemoryStream();
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        bf.Serialize(stream, obj);

        byte[] data = stream.ToArray();
        stream.Close();

        return data;
    }

    /// <summary>
    /// 将二进制数据反序列化
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static object DeserializeWithBinary(byte[] data)
    {
        MemoryStream stream = new MemoryStream();
        stream.Write(data, 0, data.Length);
        stream.Position = 0;
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        object obj = bf.Deserialize(stream);

        stream.Close();

        return obj;
    }
}


public class LanguageUtils
{
    public static string Save
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    return "保存";
                case SystemLanguage.ChineseTraditional:
                    return "保存";
                case SystemLanguage.English:
                default:
                    return "Save";
            }
        }
    }

    public static string Data
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    return "数据";
                case SystemLanguage.ChineseTraditional:
                    return "數據";
                case SystemLanguage.English:
                default:
                    return " Data";
            }
        }
    }

    public static string SaveFail
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    return "保存失败";
                case SystemLanguage.ChineseTraditional:
                    return "保存失敗";
                case SystemLanguage.English:
                default:
                    return "Save Fail";
            }
        }
    }

    public static string SaveFile
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    return "保存文件";
                case SystemLanguage.ChineseTraditional:
                    return "保存文件";
                case SystemLanguage.English:
                default:
                    return "Save File";
            }
        }
    }

    public static string DataNullPath
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    return "数据路径为空！！！";
                case SystemLanguage.ChineseTraditional:
                    return "數據路徑為空！！！";
                case SystemLanguage.English:
                default:
                    return "data path is null !!!";
            }
        }
    }
    public static string DataList
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    return "数据列表";
                case SystemLanguage.ChineseTraditional:
                    return "數據列表";
                case SystemLanguage.English:
                default:
                    return "DataList";
            }
        }
    }
    public static string Count
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case SystemLanguage.ChineseSimplified:
                    return "条";
                case SystemLanguage.ChineseTraditional:
                    return "條";
                case SystemLanguage.English:
                default:
                    return "Count";
            }
        }
    }

}
