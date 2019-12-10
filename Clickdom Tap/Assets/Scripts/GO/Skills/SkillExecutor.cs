using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillExecutor : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private ATargetedSkill skill;
    [SerializeField] private ASkillTargetPresenter targetPresenter;
    [Space]
    [SerializeField] private Image cancelIcon;

    private bool cancelSkill;
    private Vector2 currentTargetPosition;
    private Camera _camera;

    private SkillUIPresenter uiPpresenter;

    private void Start()
    {
        _camera = Camera.main;
        uiPpresenter = GetComponent<SkillUIPresenter>();
        if (uiPpresenter != null)
            uiPpresenter.Present(skill.Description);
    }
    
    private void ExecuteAtTarget(Vector3 position)
    {
        skill?.ExecuteAt(position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        currentTargetPosition = eventData.position.ScreenToWorld(_camera);
        targetPresenter?.PresentTarget(currentTargetPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!cancelSkill)
            skill?.ExecuteAt(currentTargetPosition);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        cancelSkill = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        cancelSkill = false;
        currentTargetPosition = eventData.position.ScreenToWorld(_camera);
        if (cancelIcon != null)
            cancelIcon.enabled = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(cancelIcon != null)
            cancelIcon.enabled = false;
        targetPresenter?.HidePresenter();
    }
}
