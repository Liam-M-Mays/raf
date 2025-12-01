using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private GameObject tooltip;
    private ShopTooltipUI tooltipScript;
    public Sprite soldOut;
    private Image buttonImage;

    private ItemSO item;
    private ShopManager shopManager;

    private bool canPurchase = true;

    private void Awake() {
        buttonImage = GetComponent<Image>();
        tooltip = transform.Find("Tooltip")?.gameObject;
        tooltipScript = tooltip.GetComponent<ShopTooltipUI>();
    }

    public void Setup(ItemSO itemData, ShopManager manager) {
        item = itemData;
        shopManager = manager;
        buttonImage.sprite = item.itemSprite;
        buttonImage.SetNativeSize();
        tooltipScript.SetText(item.itemName, item.cost, item.description);
        tooltip.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (!canPurchase) return;
        tooltip.SetActive(true);

        Debug.Log($"{item.itemName} - Hover Enter");
    }

    public void OnPointerExit(PointerEventData eventData) {
        tooltip.SetActive(false);
        
        Debug.Log($"{item.itemName} - Hover Exit");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tooltip.SetActive(false);
        if (!canPurchase) return;
        // Attempt to purchase the item through the shop manager
        if (shopManager != null && item != null) {
            if(!shopManager.PurchaseItem(item)) return;
            canPurchase = false;
            buttonImage.sprite = soldOut;
            buttonImage.SetNativeSize();
        }
    }
}