//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-08-12 21:51:34----------------------------------------
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
    public List<int> ListTest = new List<int>();
    public GameObject Obj;
    public List<TestEnum> EnumList = new List<TestEnum>();
}
