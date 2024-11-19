
using System.Collections;
using UnityEngine;

public interface IGetDamage
{
    void GetDamage(float damage);
}

public interface IGetPlayerDamage : IGetDamage
{
}

public interface IGetMonsterDamage : IGetDamage
{

}

public interface IKnockBack
{
    int endTime { get;  }
   
    void StartKnockBack(Vector2 dir);

    IEnumerator KnockBack();
}