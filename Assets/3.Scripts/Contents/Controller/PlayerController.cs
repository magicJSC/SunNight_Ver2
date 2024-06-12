using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    Rigidbody2D rigid;

    [HideInInspector]
    public UI_HotBar hotBar;
    [HideInInspector]
    public GameObject toolParent;

    Animator anim;
    SpriteRenderer sprite;

    List<GameObject> matters = new List<GameObject>();

    public void Init()
    {
        toolParent = Util.FindChild(gameObject, "Tool");
        rigid = GetComponent<Rigidbody2D>();
        Camera.main.GetComponent<CameraController>().target = transform;
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();  
    }

    void Update()
    {
        Move();
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!Managers.Inven.inven.gameObject.activeSelf)
                Managers.Inven.inven.gameObject.SetActive(true);
            else
                Managers.Inven.inven.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Q))
            Gather();
    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        if(x !=0)
        sprite.flipX = x < 0;

        if (y < 0)
            anim.Play("DownWalk");
        else if (y > 0)
            anim.Play("UpWalk");
        if (x != 0)
        {
            if (y > 0)
                anim.Play("UpWalk");
            else
                anim.Play("DownWalk");
        }
        anim.SetInteger("X", (int)x);
        anim.SetInteger("Y", (int)y);
        rigid.velocity = new Vector3(x, y, 0) * speed;
    }

    void Gather()
    {
        for (int i = 0; i < matters.Count; i++)
        {
            if (Managers.Inven.AddItem(matters[i].GetComponent<Item_Matter>().objName))
                matters[i].GetComponent<Item_Matter>().DestroyThis();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Item_Matter>())
        {
            matters.Add(collision.gameObject);
            collision.gameObject.GetComponent<Item_Matter>().ChangeTake();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Item_Matter>())
        {
            matters.Remove(collision.gameObject);
            collision.gameObject.GetComponent<Item_Matter>().ChangeOrigin();
        }
    }
}
