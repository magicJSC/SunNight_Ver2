using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBullet : MonoBehaviour,IKnockBack,IGetPlayerDamage
{
    public List<IGetDamage> getMonsterDamageList = new List<IGetDamage>();

    public GameObject effect;
    public int endTime  { get { return 99; } }

    Rigidbody2D rigid;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
    }


    public void Explosion()
    {
        Instantiate(effect,transform.position,Quaternion.identity);
        for(int i = 0; i < getMonsterDamageList.Count; i++)
        {
            if (getMonsterDamageList[i] != null)
            {
                getMonsterDamageList[i].GetDamage(6);
            }
        }
        Destroy(gameObject);
    }


   

    public void StartKnockBack(Vector2 dir)
    {
        rigid.velocity = dir.normalized * 10;
    }
    public IEnumerator KnockBack()
    {
        yield return null;
    }

    public void GetDamage(float damage)
    {

    }
}
