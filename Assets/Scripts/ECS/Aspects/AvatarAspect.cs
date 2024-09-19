using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public readonly partial struct AvatarAspect : IAspect
{
    public readonly Entity entity;
    private readonly RefRW<AvatarCompo> _avatarCompo;

    public int id => _avatarCompo.ValueRO.id;
}
