using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!Managers.Inven.inventoryUI.gameObject.activeSelf)
                Managers.Inven.inventoryUI.gameObject.SetActive(true);
            else
                Managers.Inven.inventoryUI.gameObject.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Q))
            Pick();
    }

    void OnMove()
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

    void Pick()
    {
        for (int i = 0; i < matters.Count; i++)
        {
            if (Managers.Inven.AddOneItem(matters[i].GetComponent<Item_Matter>().idName))
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
