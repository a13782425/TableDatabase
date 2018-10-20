//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-10-20 23:09:21----------------------------------------
//------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

public class TestSerializeData : ScriptableObject
{
    public List<Test> DataList = new List<Test>();
}

[System.Serializable]
public class Test
{
    public int Id;
    public string Str = "";
    public Vector2 Vec2;
    public Quaternion Quat;
    public List<string> ListTest = new List<string>();
    public GameObject Obj;
    public List<TestEnum> EnumList = new List<TestEnum>();
}
