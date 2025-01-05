using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public class MatterAuthoring : MonoBehaviour {
    public float2 m_position;
    public float m_mass;
    public float m_radius;

    private class Baker : Baker<MatterAuthoring> {
        public override void Bake(MatterAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Matter {
                m_position = authoring.m_position,
                m_mass = authoring.m_mass,
                m_radius = authoring.m_radius,
            });
        }
    }
}