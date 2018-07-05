//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-07-05 23:01:49----------------------------------------
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
    public int T;
    public List<int> a = new List<int>();
    public int b;
    public int c;
    public int d;
    public int e;
}
