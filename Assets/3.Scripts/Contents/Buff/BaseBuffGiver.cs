using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseBuffGiver : MonoBehaviour
{
    public BuffSO buffSO;

    public float CoolTime { get { return coolTime; } set { coolTime = value; if (coolTime <= 0 && !buffSO.noCool) Destroy(gameObject); } }
    float coolTime;

    private void Start()
    {
        CoolTime = buffSO.coolTime;
        GiveBuff();
    }

    private void OnDisable()
    {
        StopBuff();
    }

    void GiveBuff()
    {
        transform.parent.TryGetComponent<IBuffReciever>(out var buffReciever);
        buffReciever.GetBuff(this);
    }

    public void StopBuff()
    {
        transform.parent.TryGetComponent<IBuffReciever>(out var buffReciever);
        buffReciever.StopBuff(this);
    }
}
