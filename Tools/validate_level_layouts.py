"""Conservative layout smoke checks, not a Unity/Box2D gameplay test.
Run from repository root: python Tools/validate_level_layouts.py
Uses the generated levels' 0.6 x 0.8 collider, speed 4.5, jump 8,
gravity 19.62 and a 0.02s semi-implicit step. Searches sample launch
positions and steering delays against all solid and hazard rectangles.
Does not model coyote time, input buffering, moving rotations or camera comfort.
"""
import json
from pathlib import Path

SOURCE = Path(__file__).resolve().parents[1] / 'Assets/HatHop/Editor/LevelData/ThreeLevels.json'
DT, SPEED, JUMP, GRAVITY = .02, 4.5, 8., 19.62
HALF_X, HALF_Y = .3, .4


def box(x, y, width, height, name=''):
    return dict(x=x, y=y, width=width, height=height, name=name)


def transformed(b, sign):
    return dict(b, x=b['x']*sign, y=b['y']*sign)


def intersects(x, y, b):
    return abs(x-b['x']) < HALF_X+b['width']/2-1e-5 and abs(y-b['y']) < HALF_Y+b['height']/2-1e-5


def points(p):
    span = p['width']/2-HALF_X-.06
    return [p['x']+span*f for f in [-1, -.7, 0, .7, 1]]


def trajectory(start, target, solids, hazards):
    top = target['y']+target['height']/2
    for x0 in points(start):
        for aim in points(target):
            for launch in [JUMP, 0.]:
                for delay in [0, .08, .16, .24, .32, .4, .5, .6, .7, .8, .9, 1.0]:
                    x, y, vy = x0, start['y']+start['height']/2+HALF_Y+.002, launch
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
                            break
    return False


def side_entry(start, target, solids, hazards):
    # After landing on the inverted outer exit face, walk off its open right edge,
    # wait until the head clears the ceiling, then steer onto the lower shelf.
    x = start['x']+start['width']/2-HALF_X-.06
    y = start['y']+start['height']/2+HALF_Y+.002
    vy=0.; entered=False
    outside=start['x']+start['width']/2+HALF_X+.04
    inside=target['x']+target['width']/2-HALF_X-.06
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


def check(l):
    ps=l['platforms']; ex=l['exitX']; ey=l['exitFloorY']; ew=l['exitWidth']; eh=l['exitHeight']
    floor=box(ex,ey,ew,.28,'Exit Floor')
    roof=box(ex,ey+eh,ew,.28,'Exit Roof')
    back=box(ex+ew/2,ey+eh/2,.28,eh+.28,'Exit Back')
    walls=[box(-l['roomWidth']/2,0,.4,l['roomHeight']+1),box(l['roomWidth']/2,0,.4,l['roomHeight']+1)]
    hazards=l['hazards']+[box(0,-l['roomHeight']/2,l['roomWidth'],.8),box(0,l['roomHeight']/2,l['roomWidth'],.8)]
    max_rise=max(b['y']-a['y'] for a,b in zip(ps,ps[1:]))
    assert max_rise < JUMP*JUMP/(2*GRAVITY)-.2
    assert l['roomHeight'] > 20
    assert eh-.28 > .8+.4, 'body and cosmetic hop fit in alcove'
    # The complete goal trigger is covered horizontally by solid faces in either orientation.
    assert ex+.1-.325 > ex-ew/2 and ex+.1+.325 < ex+ew/2
    assert ps[0]['y']-.14 > -l['roomHeight']/2+.4
    failed=[]
    for sign, mode in [(1,'upright'),(-1,'flipped')]:
        platforms=[transformed(p,sign) for p in ps]
        f,r,b=[transformed(p,sign) for p in [floor,roof,back]]
        solids=platforms+[f,r,b]+[transformed(w,sign) for w in walls]
        danger=[transformed(h,sign) for h in hazards]
        for i in range(len(platforms)-1):
            if not trajectory(platforms[i],platforms[i+1],solids,danger):failed.append(f'{mode} {i}->{i+1}')
        if sign==1:
            if not trajectory(platforms[-1],f,solids,danger):failed.append('upright exit approach')
        else:
            # Inverted approach can use the outer face of the floor, then enter from its side.
            if not trajectory(platforms[-1],f,solids,danger):failed.append('flipped outer exit landing')
            if not side_entry(f,r,solids,danger):failed.append('flipped side entry')
    print(f"{l['key']}: {l['roomWidth']} x {l['roomHeight']} units, {len(ps)} route platforms, max rise {max_rise:.2f}, hazards {len(l['hazards'])}")
    if failed:
        raise AssertionError('No sampled collision-free full-footprint landing: '+', '.join(failed))
    print('  PASS: sampled jumps in both orientations, exit approach, boundary clearance and straight-fall goal shielding.')


if __name__=='__main__':
    failures=[]
    for level in json.loads(SOURCE.read_text())['levels']:
        try:check(level)
        except AssertionError as e:failures.append(level['key']+': '+str(e))
    assert not failures, '\n'.join(failures)
    print('All static checks passed. Unity compile, full runs and live flips still require playtesting.')
