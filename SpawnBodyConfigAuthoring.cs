using UnityEngine;
using Unity.Entities;

public class SpawnBodyConfigAuthoring : MonoBehaviour {
    public GameObject bodyPrefab;
    public int amountToSpawn;

    public class Baker: Baker<SpawnBodyConfigAuthoring> {
        public override void Bake(SpawnBodyConfigAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SpawnBodyConfig {
                bodyPrefabEntity = GetEntity(authoring.bodyPrefab, TransformUsageFlags.Dynamic),
                amountToSpawn = authoring.amountToSpawn,
            });
        }
    }
}

public struct SpawnBodyConfig : IComponentData {
    public Entity bodyPrefabEntity;
    public int amountToSpawn;
}