using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

public readonly partial struct HumanVisuAspect : IAspect
{
    public readonly Entity entity;
    private readonly RefRW<HumanVisuCompo> _humanVisuCompo;

    public bool isCreated => _humanVisuCompo.ValueRO.isCreated;
    public bool isActivated => _humanVisuCompo.ValueRO.isActivated;

    public void setIsCreated(bool created)
    {
        _humanVisuCompo.ValueRW.isCreated = created;
    }

    public void setIsActivated(bool activated)
    {
        _humanVisuCompo.ValueRW.isActivated = activated;
    }
}
