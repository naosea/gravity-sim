using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class ForcesAuthoring : MonoBehaviour {
    public float2 m_gravitation;
    public float2 m_velocity;

    private class Baker : Baker<ForcesAuthoring> {
        public override void Bake(ForcesAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Forces {
                m_gravitation = authoring.m_gravitation,
                m_velocity = authoring.m_velocity
            });
        }
    }
}