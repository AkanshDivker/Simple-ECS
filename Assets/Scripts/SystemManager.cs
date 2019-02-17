using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine.UI;


namespace SimpleECS
{
    /*
     * Bootstrap class used to acceess MonoBehaviour while still using ECS as the core of the project.
    */
    public class SystemManager : MonoBehaviour
    {
        #region Editor Settings

        [Header("Environment Prefabs")]
        public GameObject groundPrefab;
        public GameObject leftWallPrefab;
        public GameObject rightWallPrefab;
        public GameObject topWallPrefab;
        public GameObject bottomWallPrefab;

        [Header("UI Objects")]
        public Text scoreValue;

        [Header("Object Prefabs")]
        public GameObject mainCamera;

        [Header("Settings")]
        public int scoreBoxCount = 20;
        public float circleRadius = 5.0f;
        public float rotationSpeed = 50.0f;

        #endregion

        private EntityManager manager;

        private EntityArchetype scoreBoxArchetype;
        private EntityArchetype playerArchetype;

        private RenderMesh scoreBoxMesh;
        private RenderMesh playerMesh;

        private Entity player;

        private Vector3 origin;
        private Vector3 cameraOffset;

        // One time fix to get camera position offset
        private bool getOffset = false;

        private void Start()
        {
            // Find the entity manager, or create one if it does not already exist
            manager = World.Active.GetOrCreateManager<EntityManager>();

            origin = new Vector3(0.0f, 0.5f, 0.0f);

            // Create entity archetypes with their corresponding components and get their mesh
            scoreBoxArchetype = manager.CreateArchetype(typeof(ScoreBox), typeof(Position), typeof(Rotation), typeof(RotationSpeed), typeof(Scale));
            scoreBoxMesh = GetMeshFromPrototype("ScoreBox");

            playerArchetype = manager.CreateArchetype(typeof(Player), typeof(Position), typeof(Scale), typeof(MoveSpeed));
            playerMesh = GetMeshFromPrototype("PlayerBall");

            // Add remaining environment by using prefabs assigned through the editor
            AddEnvironment();
            AddScoreBoxes();
            AddPlayer();
        }

        private void Update()
        {
            UpdateCameraPos();
            UpdateScore();
        }

        // Update the camera position to follow the player ball, with an added offset
        private void UpdateCameraPos()
        {
            Vector3 playerPos = manager.GetComponentData<Position>(player).Value;

            if (!getOffset)
            {
                cameraOffset = (mainCamera.transform.position - playerPos);
                getOffset = true;
            }

            mainCamera.transform.position = playerPos + cameraOffset;
        }

        // Update the score text in the UI according to the player's score
        private void UpdateScore()
        {
            int playerScore = manager.GetComponentData<Player>(player).Score;
            scoreValue.text = playerScore.ToString();
        }

        // Instantiate the environment prefabs in the scene
        private void AddEnvironment()
        {
            NativeArray<Entity> environmentEntities = new NativeArray<Entity>(5, Allocator.Persistent);

            manager.Instantiate(groundPrefab, environmentEntities);
            manager.Instantiate(leftWallPrefab, environmentEntities);
            manager.Instantiate(rightWallPrefab, environmentEntities);
            manager.Instantiate(topWallPrefab, environmentEntities);
            manager.Instantiate(bottomWallPrefab, environmentEntities);

            environmentEntities.Dispose();
        }

        // Create the Player entity and assign the corresponding components
        private void AddPlayer()
        {
            player = manager.CreateEntity(playerArchetype);

            manager.SetComponentData(player, new Player { Score = 0, entity = player });
            manager.SetComponentData(player, new Position { Value = new float3(0.0f, 0.5f, 0.0f) });
            manager.SetComponentData(player, new Scale { Value = new float3(1.0f, 1.0f, 1.0f) });
            manager.SetComponentData(player, new MoveSpeed { Value = 15.0f });
            manager.AddSharedComponentData(player, playerMesh);
        }

        // Create the ScoreBox entities and assign the corresponding components
        private void AddScoreBoxes()
        {
            NativeArray<Entity> scoreBoxEntities = new NativeArray<Entity>(scoreBoxCount, Allocator.Temp);
            manager.CreateEntity(scoreBoxArchetype, scoreBoxEntities);

            for (int i = 0; i < scoreBoxCount; i++)
            {
                manager.SetComponentData(scoreBoxEntities[i], new ScoreBox { entity = scoreBoxEntities[i] });
                manager.SetComponentData(scoreBoxEntities[i], new Position { Value = GenerateBoxSpawn(circleRadius) });
                manager.SetComponentData(scoreBoxEntities[i], new Scale { Value = new float3(1.0f, 1.0f, 1.0f) });
                manager.SetComponentData(scoreBoxEntities[i], new Rotation { Value = new quaternion(0.0f, 0.0f, 0.0f, 1.0f) });
                manager.SetComponentData(scoreBoxEntities[i], new RotationSpeed { Value = rotationSpeed });
                manager.AddSharedComponentData(scoreBoxEntities[i], scoreBoxMesh);
            }

            scoreBoxEntities.Dispose();
        }

        // Generate a random spawn point on a circle
        private float3 GenerateBoxSpawn(float radius)
        {
            float angle = UnityEngine.Random.Range(0.0f, 1.0f) * 360.0f;
            float3 spawnPos = float3.zero;

            spawnPos.x = origin.x + radius * math.sin(math.radians(angle));
            spawnPos.y = origin.y;
            spawnPos.z = origin.z + radius * math.cos(math.radians(angle));

            return spawnPos;
        }

        // Get the corresponding RenderMesh for a given prototype in the scene
        private static RenderMesh GetMeshFromPrototype(string prototypeName)
        {
            GameObject prototype = GameObject.Find(prototypeName);
            RenderMesh prototypeMesh = prototype.GetComponent<RenderMeshComponent>().Value;

            // Destroy the prototype in the scene after data extracted
            Destroy(prototype);

            return prototypeMesh;
        }
    }
}