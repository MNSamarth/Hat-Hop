"""Geometry smoke checks, not a Unity/Box2D gameplay test.
Run: python Tools/validate_level_layouts.py
Uses a 0.6 x 0.8 body, speed 4.5, jump 8, gravity 19.62, dt 0.02.
Models fixed/tilted rectangles, sampled jump/steering choices, ceiling stops,
walking drops and explicit safe ledges. Checks raised-tip reach separately
from neutral reach, red-face bypasses, flip-only pocket entry and recovery.
Does not simulate the moving seesaw, contact transport, live world turns,
input buffers, renderer/UI, saved scores or human timing.
"""
import json
import math
from pathlib import Path

SOURCE = Path(__file__).resolve().parents[1] / 'Assets/HatHop/Editor/LevelData/ThreeLevels.json'
DT, SPEED, JUMP, GRAVITY = .02, 4.5, 8., 19.62
HALF_X, HALF_Y = .3, .4


def box(x, y, width, height, name=''):
    return dict(x=x, y=y, width=width, height=height, name=name)


def transformed(b, sign):
    return dict(b, x=b['x']*sign, y=b['y']*sign)


def intersects(x, y, b):
    angle=math.radians(b.get('angle',0))
    if not angle:
        return abs(x-b['x']) < HALF_X+b['width']/2-1e-5 and abs(y-b['y']) < HALF_Y+b['height']/2-1e-5
    c,t=math.cos(angle),math.sin(angle);dx=x-b['x'];dy=y-b['y'];w=b['width']/2;h=b['height']/2
    return (abs(dx)<HALF_X+abs(c)*w+abs(t)*h-1e-5 and
            abs(dy)<HALF_Y+abs(t)*w+abs(c)*h-1e-5 and
            abs(dx*c+dy*t)<w+HALF_X*abs(c)+HALF_Y*abs(t)-1e-5 and
            abs(-dx*t+dy*c)<h+HALF_X*abs(t)+HALF_Y*abs(c)-1e-5)


def surface(p,x):
    a=math.radians(p.get('angle',0))
    return p['y']+p['height']/2/math.cos(a)+math.tan(a)*(x-p['x'])+abs(math.tan(a))*HALF_X


def points(p):
    span = p['width']/2*math.cos(math.radians(p.get('angle',0)))-HALF_X-.06
    return [p['x']+span*f for f in [-1, -.9, -.8, -.7, 0, .7, .8, .9, 1]]


def trajectory(start, target, solids, hazards):
    top = target['y']+target['height']/2
    for x0 in points(start):
        for aim in points(target):
            for launch in [JUMP, 0.]:
                for delay in [0, .08, .16, .24, .32, .4, .5, .6, .7, .8, .9, 1.0]:
                    x, y, vy = x0, surface(start,x0)+HALF_Y+.002, launch
                    for step in range(100):
                        old_y = y
                        vy -= GRAVITY*DT
                        y += vy*DT
                        if step*DT >= delay:
                            x += max(-SPEED*DT, min(SPEED*DT, aim-x))
                        if any(intersects(x,y,h) for h in hazards):
                            break
                        hit = next((b for b in solids if intersects(x,y,b)), None)
                        if hit is not None:
                            if hit is target and vy < 0 and old_y-HALF_Y >= top-.005 and abs(x-target['x']) <= target['width']/2-HALF_X-.025:
                                return True
                            if hit is start and launch == 0. and vy < 0 and old_y-HALF_Y >= start['y']+start['height']/2-.005:
                                y=start['y']+start['height']/2+HALF_Y+.002
                                vy=0
                                continue
                            if vy > 0 and not hit.get('angle') and old_y+HALF_Y <= hit['y']-hit['height']/2+.005:
                                y=hit['y']-hit['height']/2-HALF_Y-.002
                                vy=0
                                continue
                            break
    return False


def side_entry(start, target, solids, hazards, side=1):
    # After landing on the inverted outer exit face, walk off its open right edge,
    # wait until the head clears the ceiling, then steer onto the lower shelf.
    x = start['x']+side*(start['width']/2-HALF_X-.06)
    y = start['y']+start['height']/2+HALF_Y+.002
    vy=0.; entered=False
    outside=start['x']+side*(start['width']/2+HALF_X+.04)
    inside=target['x']+side*(target['width']/2-HALF_X-.06)
    for step in range(150):
        old_y=y
        if y+HALF_Y < start['y']-start['height']/2-.015:entered=True
        aim=inside if entered else outside
        x+=max(-SPEED*DT,min(SPEED*DT,aim-x))
        vy-=GRAVITY*DT; y+=vy*DT
        if any(intersects(x,y,h) for h in hazards):return False
        hit=next((b for b in solids if intersects(x,y,b)),None)
        if hit is start and old_y-HALF_Y>=start['y']+start['height']/2-.005:
            y=start['y']+start['height']/2+HALF_Y+.002;vy=0
        elif hit is target and vy<0 and abs(x-target['x'])<=target['width']/2-HALF_X-.025:
            return True
        elif hit is not None:return False
    return False


