//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-07-10 17:05:56----------------------------------------
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
    public string Name;
    public List<int> ListTest = new List<int>();
}
