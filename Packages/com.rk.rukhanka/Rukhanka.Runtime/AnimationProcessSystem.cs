
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Hash128 = Unity.Entities.Hash128;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka
{

[DisableAutoCreation]
[RequireMatchingQueriesForUpdate]
public partial struct AnimationProcessSystem: ISystem
{
	EntityQuery animatedObjectQuery;

	NativeParallelHashMap<Hash128, BlobAssetReference<BoneRemapTableBlob>> rigToSkinnedMeshRemapTables;
	NativeList<int2> bonePosesOffsetsArr;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnCreate(ref SystemState ss)
	{
		InitializeRuntimeData(ref ss);

		bonePosesOffsetsArr = new (Allocator.Persistent);

		using var eqb0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<RigDefinitionComponent, AnimationToProcessComponent>();
		animatedObjectQuery = ss.GetEntityQuery(eqb0);
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnDestroy(ref SystemState ss)
	{
		if (rigToSkinnedMeshRemapTables.IsCreated)
			rigToSkinnedMeshRemapTables.Dispose();

		if (bonePosesOffsetsArr.IsCreated)
			bonePosesOffsetsArr.Dispose();

		if (SystemAPI.TryGetSingleton<RuntimeAnimationData>(out var rad))
		{
			rad.Dispose();
			ss.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<RuntimeAnimationData>());
		}
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void InitializeRuntimeData(ref SystemState ss)
	{
		var rad = RuntimeAnimationData.MakeDefault();
		ss.EntityManager.CreateSingleton(rad, "Rukhanka.RuntimeAnimationData");
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	JobHandle PrepareComputationData(ref SystemState ss, NativeArray<int> chunkBaseEntityIndices, ref RuntimeAnimationData runtimeData, NativeList<Entity> entitiesArr, JobHandle dependsOn)
	{
		var rigDefinitionTypeHandle = SystemAPI.GetComponentTypeHandle<RigDefinitionComponent>(true);
		
		//	Calculate bone offsets per entity
		var calcBoneOffsetsJob = new CalculateBoneOffsetsJob()
		{
			chunkBaseEntityIndices = chunkBaseEntityIndices,
			bonePosesOffsets = bonePosesOffsetsArr.AsArray(),
			rigDefinitionTypeHandle = rigDefinitionTypeHandle
		};

		var jh = calcBoneOffsetsJob.ScheduleParallel(animatedObjectQuery, dependsOn);

		//	Do prefix sum to calculate absolute offsets
		var prefixSumJob = new DoPrefixSumJob()
		{
			boneOffsets = bonePosesOffsetsArr.AsArray()
		};

		prefixSumJob.Schedule(jh).Complete();

		var boneBufferLen = bonePosesOffsetsArr[^1];
		runtimeData.animatedBonesBuffer.Resize(boneBufferLen.x, NativeArrayOptions.UninitializedMemory);
		runtimeData.boneToEntityArr.Resize(boneBufferLen.x, NativeArrayOptions.UninitializedMemory);
		runtimeData.entityToDataOffsetMap.Capacity = math.max(boneBufferLen.x, runtimeData.entityToDataOffsetMap.Capacity);

		//	Clear flags by two resizes
		runtimeData.boneTransformFlagsHolderArr.Resize(0, NativeArrayOptions.UninitializedMemory);
		runtimeData.boneTransformFlagsHolderArr.Resize(boneBufferLen.y, NativeArrayOptions.ClearMemory);
		
		runtimeData.entityToDataOffsetMap.Clear();

		//	Fill boneToEntityArr with proper values
		var boneToEntityArrFillJob = new CalculatePerBoneInfoJob()
		{
			bonePosesOffsets = bonePosesOffsetsArr,
			boneToEntityIndices = runtimeData.boneToEntityArr,
			chunkBaseEntityIndices = chunkBaseEntityIndices,
			rigDefinitionTypeHandle = rigDefinitionTypeHandle,
			entities = entitiesArr,
			entityToDataOffsetMap = runtimeData.entityToDataOffsetMap.AsParallelWriter()
		};

		var boneToEntityJH = boneToEntityArrFillJob.ScheduleParallel(animatedObjectQuery, default);
		return boneToEntityJH;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	JobHandle AnimationCalculation(ref SystemState ss, NativeList<Entity> entitiesArr, in RuntimeAnimationData runtimeData, JobHandle dependsOn)
	{
		var animationToProcessBufferLookup = SystemAPI.GetBufferLookup<AnimationToProcessComponent>(true);
		var rootMotionAnimationStateBufferLookupRW = SystemAPI.GetBufferLookup<RootMotionAnimationStateComponent>();

		var rigDefsArr = animatedObjectQuery.ToComponentDataListAsync<RigDefinitionComponent>(ss.WorldUpdateAllocator, out var rigDefsLookupJH);
		var dataGatherJH = JobHandle.CombineDependencies(rigDefsLookupJH, dependsOn);

		var computeAnimationsJob = new ComputeBoneAnimationJob()
		{
			animationsToProcessLookup = animationToProcessBufferLookup,
			entityArr = entitiesArr,
			rigDefs = rigDefsArr,
			boneTransformFlagsArr = runtimeData.boneTransformFlagsHolderArr,
			animatedBonesBuffer = runtimeData.animatedBonesBuffer,
			boneToEntityArr = runtimeData.boneToEntityArr,
			rootMotionAnimStateBufferLookup = rootMotionAnimationStateBufferLookupRW,
		};

		var jh = computeAnimationsJob.ScheduleBatch(runtimeData.animatedBonesBuffer.Length, 16, dataGatherJH);
		return jh;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	JobHandle ProcessUserCurves(ref SystemState ss, JobHandle dependsOn)
	{
		var userCurveProcessJob = new ProcessUserCurvesJob();
		var jh = userCurveProcessJob.ScheduleParallel(dependsOn);
		return jh;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	JobHandle CopyEntityBonesToAnimationTransforms(ref SystemState ss, ref RuntimeAnimationData runtimeData, JobHandle dependsOn)
	{
		var rigDefinitionLookup = SystemAPI.GetComponentLookup<RigDefinitionComponent>(true);
		var parentComponentLookup = SystemAPI.GetComponentLookup<Parent>();
			
		//	Now take available entity transforms as ref poses overrides
		var copyEntityBoneTransforms = new CopyEntityBoneTransformsToAnimationBuffer()
		{
			rigDefComponentLookup = rigDefinitionLookup,
			boneTransformFlags = runtimeData.boneTransformFlagsHolderArr,
			entityToDataOffsetMap = runtimeData.entityToDataOffsetMap,
			animatedBoneTransforms = runtimeData.animatedBonesBuffer,
			parentComponentLookup = parentComponentLookup,
		};

		var jh = copyEntityBoneTransforms.ScheduleParallel(dependsOn);
		return jh;
	}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	[BurstCompile]
	public void OnUpdate(ref SystemState ss)
	{
		var entityCount = animatedObjectQuery.CalculateEntityCount();
		if (entityCount == 0) return;
		
		ref var runtimeData = ref SystemAPI.GetSingletonRW<RuntimeAnimationData>().ValueRW;

		bonePosesOffsetsArr.Resize(entityCount + 1, NativeArrayOptions.UninitializedMemory);
		var chunkBaseEntityIndices = animatedObjectQuery.CalculateBaseEntityIndexArrayAsync(ss.WorldUpdateAllocator, ss.Dependency, out var baseIndexCalcJH);
		var entitiesArr = animatedObjectQuery.ToEntityListAsync(ss.WorldUpdateAllocator, ss.Dependency, out var entityArrJH);

		var combinedJH = JobHandle.CombineDependencies(baseIndexCalcJH, entityArrJH);

		//	Define array with bone pose offsets for calculated bone poses
		var calcBoneOffsetsJH = PrepareComputationData(ref ss, chunkBaseEntityIndices, ref runtimeData, entitiesArr, combinedJH);

		//	User curve calculus
		var userCurveProcessJobHandle = ProcessUserCurves(ref ss, calcBoneOffsetsJH);

		//	Spawn jobs for animation calculation
		var computeAnimationJobHandle = AnimationCalculation(ref ss, entitiesArr, runtimeData, userCurveProcessJobHandle);

		//	Copy entities poses into animation buffer for non-animated parts
		var copyEntityTransformsIntoAnimationBufferJH = CopyEntityBonesToAnimationTransforms(ref ss, ref runtimeData, computeAnimationJobHandle);

		ss.Dependency = copyEntityTransformsIntoAnimationBufferJH;
	}
}
}
