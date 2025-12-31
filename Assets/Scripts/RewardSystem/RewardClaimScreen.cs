using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Listeyi işlemek için lazım
using DG.Tweening;

public class RewardClaimScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform contentParent; // ScrollView -> Viewport -> Content
    [SerializeField] private GameObject cardPrefab;   
    [SerializeField] private GameObject mainMenuButton; 
    [SerializeField] private ScrollRect scrollRect;   // Otomatik kaydırma için

    // Auto-Referenced in OnValidate
    private void OnValidate()
    {
        // Auto-find main menu button
        if (mainMenuButton == null)
        {
            var btn = transform.Find("ui_btn_mainmenu")?.GetComponent<Button>();
            if (btn != null)
            {
                mainMenuButton = btn.gameObject;
            }
            else
            {
                var buttons = GetComponentsInChildren<Button>(true);
                foreach (var b in buttons)
                {
                    if (b.name == "ui_btn_mainmenu" || b.name.Contains("mainmenu") || b.name.Contains("main_menu"))
                    {
                        mainMenuButton = b.gameObject;
                        break;
                    }
                }
            }
        }

        // Auto-find content parent
        if (contentParent == null)
        {
            contentParent = transform.Find("ScrollView/Viewport/Content")?.transform;
            if (contentParent == null)
            {
                var scrollView = GetComponentInChildren<ScrollRect>(true);
                if (scrollView != null && scrollView.content != null)
                {
                    contentParent = scrollView.content;
                }
            }
        }

        // Auto-find scroll rect
        if (scrollRect == null)
        {
            scrollRect = GetComponentInChildren<ScrollRect>(true);
        }
    }

    public void ShowClaimSequence(List<LevelManager.RuntimeSlice> rawRewards)
    {
        gameObject.SetActive(true);
        mainMenuButton.SetActive(false);
        
        // Temizlik
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        // 1. Ödülleri Birleştir (Stacking Logic)
        List<LevelManager.RuntimeSlice> mergedRewards = CompressRewards(rawRewards);

        // 2. Animasyonu Başlat
        StartCoroutine(ShowCardsRoutine(mergedRewards));
    }

    // Listeyi sıkıştıran matematiksel fonksiyon
    private List<LevelManager.RuntimeSlice> CompressRewards(List<LevelManager.RuntimeSlice> source)
    {
        // ItemSO'ya göre grupla ve miktarları topla
        var grouped = source
            .GroupBy(r => r.item)
            .Select(g => new LevelManager.RuntimeSlice
            {
                item = g.Key,
                amount = g.Sum(r => r.amount),
                isBomb = false
            })
            .ToList();

        return grouped;
    }

    private IEnumerator ShowCardsRoutine(List<LevelManager.RuntimeSlice> rewards)
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var reward in rewards)
        {
            // Kartı oluştur
            GameObject card = Instantiate(cardPrefab, contentParent);
            
            // Veriyi bas (Image ve Text bulup)
            var img = card.transform.Find("ui_image_icon_value")?.GetComponent<Image>();
            var txt = card.transform.Find("ui_text_amount_value")?.GetComponent<TMPro.TextMeshProUGUI>();
            
            // Prefab içindeki ikonun terslik durumunu burada da düzeltiyoruz:
            if(img) 
            {
                img.sprite = reward.item.icon;
                // Eğer prefabda ikon 180 dereceyse, UI'da düz görünmesi için dokunmuyoruz.
            }
            if(txt) txt.text = "x" + reward.amount;

            // Animasyon (Pop)
            card.transform.localScale = Vector3.zero;
            card.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

            // Scroll'u en sona kaydır (Otomatik odaklanma)
            if(scrollRect != null) 
                DOVirtual.Float(scrollRect.horizontalNormalizedPosition, 1f, 0.3f, v => scrollRect.horizontalNormalizedPosition = v);

            yield return new WaitForSeconds(0.3f); 
        }

        // Hepsi bitti, butonu aç
        yield return new WaitForSeconds(0.5f);
        if(mainMenuButton != null)
        {
            mainMenuButton.SetActive(true);
            mainMenuButton.transform.localScale = Vector3.zero;
            mainMenuButton.transform.DOScale(1f, 0.5f).SetEase(Ease.OutElastic);
        }
    }
}