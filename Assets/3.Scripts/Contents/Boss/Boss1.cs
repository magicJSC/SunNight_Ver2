using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Boss1 : MonoBehaviour
{
    Transform laserRotate;

    GameObject laser;
    Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        laserRotate = Util.FindChild<Transform>(gameObject, "LaserRotate", true);
        laser = Util.FindChild(gameObject, "Laser", true);
    }

    private void OnEnable()
    {
        StartPatterns();
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
    }

    IEnumerator Pattern1()
    {
        Debug.Log("스킬1");
        anim.Play("Pattern1", -1, 0);
        SetPoistion();
        yield return new WaitForSeconds(2f);
        laser.SetActive(true);
        yield return new WaitForSeconds(3);
        laser.SetActive(false);
        yield return new WaitForSeconds(2);
        anim.Play("Idle");
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

    void SetPoistion()
    {
        int posIndex = Random.Range(0,4);
        float randomPos;
        switch (posIndex)
        {
            case 0:
                randomPos = Random.Range(-12.4f, 17.4f);
                transform.position = new Vector2(-14.2f,randomPos);
                laserRotate.rotation = Quaternion.Euler(0,0,0);
                break;
            case 1:
                randomPos = Random.Range(-12.4f, 17.4f);
                transform.position = new Vector2(16, randomPos);
                laserRotate.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case 2:
                randomPos = Random.Range(-14.2f, 16f);
                transform.position = new Vector2(randomPos, 17.4f);
                laserRotate.rotation = Quaternion.Euler(0, 0, 270);
                break;
            case 3:
                randomPos = Random.Range(-14.2f, 16f);
                transform.position = new Vector2(randomPos,-12.4f);
                laserRotate.rotation = Quaternion.Euler(0, 0, 90);
                break;
        }
    }
}
