using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPuzzle : MonoBehaviour
{
    public GameObject closedDoor;
    public GameObject openedDoor;

    private void Start()
    {
        closedDoor.SetActive(true);
        openedDoor.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<TowerController>() != null)
        {
            closedDoor.SetActive(false);
            openedDoor.SetActive(true);
        }
    }
}
