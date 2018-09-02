using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MyTestEnum
{
    None
}

public enum TestEnum
{
    None,
    Test
}

public class NewBehaviourScript : MonoBehaviour
{
    private void Start()
    {
        Array array = Enum.GetValues(typeof(TestEnum));
        string str = "1";
        
        object obj = Enum.ToObject(typeof(TestEnum), 1);
        Debug.LogError(obj);
        foreach (var item in array)
        {
            Debug.LogError(item);
        }
    }
}

[System.Serializable]
public class MyTest
{
    public int X;
    public int Y;
}