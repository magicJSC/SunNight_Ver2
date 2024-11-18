using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBullet : MonoBehaviour,IKnockBack
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
                getMonsterDamageList[i].GetDamage(10);
            }
        }
        Destroy(gameObject);
    }


   

    public void StartKnockBack(Transform attacker)
    {
        rigid.velocity = (transform.position - attacker.position).normalized * 10;
    }
    public IEnumerator KnockBack()
    {
        yield return null;
    }
}
