// node Tools/MapArt/surge-dash.mjs <pixelfx checkout> [preview frame directory]
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { drawSurge } from './surge-pose.mjs';

const checkout = path.resolve(process.argv[2] || path.join(process.env.LOCALAPPDATA, 'pixelfx'));
const { createFrame, painterFor, composite, hex } = await import(pathToFileURL(path.join(checkout,'src/core/index.js')));
const { encodePNG } = await import(pathToFileURL(path.join(checkout,'tools/lib/png.mjs')));
const width=176, height=96, foot=[85,85], fps=24;
const sheetDir=fileURLToPath(new URL('../../Assets/1.Asset/Sprite/Monster/Surge/',import.meta.url));
const previewDir=process.argv[3];
if(previewDir) fs.mkdirSync(previewDir,{recursive:true});

// Each boot is keyed independently so the leading foot catches the brake.
// Leg keys are the knee and ankle in model pixels. Angles are in degrees.
const farRest=[31,61,29,70], nearRest=[40,61,43,70];
function pose(ticks,drop,lean,leadArm,trailArm,far,near,travel,elbow=0) {
    return {ticks,drop,lean,leadArm,trailArm,far,near,travel,elbow};
}
const idle=pose(3,0,0,0,0,farRest,nearRest,0);
const keys=[
    idle,
    pose(3,2,-7,4,-10,[29,62,29,70],[39,63,44,70],0),
    pose(3,5,-19,14,-25,[25,62,26,70],[39,65,46,70],0),
    pose(4,8,-32,24,-45,[23,64,25,70],[37,65,48,70],0),
    pose(4,9,-38,32,-54,[23,64,25,70],[38,66,49,70],0),
    pose(2,9,-39,34,-55,[23,64,25,70],[38,66,49,70],0,-12),
    pose(1,7,-55,38,-66,[24,59,28,65],[47,62,55,68],-4,-44),
    pose(1,5,-65,32,-80,[23,54,27,63],[49,58,59,62],-21,-50),
    pose(2,4,-60,22,-75,[22,54,17,64],[46,60,54,66],-43,-50),
    pose(2,5,-48,18,-64,[23,58,16,69],[47,54,52,57],-62,-53),
    pose(3,10,-28,10,-33,[23,64,16,70],[44,64,53,68],-70,-40),
    pose(3,8,-8,-12,-15,[24,62,19,70],[44,64,51,70],-72,-16),
    pose(3,5,6,-13,-5,[24,60,20,70],[44,63,49,70],-72),
    pose(3,3,5,-7,-3,[27,61,24,70],[42,62,46,70],-72),
    pose(3,1,2,-3,-1,[30,61,28,70],[41,61,44,70],-72),
    {...idle,travel:-72}
];
const sheet=new Uint8Array(width*4*height*4*4);
const amber=hex('#d7903a'),gold=hex('#ffd16b'),core=hex('#fff0b3'),oxide=hex('#98663d');
function rotate(x,y,cx,cy,degrees) {
    const angle=degrees*Math.PI/180, c=Math.cos(angle), s=Math.sin(angle);
    return [cx+(x-cx)*c-(y-cy)*s,cy+(x-cx)*s+(y-cy)*c];
}
function mapper(k) {
    return (part,x,y) => {
        if(part==='farLeg'||part==='nearLeg') {
            const far=part==='farLeg', rest=far?farRest:nearRest, leg=far?k.far:k.near;
            const lerp=(a,b,t)=>a+(b-a)*Math.min(1,Math.max(0,t));
            const lower=y>61;
            const t=lower?(y-61)/9:(y-51)/10;
            x+=lower?lerp(leg[0]-rest[0],leg[2]-rest[2],t):lerp(0,leg[0]-rest[0],t);
            y+=lower?lerp(leg[1]-rest[1],leg[3]-rest[3],t):lerp(k.drop,leg[1]-rest[1],t);
        } else {
            if(part==='sparkArm' || part==='sparkHand') {
                // Turn the wrist end of the forearm with the hand, leaving the sleeve attached.
                if(part==='sparkHand' || (x<=16 && y<=44)) [x,y]=rotate(x,y,20,46,k.elbow);
                [x,y]=rotate(x,y,23,42,k.leadArm);
            }
            if(part==='hipArm') [x,y]=rotate(x,y,52,43,k.trailArm);
            if(part==='head' || part==='quills') [x,y]=rotate(x,y,36,32,-k.lean*.50);
            [x,y]=rotate(x,y,36,51,k.lean);
            y+=k.drop;
        }
        return [Math.round(x),Math.round(y)];
    };
}
function render(i,stage=false) {
    const k=keys[i], w=stage?254:width, h=height;
    const frame=createFrame(w,h), pen=painterFor(frame);
    const originX=stage?149+k.travel:foot[0], originY=foot[1];
    const x=originX-36, y=originY-78;
    if(stage) {
        pen.rect(0,0,w,h,hex('#292c33')); pen.rect(0,originY+1,w,10,hex('#34383c'));
        for(let n=2;n<w;n+=24) pen.line(n,originY+2,n+9,originY+2,hex('#494746'));
    }
    const drawLine=(v,color)=>pen.polyline(v.map(([dx,dy])=>[originX+dx,originY+dy]),color);
    // In game the telegraph is aimed in world space. Bake it only into the standalone preview.
    if(stage && i>0 && i<=6) {
        // The spacing closes, then the warned lane turns solid immediately before release.
        const length=[0,26,42,58,72,72,68][i];
        if(i<5) {
            const dash=[0,2,3,4,6][i];
            for(let d=2;d<length;d+=9) drawLine([[-d,0],[-Math.min(length,d+dash),0]],i<=2?amber:gold);
        } else {
            drawLine([[-length,0],[-2,0]],gold);
            drawLine([[-length+2,-1],[-length+12,-1]],core);
        }
    }
    if(i>=6 && i<=10) {
        const tail=[27,52,57,46,26][i-6];
        const h=[-23,-28,-18,-21,-10][i-6];
        const trace=[[2,h+2],[11,h],[17,h+2],[24,h-3],[22,h],[tail,h-2]];
        drawLine(trace,amber);drawLine(trace.slice(1,5).map(([a,b])=>[a,b-1]),gold);
        drawLine([[10,-7],[21,-8],[26,-11],[24,-8],[tail-3,-10]],gold);
        drawLine([[17,-34],[26,-35],[tail-5,-35]],oxide);
    }
    const map=mapper(k);
    if(i>=7 && i<=9) {
        pen.place(x+13,y);
        drawSurge(pen,c=>hex(c==='#242027'?'#865b3e':'#66503d'),map);
    }
    pen.reset().place(x,y);
    drawSurge(pen,hex,map);
    if(i>=2 && i<=8) {
        const prong=(v,c)=>pen.polyline(v.map(([a,b])=>map('head',a,b)),c);
        prong([[42,4],[47,2],[49,2]],i===5?core:gold);
        prong([[55,6],[59,5],[63,7]],gold);
        if(i>=4 && i<=7) prong([[47,2],[51,1],[52,4],[57,2],[59,5]],i===5?core:amber);
        if(i===5 || i===6) {
            prong([[60,5],[64,4],[62,9],[67,7],[65,12]],core);
            prong([[62,15],[67,15],[70,12]],gold);
        }
    }
    pen.reset();
    if(i===4 || i===5) {
        const s=i===5?3:0;
        drawLine([[-8,0],[-13-s,-1],[-10-s,-3],[-16-s,-2]],i===5?core:amber);
        drawLine([[9,0],[13,-2],[16,-1],[19,-4]],gold);
    }
    if(i===7 || i===8) {
        drawLine([[29,-25],[34,-26],[33,-24],[40,-26]],core);
        drawLine([[39,-10],[43,-11],[49,-11]],gold);
    }
    if(i>=10 && i<=12) {
        const f=i-10;
        drawLine([[-18+f*3,0],[-13+f*4,-2-f],[-11+f*4,-2-f]],gold);
        drawLine([[9+f*4,0],[14+f*5,-3-f],[18+f*6,-3-f]],f?oxide:amber);
    }
    const bytes=new Uint8Array(w*h*4); composite(frame,bytes,null);
    return bytes;
}
for(let i=0;i<keys.length;i++) {
    const pixels=render(i), sx=i%4*width, sy=Math.floor(i/4)*height;
    for(let y=0;y<height;y++) {
        sheet.set(pixels.subarray(y*width*4,(y+1)*width*4),((sy+y)*width*4+sx)*4);
        if(pixels[(y*width)*4+3] || pixels[(y*width+width-1)*4+3] ||
           ((y===0 || y===height-1) && pixels.subarray(y*width*4,(y+1)*width*4).some((v,n)=>n%4===3 && v)))
            throw Error('Clipped cel '+i);
    }
    if(previewDir) fs.writeFileSync(path.join(previewDir,`frame${i.toString().padStart(2,'0')}.png`),encodePNG(254,height,render(i,true)));
}
fs.writeFileSync(path.join(sheetDir,'SurgeDash.png'),encodePNG(width*4,height*4,sheet));
if(previewDir) fs.writeFileSync(path.join(previewDir,'timing.json'),JSON.stringify({fps,ticks:keys.map(k=>k.ticks)}));
