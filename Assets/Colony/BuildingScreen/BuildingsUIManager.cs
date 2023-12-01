using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildingsUIManager : UIManager
{

    public VisualTreeAsset BuildingElement;
    public VisualElement BuildingList;
    public VisualElement BuiltBuildings;

    // Start is called before the first frame update
    void Start()
    {
        var root = GetRoot();
        BuildingList = root.Q<VisualElement>("UnBuiltBuildings");
        BuiltBuildings = root.Q<VisualElement>("BuiltBuildings");
        CreateBuildingElements();
    }




    void CreateBuildingElements()
    {
        BuildingList.Clear();
        foreach (Building building in ColonyManager.Buildings)
        {
            VisualElement newBuildingElement = BuildingElement.Instantiate();
            Label buildingName = newBuildingElement.Q<Label>("BuildingName");
            Label buildingEffects = newBuildingElement.Q<Label>("BuildingEffects");
            Label buildingCosts = newBuildingElement.Q<Label>("BuildingCosts");
            Button buildButton = newBuildingElement.Q<Button>("BuildButton");

            buildingName.text = GameManager.TranslationManager.GetTranslation(building.Name);
            string effectsText = "";
            foreach (var effect in building.Effects)
            {
                effectsText += GameManager.TranslationManager.GetTranslation(effect.Key) + " +" + (effect.Value * 100) + "%" + "\n";
            }
            string costsText = "";
            foreach (var cost in building.Costs)
            {
                costsText += cost.Value + " " + GameManager.TranslationManager.GetTranslation(cost.Key) + "\n";
            }

            buildingEffects.text = effectsText;
            buildingCosts.text = costsText;
            buildButton.clicked += BuildButtonClicked;

            void BuildButtonClicked()
            {
                ColonyManager.BuildBuilding(building);
                if (building.IsBuilt)
                {
                    buildButton.style.opacity = 0;
                    buildingName.style.opacity = 0.5f;
                    buildingCosts.style.opacity = 0.5f;
                    buildingEffects.style.color = new Color(0.1f, 0.6f, 0.1f);

                    BuildingList.Remove(newBuildingElement);
                    BuiltBuildings.Add(newBuildingElement);
                }
            }
            BuildingList.Add(newBuildingElement);
        }
    }
}
