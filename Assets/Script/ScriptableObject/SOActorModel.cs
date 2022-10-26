using UnityEngine;

[CreateAssetMenu(fileName = "Create Actor", menuName ="Actor/New Actor")]
public class SOActorModel : ScriptableObject
{
    public string actorName;
    public string description;
    public int health;
    public float movementSpeed;
    public int armor;
    public float attackCooldown;
    public int baseDamage;
    public string[] attackTypes;
}
