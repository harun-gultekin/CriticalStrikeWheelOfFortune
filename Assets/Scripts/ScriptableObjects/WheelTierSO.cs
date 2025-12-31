using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Vertigo/Wheel Tier Template", fileName = "NewWheelTier")]
public class WheelTierSO : ScriptableObject
{
    public string tierName;       // "Bronze", "Silver", "Gold"
    public bool hasBomb;          // Bu çarkta bomba olacak mı?
    public RewardItemSO bombItem; // Eğer bomba varsa, hangi item kullanılacak?
    
    [Header("Visual")]
    public Sprite wheelSprite;        // Çark base görseli (Bronze/Silver/Gold için farklı)
    public Sprite wheelIndicatorSprite; // Çark indicator/pointer görseli (Bronze/Silver/Gold için farklı)

    [Header("Pool Generation")]
    public List<RewardDef> potentialRewards; // Çarka girebilecek ödül havuzu
}

// Bu struct WheelTierSO.cs dosyasının içinde, class'ın hemen altında durabilir.
[System.Serializable]
public struct RewardDef
{
    public RewardItemSO item;
    public int baseAmount;       // Zone 1 için baz miktar (Örn: 100)
    [Range(0, 100)] 
    public float weight;         // Çıkma ağırlığı (Rastgele seçim için)
}