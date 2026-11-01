(() => {
  'use strict';
  const byId = id => document.getElementById(id);
  const fmt = (value, digits = 2) => (Math.abs(value) < 0.0000001 ? 0 : value).toFixed(digits);
  function calculate(degrees, length, facing) {
    const theta = degrees * Math.PI / 180;
    const sin = Math.sin(theta), cos = Math.cos(theta);
    const rx = length * sin, ry = length * cos;
    return { degrees, length, facing, theta, sin, cos, rx, ry,
      px: 100 + rx, py: 100 + ry, distance: Math.hypot(rx, ry),
      rotation: -theta, dx: facing * cos, dy: -facing * sin,
      vx: facing * 1000 * cos, vy: -facing * 1000 * sin };
  }
  function render() {
    const state = calculate(Number(byId('angle').value), Number(byId('length').value), Number(byId('facing').value));
    const {degrees,length,facing,sin,cos,rx,ry,dx,dy} = state;
    byId('angleLabel').textContent = fmt(degrees) + '°';
    byId('lengthLabel').textContent = fmt(length,0) + ' px';
    byId('angle').setAttribute('aria-valuetext', fmt(degrees) + '도');
    byId('length').setAttribute('aria-valuetext', fmt(length,0) + '픽셀');
    const hx=360, hy=280, scale=.5, px=hx+rx*scale, py=hy+ry*scale;
    const arrowStartX=px+dx*24, arrowStartY=py+dy*24;
    const arrowX=px+dx*95, arrowY=py+dy*95;
    const arcEndX=hx+48*sin, arcEndY=hy+48*cos;
    const angleArc=Math.abs(degrees)>.1 ? `<path d="M ${hx} ${hy+48} A 48 48 0 0 ${degrees>=0?0:1} ${arcEndX} ${arcEndY}" fill="none" stroke="#f9d47f" stroke-width="2"/>` : '';
    byId('chainStage').innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 600" role="img" aria-label="고정점과 플레이어의 거리, 사슬 각도, 시선 방향을 나타내는 도식">
      <defs><pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse"><path d="M 40 0 H 0 V 40" fill="none" stroke="#243340" stroke-width="1"/></pattern><marker id="arrow" viewBox="0 0 10 10" refX="8" refY="5" markerWidth="7" markerHeight="7" orient="auto"><path d="M 0 0 L 10 5 L 0 10 Z" fill="#ff78ad"/></marker></defs>
      <rect width="720" height="600" rx="18" fill="#101e2b"/><rect width="720" height="600" fill="url(#grid)"/>
      <g font-family="Malgun Gothic, sans-serif" font-size="14" fill="#d8e7ed">
      <text x="24" y="32" fill="#95adbd">화면 좌표  ·  오른쪽 +x  /  아래쪽 +y</text>
      <circle cx="${hx}" cy="${hy}" r="${length*scale}" fill="none" stroke="#476273" stroke-dasharray="5 8"/>
      <path d="M ${hx} ${hy} L ${px} ${hy} L ${px} ${py}" fill="none" stroke="#b7c9d3" stroke-width="2" stroke-dasharray="6 5"/>
      <path d="M ${hx} ${hy} V ${hy+70}" stroke="#f9d47f" stroke-dasharray="4 5"/>
      ${angleArc}<text x="${hx+56}" y="${hy+67}" fill="#f9d47f">θ = ${fmt(degrees)}°</text>
      <line x1="${hx}" y1="${hy}" x2="${px}" y2="${py}" stroke="#71e4d0" stroke-width="4"/>
      <circle cx="${hx}" cy="${hy}" r="7" fill="#71e4d0"/>
      <text x="${hx-20}" y="${hy+25}" text-anchor="end" fill="#71e4d0">H (100, 100)</text>
      <text x="${(hx+px)/2}" y="${hy-13}" text-anchor="middle">rx</text>
      <text x="${px+(rx>=0?20:-20)}" y="${(hy+py)/2+(Math.abs(ry)<32?-25:0)}" text-anchor="${rx>=0?'start':'end'}">ry</text>
      <line x1="${arrowStartX}" y1="${arrowStartY}" x2="${arrowX-dx*12}" y2="${arrowY-dy*12}" stroke="#ff78ad" stroke-width="4"/>
      <polygon points="${arrowX},${arrowY} ${arrowX-16*dx-7*dy},${arrowY-16*dy+7*dx} ${arrowX-16*dx+7*dy},${arrowY-16*dy-7*dx}" fill="#ff78ad"/>
      <g transform="translate(${px} ${py}) rotate(${-degrees})"><g transform="scale(${facing} 1)">
        <path d="M -8 -5 Q -28 -1 -24 20" fill="none" stroke="#ff78ad" stroke-width="5"/>
        <rect x="-12" y="-19" width="24" height="38" rx="9" fill="#18374b" stroke="#71e4d0" stroke-width="3"/>
        <path d="M 2 -10 H 12 V -5 H 2 Z" fill="#71e4d0"/>
        <circle cx="-5" cy="5" r="4" fill="#ff78ad"/>
      </g></g>
      <text x="${px-dx*34}" y="${py-dy*34+5}" text-anchor="middle" stroke="#101e2b" stroke-width="3" paint-order="stroke">P</text>
      <text x="24" y="555" fill="#71e4d0">청록 선: 사슬 길이 ${fmt(length,0)} px</text>
      <text x="24" y="579" fill="#ff78ad">분홍 화살표: ${facing===1?'D · 몸체 오른쪽':'A · 몸체 왼쪽'} 대시의 시작 방향</text>
      </g></svg>`;
    byId('liveMath').innerHTML = `<div><span>좌표 차이 (rx, ry)</span><strong>(${fmt(rx)}, ${fmt(ry)}) px</strong></div><div><span>Sin θ / Cos θ</span><strong>${fmt(sin,3)} / ${fmt(cos,3)}</strong></div><div><span>실제 거리 d / 길이 L</span><strong>${fmt(state.distance)} / ${fmt(length,0)} px</strong></div><div><span>몸체 회전 φ</span><strong>${fmt(-degrees)}°</strong></div><div><span>대시 방향 (x, y)</span><strong>(${fmt(dx,3)}, ${fmt(dy,3)})</strong></div><div><span>설명용 속도 · V = 1000 px/s</span><strong>(${fmt(state.vx,0)}, ${fmt(state.vy,0)}) px/s</strong></div>`;
    const points=[];
    for(let a=-180;a<=180;a+=2) points.push(`${40+(a+180)/360*520},${70-Math.sin(a*Math.PI/180)*42}`);
    byId('sinGraph').innerHTML=`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 150" role="img" aria-label="Sin 그래프와 현재 각도의 값"><path d="M 40 70 H 560 M 300 17 V 116" stroke="#8b9eab" fill="none"/><polyline points="${points.join(' ')}" fill="none" stroke="#16786d" stroke-width="3"/><circle cx="${40+(degrees+180)/360*520}" cy="${70-sin*42}" r="6" fill="#c13770"/><g font-family="Malgun Gothic, sans-serif" font-size="12" fill="#40556a">${[-180,-90,0,90,180].map(a=>`<text x="${40+(a+180)/360*520}" y="138" text-anchor="middle">${a}°</text>`).join('')}<text x="15" y="31">1</text><text x="10" y="115">−1</text><text x="335" y="22">sin(θ) = ${fmt(sin,3)}</text></g></svg>`;
    return state;
  }
  ['angle','length','facing'].forEach(id=>byId(id).addEventListener('input',render));
  document.querySelectorAll('[data-angle]').forEach(button=>button.addEventListener('click',()=>{byId('angle').value=button.dataset.angle;render();}));
  byId('printButton').addEventListener('click',()=>window.print());
  window.chainMathDiagram={calculate,render};
  render();
})();
