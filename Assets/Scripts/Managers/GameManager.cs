using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Data")]
    public int currentZone = 1;
    public int totalMoney = 5000;
    private List<LevelManager.RuntimeSlice> collectedRewards = new List<LevelManager.RuntimeSlice>();

    [Header("UI Panels")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private RewardClaimScreen claimScreen;

    [Header("Win Screen Elements (YENİ)")]
    [SerializeField] private Transform winCardContainer; // Kartın oluşacağı boş obje
    [SerializeField] private GameObject rewardCardPrefab; // Oluşacak kartın prefabı

    [Header("Lose Screen Elements")]
    [SerializeField] private TextMeshProUGUI failInfoText;

    [Header("System References")]
    [SerializeField] private WheelController wheelController;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private TextMeshProUGUI zoneText;

    // OTOMATİK BULUNACAK BUTONLAR
    [Header("Auto-Referenced Buttons (Read Only)")]
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnExit;
    [SerializeField] private Button btnRevive;
    [SerializeField] private Button btnGiveUp;
    [SerializeField] private Button btnMainMenu; // Claim Screen'deki "Tamam" butonu

    private void Awake() 
    { 
        Instance = this; 
    }

    private void Start()
    {
        SetupButtonListeners();
        RestartGame();
    }

    private void SetupButtonListeners()
    {
        // Tüm listenerları temizle ve yeniden bağla (Garanti olsun)
        if (btnContinue != null) { btnContinue.onClick.RemoveAllListeners(); btnContinue.onClick.AddListener(Button_Continue); }
        if (btnExit != null) { btnExit.onClick.RemoveAllListeners(); btnExit.onClick.AddListener(Button_ExitAndClaim); }
        if (btnRevive != null) { btnRevive.onClick.RemoveAllListeners(); btnRevive.onClick.AddListener(Button_Revive); }
        if (btnGiveUp != null) { btnGiveUp.onClick.RemoveAllListeners(); btnGiveUp.onClick.AddListener(Button_GiveUp); }
        
        // Claim Screen butonu (YENİ)
        if (btnMainMenu != null) { btnMainMenu.onClick.RemoveAllListeners(); btnMainMenu.onClick.AddListener(Button_FinishGame); }
    }

    // --- OYUN AKIŞI ---

    public void RestartGame()
    {
        Debug.Log("Oyun Resetleniyor...");
        currentZone = 1;
        collectedRewards.Clear();
        
        if(inventoryUI != null) inventoryUI.ClearInventory();
        
        UpdateZoneUI();
        if(wheelController != null) wheelController.SetupNewLevel(currentZone);
        
        // Tüm ekranları kapat
        if(winScreen != null) winScreen.SetActive(false);
        if(loseScreen != null) loseScreen.SetActive(false);
        if(claimScreen != null) claimScreen.gameObject.SetActive(false);
    }

    public void OnSpinEnded(LevelManager.RuntimeSlice result)
    {
        if (result.isBomb) HandleLose();
        else HandleWin(result);
    }

    private void HandleWin(LevelManager.RuntimeSlice reward)
    {
        collectedRewards.Add(reward);
        if(inventoryUI != null) inventoryUI.AddItem(reward);

        // --- GÜNCELLEME: YAZI YERİNE KART GÖSTERİMİ ---
        // Eğer container ve prefab atanmışsa kart oluştur
        if(winCardContainer != null && rewardCardPrefab != null)
        {
            // Önce eskisini temizle (Varsa)
            foreach(Transform child in winCardContainer) Destroy(child.gameObject);

            // Yeni kartı oluştur
            GameObject card = Instantiate(rewardCardPrefab, winCardContainer);
            
            // Kartın içini doldur (InventoryItemUI scripti yoksa manuel buluyoruz)
            var img = card.transform.Find("ui_image_icon_value")?.GetComponent<Image>();
            var txt = card.transform.Find("ui_text_amount_value")?.GetComponent<TMPro.TextMeshProUGUI>();
            
            if(img != null) img.sprite = reward.item.icon;
            if(txt != null) txt.text = "x" + reward.amount;
        }
        else
        {
            // Yedek: Eğer kart sistemi kurulmadıysa eski log'u bas
            Debug.LogWarning("Win Screen Card Container veya Prefab eksik!");
        }

        if(winScreen != null) winScreen.SetActive(true); 
    }

    // --- BUTON AKSİYONLARI ---

    public void Button_Continue()
    {
        if(winScreen != null) winScreen.SetActive(false);
        currentZone++;
        UpdateZoneUI();
        if(wheelController != null) wheelController.SetupNewLevel(currentZone);
    }

    public void Button_ExitAndClaim()
    {
        if(winScreen != null) winScreen.SetActive(false);
        
        // Eğer Claim Screen varsa onu aç, yoksa direkt resetle
        if(claimScreen != null) 
        {
            claimScreen.ShowClaimSequence(collectedRewards);
        }
        else 
        {
            RestartGame();
        }
    }

    public void Button_FinishGame() // Claim Screen'deki "Tamam" butonu
    {
        // Bu buton claimScreen'i kapatıp oyunu baştan başlatır
        RestartGame();
    }

    // --- KAYBETME VE DİĞERLERİ ---

    private void HandleLose() 
    { 
        if(loseScreen != null) 
        { 
            loseScreen.SetActive(true); 
            if(failInfoText != null) failInfoText.text = "Paran: " + totalMoney + "$"; 
        } 
    }

    public void Button_Revive() 
    { 
        int reviveCost = 100;
        if(totalMoney >= reviveCost) 
        { 
            totalMoney -= reviveCost; 
            if(loseScreen != null) loseScreen.SetActive(false); 
            Button_Continue(); 
        } 
    }

    public void Button_GiveUp() 
    { 
        RestartGame(); 
    }

    private void UpdateZoneUI() 
    { 
        if(zoneText == null) return;
        
        string color = "white";
        string prefix = "";
        if (currentZone % 30 == 0) { color = "gold"; prefix = "SUPER "; }
        else if (currentZone % 5 == 0) { color = "#C0C0C0"; prefix = "SAFE "; } 
        
        zoneText.text = $"<color={color}>{prefix}ZONE {currentZone}</color>";
    }

    // --- OTOMATİK BAĞLAMA (ONVALIDATE) ---
    private void OnValidate()
    {
        // 1. Win Screen Butonları
        if (winScreen != null) {
            var buttons = winScreen.GetComponentsInChildren<Button>(true);
            foreach(var b in buttons) {
                if(b.name == "ui_btn_continue") btnContinue = b;
                if(b.name == "ui_btn_exit") btnExit = b;
            }
        }

        // 2. Lose Screen Butonları
        if (loseScreen != null) {
            var buttons = loseScreen.GetComponentsInChildren<Button>(true);
            foreach(var b in buttons) {
                if(b.name == "ui_btn_revive") btnRevive = b;
                if(b.name == "ui_btn_giveup") btnGiveUp = b;
            }
        }
        
        // 3. Claim Screen Butonu (YENİ EKLENDİ)
        if (claimScreen != null) {
            // Claim screen scriptinin olduğu objenin altında buton arıyoruz
            var btn = claimScreen.transform.Find("ui_btn_mainmenu")?.GetComponent<Button>(); 
            // Veya isme göre derin arama:
            if(btn == null) {
                var buttons = claimScreen.GetComponentsInChildren<Button>(true);
                foreach(var b in buttons) {
                    if(b.name == "ui_btn_mainmenu") {
                        btnMainMenu = b;
                        break;
                    }
                }
            } else {
                btnMainMenu = btn;
            }
        }
    }
}