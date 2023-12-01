
using System.Collections.Generic;
using UnityEngine;

public static class GameManager
{
    public static int CurrentTurn = 1;
    public static Date CurrentDate = new Date();
    public static EventManager EventManager = EventManager.Instance;
    public static TranslationManager TranslationManager = TranslationManager.Instance;
    public static HistoryManager HistoryManager = new HistoryManager();
    public static BattleManager BattleManager = new BattleManager();
    public static UILoader UILoader = new UILoader();
    public static Stack<Event> eventsToDo;

    public static Army Army = new Army();

    public static bool GameOver;

    public static void RestartGame()
    {
        Application.Quit();
    }
    public static void EndOfTurn()
    {
        ColonyManager.ApplyNewTurnResources();
        eventsToDo = EventManager.GenerateEvents();
        ResolveEvent();
    }

    public static void NextTurn()
    {
        IncrementDate();
        CurrentTurn++;
        UILoader.LoadColonyScreen();
        IsGameOver();
    }

    public static void IsGameOver()
    {
        if (GameOver)
        {
            UILoader.LoadGameOver();
        }
        if (ColonyManager.Population == 0)
        {
            UILoader.LoadGameOver();
        }
    }

    public static Date GetDate() => new Date() { Year = CurrentDate.Year, Season = CurrentDate.Season }; 

    public static void ResolveEvent(ISpecialEventOutcome specialEventOutcome = null)
    {
        if (specialEventOutcome != null) 
        {
            if (specialEventOutcome is Battle)
            {
                UILoader.LoadBattleScreen((Battle) specialEventOutcome);
            }
        }
        else {
            if (eventsToDo.Count > 0)
            {
                //evaluate if event is still valid in case previous event made it invalid
                if (!EventManager.IsValidEvent(eventsToDo.Peek()))
                {
                    eventsToDo.Pop();
                    ResolveEvent();
                }
                else
                {
                    if (eventsToDo.Peek().Triggers.Count > 0)
                    {
                        EventManager.RecentEvents.Enqueue(eventsToDo.Peek());
                    }
                    UILoader.LoadEventScreen(eventsToDo.Pop());
                    if (EventManager.RecentEvents.Count > 4)
                    { 
                        EventManager.RecentEvents.Dequeue();
                    }
                }
                IsGameOver();
            }
            else
            {
                NextTurn();
            }
        }
    }

    private static void IncrementDate()
    {
        if (CurrentDate.Season < Season.Winter)
        {
            CurrentDate.Season++;
        }
        else
        {
            CurrentDate.Season = Season.Spring;
            CurrentDate.Year++;
        }
    }
}

public enum Season
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public class Date
{
    public int Year = 1492;
    public Season Season = Season.Spring;
    public override string ToString()
    {
        return Season + " " + Year;
    }
}
