using Manager;
using UnityEngine.UI;

public class WinPopUp : PopUp
{
    public Button nextLevelButton;

    protected void Awake()
    {
        nextLevelButton.onClick.AddListener(OpenNextLevel);
    }

    private void OpenNextLevel()
    {
        UIManager.Instance.RemoveGoalsUI();
        StartCoroutine(GameManager.Instance.NextLevel(0.2f));
        UIManager.Instance.CloseAllPopup();
    }
}