const fs = require('node:fs');
const path = require('node:path');
const original = fs.readFileSync('C:/Users/TAEHYEON/.codex/attachments/aed3687f-1bfd-42a7-a7f1-7482b23796b7/붙여넣은 텍스트.txt', 'utf8');
let js = original.slice(original.lastIndexOf('<script>') + 8, original.lastIndexOf('</script>')).replace(/\r\n/g, '\n');
function replaceOnce(from, to) {
  if (!js.includes(from)) throw Error('Missing source: ' + from.slice(0, 100));
  js = js.replace(from, to);
}
function replaceBetween(from, end, to) {
  const i = js.indexOf(from), j = js.indexOf(end, i);
  if (i < 0 || j < 0) throw Error('Missing boundary: ' + from);
  js = js.slice(0, i) + to + '\n\n        ' + js.slice(j);
}
replaceOnce('airDrag: 0.996', 'airDrag: 0.998');
replaceOnce('swingPumpForce: 920', 'swingPumpForce: 1850');
replaceOnce('showVectors: true', 'showVectors: false');
replaceOnce("state: 'GROUNDED', //", "state: 'AIRBORNE', //");
replaceOnce('facing: 1,', 'facing: 1,\n            rotation: 0, visualFacing: 1,');
replaceOnce('tensionForce: 0\n', 'tensionForce: 0,\n            onGround: false, dashControlLock: 0, wallDetachTimer: 0, coyoteTimer: 0, jumpBuffer: 0\n');
replaceOnce("mode: 'WALL', // 'WALL' | 'DRONE'", "mode: 'WALL', // 'WALL' | 'DRONE'\n            steerAxis: 0,");
js = js.replaceAll('grapple.active = true;', 'grapple.active = true;\n                grapple.steerAxis = 0;');
replaceOnce("player.state = 'GRAPPLING';\n                player.swingDashReady", "player.state = 'GRAPPLING';\n                updatePlayerRotation(0);\n                player.swingDashReady");
replaceOnce("player.state = 'DRONE_PULL';\n                player.swingDashReady", "player.state = 'DRONE_PULL';\n                updatePlayerRotation(0);\n                player.swingDashReady");
replaceOnce('y: player.y,\n            shakeTimer', 'y: player.y - Math.min(110, canvas.height * 0.16),\n            shakeTimer');
replaceOnce('function executeSwingDash() {', `// One chain-relative frame drives body rotation, A/D steering and dash.
        // Local up points at the hook; local right is perpendicular to the chain.
        function getGrappleFrame() {
            const rx = player.x - grapple.anchorX, ry = player.y - grapple.anchorY;
            const distance = Math.hypot(rx, ry);
            if (distance < 0.001) {
                return { rotation: player.rotation, rightX: Math.cos(player.rotation), rightY: Math.sin(player.rotation) };
            }
            return { rotation: Math.atan2(-rx, ry), rightX: ry / distance, rightY: -rx / distance };
        }

        function updateSwingControl(axis, ux, uy, dt, groundControl) {
            if (!axis) { grapple.steerAxis = 0; return; }
            const tx = uy, ty = -ux;
            const alongSwing = player.vx * tx + player.vy * ty;
            const changed = axis !== grapple.steerAxis;
            if (changed) {
                grapple.steerAxis = axis;
                // Explicit reverse input overrides dash protection; holding the
                // same key still preserves the launch rather than clamping it.
                if (alongSwing * axis < 0) player.dashControlLock = 0;
            }
            if (groundControl || player.dashControlLock > 0) return;
            const acceleration = params.swingPumpForce * 3.4;
            const reverseAcceleration = params.swingPumpForce * 16;
            const direction = axis;
            if (alongSwing * direction >= 720) return;
            const rate = alongSwing * direction < 0 ? reverseAcceleration : acceleration;
            const nextSpeed = moveTowards(alongSwing, direction * 720, rate * dt);
            const change = nextSpeed - alongSwing;
            // Modify only tangential speed; radial motion and rope length remain physical.
            player.vx += tx * change;
            player.vy += ty * change;
        }

        // Without new A/D input, keep the last chosen local facing. Neither
        // momentum nor mouse aim can silently choose the other side.
        function getDashDirection() {
            const frame = getGrappleFrame();
            const facing = getMoveAxis() || player.facing;
            return { x: frame.rightX * facing, y: frame.rightY * facing };
        }

        function executeSwingDash() {`);
