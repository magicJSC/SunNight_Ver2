// node Tools/MapArt/surge-hall.mjs <pixelfx checkout>
import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath, pathToFileURL} from 'node:url';
const checkout = path.resolve(process.argv[2]);
const {createFrame, painterFor, composite, hex} = await import(pathToFileURL(path.join(checkout,'src/core/index.js')));
const {encodePNG} = await import(pathToFileURL(path.join(checkout,'tools/lib/png.mjs')));
const w=576, h=416, frame=createFrame(w,h), pen=painterFor(frame);
const rect=(x,y,dx,dy,c)=>pen.rect(x,y,dx,dy,hex(c));
const line=(x,y,xx,yy,c)=>pen.line(x,y,xx,yy,hex(c));
const strip=(pts,c)=>pen.polyline(pts,hex(c));
let seed=37;
function rand(n){seed=(Math.imul(seed,1664525)+1013904223)>>>0;return Math.floor(seed*n/4294967296);}
const plates=['#444b4b','#454b4b','#474d4c','#434a4a'];
rect(0,0,w,h,'#292e31');
for(let y=0;y<h;y+=16){
    for(let x=(y%32===0?-16:0);x<w;x+=32){
        rect(x+1,y+1,31,15,plates[rand(4)]);
        line(x+1,y+1,x+30,y+1,'#505656');
        for(const dx of [2,28])rect(x+dx,y+13,1,1,'#343a3a');
        if(rand(4)===0){const s=x+5+rand(15);line(s,y+9,s+3,y+8,'#4c5251');}
    }
}
// Recessed cable trenches and worn paint separate the central bus from the service bays.
for(const x of [104,460]){
    rect(x,35,12,367,'#272d30');rect(x+1,35,1,367,'#77796a');
    for(let y=36;y<402;y+=4)rect(x+2,y,9,2,'#525855');
    for(let y=43;y<394;y+=16)rect(x===104?x+15:x-5,y,2,10,'#937746');
}
for(const y of [79,332]){
    for(let x=117;x<459;x+=16)rect(x,y,10,1,'#756b50');
}
const rim=[[256,163],[320,163],[334,177],[334,211],[319,226],[257,226],[241,210],[241,178],[256,163]];
strip(rim,'#8c7952');strip(rim.map(([x,y])=>[x,y+2]),'#605d4c');
pen.poly([[287,174],[302,174],[292,190],[302,190],[277,217],[285,197],[274,197]],hex('#85744f'));
// North wall: inset switch cabinets, louvers, breakers and twin overhead busbars.
rect(0,0,w,37,'#24292c');rect(0,3,w,3,'#585954');
for(let x=8;x<w;x+=32){
    rect(x,10,29,22,'#414844');rect(x+1,11,27,1,'#60645b');
    for(let y=15;y<=24;y+=3)rect(x+4,y,13,1,'#282e30');
    rect(x+21,15,3,7,'#22282a');rect(x+20,16,4,2,'#8b8d7c');
    rect(x+5,28,2,1,'#bc9253');rect(x+9,28,2,1,'#658c86');
}
for(const y of [5,34]){rect(0,y,w,2,'#8c775c');for(let x=5;x<w;x+=27)rect(x,y-1,3,4,'#33383b');}
for(const x of [0,w-8]){rect(x,37,8,h-37,'#22282a');rect(x+2,37,2,h-37,'#63665d');}
for(let x=8;x<w-8;x+=12){
    line(x,404,x+5,399,'#aa8c53');line(x+1,404,x+6,399,'#aa8c53');
}
rect(0,407,w,9,'#262c2d');
const data=new Uint8Array(w*h*4);composite(frame,data,null);
const file=fileURLToPath(new URL('../../Assets/LevelDesign/SouthwestGrid/SwitchHall.png',import.meta.url));
fs.writeFileSync(file,encodePNG(w,h,data));
