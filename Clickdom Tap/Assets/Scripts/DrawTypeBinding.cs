using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GraphixSettingsHolder))]
public class DrawTypeBinding : MonoBehaviour
{
    private GraphixSettingsHolder holder;

    private void Start()
    {
        holder = GetComponent<GraphixSettingsHolder>();
    }
    
    public void SetDrawType(int type)
    {
        try
        {
            holder.Settings.DefaultDrawType = (GraphixSettings.DrawType)type;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.ToString(), this);
            holder.Settings.DefaultDrawType = GraphixSettings.DrawType.INSTANCED;
        }
    }
}
