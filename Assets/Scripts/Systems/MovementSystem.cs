using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace SimpleECS
{
    [AlwaysSynchronizeSystem]
    public class MovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Entities
                .WithBurst()
                .WithAll<Player>()
                .ForEach((ref PhysicsVelocity velocity, in MoveSpeed moveSpeed) =>
                {
                    float3 currentVelocity = velocity.Linear;
                    float3 inputVelocity = new float3(horizontalInput, 0.0f, verticalInput);

                    currentVelocity += inputVelocity * moveSpeed.Value * deltaTime;

                    velocity.Linear = currentVelocity;
                })
                .Run();
        }
    }
}