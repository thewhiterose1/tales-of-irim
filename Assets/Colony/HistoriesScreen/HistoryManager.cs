using System.Collections.Generic;
using System.Linq;

public class HistoryManager
{
    private List<HistoryRecord> Histories = new List<HistoryRecord>();

    public void AddHistory(HistoryRecord history)
    { 
        Histories.Add(history);
    }

    public IEnumerable<HistoryRecord> GetHistories => 
        Histories?.OrderBy(x => x.Date.Year)
        .ThenBy(x => x.Date.Season) ?? Enumerable.Empty<HistoryRecord>();


    public string DisplayHistories()
    {
        var text = string.Empty;

        if (!GameManager.HistoryManager.GetHistories.Any())
        {
            return GameManager.TranslationManager.GetTranslation("COLONY_NO_HISTORY");
        }

        foreach (var grouping in GameManager.HistoryManager.GetHistories.GroupBy(record => new { record.Date.Year, record.Date.Season }))
        {
            text += grouping.First().Date.Year + ", " + grouping.First().Date.Season + "\n";

            foreach (var history in grouping)
            {
                text += $"\t The {history.Type.ToString().ToLower()} \"{GameManager.TranslationManager.GetTranslation(history.Name)}\" happend with an outcome of \"{GameManager.TranslationManager.GetTranslation(history.Description)}\" \n";
            }
            text += "\n";
        }

        return text;
    }
}

public class HistoryRecord
{
    public HistoryRecord() 
    {
        Date = GameManager.GetDate();
    }

    public Date Date { get; set; }
    public HistoryType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public enum HistoryType
{ 
    EVENT,
    BATTLE
}
