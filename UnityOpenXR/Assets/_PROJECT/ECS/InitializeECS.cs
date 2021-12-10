using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Rendering.HybridV2;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

public class InitializeECS : MonoBehaviour {
  [SerializeField] private Mesh mesh;
  [SerializeField] private Material material;
  [SerializeField] private int numberOfZombies = 10;
  [SerializeField] private bool useJobs = false;

  private void LongTask() {
    float value = 0f;

    for (int i = 0; i < 50000; i++) {
      value = math.sqrt(value * i);
    }

    Debug.Log("value = " + value);
  }

  private void Start() {

    EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

    EntityArchetype archetype = entityManager.CreateArchetype(
        typeof(LevelComponent),
        typeof(Translation),
        typeof(RenderMesh),
        typeof(RenderBounds),
        typeof(LocalToWorld),
        typeof(MovementComponent)
        );

    NativeArray<Entity> entityArray = new NativeArray<Entity>(numberOfZombies, Allocator.Temp);

    entityManager.CreateEntity(archetype, entityArray);

    for (int i = 0; i < entityArray.Length; i++) {
      Entity entity = entityArray[i];

      entityManager.SetComponentData(entity,
                                     new LevelComponent { level = UnityEngine.Random.Range(10, 20) });

      entityManager.SetSharedComponentData(
          entity, new RenderMesh {
            mesh = mesh,
            material = material
          });

      entityManager.SetComponentData(entity,
        new MovementComponent { moveSpeed = UnityEngine.Random.Range(1.0f, 5.0f) });

      entityManager.SetComponentData(entity,
        new Translation { Value = new float3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-8f, 8f)) });


    }

    entityArray.Dispose();
  }

  private void Update() {
    float startTime = Time.realtimeSinceStartup;

    if (useJobs) {
      NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.Temp);

      for (int i = 0; i < 10; i++) {
        JobHandle jobHandle = LongTaskJob();
        jobHandles.Add(jobHandle);
      }

      JobHandle.CompleteAll(jobHandles);
      jobHandles.Dispose();

    } else {
      for(int i = 0; i < 10; i++) {
        LongTask();
      }
    }

    Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
  }

  private JobHandle LongTaskJob() {
    LongTask job = new LongTask();
    return job.Schedule();
  }
}

[BurstCompile]
public struct LongTask : IJob {
  public void Execute() {
    float value = 0f;

    for (int i = 0; i < 50000; i++) {
      value = math.sqrt(value * i);
    }

  }
}
