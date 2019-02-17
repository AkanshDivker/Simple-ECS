using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

namespace SimpleECS
{
    /*
     * Utilizes C# Job System to process collisions between Player and ScoreBox entities.
    */
    public class CollisionSystem : JobComponentSystem
    {
        // BarrierSystem definition in order to access the EntityCommandBuffer
        public class Barrier : BarrierSystem { }

        // Filter data for ScoreBox entities with components
        public struct ScoreBoxGroup
        {
            [ReadOnly] public ComponentDataArray<Position> position;
            [ReadOnly] public ComponentDataArray<ScoreBox> scoreBox;
        }

        [Inject] [ReadOnly] private ScoreBoxGroup scoreBoxGroup;
        [Inject] [ReadOnly] private Barrier barrier;

        [BurstCompile]
        struct CollisionJob : IJobProcessComponentData<Player, Position>
        {
            [ReadOnly] public EntityCommandBuffer buffer;
            [ReadOnly] public ComponentDataArray<Position> scoreBoxPosition;
            [ReadOnly] public ComponentDataArray<ScoreBox> scoreBox;

            public void Execute(ref Player player, [ReadOnly] ref Position position)
            {
                float dist = 0.0f;

                for (int i = 0; i < scoreBoxPosition.Length; i++)
                {
                    // Calculate the distance between the ScoreBox and Player
                    dist = math.distance(position.Value, scoreBoxPosition[i].Value);

                    // If close enough for collision, add to the score and destroy the entity
                    if (dist < 2.0f)
                    {
                        player.Score += 1;
                        buffer.DestroyEntity(scoreBox[i].entity);
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            CollisionJob collisionJob = new CollisionJob
            {
                scoreBoxPosition = scoreBoxGroup.position,
                scoreBox = scoreBoxGroup.scoreBox,
                buffer = barrier.CreateCommandBuffer(),
            };

            return collisionJob.Schedule(this, inputDeps);
        }
    }
}