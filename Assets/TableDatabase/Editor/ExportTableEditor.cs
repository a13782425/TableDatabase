using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExportTableEditor : EditorWindow
{
    [MenuItem("Table/ExportTable", priority = 51)]
    static void CreateTable()
    {
        EditorWindow.GetWindowWithRect<ExportTableEditor>(new Rect(100, 100, 500, 500), false, "导出数据");
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
        SettingGUI();

        TableGUI();

    }

    private void TableGUI()
    {
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        GUILayout.BeginScrollView(_tableScrollVec, false, false);
        for (int i = 0; i < _exportDtoList.Count; i++)
        {
            ExportDto dto = _exportDtoList[i];
            GUILayout.BeginHorizontal("GroupBox");
            GUILayout.Label(dto.CurrentConfig.TableName);

            GUILayout.Label("大约有:" + dto.Count + "条数据");
            if (GUILayout.Button("导出", GUILayout.Width(50)))
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
            if (EditorUtility.DisplayDialog("保存失败", "没有找到数据文件!!!", "OK"))
            {
                return;
            }
        }
        string path = EditorUtility.SaveFilePanel("导出文件", "", dto.CurrentConfig.TableName + ".txt", "txt");
        if (string.IsNullOrEmpty(path))
        {
            if (EditorUtility.DisplayDialog("保存失败", "路径为空!!!", "OK"))
            {
                return;
            }
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < dto.CurrentConfig.FieldList.Count; i++)
        {
            if (dto.CurrentConfig.FieldList[i].IsExport)
            {
                sb.Append(dto.CurrentConfig.FieldList[i].Name);
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
                        object obj = o.GetType().GetField(dto.CurrentConfig.FieldList[i].Name).GetValue(o);

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
        EditorUtility.DisplayDialog("导出", "导出成功", "Ok");
    }

    private void GetString(string fieldType, object obj, ref string str)
    {
        switch (fieldType)
        {
            case "Vector2":
                Vector2 vector2 = (Vector2)obj;
                str += vector2.x + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + vector2.y;
                break;
            case "Vector3":
                Vector3 vector3 = (Vector3)obj;
                str += vector3.x + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + vector3.y + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + vector3.z;
                break;
            case "Vector4":
                Vector4 vector4 = (Vector4)obj;
                str += vector4.x + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + vector4.y + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + vector4.z + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + vector4.w;
                break;
            case "Quaternion":
                Quaternion quaternion = (Quaternion)obj;
                str += quaternion.x + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + quaternion.y + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + quaternion.z + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + quaternion.w;
                break;
            case "Rect":
                Rect rect = (Rect)obj;
                str += rect.x + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + rect.y + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + rect.width + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + rect.height;
                break;
            case "Color":
                Color color = (Color)obj;
                str += color.r + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + color.g + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + color.b + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + color.a;
                break;
            case "Color32":
                Color32 color32 = (Color32)obj;
                str += color32.r + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + color32.g + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + color32.b + TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar + color32.a;
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
                    str += TableDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar;
                }
                int length = str.LastIndexOf(TableDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar);
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

    private void SettingGUI()
    {
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal("OL Title");
        GUI.color = Color.white;
        GUILayout.Space(5);
        GUILayout.Label("Vector分割符:");
        TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar = GUILayout.TextField(TableDatabaseUtils.TableConfigSerializeData.Setting.SplitVecChar, GUILayout.Width(40));
        GUILayout.Space(5);
        GUILayout.Label("List分隔符:");
        TableDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar = GUILayout.TextField(TableDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar, GUILayout.Width(40));
        GUILayout.Space(5);
        if (GUILayout.Button("保存配置"))
        {
            TableDatabaseUtils.SavaGlobalData();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        _exportDtoList = new List<ExportDto>();
        for (int i = 0; i < TableDatabaseUtils.TableConfigSerializeData.TableConfigList.Count; i++)
        {
            TableConfig config = TableDatabaseUtils.TableConfigSerializeData.TableConfigList[i];
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
