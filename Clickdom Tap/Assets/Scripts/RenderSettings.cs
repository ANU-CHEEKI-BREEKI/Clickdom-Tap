using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GraphixSettingsHolder))]
public class RenderSettings : MonoBehaviour
{
    private GraphixSettingsHolder holder;

    private void Start()
    {
        holder = GetComponent<GraphixSettingsHolder>();

        holder.Settings.OnDrawTypeChanged += Settings_OnDrawTypeChanged;

        Settings_OnDrawTypeChanged(holder.Settings.DefaultDrawType);
    }

    private void OnDestroy()
    {
        if(holder != null)
            holder.Settings.OnDrawTypeChanged -= Settings_OnDrawTypeChanged;
    }

    private void Settings_OnDrawTypeChanged(GraphixSettings.DrawType type)
    {
        DynamicRendererCollectorSystem.Instance.UseAsDefault = type == GraphixSettings.DrawType.DYNAMIC;
        InstancedRendererCollectorSystem.Instance.UseAsDefault = type == GraphixSettings.DrawType.INSTANCED;
    }
}
