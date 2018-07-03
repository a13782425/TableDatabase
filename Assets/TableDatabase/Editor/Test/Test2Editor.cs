using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test2Editor : EditorWindow
{

    [MenuItem("Window/TestWindow _%t")]
    static void Init()
    {
        Test2Editor tw = EditorWindow.GetWindow<Test2Editor>("Test Window");

        tw.divisions = new DivisionSlider(4f, false, 50f, 50f, 50f, 50f);
        tw.divisions.SetMinSize(20f);
        tw.resizeMode = DivisionSlider.ResizeMode.DistributeSpace;
        //tw.divisions.MaxSizes[tw.divisions.Count - 1] = 150f;
        
        tw.Show();
    }

    DivisionSlider divisions;
    DivisionSlider.ResizeMode resizeMode;

    float sliderValue1 = 0f;
    float sliderValue2 = 0f;

    void OnGUI()
    {

        //		Rect areaRect = new Rect(0f, 0f, position.width, 60f);
        //		Rect divisionRect = areaRect;
        //		foreach(float size in divisions){
        //			divisionRect.width = size;
        //			GUILayout.BeginArea(divisionRect, EditorStyles.helpBox);
        //			GUILayout.Button("Size");
        //			if (GUILayout.Button(size.ToString())) { Debug.Log("Size: "+size); }
        //			GUILayout.EndArea();
        //			divisionRect.x+=divisionRect.width;
        //		}
        //		divisions.DoHorizontalSlider(areaRect);
        //		GUILayoutUtility.GetRect(position.width, 60f);

        Rect areaRect = new Rect(10f, 0f, position.width - 20f, 60f);
        foreach (Rect rect in divisions.HorizontalLayoutRects(areaRect))
        {
            GUILayout.BeginArea(rect, EditorStyles.helpBox);
            GUILayout.Button("Rect");
            if (GUILayout.Button(rect.ToString())) { Debug.Log("Size: " + rect); }
            GUILayout.EndArea();
        }
        divisions.DoHorizontalSliders(areaRect);
        //		if (Event.current.type == EventType.Layout) {
        divisions.Resize(areaRect.width, resizeMode);
        //		}
        areaRect = GUILayoutUtility.GetRect(position.width, 60f);

        //if (GUILayout.Button("Offsets"))
        //{
        //    int i = 0;
        //    foreach (float offset in divisions) Debug.Log("Size " + i + ": " + offset + " (" + divisions[i++] + ")");
        //    Debug.Log(divisions);
        //}
        //if (GUILayout.Button("Rects"))
        //{
        //    int i = 0;
        //    foreach (Rect rect in divisions.HorizontalLayoutRects(areaRect)) Debug.Log("Rect " + (i++) + ": " + rect);
        //}

        divisions.pushDivisions = EditorGUILayout.ToggleLeft("Push Divisions", divisions.pushDivisions);
        resizeMode = (DivisionSlider.ResizeMode)EditorGUILayout.EnumPopup("Resize mode", resizeMode);

        EditorGUILayout.Space();
        //Rect extraRect = GUILayoutUtility.GetRect(position.width, 60f);
        ////		GUI.enabled = false;
        //sliderValue1 = DivisionSlider.HorizontalSlider(extraRect, sliderValue1, 8f, EditorStyles.miniButton);
        //sliderValue2 = DivisionSlider.HorizontalSlider(extraRect, sliderValue2, 8f, EditorStyles.miniButton);
        //EditorGUILayout.LabelField("Values: " + sliderValue1 + ", " + sliderValue2 + " HotControl: " + GUIUtility.hotControl);
    }
}
