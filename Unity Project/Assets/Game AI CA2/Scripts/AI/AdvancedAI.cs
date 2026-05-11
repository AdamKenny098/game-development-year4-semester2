using UnityEngine;

public class AdvancedAI : MonoBehaviour
{
    public Entity entity;
    public Character character;
    public Stats stats;


    public virtual void Awake()
    {
        entity = GetComponent<Entity>();
        character = GetComponent<Character>();
        stats = entity.stats;
    }
}

