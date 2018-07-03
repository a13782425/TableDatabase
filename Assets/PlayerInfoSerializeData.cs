//------------------------------------------------------------------------------------------------------------
//-----------------------------------generate file 2018-07-03 15:27:31----------------------------------------
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
    public string PlayerName;
    public Sprite Icon;
    public List<int> Backinfo = new List<int>();
    public Vector3 Pos;
    public Quaternion Rot;
    public List<Texture> Cloths = new List<Texture>();
}
