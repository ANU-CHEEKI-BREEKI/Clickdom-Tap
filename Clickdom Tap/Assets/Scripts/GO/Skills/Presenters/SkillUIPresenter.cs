using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkillUIPresenter : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [Space]
    [SerializeField] private ATargetedSkill skill;
    [Tooltip("id for inner additional description")]
    [SerializeField] private int addDescId = -1;

    private bool internalCall = false;

    public void Present(SkillDescription description)
    {
        if (description == null)
            return;

        if (addDescId == -1 || internalCall)
            skillIcon.sprite = description.Sprite;
        else if (description.AdditionalDescriptions.Length > addDescId)
        {
            internalCall = true;
            Present(description.AdditionalDescriptions[addDescId]);
            internalCall = false;
        }
    }

    public void Present()
    {
        if (skill == null)
            return;

        Present(skill.Description);
    }
}
