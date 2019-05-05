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
     * Creating and removing entities can only be done inside the main thread.
     * This sytem uses an EntityCommandBuffer to handle tasks that can't be completed inside Jobs.
    */
    public class CollisionSystem : JobComponentSystem
    {
        // Define a ComponentGroup for ScoreBox entities
        EntityQuery ScoreBoxGroup;

        // BeginInitializationEntityCommandBufferSystem is used to create a command buffer that will be played back when the barreir system executes.
        BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;

        protected override void OnCreate()
        {
            m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

            // Query for ScoreBoxes with following components
            EntityQueryDesc scoreBoxQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(ScoreBox), typeof(Translation) }
            };

            // Get the ComponentGroup
            ScoreBoxGroup = GetEntityQuery(scoreBoxQuery);
        }

        [BurstCompile]
        struct CollisionJob : IJobForEach<Player, Translation>
        {
            // Access to the EntityCommandBuffer to Destroy entity
            [ReadOnly] public EntityCommandBuffer CommandBuffer;

            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Translation> ScoreBoxPositions;
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ScoreBox> ScoreBoxes;

            public void Execute(ref Player player, [ReadOnly] ref Translation position)
            {
                for (int i = 0; i < ScoreBoxPositions.Length; i++)
                {
                    // Calculate the distance between the ScoreBox and Player
                    float dist = math.distance(position.Value, ScoreBoxPositions[i].Value);

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
            NativeArray<Translation> scoreBoxPosition = ScoreBoxGroup.ToComponentDataArray<Translation>(Allocator.TempJob);
            NativeArray<ScoreBox> scoreBox = ScoreBoxGroup.ToComponentDataArray<ScoreBox>(Allocator.TempJob);

            CollisionJob collisionJob = new CollisionJob
            {
                ScoreBoxPositions = scoreBoxPosition,
                ScoreBoxes = scoreBox,
                CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer(),
            };

            JobHandle collisionJobHandle = collisionJob.Schedule(this, inputDeps);

            // Pass final handle to barrier system to ensure dependency completion
            // Tell the barrier system which job needs to be completed before the commands can be played back
            m_EntityCommandBufferSystem.AddJobHandleForProducer(collisionJobHandle);

            return collisionJobHandle;
        }
    }
}