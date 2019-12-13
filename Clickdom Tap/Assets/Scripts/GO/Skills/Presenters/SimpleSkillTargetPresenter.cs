using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class SimpleSkillTargetPresenter : ASkillTargetPresenter
{
    [SerializeField] protected Transform presenterTransform;
    [SerializeField] protected ScaleByPositionSettings scaleSettings;

    private Vector3 initScale;

    private void Awake()
    {
        initScale = presenterTransform.localScale;
    }

    private void Reset()
    {
        if (presenterTransform == null)
            presenterTransform = transform;
    }

    public override void PresentTarget(Vector3 position)
    {
        presenterTransform.gameObject.SetActive(true);
        transform.position = position;

        if (scaleSettings != null)
            presenterTransform.localScale = (float3)initScale * scaleSettings.LerpEvaluete(position);
    }

    public override void HidePresenter()
    {
        presenterTransform.gameObject.SetActive(false);
    }
}
