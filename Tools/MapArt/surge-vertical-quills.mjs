// Coordinates are relative to the spine. Screen-down is the rear view of the jacket.
export function drawVerticalQuills(pen, hex, index, down, turn=[0,0,-5,-2,0,0,0,0,0,-5][index]) {
    const ink='#242027', dark='#302b33', quill='#49434c', ridge='#68616a';
    const iron='#85838b', silver='#bcbcc1', gold='#bc9054', light='#fff0af';
    const fur='#947c77', shadow='#69565b', muzzle='#cbb6a5';
    const jacket='#435653', seam='#61736a', hem='#2c3e3e';
    const crouch=[0,0,2,3,5,6,7,7,2,1][index];
    const flare=[0,0,-2,0,2,3,4,5,6,0][index];
    let level=0;
    function points(coords) {
        return coords.map(([x,y])=>[Math.round(52+x+turn*level),Math.round(y+crouch*level)]);
    }
    function shape(color, coords) {
        const v=points(coords);
        pen.poly(v,hex(color)); pen.polyline([...v,v[0]],hex(ink));
    }
    function line(color,coords) { pen.polyline(points(coords),hex(color)); }
    function mirror(coords,side){return coords.map(([x,y])=>[side*x,y]);}
    for(const s of [-1,1]){
        // Ankles stay planted as the knees spread and the centre of mass drops.
        const knee=10+crouch*.38;
        shape('#514952',mirror([[1,65+crouch],[10,65+crouch],[knee+2,78],[10,87],[6,88],[5,78],[0,72+crouch]],s));
        line(ridge,mirror([[knee,78],[knee-2,80],[9,83]],s));
        shape(iron,mirror([[6,86],[11,85],[12,89],[6,90]],s));
        shape(dark,mirror([[6,89],[12,88],[16,91],[16,93],[5,93],[4,92]],s));
        line(ridge,mirror([[6,91],[11,90],[13,91]],s));
    }
    level=.75;
    shape(jacket,[[-12,51],[-6,48],[7,48],[14,52],[12,66],[10,70],[-9,70],[-12,67]]);
    line(hem,[[-9,67],[0,68],[10,66]]);
    if(down)line(seam,[[-11,54],[-5,56],[3,56],[11,54]]);
    else {
        shape(dark,[[-5,49],[6,49],[3,57],[3,69],[-2,69],[-2,56]]);
        line(seam,[[5,58],[10,58],[9,61],[6,62]]);
    }
    for(const s of [-1,1]){
        const spread=flare*.65;
        shape(fur,mirror([[12,57],[17,58],[20+spread,61],[19+spread,66],[16+spread,69],[13+spread,67],[15+spread,63],[11,61]],s));
        shape(iron,mirror([[14+spread,64],[18+spread,65],[18+spread,67],[14+spread,66]],s));
        shape(jacket,mirror([[8,50],[13,50],[18,54],[18,58],[13,61],[10,58]],s));
        line(gold,mirror([[12,51],[15,53],[16,54]],s));
    }
    level=1;
    const root=down?38:43;
    const sign=down?1:-1;
    function spike(x,dx,length,w) {
        const y=root+sign*length;
        const tip=x+dx;
        shape(quill,[[x-w,root+sign*3],[x-2,root-sign*2],[x+1,root-sign*3],[x+w,root+sign*2],[x+w,root+sign*7],[tip,y],[x-w,root+sign*6]]);
        line(ridge,[[x-w+2,root+sign*3],[x+dx*.36,root+sign*length*.48],[tip,y-sign*2]]);
        line(gold,[[tip-dx*.22,y-sign*6],[tip-dx*.12,y-sign*4]]);
        line(silver,[[tip-dx*.12,y-sign*3],[tip,y]]);
    }
    function fan() {
        for(const s of [-1,1]){
            spike(s*10,s*(6+flare),13+flare*.4,5);
            spike(s*5,s*(5+flare*.55),19+flare*.5,6);
        }
        spike(0,0,22+flare,6);
    }
    if(!down)fan();
    for(const s of [-1,1]){
        shape(fur,mirror([[8,34],[10,29],[13,28],[15,31],[14,37]],s));
        line(shadow,mirror([[11,33],[12,31],[13,32]],s));
    }
    if(down) {
        shape(quill,[[-14,36],[-10,31],[-4,29],[4,29],[11,32],[14,37],[14,42],[11,48],[7,52],[-7,52],[-12,47],[-15,42]]);
        line(ridge,[[-12,37],[-8,33],[-3,32],[1,33]]);
        fan();
        line(ridge,[[-5,36],[-2,33],[1,32],[5,33]]);
        line('#85838b',[[-2,34],[0,33],[2,33]]);
    } else {
        shape(fur,[[-14,39],[-10,34],[-4,32],[5,32],[12,36],[14,42],[11,48],[6,53],[-4,54],[-11,50],[-15,45]]);
        shape(muzzle,[[-11,46],[-6,44],[0,46],[6,44],[11,46],[8,50],[2,53],[-3,53],[-9,50]]);
        for(const s of [-1,1]){
            line('#f3bf66',mirror([[3,43],[6,42],[9,41]],s));
            line(ink,mirror([[2,42],[6,40],[10,39]],s));
        }
        shape(dark,[[-3,46],[0,45],[3,46],[2,48],[0,49],[-2,48]]);
        line(shadow,[[-1,51],[3,51]]);
    }
    if(index>=4 && index<=7){
        for(const s of [-1,1]){
            const x=s*(10+5+flare), y=root+sign*(13+flare*.4);
            line('#f3bf66',[[x,y],[x+s*2,y+sign*2],[x+s,y+sign*3],[x+s*3,y+sign*(3+index%2)]]);
        }
        line(light,[[0,root+sign*(22+flare)],[0,root+sign*(23+flare)]]);
    }
}
