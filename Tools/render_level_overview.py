"""Optional design diagram; requires matplotlib. It is not a Unity screenshot."""
import json
from pathlib import Path
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import Rectangle
ROOT=Path(__file__).resolve().parents[1]
levels=json.loads((ROOT/'Assets/HatHop/Editor/LevelData/ThreeLevels.json').read_text())['levels']
fig,axes=plt.subplots(1,3,figsize=(13,11),facecolor='#0d1422')
for ax,l in zip(axes,levels):
    ax.set_facecolor('#0d1422');w,h=l['roomWidth'],l['roomHeight'];accent=l['accent']
    ax.add_patch(Rectangle((-w/2,-h/2),w,h,facecolor='#162435',edgecolor='#718397',lw=1.2))
    def rect(x,y,w,h,color):ax.add_patch(Rectangle((x-w/2,y-h/2),w,h,color=color))
    for i,p in enumerate(l['platforms']):
        rect(p['x'],p['y'],p['width'],p['height'],'#ffc740' if p.get('seesaw') else accent if i%4==0 else '#d2dce7')
        if p.get('redUnderside'):rect(p['x'],p['y']-.6*p['height'],p['width'],.25*p['height'],'#ff4940')
        if p.get('seesaw'):ax.text(p['x'],p['y']-.7,'TILT',fontsize=7,ha='center',color='#ffc740')
    for p in l['bonusPlatforms']:rect(p['x'],p['y'],p['width'],p['height'],accent)
    for q in l['pockets']:
        rect(q['x'],q['y']+q['height'],q['width']+.2,.2,'#cc9d46')
        for sign in [-1,1]:rect(q['x']+sign*q['width']/2,q['y']+q['height']/2,.2,q['height']+.2,'#cc9d46')
    for p in l['stars']:ax.scatter(p['x'],p['y'],marker='*',s=105,color='#ffdb30',zorder=10)
    for y in [-h/2,h/2]:rect(0,y,w,.8,'#ff4940')
    for p in l['hazards']:rect(p['x'],p['y'],p['width'],p['height'],'#ff4940')
    ex,ey,ew,eh=[l[k] for k in ['exitX','exitFloorY','exitWidth','exitHeight']]
    for y in [ey,ey+eh]:rect(ex,y,ew,.28,accent)
    rect(ex+ew/2,ey+eh/2,.28,eh+.28,accent)
    rect(ex+.1,ey+eh/2,.65,eh-.5,'#45f59b')
    start=l['platforms'][0];rect(start['x'],start['y']+.56,.6,.8,'#39cef4')
    ax.text(-w/2+.5,-h/2+1,'START',fontsize=8,color='#39cef4')
    ax.annotate('EXIT',(ex,ey+eh/2),xytext=(0,ey+eh/2),ha='center',fontsize=8,color='#45f59b',arrowprops={'arrowstyle':'-','color':'#45f59b','lw':.7})
    ax.set_title(f"{l['key'].upper()} / {l['title']}\n5 stars · {h:.1f} units tall",fontsize=11,color='#eef4ff',pad=15,linespacing=1.7)
    ax.set_xlim(-10.7,10.7);ax.set_ylim(-24,24);ax.set_aspect('equal');ax.axis('off')
fig.suptitle('HAT HOP / STARS & PLATFORM CHALLENGES',fontsize=20,color='#eef4ff',y=.98)
fig.text(.5,.04,'Gold pockets require a flip. Red faces become dangerous landings when inverted.\nHard seesaws are shown neutral; the two elevated gaps use their raised tips.\nDesign overview at shared scale, not the close-up gameplay view. Unity playtesting is pending.',ha='center',color='#a6b5c9',fontsize=10,linespacing=1.6)
fig.subplots_adjust(top=.90,bottom=.12,wspace=.1,left=.04,right=.96)
fig.savefig(ROOT/'Docs/Three_Levels_Overview.png',dpi=160,facecolor=fig.get_facecolor())
