using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BossPosSign : MonoBehaviour
{
    RectTransform background;
    RectTransform arrowRotate;

    Transform player;
    Transform boss;

    void Start()
    {
        background = Util.FindChild<RectTransform>(gameObject, "Background", true);
        arrowRotate = Util.FindChild<RectTransform>(gameObject, "ArrowRotate", true);

        player = Managers.Game.player.transform;
        boss = transform.parent;
        StartCoroutine(UpdateTowerSign());
    }
    IEnumerator UpdateTowerSign()
    {
        float angle;
        while (true)
        {
            yield return null;
            if (Mathf.Abs(boss.position.x - player.position.x) < 20 && Mathf.Abs(boss.position.y - player.position.y) < 12)
            {
                background.gameObject.SetActive(false);
                continue;
            }

            background.gameObject.SetActive(true);
            Vector2 dir = (boss.position - player.position).normalized;
            if (Mathf.Abs(boss.position.x - player.position.x) > Mathf.Abs(boss.position.y - player.position.y))
            {
                if (dir.x > 0)
                    background.anchoredPosition = new Vector2(840, 540 * dir.y);
                else if (dir.x < 0)
                    background.anchoredPosition = new Vector2(-840, 540 * dir.y);
            }
            else if (Mathf.Abs(boss.position.x - player.position.x) < Mathf.Abs(boss.position.y - player.position.y))
            {
                if (dir.y > 0)
                    background.anchoredPosition = new Vector2(960 * dir.x, 430);
                else if (dir.y < 0)
                    background.anchoredPosition = new Vector2(960 * dir.x, -430);
            }


            angle = Mathf.Atan2(player.position.y - boss.position.y, player.position.x - boss.position.x) * Mathf.Rad2Deg;
            arrowRotate.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
    }
}
