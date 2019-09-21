using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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