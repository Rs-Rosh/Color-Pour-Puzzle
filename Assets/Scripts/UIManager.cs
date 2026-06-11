using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject introPanel;
    public GameObject mainMenuPanel;
    public RectTransform spinner;

    private bool isLoading = true;

    void Start()
    {
        introPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        Invoke("ShowMainMenu", 2f);
    }

    void Update()
    {
        if (isLoading)
        {
            spinner.Rotate(0, 0, -50 * Time.deltaTime);
        }
    }

    void ShowMainMenu()
    {
        isLoading = false;
        introPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}