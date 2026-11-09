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
        if (!FieldLaboratory.PlayerInside || !FieldSession.State.HasBasementMap || FieldHUD.IsOpen) return;
        if (!FieldSession.State.sunNightJournalRead)
            FieldHUD.Notify("단말에서 지구도를 복원했다. 지도에 야영지와 폭스턴 공장을 표시했다.");
        FieldSession.State.sunNightJournalRead = true;
        FieldHUD.ShowStory(FieldHUD.StoryPage.Journal);
    }
}
