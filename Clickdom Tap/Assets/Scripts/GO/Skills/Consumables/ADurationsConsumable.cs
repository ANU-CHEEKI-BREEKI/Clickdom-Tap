using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public abstract class ADurationsConsumable : ASkill
{
    [SerializeField] [Min(1)] private float duration = 10;
    public float Duration => duration;
    [Space]
    [SerializeField] private UISkillCooldown cooldownUI;

    private float _remainingDurationTime = 0;
    private Coroutine _coroutine = null;

    [SerializeField] private SingleUserProgress count;
    [SerializeField] private FloatToText[] countF2T;
    [SerializeField] private FloatToText[] durationF2T;
    [SerializeField] private InteractableByPriority interact;
    [SerializeField] private int interactPriority = 100_000;
    [Space]
    [SerializeField] private UnityEvent onConsumableUsed;

    protected void Start()
    {
        count.Value.ValueChanged += (newval, oldval) =>
        {
            Init(newval);
        };

        Init(count.Value.Value);
    }

    private void Init(float newval)
    {
        if(countF2T!= null)
            foreach (var cf2t in countF2T)
                cf2t.Float = newval;
        if (durationF2T != null)
            foreach (var df2t in durationF2T)
                df2t.Float = duration;

        interact?.SetEnabled(newval >= 1, priority: interactPriority);
    }

    public void IncreaceCount(int addCount)
    {
        count.Value.Value += addCount;
    }

    public virtual void UseConsumable()
    {
        if (!CanUse())
            return;

        onConsumableUsed?.Invoke();

        _remainingDurationTime += Duration;
        if (_coroutine == null)
            _coroutine = StartCoroutine(UseConsumableRoutine());

        cooldownUI?.SetSpeed(60f / _remainingDurationTime);
        cooldownUI?.StartCooldown();
    }

    private IEnumerator UseConsumableRoutine()
    {
        count.Value.Value -= 1;
        OnStartUse();

        while (_remainingDurationTime > 0)
        {
            yield return null;
            _remainingDurationTime -= Time.deltaTime;
        }

        _remainingDurationTime = 0;
        OnEndUse();

        _coroutine = null;
    }

    protected virtual bool CanUse()
    {
        return count.Value.Value >= 0;
    }
    protected abstract void OnStartUse();
    protected abstract void OnEndUse();
}
