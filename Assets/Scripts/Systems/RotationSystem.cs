using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

namespace SimpleECS
{
    /*
    * Utilizes C# Job System to process rotation for all ScoreBox entities.
   */
    public class RotationSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(ScoreBox))]
        struct RotationJob : IJobProcessComponentData<Rotation, RotationSpeed>
        {
            public float deltaTime;

            public void Execute(ref Rotation rotation, [ReadOnly] ref RotationSpeed rotationSpeed)
            {
                rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), rotationSpeed.Value * deltaTime));
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            RotationJob rotationJob = new RotationJob
            {
                deltaTime = Time.deltaTime,
            };

            return rotationJob.Schedule(this, inputDeps);
        }
    }
}