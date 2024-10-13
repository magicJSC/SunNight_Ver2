using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IInteractObject
{
    public GameObject canInteractSign { get; }

    public void ShowInteractSign();
    public void HideInteractSign();
    public void SetAction(PlayerInteract playerInteract);

    public void CancelAction(PlayerInteract playerInteract);
}

public class NPC : MonoBehaviour
{
   
}
