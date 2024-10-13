using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MiniMap : MonoBehaviour
{
    RectTransform targetSign;
    RectTransform arrowSign;

    Transform player;
    public static Vector2 targetPos;

    public static bool isTargeting;

    private void Awake()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    private void Start()
    {
        isTargeting = false;
        targetSign = Util.FindChild<RectTransform>(gameObject,"TargetSign",true);
        arrowSign = Util.FindChild<RectTransform>(gameObject,"ArrowSign",true);

        player = Managers.Game.player.transform;
        StartCoroutine(UpdateMiniMap());
    }

    IEnumerator UpdateMiniMap()
    {
        while (true)
        {
            yield return null;
           ShowTowerPos();
        }
    }

    void ShowTowerPos()
    {
        if (!isTargeting)
        {
            targetSign.gameObject.SetActive(false);
            return;
        }
        if (targetSign.gameObject.activeSelf)
        {
            if (player == null || (Mathf.Max(targetPos.y, player.transform.position.y) - Mathf.Min(targetPos.y, player.transform.position.y) < 6 || (Mathf.Max(targetPos.x, player.transform.position.x) - Mathf.Min(targetPos.x, player.transform.position.x) < 10)))
            {
                targetSign.gameObject.SetActive(false);
                return;
            }
        }
        else
        {
            if ((Mathf.Max(targetPos.y, player.transform.position.y) - Mathf.Min(targetPos.y, player.transform.position.y) > 6 && (Mathf.Max(targetPos.x, player.transform.position.x) - Mathf.Min(targetPos.x, player.transform.position.x) > 10)))
            {
                targetSign.gameObject.SetActive(true);
            }
        }

        float angle = Mathf.Atan2(targetPos.y - player.position.y, targetPos.x - player.position.x) * Mathf.Rad2Deg;
        arrowSign.rotation = Quaternion.Euler(0, 0, angle);

        angle = Mathf.PI * angle / 180;
        targetSign.anchoredPosition = new Vector2(Mathf.Cos(angle) * 70, Mathf.Sin(angle) * 70);
    }
}
