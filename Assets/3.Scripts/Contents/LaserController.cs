using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    LineRenderer line;


    public LayerMask layerMask;
    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine(ShootLaser());
    }


    IEnumerator ShootLaser()
    {
        while (true)
        {
            yield return null;
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.right, 50, layerMask) ;
            Vector2 point = transform.position + transform.right * 50;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.transform.TryGetComponent<IGetMonsterDamage>(out var getDamage))
                {
                    point = hit.point;
                    getDamage.GetDamage(Time.deltaTime * 10);
                    Debug.Log(getDamage);
                    break;
                }
            }
            DrawLaser(transform.position, point);
        }
    }


    void DrawLaser(Vector2 startPos,Vector2 endPos)
    {
        Debug.DrawRay(startPos, (endPos - startPos).normalized * 100, Color.green);
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
    }
}
