using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

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
            int count = dto.CurrentConfig.FieldList.Count;
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
            items.GetType().GetMethod("Clear").Invoke(items, null);
            MethodInfo addMethod = items.GetType().GetMethod("Add");
            Assembly assembly = assetObj.GetType().Assembly;
            Type itemType = assembly.GetType(dto.CurrentConfig.TableName);
            if (itemType == null)
            {
                if (EditorUtility.DisplayDialog("Error", "Table type Error", "OK"))
                {
                    return;
                }
            }
            int begin = 1;
            for (; begin < allLine.Length; begin++)
            {
                string[] contentArray = allLine[begin].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (contentArray.Length > count)
                {
                    continue;
                }

                object o = assembly.CreateInstance(itemType.FullName);

                for (int i = 0; i < dto.CurrentConfig.FieldList.Count; i++)
                {
                    FieldConfig fieldConfig = dto.CurrentConfig.FieldList[i];
                    o.GetType().GetField(fieldConfig.FieldName).SetValue(o, GetObject(fieldConfig, contentArray[i], assembly));
                }
                addMethod.Invoke(items, new object[] { o });
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

    private object GetObject(FieldConfig fieldConfig, string str, Assembly assembly)
    {
        object o = null;
        string[] strs = null;
        float f = 0;
        switch (fieldConfig.FieldType)
        {
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
                            o = Enum.Parse(enumType, enumStrs[0]);
                        }
                        else
                        {
                            throw new Exception("枚举类型不存在！类型为：" + fieldConfig.EnumName);
                        }
                    }
                }
                break;
            default:
                break;
        }



        End: return o;
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
                str += vector2.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + vector2.y;
                break;
            case "Vector3":
                Vector3 vector3 = (Vector3)obj;
                str += vector3.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + vector3.y + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + vector3.z;
                break;
            case "Vector4":
                Vector4 vector4 = (Vector4)obj;
                str += vector4.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + vector4.y + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + vector4.z + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + vector4.w;
                break;
            case "Quaternion":
                Quaternion quaternion = (Quaternion)obj;
                str += quaternion.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + quaternion.y + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + quaternion.z + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + quaternion.w;
                break;
            case "Rect":
                Rect rect = (Rect)obj;
                str += rect.x + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + rect.y + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + rect.width + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + rect.height;
                break;
            case "Color":
                Color color = (Color)obj;
                str += color.r + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + color.g + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + color.b + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + color.a;
                break;
            case "Color32":
                Color32 color32 = (Color32)obj;
                str += color32.r + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + color32.g + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + color32.b + TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar + color32.a;
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
                    str += TSDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar;
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
