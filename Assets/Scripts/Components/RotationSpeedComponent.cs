using Unity.Entities;

namespace SimpleECS
{
    // Component data for rotation speed of an entity.
    [GenerateAuthoringComponent]
    public struct RotationSpeed : IComponentData
    {
        public float Value;
    }
}