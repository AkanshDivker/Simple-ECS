# Simple ECS

This is a simple project that utilizes the core features of the new Entity Component System (ECS) that is currently in preview for Unity. In the project you move a player controlled ball around to collect score boxes. Everything is processed using ECS and the C# Job System.

Since ECS is still under development, I will keep this project updated as things change and possibly become deprecated, as well as add new functionality.

![](https://i.imgur.com/wi6SvDe.gif)

**Functionality**
- Implements the Entity Component System
- Implements the C# Job System
- Networked code for ECS and C# Job System [TO-DO]

### Changelog
**Feb 20, 2019**
- Updated project for Entities 0.0.12-preview.24
- Updated project for Hybrid Renderer 0.0.1-preview.4
- Removed Inject as it is deprecated
- ComponentDataWrapper updated to ComponentDataProxy
- Converted all Prefabs to Prototypes

## Getting Started

You can get started by cloning the repository on your desktop. Since ECS is in preview, you will need to follow some prerequisite steps in order to get things running.

### Prerequisites

I recommend using Unity 2018.3 or later for this project. You will also need the following packages, which can be added through the **Package Manager** window.

```
Entities (com.unity.entities)
Hybrid Renderer (com.unity.rendering.hybrid)
```

### Scripting

The main libraries that will be required for Entities, Job System, and Rendering are as follows.

```c#
using Unity.Entities;
using Unity.Rendering;
using Unity.Jobs;
```

In addition, the following libraries are also used in this project.

```c#
using Unity.Mathematics;
using Unity.Burst;
```

The `Unity.Burst` library is required in order to use the Burst Compiler for the C# Job System.

## Opening the Project

Once you have downloaded the repository to your computer, you can open up the DemoScene located in the Scenes folder.

### Prototypes

The Prototypes folder contains empty prefab objects which only contain a MeshRenderProxy. They are placed into the scene as prototypes. The data is then extracted and the object is deleted.

The Prototypes folder contains prototypes for the environment objects, Player, and ScoreBox.

![](https://i.imgur.com/rcVBIzm.png)

### Scripts

The Scripts folder is divided into two sub-folders. One for Components, which are simply containers for data, and the other for Systems which perform the functionality. The `SystemManager` script is used as a Bootstrap script.

![](https://i.imgur.com/rgBCCAd.png)

### Hierarchy

The project hierarchy contains some simple standard components. A directional light, canvas, and the camera being the most standard. In addition, we have an empty game object that contains our `SystemManager` script. The prototypes for the scene are added and placed under the empty Prototypes GameObject for organization.

![](https://i.imgur.com/qowOlft.png)

## Entity Component System (ECS)

The traditional (or classic) way of working in Unity requires the use of MonoBehaviours on objects to perform some behaviour. They contain both the data and the logic. The problem that is build into the classic system is that the data becomes very scattered in memory. The loading time from memory to cache is very slow, and causes a lot of pointer misses. 

With ECS, we can focus on processing only the data we need. There is no extra data that is processed, and it can be done in more than one thread.

### C# Job System

ECS and the Job System perform very well together, giving performance by default in your code. ECS separates the data from the logic, by putting it into data containers called **Components**, and functionality into **Systems**.

```c#
    namespace SimpleECS
    {
        /*
         * Component data for movement speed of an entity.
        */
        [Serializable]
        public struct MoveSpeed : IComponentData
        {
			// Only contains data
            public float Value;
        }
    
        public class MoveSpeedComponent : ComponentDataProxy<MoveSpeed> { }
    }
```
### ECS

Using ECS requires a different way of thinking than the traditional object oriented system. The way you think about objects and Unity should change. An entity isn't a container, it is just a reference to data. 

Systems contain all of the functionality. They process entities based on a filter, and provide performance by default.

```c#
    namespace SimpleECS
    {
        /*
        * Utilizes C# Job System to process rotation for all ScoreBox entities.
       */
        public class RotationSystem : JobComponentSystem
        {
            [BurstCompile]
            [RequireComponentTag(typeof(ScoreBox))]
    		// Filter for entities with the Rotation and RotationSpeed components, also require them to have a ScoreBox component (but we don't need this data to be processed)
            struct RotationJob : IJobProcessComponentData<Rotation, RotationSpeed>
            {
                public float deltaTime;
    
    			// Process the data for the given entity. [ReadOnly] value used for RotationSpeed as it will not be modified.
                public void Execute(ref Rotation rotation, [ReadOnly] ref RotationSpeed rotationSpeed)
                {
                    rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(math.up(), rotationSpeed.Value * deltaTime));
                }
            }
    
    		// JobHandle for our RotationJob
            protected override JobHandle OnUpdate(JobHandle inputDeps)
            {
                RotationJob rotationJob = new RotationJob
                {
    				// Provide deltaTime to the job execution
                    deltaTime = Time.deltaTime,
                };
    
    			// Schedule the job to run and return the JobHandle
                return rotationJob.Schedule(this, inputDeps);
            }
        }
    }
```

### Final Notes

The project is commented well and should provide detailed explanation on using ECS and the Job System. I think ECS is definitely something that should be transitioned to, as the benefits it offers simply cannot be overlooked. Looking forward to learn more as ECS development progresses. Also, here is a sample of the project running with 50,000 ScoreBox entities. I am pretty sure more could have been handled!

![](https://i.imgur.com/0gI59F1.gif)

## Authors

* **Akansh Divker** - *Project creation* - [AkanshDivker](https://github.com/AkanshDivker)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
