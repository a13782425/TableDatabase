using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

public class CreateTableEditor : EditorWindow
{

    [MenuItem("TSTable/CreateTable", priority = 50)]
    static void CreateTable()
    {
        EditorWindow.GetWindowWithRect<CreateTableEditor>(new Rect(100, 100, 800, 500), false, LanguageUtils.CreateTitle);
    }

    private ReorderableList _typeList;

    private ReorderableList _typeInfoList;

    private static TableConfig _tempTableConfig;

    private string[] _enumArray;

    private const int INFO_LABEL_WIDTH = 50;

    private const int INFO_INPUT_WIDTH = 70;

    private bool _isGenerateCode = false;

    private int _selectConfigIndex = -1;

    private Vector2 _listScollViewVec;
    private Vector2 _infoScollViewVec;

    private void OnGUI()
    {
        if (_isGenerateCode)
        {
            return;
        }
        GUILayout.BeginHorizontal("sv_iconselector_back");
        GUILayout.BeginVertical();
        ShowTypeList();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        ShowTypeInfo();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 显示类型列表
    /// Show all table lists
    /// </summary>
    private void ShowTypeList()
    {
        GUI.SetNextControlName("DataList");
        GUILayout.BeginArea(new Rect(10, 10, 200, 480), "", "box");
        GUILayout.Space(5);
        _listScollViewVec = GUILayout.BeginScrollView(_listScollViewVec, false, true);

        if (_typeList != null)
        {
            _typeList.DoLayoutList();
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    /// <summary>
    /// 显示类型具体信息
    /// Display the details of the form
    /// </summary>
    private void ShowTypeInfo()
    {
        if (_tempTableConfig != null)
        {
            GUILayout.BeginArea(new Rect(220, 10, 570, 480), "", "box");
            GUI.color = new Color(0.8f, 0.8f, 0.8f); ;
            GUILayout.BeginHorizontal("OL Title");
            GUI.color = Color.white;
            GUILayout.Label(LanguageUtils.CreateInfoTitle);
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            {
                GUILayout.BeginArea(new Rect(10, 30, 550, 440), "", "box");
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(LanguageUtils.CreateInfoTableName, GUILayout.Width(INFO_LABEL_WIDTH));
                    _tempTableConfig.TableName = EditorGUILayout.TextField(_tempTableConfig.TableName, GUILayout.Width(INFO_INPUT_WIDTH));
                    GUILayout.Label(LanguageUtils.CreateInfoDescription, GUILayout.Width(INFO_LABEL_WIDTH));
                    _tempTableConfig.Description = EditorGUILayout.TextField(_tempTableConfig.Description, GUILayout.Width(INFO_INPUT_WIDTH));
                    GUILayout.Label(LanguageUtils.CreateInfoPrimaryKey, GUILayout.Width(INFO_LABEL_WIDTH));
                    string[] keys = TSDatabaseUtils.GetPrimaryKey(_tempTableConfig);
                    if (keys.Length == 1)
                    {
                        EditorGUILayout.Popup(0, new string[0], GUILayout.Width(INFO_INPUT_WIDTH));
                    }
                    else
                    {
                        _tempTableConfig.PrimaryIndex = EditorGUILayout.Popup(_tempTableConfig.PrimaryIndex, keys, GUILayout.Width(INFO_INPUT_WIDTH));

                        _tempTableConfig.PrimaryKey = keys[_tempTableConfig.PrimaryIndex];
                        for (int i = 0; i < _tempTableConfig.FieldList.Count; i++)
                        {
                            if (_tempTableConfig.FieldList[i].FieldName == _tempTableConfig.PrimaryKey)
                            {
                                _tempTableConfig.PrimaryType = _tempTableConfig.FieldList[i].FieldType;
                                break;
                            }
                        }
                    }
                    //}
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(LanguageUtils.CommonSaveSetting))
                    {
                        WriteConfig();
                    }
                    GUILayout.Space(10);
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(5);
                ShowConfigInfo();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(LanguageUtils.CreateInfoGenerateScript))
                    {
                        GenerateCode();
                    }
                    if (GUILayout.Button(LanguageUtils.CreateInfoSaveAs))
                    {
                        _tempTableConfig.CodePath = "";
                        _tempTableConfig.DataPath = "";
                        GenerateCode();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();
                GUILayout.EndVertical();
            }

            GUILayout.EndArea();

        }
    }

    /// <summary>
    /// 显示表格中的字段详情
    /// Show field details in the table
    /// </summary>
    private void ShowConfigInfo()
    {
        _infoScollViewVec = GUILayout.BeginScrollView(_infoScollViewVec, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
        if (_typeInfoList != null)
        {
            _typeInfoList.DoLayoutList();
        }
        GUILayout.EndScrollView();
    }

    private void RemoveData(int removeIndex)
    {
        EditorUtility.DisplayProgressBar(LanguageUtils.CreateListDelete, LanguageUtils.CreateListDeleteing, 0f);
        EditorApplication.LockReloadAssemblies();
        TableConfig config = TSDatabaseUtils.TableConfigSerializeData.TableConfigList[removeIndex];
        TSDatabaseUtils.TableConfigSerializeData.TableConfigList.RemoveAt(removeIndex);
        if (!string.IsNullOrEmpty(config.DataPath))
        {
            File.Delete(Path.GetFullPath(config.DataPath));
            if (TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic.ContainsKey(config.TableName))
            {
                TSDatabaseUtils.PrimaryKeySerializeData.PrimaryKeyDic.Remove(config.TableName);
            }
        }
        EditorUtility.DisplayProgressBar(LanguageUtils.CreateListDelete, LanguageUtils.CreateListDeleteing, 0.5f);
        if (!string.IsNullOrEmpty(config.CodePath))
        {
            File.Delete(Path.GetFullPath(config.CodePath));

            string editorPath = TSDatabaseUtils.EditorFullPath + "/TableEditor/" + config.TableName + "EditorData.cs";
            if (File.Exists(editorPath))
            {
                File.Delete(editorPath);
            }
            string configPath = TSDatabaseUtils.EditorFullPath + "/Data/Table/" + config.TableName + "Config.cs";
            if (File.Exists(configPath))
            {
                File.Delete(configPath);
            }
            string configDataPath = TSDatabaseUtils.EditorFullPath + "/Config/Table/" + config.TableName + "Config.asset";
            if (File.Exists(configDataPath))
            {
                File.Delete(configDataPath);
            }
        }
        EditorUtility.DisplayProgressBar(LanguageUtils.CreateListDelete, LanguageUtils.CreateListDeleteing, 1f);

        EditorApplication.UnlockReloadAssemblies();
        WriteConfig();
        AssetDatabase.Refresh();
        if (_tempTableConfig == config)
        {
            _typeList.index = -1;
            _typeList.onSelectCallback(_typeList);
            //_tempTableConfig = null;
        }
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog(LanguageUtils.CreateListDelete, LanguageUtils.CreateListDeleted, "OK");
    }


    #region 回调

    private void TypeListSelectCallback(ReorderableList list)
    {
        if (list.index < 0)
        {
            _typeInfoList = null;
            _tempTableConfig = null;
            return;
        }
        else
        {
            _tempTableConfig = TSDatabaseUtils.TableConfigSerializeData.TableConfigList[list.index];
            _typeInfoList = new ReorderableList(_tempTableConfig.FieldList, typeof(FieldConfig), true, true, true, false);
            _typeInfoList.elementHeight = 80;
            _typeInfoList.drawHeaderCallback = TypeInfoListDrawHeaderCallback;
            _typeInfoList.drawElementCallback = TypeInfoListDrawElementCallback;
            _typeInfoList.onAddCallback = TypeInfoListAddCallback;
        }

    }

    private void TypeListAddCallback(ReorderableList list)
    {
        _tempTableConfig = new TableConfig();
        _tempTableConfig.HasPrimaryKey = true;
        _typeInfoList = new ReorderableList(_tempTableConfig.FieldList, typeof(FieldConfig), true, true, true, false);
        _typeInfoList.elementHeight = 80;
        _typeInfoList.drawHeaderCallback = TypeInfoListDrawHeaderCallback;
        _typeInfoList.drawElementCallback = TypeInfoListDrawElementCallback;
        _typeInfoList.onAddCallback = TypeInfoListAddCallback;
        TSDatabaseUtils.TableConfigSerializeData.TableConfigList.Add(_tempTableConfig);
    }

    private void TypeListDrawHeaderCallback(Rect rect)
    {
        rect.width -= 30;
        EditorGUI.LabelField(rect, LanguageUtils.CreateListHead);
        rect.x = rect.xMax + 5;
        rect.width = 30;
        if (GUI.Button(rect, "", "OL Plus"))
        {
            TypeListAddCallback(null);
            //_tempTableConfig = new TableConfig();
            //_tempTableConfig.HasPrimaryKey = true;
            //TSDatabaseUtils.TableConfigSerializeData.TableConfigList.Add(_tempTableConfig);
        }
    }
    private void TypeListDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (index < TSDatabaseUtils.TableConfigSerializeData.TableConfigList.Count)
        {
            rect.width -= 30;
            string tableName = TSDatabaseUtils.TableConfigSerializeData.TableConfigList[index].Description;
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = TSDatabaseUtils.TableConfigSerializeData.TableConfigList[index].TableName;
            }
            EditorGUI.LabelField(rect, tableName, new GUIStyle("LODRendererAddButton"));
            rect.y += 5;
            rect.x = rect.xMax + 5;
            rect.width = 30;
            if (GUI.Button(rect, "", "OL Minus"))
            {
                if (EditorUtility.DisplayDialog("警告", "是否删除？", "是", "否"))
                {
                    RemoveData(index);
                    Repaint();
                }
            }
        }
    }


    private void TypeInfoListAddCallback(ReorderableList list)
    {
        _tempTableConfig.FieldList.Add(new FieldConfig() { FieldIndex = 0, FieldType = "int", GenericIndex = 0, GenericType = "int", IsExport = true });
    }


    private void TypeInfoListDrawHeaderCallback(Rect rect)
    {
        rect.width -= 30;
        EditorGUI.LabelField(rect, "字段信息");
        rect.x = rect.xMax + 5;
        rect.width = 30;
        if (GUI.Button(rect, "", "OL Plus"))
        {
            _tempTableConfig.FieldList.Add(new FieldConfig() { FieldIndex = 0, FieldType = "int", GenericIndex = 0, GenericType = "int", IsExport = true });
        }
    }


    private void TypeInfoListDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (index < _tempTableConfig.FieldList.Count)
        {
            FieldConfig fieldConfig = _tempTableConfig.FieldList[index];
            //EditorGUI.DrawRect(rect,Color.black);

            #region 初始化几个Rect

            Rect guiRect = rect;
            guiRect.height -= 5;
            GUI.Label(guiRect, "", "OL box");

            Rect oneRect = guiRect;
            oneRect.y += 10;
            oneRect.height -= 10;
            oneRect.x += 5;
            oneRect.width -= 5;

            Rect twoRect = guiRect;
            twoRect.y += 35;
            twoRect.height -= 35;
            twoRect.x += 5;
            twoRect.width -= 5;

            Rect threeRect = guiRect;
            threeRect.y += 55;
            threeRect.height -= 55;
            threeRect.x += 5;
            threeRect.width -= 5;

            #endregion

            #region 第一行

            Rect columnLabelRect = new Rect(oneRect.xMin, oneRect.yMin, INFO_LABEL_WIDTH + 10, 15);
            EditorGUI.LabelField(columnLabelRect, LanguageUtils.CreateInfoColumnName);

            Rect columnTextFieldRect = new Rect(columnLabelRect.xMax, oneRect.yMin, INFO_INPUT_WIDTH, 15);
            fieldConfig.FieldName = EditorGUI.TextField(columnTextFieldRect, fieldConfig.FieldName);

            Rect desLabelRect = new Rect(columnTextFieldRect.xMax + 10, oneRect.yMin, INFO_LABEL_WIDTH + 10, 15);
            EditorGUI.LabelField(desLabelRect, LanguageUtils.CreateInfoDescription);

            Rect desTextFieldRect = new Rect(desLabelRect.xMax, oneRect.yMin, INFO_INPUT_WIDTH, 15);
            fieldConfig.Description = EditorGUI.TextField(desTextFieldRect, fieldConfig.Description);

            Rect exportLabelRect = new Rect(desTextFieldRect.xMax + 10, oneRect.yMin, INFO_LABEL_WIDTH + 10, 15);
            EditorGUI.LabelField(exportLabelRect, LanguageUtils.CreateInfoExport);

            Rect exportToggleRect = new Rect(exportLabelRect.xMax, oneRect.yMin, 20, 15);
            fieldConfig.IsExport = EditorGUI.Toggle(exportToggleRect, fieldConfig.IsExport);


            Rect deleteButtonRect = new Rect(oneRect.xMax - 25, oneRect.yMin, 30, 15);
            if (GUI.Button(deleteButtonRect, "", "OL Minus"))
            {
                if (EditorUtility.DisplayDialog("警告", "是否删除？", "是", "否"))
                {
                    _tempTableConfig.FieldList.RemoveAt(index);
                    Repaint();
                }
            }

            #endregion

            #region 第二行

            Rect mainTypeLabelRect = new Rect(twoRect.xMin, twoRect.yMin, INFO_LABEL_WIDTH + 10, 15);
            EditorGUI.LabelField(mainTypeLabelRect, LanguageUtils.CreateInfoType);

            Rect mainTypePopupRect = new Rect(mainTypeLabelRect.xMax, twoRect.yMin, INFO_INPUT_WIDTH, 15);
            int baseIndex = EditorGUI.Popup(mainTypePopupRect, fieldConfig.FieldIndex, TSDatabaseUtils.BaseType);
            if (baseIndex != fieldConfig.FieldIndex)
            {
                fieldConfig.FieldIndex = baseIndex;
                fieldConfig.FieldType = TSDatabaseUtils.BaseType[baseIndex];
                if (fieldConfig.FieldType == "enum" && !string.IsNullOrEmpty(fieldConfig.EnumName))
                {
                    for (int j = 0; j < _enumArray.Length; j++)
                    {
                        if (fieldConfig.EnumName == _enumArray[j])
                        {
                            fieldConfig.EnumIndex = j;
                            break;
                        }
                    }
                }
            }
            if (fieldConfig.FieldType == "List")
            {
                Rect listTypeLabelRect = new Rect(mainTypePopupRect.xMax + 10, twoRect.yMin, INFO_LABEL_WIDTH + 10, 15);
                EditorGUI.LabelField(listTypeLabelRect, LanguageUtils.CreateInfoItemType);

                Rect listTypePopupRect = new Rect(listTypeLabelRect.xMax, twoRect.yMin, INFO_INPUT_WIDTH, 15);
                int collectionIndex = EditorGUI.Popup(listTypePopupRect, fieldConfig.GenericIndex, TSDatabaseUtils.GenericType);
                if (collectionIndex != fieldConfig.GenericIndex)
                {
                    fieldConfig.GenericIndex = collectionIndex;
                    fieldConfig.GenericType = TSDatabaseUtils.GenericType[collectionIndex];
                    if (fieldConfig.GenericType == "enum" && !string.IsNullOrEmpty(fieldConfig.EnumName))
                    {
                        for (int j = 0; j < _enumArray.Length; j++)
                        {
                            if (fieldConfig.EnumName == _enumArray[j])
                            {
                                fieldConfig.EnumIndex = j;
                                break;
                            }
                        }
                    }
                }
                if (fieldConfig.GenericType == "enum")
                {
                    ShowEnum(listTypePopupRect, fieldConfig);
                }
            }
            else if (fieldConfig.FieldType == "enum")
            {
                ShowEnum(mainTypePopupRect, fieldConfig);
            }
            #endregion

            #region 第三行

            if ((fieldConfig.FieldType == "List" && (fieldConfig.GenericType == "int" || fieldConfig.GenericType == "string")) || (fieldConfig.FieldType == "int" || fieldConfig.FieldType == "string"))
            {

                Rect foreignKeyLabelRect = new Rect(threeRect.xMin, threeRect.yMin, INFO_LABEL_WIDTH - 20, 15);
                EditorGUI.LabelField(foreignKeyLabelRect, LanguageUtils.CreateInfoForeignKey);

                Rect foreignKeyToggleRect = new Rect(foreignKeyLabelRect.xMax + 5, threeRect.yMin, 15, 15);
                fieldConfig.HasForeignKey = EditorGUI.Toggle(foreignKeyToggleRect, fieldConfig.HasForeignKey);
                if (fieldConfig.HasForeignKey)
                {

                    Rect foreignKeyPopupRect = new Rect(foreignKeyToggleRect.xMax + 10, threeRect.yMin, INFO_INPUT_WIDTH, 15);
                    string[] keys = TSDatabaseUtils.GetForeignKey(_tempTableConfig, fieldConfig.GenericType);
                    //if (keys.Length == 1)
                    //{
                    //    fieldConfig.ForeignKeyIndex = 0;
                    //    fieldConfig.ForeignKey = "";
                    //    keys = new string[0];
                    //    EditorGUI.Popup(foreignKeyPopupRect,fieldConfig.ForeignKeyIndex, keys);
                    //}
                    //else
                    //{
                    fieldConfig.ForeignKeyIndex = EditorGUI.Popup(foreignKeyPopupRect, fieldConfig.ForeignKeyIndex, keys);
                    if (keys.Length > fieldConfig.ForeignKeyIndex && fieldConfig.ForeignKeyIndex >= 0)
                    {
                        fieldConfig.ForeignKey = keys[fieldConfig.ForeignKeyIndex];
                    }
                    //}
                }
                else
                {
                    GUILayout.Space(INFO_INPUT_WIDTH + 9);
                }
            }
            else
            {
                fieldConfig.HasForeignKey = false;
            }

            #endregion
        }
    }

