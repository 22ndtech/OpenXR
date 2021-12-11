using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
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
  [SerializeField] private bool useParallelJobs = false;
  [SerializeField] private Transform pfZombie;
  [SerializeField] private int jobBatchSize = 100;
  [SerializeField] private bool useParallellTransformJob = false;

  private class Zombie {
    public Transform transform;
    public float moveY;
  }

  private List<Zombie> zombieList;

  private void LongTask() {
    float value = 0f;

    for (int i = 0; i < 50000; i++) {
      value = math.sqrt(value * i);
    }

    Debug.Log("value = " + value);
  }

  private void Start() {

    zombieList = new List<Zombie>();
    for (int i = 0; i < 1000; i++) {
      Transform zombieTransform = Instantiate(pfZombie, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
      zombieList.Add(new Zombie {
        transform = zombieTransform,
        moveY = UnityEngine.Random.Range(1f, 2f)
      });
    };

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

    if (useParallellTransformJob) {
      TransformAccessArray taa = new TransformAccessArray(zombieList.Count);
      NativeArray<float> moveYValues = new NativeArray<float>(zombieList.Count, Allocator.TempJob);

      for (int i = 0; i < zombieList.Count; i++) {
        taa.Add(zombieList[i].transform);
        moveYValues[i] = zombieList[i].moveY;
      }

      LongParalellJobTransform parallelTransformJob = new LongParalellJobTransform {
        deltaTime = Time.deltaTime,
        moveYValues = moveYValues
      };

      JobHandle jobHandle = parallelTransformJob.Schedule(taa);
      jobHandle.Complete();

      for (int i = 0; i < zombieList.Count; i++) {
        zombieList[i].moveY = moveYValues[i];
      }

      taa.Dispose();
      moveYValues.Dispose();

    } else if (useParallelJobs) {
      NativeArray<float3> positions = new NativeArray<float3>(zombieList.Count, Allocator.TempJob);
      NativeArray<float> moveYValues = new NativeArray<float>(zombieList.Count, Allocator.TempJob);

      for (int i = 0; i < zombieList.Count; i++) {
        positions[i] = zombieList[i].transform.position;
        moveYValues[i] = zombieList[i].moveY;
      }

      LongParallelJobFor parallelJob = new LongParallelJobFor {
        deltaTime = Time.deltaTime,
        positions = positions,
        moveYValues = moveYValues
      };

      JobHandle jobHandle = parallelJob.Schedule(zombieList.Count, jobBatchSize);
      jobHandle.Complete();

      for (int i = 0; i < zombieList.Count; i++) {
        zombieList[i].transform.position = positions[i];
        zombieList[i].moveY = moveYValues[i];
      }

      positions.Dispose();
      moveYValues.Dispose();
    } else if (useJobs) {
      NativeList<JobHandle> jobHandles = new NativeList<JobHandle>(Allocator.Temp);

      for (int i = 0; i < 10; i++) {
        JobHandle jobHandle = LongTaskJob();
        jobHandles.Add(jobHandle);
      }

      JobHandle.CompleteAll(jobHandles);
      jobHandles.Dispose();
    } else {
      for (int i = 0; i < 10; i++) {
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

[BurstCompile]
public struct LongParallelJobFor : IJobParallelFor {
  public NativeArray<float3> positions;
  public NativeArray<float> moveYValues;
  public float deltaTime;


  public void Execute(int index) {
    positions[index] += new float3(0, moveYValues[index] = deltaTime, 0);
    if (positions[index].y > 5f) {
      moveYValues[index] = math.abs(moveYValues[index]);
    }
    if (positions[index].y < -5f) {
      moveYValues[index] += math.abs(moveYValues[index]);
    }
    float value = 0f;
    for (int i = 0; i < 1000; i++) {
      value = math.exp10(math.sqrt(value));
    }
  }
}


[BurstCompile]
public struct LongParalellJobTransform : IJobParallelForTransform {
  public NativeArray<float> moveYValues;
  public float deltaTime;
  public void Execute(int index, TransformAccess transform) {
    transform.position += new Vector3(0, moveYValues[index] = deltaTime, 0);
    if (transform.position.y > 5f) {
      moveYValues[index] = math.abs(moveYValues[index]);
    }
    if (transform.position.y < -5f) {
      moveYValues[index] += math.abs(moveYValues[index]);
    }
    float value = 0f;
    for (int i = 0; i < 1000; i++) {
      value = math.exp10(math.sqrt(value));
    }
  }
}