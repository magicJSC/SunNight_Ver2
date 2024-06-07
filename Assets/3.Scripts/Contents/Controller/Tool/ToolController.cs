using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    protected Vector3 point;
    protected float angle;
    protected bool isWorking;
    protected Animator anim;

    private void Start()
    {
        Init();
    }
    protected virtual void Init()
    {
        anim = GetComponent<Animator>();
        point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        angle = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, transform.forward);
    }

    void Update()
    {
        if (isWorking)
            return;

        Rotate();
        Ready();
    }

    void Rotate()
    {
        point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        angle = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, transform.forward), 0.4f);
    }

    protected virtual void Ready()
    {

    }
}
