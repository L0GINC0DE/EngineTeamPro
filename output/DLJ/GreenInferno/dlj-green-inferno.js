// DLJ: an approaching round green fireball followed by a much larger radial detonation.
// Flame tongues and puffs use pixelfx's Sprite Fusion fire/explosion helpers,
// MIT License, Copyright (c) 2026 Patch and Hugo Duprez.
import { fit, Rng, lerp, clamp, smooth, ease, noise, noise2, TAU, WHITE } from "../../core/index.js";
import { flameBlade, firePuff, smokePuff, spark, hotFirst, noiseFor, BIG_LAYERS } from "../fire/_lib.js";

export const meta = {
  name: "DLJ Green inferno",
  desc: "A huge round green fireball approaches from the right and grows, compresses for a beat, then detonates into giant green flames, a shockwave and embers.",
  tags: ["green", "fireball", "approach", "explosion", "huge", "left", "one-shot"],
  size: [384, 320],
  origin: [176, 160],
  duration: 3.2,
  loop: false,
  fps: 30,
  bg: "dark",
  options: {
    palette: ["#03270e", "#075020", "#0d7d2b", "#21a13b", "#38d548", "#68ee50", "#adff7a", "#e6ffc7", "#ffffff"],
    seed: 17,
  },
};

const HIT = 0.9, NL = noiseFor(0), records = new Map();
function parameters(seed) {
  if (records.has(seed)) return records.get(seed);
  const R = new Rng(seed);
  const lobes = Array.from({ length: 22 }, (_, i) => ({
    a: i / 22 * TAU + R.range(-0.12, 0.12),
    r: R.range(48, 82), size: R.range(20, 34), spin: R.range(-1, 1),
    delay: R.range(0, 0.14), life: R.range(0.9, 1.45),
  }));
  const debris = Array.from({ length: 62 }, () => ({
    a: R.range(0, TAU), speed: R.range(180, 430),
    life: R.range(0.4, 1.45), delay: R.range(0, 0.12),
    size: R.range(1, 3.6), turn: R.range(-0.7, 0.7),
  }));
  const smoke = Array.from({ length: 14 }, (_, i) => ({
    a: i / 14 * TAU, r: R.range(42, 75), size: R.range(14, 22),
    rise: R.range(8, 26), delay: R.range(0.25, 0.6), life: R.range(1.2, 1.65),
  }));
  const out = { lobes, debris, smoke };
  records.set(seed, out);
  return out;
}

function orb(p, x, y, r, t, C, H, seed, fade = 1) {
  const saved = p.fade;
  p.fade *= fade;
  const N = (rate, s = 0, off = 0) => NL(t, rate, seed + s, off);
  for (let i = 0; i < 16; i++) {
    const a = i / 16 * TAU, ca = Math.cos(a), sa = Math.sin(a);
    const dx = ca + 0.35, dy = sa, length = Math.hypot(dx, dy);
    const flameLength = 5 + r * 0.16 + 4 * (0.5 + 0.5 * N(12, i));
    flameBlade(p, x + ca * r * 0.85, y + sa * r * 0.85,
      dx / length, dy / length, flameLength, 2 + r * 0.065,
      Math.sin(t * 8 + i) * 2, N, i * 17, H, BIG_LAYERS, 0.7, 1.4);
  }
  p.shade(x - r - 3, y - r - 3, x + r + 3, y + r + 3, (px, py) => {
    const dx = px - x, dy = py - y, d = Math.hypot(dx, dy), a = Math.atan2(dy, dx);
    const rim = r + Math.sin(a * 7 + t * 17) * 1.1 + noise(a * 5 - t * 8, seed) * 1.5;
    if (d > rim) return;
    const v = d / rim, swirl = Math.sin(a * 4 - t * 10 + d * 0.18);
    const heat = 1 - v + swirl * 0.12 + noise(d * 0.14 + a * 2 + t * 4, seed + 3) * 0.1;
    if (v > 0.94) return C[2];
    if (Math.abs(swirl) < 0.12 && v > 0.24 && v < 0.88) return C[6];
    return heat > 0.8 ? C[8] : heat > 0.61 ? C[7] : heat > 0.43 ? C[6]
      : heat > 0.26 ? C[5] : heat > 0.12 ? C[4] : C[3];
  });
  p.glow(x, y, r * 1.45, C[4], 0.23);
  p.glow(x - r * 0.2, y - r * 0.2, r * 0.7, C[6], 0.12);
  p.fade = saved;
}

function approach(p, t, C, H, seed) {
  const u = clamp(t / HIT), x = lerp(118, 0, ease.inOutQuad(u));
  const y = -Math.sin(u * Math.PI) * 8;
  const compression = 1 - 0.16 * smooth(0.82, HIT, t);
  const r = (24 + 38 * ease.inQuad(u)) * compression;
  const fade = smooth(0, 0.07, t);
  const N = (rate, s = 0, off = 0) => NL(t, rate, seed + s, off);
  for (let i = 0; i < 4; i++) {
    const dy = (i - 1.5) * r * 0.24;
    flameBlade(p, x + r * 0.45, y + dy, 1, (i - 1.5) * 0.035,
      28 + 8 * Math.sin(t * 9 + i), 3 + r * 0.045, (i - 1.5) * 3,
      N, seed + i * 11, H, BIG_LAYERS, 0.6, 1.2);
  }
  orb(p, x, y, r, t, C, H, seed, fade);
  for (let i = 0; i < 14; i++) {
    const age = (t * 1.3 + i * 0.047) % 0.42;
    const dx = r + 10 + age * 64, dy = Math.sin(i * 2.2) * (5 + age * 27);
    p.line(x + dx, y + dy, x + dx + 3, y + dy - 1, C[5], (1 - age / 0.42) * fade);
  }
  if (t > 0.8) p.glow(x, y, r * 1.2, C[6], smooth(0.8, HIT, t) * 0.35);
}

