using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// First-field version. The original animation-driven boss remains available outside FieldSession.
public sealed class FieldFoxtonBattle : MonoBehaviour
{
    public static FieldFoxtonBattle Active { get; private set; }
    public bool IsCoreExposed => Phase == "Core";
    public string Phase { get; private set; } = "Ready";
    public int Pattern { get; private set; }
    public Vector2 BeamOrigin { get; private set; }
    public float BeamAngle { get; private set; }
    public bool AlternateLanes { get; private set; }
    public float StartedAt { get; private set; }
    readonly List<LineRenderer> beams = new();
    Material beamMaterial;
    float nextDamage;
    bool laneHit;
    SpriteRenderer body;
    Color bodyColor;
    bool stopped;

    IEnumerator Start()
    {
        Active=this; StartedAt=Time.time;
        // Disable the old fatal/collateral lasers. Their animation is never played in this version.
        foreach(var old in GetComponentsInChildren<StrongLaser>(true)) old.gameObject.SetActive(false);
        foreach(var old in GetComponentsInChildren<LaserController>(true)) old.gameObject.SetActive(false);
        GetComponent<Animator>().Play("DownIdle",0,0);
        body=GetComponentInChildren<SpriteRenderer>();bodyColor=body.color;
        beamMaterial=new Material(Shader.Find("Sprites/Default"));
        for(int i=0;i<4;i++)
        {
            var go=new GameObject("FoxtonTelegraph"+i);go.transform.SetParent(transform,false);
            var line=go.AddComponent<LineRenderer>();line.sharedMaterial=beamMaterial;
            line.useWorldSpace=true;line.positionCount=2;line.sortingOrder=30;
            line.enabled=false;beams.Add(line);
        }
        yield return new WaitForSeconds(5);
        int cycle=0;
        while(!stopped && !PlayerController.isDie && Managers.Game.tower.GetComponent<Stat>().Hp>0)
        {
            // First three teach A, B, C; subsequent bags contain each pattern once.
            int[] order={1,2,3};
            if(cycle>0)
            {
                for(int i=2;i>0;i--) {int j=Random.Range(0,i+1);(order[i],order[j])=(order[j],order[i]);}
                if(order[0]==Pattern) (order[0],order[1])=(order[1],order[0]);
            }
            foreach(int pattern in order)
            {
                Pattern=pattern;laneHit=false;
                BeamOrigin=pattern==3?new Vector2(1,11):new Vector2(-11,8);
                transform.position=BeamOrigin;
                Vector2 aim=(Vector2)Managers.Game.player.transform.position-BeamOrigin;
                BeamAngle=Mathf.Atan2(aim.y,aim.x)*Mathf.Rad2Deg;
                if(pattern==2) AlternateLanes=cycle%2==1;
                float angle=BeamAngle;
                Phase="Telegraph";
                float warning=pattern==1?2:3;
                for(float t=0;t<warning;t+=Time.deltaTime)
                {Draw(false);yield return null;}
                Phase="Attack";
                float duration=pattern==1?3:pattern==2?2:4;
                for(float t=0;t<duration;t+=Time.deltaTime)
                {
                    if(PlayerController.isDie) {StopBattle();yield break;}
                    if(pattern==3) BeamAngle=angle-90*t/duration;
                    Draw(true);HitPlayer();yield return null;
                }
                HideBeams();Phase="Recovery";
                yield return new WaitForSeconds(1);
                transform.position=new Vector2(1,3);
                Phase="Core";body.color=new Color(.4f,1,.7f);
                yield return new WaitForSeconds(5);
                body.color=new Color(1,.8f,.2f);
                yield return new WaitForSeconds(1);
                body.color=bodyColor;Phase="Recovery";
            }
            cycle++;
        }
        StopBattle();
    }
    void Draw(bool damaging)
    {
        int count=Pattern==2 || Pattern==3 && !damaging?4:1;
        for(int i=0;i<beams.Count;i++)
        {
            var line=beams[i];line.enabled=i<count;if(i>=count) continue;
            Vector2 a,b;float width;
            Segment(i,out a,out b,out width);
            if(Pattern==3 && !damaging)
            {
                float previewAngle=(BeamAngle-30*i)*Mathf.Deg2Rad;
                b=a+new Vector2(Mathf.Cos(previewAngle),Mathf.Sin(previewAngle))*35;
            }
            line.startWidth=line.endWidth=width;
            line.startColor=line.endColor=damaging?new Color(1,.12f,.06f,.85f):new Color(1,.8f,.1f,.35f);
            line.SetPosition(0,a);line.SetPosition(1,b);
        }
    }
    void Segment(int i,out Vector2 a,out Vector2 b,out float width)
    {
        if(Pattern==2)
        {float x=-9.5f+i*6+(AlternateLanes?3:0);a=new Vector2(x,-10);b=new Vector2(x,10);width=3;}
        else
        {a=BeamOrigin;b=a+new Vector2(Mathf.Cos(BeamAngle*Mathf.Deg2Rad),Mathf.Sin(BeamAngle*Mathf.Deg2Rad))*35;width=1;}
    }
    void HitPlayer()
    {
        if(Time.time<nextDamage || Pattern==2 && laneHit) return;
        int count=Pattern==2?4:1;
        for(int i=0;i<count;i++)
        {
            Vector2 a,b;float width;Segment(i,out a,out b,out width);
            Vector2 delta=b-a;
            // Same endpoints and width as the visible telegraph; inspect real non-trigger body colliders.
            foreach(var hit in Physics2D.OverlapBoxAll((a+b)*.5f,new Vector2(delta.magnitude,width),Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg))
            {
                if(hit.isTrigger || hit.GetComponent<PlayerController>()!=Managers.Game.player) continue;
                int damage=Pattern==2?30:20;
                Managers.Game.player.GetDamage(damage);nextDamage=Time.time+1;laneHit=true;
                return;
            }
        }
    }
    void HideBeams() {foreach(var line in beams) if(line!=null) line.enabled=false;}
    public void StopBattle()
    {
        if(stopped) return;
        stopped=true;Phase="Stopped";StopAllCoroutines();HideBeams();
        if(body!=null) body.color=bodyColor;
        if(Active==this) Active=null;
    }
    void OnDisable() {StopBattle();}
    void OnDestroy() {if(beamMaterial!=null) Destroy(beamMaterial);}
}
