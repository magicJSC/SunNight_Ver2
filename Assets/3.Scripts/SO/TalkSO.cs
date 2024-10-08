using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TalkSO", menuName = "SO/Talk")]
public class TalkSO : ScriptableObject
{
    [Serializable]
    public struct Talk
    {
        public string nameText;
        public string talkText;
        public float perTypingDelay;
        public bool noIllust;
        public Sprite illust;
        public bool isFinish;
    }

    public List<Talk> groups;
}
