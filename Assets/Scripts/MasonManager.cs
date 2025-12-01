using UnityEngine;
using TMPro;
using System.Collections;
public class MasonManager : MonoBehaviour
{
    [SerializeField] private GameObject dayPanel;
    [SerializeField] private TextMeshProUGUI dayText;

    private WaveManager waveManager;
    private ShopSpawner shopSpawner;

    public GameObject matress;
    public GameObject player;
    
      // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waveManager = GetComponent<WaveManager>();
        shopSpawner = GetComponent<ShopSpawner>();
        StartCoroutine(StartNewDay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator DisplayDay(int dayNumber) {
        dayPanel.SetActive(true);
        dayText.SetText($"Day {dayNumber}");

        yield return new WaitForSeconds(3f);
        dayPanel.SetActive(false);
        // You can add additional logic here, such as animations or timers to hide the panel after a few seconds.
    }

    public IEnumerator StartNewDay() {
        yield return new WaitForSeconds(10f); // Wait for 2 seconds before starting new day
        StartCoroutine(DisplayDay(waveManager.GetWaveNumber() + 1));
        shopSpawner.DespawnShopBoat();
        ResetPlayerPosition();

        yield return new WaitForSeconds(3.5f); // Wait for the day display to finish

        waveManager.StartWave();
    }

    public void StartShop() {
        shopSpawner.SpawnShopBoat();
    }

    void ResetPlayerPosition() {
        matress.transform.position = new Vector3(0, 0, 0);
        player.transform.position = matress.transform.position;

    }

    public void ForButton() {
        StartCoroutine(StartNewDay());
    }
}
