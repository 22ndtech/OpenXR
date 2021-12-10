using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class MovementSystem : ComponentSystem {
  protected override void OnUpdate() {
    Entities.ForEach((ref Translation translation, ref MovementComponent movementComponent) => {
      translation.Value.y += movementComponent.moveSpeed * Time.DeltaTime;
      if (translation.Value.y > 5f) {
        movementComponent.moveSpeed = -math.abs(movementComponent.moveSpeed);
      }
      if (translation.Value.y < -5f) {
        movementComponent.moveSpeed = +math.abs(movementComponent.moveSpeed);
      }
    });


  }
}
