using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_BossStat : MonoBehaviour
{
    Image fillImage;


    private void Start()
    {
        fillImage = Util.FindChild<Image>(gameObject, "Hp", true);
        transform.GetComponentInParent<Stat>().hpEvent += SetHp;
        fillImage.fillAmount = 0;
        fillImage.DOFillAmount(1f, 2f);
    }

    public void SetHp(Stat stat)
    {
        if (stat == null)
            return;
        StartCoroutine(UpdateHPBar(stat));
    }

    IEnumerator UpdateHPBar(Stat stat)
    {
        float hp = stat.Hp;
        while (true)
        {
            yield return null;

            if (Mathf.Approximately(fillImage.fillAmount, stat.Hp / stat.maxHP))
                yield break;
            if (hp != stat.Hp)
                yield break;
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, stat.Hp / stat.maxHP, 0.1f);
        }
    }
}
