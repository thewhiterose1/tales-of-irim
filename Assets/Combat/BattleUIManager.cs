using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleUIManager : UIManager
{
    //Battle elements
    public Label Name;
    public VisualElement PreBattleImage;
    public VisualElement PostBattleImage;
    public Button AttackButton;
    public Button CloseBattleButton;
    public Label FriendlyLabel;
    public Label EnemyLabel;
    public VisualElement PreBattle;
    public VisualElement PostBattle;
    public VisualElement EnemyLogo;
    public ScrollView CasualtyView;
    public Label CasualtyTitle;

    // Images
    public Texture2D BattleWin;
    public Texture2D BattleLose;

    public static BattleUIManager Instance;

    void Start()
    {
        Instance = this;
        GameManager.UILoader.BattleUIManager = Instance;

        var root = GetRoot();
        //Load battle UI elements
        Name = root.Q<Label>("Name");
        PreBattleImage = root.Q<VisualElement>("PreBattleImage");
        PostBattleImage = root.Q<VisualElement>("PostBattleImage");
        AttackButton = root.Q<Button>("AttackButton");
        CloseBattleButton = root.Q<Button>("CloseBattleButton");
        FriendlyLabel = root.Q<Label>("FriendlyLabel");
        EnemyLabel = root.Q<Label>("EnemyLabel");
        PreBattle = root.Q<VisualElement>("PreBattle");
        PostBattle = root.Q<VisualElement>("PostBattle");
        EnemyLogo = root.Q<VisualElement>("EnemyLogo");
        CasualtyView = root.Q<ScrollView>("CasualtiesView");
        CasualtyTitle = root.Q<Label>("CasualtiesLabel");

        AttackButton.clicked += ResolveBattle;
        CloseBattleButton.clicked += ConcludeBattle;

        UpdateTranslations();
    }

    public void UpdateTranslations()
    {
        AttackButton.text = GameManager.TranslationManager.GetTranslation("ATTACK_BUTTON");
        FriendlyLabel.text = GameManager.TranslationManager.GetTranslation("FRIENDLY_FORCES") + "\n\n";
        EnemyLabel.text = GameManager.TranslationManager.GetTranslation("ENEMY_FORCES") + "\n\n";
    }

    public void ConcludeBattle() 
    {
        PreBattle.visible = true;

        GameManager.ResolveEvent();
    }

    public void ResolveBattle() 
    {
        PreBattle.visible = false;

        (bool battleOutcome, Dictionary<UnitType,int> casualties, Battle battle) = GameManager.BattleManager.ResolveBattle();

        if (battleOutcome) 
        {
            PostBattleImage.style.backgroundImage = BattleWin;
            AudioManager.instance.PlayAudioCue("HEROIC");

            if (battle.WinOutcome != null)
            {
                if (battle.WinOutcome.Type == "EVENT") 
                { 
                    var gameEvent = EventManager.Instance.GetEventByName(battle.WinOutcome.Name);
                    GameManager.eventsToDo.Push(gameEvent);
                }
            }
        } 
        else 
        {
            if (battle.LossOutcome != null)
            {
                if (battle.LossOutcome.Type == "EVENT")
                {
                    var gameEvent = EventManager.Instance.GetEventByName(battle.LossOutcome.Name);
                    GameManager.eventsToDo.Push(gameEvent);
                }
            }
            PostBattleImage.style.backgroundImage = BattleLose;
            AudioManager.instance.PlayAudioCue("EVIL");
        }

        CasualtyTitle.text = battleOutcome ? "<color=green>" + GameManager.TranslationManager.GetTranslation("VICTORY") + "</color>"
    :       "<color=red>" + GameManager.TranslationManager.GetTranslation("DEFEAT") + "</color>";

        CasualtyView.Clear();
        foreach (var casualty in casualties)
        {
            if(casualty.Value <= 0) continue;
            Label casualtyLabel = new Label();
            casualtyLabel.style.fontSize = 30;
            casualtyLabel.style.alignContent = Align.Center;
            casualtyLabel.text += "<color=red>" + "-" + casualty.Value + " " + GameManager.TranslationManager.GetTranslation(casualty.Key.Name) + "</color>";

            CasualtyView.Add(casualtyLabel);
        }
        PostBattle.visible = true;
    }

    public void LoadBattle(Battle battle)
    {
        UpdateTranslations();
        AudioManager.instance.PlayAudioCue("SUSPENSEFUL");

        Name.text = $"{GameManager.GetDate()}. {GameManager.TranslationManager.GetTranslation(battle.Name)}";

        PreBattle.visible = true;
        PostBattle.visible = false;

        EnemyLabel.text += GameManager.BattleManager.LoadEnemyStrength(battle);
        FriendlyLabel.text += GameManager.BattleManager.LoadFriendlyStrength();
        LoadFactionLogo(GameManager.BattleManager.LoadEnemyFaction(battle));
    }

    public void LoadFactionLogo(string faction)
    {
        string path = "";

        switch (faction)
        {
            case "REBELS":
                path = "NecromancerIcon";
                break;
            case "HEDGEMEN":
                path = "HedgemenIcon";
                break;
            case "EVIL":
                path = "EvilIcon";
                break;
            default: 
                Debug.Log("Faction not found");
                return;    
        }

        EnemyLogo.style.backgroundImage = (StyleBackground)Resources.Load(path);
    }
}
