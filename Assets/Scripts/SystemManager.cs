using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;


namespace SimpleECS
{
    /*
     * Bootstrap class used to acceess MonoBehaviour while still using ECS as the core of the project.
    */
    public class SystemManager : MonoBehaviour
    {
        #region Editor Settings

        [Header("UI Objects")]
        public Text scoreValue;

        [Header("Object Prefabs")]
        public GameObject mainCamera;

        [Header("Settings")]
        public int scoreBoxCount = 20;
        public float circleRadius = 5.0f;
        public float rotationSpeed = 50.0f;

        #endregion

        // Set the GameObject prefabs to create entity prefabs
        public GameObject WallPrefab;
        public GameObject ScoreBoxPrefab;
        public GameObject GroundPrefab;
        public GameObject PlayerPrefab;

        private Vector3 origin;
        private Vector3 cameraOffset;

        // Access to player entity to update score and camera position
        private Entity PlayerEntity;

        private EntityManager Manager;

        // One time fix to get camera position offset
        private bool getOffset = false;

        private void Start()
        {
            Manager = World.Active.EntityManager;
            origin = new Vector3(0.0f, 0.5f, 0.0f);

            // Create entity prefabs for the scene
            AddGround();
            AddWalls();

            AddPlayer();
            AddScoreBoxes();
        }

        private void Update()
        {
            UpdateCameraPos();
            UpdateScore();
        }

        // Update the camera position to follow the player ball, with an added offset
        private void UpdateCameraPos()
        {
            Vector3 playerPos = Manager.GetComponentData<Translation>(PlayerEntity).Value;

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
            int playerScore = Manager.GetComponentData<Player>(PlayerEntity).Score;
            scoreValue.text = playerScore.ToString();
        }

        // Instantiate the ground for the scene
        private void AddGround()
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(GroundPrefab, World.Active);
            var instance = Manager.Instantiate(prefab);

            Manager.SetComponentData(instance, new Translation { Value = new float3(0.0f, 0.0f, 0.0f) });

            // Name the entity
            Manager.SetName(instance, "Ground Entity");
        }

        // Instantiate the environment prefabs in the scene
        private void AddWalls()
        {
            // 4 Walls in Total
            NativeArray<Entity> wallEntities = new NativeArray<Entity>(4, Allocator.Persistent);

            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(WallPrefab, World.Active);
            Manager.Instantiate(prefab, wallEntities);

            // Set component data for each wall
            Manager.SetComponentData(wallEntities[0], new Translation { Value = new float3(-15.0f, 0.5f, 0.0f) });
            Manager.AddComponentData(wallEntities[0], new NonUniformScale { Value = new float3(1.0f, 1.0f, 31.0f) });

            Manager.SetComponentData(wallEntities[1], new Translation { Value = new float3(15.0f, 0.5f, 0.0f) });
            Manager.AddComponentData(wallEntities[1], new NonUniformScale { Value = new float3(1.0f, 1.0f, 31.0f) });

            Manager.SetComponentData(wallEntities[2], new Translation { Value = new float3(0.0f, 0.5f, -15.0f) });
            Manager.AddComponentData(wallEntities[2], new NonUniformScale { Value = new float3(30.0f, 1.0f, 1.0f) });

            Manager.SetComponentData(wallEntities[3], new Translation { Value = new float3(0.0f, 0.5f, 15.0f) });
            Manager.AddComponentData(wallEntities[3], new NonUniformScale { Value = new float3(30.0f, 1.0f, 1.0f) });

            // Name each entity
            for (int i = 0; i < wallEntities.Length; i++)
            {
                Manager.SetName(wallEntities[i], "Wall Entity " + i);
            }

            wallEntities.Dispose();
        }

        // Create the Player entity and assign the corresponding components
        private void AddPlayer()
        {
            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(PlayerPrefab, World.Active);
            var instance = Manager.Instantiate(prefab);

            Manager.AddComponentData(instance, new Player { Score = 0, entity = instance });
            Manager.SetComponentData(instance, new Translation { Value = new float3(0.0f, 0.5f, 0.0f) });
            Manager.AddComponentData(instance, new MoveSpeed { Value = 15.0f });

            // Name the entity
            Manager.SetName(instance, "Player Entity");

            PlayerEntity = instance;
        }

        // Create the ScoreBox entities and assign the corresponding components
        private void AddScoreBoxes()
        {
            NativeArray<Entity> scoreBoxEntities = new NativeArray<Entity>(scoreBoxCount, Allocator.Temp);

            Entity prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(ScoreBoxPrefab, World.Active);
            Manager.Instantiate(prefab, scoreBoxEntities);

            for (int i = 0; i < scoreBoxCount; i++)
            {
                Manager.AddComponentData(scoreBoxEntities[i], new ScoreBox { entity = scoreBoxEntities[i] });
                Manager.SetComponentData(scoreBoxEntities[i], new Translation { Value = GenerateBoxSpawn(circleRadius) });
                Manager.SetComponentData(scoreBoxEntities[i], new Rotation { Value = new quaternion(0.0f, 0.0f, 0.0f, 1.0f) });
                Manager.AddComponentData(scoreBoxEntities[i], new RotationSpeed { Value = rotationSpeed });
            }

            // Name each entity
            for (int i = 0; i < scoreBoxEntities.Length; i++)
            {
                Manager.SetName(scoreBoxEntities[i], "ScoreBox Entity " + i);
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
    }
}