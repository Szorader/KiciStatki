using UnityEngine;
using UnityEngine.UI;

public class ShipsChecker : MonoBehaviour
{
    public Button[] buttons;
    public GameObject newButton;

    void Update()
    {
        foreach (Button button in buttons)
        {
            if (button.interactable)
            {
                newButton.gameObject.SetActive(false);
                return;
            }
        }
        newButton.gameObject.SetActive(true);
    }
}