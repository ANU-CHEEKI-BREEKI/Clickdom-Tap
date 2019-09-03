using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct VelocityAbsoluteComponentData : IComponentData
{
    public float value;
}

public struct RotationVelocityDegreeComponentData : IComponentData
{
    public float value;
}

