using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

/////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{ 

[DisableAutoCreation]
[BurstCompile]
[RequireMatchingQueriesForUpdate]
public partial struct FillAnimationsFromControllerSystem: ISystem
{
	EntityQuery fillAnimationsBufferQuery;

	BufferTypeHandle<AnimatorControllerLayerComponent> controllerLayersBufferHandle;
	BufferTypeHandle<AnimatorControllerParameterComponent> controllerParametersBufferHandle;
	BufferTypeHandle<AnimationToProcessComponent> animationToProcessBufferHandle;
	EntityTypeHandle entityTypeHandle;

/////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnCreate(ref SystemState ss)
	{
		var eqBuilder1 = new EntityQueryBuilder(Allocator.Temp)
		.WithAllRW<AnimatorControllerLayerComponent>()
		.WithAllRW<AnimationToProcessComponent>();
		fillAnimationsBufferQuery = ss.GetEntityQuery(eqBuilder1);

		controllerLayersBufferHandle = ss.GetBufferTypeHandle<AnimatorControllerLayerComponent>();
		controllerParametersBufferHandle = ss.GetBufferTypeHandle<AnimatorControllerParameterComponent>();
		animationToProcessBufferHandle = ss.GetBufferTypeHandle<AnimationToProcessComponent>();
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnDestroy(ref SystemState ss)
	{
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnUpdate(ref SystemState ss)
	{
		controllerLayersBufferHandle.Update(ref ss);
		controllerParametersBufferHandle.Update(ref ss);
		animationToProcessBufferHandle.Update(ref ss);
		entityTypeHandle.Update(ref ss);

		var fillAnimationsBufferJob = new FillAnimationsBufferJob()
		{
			controllerLayersBufferHandle = controllerLayersBufferHandle,
			controllerParametersBufferHandle = controllerParametersBufferHandle,
			animationToProcessBufferHandle = animationToProcessBufferHandle,
			entityTypeHandle = entityTypeHandle,
		};

		ss.Dependency = fillAnimationsBufferJob.ScheduleParallel(fillAnimationsBufferQuery, ss.Dependency);
	}
}
}
