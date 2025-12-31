using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private int currentAmount;

    // İlk oluşturma
    public void Setup(Sprite icon, int amount)
    {
        iconImage.sprite = icon;
        currentAmount = amount;
        UpdateText();
    }

    // Var olanın üzerine ekleme
    public void AddAmount(int amountToAdd)
    {
        currentAmount += amountToAdd;
        UpdateText();
    }

    private void UpdateText()
    {
        // 1000 yerine 1K, 1500 yerine 1.5K gibi formatlanabilir istersen
        // Şimdilik düz yazıyoruz:
        amountText.text = "x" + currentAmount.ToString();
    }
}