replaceBetween('            // Compute radial and tangential unit vectors relative to anchor', '            // Project existing speed along chosen tangent', `            updatePlayerRotation(0);
            const direction = getDashDirection();
            const tx = direction.x, ty = direction.y;
            player.state = 'GRAPPLING';
            grapple.steerAxis = getMoveAxis();
            player.dashControlLock = 0.12;
            player.wallDetachTimer = 0.12;
            player.coyoteTimer = 0;
            player.jumpBuffer = 0;`);
replaceOnce('player.facing = (player.vx >= 0) ? 1 : -1;', 'player.visualFacing = player.facing;');
replaceOnce('            player.afterimages.length = 0;', `            player.afterimages.length = 0;
            player.dashControlLock = 0;
            player.wallDetachTimer = 0;
            player.coyoteTimer = 0;
            player.jumpBuffer = 0;
            player.dashFlashTimer = 0;
            player.wallPlatform = null;
            player.onGround = false;
            player.rotation = 0;
            player.visualFacing = player.facing;
            player.tensionForce = 0;
            player.angularVel = 0;
            hitstopTimer = 0;
            slowMoTimer = 0;
            physicsAccumulator = 0;
            clearInput();
            camera.x = player.x;
            camera.y = player.y - Math.min(110, canvas.height * 0.16);
            updateSectorButtonsUI();`);
replaceOnce('            // Update Drones cooldown', `            player.dashControlLock = Math.max(0, player.dashControlLock - dt);
            player.wallDetachTimer = Math.max(0, player.wallDetachTimer - dt);
            if (player.onGround || player.state === 'GROUNDED') player.coyoteTimer = 0.1;
            else player.coyoteTimer = Math.max(0, player.coyoteTimer - dt);
            tryJump();
            player.jumpBuffer = Math.max(0, player.jumpBuffer - dt);

            // Update Drones cooldown`);
replaceOnce("            if (player.state === 'WALL_CLIMB' && !grapple.active) {", `            if (player.state === 'WALL_CLIMB' && !grapple.active && getMoveAxis() === -player.wallSide) {
                player.state = 'AIRBORNE';
                player.vx = -player.wallSide * 260;
                player.wallPlatform = null;
                player.wallDetachTimer = 0.18;
            }
            if (player.state === 'WALL_CLIMB' && !grapple.active) {`);
replaceBetween('            // Movement Input', '            // Subtle air drag', `            // Ground motion is immediate; chain input directly controls swing speed.
            const axis = getMoveAxis();
            if (axis) player.facing = axis;
            const groundControl = (player.onGround || player.state === 'GROUNDED') && player.vy >= 0 && player.dashControlLock <= 0 && (!grapple.active || !axis);
            if (groundControl) player.vx = axis * 440;
            if (grapple.active && grapple.mode === 'WALL') {
                const reelAxis = Number(keys.s) - Number(keys.w);
                grapple.length = Math.max(55, Math.min(params.maxChainRange, grapple.length + reelAxis * params.reelSpeed * dt));
                const rx = player.x - grapple.anchorX, ry = player.y - grapple.anchorY;
                const distance = Math.hypot(rx, ry) || 1;
                const ux = rx / distance, uy = ry / distance;
                updateSwingControl(axis, ux, uy, dt, groundControl);
            } else if (!groundControl && axis && player.dashControlLock <= 0) {
                // Do not clamp launch momentum when holding the same direction.
                if (player.vx * axis < 520) {
                    const acceleration = player.vx * axis < 0 ? 1750 : 1050;
                    player.vx = moveTowards(player.vx, axis * 520, acceleration * dt);
                }
            }`);
replaceOnce('player.vx *= Math.pow(params.airDrag, dt * 60);', 'if (!groundControl) player.vx *= Math.pow(params.airDrag, dt * 60);');
replaceOnce(`                        groundedThisFrame = true;
                        player.swingDashReady = true;`, `                        groundedThisFrame = true;
                        if (!grapple.active) { player.rotation = 0; player.visualFacing = player.facing; }
                        if (!player.onGround) player.swingDashReady = true;
                        if (player.dashControlLock <= 0 && (!grapple.active || !axis)) player.vx = axis * 440;`);
