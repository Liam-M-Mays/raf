using UnityEngine;
using TMPro;

public class ShopTooltipUI : MonoBehaviour
{
    private RectTransform tooltipRectTransform;
    private TextMeshProUGUI text;
    private RectTransform parentTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        tooltipRectTransform = transform.Find("Image").GetComponent<RectTransform>();
        text = transform.Find("Text").GetComponent<TextMeshProUGUI>();
        parentTransform = transform.GetComponent<RectTransform>();

    }

    public void SetText(string name, int price, string description) {
        string newText = $"{name}\n" +
            $"${price}\n" +
            $"{description}";
        text.SetText(newText);
        text.ForceMeshUpdate();

        Vector2 textSize = text.GetRenderedValues(false);
        Vector2 paddingSize = new Vector2(8, 8);

        tooltipRectTransform.sizeDelta = textSize + paddingSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
