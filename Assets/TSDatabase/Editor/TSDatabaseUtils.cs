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

public enum LanguageEnum
{
    ChineseSimplified,
    English
}

public class LanguageUtils
{
    #region Common

    public static string CommonSaveSetting
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "保存配置";
                case LanguageEnum.English:
                default:
                    return "Save Config";
            }
        }
    }

    public static string CommonDelete
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "删除";
                case LanguageEnum.English:
                default:
                    return "Del";
            }
        }
    }

    public static string CommonSaveFailed
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "保存失败";
                case LanguageEnum.English:
                default:
                    return "Save Failed";
            }
        }
    }

    public static string CommonNullPath
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "路径为空！！！";
                case LanguageEnum.English:
                default:
                    return "path is null !!!";
            }
        }
    }

    public static string CommonSaveFile
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "保存文件";
                case LanguageEnum.English:
                default:
                    return "Save File";
            }
        }
    }

    public static string CommonSaveSuccess
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "保存成功";
                case LanguageEnum.English:
                default:
                    return "Success";
            }
        }
    }

    #endregion

    #region CreateTable

    public static string CreateTitle
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "创建表格";
                case LanguageEnum.English:
                default:
                    return "Create Table";
            }
        }
    }

    public static string CreateListHead
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "Table列表";
                case LanguageEnum.English:
                default:
                    return "TableList";
            }
        }
    }
    public static string CreateListAdd
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "添加";
                case LanguageEnum.English:
                default:
                    return "Add";
            }
        }
    }

    public static string CreateListDelete
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "删除";
                case LanguageEnum.English:
                default:
                    return "Delete";
            }
        }
    }


    public static string CreateListDeleteing
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "正在删除...";
                case LanguageEnum.English:
                default:
                    return "Deleting...";
            }
        }
    }

    public static string CreateListDeleted
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "删除完毕";
                case LanguageEnum.English:
                default:
                    return "Deleted";
            }
        }
    }

    public static string CreateInfoTitle
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "Table信息";
                case LanguageEnum.English:
                default:
                    return "TableInfo";
            }
        }
    }

    public static string CreateInfoAddField
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "添加字段";
                case LanguageEnum.English:
                default:
                    return "AddField";
            }
        }
    }

    public static string CreateInfoTableName
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "表名:";
                case LanguageEnum.English:
                default:
                    return "Table:";
            }
        }
    }
    public static string CreateInfoNotEnum
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "没有枚举";
                case LanguageEnum.English:
                default:
                    return "NotEnum";
            }
        }
    }

    public static string CreateInfoDescription
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "描述:";
                case LanguageEnum.English:
                default:
                    return "Desc:";
            }
        }
    }

    public static string CreateInfoPrimaryKey
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "主键:";
                case LanguageEnum.English:
                default:
                    return "PK:";
            }
        }
    }

    public static string CreateInfoItemType
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "元素类型:";
                case LanguageEnum.English:
                default:
                    return "ItemType:";
            }
        }
    }

    public static string CreateInfoEnumType
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "枚举类型:";
                case LanguageEnum.English:
                default:
                    return "EnumType:";
            }
        }
    }

    public static string CreateInfoExport
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "是否导出:";
                case LanguageEnum.English:
                default:
                    return "IsExport:";
            }
        }
    }

    public static string CreateInfoSelectFolder
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "选择文件夹";
                case LanguageEnum.English:
                default:
                    return "Select Folder";
            }
        }
    }
    public static string CreateInfoForeignKey
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "外键:";
                case LanguageEnum.English:
                default:
                    return "FK:";
            }
        }
    }
    public static string CreateInfoColumnName
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "列名:";
                case LanguageEnum.English:
                default:
                    return "Column:";
            }
        }
    }
    public static string CreateInfoType
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "类型:";
                case LanguageEnum.English:
                default:
                    return "Type:";
            }
        }
    }

    public static string CreateInfoGenerateScript
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "生成脚本";
                case LanguageEnum.English:
                default:
                    return "Generate Script";
            }
        }
    }

    public static string CreateInfoSaveAs
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "另存脚本";
                case LanguageEnum.English:
                default:
                    return "Save As";
            }
        }
    }

    public static string CreateInfoPrimaryKeyIsNull
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "主键为空!!!";
                case LanguageEnum.English:
                default:
                    return "primary key is null";
            }
        }
    }

    public static string CreateInfoCodeError
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "生成代码失败!!!";
                case LanguageEnum.English:
                default:
                    return "generate code failed";
            }
        }
    }

    #endregion

    #region Data

    public static string DataHead
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "数据";
                case LanguageEnum.English:
                default:
                    return "Data";
            }
        }
    }

    public static string DataSave
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "保存";
                case LanguageEnum.English:
                default:
                    return "Save";
            }
        }
    }

    public static string DataSaving
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "正在保存。。。";
                case LanguageEnum.English:
                default:
                    return "Saving...";
            }
        }
    }

    public static string DataPageSize
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "每页个数:";
                case LanguageEnum.English:
                default:
                    return "PageSize:";
            }
        }
    }
    public static string DataRecords
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "数据记录:";
                case LanguageEnum.English:
                default:
                    return "Records:";
            }
        }
    }

    #endregion

    #region Export

    public static string ExportImport
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "导入";
                case LanguageEnum.English:
                default:
                    return "Import";
            }
        }
    }

    public static string ExportTitle
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "导出数据";
                case LanguageEnum.English:
                default:
                    return "Export Data";
            }
        }
    }

    public static string ExportHead
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "导出";
                case LanguageEnum.English:
                default:
                    return "Export";
            }
        }
    }

    public static string ExportDataCount(int count)
    {
        string str = "";
        switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
        {
            case LanguageEnum.ChineseSimplified:
                str = "大约有:" + count + "条数据";
                break;
            case LanguageEnum.English:
            default:
                str = "Abourt:" + count + " data";
                break;
        }
        return str;
    }

    public static string ExportFile
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "导出文件";
                case LanguageEnum.English:
                default:
                    return "Export File";
            }
        }
    }

    public static string ExportNotFound
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "没有找到数据文件!!!";
                case LanguageEnum.English:
                default:
                    return "data file is not found!!!";
            }
        }
    }

    #endregion

    #region Setting

    public static string SettingTitle
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "TS 设置";
                case LanguageEnum.English:
                default:
                    return "TS Setting";
            }
        }
    }

    public static string SettingHead
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "设置";
                case LanguageEnum.English:
                default:
                    return "Setting";
            }
        }
    }

    public static string SettingVariable
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "变量分隔符:";
                case LanguageEnum.English:
                default:
                    return "Variable Separator:";
            }
        }
    }

    public static string SettingList
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "集合分隔符:";
                case LanguageEnum.English:
                default:
                    return "List Separator:";
            }
        }
    }


    public static string SettingDefaultColumnWidth
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "默认列宽:";
                case LanguageEnum.English:
                default:
                    return "Default Column Width:";
            }
        }
    }

    public static string SettingLanguage
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "语言:";
                case LanguageEnum.English:
                default:
                    return "Language:";
            }
        }
    }

    #endregion

    #region GenerateCode

    public static string GenerateFailed
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "生成失败";
                case LanguageEnum.English:
                default:
                    return "Generate Failed";
            }
        }
    }

    public static string GenerateFailedReasonFirst
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "变量名和类名重复!!!";
                case LanguageEnum.English:
                default:
                    return "Variable name and class name are repeated!!!";
            }
        }
    }
    public static string GenerateFailedReasonSecond
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "变量名为空!!!";
                case LanguageEnum.English:
                default:
                    return "The variable name is empty!!!";
            }
        }
    }
    public static string GenerateFailedReasonThird
    {
        get
        {
            switch (TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage)
            {
                case LanguageEnum.ChineseSimplified:
                    return "变量名重复!!!";
                case LanguageEnum.English:
                default:
                    return "The variable name is repeated!!!";
            }
        }
    }

    #endregion
}
