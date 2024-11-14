using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

public struct EnemyIndividualComponent : IComponentData
{
    public float health;
    public float speed;
}
