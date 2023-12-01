using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EventManager : MonoBehaviour
{
    public Dictionary<string, bool> Flags = new Dictionary<string, bool>();
    public Dictionary<Event, float> ProbabilityPairs = new Dictionary<Event, float>();
    public EventList EventList;
    public Event ActiveEvent { get; set; }
    public Queue<Event> RecentEvents { get; set; }

    public TextAsset EventJson;
    public static EventManager Instance;

    public void Awake()
    {
        Instance = this;

        LoadJson();
        ProcessFlags();
        InitialiseProbabilityPairs();
        RecentEvents = new Queue<Event>();
    }

    public void LoadJson()
    {
        EventList = JsonUtility.FromJson<EventList>(EventJson.text);
    }

    public Event GetEventByName(string searchTerm) =>
        EventList.Events.First(x => x.Name == searchTerm);

    public void InitialiseProbabilityPairs() 
    {
        ProbabilityPairs.Clear();

        foreach (var gameEvent in EventList.Events)
        {
            ProbabilityPairs.Add(gameEvent, 0f);
        }
    }

    public void ProcessFlags()
    {
        foreach (var gameEvent in EventList.Events)
        {
            foreach (var trigger in gameEvent.Triggers)
            {
                if (trigger.Type == "FLAG") Flags.TryAdd(trigger.FlagName, false);
            }
        }
    }

    public Stack<Event> GenerateEvents()
    {
        CalculateProbabilities();
        Stack<Event> resultList = new Stack<Event>();

        float probabilitySum = 0;

        // Apply probability penalties to events that have happened recently
        ProbabilityPairs = ProbabilityPairs.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value + RecentEvents.Count(x => x.Name == kvp.Key.Name) * -0.05f
        );
        

        //Calculate the total probabilities of the events
        foreach (var probability in ProbabilityPairs)
        {
            if (probability.Value > 0) probabilitySum += probability.Value;
        }

        //For every 0.5 total probability generate an extra event
        for (float i = 0; i < probabilitySum; i += 0.5f)
        {
            if (i > 2f) break;
            var newEvent = (SelectEevent(probabilitySum));
            if (resultList.Contains(newEvent)) continue;
            if (!IsValidEvent(newEvent))
            {
                i -= 0.5f; 
                continue;
            }
            resultList.Push(newEvent);
        }

        return resultList;
    }

    public void CalculateProbabilities()
    {
        InitialiseProbabilityPairs();
        foreach (var gameEvent in EventList.Events)
        {
            foreach (var trigger in gameEvent.Triggers)
            {
                switch (trigger.Type)
                {
                    case "FOOD":
                        if (trigger.Value <= ColonyManager.Food) ProbabilityPairs[gameEvent] += trigger.Probability;
                        break;
                    case "MOOD":
                        if (trigger.Value <= ColonyManager.Mood) ProbabilityPairs[gameEvent] += trigger.Probability;
                        break;
                    case "SOLDIERS":
                        if (trigger.Value <= ColonyManager.Soldiers) ProbabilityPairs[gameEvent] += trigger.Probability;
                        break;
                    case "TRADEGOODS":
                        if (trigger.Value <= ColonyManager.TradeGoods) ProbabilityPairs[gameEvent] += trigger.Probability;
                        break;
                    case "POPULATION":
                        if (trigger.Value <= ColonyManager.Population) ProbabilityPairs[gameEvent] += trigger.Probability;
                        break;
                    case "FLAG":
                        if (Flags[trigger.FlagName]) ProbabilityPairs[gameEvent] += trigger.Probability;
                        break;
                    case "TURN":
                        if (trigger.Value <= GameManager.CurrentTurn) ProbabilityPairs[gameEvent] += trigger.Probability;
                        break;
                    default:
                        Debug.Log($"This trigger was not recognised {trigger.Type}");
                        break;
                }
            }
        }
    }

    public Event SelectEevent(float probabilitySum)
    {
        Event selectedEvent = null;
        while (selectedEvent == null)
        {
            float randomNumber = Random.Range(0, probabilitySum);
            ProbabilityPairs = ProbabilityPairs.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            foreach (var gameEvent in ProbabilityPairs)
            {
                if (randomNumber < gameEvent.Value)
                {
                    selectedEvent = gameEvent.Key;
                    break;
                }

                if (gameEvent.Value > 0) randomNumber -= gameEvent.Value;
            }
        }

        ActiveEvent = selectedEvent;
        return selectedEvent;
    }

    public void ResolveEventOption(Option option) 
    {
        ISpecialEventOutcome specialEventOutcome = null;

        foreach (var effect in option.Effects)
        {
            switch (effect.Type)
            {
                case "GAMEOVER":
                    GameManager.GameOver = true;
                    break;
                case "EVENT":
                    var gameEvent = GameManager.EventManager.GetEventByName(effect.EventName);
                    GameManager.eventsToDo.Push(gameEvent);
                    break;
                case "FOOD":
                    ColonyManager.AddFood(effect.Value);
                    break;
                case "MOOD":
                    ColonyManager.AddMood(effect.Value);
                    break;  
                case "SOLDIERS":
                    ColonyManager.AddMood(effect.Value);
                    break;
                case "TRADEGOODS":
                    ColonyManager.AddTradeGoods(effect.Value);
                    break;
                case "POPULATION":
                    ColonyManager.AddPopulation(effect.Value);
                    break;
                case "FLAG":
                    Flags[effect.FlagName] = true;
                    break;
                case "BATTLE":
                    specialEventOutcome = effect.Battle;
                    break;
                default:
                    Debug.Log($"This effect was not recognised {effect.Type}");
                    break;
            }
        }
        GameManager.HistoryManager.AddHistory(
            new HistoryRecord() {
                Name = ActiveEvent.Name,
                Description = option.Description,
                Type = HistoryType.EVENT
                }
            );
        GameManager.ResolveEvent(specialEventOutcome);
    }

    public bool IsValidEvent(Event gameEvent)
    {
        EvaluateOptions(gameEvent);
        bool valid = false;
        //check if at least 1 option is selectable
        foreach (Option option in gameEvent.Options)
        {
            if (option.Selectable) valid = true;
        }
        return valid;
    }
    public void EvaluateOptions(Event gameEvent)
    {
        foreach (Option option in gameEvent.Options)
        {
            foreach (Effect effect in option.Effects)
            {
                if (effect.Type == "FOOD")
                {
                    if (effect.Value < 0 && Math.Abs(effect.Value) > ColonyManager.Food)
                    {
                        option.Selectable = false;
                        break;
                    }
                }
                if (effect.Type == "MOOD")
                {
                    if (effect.Value < 0 && Math.Abs(effect.Value) > ColonyManager.Mood)
                    {
                        option.Selectable = false;
                        break;
                    }
                }
                if (effect.Type == "POPULATION")
                {
                    if (effect.Value < 0 && Math.Abs(effect.Value) > ColonyManager.Population)
                    {
                        option.Selectable = false;
                        break;
                    }
                }
                if (effect.Type == "TRADEGOODS")
                {
                    if (effect.Value < 0 && Math.Abs(effect.Value) > ColonyManager.TradeGoods)
                    {
                        option.Selectable = false;
                        break;
                    }
                }
                option.Selectable = true;
            }
        }
    }
}

public interface ISpecialEventOutcome
{ 

}