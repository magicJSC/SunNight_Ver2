using UnityEngine;

public sealed class BasementTerminal : MonoBehaviour, IInteractObject
{
    public GameObject canInteractSign => gameObject;
    public void ShowInteractSign() => FieldHUD.Prompt(this, "지하 연구실 단말 · F 조사");
    public void HideInteractSign() => FieldHUD.ClearPrompt(this);
    public void SetAction(PlayerInteract player) => player.interactAction += Read;
    public void CancelAction(PlayerInteract player) => player.interactAction -= Read;

    void Read()
    {
        if (!FieldSession.State.HasBasementMap || FieldHUD.IsOpen) return;
        FieldSession.State.sunNightJournalRead = true;
        FieldHUD.ShowStory(FieldHUD.StoryPage.Journal);
    }
}
