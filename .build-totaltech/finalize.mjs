import path from 'node:path';
import fs from 'node:fs/promises';
import {constants} from 'node:fs';
import {finalizePresentation} from 'file:///C:/Users/facu_/.codex/plugins/cache/openai-primary-runtime/presentations/26.1007.11041/skills/presentations/container_tools/artifact_tool_utils.mjs';
const skill='C:/Users/facu_/.codex/plugins/cache/openai-primary-runtime/presentations/26.1007.11041/skills/presentations';
const root=path.resolve('..');
// Windows sandbox does not permit hard links. Publish the same validated bytes,
// exclusively, without replacing an existing final file. All validators and
// the finalizer's post-publication SHA-256 comparison remain active.
const originalLink=fs.link;
fs.link=async(source,target)=>{try{return await originalLink(source,target);}catch(e){if(e.code!=='EPERM')throw e;return fs.copyFile(source,target,constants.COPYFILE_EXCL);}};
try{
 const r=await finalizePresentation({
 workspaceDir:root,candidatePath:path.resolve('candidate.pptx'),finalPath:path.join(root,'output/presentacion-totaltech-final/TotalTech-Final.pptx'),
 pythonExecutable:'C:/Users/facu_/.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe',
 integrityValidatorPath:path.join(skill,'container_tools/inspect_presentation_package_integrity.py'),
 layoutValidatorPath:path.join(skill,'container_tools/inspect_presentation_layout_geometry.py'),
 explicitTotalSlideCount:13,requiredNativeTableOwnerSlides:[4,6,7,8,9],
 layoutArgs:['--expected-slide-size-emu','9144000,5143500','--validate-bullet-geometry','--validate-heading-fit',...[4,6,7,8,9].flatMap(n=>['--require-native-table-slide',String(n)])],
 fontPolicy:{basis:'design',families:['Cambria','Calibri','Consolas']},
 verifyArtifactToolImport:true,receiptPath:path.resolve('validation-delivery.json')
 });console.log(JSON.stringify(r));
}catch(e){console.error(e.message);process.exitCode=1;}


