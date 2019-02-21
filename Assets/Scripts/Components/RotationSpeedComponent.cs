using Unity.Entities;
using System;

namespace SimpleECS
{
    /*
     * Component data for rotation speed of an entity.
    */
    [Serializable]
    public struct RotationSpeed : IComponentData
    {
        public float Value;
    }
    public class RotationSpeedProxy : ComponentDataProxy<RotationSpeed> { }
}