using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SkillUIPresenter : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    
    public void Present(SkillDescription description)
    {
        if(description != null)
            skillIcon.sprite = description.Sprite;
    }
}
