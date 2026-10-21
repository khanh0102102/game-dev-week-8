using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameManager : MonoBehaviour
{
    [Header("Popup UI references")]
    public GameObject popupCanvas;
    public TMP_Text titleText;
    public TMP_Text messageText;
    public Button continueButton;
    public Button restartButton;
    public TMP_Text continueButtonText;
    public TMP_Text restartButtonText;
    public string nextSceneName = "NextScene";
    private bool popupShown;

    private void Awake()
    {
        Time.timeScale = 1f;
        if (continueButton != null) continueButton.onClick.AddListener(NextScene);
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (popupCanvas != null) popupCanvas.SetActive(false);
    }

    private void Start()
    {
        SetPopupContent("FLIGHT CHECKPOINT",
            "Press W or Up Arrow once to throttle up, take off, and fly through the orange Enter Wall.");
        SetButtonLabels("Continue", "Restart");
        if (popupCanvas != null) popupCanvas.SetActive(false);
    }

    public void ShowPopup(string title, string message)
    {
        if (popupShown) return;
        if (popupCanvas == null)
        {
            Debug.LogError("GameManager: Popup Canvas is not assigned. Run Tools > Week 8 > Setup UI Popup Practice.");
            return;
        }

        popupShown = true;
        SetPopupContent(title, message);
        SetButtonLabels("Continue", "Restart");
        popupCanvas.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("Popup displayed after the airplane took off and entered the Enter Wall trigger.");
    }

    private void SetPopupContent(string title, string message)
    {
        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;
    }

    private void SetButtonLabels(string continueLabel, string restartLabel)
    {
        if (continueButtonText != null) continueButtonText.text = continueLabel;
        if (restartButtonText != null) restartButtonText.text = restartLabel;
    }

    public void NextScene()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError("Cannot load '" + nextSceneName + "'. Run Tools > Week 8 > Setup UI Popup Practice to create it and add it to Build Settings.");
            return;
        }
        SceneManager.LoadScene(nextSceneName);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        if (continueButton != null) continueButton.onClick.RemoveListener(NextScene);
        if (restartButton != null) restartButton.onClick.RemoveListener(Restart);
        Time.timeScale = 1f;
    }
}
