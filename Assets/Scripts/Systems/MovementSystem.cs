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
    * Utilizes C# Job System to process movement for Player entity based on user input.
   */
    public class MovementSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(Player))]
        struct MovementJob : IJobForEach<Translation, MoveSpeed>
        {
            public float DeltaTime;
            public float HorizontalInput;
            public float VerticalInput;

            public void Execute(ref Translation position, [ReadOnly] ref MoveSpeed moveSpeed)
            {
                position.Value += new float3(HorizontalInput, 0.0f, VerticalInput) * moveSpeed.Value * DeltaTime;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            MovementJob movementJob = new MovementJob
            {
                DeltaTime = Time.DeltaTime,
                HorizontalInput = Input.GetAxis("Horizontal"),
                VerticalInput = Input.GetAxis("Vertical"),
            };

            return movementJob.Schedule(this, inputDeps);
        }
    }
}