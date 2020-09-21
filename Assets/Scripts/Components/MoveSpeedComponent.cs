using Unity.Entities;

namespace SimpleECS
{
    // Component data for movement speed of an entity.
    [GenerateAuthoringComponent]
    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }
}