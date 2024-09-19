using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSCube : MonoBehaviour
{
    
}

public class ECSCubeBaker : Baker<ECSCube>
{
    public override void Bake(ECSCube authoring)
    {
        TransformUsageFlags transformUsageFlags = new TransformUsageFlags();
        Entity entity = this.GetEntity(transformUsageFlags);
    }
}