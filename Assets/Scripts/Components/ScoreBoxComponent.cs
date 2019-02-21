using Unity.Entities;
using System;

namespace SimpleECS
{
    /*
     * Component data for a ScoreBox entity.
    */
    [Serializable]
    public struct ScoreBox : IComponentData
    {
        public Entity entity;
    }
    public class ScoreBoxProxy : ComponentDataProxy<ScoreBox> { }
}