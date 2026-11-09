using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformAnimator : MonoBehaviour
{
    readonly Dictionary<Transform, Tween> moves = new();

    void OnDisable()
    {
        foreach (var tween in moves.Values) tween?.Kill(false);
        moves.Clear();
    }

    public void MoveTransformToTarget(Transform moveTarget,Transform startTrans,Transform endTrans,float period)
    {
        if (!isActiveAndEnabled || moveTarget == null || startTrans == null || endTrans == null) return;
        if (moves.TryGetValue(moveTarget, out var previous)) previous.Kill(false);
        if(moveTarget.GetComponent<Camera>() != null)
        {
            moveTarget.position = startTrans.position + new Vector3(0,0,-10);
            moves[moveTarget] = moveTarget.DOMove(endTrans.position + new Vector3(0, 0, -10), period)
                .SetRecyclable(false).SetLink(moveTarget.gameObject);
        }
        else
        {
            moveTarget.position = startTrans.position;
            moves[moveTarget] = moveTarget.DOMove(endTrans.position, period)
                .SetRecyclable(false).SetLink(moveTarget.gameObject);
        }
    }
}
