using UnityEngine;
using System.Collections;

public abstract class ASkill : MonoBehaviour
{
    [SerializeField] private SkillDescription description;
    public SkillDescription Description => description;
}
