using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrimaryKeySerializeData
{
    public Dictionary<string, PrimaryKeyInfo> PrimaryKeyDic = new Dictionary<string, PrimaryKeyInfo>();

}

[System.Serializable]
public class PrimaryKeyInfo
{
    public string TableName;
    public string PrimaryKey;
    public string PrimaryType;
    public Dictionary<string, int> Values = new Dictionary<string, int>();
}