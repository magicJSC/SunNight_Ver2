using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBuffReciever
{
    public List<BaseBuffGiver> buffList { get => new(); }

    public void GetBuff(BaseBuffGiver buff);
    public void StopBuff(BaseBuffGiver buff);

    IEnumerator UpdateBuff();
  
}

public class BaseBuffReciever : MonoBehaviour
{
   
}
