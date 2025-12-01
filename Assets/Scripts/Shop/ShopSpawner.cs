using UnityEngine;
using UnityEngine.InputSystem;

public class ShopSpawner : MonoBehaviour
{
    public GameObject shopBoatPrefab;

    private GameObject shop;
    private Animator shopAnimator;

    public float minDistance = 20f;
    public float maxDistance = 40f;

    private ShopIndicator shopIndicator;
    private Transform player;

    private bool shopActive = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        shopIndicator = GameObject.FindGameObjectWithTag("Raft").GetComponent<ShopIndicator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (InputSystem.actions.FindAction("Jump").triggered) {
            if (!shopActive) {
                SpawnShopBoat();
                shopActive = true;
            } else {
                DespawnShopBoat();
                shopActive = false;
            }
        }
    }

    public void SpawnShopBoat() {
        // Get random direction in 2D space (XY plane for 2D games)
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        
        // Get random distance between min and max
        float randomDistance = Random.Range(minDistance, maxDistance);
        
        // Calculate final position in 2D (XY plane, Z stays at 0)
        Vector3 offset = new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;
        Vector3 randomPosition = player.position + offset;
        
        // Ensure Z is at 0 for 2D
        randomPosition.z = 0;
        
        shop = Instantiate(shopBoatPrefab, randomPosition, Quaternion.identity);
        shopAnimator = shop.GetComponent<Animator>();

        shopIndicator.ShowIndicator();
    }

    public void CloseShop() {
        shopAnimator.SetTrigger("Close");
    }
    

    public void DespawnShopBoat() {
        shopIndicator.HideIndicator();
        if (shop != null) {
            Destroy(shop);
        }
    }
}
