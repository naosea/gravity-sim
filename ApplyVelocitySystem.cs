using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public partial struct ApplyGravitySystem : ISystem {
    private EntityQuery m_query;
    private NativeArray<Matter> currentMatter;
    private NativeArray<Forces> currentForces;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        m_query = new EntityQueryBuilder(Allocator.Temp)
                                   .WithAll<Matter, Forces, LocalTransform>()
                                   .Build(ref state);
        currentMatter = m_query.ToComponentDataArray<Matter>(Allocator.Persistent);
        currentForces = m_query.ToComponentDataArray<Forces>(Allocator.Persistent);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {
        if (currentMatter.IsCreated)
            currentMatter.Dispose();
        if (currentForces.IsCreated)
            currentForces.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        // Update the arrays if the entity count has changed
        if (m_query.CalculateEntityCount() != currentMatter.Length) {
            if (currentMatter.IsCreated)
                currentMatter.Dispose();
            if (currentForces.IsCreated)
                currentForces.Dispose();
            currentMatter = m_query.ToComponentDataArray<Matter>(Allocator.Persistent);
            currentForces = m_query.ToComponentDataArray<Forces>(Allocator.Persistent);
        }
        else {
            // If the count hasn't changed, just update the data
            m_query.CopyFromComponentDataArray(currentMatter);
            m_query.CopyFromComponentDataArray(currentForces);
        }
        int updatesPerFrame = 1;
        float stepSize = 0.01f;

        var updatedMatter = new NativeArray<Matter>(currentMatter.Length, Allocator.TempJob);
        var updatedForces = new NativeArray<Forces>(currentForces.Length, Allocator.TempJob);
        for (int step = 0; step < updatesPerFrame; step++) {
            UpdateGravityJob updateGravityJob = new UpdateGravityJob() {
                currentMatter = currentMatter,
                currentForces = currentForces,
                updatedMatter = updatedMatter,
                updatedForces = updatedForces,
                stepSize = stepSize
            };
            state.Dependency = updateGravityJob.ScheduleParallel(currentMatter.Length, 32, state.Dependency);
            state.Dependency.Complete();
            // Swap arrays for next iteration
            (currentMatter, updatedMatter) = (updatedMatter, currentMatter);
            (currentForces, updatedForces) = (updatedForces, currentForces);
        }
        m_query.CopyFromComponentDataArray(currentMatter);
        m_query.CopyFromComponentDataArray(currentForces);
        UpdateTransformJob updateTransformJob = new UpdateTransformJob() {};
        state.Dependency = updateTransformJob.ScheduleParallel(m_query, state.Dependency);
        updatedMatter.Dispose();
        updatedForces.Dispose();
    }

    [BurstCompile]
    public partial struct UpdateGravityJob : IJobFor {
        [ReadOnly] public NativeArray<Matter> currentMatter;
        [ReadOnly] public NativeArray<Forces> currentForces;
        public NativeArray<Matter> updatedMatter;
        public NativeArray<Forces> updatedForces;
        [ReadOnly] public float stepSize;

        public void Execute(int index) {
            Matter thisMatter = currentMatter[index];
            float2 totalGravity = float2.zero;
            int batchsize = 4;
            for (int i = 0; i < currentMatter.Length; i+=batchsize) {
                float2x4 differenceBatch = float2x4.zero;
                float4 squaredBatch = new float4(1f, 1f, 1f, 1f);
                float4 forceBatch = new float4(1f, 1f, 1f, 1f);
                float4 batchMask = float4.zero;
                for (int b = 0; b < batchsize; b++) {
                    if (i + b == index) continue;
                    if (i + b >= currentMatter.Length) continue;
                    Matter otherMatter = currentMatter[i + b];
                    differenceBatch[b] = otherMatter.m_position - thisMatter.m_position;
                    squaredBatch[b] = math.lengthsq(differenceBatch[b]);
                    forceBatch[b] = otherMatter.m_mass / squaredBatch[b];
                    batchMask[b] = 1f;
                }
                float4 distanceBatch = math.sqrt(squaredBatch);
                distanceBatch = math.select(distanceBatch, -thisMatter.m_radius, distanceBatch < thisMatter.m_radius);
                float4 gravityBatch = forceBatch / distanceBatch;
                gravityBatch *= batchMask;
                totalGravity += math.mul(differenceBatch, gravityBatch);
            }
            Forces newForces = new Forces();
            newForces.m_gravitation = totalGravity * 1f; //gravitational cosntant
            newForces.m_velocity = currentForces[index].m_velocity + newForces.m_gravitation * stepSize;
            updatedForces[index] = newForces;
            Matter newMatter = new Matter();
            newMatter.m_position = currentMatter[index].m_position + updatedForces[index].m_velocity * stepSize;
            newMatter.m_mass = currentMatter[index].m_mass;
            newMatter.m_radius = currentMatter[index].m_radius;
            updatedMatter[index] = newMatter;
        }
    }

    [BurstCompile]
    [WithAll(typeof(Matter))]
    public partial struct UpdateTransformJob : IJobEntity {
        public void Execute(ref LocalTransform io_transform, in Matter i_matter) {
            io_transform.Position = new float3(i_matter.m_position, 0f);
        }
    }
}