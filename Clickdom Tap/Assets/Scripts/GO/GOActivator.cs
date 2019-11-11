using UnityEngine;
using System.Collections;

public class GOActivator : MonoBehaviour
{
    public enum EffectActivation { ACTIVATE }

    [Tooltip("effect before activating object")]
    [SerializeField] private ASpawnerEffect effect;
    [SerializeField] private EffectActivation effectActivation = EffectActivation.ACTIVATE;
    [Space]
    [Tooltip("object to asctivate")]
    [SerializeField] private GameObject activationObject;

    public void ActivateWithEffect()
    {
        if(effectActivation == EffectActivation.ACTIVATE)
        {
            if (effect != null)
            {
                effect.OnEffectEnds += Effect_OnEffectEnds;
                effect.Play();
            }
            else
            {
                ActivateObject();
            }
        }
    }

    private void Effect_OnEffectEnds()
    {
        effect.OnEffectEnds -= Effect_OnEffectEnds;
        ActivateObject();
    }

    public void ActivateObject()
    { 
        activationObject.SetActive(true);
    }
}
