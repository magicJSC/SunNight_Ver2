using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IInteractObject
{
    public GameObject canInteractSign { get; }
    public void Interact();
}

public interface ICheckCanInteract
{
    public void EnterPlayer(PlayerController player);
    public void ExitPlayer(PlayerController player);
}


public class NPC : MonoBehaviour
{
   
}
