using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Goblin,
    Skeleton,
    Slime
}

public class Monster : Character
{
    [SerializeField] private ClassSystem.Classes monsterClass = ClassSystem.Classes.Warrior;
    [SerializeField] private EnemyType enemyType;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
    }
}