    private void ShowEnum(Rect rect, FieldConfig fieldConfig)
    {
        Rect enumTypeLabelRect = new Rect(rect.xMax + 10, rect.yMin, INFO_LABEL_WIDTH + 10, 15);
        EditorGUI.LabelField(enumTypeLabelRect, LanguageUtils.CreateInfoEnumType);
        if (_enumArray.Length == 0)
        {
            Rect noEnumTypeLabelRect = new Rect(enumTypeLabelRect.xMax + 10, rect.yMin, INFO_LABEL_WIDTH + 10, 15);
            EditorGUI.LabelField(noEnumTypeLabelRect, LanguageUtils.CreateInfoNotEnum);
            fieldConfig.EnumName = "";
        }
        else
        {
            #region 当项目增加枚举时候确保枚举是之前设置的值
            if (_enumArray.Length > fieldConfig.EnumIndex && fieldConfig.EnumIndex != -1)
            {
                if (_enumArray[fieldConfig.EnumIndex] != fieldConfig.EnumName)
                {
                    bool isFind = false;
                    for (int j = 0; j < _enumArray.Length; j++)
                    {
                        if (_enumArray[j] == fieldConfig.EnumName)
                        {
                            isFind = true;
                            fieldConfig.EnumIndex = j;
                            break;
                        }
                    }
                    if (!isFind)
                    {
                        fieldConfig.EnumIndex = -1;
                    }
                }
            }
            else
            {
                fieldConfig.EnumIndex = -1;
            }
            #endregion
            Rect EnumTypePopupRect = new Rect(enumTypeLabelRect.xMax, rect.yMin, INFO_INPUT_WIDTH, 15);
            fieldConfig.EnumIndex = EditorGUI.Popup(EnumTypePopupRect, fieldConfig.EnumIndex, _enumArray);
            if (fieldConfig.EnumIndex != -1)
            {
                fieldConfig.EnumName = _enumArray[fieldConfig.EnumIndex];
            }
        }
    }

