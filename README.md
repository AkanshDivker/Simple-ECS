# Simple ECS

This is a simple project that utilizes the core features of the new Entity Component System (ECS) that is currently in preview for Unity. In the project you move a player controlled ball around to collect score boxes. Everything is processed using DOTS and the C# Job System.

Since ECS is still under development, I will keep this project updated as things change and possibly become deprecated, as well as add new functionality.

![](https://i.imgur.com/wi6SvDe.gif)

**Functionality**
- Implements the Entity Component System
- Implements the C# Job System
- Implements Unity Physics Collisions and Movement with C# Job System
- Implements Hybrid Renderer
- Implements Burst Compiler

**Planned Features**
- Add NetCode support
- Convert More Systems to DOTS (Input, Camera, UI)
- Add More Game Play Functionality

### Changes

To view a log of all the previous changes DOTS and this project have gone through, please view the [CHANGES.md](CHANGES.md) file for details.

## Getting Started

You can get started by cloning the repository on your desktop. Since ECS is in preview, you will need to follow some prerequisite steps in order to get things running.

### Prerequisites

This project requires Unity 2020.2 or later. You will also need the following packages, which can be added through the **Package Manager** window.

```
Entities (com.unity.entities)
Hybrid Renderer (com.unity.rendering.hybrid)
Mathematics (com.unity.mathematics)
Physics (com.unity.physics)
```

## Opening the Project

Once you have downloaded the repository to your computer, you can open up the DemoScene located in the Scenes folder.

### Prefabs

The Prefabs folder contains game objects with Mesh Render and Mesh Filter scripts added to them. These game objects are used to be converted into entity prefabs by using the ConvertToEntity workflow.

The Prefabs folder contains prefabs for the environment objects, Player, and ScoreBox.

![](https://i.imgur.com/kEGMLhf.png)

### Scripts

The Scripts folder is divided into two sub-folders. One for Components, which are simply containers for data, and the other for Systems which perform the functionality. The `SystemManager` script is used as a Bootstrap script.

![](https://i.imgur.com/rgBCCAd.png)

### Hierarchy

The project hierarchy contains some simple and standard components. Most of these objects are converted to entities automatically through the ConvertToEntity workflow.

![](https://i.imgur.com/XZHDuki.png)

## Entity Component System (ECS)

The traditional (or classic) way of working in Unity requires the use of MonoBehaviours on objects to perform some behaviour. They contain both the data and the logic. The problem that is build into the classic system is that the data becomes very scattered in memory. The loading time from memory to cache is very slow, and causes a lot of pointer misses. 

With ECS, we can focus on processing only the data we need. There is no extra data that is processed, and it can be done in more than one thread.

### C# Job System

ECS and the Job System perform very well together, giving performance by default in your code. ECS separates the data from the logic, by putting it into data containers called **Components**, and functionality into **Systems**.

```c#
namespace SimpleECS
{
    // Component data for movement speed of an entity.
    [GenerateAuthoringComponent]
    public struct MoveSpeed : IComponentData
    {
        public float Value;
    }
}
```
### ECS

Using ECS requires a different way of thinking than the traditional object oriented system. The way you think about objects and Unity should change. An entity isn't a container, it is just a reference to data. 

Systems contain all of the functionality. They process entities based on a filter, and provide performance by default.

```c#
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
```

### Final Notes

The project is commented well and should provide detailed explanation on using ECS and the Job System. I think ECS is definitely something that should be transitioned to, as the benefits it offers simply cannot be overlooked. Looking forward to learn more as ECS development progresses. Also, here is a sample of the project running with 50,000 ScoreBox entities. I am pretty sure more could have been handled!

![](https://i.imgur.com/0gI59F1.gif)

## Authors

* **Akansh Divker** - *Project creation* - [AkanshDivker](https://github.com/AkanshDivker)

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
