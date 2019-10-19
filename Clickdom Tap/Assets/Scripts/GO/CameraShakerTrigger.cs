using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakerTrigger : MonoBehaviour
{
    public enum TriggerType { ON_CALL, ON_AWAKE, ON_START }

    [SerializeField] TriggerType type;
    [SerializeField] CameraShaker.ShakeSettings settings = CameraShaker.ShakeSettings.Default;

    private void Awake()
    {
        if (type == TriggerType.ON_AWAKE)
            CameraShaker.S_ShakeAllCameras(settings);
    }

    private void Start()
    {
        if (type == TriggerType.ON_START)
            CameraShaker.S_ShakeAllCameras(settings);
    }

    public void CallShake()
    {
        if (type == TriggerType.ON_CALL)
            CameraShaker.S_ShakeAllCameras(settings);
    }
}
