using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PrefabBufferElement : IBufferElementData
{
    public Entity value;

    public static implicit operator PrefabBufferElement(Entity value)
    {
        return new PrefabBufferElement { value = value };
    }
}
