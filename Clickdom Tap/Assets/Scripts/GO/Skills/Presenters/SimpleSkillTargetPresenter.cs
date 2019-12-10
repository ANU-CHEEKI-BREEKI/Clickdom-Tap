using UnityEngine;
using System.Collections;

public class SimpleSkillTargetPresenter : ASkillTargetPresenter
{
    [SerializeField] private Transform presenterTransform;

    private void Reset()
    {
        if (presenterTransform == null)
            presenterTransform = transform;
    }

    public override void PresentTarget(Vector3 position)
    {
        presenterTransform.gameObject.SetActive(true);
        transform.position = position;    
    }

    public override void HidePresenter()
    {
        presenterTransform.gameObject.SetActive(false);
    }
}
