
using System.Collections;
using UnityEngine;

public interface IGetDamage
{
    void GetDamage(int damage);
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
   
    void StartKnockBack(Transform attacker);

    IEnumerator KnockBack();
}