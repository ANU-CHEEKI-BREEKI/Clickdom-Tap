using UnityEngine;
using System.Collections;

public abstract class ATargetedSkill : MonoBehaviour
{
    public abstract void ExecuteAt(Vector3 position);

    public abstract SkillDescription Description { get; }
    
}
