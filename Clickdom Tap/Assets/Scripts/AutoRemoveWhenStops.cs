using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(ParticleSystem))]
public class AutoRemoveWhenStops : MonoBehaviour
{
    [SerializeField] private ParticleSystem particlesParent;
    void Start()
    {
        if(particlesParent == null)
            particlesParent = GetComponent<ParticleSystem>();

        if (particlesParent == null)
            return;

        var timer = particlesParent.main.startLifetime.constant;
        StartCoroutine(DestorySelfWithDelay(timer));
    }

    private IEnumerator DestorySelfWithDelay(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
