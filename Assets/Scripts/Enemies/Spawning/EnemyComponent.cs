using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

public struct EnemyComponent : IComponentData
{
    public Entity enemyPrefabToSpawn;
    
    public int numEnemiesPerSecond;
    public int numEnemiesIncrementAmount;
    public int maxNumEnemiesPerSecond;
    
    public float spawnRadius;
    
    public float minimumDistanceFromPlayer;
    
    public float timeToNextSpawn;
    public float currentTimeToNextSpawn;
}
