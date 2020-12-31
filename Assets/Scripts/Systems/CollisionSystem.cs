using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics.Systems;
using Unity.Physics;
using Unity.Burst;

namespace SimpleECS
{
    // Utilizes C# Job System to process collisions between Player and ScoreBox entities.
    // Creating and removing entities can only be done inside the main thread.
    // This system uses an EntityCommandBuffer to handle tasks that can't be completed inside Jobs.
    // Runs after physics/simulation system is done processing.
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class CollisionSystem : SystemBase
    {
        // Physics references
        BuildPhysicsWorld BuildPhysicsWorldSystem;
        StepPhysicsWorld StepPhysicsWorldSystem;

        // EndFixedStepSimulationEntityCommandBufferSystem is used to create a command buffer that will be played back when the barrier system executes.
        EndFixedStepSimulationEntityCommandBufferSystem EndFixedStepSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
            EndFixedStepSimulationEcbSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        }

        // Job struct for handling collision response as trigger
        [BurstCompile]
        struct ScoreBoxCollisionEventJob : ITriggerEventsJob
        {
            [WriteOnly]
            public EntityCommandBuffer CommandBuffer;
            [ReadOnly]
            public ComponentDataFromEntity<ScoreBox> ScoreBoxGroup;
            public ComponentDataFromEntity<Player> PlayerGroup;

            public void Execute(TriggerEvent triggerEvent)
            {
                Entity entityA = triggerEvent.EntityA;
                Entity entityB = triggerEvent.EntityB;

                // Identify which colliding bodies contain which components
                bool isBodyAScoreBox = ScoreBoxGroup.HasComponent(entityA);
                bool isBodyBScoreBox = ScoreBoxGroup.HasComponent(entityB);

                // If both colliding bodies have ScoreBox components, do nothing
                if (isBodyAScoreBox && isBodyBScoreBox)
                    return;

                bool isBodyAPlayer = PlayerGroup.HasComponent(entityA);
                bool isBodyBPlayer = PlayerGroup.HasComponent(entityB);

                // If both colliding bodies have Player components, do nothing
                if (isBodyAPlayer && isBodyBPlayer)
                    return;

                // Depending on which body is which, update the player score and destroy the ScoreBox entity
                if (isBodyAScoreBox && isBodyBPlayer)
                {
                    var scoreBoxComponent = ScoreBoxGroup[entityA];
                    var playerComponent = PlayerGroup[entityB];

                    playerComponent.Score += scoreBoxComponent.Points;
                    PlayerGroup[entityB] = playerComponent;

                    CommandBuffer.DestroyEntity(triggerEvent.EntityA);
                }
                else if (isBodyBScoreBox && isBodyAPlayer)
                {
                    var scoreBoxComponent = ScoreBoxGroup[entityB];
                    var playerComponent = PlayerGroup[entityA];

                    playerComponent.Score += scoreBoxComponent.Points;
                    PlayerGroup[entityA] = playerComponent;

                    CommandBuffer.DestroyEntity(triggerEvent.EntityB);
                }
            }
        }

        protected override void OnUpdate()
        {
            var commandBuffer = EndFixedStepSimulationEcbSystem.CreateCommandBuffer();

            Dependency = new ScoreBoxCollisionEventJob()
            {
                CommandBuffer = commandBuffer,
                ScoreBoxGroup = GetComponentDataFromEntity<ScoreBox>(true),
                PlayerGroup = GetComponentDataFromEntity<Player>()
            }
            .Schedule(StepPhysicsWorldSystem.Simulation, ref BuildPhysicsWorldSystem.PhysicsWorld, this.Dependency);

            // Add the job as a dependency
            EndFixedStepSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}