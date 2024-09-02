using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TreeEditor;
using UnityEngine;

public class bullet : MonoBehaviour
{
    [SerializeField]
    private float _bulletSpeed;
    private float _destroyBulletTime = 7;

    public Transform target;

    private void Start()
    {
        StartCoroutine(DestroyBullet());
    }

    public void Update()
    {
        transform.Translate(Vector2.right * _bulletSpeed * Time.deltaTime);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(_destroyBulletTime);
        Destroy(gameObject);
    }
}
