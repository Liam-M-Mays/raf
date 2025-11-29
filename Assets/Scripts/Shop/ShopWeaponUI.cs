using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopWeaponUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private GameObject tooltip;
    private ShopTooltipUI tooltipScript;
    public Sprite soldOut;
    private Image buttonImage;

    private WeaponSO weapon;
    private ShopManager shopManager;
    private bool canPurchase = true;

    private void Awake() {
        buttonImage = GetComponent<Image>();
        tooltip = transform.Find("Tooltip")?.gameObject;
        tooltipScript = tooltip.GetComponent<ShopTooltipUI>();
    }

    public void Setup(WeaponSO weaponData, ShopManager manager) {
        weapon = weaponData;
        shopManager = manager;
        buttonImage.sprite = weapon.weaponSprite;
        buttonImage.SetNativeSize();
        tooltipScript.SetText(weapon.weaponName, weapon.cost, weapon.description);
        tooltip.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!canPurchase) return;
        tooltip.SetActive(true);

        Debug.Log($"{weapon.weaponName} - Hover Enter");
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!canPurchase) return;
        tooltip.SetActive(false);
        
        Debug.Log($"{weapon.weaponName} - Hover Exit");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tooltip.SetActive(false);
        if (!canPurchase) return;

        // Attempt to purchase the weapon through the shop manager
        if (shopManager != null && weapon != null) {
            shopManager.PurchaseWeapon(weapon, weapon.isMelee);
            buttonImage.sprite = soldOut;
            buttonImage.SetNativeSize();
            canPurchase = false;
        }
    }
}