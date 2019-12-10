using UnityEngine;
using System.Collections;

public class TrailsRenderSettingsHolder : MonoBehaviour
{
    [SerializeField] private TrailsParticleSettings settings;

    public static TrailsRenderSettingsHolder Instance { get; private set; }

    public TrailsParticleSettings Settings => settings;

    private void Awake()
    {
        Instance = this;
    }

}