replaceOnce("            if (!groundedThisFrame && player.state === 'GROUNDED') {", "            player.onGround = groundedThisFrame;\n            if (!groundedThisFrame && player.state === 'GROUNDED') {");
js = js.replaceAll("plat.type === 'grabbable' && !grapple.active && player.state !== 'GROUNDED'", "plat.type === 'grabbable' && !grapple.active && player.state !== 'GROUNDED' && player.wallDetachTimer <= 0");
replaceOnce('        // Update trailing scarf physics', `        // Visual pose is independent of the circular collider and steering input.
        function updatePlayerRotation(dt) {
            if (grapple.active) {
                const axis = getMoveAxis();
                if (axis) player.facing = axis;
                player.visualFacing = player.facing;
                const target = getGrappleFrame().rotation;
                // Snap to the actual chain line, preserving continuous angle values.
                // This also runs inside the attachment/input event before any frame.
                player.rotation += Math.atan2(Math.sin(target - player.rotation), Math.cos(target - player.rotation));
                return;
            }
            if (player.onGround || player.state === 'GROUNDED' || player.state === 'WALL_CLIMB') {
                player.rotation = 0;
                player.visualFacing = player.facing;
                return;
            }
            let target = 0;
            if (Math.hypot(player.vx, player.vy) > 120) {
                if (Math.abs(player.vx) > 40) player.visualFacing = Math.sign(player.vx);
                target = Math.atan2(player.vy, player.vx) - (player.visualFacing < 0 ? Math.PI : 0);
            }
            // Unwrap across +/-PI so a full orbit never causes a backward spin.
            const delta = Math.atan2(Math.sin(target - player.rotation), Math.cos(target - player.rotation));
            player.rotation += delta * (1 - Math.exp(-32 * dt));
        }

        // Update trailing scarf physics`);
replaceOnce('function updatePhysics(dt) {', `function updatePhysics(dt) {
            simulatePhysics(dt);
            // Position constraints/collisions have finished: the visible pose and
            // next dash now use the exact same, current chain geometry.
            updatePlayerRotation(dt);
        }

        function simulatePhysics(dt) {`);
replaceOnce(`                    y: player.y,
                    color: player.dashFlashTimer`, `                    y: player.y,
                    rotation: player.rotation,
                    facing: player.visualFacing,
                    color: player.dashFlashTimer`);
replaceOnce(`            player.scarfNodes[0].x = player.x - player.facing * 6;
            player.scarfNodes[0].y = player.y - 4;`, `            const cos = Math.cos(player.rotation), sin = Math.sin(player.rotation);
            const neckX = -player.visualFacing * 6;
            player.scarfNodes[0].x = player.x + cos * neckX + sin * 4;
            player.scarfNodes[0].y = player.y + sin * neckX - cos * 4;`);
replaceOnce('                curr.x -= player.facing * 35 * dt;', `                curr.x -= cos * player.visualFacing * 35 * dt;
                curr.y -= sin * player.visualFacing * 35 * dt;`);
replaceOnce(`                ctx.arc(ghost.x, ghost.y, player.radius * 0.92, 0, Math.PI * 2);`, `                ctx.translate(ghost.x, ghost.y);
                ctx.rotate(ghost.rotation);
                ctx.scale(ghost.facing, 1);
                ctx.ellipse(0, 0, player.radius * 0.72, player.radius * 0.92, 0, 0, Math.PI * 2);`);
replaceOnce(`            // 12. Player Character (Brigadier General Cyber-Unit)
            ctx.save();
            ctx.translate(player.x, player.y);`, `            // 12. Player Character (Brigadier General Cyber-Unit)
            ctx.save();
            ctx.translate(player.x, player.y);
            ctx.rotate(player.rotation);
            ctx.scale(player.visualFacing, 1);`);
replaceOnce('ctx.arc(0, 0, player.radius, 0, Math.PI * 2);', 'ctx.ellipse(0, 0, player.radius * 0.78, player.radius, 0, 0, Math.PI * 2);');
replaceOnce('ctx.fillRect(player.facing > 0 ? 2 : -11, -5, 9, 4);', 'ctx.fillRect(2, -5, 9, 4);');
replaceOnce('ctx.arc(-player.facing * 4, 4, 5, 0, Math.PI * 2);', 'ctx.arc(-4, 4, 5, 0, Math.PI * 2);');
replaceOnce('            // 13. Physics Vector Visualization', `            drawDashPreview();

            // 13. Physics Vector Visualization`);
