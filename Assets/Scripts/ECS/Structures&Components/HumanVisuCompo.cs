using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct HumanVisuCompo : IComponentData
{
    public bool isCreated;
    public bool isActivated;
}
