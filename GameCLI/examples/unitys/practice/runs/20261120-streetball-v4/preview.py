from pathlib import Path
from PIL import Image,ImageDraw
r=Path('GameCLI/examples/unitys/practice/runs/20261120-streetball-v4')
sheet=Image.new('RGB',(1150,720),'#262a31');d=ImageDraw.Draw(sheet)
for j,n in enumerate(['settings-state-a.png','settings-state-b.png']):
 im=Image.open(r/n).crop((660,630,900,705)).resize((480,150));sheet.paste(im,(j*570,40));d.text((j*570+10,15),'OFF' if j==0 else 'ON',fill='white')
im=Image.open(r/'home-fixed.png').crop((1360,450,1860,640));im.thumbnail((1000,400));sheet.paste(im,(280,240));d.text((280,215),'Home: training / recruit spacing',fill='white');sheet.save(r/'validated-details.png')
