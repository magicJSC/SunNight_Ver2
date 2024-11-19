using System.Collections;
using UnityEngine;

public class Obstacle : MonoBehaviour,IKnockBack,IGetPlayerDamage
{
    Rigidbody2D rigid;

    Stat stat;

    public int endTime { get { return 1; } }
    public float curEndTime = 0;

    private void Start()
    {
        stat = GetComponent<Stat>();
        stat.dieEvent += Die;
        rigid = GetComponent<Rigidbody2D>();
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void StartKnockBack(Vector2 dir)
    {
        curEndTime = 0;
        rigid.constraints = RigidbodyConstraints2D.None;
        rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        rigid.velocity = dir.normalized * 5;
        StartCoroutine(KnockBack());
    }

    public IEnumerator KnockBack()
    {
        while (true)
        {
            yield return null;
            if (rigid.velocity.magnitude > 0.01f && curEndTime < endTime)
            {
               rigid.velocity = Vector2.Lerp(rigid.velocity, Vector2.zero, 0.05f);
                curEndTime += Time.deltaTime;
            }
            else
                break;
        }
        rigid.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void GetDamage(float damage)
    {
        stat.GetDamage(damage);
    }
}