replaceOnce('        // HUD & TELEMETRY UPDATER', '        // HUD & TELEMETRY UPDATER');
replaceOnce('        function formatTime(sec) {', `        function drawDashPreview() {
            if (!grapple.active || grapple.mode !== 'WALL' || !player.swingDashReady) return;
            const direction = getDashDirection();
            const angle = Math.atan2(direction.y, direction.x);
            ctx.save();
            ctx.translate(player.x, player.y);
            ctx.rotate(angle);
            ctx.strokeStyle = '#ff5baa';
            ctx.fillStyle = '#ff5baa';
            ctx.shadowColor = '#ff007f';
            ctx.shadowBlur = 10;
            ctx.lineWidth = 3;
            ctx.setLineDash([6, 5]);
            ctx.beginPath(); ctx.moveTo(27, 0); ctx.lineTo(98, 0); ctx.stroke();
            ctx.setLineDash([]);
            ctx.beginPath(); ctx.moveTo(110, 0); ctx.lineTo(94, -7); ctx.lineTo(94, 7); ctx.closePath(); ctx.fill();
            ctx.restore();
        }

        function formatTime(sec) {`);
replaceOnce('            domTimer.textContent = formatTime(trialTime);', `            domTimer.textContent = formatTime(trialTime);
            const axis = getMoveAxis();
            document.getElementById('inputA').classList.toggle('active', axis === -1);
            document.getElementById('inputD').classList.toggle('active', axis === 1);
            document.getElementById('dashHint').textContent = grapple.active && grapple.mode === 'WALL'
                ? (player.facing < 0 ? '사슬 기준 왼쪽을 보는 중 · 그 방향으로 대시' : '사슬 기준 오른쪽을 보는 중 · 그 방향으로 대시')
                : '청록색 벽·천장에 좌클릭 유지 → Shift 대시';`);
replaceOnce("domDashStatus.textContent = 'READY';", "domDashStatus.textContent = grapple.active && grapple.mode === 'WALL' ? '대시 준비' : '사슬 연결 대기';");
replaceOnce("domDashStatus.textContent = 'EXPENDED';", "domDashStatus.textContent = '재연결 · 착지로 충전';");
replaceBetween('        function updateSectorButtonsUI() {', '        // ============================================================================\n        // INPUT LISTENERS', `        function updateSectorButtonsUI() {
            document.querySelectorAll('.sector-btn').forEach(btn => {
                btn.classList.toggle('selected', Number(btn.dataset.sector) === player.checkpointIndex);
            });
        }`);
