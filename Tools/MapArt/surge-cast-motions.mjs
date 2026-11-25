// node Tools/MapArt/surge-cast-motions.mjs <pixelfx checkout> [preview directory]
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath, pathToFileURL} from 'node:url';
import {drawSurge} from './surge-pose.mjs';
import {drawVerticalQuills} from './surge-vertical-quills.mjs';
const fx=path.resolve(process.argv[2]);
const {createFrame,painterFor,composite,hex}=await import(pathToFileURL(path.join(fx,'src/core/index.js')));
const {encodePNG}=await import(pathToFileURL(path.join(fx,'tools/lib/png.mjs')));
const output=fileURLToPath(new URL('../../Assets/1.Asset/Sprite/Monster/Surge/',import.meta.url));
const size=112;
// Pose keys: centiseconds, knee bend, torso pitch, head pitch, quill lift, casting arm, off arm.
const fan=[
    [0,1,0,0,0,-4,2],[12,3,3,2,-2,-12,-6],[24,5,2,0,0,-28,-12],
    [36,6,2,2,-2,-40,-20],[49,8,4,4,-3,-48,-24],[63,9,5,5,-4,-54,-27],
    [78,10,6,6,-5,-60,-30],[91,10,6,6,-5,-60,-30],
    [100,7,-2,2,4,-67,-17],[111,3,2,0,0,-23,-8]
];
const ground=[
    [0,1,0,0,0,0,0],[14,2,4,3,0,13,-6],[29,2,9,7,0,26,-15],
    [44,1,13,8,0,42,-22],[60,0,15,7,0,53,-25],[78,1,16,5,0,57,-28],
    [94,4,7,1,0,22,-19],[106,9,-8,-4,0,-52,8],
    [115,16,-24,-7,0,-143,42],[131,9,-13,-2,0,-105,22]
];
const width=size*fan.length, height=2*size, atlas=new Uint8Array(width*height*4);
function rotate(x,y,cx,cy,angle){
    const a=angle*Math.PI/180,c=Math.cos(a),s=Math.sin(a);
    return [cx+(x-cx)*c-(y-cy)*s,cy+(x-cx)*s+(y-cy)*c];
}
function draw(pen,key,isFan,index){
    const [,bend,lean,nod,lift,arm,offArm]=key;
    function point(part,x,y){
        if(part==='nearLeg' || part==='farLeg'){
            const t=Math.max(0,Math.min(1,(71-y)/20));
            const knee=Math.max(0,1-Math.abs(y-61)/10);
            x-=knee*bend*(part==='nearLeg'?.55:.85);
            y+=bend*t;
        }else{
            if(part==='quills') [x,y]=rotate(x,y,35,18,isFan && index===9?18:lift);
            if(part==='quills' || part==='head') [x,y]=rotate(x,y,36,32,nod);
            if(part==='sparkArm' || part==='sparkHand'){
                // Fold the casting elbow on the lift so the wrist clears the face.
                const fold=isFan?0:Math.max(0,arm)*2.55;
                if(part==='sparkHand' || (x<=16 && y<=44)) [x,y]=rotate(x,y,20,46,-fold);
                [x,y]=rotate(x,y,23,42,arm+fold*.4);
            }
            if(part==='hipArm') [x,y]=rotate(x,y,52,43,offArm);
            [x,y]=rotate(x,y,36,51,lean);
            y+=bend;
        }
        if(isFan){
            // Pivot around the planted feet. The library model looks left at rest.
            const turn=[1,.78,.72,-.82,-1,-1,-1,-1,-1,.64][index];
            x=36+(x-36)*turn;
        }
        return [Math.round(x+16),Math.round(y+18)];
    }
    drawSurge(pen,hex,point);
    if(isFan && index>=2){
        const patch=(part,coords,color)=>{
            const v=coords.map(([x,y])=>point(part,x,y));
            pen.poly(v,hex(color));
            pen.polyline([...v,v[0]],hex('#242027'));
        };
        // The pivot exposes the back yoke and nape before the opposite profile reads.
        patch('chest',[[26,36],[33,33],[42,32],[49,35],[47,41],[46,50],[29,51],[28,43]],'#435653');
        pen.polyline([[27,37],[34,39],[41,38],[48,36]].map(([x,y])=>point('chest',x,y)),hex('#61736a'));
        const backView=index===2 || index===9;
        patch('head',backView
            ? [[19,21],[23,16],[31,12],[40,11],[46,15],[47,20],[53,27],[45,25],[47,31],[40,28],[39,34],[33,30],[29,34],[27,30],[22,31],[23,27],[17,26]]
            : [[33,18],[36,13],[42,12],[47,16],[49,21],[54,28],[47,25],[49,30],[41,27],[41,32],[35,28],[31,29],[34,24]],
            '#49434c');
        pen.polyline([[34,19],[39,16],[44,17],[48,21]].map(([x,y])=>point('head',x,y)),hex('#68616a'));
    }
    if(isFan && index>=3 && index<=7){
        for(const [x,y] of [[45,3],[61,6],[66,19]]){
            const [a,b]=point('quills',x,y);
            pen.polyline([[a-2,b+1],[a,b],[a+2,b-3],[a+3,b-2],[a+5,b-5]],hex('#eeb959'));
            pen.px(a+2,b-3,hex('#fff1b3'));
        }
    }
    if(!isFan && index>=3 && index<=8){
        const [x,y]=point('sparkHand',14,27);
        const sign=index<7?-1:1;
        pen.polyline([[x,y],[x-3,y+sign*4],[x,y+sign*3],[x-4,y+sign*7]],hex('#ffe29a'));
    }
}
const frames=[fan,ground].map((keys,row)=>keys.map((key,i)=>{
    const frame=createFrame(size,size), pen=painterFor(frame);
    // A rear-facing pivot hides the change of mirror between approach and release.
    if(row===0 && i===2) drawVerticalQuills(pen,hex,2,true,0);
    else draw(pen,key,row===0,i);
    const pixels=new Uint8Array(size*size*4);composite(frame,pixels,null);
    for(let y=0;y<size;y++){
        for(let x=0;x<size;x++) if((x===0 || y===0 || x===size-1 || y===size-1) && pixels[(y*size+x)*4+3])throw Error('Clipped pose '+row+'/'+i);
        atlas.set(pixels.subarray(y*size*4,(y+1)*size*4),((row*size+y)*width+i*size)*4);
    }
    return pixels;
}));
fs.writeFileSync(path.join(output,'SurgeCastMotions.png'),encodePNG(width,height,atlas));
const vertical=new Uint8Array(atlas.length);
for (const [row,down] of [false,true].entries()) {
    fan.forEach((key,i)=>{
        const frame=createFrame(size,size), pen=painterFor(frame);
        if(i<2 || i===9) draw(pen,key,true,i===9 && !down?1:i);
        else drawVerticalQuills(pen,hex,i,down);
        const pixels=new Uint8Array(size*size*4);
        composite(frame,pixels,null);
        for(let y=0;y<size;y++) vertical.set(pixels.subarray(y*size*4,(y+1)*size*4),((row*size+y)*width+i*size)*4);
    });
}
fs.writeFileSync(path.join(output,'SurgeVerticalQuills.png'),encodePNG(width,height,vertical));
const preview=process.argv[3];
if(preview){
    fs.mkdirSync(preview,{recursive:true});
    const idle=createFrame(size,size);draw(painterFor(idle),[0,0,0,0,0,0,0],false,0);
    const rest=new Uint8Array(size*size*4);composite(idle,rest,null);
    const ends=[124,147], tracks=[fan,ground];
    const times=[...new Set([...fan,...ground].map(k=>k[0]).concat(ends))].sort((a,b)=>a-b);
    const w=254;
    times.forEach((time,i)=>{
        const stage=createFrame(w,size),pen=painterFor(stage);
        pen.rect(0,0,w,size,hex('#292c33'));pen.rect(0,96,w,16,hex('#34383c'));
        const pixels=new Uint8Array(w*size*4);composite(stage,pixels,null);
        tracks.forEach((keys,row)=>{
            const cel=time>=ends[row]?rest:frames[row][keys.findLastIndex(k=>k[0]<=time)];
            const x0=row?136:10;
            for(let j=0;j<size*size;j++)if(cel[j*4+3]) pixels.set(cel.subarray(j*4,j*4+4),(Math.floor(j/size)*w+x0+j%size)*4);
        });
        fs.writeFileSync(path.join(preview,'frame'+String(i).padStart(2,'0')+'.png'),encodePNG(w,size,pixels));
    });
    fs.writeFileSync(path.join(preview,'timing.json'),JSON.stringify({fps:100,ticks:times.map((t,i)=>(times[i+1]??195)-t)}));
    fs.writeFileSync(path.join(preview,'keys.json'),JSON.stringify({fan:fan.map(k=>k[0]),ground:ground.map(k=>k[0])}));
}
