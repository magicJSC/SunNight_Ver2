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
            if(towerSign.gameObject.activeSelf)
            {
                if (tower == null || player == null || (Mathf.Max(tower.transform.position.y, player.transform.position.y) - Mathf.Min(tower.transform.position.y, player.transform.position.y) < 6 || (Mathf.Max(tower.transform.position.x, player.transform.position.x) - Mathf.Min(tower.transform.position.x, player.transform.position.x) < 10)))
                {
                    towerSign.gameObject.SetActive(false);
                    continue;
                }
            }
            else
            {
                if ((Mathf.Max(tower.transform.position.y, player.transform.position.y) - Mathf.Min(tower.transform.position.y, player.transform.position.y) > 6 && (Mathf.Max(tower.transform.position.x, player.transform.position.x) - Mathf.Min(tower.transform.position.x, player.transform.position.x) > 10)))
                {
                      towerSign.gameObject.SetActive(true);
                }
            }

            float angle = Mathf.Atan2(tower.position.y - player.position.y,tower.position.x - player.position.x) * Mathf.Rad2Deg;
            arrowSign.rotation = Quaternion.Euler(0,0,angle);

            angle = Mathf.PI * angle / 180;
            towerSign.anchoredPosition = new Vector2(Mathf.Cos(angle) * 70,Mathf.Sin(angle) * 70);
        }
    }
}
