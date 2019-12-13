using UnityEngine;
using System.Collections;

public class UISkillCooldown : MonoBehaviour, ISpeedSettable
{
    [SerializeField] private AProgressBar progressBar;
    [SerializeField] private InteractableByPriority presenterIntaraction;

    private float coolDownDurationInSeconds = 0f;

    public void SetSpeed(float speed)
    {
        if (speed == 0)
            speed = 1;
        coolDownDurationInSeconds = 60 / speed;
    }

    public void StartCooldown()
    {
        StartCoroutine(Cooldown(coolDownDurationInSeconds));
    }

    private IEnumerator Cooldown(float duration)
    {
        presenterIntaraction?.SetEnabled(false, int.MaxValue);
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

        presenterIntaraction?.SetEnabled(true, int.MaxValue);
    }
}
