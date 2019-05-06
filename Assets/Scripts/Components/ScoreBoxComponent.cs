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
        public int ScoreValue;
    }
}