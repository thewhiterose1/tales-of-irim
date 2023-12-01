using System;
using System.Collections.Generic;
using System.Linq;

public static class ColonyManager
{
    public static int Population { get; set; } = 10;
    public static int Mood { get; set; } = 50;
    public static int Food { get; set; } = 20;
    public static int Soldiers => CalculateSoldierTotal();
    public static int TradeGoods { get; set; } = 10;
    public static int Militia => CalculateMilitiaTotal();

    public static IEnumerable<Building> Buildings = GetBuildings();

    public static void ApplyNewTurnResources() 
    {
        var projections = GetProjections();

        if (TradeGoods <= 0 && projections["TRADEGOODS"] < 0)
        {
            for (int i = 0; i < Math.Abs(projections["TRADEGOODS"]); i += 4)
            {
                var armyByPower = GameManager.Army.ArmyContents.OrderBy(x => x.Key.Power).ToDictionary(x => x.Key, x => x.Value);
                foreach (var unit in armyByPower)
                {
                    if (unit.Value > 0)
                    {
                        GameManager.Army.KillUnit(unit.Key);
                        break;
                    }
                }
            }
        }

        AddPopulation(projections["POPULATION"]);
        AddFood(projections["FOOD"]);
        AddMood(projections["MOOD"]);
        AddTradeGoods(projections["TRADEGOODS"]);
    }

    private static int CalculateMilitiaTotal()
    {
        int total = 0;

        if (JobsUIManager.Instance != null)
        {
            total += JobsUIManager.Instance.MilitiaSlider.value;
        }

        return total;
    }

    public static int CalculateSoldierTotal()
    {
        int total = 0;

        if (GameManager.Army.ArmyContents.Any())
        {
            foreach (var units in GameManager.Army.ArmyContents)
            {
                total += units.Value;
            }
        }

        return total;
    }

    public static Dictionary<string, int> GetProjections()
    {
        Dictionary<string, int> armyUpkeep = GameManager.Army.GetTotalUpkeep();
        float buildingsFoodModifier = 1;
        float buildingsMoodModifier = 1;
        float buildingsTradeGoodsModifier = 1;
        float buildingsPopulationModifier = 1;
        foreach (Building building in Buildings )
        {
            if (building.IsBuilt)
            {
                float foodEffect;
                float moodEffect;
                float tradeGoodsEffect;
                float populationEffect;
                building.Effects.TryGetValue("FOOD", out foodEffect);
                building.Effects.TryGetValue("MOOD", out moodEffect);
                building.Effects.TryGetValue("TRADEGOODS", out tradeGoodsEffect);
                building.Effects.TryGetValue("POPULATION", out populationEffect);

                buildingsFoodModifier += foodEffect;
                buildingsMoodModifier += moodEffect;
                buildingsTradeGoodsModifier += tradeGoodsEffect;
                buildingsPopulationModifier += populationEffect;
            }
        }

        armyUpkeep.TryGetValue("FOOD", out int foodUpkeep);
        armyUpkeep.TryGetValue("TRADEGOODS", out int tradeGoodUpkeep);

        // Seasonal penalties
        var seasonPenalty = GameManager.GetDate().Season == Season.Winter ? 0.6 : 1;
        float growthPenaltyScaler = 1f;

        int foodConsumption = (int)(Population * GameModifiers.POPULATION_FOOD_CONSUMPTION);
        int moodConsumption = (int)(Population * GameModifiers.POPULATION_MOOD_CONSUMPTION);

        int foodGain = (int)Math.Round(JobsUIManager.Instance.FarmerSlider.value * GameModifiers.FARMER_OUTPUT * buildingsFoodModifier * seasonPenalty);
        int foodProjection = foodGain - foodConsumption - foodUpkeep;
        if (foodProjection < 0) growthPenaltyScaler -= (float)foodProjection / 10; 

        int populationGain = (int)Math.Round(Food <= 0 ? GameModifiers.STARVATION_GROWTH_PENALTY * growthPenaltyScaler * Population : GameModifiers.POPULATION_GROWTH * Population * buildingsPopulationModifier);
        if (Population > 105) populationGain *= (105 / Population);
        int moodGain = (int)Math.Round(JobsUIManager.Instance.EntertainerSlider.value * GameModifiers.ENTERTAINER_OUTPUT * buildingsMoodModifier);
        int tradeGoodsGain = (int)Math.Round(JobsUIManager.Instance.ArtisanSlider.value * GameModifiers.ARTISAN_OUTPUT * buildingsTradeGoodsModifier);


        //guarantee some pop growth
        if (populationGain == 0) populationGain = 1;

        return new Dictionary<string, int>()
        {
            { "FOOD", foodProjection},
            { "POPULATION", populationGain },
            { "MOOD", moodGain - moodConsumption},
            { "TRADEGOODS", tradeGoodsGain - tradeGoodUpkeep }
        };
    }



