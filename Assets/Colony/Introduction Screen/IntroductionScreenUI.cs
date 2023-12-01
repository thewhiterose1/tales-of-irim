using UnityEngine.UIElements;

public class IntroductionScreen : UIManager
{
    public Label IntroductionText;
    public Button StartGameButton;
    public TextField ColonyNameTextField;

    void Start()
    {
        Show();
        IntroductionText = GetRoot().Q<Label>("GameIntroduction");
        StartGameButton = GetRoot().Q<Button>("StartGame");
        ColonyNameTextField = GetRoot().Q<TextField>("ColonyName");

        StartGameButton.clicked += StartGame;

        UpdateTranslations();
    }

    void UpdateTranslations()

    {
        IntroductionText.text = GameManager.TranslationManager.GetTranslation("GAME_INTRODUCTION");
    }

    void StartGame()
    {
        ColonyMainScreenUIManager.Instance.ColonyName.text =  !string.IsNullOrEmpty(ColonyNameTextField.text) ? ColonyNameTextField.text : "Imperial Expedition";
        Hide();
    }
}