replaceBetween("        window.addEventListener('mousemove'", '        // Inspector & UI Buttons', `        const heldKeys = new Set();
        let lastMoveAxis = 1;
        let paused = false;
        let physicsAccumulator = 0;
        const FIXED_DT = 1 / 240;
        function moveTowards(value, target, step) {
            return value < target ? Math.min(target, value + step) : Math.max(target, value - step);
        }
        function getMoveAxis() {
            return keys.a && keys.d ? lastMoveAxis : Number(keys.d) - Number(keys.a);
        }
        function syncKeys() {
            keys.a = heldKeys.has('KeyA') || heldKeys.has('ArrowLeft');
            keys.d = heldKeys.has('KeyD') || heldKeys.has('ArrowRight');
            keys.w = heldKeys.has('KeyW') || heldKeys.has('ArrowUp');
            keys.s = heldKeys.has('KeyS') || heldKeys.has('ArrowDown');
            keys.space = heldKeys.has('Space');
            keys.shift = heldKeys.has('ShiftLeft') || heldKeys.has('ShiftRight');
            const axis = getMoveAxis();
            if (axis) player.facing = axis;
            if (grapple.active) updatePlayerRotation(0);
            if ((player.onGround || player.state === 'GROUNDED') && player.vy >= 0 && player.dashControlLock <= 0 && (!grapple.active || !axis)) {
                player.vx = getMoveAxis() * 440;
            }
        }
        function clearInput() {
            heldKeys.clear();
            syncKeys();
            mouse.leftDown = mouse.rightDown = false;
        }
        function setPaused(value) {
            paused = value;
            clearInput();
            player.jumpBuffer = 0;
            physicsAccumulator = 0;
            lastFrameTime = performance.now();
            document.getElementById('pauseOverlay').classList.toggle('hidden', !value);
        }
        function tryJump() {
            if (player.jumpBuffer <= 0) return;
            if (grapple.active) {
                releaseChainArm();
                player.vy -= 260;
            } else if (player.state === 'WALL_CLIMB') {
                player.vx = -player.wallSide * 540;
                player.vy = -610;
                player.facing = -player.wallSide;
                player.wallDetachTimer = 0.2;
                player.wallPlatform = null;
                player.swingDashReady = true;
            } else if (player.state === 'GROUNDED' || player.coyoteTimer > 0) {
                player.vy = -590;
            } else return;
            player.state = 'AIRBORNE';
            player.onGround = false;
            player.coyoteTimer = 0;
            player.jumpBuffer = 0;
            spawnParticles(player.x, player.y + player.radius, 10, '#00f3ff', 50, 180);
        }
        window.addEventListener('mousemove', e => {
            mouse.screenX = e.clientX;
            mouse.screenY = e.clientY;
        });
        canvas.addEventListener('mousedown', e => {
            if (paused) return;
            e.preventDefault();
            if (document.activeElement instanceof HTMLElement) document.activeElement.blur();
            mouse.screenX = e.clientX;
            mouse.screenY = e.clientY;
            if (e.button === 0) {
                mouse.leftDown = true;
                fireChainArm();
            } else if (e.button === 2) {
                mouse.rightDown = true;
                if (grapple.active) executeSwingDash();
                else {
                    updateAimRaycast();
                    if (aimResult.hasHit && aimResult.isDrone) fireChainArm();
                }
            }
        });
        window.addEventListener('mouseup', e => {
            if (e.button === 0) { mouse.leftDown = false; if (!paused) releaseChainArm(); }
            if (e.button === 2) mouse.rightDown = false;
        });
        window.addEventListener('keydown', e => {
            const code = e.code;
            if (code === 'Escape') { if (!e.repeat) setPaused(!paused); return; }
            if (paused) return;
            if (e.target instanceof HTMLElement && e.target.matches('input, button, select, textarea')) return;
            if (['Space','ArrowLeft','ArrowRight','ArrowUp','ArrowDown'].includes(code)) e.preventDefault();
            if (e.repeat) return;
            heldKeys.add(code);
            if (code === 'KeyA' || code === 'ArrowLeft') lastMoveAxis = -1;
            if (code === 'KeyD' || code === 'ArrowRight') lastMoveAxis = 1;
            const shiftWasDown = keys.shift;
            syncKeys();
            if (['KeyA','KeyD','ArrowLeft','ArrowRight','Space'].includes(code) && !trialFinished) trialRunning = true;
            if ((code === 'ShiftLeft' || code === 'ShiftRight') && !shiftWasDown) executeSwingDash();
            if (code === 'Space') { player.jumpBuffer = 0.12; tryJump(); }
            if (code === 'KeyR') respawnAtCheckpoint();
            if (code === 'KeyH') document.getElementById('controlsGuide').classList.toggle('hidden');
            if (/^Digit[1-4]$/.test(code)) teleportToSector(Number(code.slice(-1)) - 1);
        });
        window.addEventListener('keyup', e => { heldKeys.delete(e.code); syncKeys(); });
        window.addEventListener('blur', () => setPaused(true));
        document.addEventListener('visibilitychange', () => { if (document.hidden) setPaused(true); });
        document.getElementById('btnResume').addEventListener('click', () => { setPaused(false); document.getElementById('btnResume').blur(); });
        document.getElementById('btnHelp').addEventListener('click', () => document.getElementById('controlsGuide').classList.toggle('hidden'));
        document.getElementById('btnFullscreen').addEventListener('click', async () => {
            try {
                if (document.fullscreenElement) await document.exitFullscreen();
                else await document.documentElement.requestFullscreen();
            } catch { showBannerToast('브라우저의 F11 키로 전체화면을 사용할 수 있습니다.'); }
        });`);
// UI click focus must not swallow the next Space/Shift game input.
replaceOnce('        // Physics Sliders Binding', `        document.querySelectorAll('button').forEach(button => {
            button.addEventListener('click', () => button.blur());
        });

        // Physics Sliders Binding`);
replaceOnce('            // Camera shake countdown', `            if (paused) { requestAnimationFrame(gameLoop); return; }

            // Camera shake countdown`);
replaceBetween('            // Run physics in 4 high-precision', '            updateScarfAndParticles(dt);', `            // Fixed 240 Hz physics: max launch travel is under 8 px per step.
            physicsAccumulator += dt;
            let physicsSteps = 0;
            while (physicsAccumulator >= FIXED_DT && physicsSteps < 24) {
                physicsAccumulator -= FIXED_DT;
                updatePhysics(FIXED_DT);
                physicsSteps++;
                if (hitstopTimer > 0) { physicsAccumulator = 0; break; }
            }`);
