using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "SkillDescription")]
public class SkillDescription : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    public Sprite Sprite => sprite;

    [SerializeField] private SkillDescription[] additionalDescriptions;
    public SkillDescription[] AdditionalDescriptions => additionalDescriptions;
}

