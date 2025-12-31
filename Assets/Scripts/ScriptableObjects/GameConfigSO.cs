using UnityEngine;

[CreateAssetMenu(menuName = "Vertigo/Game Config", fileName = "GameConfig")]
public class GameConfigSO : ScriptableObject
{
    [Header("Wheel Tiers (Çark Şablonları)")]
    public WheelTierSO bronzeTier; // Standart (Bomba var)
    public WheelTierSO silverTier; // Safe Zone (Her 5)
    public WheelTierSO goldTier;   // Super Zone (Her 30)

    [Header("Zone Rules (Kurallar)")]
    public int safeZoneInterval = 5;
    public int superZoneInterval = 30;

    [Header("Economy (Zorluk)")]
    [Tooltip("Her zone başına ödül ne kadar artsın? 0.1 = %10 artış")]
    public float amountMultiplierPerZone = 0.1f; 

    // Helper: Level Manager bu fonksiyonu çağırıp doğru çarkı isteyecek
    public WheelTierSO GetTierForZone(int zoneIndex)
    {
        // Önce Super Zone kontrolü (Çünkü 30, hem 5'e hem 30'a bölünür)
        if (zoneIndex % superZoneInterval == 0) 
            return goldTier;
        
        // Sonra Safe Zone kontrolü
        if (zoneIndex % safeZoneInterval == 0) 
            return silverTier;

        // Hiçbiri değilse Standart
        return bronzeTier;
    }
}