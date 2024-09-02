using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToolController : MonoBehaviour
{
    protected Vector3 point;
    protected float angle;
    protected bool isWorking;
    protected Animator anim;

    [SerializeField]
    protected int _damage;

    private void Start()
    {
        Init();
    }
    protected virtual void Init()
    {
        anim = GetComponent<Animator>();
        point = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0,0,10);
        if (transform.parent.position.x < point.x)
        {
            angle = Mathf.Atan2(-(point.y - transform.position.y), point.x - transform.position.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 180, angle + 180);
        }
        else
        {
            angle = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        StartCoroutine(RotateToPoint());
    }

    IEnumerator RotateToPoint()
    {
        while (true)
        {
            point = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
            if (!isWorking)
            {
                if (transform.parent.position.x < point.x)
                {
                    angle = Mathf.Atan2(-(point.y - transform.position.y), point.x - transform.position.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 180, angle + 180);
                }
                else
                {
                    angle = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
            yield return null;
        }
    }

    protected virtual void OnAttack()
    {
        if (isWorking)
            return;

        anim.SetTrigger("Attack");
        isWorking = true;
    }

    void EndAtk()
    {
        isWorking = false;
    }
}
