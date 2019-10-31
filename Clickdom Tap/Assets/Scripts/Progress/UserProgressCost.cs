using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UserProgress))]
public class UserProgressCost : MonoBehaviour
{
    [SerializeField] ProgressCost count = new ProgressCost(0, 60);
    [SerializeField] ProgressCost spawnRate = new ProgressCost(1, 60);
    [SerializeField] ProgressCost damage = new ProgressCost(1, 60);
    [SerializeField] ProgressCost attackSpeed = new ProgressCost(1, 60);

    private UserProgress relatedProgress;

    public ProgressCost Count => count;
    public ProgressCost SpawnRate => spawnRate;
    public ProgressCost Damage => damage;
    public ProgressCost AttackSpeed => attackSpeed;

    private void Start()
    {
        relatedProgress = GetComponent<UserProgress>();

        count.RelatedProgress = relatedProgress.Count;
        spawnRate.RelatedProgress = relatedProgress.SpawnRate;
        damage.RelatedProgress = relatedProgress.Damage;
        attackSpeed.RelatedProgress = relatedProgress.AttackSpeed;
    }
}
