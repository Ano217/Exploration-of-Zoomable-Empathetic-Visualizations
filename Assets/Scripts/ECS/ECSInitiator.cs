using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ECSInitiator : MonoBehaviour
{
    private MySceneManager _sceneManager;

    public MySceneManager GetMySceneManager()
    {
        _sceneManager = GameObject.FindObjectOfType<MySceneManager>();
        if (_sceneManager != null) return _sceneManager;
        return null;
    }

    private void OnDisable()
    {
        
    }
}

public class ECSInitiatorBaker : Baker<ECSInitiator>
{
    public override void Bake(ECSInitiator authoring)
    {
        var sceneManager =authoring.GetMySceneManager();
        if(sceneManager != null)
        {
            // Get the entity
            TransformUsageFlags transformUsageFlags = new TransformUsageFlags();
            Entity entity = this.GetEntity(transformUsageFlags);

            // Prefab DynamicBuffer
            DynamicBuffer<PrefabBufferElement> prefabBuffer;
            prefabBuffer = AddBuffer<PrefabBufferElement>(entity);

            // Add entities to Buffer
            var nbPrefabs = sceneManager.getNbPrefabs();
            for(int i = 0; i < nbPrefabs; i++)
            {
                var prefab = sceneManager.getPrefab(i);
                Debug.Log(prefab.name);
                prefabBuffer.Add(GetEntity(prefab, TransformUsageFlags.Dynamic));
            }

            AddComponent(entity, new HumanVisuCompo { isCreated = false, isActivated = false });
        }
        
    }
}