using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public abstract class ATargetedSkill : MonoBehaviour
{
    [SerializeField] private UnityEvent onSkillExecutionStart;

    protected bool IsSkillExecutionStartEventDisabled { get; set; } = false;

    public void CallOnSkillExecutionStartEvent()
    {
        if (IsSkillExecutionStartEventDisabled)
            return;
        onSkillExecutionStart?.Invoke();
    }

    public abstract void ExecuteAt(Vector3 position);

    public abstract SkillDescription Description { get; }
    
}