function detonate(p, age, C, H, seed) {
  const data = parameters(seed), N = (rate, s = 0, off = 0) => NL(age, rate, seed + s, off);
  const bodyFade = 1 - smooth(0.48, 1.1, age);
  // Smoke stays behind the hot fronts and rises gently as the fire burns out.
  for (const q of data.smoke) {
    const u = (age - q.delay) / q.life;
    if (u <= 0 || u >= 1) continue;
    smokePuff(p, Math.cos(q.a) * q.r, Math.sin(q.a) * q.r * 0.75 - q.rise * u,
      1 - u, q.size, C[1], true, 0.65);
  }
  const grow = ease.outCubic(clamp(age / 0.28));
  for (const q of data.lobes) {
    const u = (age - q.delay) / q.life;
    if (u < 0 || u >= 1) continue;
    const a = q.a + q.spin * u * 0.25, reach = q.r * grow;
    firePuff(p, Math.cos(a) * reach, Math.sin(a) * reach * 0.92 - u * 10,
      1 - u, q.size * (0.7 + 0.45 * grow), u * 0.5, H, 0.045, Math.cos(a), Math.sin(a));
  }
  if (bodyFade > 0) {
    const radius = (12 + 83 * ease.outExpo(clamp(age / 0.22))) * (1 - 0.15 * smooth(0.4, 1.1, age));
    const saved = p.fade;
    p.shade(-radius - 10, -radius - 10, radius + 10, radius + 10, (x, y) => {
      const a = Math.atan2(y, x), d = Math.hypot(x, y);
      const edge = radius * (1 + 0.075 * Math.sin(a * 9 + age * 15) + 0.045 * noise(a * 6 - age * 9, seed));
      if (d > edge) return;
      const turbulence = noise2(x * 0.095 + age * 5, y * 0.095 - age * 7, seed + 4);
      const erosion = smooth(0.48, 1.1, age);
      if (erosion > 0 && (turbulence + 1) * 0.5 < erosion) return;
      const heat = 1 - d / edge + turbulence * 0.25 - age * 0.42;
      return heat > 0.68 ? C[8] : heat > 0.51 ? C[7] : heat > 0.34 ? C[6]
        : heat > 0.19 ? C[5] : heat > 0.03 ? C[4] : C[2];
    });
    p.fade = saved;
    p.glow(0, 0, 126, C[4], 0.15 * bodyFade);
  }
  // Long, flickering flame tongues tear outward from the pressure front.
  if (age > 0.04 && age < 0.82) {
    const k = 1 - smooth(0.26, 0.82, age);
    for (let i = 0; i < 16; i++) {
      const a = i / 16 * TAU + Math.sin(i * 2.1) * 0.1, r = 43 + 34 * grow;
      flameBlade(p, Math.cos(a) * r, Math.sin(a) * r, Math.cos(a), Math.sin(a),
        (18 + (i % 4) * 6) * k, 5 * k + 1, Math.sin(age * 12 + i) * 5,
        N, 80 + i * 13, H, BIG_LAYERS, 0.9, 2);
    }
  }
  for (const q of data.debris) {
    const t = age - q.delay;
    if (t <= 0 || t >= q.life) continue;
    const u = t / q.life, r = 12 + q.speed * 0.3 * (1 - Math.exp(-t / 0.3));
    const a = q.a + q.turn * u * 0.2, x = Math.cos(a) * r, y = Math.sin(a) * r * 0.9 + t * t * 8;
    const k = 1 - u, vx = Math.cos(a) * q.speed * Math.exp(-t / 0.3), vy = Math.sin(a) * q.speed * Math.exp(-t / 0.3) + t * 16;
    if (q.size > 2.4) flameBlade(p, x - Math.cos(a) * 5 * k, y - Math.sin(a) * 5 * k,
      Math.cos(a), Math.sin(a), (6 + q.size * 3) * k, 1 + q.size * k * 0.6,
      Math.sin(t * 12 + a) * 2, N, Math.floor(q.a * 100), H, BIG_LAYERS, 0.4, 0.8);
    else spark(p, x, y, vx, vy, k, [C[7], C[6], C[4], C[2]]);
  }
  if (age < 0.5) {
    const u = age / 0.5, r = 20 + 118 * ease.outCubic(u);
    p.ring(0, 0, r, Math.max(1, 4 * (1 - u)), age < 0.08 ? C[7] : C[5], (1 - u) * 0.9);
    p.glowRing(0, 0, r, 4, C[4], 0.18 * (1 - u));
  }
  if (age < 0.08) {
    const u = age / 0.08;
    p.circle(0, 0, 30 + 27 * Math.sin(u * Math.PI), C[7]);
    p.circle(-2, -2, 20 + 20 * Math.sin(u * Math.PI), WHITE);
    p.glow(0, 0, 135, C[5], 0.45 * (1 - u));
  }
}

export function draw(p, t, o) {
  if (t <= 0 || t >= meta.duration) return;
  const C = fit(o.palette, 9), H = hotFirst(C), saved = p.fade;
  p.fade *= 1 - smooth(2.85, meta.duration, t);
  if (t < HIT) approach(p, t, C, H, o.seed);
  else detonate(p, t - HIT, C, H, o.seed);
  p.fade = saved;
}
