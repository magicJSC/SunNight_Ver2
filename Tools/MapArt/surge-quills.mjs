// node Tools/MapArt/surge-quills.mjs <pixelfx checkout>
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
    const rgba=new Uint8Array(w*h*4); composite(frame,rgba,null);
    fs.writeFileSync(path.join(output,name+'.png'),encodePNG(w,h,rgba));
}
const hot=hex('#fff4c9'), amber=hex('#eeb959');
for(const release of [false,true]) {
    render(release?'SurgeQuillRelease':'SurgeQuillCharge',80,80,pen=>{
        const pose=(part,x,y)=>{
            const upper=Math.max(0,Math.min(1,(60-y)/22));
            return [Math.round(x+upper*(release?4:-4)),Math.round(y+upper*(release?0:4))];
        };
        drawSurge(pen,hex,pose);
        for(const [x,y] of [[45,3],[60,6],[66,19]]) {
            const [a,b]=pose('head',x,y);
            pen.line(a-3,b+2,a,b,hot);
            if(!release) {
                pen.polyline([[a,b],[a+3,b-2],[a+4,b],[a+7,b-3]],amber);
                pen.px(a+6,b-3,hot);
            }
        }
        if(!release)pen.polyline([[44,6],[47,3],[49,7],[55,5]],hot);
    });
}
render('SurgeQuill',30,11,pen=>{
    pen.poly([[1,6],[9,4],[12,2],[20,3],[28,5],[17,8],[9,8]],hex('#302c38'));
    pen.poly([[5,6],[12,3],[19,4],[26,5],[16,7],[10,7]],hex('#b68448'));
    pen.poly([[14,4],[21,4],[27,5],[16,6]],hex('#cdc7bf'));
    pen.line(14,4,22,4,hot);pen.line(7,6,12,4,amber);
    pen.polyline([[0,8],[4,7],[3,5],[8,5]],hot);
});
