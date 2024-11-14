using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct EnemySystem : ISystem
{
    private EntityManager entityManager;
    
    private Entity enemySpawnerEntity;
    private EnemyComponent enemySpawnerComponent;
    
    private Entity playerEntity;

    private Unity.Mathematics.Random random;
    
    public void OnCreate(ref SystemState state)
    {
        random = Unity.Mathematics.Random.CreateFromIndex((uint)enemySpawnerComponent.GetHashCode());
    }

    
    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        enemySpawnerEntity = SystemAPI.GetSingletonEntity<EnemyComponent>();
        enemySpawnerComponent = entityManager.GetComponentData<EnemyComponent>(enemySpawnerEntity);
        playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        
        SpawnEnemy(ref state);
    }

    [BurstCompile]
    private void SpawnEnemy(ref SystemState state)
    {
        enemySpawnerComponent.currentTimeToNextSpawn -= SystemAPI.Time.DeltaTime;
        if (enemySpawnerComponent.currentTimeToNextSpawn <= 0f)
        {
            for (int i = 0; i < enemySpawnerComponent.numEnemiesPerSecond; i++)
            {
                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);
                Entity enemyEntity = entityManager.Instantiate(enemySpawnerComponent.enemyPrefabToSpawn);
                
                LocalTransform enemyTransform = entityManager.GetComponentData<LocalTransform>(enemyEntity);
                LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);
                
                float minDistanceSquared = enemySpawnerComponent.minimumDistanceFromPlayer * enemySpawnerComponent.minimumDistanceFromPlayer;
                float2 randomOffset = random.NextFloat2Direction() * random.NextFloat(enemySpawnerComponent.minimumDistanceFromPlayer,enemySpawnerComponent.spawnRadius);
                float2 playerPosition = new float2(playerTransform.Position.x,playerTransform.Position.y);
                float2 spawnPosition = playerPosition + randomOffset;
                float distanceSquared = math.lengthsq(playerPosition - randomOffset);

                if (distanceSquared < minDistanceSquared)
                {
                    spawnPosition = playerPosition + math.normalize(randomOffset) * math.sqrt(minDistanceSquared);
                }
                enemyTransform.Position = new float3(spawnPosition.x,spawnPosition.y,0f);
                
                float3 direction = math.normalize(playerTransform.Position - enemyTransform.Position);
                float angle = math.atan2(direction.y,direction.x);
                angle -= math.radians(90f);
                
                enemyTransform.Rotation = quaternion.AxisAngle(new float3(0f,0f,1f),angle);

                ECB.SetComponent(enemyEntity, enemyTransform);
                
                ECB.AddComponent(enemyEntity, new EnemyIndividualComponent
                {
                    health = 200f,
                    speed = 1.25f
                });
                
                ECB.Playback(entityManager);
                ECB.Dispose();
            }
            
            int desiredNumEnemiesPerSecond = enemySpawnerComponent.numEnemiesPerSecond + enemySpawnerComponent.numEnemiesIncrementAmount;
            int numEnemiesPerSecond = math.min(desiredNumEnemiesPerSecond,enemySpawnerComponent.maxNumEnemiesPerSecond);
            enemySpawnerComponent.numEnemiesPerSecond = numEnemiesPerSecond;
            
            enemySpawnerComponent.currentTimeToNextSpawn = enemySpawnerComponent.timeToNextSpawn;
        }
        
        entityManager.SetComponentData(enemySpawnerEntity, enemySpawnerComponent);
    }

}