replaceOnce('const targetCamY = player.y + lookaheadY;', 'const targetCamY = player.y - Math.min(110, canvas.height * 0.16) + lookaheadY;');
replaceOnce("showBannerToast('제1구역 : 좌클릭으로 청록색 천장에 사슬팔을 발사하세요');", "showBannerToast('D로 이동 → 청록색 천장에 좌클릭 유지 → D + Shift로 스윙 대시');\n            updateSectorButtonsUI();\n            updateAimRaycast();\n            updateHUD();\n            render();");
// The intro comment describes the modified mechanic accurately.
js = js.replace('Instant high-impulse velocity injection along desired tangent or cursor vector.', 'Keep chain connected and inject speed along its A/D-selected tangent.');

const html = `<!DOCTYPE html>
<html lang="ko"><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>산나비 · 사슬팔 액션 — A/D FLOW</title>
<style>
:root{color-scheme:dark;--cyan:#67f1ed;--pink:#ff68b0;--muted:#899bb2;--line:#203647;--panel:rgba(7,15,25,.88)}
*{box-sizing:border-box;user-select:none}body{margin:0;overflow:hidden;background:#060a12;color:#e7f2f7;font-family:"Malgun Gothic","Apple SD Gothic Neo",sans-serif;font-size:12px}canvas{display:block;width:100vw;height:100vh;cursor:none}button,input{font:inherit}button{cursor:pointer;color:#c1d3df;background:#101e2c;border:1px solid #294457;border-radius:7px;padding:9px 12px;transition:background .15s,border-color .15s}button:hover{background:#193248;border-color:var(--cyan);color:white}button:focus-visible,input:focus-visible{outline:2px solid var(--cyan);outline-offset:3px}kbd{font:700 11px Consolas,monospace;background:#172c3d;border:1px solid #35556b;border-bottom-width:2px;padding:3px 6px;border-radius:4px;color:#d4edf8;white-space:nowrap}.panel{background:var(--panel);border:1px solid var(--line);border-radius:12px;backdrop-filter:blur(10px);box-shadow:0 8px 30px #0004}.hud{position:fixed;left:20px;top:20px;width:320px;padding:17px 20px;pointer-events:none;border-top:2px solid var(--cyan)}.eyebrow{font:10px Consolas,monospace;letter-spacing:2px;color:var(--cyan)}h1{font-size:20px;letter-spacing:-.8px;margin:6px 0 13px}h1 small{font:10px Consolas,monospace;color:var(--pink);letter-spacing:1px;margin-left:6px}.metrics{display:grid;grid-template-columns:1fr 1fr;gap:16px}.label{color:var(--muted);font-size:10px;margin-bottom:5px}.value{font:700 19px Consolas,monospace}#hudTimer{color:#ffe39b}.bar{height:4px;border-radius:4px;background:#203041;overflow:hidden;margin-top:7px}.bar>div{height:100%;background:var(--cyan);width:0}#dashChargeBar{background:linear-gradient(90deg,var(--cyan),var(--pink));width:100%}.dash-row{display:flex;justify-content:space-between;margin-top:14px;font-size:10px}#dashStatusText{color:var(--pink)}.tools{position:fixed;right:20px;top:20px;display:flex;gap:6px}.tools button{background:var(--panel)}.guide{position:fixed;left:20px;bottom:76px;width:320px;padding:17px 20px;pointer-events:none}.guide h2{font-size:12px;margin:0 0 12px;display:flex;justify-content:space-between}.guide h2 span{color:var(--muted);font-size:10px;font-weight:400}.guide-row{display:flex;align-items:center;gap:9px;margin:9px 0;color:#b6cada;font-size:11px}.guide-row kbd{min-width:67px;text-align:center}.guide-note{border-top:1px solid var(--line);padding-top:10px;margin-top:12px;line-height:1.7;color:var(--muted);font-size:10px}.guide-note b{color:var(--pink);font-weight:500}.bottom{position:fixed;bottom:18px;left:20px;right:20px;display:flex;justify-content:space-between;align-items:center;gap:12px}.sectors{padding:7px;display:flex;gap:4px}.sector-btn{border-color:transparent;background:transparent;padding:8px;font-size:10px}.sector-btn.selected{color:var(--cyan);background:#12303a;border-color:#256373}.input-status{display:flex;align-items:center;gap:10px;padding:10px 13px;pointer-events:none}.input-status kbd{font-size:14px;min-width:27px;text-align:center}.input-status kbd.active{color:#06131a;background:var(--cyan);box-shadow:0 0 14px #67f1ed55}.input-status span{font-size:11px;color:#b9cad9}#inspectorPanel{position:fixed;right:20px;top:68px;width:295px;padding:18px;max-height:calc(100vh - 150px);overflow:auto}.inspector-title{font-size:13px;font-weight:700;margin-bottom:12px}#stateBadge{color:var(--cyan);font:10px Consolas,monospace}.telemetry{display:grid;grid-template-columns:1fr 1fr;gap:12px;padding:12px 0;border-block:1px solid var(--line);margin:12px 0;font:12px Consolas,monospace}.telemetry small{display:block;color:var(--muted);font-size:9px;margin-bottom:6px}.field{margin-top:15px}.field label{display:flex;justify-content:space-between;font-size:11px}.field label span{color:var(--cyan)}input[type=range]{width:100%;accent-color:var(--cyan);margin:9px 0 0;height:6px}input[type=checkbox]{accent-color:var(--cyan)}#bannerToast{position:fixed;top:174px;left:50%;transform:translateX(-50%);padding:10px 17px;max-width:80vw;color:#dffcff;font-size:12px;pointer-events:none;transition:opacity .25s;text-align:center;border-color:#36706f}.hidden{display:none!important}.opacity-0{opacity:0}.pause{position:fixed;inset:0;display:grid;place-items:center;background:#030914b8;backdrop-filter:blur(8px)}.pause-card{text-align:center;padding:32px 45px;max-width:90vw}.pause-card h2{font-size:24px;margin:12px 0}.pause-card p{color:#9eb5c6;line-height:1.8}.pause-card button{color:#03131b;background:var(--cyan);border:0;padding:12px 30px;font-weight:700;margin-top:14px}.scanline{position:fixed;inset:0;pointer-events:none;background:repeating-linear-gradient(0deg,#0001 0px,#0001 1px,transparent 1px,transparent 4px);opacity:.25}.badge{position:fixed;left:50%;top:24px;transform:translateX(-50%);font:10px Consolas,monospace;letter-spacing:2px;color:#52707f;pointer-events:none}
@media(max-width:1050px){.badge{display:none}.tools{gap:4px}.tools button{padding:9px;font-size:10px}.input-status span{max-width:210px}.guide{width:290px}.hud{width:290px}}
@media(max-height:700px){.guide{padding:10px 16px;bottom:70px}.guide-row{margin:6px 0}.guide-note{display:none}.hud{padding:12px 16px}#bannerToast{top:155px}}
@media(max-width:750px){.hud{width:250px;left:12px;top:12px}.tools{top:12px;right:12px;max-width:210px;justify-content:flex-end;flex-wrap:wrap}.tools button{font-size:10px;padding:7px}.guide{left:12px;width:260px}.bottom{left:12px;right:12px;bottom:12px}.input-status span{display:none}.sector-btn{padding:7px 4px}.sectors{gap:0}.badge{display:none}#inspectorPanel{right:12px;top:92px}h1{font-size:17px}}
</style></head><body>
<canvas id="gameCanvas" aria-label="사슬팔 액션 게임. 키보드와 마우스를 사용하세요."></canvas><div class="scanline"></div>
<header class="hud panel"><div class="eyebrow">CHAIN-ARM ACTION LAB / 04</div><h1>산나비 · 사슬팔 액션 <small>A/D FLOW</small></h1><div class="metrics"><div><div class="label">구간 기록</div><div id="hudTimer" class="value">00:00.00</div></div><div><div class="label">순간 속력</div><div id="speedText" class="value">0 px/s</div><div class="bar"><div id="speedBar"></div></div></div></div><div class="dash-row"><span>사슬팔 대시</span><span id="dashStatusText">사슬 연결 대기</span></div><div class="bar"><div id="dashChargeBar"></div></div></header>
<div class="badge">MAGO CITY / MOVEMENT STUDY</div>
<nav class="tools" aria-label="게임 메뉴"><button id="btnResetPlayer">복귀 <b>R</b></button><button id="btnResetCourse">처음부터</button><button id="btnHelp">조작 <b>H</b></button><button id="btnToggleInspector">물리 설정</button><button id="btnFullscreen">전체화면</button></nav>
<section id="controlsGuide" class="guide panel"><h2>사슬을 유지하며 가속하세요 <span>H 접기</span></h2><div class="guide-row"><kbd>A / D</kbd><span>사슬 회전 기준 좌우 보기 / 빠른 반전</span></div><div class="guide-row"><kbd>좌클릭 유지</kbd><span>벽·천장에 사슬 고정 / 놓으면 해제</span></div><div class="guide-row"><kbd>Shift / 우클릭</kbd><span>사슬 유지 + 스윙 대시</span></div><div class="guide-row"><kbd>A·D + 대시</kbd><span>몸체가 바라보는 방향으로 대시</span></div><div class="guide-row"><kbd>W / S</kbd><span>사슬 감기·풀기 / 벽 등반</span></div><div class="guide-row"><kbd>Space</kbd><span>점프 · 벽 차기 · 사슬 해제 도약</span></div><div class="guide-note"><b>보는 방향 = 분홍 화살표 = 대시 방향</b><br>대시 중 사슬 유지. 좌클릭을 놓으면 해제됩니다.<br>훅 즉시 사슬 선에 회전 고정 · A/D는 몸체 기준<br>지상에서는 A/D를 놓으면 미끄러짐 없이 정지.<br>새 사슬 연결·착지 시 충전 · Esc 일시정지<br>키보드·마우스용 팬 제작 조작 프로토타입</div></section>
<div id="bannerToast" class="panel opacity-0"><span id="bannerToastText"></span></div>
<aside id="inspectorPanel" class="panel hidden"><div class="inspector-title">물리 설정 <span id="stateBadge">AIRBORNE</span></div><div class="telemetry"><div><small>VELOCITY</small><span id="telVel">0, 0</span></div><div><small>CHAIN d / L</small><span id="telChainLen">-- / --</span></div><div><small>ANGULAR VELOCITY</small><span id="telOmega">0</span></div><div><small>TENSION</small><span id="telTension">0</span></div></div><label><input id="chkVectors" type="checkbox"> 속도·장력 벡터 표시</label>
<div class="field"><label for="sliderMaxRange">사슬 사거리 <span id="valMaxRange">480 px</span></label><input id="sliderMaxRange" type="range" min="280" max="750" step="10" value="480"></div>
<div class="field"><label for="sliderDashPower">대시 추진력 <span id="valDashPower">1080</span></label><input id="sliderDashPower" type="range" min="600" max="1600" step="20" value="1080"></div>
<div class="field"><label for="sliderGravity">중력 <span id="valGravity">1480</span></label><input id="sliderGravity" type="range" min="700" max="2400" step="20" value="1480"></div>
<div class="field"><label for="sliderBounce">사슬 탄성 <span id="valBounce">0.18</span></label><input id="sliderBounce" type="range" min="0" max="0.5" step="0.02" value="0.18"></div>
<div class="field"><label for="sliderTimeScale">시간 배율 <span id="valTimeScale">1.00x</span></label><input id="sliderTimeScale" type="range" min="0.25" max="1.25" step="0.05" value="1"></div>
<button id="btnResetParams" style="margin-top:18px;width:100%">기본값 복원</button></aside>
<footer class="bottom"><nav class="sectors panel" aria-label="연습 구간"><button class="sector-btn selected" data-sector="0">1 하층 훈련장</button><button class="sector-btn" data-sector="1">2 진자 협곡</button><button class="sector-btn" data-sector="2">3 수직 샤프트</button><button class="sector-btn" data-sector="3">4 코어 타워</button></nav><div class="input-status panel"><kbd id="inputA">A</kbd><kbd id="inputD">D</kbd><span id="dashHint">청록색 천장에 좌클릭 유지</span></div></footer>
<div id="pauseOverlay" class="pause hidden"><div class="pause-card panel"><div class="eyebrow">TAKE A BREATH</div><h2>잠시 멈췄습니다</h2><p>돌아오면 같은 위치에서 이어집니다.<br>사슬을 유지하려면 좌클릭을 다시 눌러주세요.</p><button id="btnResume">계속 플레이 · Esc</button></div></div>
<script>${js}</script></body></html>`;
fs.writeFileSync(path.join(__dirname, 'sanabi-chain-arm.html'), html);
console.log('Created standalone HTML:', Buffer.byteLength(html), 'bytes');
