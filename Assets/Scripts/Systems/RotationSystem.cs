using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;

namespace SimpleECS
{
    // System to apply a changing rotation to Entities with a RotationSpeed component
    [AlwaysSynchronizeSystem]
    public class RotationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = Time.DeltaTime;

            Entities
                .WithBurst()
                .ForEach((ref Rotation rotation, in RotationSpeed rotationSpeed) =>
                {
                    rotation.Value = math.mul(rotation.Value, quaternion.RotateY(deltaTime * rotationSpeed.Value));

                })
                .ScheduleParallel();
        }
    }
}