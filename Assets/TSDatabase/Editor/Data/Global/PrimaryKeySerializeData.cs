using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrimaryKeySerializeData
{
    /// <summary>
    /// key：表明
    /// </summary>
    public Dictionary<string, PrimaryKeyInfo> PrimaryKeyDic = new Dictionary<string, PrimaryKeyInfo>();

}

[System.Serializable]
public class PrimaryKeyInfo
{
    public string TableName;
    public string PrimaryKey;
    public string PrimaryType;
    /// <summary>
    /// Key：主键
    /// value：出现次数
    /// </summary>
    public Dictionary<string, int> Values = new Dictionary<string, int>();
}