using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            if (GUILayout.Button(LanguageUtils.ExportHead, GUILayout.Width(50)))
            {
                ExportData(dto);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
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
