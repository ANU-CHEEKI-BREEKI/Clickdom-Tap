using UnityEngine;
using System.Collections;

public class MultipleSkillPresenter : SimpleSkillTargetPresenter
{
    [Space]
    [SerializeField] private ASkillTargetPositionProvider posProvider;
    [SerializeField] private MultipleSkill skill;
    [SerializeField] private GameObject presenterPrefab;

    private ASkillTargetPresenter[] presenters;

    private void Awake()
    {
        var cnt = skill.SkillsCount;
        presenters = new ASkillTargetPresenter[cnt];

        for (int i = 0; i < cnt; i++)
        {
            var go = Instantiate(presenterPrefab).GetComponent<ASkillTargetPresenter>();
            var transform = go.transform;
            go.gameObject.SetActive(true);
            transform.SetParent(presenterTransform);
            presenters[i] = go;
        }
    }

    public override void PresentTarget(Vector3 position)
    {
        var scale = 1f;
        if (scaleSettings != null)
            scale = scaleSettings.LerpEvaluete(position);

        presenterTransform.gameObject.SetActive(true);
        for (int i = 0; i < presenters.Length; i++)
        {
            var pos = posProvider.GetTargetPosition(position, presenters.Length, i, scale);
            presenters[i].PresentTarget(pos);
        }
    }
}
