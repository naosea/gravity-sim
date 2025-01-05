using Unity.Entities;
using Unity.Mathematics;

public struct Matter : IComponentData {
    public float2 m_position;
    public float m_mass;
    public float m_radius;
}