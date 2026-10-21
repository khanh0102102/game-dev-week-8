using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class NextSceneMenu : MonoBehaviour
{
    public Button returnButton;
    private void Awake()
    {
        if (returnButton != null) returnButton.onClick.AddListener(ReturnToFlight);
    }

    public void ReturnToFlight()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    private void OnDestroy()
    {
        if (returnButton != null) returnButton.onClick.RemoveListener(ReturnToFlight);
        Time.timeScale = 1f;
    }
}
