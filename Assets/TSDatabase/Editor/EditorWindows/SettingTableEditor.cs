using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SettingTableEditor : EditorWindow
{
    [MenuItem("TSTable/TSSetting", priority = 100)]
    static void TSSetting()
    {
        EditorWindow.GetWindow<SettingTableEditor>(false, LanguageUtils.SettingTitle).position = new Rect(100, 100, 200, 300);
    }

    void OnGUI()
    {
        HeadGUI();

        SettingGUI();
    }

    private void HeadGUI()
    {
        GUI.color = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal("OL Title");
        GUI.color = Color.white;
        GUILayout.Label(LanguageUtils.SettingHead);
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(LanguageUtils.CommonSaveSetting))
        {
            TSDatabaseUtils.SavaGlobalData();
        }
        GUILayout.EndHorizontal();
    }

    private void SettingGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label(LanguageUtils.SettingVariable);
        TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar = GUILayout.TextField(TSDatabaseUtils.TableConfigSerializeData.Setting.SplitVarChar);

        GUILayout.Space(5);
        GUILayout.Label(LanguageUtils.SettingList);
        TSDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar = GUILayout.TextField(TSDatabaseUtils.TableConfigSerializeData.Setting.SplitListChar);

        GUILayout.Space(5);
        GUILayout.Label(LanguageUtils.SettingDefaultColumnWidth);
        TSDatabaseUtils.TableConfigSerializeData.Setting.ColumnWidth = EditorGUILayout.Slider(TSDatabaseUtils.TableConfigSerializeData.Setting.ColumnWidth, 50f, 300f);

        GUILayout.Space(5);
        GUILayout.Label(LanguageUtils.SettingLanguage);
        TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage = (LanguageEnum)EditorGUILayout.EnumPopup((Enum)TSDatabaseUtils.TableConfigSerializeData.Setting.CurrentLanguage);

        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

    }
}
