using UnityEngine;
using System.Collections;

public abstract class ASkillTargetPresenter : MonoBehaviour
{
    public abstract void PresentTarget(Vector3 position);

    public abstract void HidePresenter();
}
