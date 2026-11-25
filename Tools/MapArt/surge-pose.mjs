// Neutral-model coordinates, shared by the idle and attack cels.
export function drawSurge(pen, hex, map = (_, x, y) => [x,y]) {
    const colors = {
        ink:'#242027', deep:'#302b33', quill:'#49434c', ridge:'#68616a',
        iron:'#85838b', silver:'#bcbcc1', glint:'#e1ddd8',
        shadow:'#69565b', fur:'#947c77', warm:'#b49b8e', muzzle:'#cbb6a5',
        jacketDark:'#2c3e3e', jacket:'#435653', seam:'#61736a',
        belt:'#413b40', pants:'#514952', khaki:'#bc9054', gold:'#f3bf66', hot:'#fff0af',
    };
    const palette = Object.fromEntries(Object.entries(colors).map(([key,value]) => [key,hex(value)]));
    let part = 'farLeg';
    const vertices = str => str.split(' ').map(pair => map(part, ...pair.split(',').map(Number)));
    function fill(k, str, outline=false) {
        const v = vertices(str);
        pen.poly(v,palette[k]);
        if(outline) pen.polyline([...v,v[0]],palette.ink);
    }
    function line(k,str) {pen.polyline(vertices(str),palette[k]);}
    function dot(k,x,y) {pen.px(...map(part,x,y),palette[k]);}

    // Far leg and load-bearing leg. The exposed ankle hinges separate the boots from the cloth.
    fill('belt','28,48 39,48 40,55 36,61 34,69 31,71 25,70 26,66 28,58',true);
    fill('pants','30,52 35,53 32,59 29,64 28,68 27,68 29,59');
    line('deep','33,56 31,58 29,58');
    fill('iron','26,69 32,69 32,72 25,72',true); line('silver','27,70 29,70');
    fill('deep','25,71 32,71 33,75 31,76 20,76 19,75 20,73 24,72',true);
    line('ridge','21,74 25,73 27,73');line('quill','20,75 30,75');
    part = 'nearLeg';
    fill('pants','34,48 45,47 47,51 44,58 45,62 47,69 45,71 40,71 38,63 34,58 32,54',true);
    fill('belt','34,52 38,53 39,58 42,62 44,69 40,69 38,62 34,58');
    line('ridge','43,53 41,57 40,58');line('deep','42,61 44,63');
    fill('iron','40,70 46,69 47,72 41,73',true);dot('silver',42,70);
    fill('deep','40,72 46,71 48,73 51,74 51,76 49,77 40,77 39,76',true);
    line('ridge','42,73 45,72 47,73');line('quill','41,76 49,76');

    part = 'chest';
    // Thin charcoal undershirt, open short-sleeved work shirt, one hand resting at the hip.
    fill('shadow','32,32 40,30 44,32 43,38 38,42 31,38',true);
    fill('belt','32,35 41,34 45,37 44,50 38,52 29,51 27,46',true);
    line('quill','30,48 34,48 39,45');
    fill('jacket','30,33 34,34 33,38 30,44 29,51 25,50 26,43 23,43 21,41 23,36',true);
    fill('jacketDark','28,39 30,38 28,44 28,49 26,50');
    line('khaki','24,36 28,34');line('seam','23,39 24,37');
    fill('jacket','41,32 46,32 51,35 55,41 50,45 47,42 46,48 47,51 38,52 36,50 37,44 37,37',true);
    fill('jacketDark','41,38 39,44 39,50 36,51 37,43 37,38');
    fill('seam','43,34 48,34 50,36 46,35 42,36');
    fill('khaki','44,33 47,33 50,35 48,35 44,34');
    fill('jacketDark','48,38 50,39 54,40 51,43 48,42');
    line('jacketDark','40,42 44,42 44,45 42,46 40,45');
    line('seam','41,41 44,41');line('seam','38,48 38,50');
    part = 'hipArm';
    fill('fur','50,43 54,41 56,44 55,46 49,50 46,51 43,54 41,54 41,52 39,52 40,50 44,47 47,47 51,45',true);
    fill('shadow','54,43 55,44 54,46 49,49 46,50 44,52 43,52 44,49 48,48');
    line('warm','41,50 43,49 45,49');
    // A recessed hinge pin joins the palm to the forearm resting on the hip.
    fill('iron','45,46 47,46 49,48 49,49 47,51 45,51 44,49 44,47',true);
    line('silver','45,47 47,47 48,48');dot('deep',46,49);dot('iron',47,49);
    part = 'chest';
    fill('jacketDark','29,34 32,33 34,34 32,37 29,39 30,36',true);
    fill('jacket','41,32 44,32 44,36 40,35 37,39 38,35',true);

    part = 'sparkArm';
    // Crooked fingers hold one short electrical discharge.
    fill('fur','21,41 25,43 24,46 21,49 18,49 15,47 13,43 15,40 18,44 20,45',true);
    fill('shadow','16,44 19,46 21,46 23,44 24,45 21,48 18,48');
    part = 'sparkHand';
    fill('iron','12,39 15,38 17,41 15,43 12,42',true);dot('silver',14,40);
    fill('fur','12,40 10,38 10,35 12,31 14,30 15,31 13,34 14,35 16,34 18,32 19,33 18,36 16,37 15,39',true);
    line('warm','11,36 12,34 13,33');dot('warm',17,35);
    line('khaki','10,30 11,27 13,28 14,26 16,29 18,31');
    line('gold','10,29 11,27 13,28 14,26 16,29');dot('hot',14,27);

    part = 'quills';
    // Quills radiate from the crown and nape, ending above the shoulder blades.
    fill('deep','39,23 48,19 54,22 58,26 62,33 55,30 51,28 54,34 54,37 48,32 43,29',true);
    fill('quill','47,24 54,24 58,28 60,31 55,28 51,27');
    fill('quill','40,17 47,13 55,13 61,15 66,19 67,21 60,20 54,22 59,26 60,28 52,25 47,24 43,23',true);
    fill('ridge','48,16 53,14 59,15 62,17 55,16 50,18');
    fill('khaki','57,15 60,15 63,17 64,19 61,17');
    fill('iron','61,16 64,17 67,20 63,19');line('silver','63,17 65,18');
    fill('quill','33,15 36,10 41,7 49,5 57,5 62,7 58,8 53,12 48,15 42,18',true);
    fill('ridge','38,12 43,9 48,7 53,6 56,6 49,8 45,11');
    fill('khaki','47,7 51,6 54,6 50,7 46,10');
    fill('iron','53,6 58,5 62,7 59,7 55,9');line('silver','55,6 59,6');dot('glint',57,6);
    fill('quill','27,18 28,12 31,8 36,5 41,3 48,2 44,5 41,9 37,14 32,18',true);
    fill('ridge','29,14 32,9 37,6 40,5 36,9 33,13');
    fill('khaki','33,10 36,7 38,6 36,9');
    fill('iron','40,4 44,3 47,3 44,5 41,7');line('silver','42,4 46,3');dot('glint',44,3);

    part = 'head';
    // Long nose, low brow and round ears retain the hedgehog silhouette in three-quarter view.
    fill('fur','24,18 22,15 22,12 24,11 27,13 28,17',true);
    line('shadow','24,13 25,15');
    fill('fur','22,18 25,15 29,16 32,14 37,12 41,13 45,16 47,20 46,23 49,25 46,26 48,28 43,29 40,32 36,34 31,34 28,32 22,30 20,27 21,23',true);
    fill('shadow','40,16 44,17 46,20 44,23 47,25 44,26 45,27 41,28 37,32 34,32 37,26');
    fill('fur','36,16 36,12 38,9 41,9 43,11 43,15 41,18',true);
    fill('warm','38,15 38,12 40,11 41,11 41,15 40,16');dot('shadow',40,13);
    fill('warm','19,24 24,24 28,25 33,24 37,22 40,24 40,28 37,31 33,33 28,32 24,30 19,28 17,26',true);
    fill('muzzle','19,25 25,25 28,26 33,25 37,24 38,25 34,27 29,28 24,28 20,27');
    fill('deep','17,25 18,24 22,24 23,25 21,27 19,27',true);dot('ridge',19,25);
    fill('muzzle','28,22 32,20 37,19 35,22 32,24 29,24');
    fill('gold','30,22 32,21 35,20 34,22 32,23');dot('deep',33,22);
    line('ink','27,22 31,21 34,20 37,19');
    dot('gold',32,22);dot('hot',31,22);dot('khaki',34,21);
    line('ink','22,20 25,21 26,22');dot('gold',24,22);
    line('shadow','28,19 31,17 34,17');line('shadow','28,30 30,29 32,29');dot('ink',32,29);
    dot('iron',40,25);dot('glint',39,24);
}
