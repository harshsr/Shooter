using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class EnemyAuthoring : MonoBehaviour
{
    public GameObject enemyPrefabToSpawn;
    
    public int numEnemiesPerSecond = 50;
    public int numEnemiesIncrementAmount = 2;
    public int maxNumEnemiesPerSecond = 100;
    
    public float spawnRadius = 40f;
    
    public float minimumDistanceFromPlayer =5f ;
    
    public float timeToNextSpawn = 2f;

    public class EnemySpwnBaker : Baker<EnemyAuthoring>
    {
        public override void Bake(EnemyAuthoring authoring)
        {
            Entity enemyEntity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(enemyEntity, new EnemyComponent
                {
                    enemyPrefabToSpawn = GetEntity(authoring.enemyPrefabToSpawn, TransformUsageFlags.None),
                    numEnemiesPerSecond = authoring.numEnemiesPerSecond,
                    numEnemiesIncrementAmount = authoring.numEnemiesIncrementAmount,
                    maxNumEnemiesPerSecond = authoring.maxNumEnemiesPerSecond,
                    spawnRadius = authoring.spawnRadius,
                    minimumDistanceFromPlayer = authoring.minimumDistanceFromPlayer,
                    timeToNextSpawn = authoring.timeToNextSpawn,
                });
        }
    }
}
