using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableConfigSerializeData : ScriptableObject
{
    public TableSetting Setting = new TableSetting();

    public List<TableConfig> TableConfigList = new List<TableConfig>();
}

[System.Serializable]
public class TableSetting
{
    
    public bool IsUseSpace = false;
    public string SplitVecChar = "|";
    public string SplitListChar = ",";
}


[System.Serializable]
public class TableConfig
{
    /// <summary>
    /// 表名
    /// </summary>
    public string TableName;
    /// <summary>
    /// 显示名称
    /// </summary>
    public string ShowName;
    /// <summary>
    /// 代码路径
    /// </summary>
    public string CodePath;
    /// <summary>
    /// 数据路径
    /// </summary>
    public string DataPath;

    public bool HasDescription = false;
    public int DescriptionIndex;
    public string Description;

    public bool HasPrimaryKey = false;
    /// <summary>
    /// 主键
    /// </summary>
    public string PrimaryKey;
    public int PrimaryIndex;
    public string PrimaryType;
    /// <summary>
    /// 所有字段
    /// </summary>
    public List<FieldConfig> FieldList = new List<FieldConfig>();
}

[System.Serializable]
public class FieldConfig
{
    /// <summary>
    /// 字段名
    /// </summary>
    public string Name;
    /// <summary>
    /// 显示名称
    /// </summary>
    public string ShowName;
    /// <summary>
    /// 字段类型
    /// </summary>
    public string FieldType = "int";

    public int FieldIndex = 0;
    /// <summary>
    /// 字段泛型类型
    /// </summary>
    public string GenericType = "int";

    public int GenericIndex = 0;

    public int EnumIndex = 0;

    public string EnumName = "";

    public bool IsExport = false;

    public bool HasForeignKey = false;

    public int ForeignKeyIndex;
    /// <summary>
    /// 外键
    /// </summary>
    public string ForeignKey;
}

