using System.Collections.Generic;
using UnityEngine.UIElements;

public class ArmyScreenUIManager : UIManager 
{
    public VisualTreeAsset UnitRecruiter;
    public VisualElement UnitList;
    private Dictionary<UnitType, Label> _unitCounts;
    void Start()
    {
        _unitCounts = new Dictionary<UnitType, Label>();
        UnitList = GetRoot().Q<VisualElement>("UnitList");
        LoadUnitList();
    }

    void Update()
    {
        //Update count values
        foreach (var unit in _unitCounts)
        {
            unit.Value.text = GameManager.Army.ArmyContents[unit.Key].ToString();
        }
    }
    void LoadUnitList()
    {
        UnitList.Clear();
        foreach (var unit in GameManager.Army.ArmyContents)
        {
            if (!unit.Key.IsRecruitable) return;

            //Load the sub elements of a unitElement
            VisualElement unitElement = UnitRecruiter.Instantiate();
            Label unitName = unitElement.Q<Label>("UnitName");
            Label unitPower = unitElement.Q<Label>("UnitPower");
            Label unitCount = unitElement.Q<Label>("UnitCount");
            Label totalUpkeep = unitElement.Q<Label>("TotalUpkeep");

            Button addUnit = unitElement.Q<Button>("AddUnit");
            Button removeUnit = unitElement.Q<Button>("RemoveUnit");

            //Populate unitcounts dictionary
            _unitCounts.Add(unit.Key, unitCount);

            //Assign the text values to the appropriate elements
            unitName.text = GameManager.TranslationManager.GetTranslation(unit.Key.Name);
            unitPower.text = unit.Key.Power.ToString();
            unitCount.text = unit.Value.ToString();
            totalUpkeep.text = "";

            //Load the upkeep text
            foreach (var upkeep in unit.Key.Upkeep)
            {
                totalUpkeep.text += GameManager.TranslationManager.GetTranslation(upkeep.Key) + " " + (upkeep.Value) + "\n";
            }
            totalUpkeep.text = totalUpkeep.text.TrimEnd('\n');

            unitElement.style.height = Length.Auto();

            //Tooltip logic
            Label toolTip = new Label();
            toolTip.AddToClassList("option-style");
            toolTip.style.position = Position.Absolute;
            toolTip.style.visibility = Visibility.Hidden;

            toolTip.text = "";

            void ShowPlusToolTip(MouseOverEvent evt)
            {
                toolTip.text = "";
                foreach (var cost in unit.Key.Cost)
                {
                    toolTip.text += "<color=red>" + GameManager.TranslationManager.GetTranslation(cost.Key) + " " + (-cost.Value) + "</color>" + "\n";
                }
                toolTip.text = toolTip.text.TrimEnd('\n');

                float mouseXPosition = evt.mousePosition.x;
                float mouseYPosition = evt.mousePosition.y;
                float tooltipXPosition = mouseXPosition;
                float tooltipYPosition = mouseYPosition;

                toolTip.style.visibility = Visibility.Visible;
                toolTip.style.left = tooltipXPosition;
                toolTip.style.top = tooltipYPosition;
            }
            void ShowMinusToolTip(MouseOverEvent evt)
            {
                toolTip.text = "";
                foreach (var cost in unit.Key.Cost)
                {
                    int value = 0;
                    if (cost.Key != "POPULATION")
                    {
                        value = cost.Value / 2;
                    }
                    else
                    {
                        value = cost.Value;
                    }

                    if(value != 0)
                    toolTip.text += "<color=#10870B>" + GameManager.TranslationManager.GetTranslation(cost.Key) + " + " + (value) + "</color>" + "\n";
                }
                toolTip.text = toolTip.text.TrimEnd('\n');

                float mouseXPosition = evt.mousePosition.x;
                float mouseYPosition = evt.mousePosition.y;
                float tooltipXPosition = mouseXPosition;
                float tooltipYPosition = mouseYPosition;

                toolTip.style.visibility = Visibility.Visible;
                toolTip.style.left = tooltipXPosition;
                toolTip.style.top = tooltipYPosition;
            }
            void HideToolTip()
            {
                toolTip.style.visibility = Visibility.Hidden;
            }

            //handle click logic
            void ClickedPlus()
            {
                GameManager.Army.RecruitUnit(unit.Key);
            }

            void ClickedMinus()
            {
                GameManager.Army.DisbandUnit(unit.Key);
            }

            //assign butotn events
            removeUnit.clicked += ClickedMinus;
            removeUnit.RegisterCallback<MouseOverEvent>(ShowMinusToolTip);
            removeUnit.RegisterCallback<MouseOutEvent>(evt => HideToolTip());
            addUnit.clicked += ClickedPlus;
            addUnit.RegisterCallback<MouseOverEvent>(ShowPlusToolTip);
            addUnit.RegisterCallback<MouseOutEvent>(evt => HideToolTip());

            GetRoot().Add(toolTip);
            UnitList.Add(unitElement);
        }
    }
}