def geometry(l, sign):
    ps=[transformed(p,sign) for p in l['platforms']]
    ex,ey,ew,eh=[l[k] for k in ['exitX','exitFloorY','exitWidth','exitHeight']]
    f,r,b=[transformed(p,sign) for p in [box(ex,ey,ew,.28,'Exit Floor'),box(ex,ey+eh,ew,.28,'Exit Roof'),box(ex+ew/2,ey+eh/2,.28,eh+.28,'Exit Back')]]
    solids=ps+[f,r,b]+[transformed(p,sign) for p in l['bonusPlatforms']]
    solids += [transformed(box(side*l['roomWidth']/2,0,.4,l['roomHeight']+1),sign) for side in [-1,1]]
    caps=[]
    for q in l['pockets']:
        cap=transformed(box(q['x'],q['y']+q['height'],q['width']+.2,.2,q['name']),sign)
        caps.append(cap);solids.append(cap)
        solids += [transformed(box(q['x']+side*q['width']/2,q['y']+q['height']/2,.2,q['height']+.2),sign) for side in [-1,1]]
    hazards=[transformed(h,sign) for h in l['hazards']]
    hazards += [transformed(box(0,side*l['roomHeight']/2,l['roomWidth'],.8),sign) for side in [-1,1]]
    for p in l['platforms']:
        if p.get('redUnderside'):hazards.append(transformed(box(p['x'],p['y']-.6*p['height'],p['width'],p['height']*.25),sign))
    return ps,solids,hazards,f,r,caps


def pocket_escape(q, target, solids, hazards):
    side=-1 if q['x']>0 else 1
    x=q['x']+side*(q['width']/2-.1-HALF_X-.04)
    y=q['y']+q['height']-.1-HALF_Y-.002
    vy=0
    for step in range(160):
        old_y=y
        if y+HALF_Y<q['y']-.115:
            aim=target['x']-side*(target['width']/2-HALF_X-.06)
            x+=max(-SPEED*DT,min(SPEED*DT,aim-x))
        vy-=GRAVITY*DT;y+=vy*DT
        if any(intersects(x,y,h) for h in hazards):return False
        hit=next((b for b in solids if intersects(x,y,b)),None)
        if hit is target and vy<0 and old_y-HALF_Y>=target['y']+target['height']/2-.005 and abs(x-target['x'])<=target['width']/2-HALF_X-.025:return True
        if hit is not None:return False
    return False


def check(l):
    failed=[]
    assert len(l['stars'])==5 and len(l['pockets'])==2
    assert len({(star['x'],star['y']) for star in l['stars']})==5
    assert all(not (p.get('seesaw') and p.get('redUnderside')) for p in l['platforms'])
    assert all(l[key] > 0 for key in ('traversalSeconds', 'warningSeconds', 'turnSeconds'))
    for i,q in enumerate(l['pockets'], 1):
        assert q['width']-.2 > 2*HALF_X+.4
        assert abs(q['x'])+q['width']/2+.1 < l['roomWidth']/2-.2
        assert abs(q['y']+q['height']) < l['roomHeight']/2-.4
        # Even a best-case launch with feet at the mouth cannot touch the star.
        star=next(s for s in l['stars'] if s['name']==f'Flip Star {i}')
        assert abs(star['x']-q['x'])+.55*.38 < q['width']/2-.1
        assert star['y']+.55*.38 < q['y']+q['height']-.1
        minimum_star_bottom=star['y']-q['y']-.55*.38
        assert minimum_star_bottom>JUMP*JUMP/(2*GRAVITY)+2*HALF_Y
    for sign,mode in [(1,'upright'),(-1,'flipped')]:
        ps,solids,hazards,f,r,caps=geometry(l,sign)
        route=[i for i,p in enumerate(l['platforms']) if sign==1 or not p.get('redUnderside')]
        for a,b in zip(route,route[1:]):
            start,target=ps[a],ps[b]
            if sign==1 and start.get('seesaw'):
                if trajectory(start,target,solids,hazards):failed.append(f'neutral seesaw {a} unexpectedly reaches ledge')
                start['angle']=18
            okay=trajectory(start,target,solids,hazards)
            if not okay and sign==-1:
                okay=any(side_entry(start,target,solids,hazards,side) for side in [-1,1])
            if not okay:
                helpers=[p for p in solids if p.get('name','').startswith('Pocket ') and abs(p['y']-start['y'])<4]
                for helper in helpers:
                    if trajectory(start,helper,solids,hazards) and trajectory(helper,target,solids,hazards):
                        okay=True;break
            if not okay:failed.append(f'{mode} {a}->{b}')
            start.pop('angle',None)
        if not trajectory(ps[-1],f,solids,hazards):failed.append(mode+' exit approach')
        if sign==-1 and not side_entry(f,r,solids,hazards):failed.append('flipped exit side entry')
        for i,q in enumerate(l['pockets']):
            if sign==-1:
                if not trajectory(ps[q['anchor']],caps[i],solids,hazards):failed.append(f'flip pocket {i+1} entry')
            else:
                recovery=next(p for p in solids if p.get('name')==f'Pocket {i+1} Recovery')
                step=next(p for p in solids if p.get('name')==f'Pocket {i+1} Return Step')
                if not pocket_escape(q,recovery,solids,hazards):failed.append(f'pocket {i+1} escape')
                if not trajectory(recovery,step,solids,hazards):failed.append(f'pocket {i+1} return step')
                if not trajectory(step,ps[q['anchor']-1],solids,hazards):failed.append(f'pocket {i+1} rejoin')
    print(l['key']+': '+('PASS' if not failed else ', '.join(failed)),flush=True)
    return failed


if __name__=='__main__':
    failures=[]
    for level in json.loads(SOURCE.read_text())['levels']:
        failures += [level['key']+': '+problem for problem in check(level)]
    assert not failures, '\n'.join(failures)
    print('Sampled static routes, red-face detours, flip pocket entry/escape and raised-tip reach passed. Live physics tests remain pending.')
