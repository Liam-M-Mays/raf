using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponSO : ScriptableObject
{
    public string weaponName;
    public int cost;
    public Sprite weaponSprite;
    public int damage;
    public string description;
    public bool isMelee;
}
