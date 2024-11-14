using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Physics;
using Unity.Burst;

[BurstCompile]
public partial struct BulletSystem : ISystem
{
    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        NativeArray<Entity> allEntities = entityManager.GetAllEntities();
        
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        foreach (Entity entity in allEntities)
        {
            if (entityManager.HasComponent<BulletComponent>(entity) && entityManager.HasComponent<BulletLifeTimeComponent>(entity))
            {
                LocalTransform bulletTransform = entityManager.GetComponentData<LocalTransform>(entity);
                BulletComponent bulletComponent = entityManager.GetComponentData<BulletComponent>(entity);
                
                bulletTransform.Position += bulletTransform.Right() * bulletComponent.speed * SystemAPI.Time.DeltaTime;
                entityManager.SetComponentData(entity, bulletTransform);
                
                BulletLifeTimeComponent bulletLifeTimeComponent = entityManager.GetComponentData<BulletLifeTimeComponent>(entity);
                bulletLifeTimeComponent.remainingLifeTime -= SystemAPI.Time.DeltaTime;
                if (bulletLifeTimeComponent.remainingLifeTime <= 0f)
                {
                    entityManager.DestroyEntity(entity);
                    continue;
                }
                entityManager.SetComponentData(entity, bulletLifeTimeComponent);
                
                // check for collisions
                
                NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);
                float3 start = new float3(bulletTransform.Position - bulletTransform.Right() * 0.15f);
                float3 end = new float3(bulletTransform.Position + bulletTransform.Right() * 0.15f);
                
                uint layerMask = LayerMaskHelper.GetLayerMaskFromTwoLayers(CollisionLayer.Wall, CollisionLayer.Enemy);
                
                physicsWorld.CapsuleCastAll(start,end,bulletComponent.size/2,float3.zero ,1f,ref hits,new CollisionFilter
                {
                   BelongsTo = (uint)CollisionLayer.Default,
                   CollidesWith = layerMask,
                });

                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        
                        Entity hitEntity = hits[i].Entity;
                        if (entityManager.HasComponent<EnemyIndividualComponent>(hitEntity))
                        {
                            EnemyIndividualComponent enemyComponent = entityManager.GetComponentData<EnemyIndividualComponent>(hitEntity);
                            enemyComponent.health -= bulletComponent.damage;
                            entityManager.SetComponentData(hitEntity, enemyComponent);

                            if (enemyComponent.health <= 0f)
                            {
                                entityManager.DestroyEntity(hitEntity);
                            }
                        }
                        
                        
                        //entityManager.DestroyEntity(hits[i].Entity);
                    }
                    entityManager.DestroyEntity(entity);
                }
                hits.Dispose();
            }
        }
    }
   
}
