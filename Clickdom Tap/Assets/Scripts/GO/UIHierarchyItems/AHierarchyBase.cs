using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(InteractableByPriority))]
[ExecuteInEditMode]
public abstract class AHierarchyBase : MonoBehaviour
{
    [SerializeField] private bool initOnStart = true;
    [SerializeField] private bool initActiveSelfOnStart = true;
    [SerializeField] private bool initActiveSelfValue = false;
    [Space]
    [SerializeField] private int disablePriority = 0;

    protected RectTransform _transform;
    protected InteractableByPriority interactable;

    private bool isActiveInHierarchy = false;
    public bool IsActiveInHierarchy
    {
        get => isActiveInHierarchy;
        protected set
        {
            isActiveInHierarchy = value;

            //_canvasGroup.alpha = isActiveInHierarchy ? 1 : 0.6f;
            //_canvasGroup.blocksRaycasts = IsActiveInHierarchy;
            interactable.SetEnabled(IsActiveInHierarchy, disablePriority);
        }
    }

    private bool isActiveSelf = false;
    public virtual bool IsActiveSelf
    {
        get => isActiveSelf;
        set
        {
            isActiveSelf = value;
            IsActiveInHierarchy = value;
        }
    }

    private void Start()
    {
        if (!initOnStart)
            return;

        Init();

        if(initActiveSelfOnStart)
            IsActiveSelf = initActiveSelfValue;
    }

    protected virtual void Init()
    {
        if (_transform == null)
            _transform = transform as RectTransform;

        if (interactable == null)
            interactable = GetComponent<InteractableByPriority>();
    }
}
