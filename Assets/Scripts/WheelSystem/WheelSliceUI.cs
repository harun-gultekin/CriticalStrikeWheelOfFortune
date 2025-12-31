using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WheelSliceUI : MonoBehaviour
{
    [Header("UI Elements")]
    // Döküman kuralı: İsimler "_value" ile bitmeli, biz otomatik bulacağız.
    [SerializeField] public Image iconImage;
    [SerializeField] public TextMeshProUGUI amountText;
    [SerializeField] private Image borderImage; // Rarity rengi için (Opsiyonel)
    private LevelManager.RuntimeSlice currentData; // Veriyi sakla

    // Setup fonksiyonu: Dışarıdan veriyi alır ve ekrana basar
    public void Setup(LevelManager.RuntimeSlice data)
    {
        currentData = data;
        // 1. İkonu koy
        iconImage.sprite = data.item.icon;
        
        // 2. Miktarı yaz (Eğer 1 ise boş bırakabilirsin veya direkt yaz)
        // Dökümanda bomba miktar yazmıyor, sadece ikon.
        if (data.item.type == RewardType.Bomb)
            amountText.text = ""; 
        else
            amountText.text = data.amount > 1 ? "x" + data.amount.ToString() : "x1";

        // 3. (Opsiyonel) Rarity Rengi
        // borderImage.color = GetRarityColor(data.item.rarity);
    }

    public LevelManager.RuntimeSlice GetData() => currentData;

    // Döküman kuralı: "Button references should be automatically set from OnValidate"
    // Biz bunu tüm UI elementleri için yapalım ki elle sürüklemeyelim.
    private void OnValidate()
    {
        // 1. İkon Bulma: Alt objelerde Image arar
        if (iconImage == null)
        {
            // Tüm çocukları tara (Image olanları)
            foreach (var img in GetComponentsInChildren<Image>(true))
            {
                // İsmi "icon" içeriyorsa veya "_value" ile bitiyorsa ve bu obje (kendisi) değilse
                if (img.gameObject != this.gameObject && (img.name.Contains("icon") || img.name.Contains("Icon")))
                {
                    iconImage = img;
                    break; // Bulunca döngüden çık
                }
            }
        }

        // 2. Text Bulma: Alt objelerde TMP arar
        if (amountText == null)
        {
            // Direkt TextMeshProUGUI olanı bulur (Zaten prefabda genelde 1 tane olur)
            amountText = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }
}