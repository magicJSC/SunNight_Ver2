using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAnimator : MonoBehaviour
{
    public void MoveTransformToTarget(Transform moveTarget,Transform startTrans,Transform endTrans,float period)
    {
        if(moveTarget.GetComponent<Camera>() != null)
        {
            moveTarget.position = startTrans.position + new Vector3(0,0,-10);
            moveTarget.DOMove(endTrans.position + new Vector3(0, 0, -10), period);
        }
        else
        {
            moveTarget.position = startTrans.position;
            moveTarget.DOMove(endTrans.position, period);
        }
    }
}
