using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Die : MonoBehaviour
{
    void Spawn()
    {
        Managers.Game.player.gameObject.SetActive(true);
        Managers.Game.player.transform.position = Managers.Game.tower.transform.position + new Vector3(0, -1);
    }

    void Disappear()
    {
        Destroy(gameObject);
    }
}
