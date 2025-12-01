using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;
    
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponTypeText; // Shows "RANGED" or "MELEE"
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponIndexText; // Shows "1/5" for example
    
    [Header("Ranged Weapon Stats")]
    [SerializeField] private GameObject rangedStatsPanel;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI fireRateText;
    [SerializeField] private TextMeshProUGUI fireModeText;
    //[SerializeField] private TextMeshProUGUI ammoText;
    
    /* MELEE DISABLED
    [Header("Melee Weapon Stats")]
    [SerializeField] private GameObject meleeStatsPanel;
    [SerializeField] private TextMeshProUGUI meleeDamageText;
    [SerializeField] private TextMeshProUGUI meleeRangeText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    */

    void Start()
    {
        if (weaponManager == null)
        {
            weaponManager = GetComponent<WeaponManager>();
        }
        
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged += UpdateWeaponDisplay;
            
            // Initial display
            WeaponSO currentWeapon = weaponManager.GetCurrentWeapon();
            if (currentWeapon != null)
            {
                UpdateWeaponDisplay(currentWeapon, weaponManager.GetCurrentWeaponIndex());
            }
        }
    }

    void OnDestroy()
    {
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged -= UpdateWeaponDisplay;
        }
    }

    void UpdateWeaponDisplay(WeaponSO weapon, int index)
    {
        if (weapon == null) return;
        
        // Update weapon name
        if (weaponNameText != null)
        {
            weaponNameText.text = weapon.weaponName;
        }
        
        // Update weapon type
        if (weaponTypeText != null)
        {
            weaponTypeText.text = weapon.weaponType.ToString().ToUpper();
            
            // Color code the type - Orange for ranged
            weaponTypeText.color = new Color(1f, 0.5f, 0f); // Orange for ranged
            
            /* MELEE DISABLED
            if (weapon.weaponType == WeaponType.Ranged)
            {
                weaponTypeText.color = new Color(1f, 0.5f, 0f); // Orange for ranged
            }
            else
            {
                weaponTypeText.color = new Color(0.5f, 0.5f, 1f); // Blue for melee
            }
            */
        }
        
        // Update weapon icon
        if (weaponIcon != null && weapon.weaponSprite != null)
        {
            weaponIcon.sprite = weapon.weaponSprite;
            weaponIcon.enabled = true;
        }
        else if (weaponIcon != null)
        {
            weaponIcon.enabled = false;
        }
        
        // Update weapon index (1/5, 2/5, etc.)
        if (weaponIndexText != null && weaponManager != null)
        {
            weaponIndexText.text = $"{index + 1}/{weaponManager.GetWeaponCount()}";
        }
        
        // Show only ranged stats panel
        if (rangedStatsPanel != null) rangedStatsPanel.SetActive(true);
        // if (meleeStatsPanel != null) meleeStatsPanel.SetActive(false); // MELEE DISABLED
        UpdateRangedStats(weapon);
        
        /* MELEE DISABLED
        // Show/hide appropriate stats panels
        if (weapon.weaponType == WeaponType.Ranged)
        {
            if (rangedStatsPanel != null) rangedStatsPanel.SetActive(true);
            if (meleeStatsPanel != null) meleeStatsPanel.SetActive(false);
            UpdateRangedStats(weapon);
        }
        else
        {
            if (rangedStatsPanel != null) rangedStatsPanel.SetActive(false);
            if (meleeStatsPanel != null) meleeStatsPanel.SetActive(true);
            UpdateMeleeStats(weapon);
        }
        */
    }

    void UpdateRangedStats(WeaponSO weapon)
    {
        if (damageText != null)
        {
            damageText.text = $"DMG: {weapon.projectileDamage}";
        }
        
        if (fireRateText != null)
        {
            float shotsPerSecond = 1f / weapon.fireRate;
            fireRateText.text = $"RPM: {(shotsPerSecond * 60f):F0}";
        }
        
        if (fireModeText != null)
        {
            fireModeText.text = weapon.autoFire ? "AUTO" : "SEMI";
        }
        
        /*if (ammoText != null && weapon.usesAmmo)
        {
            ammoText.text = $"AMMO: {weapon.magazineSize}/{weapon.maxReserveAmmo}";
        }
        else if (ammoText != null)
        {
            ammoText.text = "AMMO: ∞";
        } */
    }

    /* MELEE DISABLED
    void UpdateMeleeStats(WeaponData weapon)
    {
        if (meleeDamageText != null)
        {
            meleeDamageText.text = $"DMG: {weapon.meleeDamage}";
        }
        
        if (meleeRangeText != null)
        {
            meleeRangeText.text = $"RANGE: {weapon.meleeRange:F1}m";
        }
        
        if (attackSpeedText != null)
        {
            float attacksPerSecond = 1f / weapon.attackCooldown;
            attackSpeedText.text = $"SPD: {attacksPerSecond:F1}/s";
        }
    }
    */
}