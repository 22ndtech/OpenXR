using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Rendering.HybridV2;
using Unity.Mathematics;

public class InitializeECS : MonoBehaviour {
  [SerializeField] private Mesh mesh;
  [SerializeField] private Material material;
  [SerializeField] private int numberOfZombies = 10;

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
}
