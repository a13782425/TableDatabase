using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(UnityEngine.Object),true)]
public class MapEditor : Editor
{

    //[MenuItem("Table/Test11")]
    //static void Test()
    //{
    //    //MapSerialized mapSerialized = UnityEditor.AssetDatabase.LoadAssetAtPath<MapSerialized>("Assets/Test.asset");
    //    //MapSerialized mapSerialized = Resources.Load<MapSerialized>("Test");
    //    //Debug.LogError(mapSerialized.MapList[0].Id);
    //    //MapSerialized mapSerialized = ScriptableObject.CreateInstance<MapSerialized>();
    //    //mapSerialized.TestId = 10;
    //    //mapSerialized.MapList.Add(new MapTable() { Id = 1 });
    //    //mapSerialized.MapList.Add(new MapTable() { Id = 2 });
    //    //AssetDatabase.CreateAsset(mapSerialized, "Assets/Test.asset");
    //    //AssetDatabase.SaveAssets();
    //    //AssetDatabase.Refresh();

    //}

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        //EditorGUILayout.ObjectField()
    }
}
