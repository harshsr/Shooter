using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct EnemyIndividualSystem : ISystem
{

    private EntityManager entityManager;
    private Entity playerEntity;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);
        
        NativeArray<Entity> allEntities = entityManager.GetAllEntities();
        
        foreach (Entity entity in allEntities)
        {
            if (entityManager.HasComponent<EnemyIndividualComponent>(entity))
            {
                LocalTransform enemyTransform = entityManager.GetComponentData<LocalTransform>(entity);
                EnemyIndividualComponent enemyComponent = entityManager.GetComponentData<EnemyIndividualComponent>(entity);
                
                float3 direction = math.normalize(playerTransform.Position - enemyTransform.Position);
                float angle = math.atan2(direction.y,direction.x);
                angle -= math.radians(90f);
                enemyTransform.Rotation = quaternion.AxisAngle(new float3(0f,0f,1f),angle);
              
                enemyTransform.Position += enemyTransform.Up() * enemyComponent.speed * SystemAPI.Time.DeltaTime;
                
                entityManager.SetComponentData(entity, enemyTransform);
            }
        }
    }
}
