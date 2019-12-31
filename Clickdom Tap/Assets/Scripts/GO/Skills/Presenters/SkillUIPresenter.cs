using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkillUIPresenter : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image[] addIcons;
    [Header("optional")]
    [SerializeField] private ASkill skill;
    [Tooltip("id for inner additional description")]
    [SerializeField] private int addDescId = -1;

    private bool internalCall = false;

    public int AddDescId { get => addDescId; set => addDescId = value; }

    public void Present(SkillDescription description, int addDescId = -1)
    {
        if (description == null)
            return;

        if (addDescId == -1 || internalCall)
        {
            skillIcon.sprite = description.Sprite;
            foreach (var addIcon in addIcons)
                addIcon.sprite = description.Sprite;
        }
        else if (description.AdditionalDescriptions.Length > addDescId)
        {
            internalCall = true;
            Present(description.AdditionalDescriptions[addDescId]);
            internalCall = false;
        }
    }

    public void Present(SkillDescription description)
    {
        Present(description, addDescId);
    }

    public void Present()
    {
        Present(addDescId);
    }

    public void Present(int id)
    {
        if (skill == null)
            return;

        Present(skill.Description, id);
    }
}
