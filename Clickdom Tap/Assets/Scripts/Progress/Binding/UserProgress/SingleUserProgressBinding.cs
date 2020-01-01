using UnityEngine;
using System.Collections;

public class SingleUserProgressBinding : AUserProgressBindingBase
{
    [Header("binding source")]
    [SerializeField] protected SingleUserProgress source;

    virtual protected void Start()
    {
        BindingSource = source.Value;
    }
}
