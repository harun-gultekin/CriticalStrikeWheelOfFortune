using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent; 
    [SerializeField] private GameObject itemPrefab;   

    // Hangi itemden elimizde var? (ItemSO -> UI Script Eşleşmesi)
    private Dictionary<RewardItemSO, InventoryItemUI> itemMap = new Dictionary<RewardItemSO, InventoryItemUI>();

    public void AddItem(LevelManager.RuntimeSlice reward)
    {
        // 1. Bu itemden zaten var mı?
        if (itemMap.ContainsKey(reward.item))
        {
            // VARSA: Mevcut olanı bul ve miktarını artır
            InventoryItemUI existingItem = itemMap[reward.item];
            existingItem.AddAmount(reward.amount);
            
            // Opsiyonel: Güncellenen itemi en üste veya en alta taşımak istersen:
            // existingItem.transform.SetAsLastSibling(); 
        }
        else
        {
            // YOKSA: Yeni oluştur
            GameObject newItemObj = Instantiate(itemPrefab, contentParent);
            
            if (newItemObj.TryGetComponent(out InventoryItemUI newItemUI))
            {
                newItemUI.Setup(reward.item.icon, reward.amount);
                
                // Sözlüğe kaydet ki bir dahakine bulabilelim
                itemMap.Add(reward.item, newItemUI);
            }
        }
    }

    public void ClearInventory()
    {
        // UI'ı temizle
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        
        // Hafızayı temizle
        itemMap.Clear();
    }
}