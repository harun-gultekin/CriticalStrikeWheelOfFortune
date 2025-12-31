using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Listeyi karıştırmak için gerekli

public class LevelManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameConfigSO gameConfig;
    
    // Bu struct, UI'ın ekrana basacağı nihai veridir
    [System.Serializable]
    public class RuntimeSlice
    {
        public RewardItemSO item;
        public int amount;
        public bool isBomb;
    }

    // Dışarıdan çağrılacak ana fonksiyon
    public List<RuntimeSlice> GenerateLevel(int currentZoneIndex)
    {
        List<RuntimeSlice> generatedSlices = new List<RuntimeSlice>();

        // 1. Config'den bu zone için hangi Tier (Bronze/Silver/Gold) gerektiğini sor
        WheelTierSO currentTier = gameConfig.GetTierForZone(currentZoneIndex);

        // 2. Eğer bu tier'da bomba varsa ekle (Döküman: "One of the slices is a bomb")
        if (currentTier.hasBomb)
        {
            generatedSlices.Add(new RuntimeSlice
            {
                item = currentTier.bombItem,
                amount = 1, // Bombanın miktarı 1'dir
                isBomb = true
            });
        }

        // 3. Kalan boşlukları (Toplam 8 dilim) ağırlıklı rastgele itemlerle doldur
        int slotsToFill = 8 - generatedSlices.Count;
        
        for (int i = 0; i < slotsToFill; i++)
        {
            RewardDef selectedReward = GetRandomRewardWeighted(currentTier.potentialRewards);
            
            // Miktarı hesapla: BaseAmount * (1 + (Level * Çarpan))
            // Örn: Level 10, Base 100, Çarpan 0.1 => 100 * (1 + 1) = 200
            float multiplier = 1 + (currentZoneIndex * gameConfig.amountMultiplierPerZone);
            int finalAmount = Mathf.RoundToInt(selectedReward.baseAmount * multiplier);

            generatedSlices.Add(new RuntimeSlice
            {
                item = selectedReward.item,
                amount = finalAmount,
                isBomb = false
            });
        }

        // 4. Listeyi karıştır (Shuffle) ki bomba hep ilk sırada olmasın
        return ShuffleList(generatedSlices);
    }

    // Matematiksel Ağırlıklı Seçim (Weighted Random)
    private RewardDef GetRandomRewardWeighted(List<RewardDef> pool)
    {
        int totalWeight = 0;
        foreach (var item in pool) totalWeight += (int)item.weight;

        int randomValue = Random.Range(0, totalWeight);
        int currentSum = 0;

        foreach (var item in pool)
        {
            currentSum += (int)item.weight;
            if (randomValue < currentSum)
                return item;
        }

        return pool[0]; // Hata olursa ilkini döndür
    }

    // Listeyi Karıştırma Fonksiyonu (Fisher-Yates Shuffle)
    private List<T> ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
}