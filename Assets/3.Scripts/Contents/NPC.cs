using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICaninteract
{
    public bool isConnected { get; set; }
    public bool canInteract { get; set; }

    public GameObject canInteractSign { get; set; }

    public void Interact();
}


public class NPC : MonoBehaviour
{
   
}
