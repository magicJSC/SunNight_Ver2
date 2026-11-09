"""Generate the v0.3 design diagram from the verified Unity placement export."""
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

root = Path(__file__).resolve().parents[1]
data = json.loads((root / 'Logs/OpenField/layout-audit.json').read_text(encoding='utf-8-sig'))
im = Image.new('RGB', (1640, 1080), '#111c22')
d = ImageDraw.Draw(im)
font = lambda size: ImageFont.truetype('C:/Windows/Fonts/malgun.ttf', size)
def xy(x, y): return (60 + (x + 112)*2.96, 155 + (68-y)*2.96)
def label(x,y,text,size=18,color='#e7eeea'):
    d.text(xy(x,y),text,font=font(size),fill=color,stroke_width=2,stroke_fill='#16272b')
def ellipse(x,y,rx,ry,fill,outline):
    a,b=xy(x-rx,y+ry); c,e=xy(x+rx,y-ry)
    d.ellipse((a,b,c,e),fill=fill,outline=outline,width=2)
d.text((60,35),'SunNight · 자유 탐색 필드 v0.3',font=font(38),fill='#f1f2e9')
d.text((62,93),'연결 길을 없애고, 보급·폐허·위험을 발견하며 방향을 선택합니다.',font=font(22),fill='#b8c9c8')
d.rectangle((55,150,1240,958),outline='#3b4e53',width=2)
for x in range(-100,281,50):
    a,b=xy(x,68); c,e=xy(x,-202); d.line((a,b,c,e),fill='#203137')
    d.text((a-12,965),str(x),font=font(12),fill='#789394')
for y in range(-200,69,50):
    a,b=xy(-112,y); c,e=xy(285,y); d.line((a,b,c,e),fill='#203137')
    d.text((14,b-8),str(y),font=font(12),fill='#789394')
ellipse(-77,-118,32,70,'#1b342f','#2c5146')
ellipse(17,-152,43,47,'#30332c','#515244')
ellipse(147,-164,60,35,'#34312b','#665344')
ellipse(135,25,61,34,'#20313a','#3b5366')
ellipse(70,-70,30,30,'#203d37','#65b799')
for rect,name in [((-90,-40,-19,44),'튜토리얼'),((-32,-174,-14,-125),'기존 주거지'),((129,-146,182,-105),'기존 전투 시설'),((230,-47,282,-14),'폭스턴 공장')]:
    x0,y0,x1,y1=rect
    a,b=xy(x0,y1); c,e=xy(x1,y0)
    d.rectangle((a,b,c,e),fill='#3b3b38',outline='#8f9186',width=2)
    d.text((a+5,b+7),name,font=font(16),fill='#e5e6dc')
for g in data['patrolChecks']:
    x,y=xy(g['x'],g['y'])
    color='#ee927a' if g['patrolRadius'] else '#bd6a61'
    if g['patrolRadius']:
        d.line((x-9,y,x+9,y),fill=color,width=2)
        d.polygon([(x,y-6),(x-6,y+5),(x+6,y+5)],fill=color)
    else: d.ellipse((x-5,y-5,x+5,y+5),fill=color)
for r in data['pickupChecks']:
    x,y=xy(r['x'],r['y'])
    if r['collectSeconds']==2:
        d.ellipse((x-4,y-4,x+4,y+4),fill='#f1d982')
    else:
        color='#72ccdb' if r['group'] else '#bad87f'
        d.rectangle((x-5,y-5,x+5,y+5),fill=color,outline='#15232b')
        if r['group']: d.text((x+7,y-8),r['id'],font=font(12),fill=color)
label(-101,-82,'숲 · 야영 흔적',17)
label(0,-155,'생활 폐허',17)
label(105,-198,'고철장 · 고위험 보상',17)
label(104,64,'작업장 · 정비 부품',17)
label(45,-59,'중앙 야영지',20)
label(-6,-37,'열린 공터',18)
label(216,-56,'공장 외곽',17)
x=1280
d.text((x,164),'배치 원칙',font=font(26),fill='#f1f2e9')
rows=[('● 작은 고철 34곳','#f1d982'),('■ 상자·식량·보급 11곳','#bad87f'),('■ 경비 보상','#72ccdb'),('▲ 순찰 몬스터 14마리','#ee927a'),('● 보상 경비 11마리','#bd6a61')]
for i,(s,c) in enumerate(rows): d.text((x,222+i*43),s,font=font(18),fill=c)
for i,s in enumerate(['총 재화는 유지','고철 71 / 부품 6','음식 4 / 약 5','(튜토리얼·첫밤 포함)','','작은 고철: 2초 조사','보상 상자: 4초 조사','','얇은 구역 표시 = 분위기','이동을 막는 경계가 아님','','공장 방문 순서 자유','경비는 상자만 지킴']):
    d.text((x,480+i*30),s,font=font(17),fill='#b5c9ca')
d.text((60,1010),'실제 Unity 좌표 기반 배치도 · 길/방문 순서 표시 없음 · 기존 건물 내부는 원본 유지',font=font(18),fill='#a0b6b8')
out=root/'docs/level-design/v03-open-field-layout.png'
im.save(out)
print(out)
