using System;
using UnityEngine;

public sealed class LabInteraction : MonoBehaviour, IInteractObject
{
    string prompt;
    Action interact;
    public GameObject canInteractSign => gameObject;
    public void Configure(string message, Action action) { prompt = message; interact = action; }
    public void ShowInteractSign() => FieldHUD.Prompt(this, prompt);
    public void HideInteractSign() => FieldHUD.ClearPrompt(this);
    void Use() { if (!PlayerController.isDie && !Managers.Game.isCantPlay && !FieldHUD.IsOpen) interact?.Invoke(); }
    public void SetAction(PlayerInteract player) => player.interactAction += Use;
    public void CancelAction(PlayerInteract player) => player.interactAction -= Use;
    void OnDisable() => FieldHUD.ClearPrompt(this);
}
