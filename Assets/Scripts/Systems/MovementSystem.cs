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
            public float deltaTime;
            public float horizontalInput;
            public float verticalInput;

            public void Execute(ref Translation position, [ReadOnly] ref MoveSpeed moveSpeed)
            {
                position.Value += new float3(horizontalInput, 0.0f, verticalInput) * moveSpeed.Value * deltaTime;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            MovementJob movementJob = new MovementJob
            {
                deltaTime = Time.deltaTime,
                horizontalInput = Input.GetAxis("Horizontal"),
                verticalInput = Input.GetAxis("Vertical"),
            };

            return movementJob.Schedule(this, inputDeps);
        }
    }
}