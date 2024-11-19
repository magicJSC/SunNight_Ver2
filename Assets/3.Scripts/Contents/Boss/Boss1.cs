using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Boss1 : MonoBehaviour
{
    public GameObject energyBomb;

    public GameObject timeline;

    public GameObject changeEffect;
    public GameObject dieEffect;

    Transform laserRotate;

    Stat stat;

    GameObject laser;
    Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        laserRotate = Util.FindChild<Transform>(gameObject, "LaserRotate", true);
        laser = Util.FindChild(gameObject, "Laser", true);
        stat = GetComponent<Stat>();
        stat.dieEvent += Die;
    }

    private void OnEnable()
    {
        StartPatterns();
    }

    void Die()
    {
        Instantiate(timeline);
        Instantiate(dieEffect,transform.position,Quaternion.identity);
        Destroy(gameObject);
    }

    void StartPatterns()
    {
        int patternIndex = Random.Range(0, 3);
        switch (patternIndex)
        {
            case 0:
                StartCoroutine(Pattern1());
                break;
            case 1:
                StartCoroutine(Pattern2());
                break;
            case 2:
                StartCoroutine(Pattern3());
                break;
        }
        Instantiate(changeEffect, transform.position, Quaternion.identity);
    }

    IEnumerator Pattern1()
    {
        Debug.Log("스킬1");
        laser.SetActive(false);
        
        int index = SetPoistion();
        yield return new WaitForSeconds(2f);
        laser.SetActive(true);
        yield return new WaitForSeconds(3);
        laser.SetActive(false);
        
        for(int i=0;i<3; i++)
        {
            SetBombPoistion(Instantiate(energyBomb).transform);
        }

        switch (index)
        {
            case 0:
                anim.Play("DownIdle");
                break;
        }
        anim.Play("DownIdle");
        yield return new WaitForSeconds(6);
        StartPatterns();
    }

    IEnumerator Pattern2()
    {
        Debug.Log("스킬2");
        anim.Play("Pattern2",-1,0);
        transform.position = new Vector2(1.4f, 19.2f);
        yield return new WaitForSeconds(5);
        anim.Play("Idle");
        yield return new WaitForSeconds(2);
        StartPatterns();
    }

    IEnumerator Pattern3()
    {
        Debug.Log("스킬3");
        anim.Play("Pattern3", -1, 0);
        transform.position = new Vector2(1.4f,19.2f);
        laserRotate.rotation = Quaternion.Euler(new Vector3(0,0,-30));
        yield return new WaitForSeconds(3);
        laserRotate.DORotateQuaternion(Quaternion.Euler(new Vector3(0, 0, -140)),5);
        laser.SetActive(true);
        yield return new WaitForSeconds(5);
        laser.SetActive(false);
        yield return new WaitForSeconds(2);
        anim.Play("Idle");
        StartPatterns();
    }

    int SetPoistion()
    {
        int posIndex = Random.Range(0,3);
        float randomPos;
        switch (posIndex)
        {
            case 0:
                randomPos = Random.Range(-3f, 8f);
                transform.position = new Vector2(-14.2f,randomPos);
                laserRotate.rotation = Quaternion.Euler(0,0,0);
                anim.Play("DownPattern1", -1, 0);
                break;
            case 1:
                randomPos = Random.Range(-3f, 8);
                transform.position = new Vector2(16, randomPos);
                laserRotate.rotation = Quaternion.Euler(0, 0, 180);
                anim.Play("DownPattern1", -1, 0);
                break;
            case 2:
                randomPos = Random.Range(-8, 3f);
                transform.position = new Vector2(randomPos, 17.4f);
                laserRotate.rotation = Quaternion.Euler(0, 0, 270);
                anim.Play("DownPattern1", -1, 0);
                break;
        }
        return posIndex;
    }

    void SetBombPoistion(Transform bombTransform)
    {
        bombTransform.position = new Vector3(Random.Range(-3,8),Random.Range(-8f,3));
    }
}
