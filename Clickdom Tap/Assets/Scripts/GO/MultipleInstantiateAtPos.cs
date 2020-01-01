using UnityEngine;
using System.Collections;

public class MultipleInstantiateAtPos : MonoBehaviour
{
    [SerializeField] private InstantiateAtPosition instantiator;
    [SerializeField] [Min(1)] private int count;
    [SerializeField] [Min(0)] private float delayBetweebInstantistions;
    [SerializeField] private Transform position;

    public void InstantiateAtPos()
    {
        InstantiateAtPos(position);
    }

    public void InstantiateAtPos(Transform position)
    {
        instantiator.InstantiateAtPosMultiple(position, count, delayBetweebInstantistions);
    }
}
