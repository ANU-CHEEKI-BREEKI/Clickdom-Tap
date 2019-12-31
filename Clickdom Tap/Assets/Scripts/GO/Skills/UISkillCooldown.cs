using UnityEngine;
using System.Collections;

public class UISkillCooldown : MonoBehaviour, ISpeedSettable
{
    [SerializeField] private AProgressBar progressBar;
    [SerializeField] private bool hideProgressBarOnCompleted = false;
    [Space]
    [SerializeField] private InteractableByPriority presenterIntaraction;
    [SerializeField] private int interactionPriority = 100_000;

    private Coroutine coroutine = null;

    private float coolDownDurationInSeconds = 0f;

    public void SetSpeed(float speed)
    {
        if (speed == 0)
            speed = 1;
        coolDownDurationInSeconds = 60 / speed;
    }

    public void StartCooldown()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(Cooldown(coolDownDurationInSeconds));
    }

    private IEnumerator Cooldown(float duration)
    {
        presenterIntaraction?.SetEnabled(false, interactionPriority);

        progressBar.gameObject.SetActive(true);
        progressBar.MinValue = 0;
        progressBar.MaxValue = duration;
        progressBar.Type = AProgressBar.ProgressType.DECREACE;
        progressBar.Format = AProgressBar.ProgressFormat.ACTUAL_VALUE;

        progressBar?.SetProgress(duration);
        if(progressBar != null)
            progressBar.TextVisibility = true;

        var timer = 0f;
        while(timer < duration)
        {
            yield return null;
            timer += Time.deltaTime;
            progressBar?.SetProgress(duration - timer);
        }

        progressBar?.SetProgress(0);
        if (progressBar != null)
            progressBar.TextVisibility = false;

        presenterIntaraction?.SetEnabled(true, interactionPriority);

        if (hideProgressBarOnCompleted)
            progressBar.gameObject.SetActive(false);

        coroutine = null;
    }
}
