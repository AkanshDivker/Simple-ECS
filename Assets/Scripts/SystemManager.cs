using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using Unity.Jobs;

namespace SimpleECS
{
    // Bootstrap class used to acceess MonoBehaviour while still using ECS as the core of the project.
    public class SystemManager : MonoBehaviour
    {
        #region Editor Settings

        [Header("UI Objects")]
        public Text ScoreValue;
        public Text FpsValue;

        [Header("GameObjects")]
        public GameObject MainCamera;
        public GameObject ScoreBoxPrefab;

        [Header("Settings")]
        public int ScoreBoxCount = 20;
        public float CircleRadius = 8.0f;
        public float RotationSpeed = 10.0f;

        #endregion

        // One time fix to get camera position offset
        Vector3 CameraOffset;
        bool GetOffset = false;
        float DeltaTime = 0.0f;

        EntityManager Manager;
        BeginInitializationEntityCommandBufferSystem CommandBufferSystem;

        GameObjectConversionSettings Settings;
        BlobAssetStore BlobStore;

        EntityQuery PlayerQuery;
        Entity PlayerEntity;

        private void Start()
        {
            Manager = World.DefaultGameObjectInjectionWorld.EntityManager;

            BlobStore = new BlobAssetStore();
            Settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, BlobStore);

            CommandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

            // Query for finding Player Entity
            EntityQueryDesc playerQuery = new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Player) },
            };

            // Execute query and cache the Player Entity for use
            PlayerQuery = Manager.CreateEntityQuery(playerQuery);
            PlayerEntity = PlayerQuery.GetSingletonEntity();

            // Create ScoreBoxes on level start
            AddScoreBoxes();
        }

        private void OnApplicationQuit()
        {
            BlobStore.Dispose();
        }

        private void Update()
        {
            UpdateCameraPos();

            UpdateScore();
            UpdateFPS();
        }

        // Update the camera position to follow the player ball, with an added offset
        private void UpdateCameraPos()
        {
            Vector3 playerPos = Manager.GetComponentData<Translation>(PlayerEntity).Value;

            if (!GetOffset)
            {
                CameraOffset = MainCamera.transform.position - playerPos;
                GetOffset = true;
            }

            MainCamera.transform.position = playerPos + CameraOffset;
        }

        // Update the score text in the UI according to the player's score
        private void UpdateScore()
        {
            int playerScore = Manager.GetComponentData<Player>(PlayerEntity).Score;
            ScoreValue.text = playerScore.ToString();
        }

        // Simple FPS display from Unity sample page
        private void UpdateFPS()
        {
            DeltaTime += (Time.unscaledDeltaTime - DeltaTime) * 0.1f;

            float msec = DeltaTime * 1000.0f;
            float fps = 1.0f / DeltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

            FpsValue.text = text;
        }

        // Create the ScoreBox entities and assign the corresponding components
        private void AddScoreBoxes()
        {
            // Create Entity Command Buffer for parallel writing
            var commandBuffer = CommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
            NativeArray<Entity> scoreBoxEntities = new NativeArray<Entity>(ScoreBoxCount, Allocator.TempJob);

            // Convert our ScoreBox GameObject Prefab into an Entity prefab
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(ScoreBoxPrefab, Settings);
            Manager.Instantiate(prefab, scoreBoxEntities);

            // Generate Random with default seed
            var generator = new Unity.Mathematics.Random(0x6E624EB7u);

            // Create the SpawnerJob to randomly spawn ScoreBox entities
            var spawnerJob = new SpawnerJob()
            {
                CommandBuffer = commandBuffer,
                ScoreBoxEntities = scoreBoxEntities,
                Generator = generator,
                CircleRadius = CircleRadius,
            };

            // Schedule the job to run in parallel
            JobHandle spawnerJobHandle = new JobHandle();
            JobHandle sheduleParralelJobHandle = spawnerJob.ScheduleParallel(scoreBoxEntities.Length, 64, spawnerJobHandle);

            // Ensure the job is completed
            sheduleParralelJobHandle.Complete();

            scoreBoxEntities.Dispose();
        }
    }

    // Define the SpawnerJob to spawn ScoreBox entities
    struct SpawnerJob : IJobFor
    {
        [WriteOnly]
        public EntityCommandBuffer.ParallelWriter CommandBuffer;
        [ReadOnly]
        public NativeArray<Entity> ScoreBoxEntities;
        [ReadOnly]
        public Unity.Mathematics.Random Generator;
        [ReadOnly]
        public float CircleRadius;

        // Generate a random spawn point on a circle
        private float3 GenerateBoxSpawn(float radius)
        {
            Vector3 origin = new Vector3(0.0f, 0.5f, 0.0f);

            float angle = Generator.NextFloat(0.0f, 1.0f) * 360.0f;
            float3 spawnPos = float3.zero;

            spawnPos.x = origin.x + radius * math.sin(math.radians(angle));
            spawnPos.y = origin.y;
            spawnPos.z = origin.z + radius * math.cos(math.radians(angle));

            return spawnPos;
        }

        public void Execute(int i)
        {
            // Randomly generate a score value for each ScoreBox
            int score = Generator.NextInt(1, 3);

            // Assign the points per ScoreBox and it's position on the spawn circle
            CommandBuffer.AddComponent(i, ScoreBoxEntities[i], new ScoreBox { Points = score });
            CommandBuffer.SetComponent(i, ScoreBoxEntities[i], new Translation { Value = GenerateBoxSpawn(CircleRadius) });
        }
    }
}