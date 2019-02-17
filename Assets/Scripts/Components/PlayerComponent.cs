using Unity.Entities;
using Unity.Mathematics;
using System;

namespace SimpleECS
{
    /*
     * Component data for a Player entity.
    */
    [Serializable]
    public struct Player : IComponentData
    {
        public int Score;
        public Entity entity;
    }

    public class PlayerComponent : ComponentDataWrapper<Player> { }
}