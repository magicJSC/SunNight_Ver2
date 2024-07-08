using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICaninteract
{

    public GameObject canInteractSign { get; set; }

    public void Interact();

    public void EnterPlayer(PlayerController player);
    public void ExitPlayer(PlayerController player);
}


public class NPC : MonoBehaviour
{
   
}
