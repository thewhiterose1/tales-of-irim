using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class EventUIManager : UIManager
{
    //Event elements
    public VisualElement Image;
    public ScrollView Options;
    public Label Description;
    public Label Name;

    //ColonyResources
    public Label Food;
    public Label Soldiers;
    public Label Population;
    public Label TradeGoods;
    public Label Mood;
    public PanelSettings PanelSettings;

    public static EventUIManager Instance;
    // Start is called before the first frame update
    void Start()
    {

        Instance = this;
        GameManager.UILoader.EventUIManager = Instance;

        var root = GetRoot();

        //Load event UI elements
        Image = root.Q<VisualElement>("Image");
        Options = root.Q<ScrollView>("OptionsScroller");
        Description = root.Q<Label>("Description");
        Name = root.Q<Label>("Name");

        //Load ColonyResources UI elements
        Food = root.Q<Label>("FoodValue");
        Soldiers = root.Q<Label>("SoldiersValue");
        Population = root.Q<Label>("PopulationValue");
        TradeGoods = root.Q<Label>("TradeGoodsValue");
        Mood = root.Q<Label>("MoodValue");
        SetStats();

    }

    void SetStats()
    {
        Food.text = ColonyManager.Food.ToString();
        Soldiers.text = ColonyManager.Soldiers.ToString();
        Population.text = ColonyManager.Population.ToString();
        TradeGoods.text = ColonyManager.TradeGoods.ToString();
        Mood.text = ColonyManager.Mood.ToString();
    }

    public void UpdateTranslations()
    {
        GetRoot().Q<Label>("FoodLabel").text = GameManager.TranslationManager.GetTranslation("FOOD");
        GetRoot().Q<Label>("PopulationLabel").text = GameManager.TranslationManager.GetTranslation("POPULATION");
        GetRoot().Q<Label>("SoldiersLabel").text = GameManager.TranslationManager.GetTranslation("SOLDIERS");
        GetRoot().Q<Label>("MoodLabel").text = GameManager.TranslationManager.GetTranslation("MOOD");
        GetRoot().Q<Label>("TradeGoodsLabel").text = GameManager.TranslationManager.GetTranslation("TRADEGOODS");
    }

    void LoadEvent(Event gameEvent)
    {
        AudioManager.instance.PlayAudioCue(gameEvent.AudioCue);

        //Load image and text from event
        Image.style.backgroundImage = (StyleBackground)Resources.Load(gameEvent.Image); 
        Description.text = GameManager.TranslationManager.GetTranslation(gameEvent.Description);
        Name.text = GameManager.CurrentDate.ToString() + ", " + GameManager.TranslationManager.GetTranslation(gameEvent.Name);
        var descriptionScroller = GetRoot().Q<ScrollView>("scroller");

        //Load list of options
        VisualElement optionsContainer = Options.Q<VisualElement>("unity-content-container");
        optionsContainer.Clear();
        foreach (Option option in gameEvent.Options)
        {
            optionsContainer.Add(CreateOptionButton(option));
        }
            
        //Adjust scrollers to right size
        ForceUpdate(descriptionScroller);
        ForceUpdate(Options);
    }

    public Button CreateOptionButton(Option option)
    {
        Button optionButton = new Button();
        optionButton.text = GameManager.TranslationManager.GetTranslation(option.Description);
        optionButton.AddToClassList("option-style");

        if (!option.Selectable)
        {
            optionButton.SetEnabled(false);
        }

        Label toolTip = new Label();
        toolTip.AddToClassList("option-style");
        toolTip.style.position = Position.Absolute;
        toolTip.style.visibility = Visibility.Hidden;

        var outcomeList = option.Effects.Where(e => e.Type != "FLAG").ToList();
        for (int i = 0; i < outcomeList.Count; i++)
        {
            
            if (outcomeList[i].Type == "EVENT") continue;

            if (outcomeList[i].Type == "GAMEOVER")
            {
                toolTip.text += "<color=red>" + "Game Over" + "</color>";
            }
            else if (outcomeList[i].Type == "BATTLE")
            {
                toolTip.text += "<color=red>" + GameManager.TranslationManager.GetTranslation("WILL_START_BATTLE") + "</color>";
                if (outcomeList[i].Battle.IsAtColony == true) toolTip.text += "<color=red>" + GameManager.TranslationManager.GetTranslation("GAME_LOSING_BATTLE") + "</color>";
            }
            else
            {
                toolTip.text += outcomeList[i].Value + " " + GameManager.TranslationManager.GetTranslation(outcomeList[i].Type);
            }
            if (i != outcomeList.Count - 1) toolTip.text += "\n";
        }

        optionButton.clicked += () => OptionClicked(option);
        optionButton.clicked += () => RemoveToolTip();
        if (option.Effects.Any(x => x.Type != "FLAG"))
        {
            optionButton.RegisterCallback<MouseOverEvent>(ShowToolTip);
        }
        optionButton.RegisterCallback<MouseOutEvent>(evt => HideToolTip());


        void ShowToolTip(MouseOverEvent evt)
        {
            float mouseXPosition = evt.mousePosition.x;
            float mouseYPosition = evt.mousePosition.y;
            float tooltipXPosition = mouseXPosition;
            float tooltipYPosition = mouseYPosition;

            if (toolTip.text != "")
            {
                toolTip.style.visibility = Visibility.Visible;
            }

            toolTip.style.left = tooltipXPosition;
            toolTip.style.top = tooltipYPosition;
        }
        void HideToolTip()
        {
            toolTip.style.visibility = Visibility.Hidden;
        }

        void RemoveToolTip()
        {
            GetRoot().Remove(toolTip);
        }
        GetRoot().Add(toolTip);
        return optionButton;
    }

    public void OptionClicked(Option option)
    {
        GameManager.EventManager.ResolveEventOption(option);
    }

    public void UpdateEvent(Event _event)
    {
        if (_event != null)
        {
            LoadEvent(_event);
        }
        SetStats();
    }






    //Update scroller size
    void ForceUpdate(ScrollView view)
    {
        view.schedule.Execute(() =>
        {
            var fakeOldRect = Rect.zero;
            var fakeNewRect = view.layout;

            using var evt = GeometryChangedEvent.GetPooled(fakeOldRect, fakeNewRect);
            evt.target = view.contentContainer;
            view.contentContainer.SendEvent(evt);
        });
    }

}
