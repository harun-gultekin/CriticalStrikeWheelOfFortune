using UnityEngine;

[CreateAssetMenu(menuName = "Critical Strike/Reward Item")]
public class RewardItemSO : ScriptableObject
{
    [Header("Identity")]
    public string id;              // "weapon_shotgun_t1", "point_rifle"
    public string displayName;     // UI'da görünecek isim: "M3 Super Shotgun"
    public Sprite icon;            // Kartın içindeki resim
    public Sprite frameIcon;       // (Opsiyonel) Çerçeve rengi görseli (Rarity'e göre)

    [Header("Settings")]
    public RewardType type;        // Weapon, Point, Bomb vs.
    public Rarity rarity;          // Tier bilgisi buraya
}