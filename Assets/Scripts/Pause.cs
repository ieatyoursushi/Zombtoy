using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Pause : MonoBehaviour {
    public bool isPaused;
    public GameObject panel;
    [SerializeField] private PlayerHealth playerhealth;
    public GameObject firstPerson;
    // Use this for initialization
    private void Awake()
    {
        if (playerhealth == null)
        {
            var playerGo = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
            if (playerGo != null)
            {
                playerhealth = playerGo.GetComponent<PlayerHealth>();
            }
        }
    }
    private void Start()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            var img = panel.GetComponentInParent<Image>();
            if (img != null) img.enabled = false;
        }
        if (firstPerson == null)
        {
            firstPerson = GameObject.Find("FirstPerson");
        }
        Time.timeScale = 1;
    }
    // Update is called once per frame
    void Update () {
		if((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab)) && playerhealth != null && !playerhealth.isDead)
        {
            pause();
            Cursor.lockState = CursorLockMode.None;
        }
	}
    public void pause()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            var img = panel.GetComponentInParent<Image>();
            if (img != null) img.enabled = true;
        }
        isPaused = true;
        Time.timeScale = 0;
    }
    public void resume()
    {
        Time.timeScale = 1;
        if (panel != null) panel.SetActive(false);
        isPaused = false;
        if (firstPerson != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (panel != null)
        {
            var img = panel.GetComponentInParent<Image>();
            if (img != null) img.enabled = false;
        }
    }
    public void exit()
    {
        SceneManager.LoadScene(2);
        Time.timeScale = 1;
    }
}
