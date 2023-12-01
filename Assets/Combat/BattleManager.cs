using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public Dictionary<string, float> EnemyAttackPower;

    public Army Army;
    public Battle Battle;

    public BattleManager() 
    {
        EnemyAttackPower = new Dictionary<string, float>
        {
            // Add more enemy types and their attack power here
            { "REBELS", 1f },
            { "GIANT_MOLE", 20f},
            { "HEDGEMEN_INFANTRY", 3f},
            { "HEDGEMEN_KNIGHTS", 7f},
            { "BIRDMAN", 1f},
            { "CORSAIR", 3f},
            { "SKELETONS", 1f },
            { "TREEMAN", 10f},
            { "TREEDEMON", 150f},
            { "DRAGON", 500f }
        };
    }

    public string LoadEnemyStrength(Battle battle)
    {
        var enemyStrength = string.Empty;

        foreach (var enemy in battle.EnemyList)
        {
            enemyStrength += $"<b>{GameManager.TranslationManager.GetTranslation(enemy.Type)}</b>\n";
            if (enemy.Multiplier != 0 && enemy.Quantity == 0)
            {
                var enemyNumbers = (int) (enemy.Multiplier * ColonyManager.Population);
                enemyStrength += $"{GameManager.TranslationManager.GetTranslation("STRENGTH")}: {enemyNumbers} \n";
                enemyStrength += $"{GameManager.TranslationManager.GetTranslation("ATTACK_POWER")}: {EnemyAttackPower[enemy.Type] * enemyNumbers}\n";
            }
            else if (enemy.Multiplier == 0 && enemy.Quantity != 0)
            {
                var enemyNumbers = enemy.Quantity;
                enemyStrength += $"{GameManager.TranslationManager.GetTranslation("STRENGTH")}: {enemyNumbers}\n";
                enemyStrength += $"{GameManager.TranslationManager.GetTranslation("ATTACK_POWER")}: {EnemyAttackPower[enemy.Type] * enemyNumbers}\n";
            }
            else
            {
                Debug.Log($"The battle {battle.Name} has issues loading the enemy.");
            }
        }
        Battle = battle;

        return enemyStrength;
    }

    public string LoadEnemyFaction(Battle battle)
    {
        return battle.Faction;
    }

    public Army GeneratePlayerArmy()
    {
        var army = GameManager.Army;
        if (JobsUIManager.Instance.MilitiaSlider.value >=  0)
        {
            var militia = army.ArmyContents.Where(x => x.Key is Militia).Select(x => x.Key).FirstOrDefault();    
            army.ArmyContents[militia] = JobsUIManager.Instance.MilitiaSlider.value;
        }

        // The army is empty, deploy the drunk from the local tavern
        if (army.ArmyContents.Values.Sum() == 0)
        {
            army.ArmyContents.Add(new Drunk(), 1);
        }
        return army;
    }

    public string LoadFriendlyStrength()
    {
        var friendlyStrength = string.Empty;
        var army = GeneratePlayerArmy();

        foreach (var unit in army.ArmyContents)
        {
            if (unit.Value <= 0) continue;
            friendlyStrength += $"<b>{GameManager.TranslationManager.GetTranslation(GameManager.TranslationManager.GetTranslation(unit.Key.Name))}</b>\n";
            friendlyStrength += $"{GameManager.TranslationManager.GetTranslation("STRENGTH")}: {unit.Value} \n";
            friendlyStrength += $"{GameManager.TranslationManager.GetTranslation("ATTACK_POWER")}: {unit.Key.Power * unit.Value * GameModifiers.ARMY_DAMAGE_MULTIPLIER}\n";
        }
        Army = army;

        return friendlyStrength;
    }

    public (bool, Dictionary<UnitType,int>, Battle) ResolveBattle()
    {
        float? sumEnemyStrength = 0f;
        float? sumFriendlyStrength = 0f;

        // Determine how strong each army is

        foreach (var units in Army.ArmyContents)
        {
            sumFriendlyStrength += units.Key.Power * units.Value * GameModifiers.ARMY_DAMAGE_MULTIPLIER;
        }


        foreach (var units in Battle.EnemyList)
        {
            if (units.Multiplier != 0 && units.Quantity == 0)
            {
                sumEnemyStrength += (int) (EnemyAttackPower[units.Type] * (units.Multiplier * ColonyManager.Population));
                if (units.Type == "REBELS") ColonyManager.Population -= (int) (units.Multiplier * ColonyManager.Population) / 2;
            }
            else if (units.Multiplier == 0 && units.Quantity != 0)
            {
                sumEnemyStrength += units.Quantity * EnemyAttackPower[units.Type];
            }
            else
            {
                Debug.Log($"The battle {Battle.Name} has issues loading the enemy.");
            }
        }

        // Now calculate victor
        var battleVictor = sumFriendlyStrength >= sumEnemyStrength;

        //Calculate casualties
        var casualties = CalculateCasualties((float)sumEnemyStrength,battleVictor);
        AssignCasualties(casualties);

        // Enter into the histories
        GameManager.HistoryManager.AddHistory(new HistoryRecord()
        {
            Type = HistoryType.BATTLE,
            Name = Battle.Name,
            Description = battleVictor ? GameManager.TranslationManager.GetTranslation("VICTORY") : GameManager.TranslationManager.GetTranslation("DEFEAT")
        });

        // If the battle was at the colony, the player is now dead.
        if (Battle.IsAtColony && !battleVictor) GameManager.GameOver = true;

        return (battleVictor,casualties, Battle);
    }

    public void AssignCasualties(Dictionary<UnitType, int> casualties)
    {
        foreach (var unitCasualty in casualties)
        {
            if (unitCasualty.Key is Militia)
            {
                ColonyManager.AddPopulation(-unitCasualty.Value);
                GameManager.Army.ArmyContents[unitCasualty.Key] = 0;
            }
            else
            {
                GameManager.Army.ArmyContents[unitCasualty.Key] -= unitCasualty.Value;
                if (GameManager.Army.ArmyContents[unitCasualty.Key] <= 0)
                    GameManager.Army.ArmyContents[unitCasualty.Key] = 0;
            }
        }
    }
    public Dictionary<UnitType, int> CalculateCasualties(float enemyStrength, bool victory)
    {
        Dictionary<UnitType, int> casulatiesDictionary = new Dictionary<UnitType, int>();
        int damageToDeal = 0;
        if (victory)
        {
            damageToDeal = (int)(enemyStrength * 0.4f);
        }
        else
        {
            damageToDeal = (int)(enemyStrength * 0.8f);
        }

        int unitCount = Army.ArmyContents.Values.Sum();
        if (unitCount == 0) return casulatiesDictionary;
        while (damageToDeal > 0)
        {
            if (unitCount <= 0) break;
            foreach (var unitType in Army.ArmyContents)
            {
                if(unitType.Value <= 0 ) continue;
                if (UnityEngine.Random.Range(0.0000001f, 1) < (float)unitType.Value / (float)unitCount / (float)unitType.Key.Power)
                {
                    damageToDeal-= unitType.Key.Power;
                    if (casulatiesDictionary.TryAdd(unitType.Key, 1));
                    else casulatiesDictionary[unitType.Key] += 1;
                    unitCount--;
                }
            }
        }

        return casulatiesDictionary;
    }
}

