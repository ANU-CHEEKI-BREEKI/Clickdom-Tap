using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAnimationPlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    public void PlayAnimation(bool isOn)
    {
        animator.SetBool(nameof(isOn), isOn);
    }
     
}
