// Bake Surge's neutral pose at its native gameplay resolution.
// node Tools/MapArt/surge-idle.mjs <pixelfx checkout>
import fs from 'node:fs';
import { drawSurge } from './surge-pose.mjs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const checkout = process.argv[2] || path.join(process.env.LOCALAPPDATA, 'pixelfx');
const { createFrame, painterFor, composite, hex } = await import(pathToFileURL(path.resolve(checkout, 'src/core/index.js')));
const { encodePNG } = await import(pathToFileURL(path.resolve(checkout, 'tools/lib/png.mjs')));
const output = fileURLToPath(new URL('../../Assets/1.Asset/Sprite/Monster/Surge/', import.meta.url));
const size = 80;
const frame = createFrame(size, size);
const pen = painterFor(frame);
drawSurge(pen, hex);
const pixels = new Uint8Array(size*size*4);
composite(frame,pixels,null);
fs.mkdirSync(output,{recursive:true});
fs.writeFileSync(path.join(output,'SurgeIdle.png'),encodePNG(size,size,pixels));
