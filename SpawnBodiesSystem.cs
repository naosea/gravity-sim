using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class SpawnBodiesSystem : SystemBase {
    protected override void OnCreate() {
        RequireForUpdate<SpawnBodyConfig>();
    }

    protected override void OnUpdate() {
        this.Enabled = false;

        SpawnBodyConfig spawnBodyConfig = SystemAPI.GetSingleton<SpawnBodyConfig>();
        for (int i = 0; i < spawnBodyConfig.amountToSpawn; i++) {
            Entity spawnedEntity = EntityManager.Instantiate(spawnBodyConfig.bodyPrefabEntity);
            float2 position = new float2(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f));
            float2 velcoity = new float2(UnityEngine.Random.Range(-0.0f, 0.0f), UnityEngine.Random.Range(-0.0f, 0.0f));
            float size = UnityEngine.Random.Range(1.0f, 1.0f);
            EntityManager.SetComponentData(spawnedEntity, new Matter {
                m_position = position,
                m_mass = 1f,
                m_radius = 0.1f
            });
            EntityManager.SetComponentData(spawnedEntity, new Forces {
                m_velocity = velcoity,
            });
            EntityManager.SetComponentData(spawnedEntity, new LocalTransform {
                Position = new float3(position, 0f),
                Scale = 1f,
            });
        }
    }
}
