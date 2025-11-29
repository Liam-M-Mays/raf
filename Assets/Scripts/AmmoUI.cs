using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// UI Display for the ammo system
/// Attach this to a UI Canvas
public class AmmoUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AmmoSystem ammoSystem;
    
    [Header("UI Elements - Text")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI reserveAmmoText;
    [SerializeField] private TextMeshProUGUI reloadText;
    
    [Header("UI Elements - Images (Optional)")]
    [SerializeField] private Image ammoBarFill;
    [SerializeField] private Image reloadBarFill;
    
    [Header("Display Options")]
    [SerializeField] private bool showReserveAmmo = true;
    [SerializeField] private bool showAmmoBar = true;
    [SerializeField] private bool showReloadBar = true;
    
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowAmmoColor = Color.yellow;
    [SerializeField] private Color emptyColor = Color.red;
    [SerializeField] private float lowAmmoThreshold = 0.3f;
    
    private GameObject reloadBarObject;

    void Start()
    {
        if (ammoSystem == null)
        {
            ammoSystem = FindFirstObjectByType<AmmoSystem>();
        }
        
        if (ammoSystem != null)
        {
            // Subscribe to ammo system events
            ammoSystem.OnAmmoChanged += UpdateAmmoDisplay;
            ammoSystem.OnReloadStarted += ShowReloadBar;
            ammoSystem.OnReloadComplete += HideReloadBar;
        }
        
        // Hide reload elements initially
        if (reloadText != null) reloadText.gameObject.SetActive(false);
        if (reloadBarFill != null) 
        {
            reloadBarObject = reloadBarFill.transform.parent.gameObject;
            reloadBarObject.SetActive(false);
        }
        
        // Initial update
        if (ammoSystem != null)
        {
            UpdateAmmoDisplay(ammoSystem.GetCurrentAmmo(), ammoSystem.GetMaxAmmo(), ammoSystem.GetReserveAmmo());
        }
    }

    void Update()
    {
        // Update reload bar progress
        if (ammoSystem != null && ammoSystem.IsReloading() && reloadBarFill != null)
        {
            reloadBarFill.fillAmount = ammoSystem.GetReloadProgress();
        }
    }

    void UpdateAmmoDisplay(int current, int max, int reserve)
    {
        // Update text
        if (ammoText != null)
        {
            ammoText.text = $"{current} / {max}";
            
            // Change color based on ammo amount
            float ammoPercent = (float)current / max;
            if (current == 0)
            {
                ammoText.color = emptyColor;
            }
            else if (ammoPercent <= lowAmmoThreshold)
            {
                ammoText.color = lowAmmoColor;
            }
            else
            {
                ammoText.color = normalColor;
            }
        }
        
        // Update reserve ammo
        if (reserveAmmoText != null && showReserveAmmo)
        {
            reserveAmmoText.text = $"Reserve: {reserve}";
        }
        
        // Update ammo bar
        if (ammoBarFill != null && showAmmoBar)
        {
            ammoBarFill.fillAmount = (float)current / max;
            
            // Change bar color
            float ammoPercent = (float)current / max;
            if (current == 0)
            {
                ammoBarFill.color = emptyColor;
            }
            else if (ammoPercent <= lowAmmoThreshold)
            {
                ammoBarFill.color = lowAmmoColor;
            }
            else
            {
                ammoBarFill.color = normalColor;
            }
        }
    }

    void ShowReloadBar(float reloadTime)
    {
        if (reloadText != null)
        {
            reloadText.gameObject.SetActive(true);
            reloadText.text = "RELOADING...";
        }
        
        if (reloadBarObject != null && showReloadBar)
        {
            reloadBarObject.SetActive(true);
            if (reloadBarFill != null)
            {
                reloadBarFill.fillAmount = 0f;
            }
        }
    }

    void HideReloadBar()
    {
        if (reloadText != null)
        {
            reloadText.gameObject.SetActive(false);
        }
        
        if (reloadBarObject != null)
        {
            reloadBarObject.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (ammoSystem != null)
        {
            ammoSystem.OnAmmoChanged -= UpdateAmmoDisplay;
            ammoSystem.OnReloadStarted -= ShowReloadBar;
            ammoSystem.OnReloadComplete -= HideReloadBar;
        }
    }
}