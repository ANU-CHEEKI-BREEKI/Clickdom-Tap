using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileEffectSettings")]
public class ProjectileEffectSettings : ScriptableObject
{
    [SerializeField] private List<EffectContainer> effects;
    
    public EffectContainer GetEffect(EffectId id)
    {
        return effects.Where(e => e.id == id).FirstOrDefault();
    }
}

public enum EffectId
{
    NONE,
    THEBUCHET_HIT,
    ARROW_HIT_SOUND
}

[Serializable]
public struct EffectContainer
{
    public EffectId id;
    public GameObject effect;

    public bool Equals(EffectContainer obj)
    {
        return id == obj.id;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is EffectContainer))
            return false;
        else
            return Equals((EffectContainer)obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}