using UnityEngine;
using System.Collections;

public class SingleUserProgress : ASavable
{
    [SerializeField] Progress value = new Progress(0, 60);
    public Progress Value => value;
}