    #endregion


    #region 初始化和功能方法

    void OnDestroy()
    {
        try
        {
            TSDatabaseUtils.SavaGlobalData();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    void OnEnable()
    {
        _enumArray = TSDatabaseUtils.GetEnums();
        _typeList = new ReorderableList(TSDatabaseUtils.TableConfigSerializeData.TableConfigList, typeof(TableConfig), true, true, true, false);
        _typeList.elementHeight = 30;
        _typeList.onSelectCallback = TypeListSelectCallback;
        _typeList.drawHeaderCallback = TypeListDrawHeaderCallback;
        _typeList.drawElementCallback = TypeListDrawElementCallback;
        _typeList.onAddCallback = TypeListAddCallback;
        if (_selectConfigIndex >= 0)
        {
            if (_selectConfigIndex >= TSDatabaseUtils.TableConfigSerializeData.TableConfigList.Count)
            {
                _selectConfigIndex = 0;
            }
            else
            {
                _tempTableConfig = TSDatabaseUtils.TableConfigSerializeData.TableConfigList[_selectConfigIndex];
                _typeInfoList = new ReorderableList(_tempTableConfig.FieldList, typeof(FieldConfig), true, true, true, false);
                _typeInfoList.drawHeaderCallback = TypeInfoListDrawHeaderCallback;
                _typeInfoList.drawElementCallback = TypeInfoListDrawElementCallback;
                _typeInfoList.elementHeight = 80;
            }
        }
    }


    private void GenerateCode()
    {
        _isGenerateCode = true;
        string csName = _tempTableConfig.TableName + "SerializeData.cs";
        if (string.IsNullOrEmpty(_tempTableConfig.CodePath))
        {
            string path = EditorUtility.SaveFolderPanel(LanguageUtils.CreateInfoSelectFolder, Application.dataPath, "");
            int index = path.IndexOf("Asset");
            if (index < 0)
            {
                if (EditorUtility.DisplayDialog(LanguageUtils.CommonSaveFailed, LanguageUtils.CommonNullPath, "OK"))
                {
                    _isGenerateCode = false;
                    return;
                }
            }
            path = path.Substring(index, path.Length - index);
            _tempTableConfig.CodePath = path + "/" + csName;
        }
        else
        {
            if (Path.GetFileName(_tempTableConfig.CodePath) != csName)
            {
                _tempTableConfig.CodePath = Path.GetDirectoryName(_tempTableConfig.CodePath) + "/" + csName;
            }
        }
        if (string.IsNullOrEmpty(_tempTableConfig.PrimaryKey))
        {
            if (EditorUtility.DisplayDialog(LanguageUtils.CommonSaveFailed, LanguageUtils.CreateInfoPrimaryKeyIsNull, "OK"))
            {
                _isGenerateCode = false;
                return;
            }
        }
        EditorApplication.LockReloadAssemblies();
        try
        {

            GenerateCode code = new GenerateCode(_tempTableConfig);
            if (!code.GenerateTable())
            {
                if (EditorUtility.DisplayDialog(LanguageUtils.CommonSaveFailed, LanguageUtils.CreateInfoCodeError, "OK"))
                {
                    _isGenerateCode = false;
                    return;
                }
            }
            EditorApplication.UnlockReloadAssemblies();
            _isGenerateCode = false;
            if (EditorUtility.DisplayDialog("success", LanguageUtils.CommonSaveSuccess, "OK"))
            {
                AssetDatabase.Refresh();
            }
        }
        catch (Exception ex)
        {
            EditorApplication.UnlockReloadAssemblies();
            _isGenerateCode = false;
            EditorUtility.DisplayDialog(LanguageUtils.CommonSaveFailed, ex.Message, "OK");
        }
    }

    private void WriteConfig()
    {
        TSDatabaseUtils.SavaGlobalData();
        AssetDatabase.SaveAssets();
    }

    #endregion

}
