//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-07-13 17:35:00----------------------------------------
//------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfoSerializeData : ScriptableObject
{
    public List<PlayerInfo> DataList = new List<PlayerInfo>();
}

[System.Serializable]
public class PlayerInfo
{
    public int Id;
    public TestEnum Name;
    public List<TestEnum> ListTest = new List<TestEnum>();
}
