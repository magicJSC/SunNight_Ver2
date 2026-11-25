// node Tools/MapArt/surge-ground-strike.mjs <pixelfx checkout>
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath, pathToFileURL} from 'node:url';
import {drawSurge} from './surge-pose.mjs';
const fx = path.resolve(process.argv[2]);
const {createFrame,painterFor,composite,hex} = await import(pathToFileURL(path.join(fx,'src/core/index.js')));
const {encodePNG} = await import(pathToFileURL(path.join(fx,'tools/lib/png.mjs')));
const output = fileURLToPath(new URL('../../Assets/1.Asset/Sprite/Monster/Surge/', import.meta.url));
function render(name,w,h,draw) {
    const frame=createFrame(w,h);
    draw(painterFor(frame));
    const pixels=new Uint8Array(w*h*4); composite(frame,pixels,null);
    fs.writeFileSync(path.join(output,name+'.png'),encodePNG(w,h,pixels));
}
const amber=hex('#f3b34f'), core=hex('#fff1b3'), violet=hex('#696092'), blue=hex('#a4dbeb');
render('SurgeGroundCast',80,80,pen=>{
    drawSurge(pen,hex,(part,x,y)=>{
        if(part==='sparkArm' || part==='sparkHand'){
            const dx=x-23,dy=y-42;
            x=23-.17*dx+.98*dy;y=42-.98*dx-.17*dy;
        }
        const bend=Math.max(0,Math.min(1,(71-y)/28));
        return [Math.round(x-2*bend),Math.round(y+4*bend)];
    });
    pen.polyline([[9,58],[6,62],[10,61],[5,68]],amber);
    pen.polyline([[8,60],[6,62],[9,61]],core);
});
render('SurgeGroundWarning',128,128,pen=>{
    pen.circle(64,64,63,hex('#be772e'),.18);
    pen.ring(64,64,63,2,amber);
    for(let i=0;i<12;i++){
        const a=i*Math.PI/6;
        const at=r=>[Math.round(64+r*Math.cos(a)),Math.round(64+r*Math.sin(a))];
        pen.line(...at(56),...at(62),core);
    }
    pen.poly([[64,55],[70,55],[65,63],[70,63],[59,75],[62,66],[57,66]],amber);
});
render('SurgeGroundBolt',96,168,pen=>{
    const spine=[[62,1],[52,25],[59,23],[39,55],[52,49],[32,84],[44,77],[35,109],[48,101],[43,126],[48,144]];
    function stroke(points){
        for(const offset of [-3,-2,2,3])pen.polyline(points.map(([x,y])=>[x+offset,y]),violet);
        for(const offset of [-1,1])pen.polyline(points.map(([x,y])=>[x+offset,y]),blue);
        pen.polyline(points,core);
    }
    stroke(spine);
    pen.polyline([[41,54],[33,50],[27,52],[17,36],[20,47],[11,43]],blue);
    pen.polyline([[46,104],[60,91],[64,93],[75,72]],blue);
    pen.polyline([[4,149],[20,141],[27,147],[39,140],[48,144],[62,139],[69,143],[91,136]],amber);
    pen.poly([[48,132],[50,140],[58,137],[53,143],[64,145],[52,146],[56,152],[48,148],[43,153],[45,146],[34,148],[42,143],[37,138],[46,141]],core);
});
