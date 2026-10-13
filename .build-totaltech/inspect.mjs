import fs from 'node:fs/promises';
import {FileBlob,PresentationFile} from 'file:///C:/Users/facu_/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@oai/artifact-tool/dist/artifact_tool.mjs';
const p=await PresentationFile.importPptx(await FileBlob.load('C:/Users/facu_/OneDrive/Escritorio/T.S.D.S/2-AÑO/totaltech/presentacion_taller_mecanico.pptx'));
await fs.mkdir('source',{recursive:true});
console.log((await p.inspect({kind:'slide,layout,textbox',maxChars:14000})).ndjson);
for(let i=0;i<p.slides.items.length;i++) {
 const b=await p.export({slide:p.slides.items[i],format:'png',scale:1.2});
 await fs.writeFile(`source/${i+1}.png`,new Uint8Array(await b.arrayBuffer()));
}
console.log('RENDERED',p.slides.items.length);
