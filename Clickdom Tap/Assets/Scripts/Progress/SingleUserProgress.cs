using UnityEngine;
using System.Collections;

public class SingleUserProgress : AUserProgressBase
{
    [SerializeField] Progress value = new Progress(0, 60);
    public Progress Value => value;
}
