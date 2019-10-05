using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct VelocityAbsoluteComponentData : IComponentData
{
    public float value;
}

public struct VelocityComponentData : IComponentData
{
    public float2 value;
}

public struct RotationVelocityDegreeComponentData : IComponentData
{
    public float value;
}

public struct HealthComponentData : IComponentData
{
    public float value;
}

public struct NativeArrayIndexer<T> : Utils.Algoritm.IIndexer<T> where T : struct
{
    private NativeArray<T> array;

    public NativeArrayIndexer(NativeArray<T> array)
    {
        this.array = array;
    }

    public T this[int index]
    {
        get => array[index];
        set => array[index] = value;
    }

    public int Length => array.Length;
}