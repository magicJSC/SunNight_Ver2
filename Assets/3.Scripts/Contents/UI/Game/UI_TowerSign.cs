using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class UI_TowerSign : MonoBehaviour
{
    RectTransform background;
    RectTransform arrowRotate;

    Transform player;
    Transform tower;

    void Start()
    {
        background = Util.FindChild<RectTransform>(gameObject,"Background",true);
        arrowRotate = Util.FindChild<RectTransform>(gameObject, "ArrowRotate", true);

        player = Managers.Game.player.transform;
        tower = Managers.Game.tower.transform;
        StartCoroutine(UpdateTowerSign());
    }
    IEnumerator UpdateTowerSign()
    {
        float angle;
        while (true)
        {
            yield return null;
            if(Mathf.Abs(tower.position.x - player.position.x) < 20 || Mathf.Abs(tower.position.y - player.position.y) < 12)
            {
                background.gameObject.SetActive(false);
                continue;
            }

            background.gameObject.SetActive(true);
            Vector2 dir = (tower.position - player.position).normalized;
            if (Mathf.Abs(tower.position.x - player.position.x) > Mathf.Abs(tower.position.y - player.position.y))
            {
                if (dir.x > 0)
                    background.anchoredPosition = new Vector2(840, 540 * dir.y);
                else if (dir.x < 0)
                    background.anchoredPosition = new Vector2(-840, 540 * dir.y);
            }
            else if (Mathf.Abs(tower.position.x - player.position.x) < Mathf.Abs(tower.position.y - player.position.y))
            {
                if (dir.y > 0)
                    background.anchoredPosition = new Vector2(960 * dir.x, 430);
                else if (dir.y < 0)
                    background.anchoredPosition = new Vector2(960 * dir.x, -430);
            }


            angle = Mathf.Atan2(player.position.y - tower.position.y, player.position.x - tower.position.x) * Mathf.Rad2Deg;
            arrowRotate.rotation = Quaternion.Euler(0,0,angle - 90);
        }
    }
}
