using Manager;
using UnityEngine.UI;

public class LossPopUp : PopUp
{
    public Button restartLevelButton;

    protected void Awake()
    {
        restartLevelButton.onClick.AddListener(RestartLevel);
    }

    private void RestartLevel()
    {
        StartCoroutine(GameManager.Instance.RestartLevel(0.2f));
    }

    public override void Open()
    {
        gameObject.SetActive(true);
    }
}