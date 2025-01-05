using Unity.Entities;
using Unity.Mathematics;

public struct Forces : IComponentData {
    public float2 m_gravitation;
    public float2 m_velocity;
}