using ANU.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class HierarchyItem : AHierarchyBase
{
    [SerializeField] private HierarchyItem parent;
    [SerializeField] private HierachyConnector connector;
    private List<HierarchyItem> childrens = new List<HierarchyItem>();
    
    public override bool IsActiveSelf
    {
        get => base.IsActiveSelf;
        set
        {
            var paih = true;
            if (parent != null)
                paih = parent.IsActiveInHierarchy;

            if (connector != null)
                connector.IsActiveSelf = paih;

            base.IsActiveSelf = value;
            IsActiveInHierarchy = value && paih;

            foreach (var child in childrens)
                child.IsActiveInHierarchy = child.IsActiveSelf && IsActiveInHierarchy;
        }
    }

    private bool isActiveChildrens = false;
    public virtual bool IsActiveChildrens
    {
        get => isActiveChildrens;
        set
        {
            IsActiveSelf = value;
            foreach (var child in childrens)
                child.IsActiveSelf = value;
        }
    }


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

    protected override void Init()
    {
        base.Init();

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
}
