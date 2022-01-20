from PIL import Image
from os import listdir
from os.path import isfile, join

images = [f for f in listdir('./') if isfile(join('./', f))]
rez = ['main_id,supplementary_id,average_color']

for i in images:
    if(i.endswith('.png')):
        im = Image.open(i)
        r = 0
        g = 0
        b = 0
        for x in range(16):
            for y in range(16):
                pixel = im.getpixel((x, y))
                r = r + pixel[0]
                g = g + pixel[1]
                b = b + pixel[2]
        r = int(r/256)
        g = int(g/256)
        b = int(b/256)
        block = [j for j in i.split('.')[0].split('_')]
        rez.append(f"{block[0]},{block[1]},{(hex(r)[2:]).rjust(2, '0')}{(hex(g)[2:]).rjust(2, '0')}{(hex(b)[2:]).rjust(2, '0')}".upper())
        im.close()

open('blocks.csv', 'w').write('\n'.join(rez))