    public static void AddPopulation(int populationGain)
    {
        Population += populationGain;
        if (Population < 0)
        {
            Population = 0;
            //fire game over
        }
    }

    public static void AddMood(int moodGain)
    {
        Mood += moodGain;
        if (Mood < 0)
        {
            Mood = 0;
        }
        else if (Mood > 100)
        {
            Mood = 100;
        }
    }

    public static void AddTradeGoods(int tradeGoodsGain)
    {
        TradeGoods += tradeGoodsGain;
        if (TradeGoods < 0)
        {
            TradeGoods = 0;
        }
    }
    public static void AddFood(int foodGain)
    {
        Food += foodGain;
        if (Food < 0)
        {
            Food = 0;
        }
    }

    public static void BuildBuilding(Building building)
    {
        var costs = building.Costs;
        int foodCost;
        int tradeGoodCost;
        costs.TryGetValue("FOOD", out foodCost);
        costs.TryGetValue("TRADEGOODS", out tradeGoodCost);

        if (foodCost > Food) return;
        if (tradeGoodCost > TradeGoods) return;
        if (building.IsBuilt) return;

        if (building.Effects.ContainsKey("IMPERIAL_SHIPMENTS"))
        {
            EventManager.Instance.Flags["IMPERIAL_SHIPMENTS"] = true;
        }
        else if (building.Effects.ContainsKey("ARMY_POWER"))
        {
            GameModifiers.ARMY_DAMAGE_MULTIPLIER += 0.1f;
        }

        AddFood(-foodCost);
        AddTradeGoods(-tradeGoodCost);
        building.IsBuilt = true;
    }

    public static IEnumerable<Building> GetBuildings() =>
        new List<Building>()
            { 
                new Building() { 
                    Name = "BUILDING_CHAPEL",
                    Costs = new Dictionary<string, int> {{ "TRADEGOODS", 30 }},
                    Effects = new Dictionary<string, float> { {"MOOD", 0.15f }}
                },
                new Building() {
                    Name = "BUILDING_GRANARY",
                    Costs = new Dictionary<string, int> {{ "TRADEGOODS", 30 }},
                    Effects = new Dictionary<string, float> { {"FOOD", 0.15f }}
                },
                new Building() {
                    Name = "BUILDING_WORKSHOP",
                    Costs = new Dictionary<string, int> {{ "TRADEGOODS", 30 }},
                    Effects = new Dictionary<string, float> { {"TRADEGOODS", 0.15f }}
                },
                new Building() {
                    Name = "BUILDING_PLANTATION",
                    Costs = new Dictionary<string, int> {{ "TRADEGOODS", 30 }},
                    Effects = new Dictionary<string, float> { {"TRADEGOODS", 0.15f }}
                },
                new Building() {
                    Name = "BUILDING_HARBOUR",
                    Costs = new Dictionary<string, int> {{ "TRADEGOODS", 20 }},
                    Effects = new Dictionary<string, float> { {"IMPERIAL_SHIPMENTS", 0.05f }}
                },
                new Building() {
                    Name = "BUILDING_APOTHECARY",
                    Costs = new Dictionary<string, int> {{ "TRADEGOODS", 100 }},
                    Effects = new Dictionary<string, float> { {"POPULATION", 0.3f }}
                },
                new Building() {
                    Name = "BUILDING_BARRACKS",
                    Costs = new Dictionary<string, int> {{ "TRADEGOODS", 50 }},
                    Effects = new Dictionary<string, float> { {"ARMY_POWER", 0.10f }}
                },
            };
}

public class Building
{
    public string Name;
    public bool IsBuilt;
    public Dictionary<string, int> Costs;
    public Dictionary<string, float> Effects;
}

public static class GameModifiers
{
    public const float STARVATION_GROWTH_PENALTY = -0.2f;
    public const float POPULATION_FOOD_CONSUMPTION = 1f;
    public const float POPULATION_MOOD_CONSUMPTION = 1f;
    public const float FARMER_OUTPUT = 3f;
    public const float ENTERTAINER_OUTPUT = 3f;
    public const float ARTISAN_OUTPUT = 2f;
    public const float POPULATION_GROWTH = 0.1f;
    public const int MILITIA_COST = 5;
    public static float ARMY_DAMAGE_MULTIPLIER = 1f;
}