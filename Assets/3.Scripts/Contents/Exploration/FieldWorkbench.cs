using UnityEngine;
public sealed class FieldWorkbench : MonoBehaviour,IInteractObject
{
    public GameObject canInteractSign => gameObject;
    public void ShowInteractSign() { FieldHUD.Prompt(this,"중앙 야영지 · 상호작용: 연구 / 탐색 진행 저장"); }
    public void HideInteractSign() { FieldHUD.ClearPrompt(this); }
    void Open() { FieldHUD.Open(this); }
    public void SetAction(PlayerInteract player) { player.interactAction+=Open; }
    public void CancelAction(PlayerInteract player) { player.interactAction-=Open; }
}
