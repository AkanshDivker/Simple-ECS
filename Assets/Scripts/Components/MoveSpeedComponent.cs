using Unity.Entities;
using System;

namespace SimpleECS
{
    /*
     * Component data for movement speed of an entity.
    */
    [Serializable]
    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }

    public class MoveSpeedProxy : ComponentDataProxy<MoveSpeed> { }
}