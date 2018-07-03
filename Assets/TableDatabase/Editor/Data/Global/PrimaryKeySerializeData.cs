using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryKeySerializeData : ScriptableObject
{
    public List<PrimaryKeyInfo> PrimaryKeyInfoList = new List<PrimaryKeyInfo>();
}

[System.Serializable]
public class PrimaryKeyInfo
{
    public string TableName;
    public string PrimaryKey;
    public string PrimaryType;
    public List<IntPrimaryKeyCount> IntValues = new List<IntPrimaryKeyCount>();
    public List<StringPrimaryKeyCount> StringValues = new List<StringPrimaryKeyCount>();
}
[System.Serializable]
public class IntPrimaryKeyCount
{
    public int Value;
    public int Count;
}
[System.Serializable]
public class StringPrimaryKeyCount
{
    public string Value;
    public int Count;
}