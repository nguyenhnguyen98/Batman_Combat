using UnityEngine;

public class Actor : MonoBehaviour
{
    int health;
    float moveSpeed;
    int armor;
    int baseDamage;
    float attackCooldown;
    string[] attackTypes;

    public int Health
    {
        get { return health; }
        set { health = value; }
    }

    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }

    public string[] AttackTypes
    {
        get { return attackTypes; }
        set { attackTypes = value; }
    }

    public int Armor
    {
        get { return armor; }
        set { armor = value; }
    }

    public int BaseDamage
    {
        get { return baseDamage; }
        set { baseDamage = value; }
    }

    public float AttackCooldown
    {
        get { return attackCooldown; }
        set { attackCooldown = value; }
    }

    [SerializeField]
    private SOActorModel model;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        health = model.health;
        moveSpeed = model.movementSpeed;
        armor = model.armor;
        baseDamage = model.baseDamage;
        attackTypes = model.attackTypes;
        attackCooldown = model.attackCooldown;
    }
}
