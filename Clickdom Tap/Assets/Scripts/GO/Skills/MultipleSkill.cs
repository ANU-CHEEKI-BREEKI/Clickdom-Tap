using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MultipleSkill : VolleyShootSkill
{
    [SerializeField] private ASkillTargetPositionProvider positionProvider;
    [SerializeField] private int targetsCount = 5;

    public int SkillsCount => targetsCount;

    public override void ExecuteAt(Vector3 position)
    {
        var scale = 1f;
        if (scaleSettings != null)
            scale = scaleSettings.LerpEvaluete(position);

        for (int i = 0; i < targetsCount; i++)
        {
            var pos = positionProvider.GetTargetPosition(position, targetsCount, i, scale);
            base.ExecuteAt(pos);
        }
    }
}
