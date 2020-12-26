using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using UnityEngine;

namespace SimpleECS
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class MovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            // Input (old system) from UnityEngine since DOTS native input system doesn't exist yet
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Entities
                .WithBurst()
                .WithAll<Player>()
                .ForEach((ref PhysicsVelocity velocity, ref PhysicsMass physicsMass, in Movement movement) =>
                {
                    // Set direction of impulse based on player input
                    float3 direction = new float3(horizontalInput, 0.0f, verticalInput);

                    // Apply Linear Impulse from Physics Extension methods
                    PhysicsComponentExtensions.ApplyLinearImpulse(ref velocity, physicsMass, direction * movement.Force);
                })
                .Run();
        }
    }
}
