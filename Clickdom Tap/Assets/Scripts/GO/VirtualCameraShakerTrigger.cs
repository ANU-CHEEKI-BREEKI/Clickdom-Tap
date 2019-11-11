using UnityEngine;
using System.Collections;

public class VirtualCameraShakerTrigger : MonoBehaviour
{
    public enum TriggerType { ON_CALL, ON_AWAKE, ON_START }

    [SerializeField] TriggerType type;
    [SerializeField] Shaker.ShakeSettings settings = Shaker.ShakeSettings.Default;

    private void Awake()
    {
        if (type == TriggerType.ON_AWAKE)
            VirtualCameraShaker.S_ShakeAllVirtualCameras(settings);
    }

    private void Start()
    {
        if (type == TriggerType.ON_START)
            VirtualCameraShaker.S_ShakeAllVirtualCameras(settings);
    }

    public void CallShake()
    {
        if (type == TriggerType.ON_CALL)
            VirtualCameraShaker.S_ShakeAllVirtualCameras(settings);
    }
}
