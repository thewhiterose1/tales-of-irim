using System.Linq;
using UnityEngine.UIElements;

public class HistoryUIManager : UIManager
{
    Label Historytext;

    void Start()
    {
        Historytext = GetRoot().Q<Label>("HistoryText");
    }

    void Update()
    {
        Historytext.text = GameManager.HistoryManager.DisplayHistories();    
    }

}