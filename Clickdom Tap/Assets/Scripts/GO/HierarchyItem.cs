using ANU.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class HierarchyItem : MonoBehaviour
{
    [SerializeField] private HierarchyItem parent;
    [SerializeField] private HierachyConnector connector;
    private List<HierarchyItem> childrens = new List<HierarchyItem>();

    private RectTransform _transform;
    private CanvasGroup _canvasGroup;
    
    private void Reset()
    {
        Init();
        CalcConnectorTransform();
    }

    private void Update()
    {
        Init();
        CalcConnectorTransform();
    }

    private void Init()
    {
        if(_transform == null)
            _transform = transform as RectTransform;

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (parent != null && !parent.childrens.Contains(this))
            parent.childrens.Add(this);
    }

    private void CalcConnectorTransform()
    {
        if (connector == null)
            return;

        connector.gameObject.SetActive(parent != null);

        if (parent != null)
            connector.Connect(_transform.anchoredPosition, parent._transform.anchoredPosition);
    }

    public void SetActive(bool active)
    {
        _canvasGroup.alpha = active ? 1 : 0.6f;
        _canvasGroup.blocksRaycasts = active;

        foreach (var child in childrens)
            child.SetActive(active);

        //connector.SetActive(active);
    }
}
