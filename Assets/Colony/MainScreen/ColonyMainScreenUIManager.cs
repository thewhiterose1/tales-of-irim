using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ColonyMainScreenUIManager : UIManager
{

    //Screens
    public VisualElement MainContent;
    public VisualElement JobsContent;
    public VisualElement BuildingsContent;
    public VisualElement HistoriesContent;
    public VisualElement ArmyContent;

    public VisualElement ColonyImage;

    //Menu buttons
    public Button MainButton;
    public Button JobsButton;
    public Button BuildingsButton;
    public Button HistoryButton;
    public Button ArmyButton;

    //ColonyResources
    public Button EndTurnButton;
    public Label FoodValue;
    public Label SoldiersValue;
    public Label PopulationValue;
    public Label TradeGoodsValue;
    public Label MoodValue;
    public Label DateValue;
    public Label ColonyDescription;
    public Label ColonyName;
    public Label MilitiaValue;
    public Label ArmyPowerValue;

    // Resource projections
    public Label ProjectedFood;
    public Label ProjectedPopulation;
    public Label ProjectedTradeGoods;
    public Label ProjectedMood;

    // Colony season images
    public Texture2D[] ColonyImages = new Texture2D[3];

    public Dictionary<string, int> Projections;

    public static ColonyMainScreenUIManager Instance;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        GameManager.UILoader.ColonyMainScreenUIManager = Instance;

        var root = GetRoot();

        EndTurnButton = root.Q<Button>("EndTurnButton");
        DateValue = root.Q<Label>("Date");
        ColonyName = root.Q<Label>("ColonyName");

        // Load ColonyResources UI elements
        FoodValue = root.Q<Label>("FoodValue");
        SoldiersValue = root.Q<Label>("SoldiersValue");
        MilitiaValue = root.Q<Label>("MilitiaValue");
        ArmyPowerValue = root.Q<Label>("PowerValue");
        PopulationValue = root.Q<Label>("PopulationValue");
        TradeGoodsValue = root.Q<Label>("TradeGoodsValue");
        MoodValue = root.Q<Label>("MoodValue");

        // Load screen contents
        MainContent = root.Q<VisualElement>("MainContent");
        JobsContent = root.Q<VisualElement>("JobsContent");
        BuildingsContent = root.Q<VisualElement>("BuildingsContent");
        HistoriesContent = root.Q<VisualElement>("HistoriesContent");
        ColonyImage = root.Q<VisualElement>("ColonyImage");
        ArmyContent = root.Q<VisualElement>("ArmyContent");

        // Load menu buttons
        MainButton = root.Q<Button>("MainButton");
        JobsButton = root.Q<Button>("JobsButton");
        BuildingsButton = root.Q<Button>("BuildingsButton");
        HistoryButton = root.Q<Button>("HistoryButton");
        ArmyButton = root.Q<Button>("ArmyButton");

        // Load resource projections
        ProjectedFood = root.Q<Label>("ProjectedFood");
        ProjectedPopulation = root.Q<Label>("ProjectedPopulation");
        ProjectedTradeGoods = root.Q<Label>("ProjectedTradeGoods");
        ProjectedMood = root.Q<Label>("ProjectedMood");

        // Colony Overview
        ColonyDescription = root.Q<Label>("ColonyDescription");

        EndTurnButton.clicked += EndTurnPressed;
        MainButton.clicked += LoadMainContent;
        JobsButton.clicked += LoadJobsContent;
        BuildingsButton.clicked += LoadBuildingContent;
        HistoryButton.clicked += LoadHistoriesContent;
        ArmyButton.clicked += LoadArmyContent;

        SetStats();
        UpdateTranslations();
        UpdateColonyImage();
        LoadMainContent();
    }

    void Update()
    {
        SetStats();
        SetColonyOverviewText();
        Projections = ColonyManager.GetProjections();

        ProjectedFood.text = Projections["FOOD"] > 0 ? "<color=#10870B>" + $"+{Projections["FOOD"]}" : "<color=red>" + Projections["FOOD"];
        ProjectedPopulation.text = Projections["POPULATION"] > 0 ? "<color=#10870B>" + $"+{Projections["POPULATION"]}" : "<color=red>" + Projections["POPULATION"];
        ProjectedMood.text = Projections["MOOD"] > 0 ? "<color=#10870B>" + $"+{Projections["MOOD"]}" : "<color=red>" + Projections["MOOD"];
        ProjectedTradeGoods.text = Projections["TRADEGOODS"] > 0 ? "<color=#10870B>" + $"+{Projections["TRADEGOODS"]}": "<color=red>" + Projections["TRADEGOODS"];
    }

    void SetStats()
    {
        FoodValue.text = ColonyManager.Food.ToString();
        SoldiersValue.text = ColonyManager.Soldiers.ToString();
        MilitiaValue.text = ColonyManager.Militia.ToString();
        PopulationValue.text = ColonyManager.Population.ToString();
        TradeGoodsValue.text = ColonyManager.TradeGoods.ToString();
        MoodValue.text = ColonyManager.Mood.ToString();
        DateValue.text = GameManager.CurrentDate.ToString();
        ArmyPowerValue.text = GameManager.Army.TotalPower().ToString();
    }

    public void UpdateTranslations() 
    {
        GetRoot().Q<Label>("FoodLabel").text = GameManager.TranslationManager.GetTranslation("FOOD");
        GetRoot().Q<Label>("PopulationLabel").text = GameManager.TranslationManager.GetTranslation("POPULATION");
        GetRoot().Q<Label>("SoldierLabel").text = GameManager.TranslationManager.GetTranslation("SOLDIERS");
        GetRoot().Q<Label>("MilitiaLabel").text = GameManager.TranslationManager.GetTranslation("MILITIA");
        GetRoot().Q<Label>("PowerLabel").text = GameManager.TranslationManager.GetTranslation("TOTAL_POWER");
        GetRoot().Q<Label>("MoodLabel").text = GameManager.TranslationManager.GetTranslation("MOOD");
        GetRoot().Q<Label>("TradegoodsLabel").text = GameManager.TranslationManager.GetTranslation("TRADEGOODS");
        GetRoot().Q<Button>("MainButton").text = GameManager.TranslationManager.GetTranslation("MAIN_BUTTON");
        GetRoot().Q<Label>("ResourceLabel").text = GameManager.TranslationManager.GetTranslation("RESOURCES_LABEL");
        GetRoot().Q<Button>("BuildingsButton").text = GameManager.TranslationManager.GetTranslation("BUILDINGS_BUTTON");
        GetRoot().Q<Button>("MainButton").text = GameManager.TranslationManager.GetTranslation("MAIN_BUTTON");
        GetRoot().Q<Button>("JobsButton").text = GameManager.TranslationManager.GetTranslation("JOBS_BUTTON");
        GetRoot().Q<Button>("ArmyButton").text = GameManager.TranslationManager.GetTranslation("ARMY_BUTTON");
        GetRoot().Q<Button>("EndTurnButton").text = GameManager.TranslationManager.GetTranslation("ENDTURN_BUTTON");
        GetRoot().Q<Button>("HistoryButton").text = GameManager.TranslationManager.GetTranslation("HISTORY_BUTTON");
    }

    #region ColonyOverviewDescription

    public string GetSeasonOverview()
    {
        var seasonOverview = string.Empty;

        if (GameManager.CurrentDate.Season == Season.Spring) seasonOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_SPRING")}";
        else if (GameManager.CurrentDate.Season == Season.Summer) seasonOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_SUMMER")}";
        else if (GameManager.CurrentDate.Season == Season.Autumn) seasonOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_AUTUMN")}";
        else if (GameManager.CurrentDate.Season == Season.Winter) seasonOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_WINTER")}";

        return seasonOverview;
    }

    // Probably should be moved later, could do some complicated stuff here
    public void SetColonyOverviewText()
    {
        var description = string.Empty;

        // Starting turn extra text
        if (GameManager.CurrentTurn == 1)
        {
            description = GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_INITIAL");
            description += "\n\n";
        };

        // Economic overview
        string economicOverview = string.Empty;

        if (GameManager.CurrentTurn == 1)
        {
            economicOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_ECONOMIC_INITIAL")}";
        }
        else
        {
            economicOverview += GetSeasonOverview();
            if (Projections["FOOD"] < 0 && ColonyManager.Food == 0) economicOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_FAMINE")} ";
            if (ColonyManager.TradeGoods <= 0 && Projections["TRADEGOODS"] < 0) economicOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_NEGATIVETRADEGOODS")} ";
            if (ColonyManager.Population < 10) economicOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_LOWPOPULATION")} ";
        }

        if (economicOverview != string.Empty) economicOverview += "\n\n";
        description += economicOverview;

        // Diplomatic/military overview
        string diplomaticMilitaryOverview = string.Empty;
        if (GameManager.CurrentTurn < 6)
        {
            diplomaticMilitaryOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_THREAT_LOW")}";
        }
        else
        {
            diplomaticMilitaryOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_GENERAL_THREAT")} ";
            if (ColonyManager.Mood + Projections["MOOD"] < 40) diplomaticMilitaryOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_REVOLTRISK")} ";
            if (EventManager.Instance.Flags["WICKED_WILLOW_AWAKE"] && !EventManager.Instance.Flags["WICKED_WILLOW_DEAD"]) diplomaticMilitaryOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_WILLOW_RISK")} ";
            if (EventManager.Instance.Flags["HEDGEMEN_WAR"] && !EventManager.Instance.Flags["HEDGEMEN_DEAD"]) diplomaticMilitaryOverview += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_HEDGEMEN_WAR")}";
        }

        if (diplomaticMilitaryOverview != string.Empty) diplomaticMilitaryOverview += "\n\n";
        description += diplomaticMilitaryOverview;

        // Quest overview
        string questOverView = string.Empty;
        if (!GameManager.EventManager.Flags["GAME_WON"])
        {
            if (GameManager.EventManager.Flags["FLAG_BIRDS_OF_TRADE_2_DONE"])
            {
                questOverView += GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_BIRDS_DONE") + " ";
            }
            if (GameManager.EventManager.Flags[("FLAG_WYRDTREE_DONE")] && GameManager.EventManager.Flags["FLAG_HEDGEMEN_DONE"])
            {
                questOverView += GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_DRAGON_FINDABLE") + " ";
            }
            else if (GameManager.EventManager.Flags[("FLAG_HEDGEMEN_DONE")])
            {
                questOverView += GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_HEDGEMEN_DONE") + " ";
            }
            else if (GameManager.EventManager.Flags[("FLAG_WYRDTREE_DONE")])
            {
                questOverView += GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_WYRDTREES_DONE") + " ";
            }

            if (questOverView == string.Empty)
            {
                questOverView += $"{GameManager.TranslationManager.GetTranslation("COLONY_DESCRIPTION_STORY_INITIAL")} ";
            }
        }
        else 
        {
            questOverView += GameManager.TranslationManager.GetTranslation("GAME_WON");
        }
        description += questOverView;

        ColonyDescription.text = description;
    }
    #endregion

    void LoadJobsContent()
    {
        HideAll();
        JobsContent.style.display = DisplayStyle.Flex;
    }

    void LoadMainContent()
    {
        HideAll();
        MainContent.style.display = DisplayStyle.Flex;
    }

    void LoadBuildingContent()
    {
        HideAll();
        BuildingsContent.style.display = DisplayStyle.Flex;
    }

    void LoadHistoriesContent()
    {
        HideAll();
        HistoriesContent.style.display = DisplayStyle.Flex;
    }

    void LoadArmyContent()
    {
        HideAll();
        ArmyContent.style.display = DisplayStyle.Flex;
    }

    void HideAll()
    {
        JobsContent.style.display = DisplayStyle.None;
        MainContent.style.display = DisplayStyle.None;
        BuildingsContent.style.display = DisplayStyle.None;
        HistoriesContent.style.display = DisplayStyle.None;
        ArmyContent.style.display = DisplayStyle.None;
    }

    public void UpdateColonyImage()
    {
        ColonyImage.style.backgroundImage = ColonyImages.First(x => x.name == $"Colony{GameManager.GetDate().Season}");
    }

    void EndTurnPressed()
    {
        GameManager.EndOfTurn();
        SetStats();
    }

    public override void Show() 
    {
        base.Show();

        UpdateColonyImage();
        LoadMainContent();
        SetStats();
    }
}
