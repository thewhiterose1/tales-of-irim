using System;
using System.Collections.Generic;

/*
 * These are the possible event factors:
 * 
 *  FOOD,
    TRADEGOODS,
    SOLDIERS,
    TURN,
    POPULATION,
    MOOD,
    BATTLE,
    FLAG
 */

public class EventList
{
    public List<Event> Events;
}

[Serializable]
public class Event : ISpecialEventOutcome
{
    public string Name;
    public string Description;
    public string Image;
    public string Type = "EVENT";
    public string AudioCue;

    public List<Trigger> Triggers;
    public List<Option> Options;
}

[Serializable]
public class Trigger
{
    public string Type;
    public string FlagName;
    public float Probability;
    public int Value = 0;
}

[Serializable]
public class Option
{
    public string Description;
    public List<Effect> Effects;
    public bool Selectable = true;
}

[Serializable]
public class Effect
{
    public string Type;
    public string FlagName;
    public string EventName;
    public int Value = 0;
    public Battle Battle;
}