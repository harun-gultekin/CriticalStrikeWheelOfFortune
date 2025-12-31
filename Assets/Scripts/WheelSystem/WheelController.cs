using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 
using System.Collections.Generic;

public class WheelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform wheelBody;   
    [SerializeField] private Button spinButton;     
    [SerializeField] private WheelSliceUI[] slices; 
    [SerializeField] private Image wheelImage;          // Wheel base image (for tier changes)
    [SerializeField] private Image wheelIndicatorImage;  // Wheel indicator/pointer image (for tier changes)

    [Header("Settings")]
    [SerializeField] private float spinDuration = 4f; 
    [SerializeField] private int spinRounds = 5;

    private bool isSpinning = false;
    private LevelManager levelManager;

    private void Awake()
    {
        levelManager = FindObjectOfType<LevelManager>();
        
        if(spinButton != null)
        {
            spinButton.onClick.RemoveAllListeners();
            spinButton.onClick.AddListener(SpinWheel);
        }
    }

    private void Start()
    {
        // İlk açılışta animasyonsuz direkt veriyi bas (Göz kırpma olmasın)
        List<LevelManager.RuntimeSlice> initialData = levelManager.GenerateLevel(1);
        for (int i = 0; i < slices.Length; i++)
        {
            if (i < initialData.Count) slices[i].Setup(initialData[i]);
        }
        spinButton.interactable = true;
    }

    public void SetupNewLevel(int zoneIndex)
    {
        // 1. Yeni veriyi al
        List<LevelManager.RuntimeSlice> levelData = levelManager.GenerateLevel(zoneIndex);

        // 2. Update wheel image based on tier
        UpdateWheelImage(zoneIndex);

        // 3. Flip Animasyonunu Başlat
        AnimateSlicesChange(levelData);
        
        spinButton.interactable = true;
    }

    private void UpdateWheelImage(int zoneIndex)
    {
        if (levelManager == null) return;

        // Get tier from LevelManager's gameConfig
        var configField = typeof(LevelManager).GetField("gameConfig", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (configField == null) return;

        GameConfigSO gameConfig = configField.GetValue(levelManager) as GameConfigSO;
        if (gameConfig == null) return;

        WheelTierSO tier = gameConfig.GetTierForZone(zoneIndex);
        if (tier == null) return;

        // Update base wheel image
        if (wheelImage != null && tier.wheelSprite != null)
        {
            wheelImage.sprite = tier.wheelSprite;
        }

        // Update indicator/pointer image
        if (wheelIndicatorImage != null && tier.wheelIndicatorSprite != null)
        {
            wheelIndicatorImage.sprite = tier.wheelIndicatorSprite;
        }
    }

    private void AnimateSlicesChange(List<LevelManager.RuntimeSlice> newData)
    {
        Sequence seq = DOTween.Sequence();

        // A. Kapanış (Flip In): Scale X'i 0 yap (Kağıt gibi incelir)
        // Rotasyonu ellemiyoruz, böylece ikonun açısı neyse öyle kalıyor!
        foreach (var slice in slices)
        {
            // İkon ve Yazıyı "Yatayda" kapat
            seq.Join(slice.iconImage.rectTransform.DOScaleX(0f, 0.2f).SetEase(Ease.InQuad));
            seq.Join(slice.amountText.rectTransform.DOScaleX(0f, 0.2f).SetEase(Ease.InQuad));
        }

        // B. Veri Değişimi (Tam ortada, görünmezken)
        seq.AppendCallback(() => 
        {
            // Çarkı SIFIRLAMIYORUZ. Nerede kaldıysa oradan devam.
            for (int i = 0; i < slices.Length; i++)
            {
                if (i < newData.Count)
                    slices[i].Setup(newData[i]);
            }
        });

        // C. Açılış (Flip Out): Scale X'i tekrar 1 yap
        foreach (var slice in slices)
        {
            seq.Join(slice.iconImage.rectTransform.DOScaleX(1f, 0.3f).SetEase(Ease.OutBack));
            seq.Join(slice.amountText.rectTransform.DOScaleX(1f, 0.3f).SetEase(Ease.OutBack));
        }
    }

    private void SpinWheel()
    {
        if (isSpinning) return;
        isSpinning = true;
        spinButton.interactable = false;

        // 1. Rastgele bir dilim seç
        int targetIndex = Random.Range(0, slices.Length);
        WheelSliceUI targetSlice = slices[targetIndex];

        // 2. AKILLI AÇI HESABI (Transform Logic)
        // Hedef dilim şu an nerede? (Örn: 225 derecede)
        float currentSliceAngle = targetSlice.transform.localEulerAngles.z;
        
        // Onu tepeye (0 dereceye) getirmek için ne kadar dönmeliyiz?
        // Dilim 45'teyse, çarkı -45 çevirmeliyiz.
        float targetRotation = -currentSliceAngle;

        // Çoklu tur ekle (Spin Rounds)
        // Hep aynı yöne dönmesi için "FastBeyond360" moduna uygun negatif turlar ekliyoruz
        float finalRotation = targetRotation - (spinRounds * 360f);

        // --- Opsiyonel: İnce Ayar (Offset) ---
        // Eğer ok işareti tam tepede (12 yönünde) değil de biraz sağda/soldaysa
        // bu sayıyı değiştirerek kalibre edebilirsin.
        // float indicatorOffset = 0f; 
        // finalRotation += indicatorOffset;

        // 3. Döndür
        wheelBody.DORotate(new Vector3(0, 0, finalRotation), spinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                isSpinning = false;
                Debug.Log($"Çark Durdu! Hedef: {targetSlice.name} (Index: {targetIndex})");

                LevelManager.RuntimeSlice wonSlice = targetSlice.GetData();
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.OnSpinEnded(wonSlice);
                }
            });
    }

    private void OnValidate()
    {
        if (spinButton == null) spinButton = transform.Find("ui_btn_spin")?.GetComponent<Button>();
        if (wheelBody == null) wheelBody = transform.Find("ui_image_wheel_value") ?? transform.Find("ui_image_wheel_body");
        if (slices == null || slices.Length == 0 && wheelBody != null) slices = wheelBody.GetComponentsInChildren<WheelSliceUI>();
        
        // Auto-find wheel base image component
        if (wheelImage == null && wheelBody != null)
        {
            wheelImage = wheelBody.GetComponent<Image>();
            if (wheelImage == null)
            {
                wheelImage = wheelBody.GetComponentInChildren<Image>();
            }
        }

        // Auto-find wheel indicator image component
        if (wheelIndicatorImage == null)
        {
            // Try to find by name
            var indicatorObj = transform.Find("ui_image_wheel_indicator_value");
            if (indicatorObj != null)
            {
                wheelIndicatorImage = indicatorObj.GetComponent<Image>();
            }
            else
            {
                // Search in all children
                var images = GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img.name.Contains("indicator") || img.name.Contains("pointer") || img.name.Contains("arrow"))
                    {
                        wheelIndicatorImage = img;
                        break;
                    }
                }
            }
        }
    }
}