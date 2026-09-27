using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject[] cards;

    private void Start()
    {
        ShowCard(0);
    }

    public void ShowCard(int index)
    {
        if (index < 0 || index >= cards.Length)
            return;

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].SetActive(i == index);
        }
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}