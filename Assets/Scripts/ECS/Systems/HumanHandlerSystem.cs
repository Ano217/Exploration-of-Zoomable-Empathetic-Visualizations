using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;

public partial struct HumanHandlerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb= ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var sceneManager = MySceneManager.Instance;

        Entity ecsInitiator;
        var foundECSInitiator = SystemAPI.TryGetSingletonEntity<HumanVisuCompo>(out ecsInitiator);
        if (foundECSInitiator)
        {
            var initiatorAspect = SystemAPI.GetAspect<HumanVisuAspect>(ecsInitiator);
            // Check if need to create humans
            var visuStatus = sceneManager.getVisuStatus();

            if (visuStatus >= 2)
            {
                if (initiatorAspect.isCreated)
                {
                    // Create
                    // Prefab buffer
                    var avatarBuffer = SystemAPI.GetSingletonBuffer<PrefabBufferElement>();
                    var nbAvatar = sceneManager.getNbPoints();
                    for(var i = 0; i<nbAvatar; i++)
                    {
                        var prefabdID = sceneManager.getPointPrefabID(i);
                        var entityAvatar = ecb.Instantiate(avatarBuffer[prefabdID].value);
                        ecb.AddComponent(entityAvatar, new AvatarCompo { id = i });
                        //ecb.AddComponent(entityAvatar, new LocalTransform { })
                    }
                    initiatorAspect.setIsCreated(true);
                    initiatorAspect.setIsActivated(true);
                }
                else if (!initiatorAspect.isActivated)
                {
                    // Activate
                }
            }
            else if(visuStatus < 2)
            {
                if (initiatorAspect.isCreated && initiatorAspect.isActivated)
                {
                    // Deactivate
                }
            }
        }
        
        
    }
}
