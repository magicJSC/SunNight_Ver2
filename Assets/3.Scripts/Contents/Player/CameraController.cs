using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform player;

    public Vector2 center;
    public Vector2 size;
    float height;
    float width;

    bool shaking;

    private void Start()
    {
        player = Managers.Game.player.transform;
        height = Camera.main.orthographicSize;
        width = height * Screen.width / Screen.height;
    }

    private void OnEnable()
    {
        StartCoroutine(MovePlayer());
        
    }
    IEnumerator MovePlayer()
    {
        while (true)
        {
            yield return null;

            if (player == null || shaking)
                continue;
            transform.position = player.position;
            float lx = size.x * 0.5f - width;
            float clampX = Mathf.Clamp(transform.position.x,-lx + center.x,lx + center.x);
            float ly = size.y * 0.5f - height;
            float clampY = Mathf.Clamp(transform.position.y, -ly + center.y, ly + center.y);

            transform.position = new Vector3(clampX, clampY, -10);
        }
    }

    public void Shake(float duration,float strength)
    {
        transform.parent = Managers.Game.player.transform;
        shaking = true;
        transform.DOShakePosition(duration, strength).onComplete += End;
    }
    void End()
    {
        shaking = false;
        transform.parent = null;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }
}
