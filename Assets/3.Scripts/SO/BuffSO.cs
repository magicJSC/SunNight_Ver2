using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffSO", menuName = "SO/Buff")]
public class BuffSO : ScriptableObject
{
    public List<TypeAndAmount> buffList;

    [Serializable]
    public struct TypeAndAmount
    {
        public Define.BuffType buffType;
        public float amount;
    }

    public bool noCool;
    public float coolTime;
}
