//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-06-28 17:05:14----------------------------------------
//------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

public class Test2SerializeData : ScriptableObject
{
    public List<Test2> DataList = new List<Test2>();
}

[System.Serializable]
public class Test2 : ScriptableObject
{
    public int aaa;
}
