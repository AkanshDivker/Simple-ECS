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
                All = new ComponentType[] { typeof(ScoreBox), typeof(Translation) },
            };

            // Get the ComponentGroup
            ScoreBoxGroup = GetEntityQuery(scoreBoxQuery);
        }

        [BurstCompile]
        struct CollisionJob : IJobForEachWithEntity<Player, Translation>
        {
            // Access to the EntityCommandBuffer to Destroy entity
            [WriteOnly] public EntityCommandBuffer.Concurrent CommandBuffer;

            // When dealing with more than one component, better to iterate through chunks
            [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
            [ReadOnly] public ArchetypeChunkComponentType<ScoreBox> ScoreBoxType;

            [ReadOnly] public ArchetypeChunkEntityType ScoreBoxEntity;

            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;

            public void Execute(Entity entity, int index, ref Player player, [ReadOnly] ref Translation position)
            {
                for (int i = 0; i < Chunks.Length; i++)
                {
                    var chunk = Chunks[i];

                    var translations = chunk.GetNativeArray(TranslationType);
                    var scoreBoxes = chunk.GetNativeArray(ScoreBoxType);
                    var scoreBoxEntities = chunk.GetNativeArray(ScoreBoxEntity);

                    for (int j = 0; j < scoreBoxes.Length; j++)
                    {
                        // Calculate the distance between the ScoreBox and Player
                        // Use squared distance value to increase performance (saves call to sqrt())
                        float dist = math.distancesq(position.Value, translations[j].Value);

                        // If close enough for collision, add to the score and destroy the entity
                        // Check the squared value of distance threshold (2^2 = 4)
                        if (dist < 4.0f)
                        {
                            player.Score += scoreBoxes[j].ScoreValue;
                            CommandBuffer.DestroyEntity(index, scoreBoxEntities[j]);
                        }
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var translationType = GetArchetypeChunkComponentType<Translation>(true);
            var scoreBoxType = GetArchetypeChunkComponentType<ScoreBox>(true);
            var scoreBoxEntity = GetArchetypeChunkEntityType();
            var chunks = ScoreBoxGroup.CreateArchetypeChunkArrayAsync(Allocator.TempJob, out var handle);

            // Create the job and add dependency
            var collisionJobHandle = new CollisionJob
            {
                CommandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                TranslationType = translationType,
                ScoreBoxType = scoreBoxType,
                ScoreBoxEntity = scoreBoxEntity,
                Chunks = chunks,
            }.Schedule(this, JobHandle.CombineDependencies(inputDeps, handle));

            // Pass final handle to barrier system to ensure dependency completion
            // Tell the barrier system which job needs to be completed before the commands can be played back
            m_EntityCommandBufferSystem.AddJobHandleForProducer(collisionJobHandle);

            return collisionJobHandle;
        }
    }
}