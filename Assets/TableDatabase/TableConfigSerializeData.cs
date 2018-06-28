using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableConfigSerializeData : ScriptableObject
{
    public List<TableConfig> TableConfigList = new List<TableConfig>();
}
[System.Serializable]
public class TableConfig
{
    public string TableName;
    public string CodePath;
    public string DataPath;
    public List<FieldConfig> FieldList = new List<FieldConfig>();
}

[System.Serializable]
public class FieldConfig
{
    public string Name;
    public string FieldType;
    public int FieldIndex;
    public string GenericType;
    public int GenericIndex;
}