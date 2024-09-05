using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MiniMap : MonoBehaviour
{
    RectTransform towerSign;
    RectTransform arrowSign;

    Transform tower;
    Transform player;

    private void Start()
    {
        towerSign = Util.FindChild<RectTransform>(gameObject,"TowerSign",true);
        arrowSign = Util.FindChild<RectTransform>(gameObject,"ArrowSign",true);

        tower = Managers.Game.tower.transform;
        player = Managers.Game.player.transform;
        StartCoroutine(UpdateMiniMap());
    }

    IEnumerator UpdateMiniMap()
    {
        while (true)
        {
            yield return null;
            if (tower == null || player == null || Managers.Game.isKeepingTower || (Mathf.Abs(tower.transform.position.y) - Mathf.Abs(player.transform.position.y) < 7 && Mathf.Abs(tower.transform.position.x) - Mathf.Abs(player.transform.position.x) < 10))
            {
                towerSign.gameObject.SetActive(false);
                continue;
            }
            else
                towerSign.gameObject.SetActive(true);

            float angle = Mathf.Atan2(tower.position.y - player.position.y,tower.position.x - player.position.x) * Mathf.Rad2Deg;
            arrowSign.rotation = Quaternion.Euler(0,0,angle);

            angle = Mathf.PI * angle / 180;
            towerSign.anchoredPosition = new Vector2(Mathf.Cos(angle) * 70,Mathf.Sin(angle) * 70);
        }
    }
}
