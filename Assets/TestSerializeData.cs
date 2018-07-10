//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-07-10 16:02:59----------------------------------------
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
}
