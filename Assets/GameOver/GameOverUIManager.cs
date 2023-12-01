
using UnityEngine.UIElements;

public class GameOverUIManager : UIManager
{
    public Label HistoryLabel;
    
    public Button RestartButton;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.UILoader.GameOverUIManager = this;
        HistoryLabel = GetRoot().Q<Label>("HistoryLabel");
        RestartButton = GetRoot().Q<Button>("RestartButton");
        RestartButton.clicked += GameManager.RestartGame;
    }

    // Update is called once per frame
    void Update()
    {
        HistoryLabel.text = GameManager.HistoryManager.DisplayHistories();
    }
}
