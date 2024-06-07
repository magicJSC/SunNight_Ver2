using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed;
    Rigidbody2D rigid;

    [HideInInspector]
    public UI_HotBar hotBar;
    [HideInInspector]
    public GameObject toolParent;

    List<GameObject> matters = new List<GameObject>();

    public void Init()
    {
        toolParent = Util.FindChild(gameObject, "Tool");
        rigid = GetComponent<Rigidbody2D>();
        Camera.main.GetComponent<CameraController>().target = transform;

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

        rigid.velocity = new Vector3(x, y, 0) * speed;
    }

    void Gather()
    {
        for (int i = 0; i < matters.Count; i++)
        {
            if (Managers.Inven.AddItem(matters[i].GetComponent<Item_Matter>().id))
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
