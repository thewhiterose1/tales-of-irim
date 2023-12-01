using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;

public abstract class UnitType
    {
        public string Name;
        public int Power;
        public Dictionary<string, int> Upkeep;
        public Dictionary<string, int> Cost;
        public bool IsRecruitable = true;
    }

public class Musketeer : UnitType
{
    public static Musketeer instance;
    public Musketeer()
    {
        Name = "MUSKETEER";
        Power = 5;
        Upkeep = new Dictionary<string, int>()
        {
            { "FOOD", 1 },
            { "TRADEGOODS", 1 }
        };
        Cost = new Dictionary<string, int>()
        {
            {"TRADEGOODS", 2 },
            {"POPULATION", 1}
        };
    }
}

public class Cavalry : UnitType
{
    public Cavalry()
    {
        Name = "CAVALRY";
        Power = 10;
        Upkeep = new Dictionary<string, int>()
        {
            { "FOOD", 2 },
            { "TRADEGOODS", 1 }
        };
        Cost = new Dictionary<string, int>()
        {
            { "FOOD", 2 },
            { "TRADEGOODS", 4 },
            { "POPULATION", 1 }
        };
    }
}
public class Cannon : UnitType
{
    public Cannon()
    {
        Name = "CANNON";
        Power = 30;
        Upkeep = new Dictionary<string, int>()
        {
            { "FOOD", 2 },
            { "TRADEGOODS", 5 }
        };
        Cost = new Dictionary<string, int>()
        {
            { "TRADEGOODS", 15 },
            { "POPULATION", 2 }
        };
    }
}

public class Militia : UnitType
{
    public Militia()
    {
        Name = "MILITIA";
        Power = 3;
        Upkeep = new Dictionary<string, int>();
        Cost = new Dictionary<string, int>();
        IsRecruitable = false;
    }
}
public class Drunk : UnitType
{
    public Drunk()
    {
        Name = "DRUNK";
        Power = 2;
        Upkeep = new Dictionary<string, int>();
        Cost = new Dictionary<string, int>();
        IsRecruitable = false;
    }
}

