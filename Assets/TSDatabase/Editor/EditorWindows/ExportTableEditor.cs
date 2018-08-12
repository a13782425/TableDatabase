using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class ExportTableEditor : EditorWindow
{
    [MenuItem("TSTable/ExportTable", priority = 51)]
    static void CreateTable()
    {
        EditorWindow.GetWindowWithRect<ExportTableEditor>(new Rect(100, 100, 500, 500), false, LanguageUtils.ExportTitle);
    }

    class ExportDto
    {
        public int Count;
        public TableConfig CurrentConfig;
    }

    private List<ExportDto> _exportDtoList;

    private Vector2 _tableScrollVec;
    private void OnGUI()
    {
        HeadGUI();

        TableGUI();

    }

    private void HeadGUI()
    {
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal("OL Title");
        GUI.color = Color.white;
        GUILayout.Space(5);
        GUILayout.Label(LanguageUtils.ExportHead);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void TableGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        _tableScrollVec = GUILayout.BeginScrollView(_tableScrollVec, false, false);
        for (int i = 0; i < _exportDtoList.Count; i++)
        {
            ExportDto dto = _exportDtoList[i];
            GUILayout.BeginHorizontal("GroupBox");
            GUILayout.Label(dto.CurrentConfig.TableName, GUILayout.Width(180));

            GUILayout.Label(LanguageUtils.ExportDataCount(dto.Count));
            if (GUILayout.Button(LanguageUtils.ExportImport, GUILayout.Width(50)))
            {
                ImportData(dto);
            }
            if (GUILayout.Button(LanguageUtils.ExportHead, GUILayout.Width(50)))
            {
                ExportData(dto);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void ImportData(ExportDto dto)
    {
        string fileName = EditorUtility.OpenFilePanel("", "", "txt");
        if (!string.IsNullOrEmpty(fileName))
        {
            List<FieldConfig> importFieldConfigList = new List<FieldConfig>();
            for (int i = 0; i < dto.CurrentConfig.FieldList.Count; i++)
            {
                if (dto.CurrentConfig.FieldList[i].FieldName == dto.CurrentConfig.PrimaryKey)
                {
                    if (dto.CurrentConfig.FieldList[i].IsExport == false)
                    {
                        if (EditorUtility.DisplayDialog("Error", "PrimaryKey 没有导出", "OK"))
                        {
                            return;
                        }
                    }
                }
                if (dto.CurrentConfig.FieldList[i].IsExport)
                {
                    importFieldConfigList.Add(dto.CurrentConfig.FieldList[i]);
                }
            }
            int count = importFieldConfigList.Count;
            string[] allLine = File.ReadAllLines(fileName);
            if (allLine.Length < 1)
            {
                if (EditorUtility.DisplayDialog("Error", "File Error", "OK"))
                {
                    return;
                }
            }
            string[] fieldArray = allLine[0].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (fieldArray.Length != count)
            {
                Debug.LogError("fieldArray.Length");
                if (EditorUtility.DisplayDialog("Error", "File Error", "OK"))
                {
                    return;
                }
            }
            ScriptableObject assetObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(dto.CurrentConfig.DataPath);
            object items = assetObj.GetType().GetField("DataList").GetValue(assetObj);
            if (items == null)
            {
                if (EditorUtility.DisplayDialog("Error", "Unity data is null", "OK"))
                {
                    return;
                }
            }

            Assembly assembly = assetObj.GetType().Assembly;
            Type itemType = assembly.GetType(dto.CurrentConfig.TableName);
            object targetItems = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

            MethodInfo addMethod = items.GetType().GetMethod("Add");
            MethodInfo getMethod = items.GetType().GetMethod("get_Item");
            int orginCount = (int)items.GetType().GetProperty("Count").GetValue(items, null);

            if (itemType == null)
            {
                if (EditorUtility.DisplayDialog("Error", "Table type Error", "OK"))
                {
                    return;
                }
            }
            //清空所有主键重新键入
            if (TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic.ContainsKey(dto.CurrentConfig.TableName))
            {
                TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[dto.CurrentConfig.TableName].Values.Clear();
            }
            else
            {
                TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic.Add(dto.CurrentConfig.TableName, new PrimaryKeyInfo());
                TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[dto.CurrentConfig.TableName].TableName = dto.CurrentConfig.TableName;
                TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[dto.CurrentConfig.TableName].PrimaryKey = dto.CurrentConfig.PrimaryKey;
                TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[dto.CurrentConfig.TableName].PrimaryType = dto.CurrentConfig.PrimaryType;
                TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[dto.CurrentConfig.TableName].Values = new Dictionary<string, int>();
            }

            string keyName = dto.CurrentConfig.PrimaryKey;
            int keyIndex = 0;
            for (int i = 0; i < count; i++)
            {
                if (fieldArray[i] == dto.CurrentConfig.PrimaryKey)
                {
                    keyIndex = i;
                    break;
                }
            }
            EditorUtility.DisplayProgressBar("开始导入", "正在导入0/" + (allLine.Length - 1), 0f);
            try
            {
                int begin = 1;
                for (; begin < allLine.Length; begin++)
                {
                    string[] contentArray = allLine[begin].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (contentArray.Length > count)
                    {
                        continue;
                    }
                    object o = null;
                    object keyValue = null;
                    //找到主键ID 一样的数据
                    for (int j = 0; j < orginCount; j++)
                    {
                        o = getMethod.Invoke(items, new object[] { j });
                        keyValue = o.GetType().GetField(keyName).GetValue(o);
                        if (keyValue.ToString() == contentArray[keyIndex])
                        {
                            break;
                        }
                        else
                        {
                            o = null;
                        }
                    }
                    //如果原表中没有查询到，则创建新的
                    if (o == null)
                    {
                        o = assembly.CreateInstance(itemType.FullName);
                    }

                    for (int i = 0; i < importFieldConfigList.Count; i++)
                    {
                        FieldConfig fieldConfig = importFieldConfigList[i];
                        ///如果是Unity对象类型则不赋值
                        switch (fieldConfig.FieldType)
                        {
                            case "Sprite":
                            case "Texture":
                            case "GameObject":
                            case "Texture2D":
                                continue;
                            case "List":
                                switch (fieldConfig.GenericType)
                                {
                                    case "Sprite":
                                    case "Texture":
                                    case "GameObject":
                                    case "Texture2D":
                                        continue;
                                }
                                break;
                        }
                        o.GetType().GetField(fieldConfig.FieldName).SetValue(o, GetObject(fieldConfig, contentArray[i], assembly, fieldConfig.FieldType));
                        if (keyName == fieldConfig.FieldName)
                        {
                            keyValue = o.GetType().GetField(fieldConfig.FieldName).GetValue(o);
                        }
                    }

                    addMethod.Invoke(targetItems, new object[] { o });
                    if (TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[dto.CurrentConfig.TableName].Values.ContainsKey(keyValue.ToString()))
                    {
                        TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[dto.CurrentConfig.TableName].Values[keyValue.ToString()]++;
                    }
                    else
                    {
                        TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic[dto.CurrentConfig.TableName].Values.Add(keyValue.ToString(), 1);
                    }
                    EditorUtility.DisplayProgressBar("开始导入", "正在导入" + (begin - 1) + "/" + (allLine.Length - 1), (begin - 1) * 1f / (allLine.Length - 1));
                    System.Threading.Thread.Sleep(10);
                }
                assetObj.GetType().GetField("DataList").SetValue(assetObj, targetItems);
                EditorUtility.SetDirty(assetObj);
                TSDatabaseUtils.SavaGlobalData();
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("成功", "导入成功！", "OK");
            }
            catch (Exception ex)
            {
                EditorUtility.ClearProgressBar();
                throw ex;
            }
        }
        else
        {
            if (EditorUtility.DisplayDialog("Error", "Path is Error", "OK"))
            {
                return;
            }
        }
    }

    private object GetObject(FieldConfig fieldConfig, string str, Assembly assembly, string typeName)
    {
        object o = null;
        string[] strs = null;
        float f = 0;
        switch (typeName)
        {
            case "int":
                {
                    o = Convert.ToInt32(str);
                }
                break;
            case "float":
                {
                    o = Convert.ToSingle(str);
                }
                break;
            case "bool":
                {
                    o = Convert.ToBoolean(str);
                }
                break;
            case "Vector2":
                {
                    strs = str.Split(new char[] { TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length != 2)
                    {
                        o = Vector2.zero;
                        goto End;
                    }
                    Vector2 vector2 = new Vector2();
                    if (float.TryParse(strs[0], out f))
                    {
                        vector2.x = f;
                    }
                    if (float.TryParse(strs[1], out f))
                    {
                        vector2.y = f;
                    }
                    o = vector2;
                }
                break;
            case "Vector3":
                {
                    strs = str.Split(new char[] { TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length != 3)
                    {
                        o = Vector3.zero;
                        goto End;
                    }
                    Vector3 vector3 = new Vector3();
                    if (float.TryParse(strs[0], out f))
                    {
                        vector3.x = f;
                    }
                    if (float.TryParse(strs[1], out f))
                    {
                        vector3.y = f;
                    }
                    if (float.TryParse(strs[2], out f))
                    {
                        vector3.z = f;
                    }
                    o = vector3;
                }
                break;
            case "Vector4":
                {
                    strs = str.Split(new char[] { TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length != 4)
                    {
                        o = Vector4.zero;
                        goto End;
                    }
                    Vector4 vector4 = new Vector4();
                    if (float.TryParse(strs[0], out f))
                    {
                        vector4.x = f;
                    }
                    if (float.TryParse(strs[1], out f))
                    {
                        vector4.y = f;
                    }
                    if (float.TryParse(strs[2], out f))
                    {
                        vector4.z = f;
                    }
                    if (float.TryParse(strs[3], out f))
                    {
                        vector4.w = f;
                    }
                    o = vector4;
                }
                break;
            case "Quaternion":
                {
                    strs = str.Split(new char[] { TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length != 4)
                    {
                        o = Quaternion.identity;
                        goto End;
                    }
                    Quaternion quaternion = new Quaternion();
                    if (float.TryParse(strs[0], out f))
                    {
                        quaternion.x = f;
                    }
                    if (float.TryParse(strs[1], out f))
                    {
                        quaternion.y = f;
                    }
                    if (float.TryParse(strs[2], out f))
                    {
                        quaternion.z = f;
                    }
                    if (float.TryParse(strs[3], out f))
                    {
                        quaternion.w = f;
                    }
                    o = quaternion;
                }
                break;
            case "Rect":
                {
                    strs = str.Split(new char[] { TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length != 4)
                    {
                        o = new Rect();
                        goto End;
                    }
                    Rect rect = new Rect();
                    if (float.TryParse(strs[0], out f))
                    {
                        rect.x = f;
                    }
                    if (float.TryParse(strs[1], out f))
                    {
                        rect.y = f;
                    }
                    if (float.TryParse(strs[2], out f))
                    {
                        rect.width = f;
                    }
                    if (float.TryParse(strs[3], out f))
                    {
                        rect.height = f;
                    }
                    o = rect;
                }
                break;
            case "Color":
                {
                    strs = str.Split(new char[] { TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length < 3)
                    {
                        o = Color.white;
                        goto End;
                    }
                    Color color = new Color();
                    if (float.TryParse(strs[0], out f))
                    {
                        color.r = f;
                    }
                    if (float.TryParse(strs[1], out f))
                    {
                        color.g = f;
                    }
                    if (float.TryParse(strs[2], out f))
                    {
                        color.b = f;
                    }
                    if (strs.Length == 3)
                    {
                        color.a = 1f;
                    }
                    else
                    {
                        if (float.TryParse(strs[3], out f))
                        {
                            color.a = f;
                        }
                    }
                    o = color;
                }
                break;
            case "Color32":
                {
                    strs = str.Split(new char[] { TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar }, StringSplitOptions.RemoveEmptyEntries);
                    if (strs.Length < 3)
                    {
                        o = new Color32(255, 255, 255, 255);
                        goto End;
                    }
                    Color32 color32 = new Color32(255, 255, 255, 255);
                    byte color32Temp = 0;
                    if (byte.TryParse(strs[0], out color32Temp))
                    {
                        color32.r = color32Temp;
                    }
                    if (byte.TryParse(strs[1], out color32Temp))
                    {
                        color32.g = color32Temp;
                    }
                    if (byte.TryParse(strs[2], out color32Temp))
                    {
                        color32.b = color32Temp;
                    }
                    if (strs.Length == 3)
                    {
                        color32.a = 255;
                    }
                    else
                    {
                        if (byte.TryParse(strs[3], out color32Temp))
                        {
                            color32.a = color32Temp;
                        }
                    }
                    o = color32;
                }
                break;
            case "Sprite":
            case "Texture":
            case "GameObject":
            case "Texture2D":
                break;
            case "List":
                {
                    strs = str.Split(new char[] { TSDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar }, StringSplitOptions.RemoveEmptyEntries);
                    Type genericType = GetType(fieldConfig.GenericType, fieldConfig.EnumName, assembly);
                    o = Activator.CreateInstance(typeof(List<>).MakeGenericType(genericType));
                    MethodInfo addMethod = o.GetType().GetMethod("Add");

                    for (int i = 0; i < strs.Length; i++)
                    {
                        addMethod.Invoke(o, new object[] { GetObject(fieldConfig, strs[i], assembly, fieldConfig.GenericType) });
                    }
                }
                break;
            case "enum":
                {
                    Type enumType = assembly.GetType(fieldConfig.EnumName);
                    if (enumType == null)
                    {
                        throw new Exception("枚举类型不存在！类型为：" + fieldConfig.EnumName);
                    }
                    int enumTemp = 0;
                    if (int.TryParse(str, out enumTemp))
                    {
                        o = Enum.ToObject(enumType, enumTemp);
                    }
                    else
                    {
                        string[] enumStrs = Enum.GetNames(enumType);
                        if (enumStrs.Length > 0)
                        {
                            if (enumStrs.Contains(str))
                            {
                                o = Enum.Parse(enumType, str);
                            }
                            else
                            {
                                o = Enum.Parse(enumType, enumStrs[0]);
                            }
                        }
                        else
                        {
                            throw new Exception("枚举类型不存在！类型为：" + fieldConfig.EnumName);
                        }
                    }
                }
                break;
            case "string":
                if (string.IsNullOrEmpty(str))
                {
                    o = "";
                    goto End;
                }
                o = str;
                break;
            default:
                break;
        }



        End: return o;
    }

    private Type GetType(string genericType, string enumType, Assembly assembly)
    {
        switch (genericType)
        {
            case "int":
                return typeof(int);
            case "float":
                return typeof(float);
            case "string":
                return typeof(string);
            case "bool":
                return typeof(bool);
            case "enum":
                return assembly.GetType(enumType);
            case "Vector2":
                return typeof(Vector2);
            case "Vector3":
                return typeof(Vector3);
            case "Vector4":
                return typeof(Vector4);
            case "Quaternion":
                return typeof(Quaternion);
            case "Color":
                return typeof(Color);
            case "Color32":
                return typeof(Color32);
            default:
                return null;
        }
    }

    private void ExportData(ExportDto dto)
    {
        if (string.IsNullOrEmpty(dto.CurrentConfig.DataPath))
        {
            if (EditorUtility.DisplayDialog(LanguageUtils.CommonSaveFailed, LanguageUtils.ExportNotFound, "OK"))
            {
                return;
            }
        }
        string path = EditorUtility.SaveFilePanel(LanguageUtils.ExportFile, "", dto.CurrentConfig.TableName + ".txt", "txt");
        if (string.IsNullOrEmpty(path))
        {
            if (EditorUtility.DisplayDialog(LanguageUtils.CommonSaveFailed, LanguageUtils.CommonNullPath, "OK"))
            {
                return;
            }
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < dto.CurrentConfig.FieldList.Count; i++)
        {
            if (dto.CurrentConfig.FieldList[i].IsExport)
            {
                sb.Append(dto.CurrentConfig.FieldList[i].FieldName);
            }
            sb.Append("\t");
        }
        sb.Append("\r\n");
        ScriptableObject assetObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(dto.CurrentConfig.DataPath);
        object items = assetObj.GetType().GetField("DataList").GetValue(assetObj);
        if (items != null)
        {
            int count = (int)items.GetType().GetProperty("Count").GetValue(items, null);
            System.Reflection.PropertyInfo property = items.GetType().GetProperty("Item");
            for (int j = 0; j < count; j++)
            {
                object o = property.GetValue(items, new object[] { j });
                for (int i = 0; i < dto.CurrentConfig.FieldList.Count; i++)
                {
                    if (dto.CurrentConfig.FieldList[i].IsExport)
                    {
                        string str = "";
                        object obj = o.GetType().GetField(dto.CurrentConfig.FieldList[i].FieldName).GetValue(o);

                        GetString(dto.CurrentConfig.FieldList[i].FieldType, obj, ref str);


                        sb.Append(str);
                    }
                    sb.Append("\t");
                }
                sb.Append("\r\n");
            }
        }
        if (!File.Exists(path))
        {
            File.Create(path).Dispose();
        }
        File.WriteAllText(path, sb.ToString());
        EditorUtility.DisplayDialog(LanguageUtils.ExportHead, "Success", "Ok");
    }

    private void GetString(string fieldType, object obj, ref string str)
    {
        switch (fieldType)
        {
            case "Vector2":
                Vector2 vector2 = (Vector2)obj;
                str += vector2.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + vector2.y;
                break;
            case "Vector3":
                Vector3 vector3 = (Vector3)obj;
                str += vector3.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + vector3.y + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + vector3.z;
                break;
            case "Vector4":
                Vector4 vector4 = (Vector4)obj;
                str += vector4.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + vector4.y + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + vector4.z + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + vector4.w;
                break;
            case "Quaternion":
                Quaternion quaternion = (Quaternion)obj;
                str += quaternion.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + quaternion.y + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + quaternion.z + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + quaternion.w;
                break;
            case "Rect":
                Rect rect = (Rect)obj;
                str += rect.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + rect.y + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + rect.width + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + rect.height;
                break;
            case "Color":
                Color color = (Color)obj;
                str += color.r + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + color.g + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + color.b + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + color.a;
                break;
            case "Color32":
                Color32 color32 = (Color32)obj;
                str += color32.r + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + color32.g + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + color32.b + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar.ToString() + color32.a;
                break;
            case "Sprite":
            case "Texture":
            case "GameObject":
            case "Texture2D":
                UnityEngine.Object unityObject = (UnityEngine.Object)obj;
                if (unityObject == null)
                {
                    str += "";
                }
                else
                {
                    str += unityObject.name;
                }
                break;
            case "List":
                IEnumerable enumerator = (IEnumerable)obj;
                foreach (object item in enumerator)
                {
                    GetString(item.GetType().Name, item, ref str);
                    str += TSDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar.ToString();
                }
                int length = str.LastIndexOf(TSDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar);
                if (length > 0)
                {
                    str = str.Substring(0, length);
                }
                break;
            case "enum":
                str += (int)obj;
                break;
            default:
                str += obj.ToString();
                break;
        }
    }



    private void OnEnable()
    {
        _exportDtoList = new List<ExportDto>();
        for (int i = 0; i < TSDatabaseUtils.TableConfigSerializeData.TableConfigList.Count; i++)
        {
            TableConfig config = TSDatabaseUtils.TableConfigSerializeData.TableConfigList[i];
            ExportDto dto = new ExportDto();
            dto.CurrentConfig = config;
            dto.Count = 0;
            if (!string.IsNullOrEmpty(config.DataPath))
            {
                ScriptableObject assetObj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(config.DataPath);
                object items = assetObj.GetType().GetField("DataList").GetValue(assetObj);
                if (items != null)
                {
                    dto.Count = (int)items.GetType().GetProperty("Count").GetValue(items, null);
                }
            }
            _exportDtoList.Add(dto);
        }
    }
}
