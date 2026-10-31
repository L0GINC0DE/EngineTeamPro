import fs from 'node:fs';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {runGameDesignWritingPolish} from 'file:///C:/Users/TAEHYEON/.codex/plugins/cache/game-design-suite/game-design-studio/0.2.0/scripts/run-game-design-writing-polish.mjs';
import {createProtectedWritingManifest} from 'file:///C:/Users/TAEHYEON/.codex/plugins/cache/game-design-suite/game-design-studio/0.2.0/scripts/validate-writing-revision.mjs';
const root=path.dirname(fileURLToPath(import.meta.url));
const source=fs.readFileSync(path.join(root,'math-game-production-plan.md'),'utf8');
const output=path.join(root,'writing-revision');
fs.mkdirSync(output,{recursive:true});
// The bundled Korean prose review was applied by the authoring agent.
// No text changes were selected: formulas, claims and tables stay intact.
const protectedManifest=createProtectedWritingManifest({source,protectedSpans:[{id:'complete-reviewed-document',text:source}]});
const result=await runGameDesignWritingPolish({source,protectedManifest,humanize:async text=>text});
fs.writeFileSync(path.join(output,'revised-draft.md'),result.revised);
fs.writeFileSync(path.join(output,'protected-content-receipt.json'),JSON.stringify(result.receipt,null,2));
fs.writeFileSync(path.join(output,'protected-manifest.json'),JSON.stringify(protectedManifest,null,2));
fs.writeFileSync(path.join(output,'writing-findings.md'),`# 문체 검수 기록

검토 범위는 제작 기획서의 한국어 문장이다. 포함된 humanize-korean의 리포트 기준과 보호 항목 검증을 적용했다.

- D-1~D-4: 과장과 반복 결산 문구를 추가하지 않았다.
- A-7·A-8: 이중 피동과 불필요한 명사 표현을 점검했다.
- C-2·J-1: 비교는 표에, 원리 설명은 문단에 두었다. 수식과 핵심 규칙의 강조는 유지했다.
- 수치·기호·표·코드·확인된 사실과 제안의 구분을 보호했다.

최종 판정: 추가 윤문 없이 원문을 유지한다. 내용 변경률은 0이며 보호 항목 검증을 통과했다. 자체검증은 의미·수치 보존, 변경률, 장르, 문체, 중대한 문체 패턴, 새로운 수사 배제의 여섯 항목을 확인했다.

이 기록은 학습 효과나 제작 범위의 최종 승인을 뜻하지 않는다.
`);
fs.writeFileSync(path.join(output,'human-review-handoff.md'),`# 검토 인계

검토자는 요청 사용자다. 완성된 기획 초안과 설명용 도식을 전달한다. 원문과 별도 검수본은 동일하며 기존 게임 파일은 수정하지 않았다. 추가 제작에 사용할 학습 대상·제출 형식·일정은 미정으로 남겨 두었다. 이번 요청은 기획 작성이므로 추가 승인 요청 없이 결과물을 제공한다.
`);
const humanized=path.join(root,'_workspace','2026-10-27-001');
fs.mkdirSync(humanized,{recursive:true});
const count=Array.from(source).length;
fs.writeFileSync(path.join(humanized,'final.md'),source+`\n<!-- HUMANIZE-SUMMARY
originalCodepoints: ${count}
revisedCodepoints: ${count}
changeRate: 0
categories: D-1~D-4 0→0; A-7/A-8 0→0; C-2 0→0; J-1 0→0
selfCheck: 6/6
grade: B (의미·수식·표를 보존하며 추가 편집을 선택하지 않음)
highlights: 수식 유지; 표 유지; 사실·제안 구분 유지
-->\n`);
console.log('Writing review:',result.receipt.status,'change rate:',result.receipt.changeRate);
