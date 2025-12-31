using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Data")]
    public int currentZone = 1;
    public int totalMoney = 5000;
    private List<LevelManager.RuntimeSlice> collectedRewards = new List<LevelManager.RuntimeSlice>();
    private List<LevelManager.RuntimeSlice> rewardsToClaim = new List<LevelManager.RuntimeSlice>(); // Rewards waiting to be claimed

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
    [SerializeField] private GameConfigSO gameConfig;
    
    [Header("Zone Info UI")]
    [SerializeField] private TextMeshProUGUI currentZoneText;
    [SerializeField] private TextMeshProUGUI nextSilverZoneText;
    [SerializeField] private TextMeshProUGUI nextGoldenZoneText;
    
    [Header("Cash UI")]
    [SerializeField] private TextMeshProUGUI cashText; // Current cash amount display

    // OTOMATİK BULUNACAK BUTONLAR
    [Header("Auto-Referenced Buttons (Read Only)")]
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnExit; // Win screen exit button
    [SerializeField] private Button btnExitMain; // Main game screen exit button (always visible)
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
        UpdateCashUI(); // Initialize cash display
        RestartGame();
    }

    private void SetupButtonListeners()
    {
        // Tüm listenerları temizle ve yeniden bağla (Garanti olsun)
        if (btnContinue != null) { btnContinue.onClick.RemoveAllListeners(); btnContinue.onClick.AddListener(Button_Continue); }
        if (btnExit != null) { btnExit.onClick.RemoveAllListeners(); btnExit.onClick.AddListener(Button_ExitAndClaim); }
        if (btnExitMain != null) { btnExitMain.onClick.RemoveAllListeners(); btnExitMain.onClick.AddListener(Button_ExitMainGame); }
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
        UpdateCashUI();
        if(wheelController != null) wheelController.SetupNewLevel(currentZone);
        
        // Tüm ekranları kapat
        if(winScreen != null) winScreen.SetActive(false);
        if(loseScreen != null) loseScreen.SetActive(false);
        if(claimScreen != null) claimScreen.gameObject.SetActive(false);
        
        // Re-enable and show exit button when new game starts
        if (btnExitMain != null)
        {
            btnExitMain.interactable = true;
            btnExitMain.gameObject.SetActive(true);
        }
        
        // Update exit button state (check if wheel is spinning)
        UpdateExitButtonState();
    }

    public void OnSpinEnded(LevelManager.RuntimeSlice result)
    {
        // Re-enable exit button when spin ends
        UpdateExitButtonState();
        
        if (result.isBomb) HandleLose();
        else HandleWin(result);
    }

    // Called when wheel starts spinning
    public void OnSpinStarted()
    {
        UpdateExitButtonState();
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

        ShowWinScreen();
    }

    private void ShowWinScreen()
    {
        if(winScreen != null)
        {
            winScreen.SetActive(true);
            // Hide exit button when win screen is shown
            HideExitButton();
            // Pop in animation
            winScreen.transform.localScale = Vector3.zero;
            winScreen.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }

    private void HideWinScreen()
    {
        if(winScreen != null)
        {
            // Pop out animation
            winScreen.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                .OnComplete(() => {
                    winScreen.SetActive(false);
                    // Show exit button when win screen is hidden (always show, but keep disabled state)
                    //ShowExitButton();
                });
        }
    }

    // --- BUTON AKSİYONLARI ---

    public void Button_Continue()
    {
        HideWinScreen();
        ShowExitButton();
        // Exit button will be shown in HideWinScreen callback
        AnimateZoneChange();
    }

    private void AdvanceZone()
    {
        currentZone++;
        UpdateZoneUI();
        if(wheelController != null) 
        {
            wheelController.SetupNewLevel(currentZone);
            // Update exit button state after wheel setup (wheel is ready, not spinning)
            UpdateExitButtonState();
        }
    }

    public void Button_ExitAndClaim()
    {
        HideWinScreen();
        ShowClaimScreen();
    }

    // Exit button from main game screen (always visible)
    public void Button_ExitMainGame()
    {
        // Only allow exit if wheel is not spinning
        if (wheelController != null && wheelController.IsSpinning)
        {
            return;
        }

        // Disable exit button after being pressed
        if (btnExitMain != null)
        {
            btnExitMain.interactable = false;
        }

        // Show claim screen with all collected rewards
        ShowClaimScreen();
    }

    private void ShowClaimScreen()
    {
        // Hide exit button when claim screen is shown
        HideExitButton();
        
        // Eğer Claim Screen varsa onu aç, yoksa direkt resetle
        if(claimScreen != null) 
        {
            // Store rewards that will be claimed
            rewardsToClaim = new List<LevelManager.RuntimeSlice>(collectedRewards);
            claimScreen.ShowClaimSequence(collectedRewards);
        }
        else 
        {
            // If no claim screen, directly add cash from rewards
            AddCashFromRewards(collectedRewards);
            RestartGame();
        }
    }

    // Calculate and add cash from currency rewards
    private void AddCashFromRewards(List<LevelManager.RuntimeSlice> rewards)
    {
        int totalCash = 0;
        foreach (var reward in rewards)
        {
            if (reward.item != null && reward.item.id == "currency_cash")
            {
                totalCash += reward.amount;
            }
        }
        
        if (totalCash > 0)
        {
            totalMoney += totalCash;
            Debug.Log($"Added {totalCash} cash from rewards. Total: {totalMoney}");
        }
    }

    public void Button_FinishGame() // Claim Screen'deki "Tamam" butonu
    {
        // Add cash from claimed rewards before restarting
        if (rewardsToClaim != null && rewardsToClaim.Count > 0)
        {
            AddCashFromRewards(rewardsToClaim);
            rewardsToClaim.Clear();
        }
        
        // Bu buton claimScreen'i kapatıp oyunu baştan başlatır
        RestartGame();
    }

    // --- KAYBETME VE DİĞERLERİ ---

    private void HandleLose() 
    { 
        ShowLoseScreen();
    }

    private void ShowLoseScreen()
    {
        if(loseScreen != null) 
        { 
            loseScreen.SetActive(true);
            if(failInfoText != null) failInfoText.text = "Cash: " + totalMoney + "$";
            
            // Hide exit button when lose screen is shown
            HideExitButton();
            
            // Update revive button text with cost
            UpdateReviveButton();
            
            // Pop in animation
            loseScreen.transform.localScale = Vector3.zero;
            loseScreen.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }

    private void UpdateReviveButton()
    {
        if (btnRevive == null) return;
        
        int reviveCost = 100;
        var buttonText = btnRevive.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = $"Revive ({reviveCost}$)";
        }
        
        // Enable/disable button based on available cash
        btnRevive.interactable = totalMoney >= reviveCost;
    }

    private void HideLoseScreen()
    {
        if(loseScreen != null)
        {
            // Pop out animation
            loseScreen.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                .OnComplete(() => {
                    loseScreen.SetActive(false);
                    // Show exit button when lose screen is hidden (always show, but keep disabled state)
                });
        }
    }

    public void Button_Revive() 
    { 
        int reviveCost = 100;
        if(totalMoney >= reviveCost) 
        { 
            totalMoney -= reviveCost;
            UpdateCashUI();
            HideLoseScreen();
            ShowExitButton();
            // Exit button will be shown in HideLoseScreen callback
            AnimateZoneChange();
        } 
    }

    public void Button_GiveUp() 
    { 
        RestartGame(); 
    }

    private void UpdateZoneUI() 
    { 
        // Update zone info texts
        if(currentZoneText != null)
        {
            
            currentZoneText.text = $"{currentZone}";
        }

        if(nextSilverZoneText != null && gameConfig != null)
        {
            int nextSilver = GetNextSilverZone(currentZone);
            nextSilverZoneText.text = $"{nextSilver}";
        }

        if(nextGoldenZoneText != null && gameConfig != null)
        {
            int nextGolden = GetNextGoldenZone(currentZone);
            nextGoldenZoneText.text = $"{nextGolden}";
        }
    }

    private void UpdateCashUI()
    {
        if (cashText != null)
        {
            cashText.text = totalMoney.ToString("N0") + "$"; // Format with commas and $ sign
        }
    }

    private void UpdateExitButtonState()
    {
        if (btnExitMain != null && wheelController != null)
        {
            // Disable exit button while wheel is spinning (only if button is enabled)
            if (btnExitMain.interactable)
            {
                btnExitMain.interactable = !wheelController.IsSpinning;
            }
        }
    }

    private void HideExitButton()
    {
        if (btnExitMain != null)
        {
            btnExitMain.gameObject.SetActive(false);
        }
    }

    private void ShowExitButton()
    {
        if (btnExitMain != null)
        {
            // Always show the exit button (make it visible)
            btnExitMain.gameObject.SetActive(true);
            btnExitMain.interactable = true;
            // Update state based on wheel spinning (only if button is enabled)
            UpdateExitButtonState();
        }
    }

    private void ShowExitButtonIfEnabled()
    {
        if (btnExitMain != null)
        {
            // Only show if button is enabled (not disabled after being clicked)
            if (btnExitMain.interactable)
            {
                btnExitMain.gameObject.SetActive(true);
                // Update state based on wheel spinning
                UpdateExitButtonState();
            }
        }
    }

    private int GetNextSilverZone(int currentZone)
    {
        if (gameConfig == null) return 5;
        int interval = gameConfig.safeZoneInterval;
        int next = ((currentZone / interval) + 1) * interval;
        return next;
    }

    private int GetNextGoldenZone(int currentZone)
    {
        if (gameConfig == null) return 30;
        int interval = gameConfig.superZoneInterval;
        int next = ((currentZone / interval) + 1) * interval;
        return next;
    }

    private void AnimateZoneChange()
    {
        // Animate zone info texts with rotation
        Sequence seq = DOTween.Sequence();

        // Rotate out current zone text
        if(currentZoneText != null)
        {
            seq.Join(currentZoneText.transform.DORotate(new Vector3(0, 0, 90), 0.2f).SetEase(Ease.InQuad));
        }
        if(nextSilverZoneText != null)
        {
            seq.Join(nextSilverZoneText.transform.DORotate(new Vector3(0, 0, 90), 0.2f).SetEase(Ease.InQuad));
        }
        if(nextGoldenZoneText != null)
        {
            seq.Join(nextGoldenZoneText.transform.DORotate(new Vector3(0, 0, 90), 0.2f).SetEase(Ease.InQuad));
        }

        // Update values in the middle (after rotation out)
        seq.AppendCallback(() => {
            AdvanceZone();
        });

        // Rotate back in
        if(currentZoneText != null)
        {
            seq.Append(currentZoneText.transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack));
        }
        if(nextSilverZoneText != null)
        {
            seq.Join(nextSilverZoneText.transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack));
        }
        if(nextGoldenZoneText != null)
        {
            seq.Join(nextGoldenZoneText.transform.DORotate(Vector3.zero, 0.3f).SetEase(Ease.OutBack));
        }
    }

    // --- OTOMATİK BAĞLAMA (ONVALIDATE) ---
    private void OnValidate()
    {
        // 1. Win Screen Butonları
        if (winScreen != null) {
            var buttons = winScreen.GetComponentsInChildren<Button>(true);
            foreach(var b in buttons) {
                if(b.name == "ui_btn_continue" || b.name.Contains("continue")) btnContinue = b;
                if(b.name == "ui_btn_exit" || b.name.Contains("exit")) btnExit = b;
            }
        }

        // 2. Lose Screen Butonları
        if (loseScreen != null) {
            var buttons = loseScreen.GetComponentsInChildren<Button>(true);
            foreach(var b in buttons) {
                if(b.name == "ui_btn_revive" || b.name.Contains("revive")) btnRevive = b;
                if(b.name == "ui_btn_giveup" || b.name.Contains("giveup") || b.name.Contains("give_up")) btnGiveUp = b;
            }
        }
        
        // 3. Claim Screen Butonu
        if (claimScreen != null) {
            var btn = claimScreen.transform.Find("ui_btn_mainmenu")?.GetComponent<Button>(); 
            if(btn == null) {
                var buttons = claimScreen.GetComponentsInChildren<Button>(true);
                foreach(var b in buttons) {
                    if(b.name == "ui_btn_mainmenu" || b.name.Contains("mainmenu") || b.name.Contains("main_menu")) {
                        btnMainMenu = b;
                        break;
                    }
                }
            } else {
                btnMainMenu = btn;
            }
        }

        // 4. Auto-find zone info texts
        if (currentZoneText == null)
        {
            var texts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach(var txt in texts)
            {
                if(txt.name.Contains("current_zone") || txt.name.Contains("currentZone") || txt.name == "ui_text_current_zone")
                {
                    currentZoneText = txt;
                    break;
                }
            }
        }

        if (nextSilverZoneText == null)
        {
            var texts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach(var txt in texts)
            {
                if(txt.name.Contains("next_silver") || txt.name.Contains("nextSilver") || txt.name == "ui_text_next_silver")
                {
                    nextSilverZoneText = txt;
                    break;
                }
            }
        }

        if (nextGoldenZoneText == null)
        {
            var texts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach(var txt in texts)
            {
                if(txt.name.Contains("next_golden") || txt.name.Contains("nextGolden") || txt.name == "ui_text_next_golden")
                {
                    nextGoldenZoneText = txt;
                    break;
                }
            }
        }

        // 5. Auto-find game config
        if (gameConfig == null)
        {
            gameConfig = Resources.FindObjectsOfTypeAll<GameConfigSO>()[0];
            if (gameConfig == null && levelManager != null)
            {
                var configField = typeof(LevelManager).GetField("gameConfig", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (configField != null)
                {
                    gameConfig = configField.GetValue(levelManager) as GameConfigSO;
                }
            }
        }

        // 6. Auto-find cash text
        if (cashText == null)
        {
            var texts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach(var txt in texts)
            {
                if(txt.name.Contains("cash") || txt.name.Contains("money") || txt.name == "ui_text_cash" || txt.name == "ui_text_money")
                {
                    cashText = txt;
                    break;
                }
            }
        }

        // 7. Auto-find main game exit button (not in win screen)
        if (btnExitMain == null)
        {
            // Search for exit button that's NOT in win screen
            var allButtons = FindObjectsOfType<Button>(true);
            foreach(var btn in allButtons)
            {
                // Check if button name contains "exit_main" or is specifically named for main game
                if(btn.name.Contains("exit_main") || btn.name == "ui_btn_exit_main")
                {
                    btnExitMain = btn;
                    break;
                }
            }
            
            // If still not found, look for exit button that's not in win/lose/claim screens
            if (btnExitMain == null)
            {
                foreach(var btn in allButtons)
                {
                    if(btn.name.Contains("exit"))
                    {
                        // Check if button is NOT in any screen
                        bool isInScreen = false;
                        if(winScreen != null && IsChildOf(btn.transform, winScreen.transform)) isInScreen = true;
                        if(loseScreen != null && IsChildOf(btn.transform, loseScreen.transform)) isInScreen = true;
                        if(claimScreen != null && IsChildOf(btn.transform, claimScreen.transform)) isInScreen = true;
                        
                        if(!isInScreen)
                        {
                            btnExitMain = btn;
                            break;
                        }
                    }
                }
            }
        }
    }

    // Helper method to check if transform is child of parent
    private bool IsChildOf(Transform child, Transform parent)
    {
        if (parent == null || child == null) return false;
        Transform current = child;
        while (current != null)
        {
            if (current == parent) return true;
            current = current.parent;
        }
        return false;
    }
}