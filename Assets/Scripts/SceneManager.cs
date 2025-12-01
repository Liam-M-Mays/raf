using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{

    public GameObject menu;

    public void PlayGame() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleSceneWorking");
    }

    public void openMenu() {
        if (menu != null) {
            menu.SetActive(true);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
