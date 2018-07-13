using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CreateTableEditor : EditorWindow
{

    [MenuItem("TSTable/CreateTable", priority = 50)]
    static void CreateTable()
    {
        EditorWindow.GetWindowWithRect<CreateTableEditor>(new Rect(100, 100, 800, 500), false, LanguageUtils.CreateTitle);
    }

    private CreateTableEditor createTableEditor;

    private static TableConfig _tempTableConfig;

    //private List<string> _primaryKeyList = new List<string>();

    private string[] _enumArray;

    private const int INFO_LABEL_WIDTH = 50;
    //private const int INFO_TEXT_WIDTH = 60;
    //private const int INFO_POPUP_WIDTH = 70;

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
    /// </summary>
    private void ShowTypeList()
    {
        GUI.SetNextControlName("DataList");
        GUILayout.BeginArea(new Rect(10, 10, 200, 480), "", "box");
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal("OL Title");
        GUI.color = Color.white;
        GUILayout.Label(LanguageUtils.CreateListHead);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(LanguageUtils.CreateListAdd, "OL Plus"))
        {
            _tempTableConfig = new TableConfig();
            _tempTableConfig.HasPrimaryKey = true;
            TSDatabaseUtils.TableConfigSerializeData.TableConfigList.Add(_tempTableConfig);
            _selectConfigIndex = TSDatabaseUtils.TableConfigSerializeData.TableConfigList.Count - 1;
            GUI.FocusControl("DataList");
        }
        GUILayout.EndHorizontal();
        int removeIndex = -1;
        GUILayout.Space(5);
        _listScollViewVec = GUILayout.BeginScrollView(_listScollViewVec, false, true);
        for (int i = 0; i < TSDatabaseUtils.TableConfigSerializeData.TableConfigList.Count; i++)
        {
            if (_tempTableConfig == TSDatabaseUtils.TableConfigSerializeData.TableConfigList[i])
            {
                GUI.color = Color.yellow;
            }
            GUILayout.BeginHorizontal("GroupBox", GUILayout.Height(30));
            GUI.color = Color.white;
            GUILayout.Space(5);
            if (GUILayout.Button(TSDatabaseUtils.TableConfigSerializeData.TableConfigList[i].TableName, "OL Title", GUILayout.Width(90)))
            {
                _selectConfigIndex = i;
                _tempTableConfig = TSDatabaseUtils.TableConfigSerializeData.TableConfigList[i];
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(LanguageUtils.CommonDelete, "OL Minus"))
            {
                removeIndex = i;
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        GUILayout.EndScrollView();
        if (removeIndex > -1)
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
                _tempTableConfig = null;
            }
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(LanguageUtils.CreateListDelete, LanguageUtils.CreateListDeleted, "OK");
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// 显示类型具体信息
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
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(LanguageUtils.CreateInfoAddField, "OL Plus"))
            {
                _tempTableConfig.FieldList.Add(new FieldConfig() { FieldIndex = 0, FieldType = "int", GenericIndex = 0, GenericType = "int" });
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginArea(new Rect(10, 30, 550, 440), "", "box");
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label(LanguageUtils.CreateInfoTableName, GUILayout.Width(INFO_LABEL_WIDTH));
            _tempTableConfig.TableName = GUILayout.TextField(_tempTableConfig.TableName, GUILayout.Width(INFO_INPUT_WIDTH));
            GUILayout.Label(LanguageUtils.CreateInfoDescription, GUILayout.Width(INFO_LABEL_WIDTH));
            _tempTableConfig.Description = GUILayout.TextField(_tempTableConfig.Description, GUILayout.Width(INFO_INPUT_WIDTH));
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
            GUILayout.Space(5);
            ShowConfigInfo();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
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
            GUILayout.EndVertical();
            GUILayout.EndArea();
            GUILayout.EndVertical();

            GUILayout.EndArea();

        }
    }

    private void ShowConfigInfo()
    {
        _infoScollViewVec = GUILayout.BeginScrollView(_infoScollViewVec, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);
        List<FieldConfig> removeList = new List<FieldConfig>();
        for (int i = 0; i < _tempTableConfig.FieldList.Count; i++)
        {
            FieldConfig fieldConfig = _tempTableConfig.FieldList[i];
            GUILayout.BeginHorizontal("GroupBox");
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();//Name
                    {
                        GUILayout.Label(LanguageUtils.CreateInfoColumnName, GUILayout.Width(INFO_LABEL_WIDTH + 5));
                        fieldConfig.FieldName = GUILayout.TextField(fieldConfig.FieldName, GUILayout.Width(INFO_INPUT_WIDTH));
                        GUILayout.Label(LanguageUtils.CreateInfoDescription, GUILayout.Width(INFO_LABEL_WIDTH + 15));
                        fieldConfig.Description = GUILayout.TextField(fieldConfig.Description, GUILayout.Width(INFO_INPUT_WIDTH));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(LanguageUtils.CommonDelete, "OL Minus"))
                        {
                            removeList.Add(fieldConfig);
                        }
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();//Type
                    {
                        GUILayout.Label(LanguageUtils.CreateInfoType, GUILayout.Width(INFO_LABEL_WIDTH + 5));
                        int baseIndex = EditorGUILayout.Popup(fieldConfig.FieldIndex, TSDatabaseUtils.BaseType, GUILayout.Width(INFO_INPUT_WIDTH));
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
                            GUILayout.Label(LanguageUtils.CreateInfoItemType, GUILayout.Width(INFO_LABEL_WIDTH + 15));
                            int collectionIndex = EditorGUILayout.Popup(fieldConfig.GenericIndex, TSDatabaseUtils.GenericType, GUILayout.Width(INFO_INPUT_WIDTH));
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
                        }
                        if (fieldConfig.FieldType == "enum" || (fieldConfig.FieldType == "List" && fieldConfig.GenericType == "enum"))
                        {
                            GUILayout.Label(LanguageUtils.CreateInfoEnumType, GUILayout.Width(INFO_LABEL_WIDTH + 15));
                            if (_enumArray.Length == 0)
                            {
                                GUILayout.Label(LanguageUtils.CreateInfoNotEnum, GUILayout.Width(INFO_LABEL_WIDTH + 5));
                                fieldConfig.EnumName = "";
                            }
                            else
                            {
                                fieldConfig.EnumIndex = EditorGUILayout.Popup(fieldConfig.EnumIndex, _enumArray, GUILayout.Width(INFO_INPUT_WIDTH));
                                fieldConfig.EnumName = _enumArray[fieldConfig.EnumIndex];
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.Space(5);

                    GUILayout.BeginHorizontal();//Config
                    {

                        if ((fieldConfig.FieldType == "List" && (fieldConfig.GenericType == "int" || fieldConfig.GenericType == "string")) || (fieldConfig.FieldType == "int" || fieldConfig.FieldType == "string"))
                        {
                            GUILayout.Label(LanguageUtils.CreateInfoForeignKey, GUILayout.Width(INFO_LABEL_WIDTH - 20));
                            fieldConfig.HasForeignKey = GUILayout.Toggle(fieldConfig.HasForeignKey, "", GUILayout.Width(15));
                            if (fieldConfig.HasForeignKey)
                            {
                                GUILayout.Space(5);
                                string[] keys = TSDatabaseUtils.GetForeignKey(_tempTableConfig, fieldConfig.GenericType);
                                if (keys.Length == 1)
                                {
                                    fieldConfig.ForeignKeyIndex = 0;
                                    fieldConfig.ForeignKey = "";
                                    keys = new string[0];
                                    EditorGUILayout.Popup(fieldConfig.ForeignKeyIndex, keys, GUILayout.Width(INFO_INPUT_WIDTH));
                                }
                                else
                                {
                                    fieldConfig.ForeignKeyIndex = EditorGUILayout.Popup(fieldConfig.ForeignKeyIndex, keys, GUILayout.Width(INFO_INPUT_WIDTH));
                                    fieldConfig.ForeignKey = keys[fieldConfig.ForeignKeyIndex];
                                }
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
                        GUILayout.Label(LanguageUtils.CreateInfoExport, GUILayout.Width(INFO_LABEL_WIDTH + 20));
                        fieldConfig.IsExport = GUILayout.Toggle(fieldConfig.IsExport, "", GUILayout.Width(20));
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }

        if (removeList.Count > 0)
        {
            for (int i = 0; i < removeList.Count; i++)
            {
                _tempTableConfig.FieldList.Remove(removeList[i]);
            }
            WriteConfig();
        }

        GUILayout.EndScrollView();
    }

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
        if (_selectConfigIndex >= 0)
        {
            if (_selectConfigIndex >= TSDatabaseUtils.TableConfigSerializeData.TableConfigList.Count)
            {
                _selectConfigIndex = 0;
            }
            else
            {
                _tempTableConfig = TSDatabaseUtils.TableConfigSerializeData.TableConfigList[_selectConfigIndex];
            }
        }
        _enumArray = TSDatabaseUtils.GetEnums();
    }

    private void GenerateCode()
    {
        _isGenerateCode = true;
        string csName = _tempTableConfig.TableName + "SerializeData.cs";
        if (string.IsNullOrEmpty(_tempTableConfig.CodePath))
        {
            string path = EditorUtility.SaveFilePanelInProject(LanguageUtils.CommonSaveFile, _tempTableConfig.TableName + "SerializeData.cs", "cs", "");
            if (string.IsNullOrEmpty(path))
            {
                if (EditorUtility.DisplayDialog(LanguageUtils.CommonSaveFailed, LanguageUtils.CommonNullPath, "OK"))
                {
                    _isGenerateCode = false;
                    return;
                }
            }
            _tempTableConfig.CodePath = Path.GetDirectoryName(Path.GetFullPath(path)) + "/" + csName;
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
