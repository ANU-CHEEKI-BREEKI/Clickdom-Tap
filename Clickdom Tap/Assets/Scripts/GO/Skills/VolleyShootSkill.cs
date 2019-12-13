using UnityEngine;
using System.Collections;

public class VolleyShootSkill : SingleShootSkill, ICountSettable, IFrequencySettable
{
    [Space]
    [SerializeField] private float radius = 3;
    [SerializeField] private int shootCount = 5;
    [SerializeField] private float pauseBetwenShoots = 1f;

    public override void ExecuteAt(Vector3 position)
    {
        var radius = this.radius;
        if (this.scaleByPos)
            radius *= this.scaleSettings.LerpEvaluete(position);
        StartCoroutine(Volley(shootCount, position, radius, pauseBetwenShoots));
    }

    private IEnumerator Volley(int count, Vector3 position, float radius, float pauseBetwenShoots)
    {
        for (int i = 0; i < shootCount; i++)
        {
            var x = UnityEngine.Random.Range(-1f, 1f);
            var maxY = Mathf.Sqrt(1 - x*x);
            var y = UnityEngine.Random.Range(-maxY, maxY);

            var pos = position + new Vector3(x * radius, y * radius, 0);
            base.ExecuteAt(pos);

            yield return new WaitForSeconds(pauseBetwenShoots);
        }
    }

    void ICountSettable.SetCount(float count)
    {
        shootCount = (int)count;
    }

    /// <summary>
    /// radius of skill zone
    /// </summary>
    /// <param name="frequency"></param>
    void IFrequencySettable.SetFrequency(float frequency)
    {
        radius = frequency;
    }
}
