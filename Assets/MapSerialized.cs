
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapSerialized : ScriptableObject
{
    public int TestId;
    public List<MapTable> MapList = new List<MapTable>();
}

[System.Serializable]
public class MapTable : ScriptableObject
{
    public int Id;
}
