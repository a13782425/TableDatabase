//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-06-28 17:39:54----------------------------------------
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
    public GameObject aaa;
    public bool T;
}