[Serializable]
public class Battle : ISpecialEventOutcome
{
    public string Name;
    public string Faction;
    public Enemy[] EnemyList;
    public Outcome WinOutcome;
    public Outcome LossOutcome;
    public bool IsAtColony = false;
}

[Serializable]
public class Outcome
{
    public string Type;
    public string Name;
}

[Serializable]
public class Enemy
{
    public string Type;
    public int Quantity = 0;
    public float Multiplier = 0f;
}

public class Army
{
    public Dictionary<UnitType, int> ArmyContents;

    public Army()
    {
        ArmyContents = new Dictionary<UnitType, int>
        {
            { new Musketeer(), 0 },
            { new Cavalry(), 0 },
            { new Cannon(), 0 },
            { new Militia(), 0}
        };
    }

    public int TotalPower() 
    {
        var total = 0;

        foreach (var unit in ArmyContents) 
        {
            total += (int) (unit.Key.Power * unit.Value * GameModifiers.ARMY_DAMAGE_MULTIPLIER);
        }
        total += (int) (ColonyManager.Militia * GameModifiers.ARMY_DAMAGE_MULTIPLIER);

        return total;
    }

    public void RecruitUnit(UnitType type)
    {
        int foodCost = 0;
        int popCost = 0;
        int tradeGoodsCost = 0;

        type.Cost.TryGetValue("FOOD", out foodCost);
        type.Cost.TryGetValue("POPULATION", out popCost);
        type.Cost.TryGetValue("TRADEGOODS", out tradeGoodsCost);

        if (foodCost > ColonyManager.Food) return;
        if (popCost > ColonyManager.Population + 1) return; //Dont want to recruit last pop
        if(tradeGoodsCost > ColonyManager.TradeGoods) return;

        ArmyContents[type]++;
        ColonyManager.AddFood(-foodCost);
        ColonyManager.AddPopulation( -popCost);
        ColonyManager.AddTradeGoods(-tradeGoodsCost);
    }
    public Dictionary<string,int> GetTotalUpkeep()
    {
        Dictionary<string,int> returnDictionary = new Dictionary<string,int>();
        foreach (var unit in ArmyContents)
        {
            foreach (var upkeep in unit.Key.Upkeep)
            {
                if (returnDictionary.ContainsKey(upkeep.Key))
                {
                    returnDictionary[upkeep.Key] += upkeep.Value * unit.Value;
                }
                else
                {
                    returnDictionary.Add(upkeep.Key, upkeep.Value * unit.Value);
                }
            }
        }
        return returnDictionary;
    }

    public void DisbandUnit(UnitType type)
    {
        if (ArmyContents[type] <= 0) return;
        ArmyContents[type] -= 1;
        type.Cost.TryGetValue("FOOD", out int foodCost);
        type.Cost.TryGetValue("POPULATION", out int popCost);
        type.Cost.TryGetValue("TRADEGOODS", out int tradeGoodsCost);

        ColonyManager.AddFood(foodCost/2);
        ColonyManager.AddPopulation(popCost/2);
        ColonyManager.AddTradeGoods(tradeGoodsCost/2);
        ColonyManager.AddPopulation(type.Cost["POPULATION"]);
    }

    public void KillUnit(UnitType type)
    {
        if (ArmyContents[type] <= 0) return;
        ArmyContents[type] -= 1;
    }
}