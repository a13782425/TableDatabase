//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-07-05 21:28:30----------------------------------------
//------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

public class Test2SerializeData : ScriptableObject
{
    public List<Test2> DataList = new List<Test2>();
}

[System.Serializable]
public class Test2
{
    public int T2;
}
