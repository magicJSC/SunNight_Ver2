using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="WaveSO",menuName ="SO/Wave")]
public class WaveSO : ScriptableObject
{
    [Serializable]
   public struct Group
    {
        public GameObject monster;
        public int count;
        public float spawnDelay;
        public float groupDelay;
    }

    public List<Group> groups;
}
