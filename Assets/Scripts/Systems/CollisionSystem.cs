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
        public class CollisionSystemBarrier : BarrierSystem { }

        // Define a ComponentGroup for ScoreBox entities
        ComponentGroup ScoreBoxGroup;
        CollisionSystemBarrier Barrier;

        protected override void OnCreateManager()
        {
            // Get or Create the BarrierSystem
            Barrier = World.Active.GetOrCreateManager<CollisionSystemBarrier>();

            // Query for ScoreBoxes with following components
            var scoreBoxQuery = new EntityArchetypeQuery
            {
                All = new ComponentType[] { typeof(ScoreBox), typeof(Position) }
            };

            // Get the ComponentGroup
            ScoreBoxGroup = GetComponentGroup(scoreBoxQuery);
        }

        [BurstCompile]
        struct CollisionJob : IJobProcessComponentData<Player, Position>
        {
            // Access to the EntityCommandBuffer to Destroy entity
            [ReadOnly] public EntityCommandBuffer CommandBuffer;

            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Position> ScoreBoxPositions;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ScoreBox> ScoreBoxes;

            public void Execute(ref Player player, [ReadOnly] ref Position position)
            {
                float dist = 0.0f;

                for (int i = 0; i < ScoreBoxPositions.Length; i++)
                {
                    // Calculate the distance between the ScoreBox and Player
                    dist = math.distance(position.Value, ScoreBoxPositions[i].Value);

                    // If close enough for collision, add to the score and destroy the entity
                    if (dist < 2.0f)
                    {
                        player.Score += 1;
                        CommandBuffer.DestroyEntity(ScoreBoxes[i].entity);
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<Position> scoreBoxPosition = ScoreBoxGroup.ToComponentDataArray<Position>(Allocator.TempJob);
            NativeArray<ScoreBox> scoreBox = ScoreBoxGroup.ToComponentDataArray<ScoreBox>(Allocator.TempJob);

            CollisionJob collisionJob = new CollisionJob
            {
                ScoreBoxPositions = scoreBoxPosition,
                ScoreBoxes = scoreBox,
                CommandBuffer = Barrier.CreateCommandBuffer(),
            };

            JobHandle collisionJobHandle = collisionJob.Schedule(this, inputDeps);

            // Pass final handle to Barrier to ensure dependency completion
            Barrier.AddJobHandleForProducer(collisionJobHandle);

            return collisionJobHandle;
        }
    }
}