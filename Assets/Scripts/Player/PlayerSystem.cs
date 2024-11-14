
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

public partial struct PlayerSystem : ISystem
{
    private EntityManager entityManager;
    private Entity playerEntity;
    private Entity inputEntity;
    private PlayerComponent playerComponent;
    private InputComponent inputComponent;

    public void OnUpdate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        inputEntity = SystemAPI.GetSingletonEntity<InputComponent>();
        playerComponent = entityManager.GetComponentData<PlayerComponent>(playerEntity);
        inputComponent = entityManager.GetComponentData<InputComponent>(inputEntity);
        
        MovePlayer(ref state);
        Shoot(ref state);
    }
    
    private void MovePlayer(ref SystemState state)
    {
        LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);
        
        Vector2 direction = (Vector2) inputComponent.mousePosition - (Vector2)Camera.main.WorldToScreenPoint(playerTransform.Position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        playerTransform.Rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // forward and backward movement
        //playerTransform.Position += playerTransform.Up() * inputComponent.movement.y * playerComponent.MoveSpeed * SystemAPI.Time.DeltaTime;
        // left and right movement
        playerTransform.Position += playerTransform.Right() * inputComponent.movement.y * playerComponent.MoveSpeed * SystemAPI.Time.DeltaTime;
        
        entityManager.SetComponentData(playerEntity, playerTransform);
    }
    
    private void Shoot(ref SystemState state)
    {
        if (inputComponent.fire)
        {
            for (int i = 0; i < playerComponent.BulletCount; i++)
            {
                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);
                
                Entity bulletEntity = entityManager.Instantiate(playerComponent.BulletPrefab);
                ECB.AddComponent(bulletEntity, new BulletComponent
                {
                    speed = 25f,
                    size = 0.25f,
                    damage = 1
                });
                ECB.AddComponent(bulletEntity, new BulletLifeTimeComponent
                {
                    remainingLifeTime = 2f
                });
                
                LocalTransform bulletTransform = entityManager.GetComponentData<LocalTransform>(bulletEntity);
                LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);
                bulletTransform.Rotation = playerTransform.Rotation;
                
                float spread = UnityEngine.Random.Range(-playerComponent.BulletSpread, playerComponent.BulletSpread);
                bulletTransform.Position = playerTransform.Position + playerTransform.Right() * 1.65f + bulletTransform.Up() * spread - bulletTransform.Right() * math.abs(spread);
                
                ECB.SetComponent(bulletEntity, bulletTransform);
                
                ECB.Playback(entityManager);
                ECB.Dispose();
            }
        }
    }
    
}
