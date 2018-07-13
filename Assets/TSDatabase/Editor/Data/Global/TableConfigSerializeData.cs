using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TableConfigSerializeData : ScriptableObject
{
    public TableSetting Setting = new TableSetting();

    public List<TableConfig> TableConfigList = new List<TableConfig>();
}

[System.Serializable]
public class TableSetting
{
    public float ColumnWidth = 150;
    public string SplitVarChar = "|";
    public string SplitListChar = ",";
    public LanguageEnum CurrentLanguage = LanguageEnum.English;
}


[System.Serializable]
public class TableConfig
{
    /// <summary>
    /// TableName
    /// </summary>
    public string TableName = "";
    /// <summary>
    /// ShowName
    /// </summary>
    public string Description = "";
    /// <summary>
    /// CodePath
    /// </summary>
    public string CodePath;
    /// <summary>
    /// DataPath
    /// </summary>
    public string DataPath;

    public bool HasPrimaryKey = false;
    /// <summary>
    /// PrimaryKey
    /// </summary>
    public string PrimaryKey;
    public int PrimaryIndex;
    public string PrimaryType;
    /// <summary>
    /// All Field
    /// </summary>
    public List<FieldConfig> FieldList = new List<FieldConfig>();
}

[System.Serializable]
public class FieldConfig
{
    /// <summary>
    /// FieldName
    /// </summary>
    public string FieldName = "";
    /// <summary>
    /// ShowName
    /// </summary>
    public string Description = "";
    /// <summary>
    /// FieldType
    /// </summary>
    public string FieldType = "int";
    /// <summary>
    /// FieldType Index
    /// </summary>
    public int FieldIndex = 0;
    /// <summary>
    /// GenericType
    /// </summary>
    public string GenericType = "int";

    public int GenericIndex = 0;

    public int EnumIndex = 0;

    public string EnumName = "";

    public bool IsExport = false;

    public bool HasForeignKey = false;

    public int ForeignKeyIndex;
    /// <summary>
    /// ForeignKey
    /// </summary>
    public string ForeignKey;
}

