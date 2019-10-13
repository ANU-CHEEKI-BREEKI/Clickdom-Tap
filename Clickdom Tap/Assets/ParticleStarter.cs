using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleStarter : MonoBehaviour
{
    [SerializeField] ParticleSystem[] particles;

    public void StartParticleSystems()
    {
        if (particles == null)
            return;
        foreach (var sys in particles)
            sys?.Play();
    }

    public void StartParticleSystem(int index)
    {
        if (particles == null || particles.Length <= index)
            return;

        particles[index]?.Play();
    }
}
