from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'research/python-libs'))
from PIL import Image,ImageDraw
files=list((ROOT/'extracted-resources/assets/data/ui/background').glob('*.png'))+list((ROOT/'extracted-resources/assets/data/ui/fullbody').glob('*.png'))+list((ROOT/'extracted-resources/fairygui/Main').rglob('*.png'))+list((ROOT/'extracted-resources/fairygui/Common/images').glob('*btn*.png'))
files=[p for p in files if Image.open(p).width<1800][:160]
for page in range((len(files)+39)//40):
    sheet=Image.new('RGB',(1000,1500),'#666666'); draw=ImageDraw.Draw(sheet)
    for i,p in enumerate(files[page*40:page*40+40]):
        im=Image.open(p).convert('RGBA'); im.thumbnail((190,150))
        x=(i%5)*200;y=(i//5)*187
        sheet.paste(im,(x+(190-im.width)//2,y),im)
        draw.text((x,y+151),p.stem[:27],fill='white')
        draw.text((x,y+166),str(Image.open(p).size),fill='white')
    sheet.save(Path(__file__).parent/f'contact-{page}.jpg')
print(len(files))
