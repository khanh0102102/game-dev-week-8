using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class EnterGame : MonoBehaviour
{
    public GameManager gameManager;
    public string popupTitle = "TAKEOFF COMPLETE!";
    [TextArea(2, 4)]
    public string popupMessage = "Great job! Your airplane has taken off and reached the Enter Wall. Continue to the next scene or restart the flight.";
    private bool hasOpenedPopup;

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null) trigger.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) => TryShowPopup(other);
    private void OnTriggerStay(Collider other) => TryShowPopup(other);

    private void TryShowPopup(Collider other)
    {
        if (hasOpenedPopup) return;
        AirplaneFlightController airplane = other.GetComponentInParent<AirplaneFlightController>();
        if (airplane == null || !airplane.HasTakenOff) return;

        if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("EnterGame: GameManager was not found in the scene.");
            return;
        }

        hasOpenedPopup = true;
        gameManager.ShowPopup(popupTitle, popupMessage);
    }
}
