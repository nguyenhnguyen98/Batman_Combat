using UnityEngine;

[CreateAssetMenu(fileName = "Create Actor", menuName ="Actor/New Actor")]
public class SOActorModel : ScriptableObject
{
    public string actorName;
    public string description;
    public int health;
    public int movementSpeed;
    public int armor;
    public int baseDamage;
    public string[] attackTypes;
}
