using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PrefabHolder : ScriptableObject
{
    [SerializeField, NonReorderable] public List<GameObject> prefabs;
    public string associatedName;
}