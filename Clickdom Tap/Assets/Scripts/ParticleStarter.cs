using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParticleStarter : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particles;
    public UnityEvent OnStartPlayParticleSystem = new UnityEvent();

    public void StartParticleSystems()
    {
        if (particles == null)
            return;
        foreach (var sys in particles)
            sys?.Play();

        OnStartPlayParticleSystem.Invoke();
    }

    public void StartParticleSystem(int index)
    {
        if (particles == null || particles.Length <= index)
            return;

        particles[index]?.Play();

        OnStartPlayParticleSystem.Invoke();
    }
}
