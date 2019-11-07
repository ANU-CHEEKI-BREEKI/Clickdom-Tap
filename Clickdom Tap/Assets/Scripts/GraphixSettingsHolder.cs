using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GraphixSettings
{
    [System.Serializable]
    public enum DrawType { DYNAMIC, INSTANCED, AUTO_CHOOSE }

    [SerializeField]  private DrawType defaultDrawType;
    public DrawType DefaultDrawType
    {
        get => defaultDrawType;
        set
        {
            if(value != defaultDrawType)
            {
                defaultDrawType = value;
                OnDrawTypeChanged?.Invoke(value);
            }
        }
    }
    public event Action<DrawType> OnDrawTypeChanged;
}

public class GraphixSettingsHolder : MonoBehaviour
{
    [SerializeField] private GraphixSettings settings;
    public GraphixSettings Settings => settings;

    private GraphixSettings oldSettings;

    private void Start()
    {
        oldSettings = settings;
    }

    private void OnValidate()
    {
        //вызываем событие
        if (oldSettings != null && oldSettings.DefaultDrawType != settings.DefaultDrawType)
            settings.DefaultDrawType = settings.DefaultDrawType;
        oldSettings = settings;
    }
}
