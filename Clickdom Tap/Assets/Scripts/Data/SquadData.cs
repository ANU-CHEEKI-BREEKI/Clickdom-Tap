using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "SquadData")]
public class SquadData : ScriptableObject
{
    [SerializeField] SquadTagSharedComponentData.Data data;
    public SquadTagSharedComponentData.Data Data => data;
}
