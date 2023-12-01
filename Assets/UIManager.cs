using UnityEngine;
using UnityEngine.UIElements;

public class UILoader
{
    public EventUIManager EventUIManager;
    public ColonyMainScreenUIManager ColonyMainScreenUIManager;
    public BattleUIManager BattleUIManager;
    public GameOverUIManager GameOverUIManager;

    public void LoadEventScreen(Event gameEvent)
    {
        EventUIManager.Show();
        ColonyMainScreenUIManager.Hide();
        EventUIManager.UpdateEvent(gameEvent);
        BattleUIManager.Hide();
        GameOverUIManager.Hide();
    }

    public void LoadColonyScreen()
    {
        EventUIManager.Hide();
        ColonyMainScreenUIManager.Show();
        BattleUIManager.Hide();
        GameOverUIManager.Hide();
    }

    public void LoadBattleScreen(Battle battle)
    {
        EventUIManager.Hide();
        ColonyMainScreenUIManager.Hide();
        BattleUIManager.LoadBattle(battle);
        BattleUIManager.Show();
        GameOverUIManager.Hide();
    }

    public void LoadGameOver()
    {
        EventUIManager.Hide();
        ColonyMainScreenUIManager.Hide();
        BattleUIManager.Hide();
        GameOverUIManager.Show();
    }
}


public abstract class UIManager : MonoBehaviour
{
    public VisualElement GetRoot()
    {
        return GetComponent<UIDocument>().rootVisualElement;
    }

    public virtual void Hide()
    {
        GetRoot().style.display = DisplayStyle.None;
    }

    public virtual void Show()
    {
        GetRoot().style.display = DisplayStyle.Flex;
    }
}