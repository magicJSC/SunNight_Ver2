using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Die : MonoBehaviour
{
    void Spawn()
    {
        Managers.Game.player.gameObject.SetActive(true);
    }

    void Disappear()
    {
        Destroy(gameObject);
    }
}
