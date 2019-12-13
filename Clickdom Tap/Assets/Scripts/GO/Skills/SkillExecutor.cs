using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class SkillExecutor : MonoBehaviour, IPointerClickHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private ATargetedSkill skill;
    [SerializeField] private ASkillTargetPresenter targetPresenter;
    [SerializeField] private SkillUIPresenter uiPpresenter;
    [Space]
    [SerializeField] private Image cancelIcon;

    private bool cancelSkill;
    private Vector2 currentTargetPosition;
    private Camera _camera;

    private CanvasGroup cg;

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();
        _camera = Camera.main;
        uiPpresenter = GetComponent<SkillUIPresenter>();
        if (uiPpresenter != null && skill != null)
            uiPpresenter.Present(skill.Description);
    }
    
    private void ExecuteAtTarget(Vector3 position)
    {
        skill?.ExecuteAt(position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!cg.interactable)
            return;

        currentTargetPosition = eventData.position.ScreenToWorld(_camera);
        targetPresenter?.PresentTarget(currentTargetPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!cg.interactable)
            return;

        if (!cancelSkill)
            ExecuteAtTarget(currentTargetPosition);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!cg.interactable)
            return;

        cancelSkill = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!cg.interactable)
            return;

        cancelSkill = false;
        currentTargetPosition = eventData.position.ScreenToWorld(_camera);
        if (cancelIcon != null)
            cancelIcon.enabled = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!cg.interactable)
            return;

        if (cancelIcon != null)
            cancelIcon.enabled = false;
        targetPresenter?.HidePresenter();
    }
}
