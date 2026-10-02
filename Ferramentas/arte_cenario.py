# -*- coding: utf-8 -*-
"""
Cenário: tiles (com auto-tiling de 16 combinações), plataformas, perigos,
água/brasas animadas, decorações, árvores grandes e camadas de fundo
(parallax) das duas fases.
"""
import math
import random
from pixelart import Canvas, cor, misturar, com_alpha, ruido_periodico

K = cor('#2a1a22')

TEMAS = {
    'mata': {
        'grama': [cor('#b4ec6a'), cor('#6cc94a'), cor('#3f9a3e'), cor('#2a6e34')],
        'terra': [cor('#a8744a'), cor('#8a5a3c'), cor('#6b4430'), cor('#4d2f25')],
        'pedra': [cor('#b8b0a8'), cor('#8a827c')],
        'raiz': cor('#5a3826'),
        'madeira': [cor('#d8a060'), cor('#b07a4a'), cor('#7e5230')],
        'cipo': cor('#3f9a3e'),
    },
    'queimada': {
        'grama': [cor('#9a9496'), cor('#6e6a6e'), cor('#4a4650'), cor('#2e2a34')],
        'terra': [cor('#5e4a44'), cor('#4a3834'), cor('#382a28'), cor('#241a1a')],
        'pedra': [cor('#7a7478'), cor('#55505a')],
        'raiz': cor('#1c1414'),
        'madeira': [cor('#6a5a52'), cor('#4a3c38'), cor('#2e2424')],
        'cipo': cor('#ff7a2a'),
    },
}

CIMA, DIREITA, BAIXO, ESQUERDA = 1, 2, 4, 8


def tile_terra(tema, mascara, variante=0):
    """
    Gera o tile de chão para a máscara de vizinhos (bits: 1=cima, 2=direita,
    4=baixo, 8=esquerda). Bit ligado = existe chão vizinho naquele lado.
    """
    t = TEMAS[tema]
    rnd = random.Random(mascara * 31 + variante * 977 + (7 if tema == 'queimada' else 0))
    c = Canvas(16, 16)
    terra = t['terra']
    # --- preenchimento de terra com textura
    for y in range(16):
        for x in range(16):
            v = rnd.random()
            cc = terra[1]
            if v < 0.10:
                cc = terra[2]
            elif v < 0.16:
                cc = terra[0]
            c.set(x, y, cc)
    # pedrinhas e raízes
    for _ in range(2 + variante):
        px, py = rnd.randint(2, 12), rnd.randint(6, 13)
        c.rect(px, py, 2, 2, t['pedra'][1])
        c.set(px, py, t['pedra'][0])
    if variante == 2:
        for i in range(5):
            c.set(3 + i, 9 + (i % 2), t['raiz'])
    # --- bordas expostas
    aberto_cima = not (mascara & CIMA)
    aberto_dir = not (mascara & DIREITA)
    aberto_baixo = not (mascara & BAIXO)
    aberto_esq = not (mascara & ESQUERDA)

    if aberto_baixo:
        for x in range(16):
            c.set(x, 13, terra[2])
            c.set(x, 14, terra[3])
            c.set(x, 15, K)
    if aberto_esq:
        for y in range(16):
            c.set(1, y, terra[2])
            c.set(0, y, K)
    if aberto_dir:
        for y in range(16):
            c.set(14, y, terra[2])
            c.set(15, y, K)

    if aberto_cima:
        g = t['grama']
        # faixa de grama com borda irregular por baixo
        for x in range(16):
            fundo = 4 + (1 if (x * 7 + mascara) % 5 == 0 else 0) + (1 if x % 4 == 1 else 0)
            for y in range(0, fundo + 1):
                cc = g[1]
                if y <= 1:
                    cc = g[0]
                if y >= fundo - 1:
                    cc = g[2]
                if y == fundo:
                    cc = g[3]
                c.set(x, y, cc)
        # recortes no topo (pontas de capim)
        for x in range(16):
            if (x + mascara) % 3 == 0:
                c.set(x, 0, None)
        c.hline(0, 15, 1, g[0])
        for x in range(0, 16, 3):
            c.set(x, 0, g[0] if (x + mascara) % 3 else None)
        # contorno superior
        for x in range(16):
            if c.get(x, 0) is None:
                c.set(x, 1, g[0])
        if aberto_esq:
            for y in range(0, 7):
                c.set(0, y, K)
                c.set(1, y, g[2])
            c.set(0, 0, None)
        if aberto_dir:
            for y in range(0, 7):
                c.set(15, y, K)
                c.set(14, y, g[2])
            c.set(15, 0, None)
        if tema == 'queimada':
            # brasinhas acesas nas cinzas
            for _ in range(2):
                bx = rnd.randint(2, 13)
                c.set(bx, rnd.randint(2, 4), cor('#ff7a2a'))
    # quinas arredondadas
    if aberto_baixo and aberto_esq:
        c.set(0, 15, None)
        c.set(1, 14, K)
    if aberto_baixo and aberto_dir:
        c.set(15, 15, None)
        c.set(14, 14, K)
    return c


def tiles_chao(tema):
    q = []
    for m in range(16):
        q.append(('%s_terra_%d' % (tema, m), tile_terra(tema, m, 0)))
    q.append(('%s_terra_15b' % tema, tile_terra(tema, 15, 1)))
    q.append(('%s_terra_15c' % tema, tile_terra(tema, 15, 2)))
    return q


def tile_plataforma(tema, tipo):
    """Plataforma de madeira atravessável por baixo. tipo: e, m, d, u (única)."""
    t = TEMAS[tema]
    mad = t['madeira']
    c = Canvas(16, 16)
    for y in range(0, 6):
        for x in range(16):
            cc = mad[1]
            if y == 0:
                cc = mad[0]
            if y >= 4:
                cc = mad[2]
            if (x + y * 5) % 9 == 0 and 0 < y < 4:
                cc = mad[2]
            c.set(x, y, cc)
    for x in range(16):
        c.set(x, 6, K)
    # emendas entre tábuas
    if tipo in ('m',):
        c.vline(8, 1, 4, mad[2])
    if tipo in ('e', 'u'):
        for y in range(0, 7):
            c.set(0, y, K)
        c.set(1, 7, mad[2])
        c.set(1, 8, mad[2])
        c.set(1, 9, K)
    if tipo in ('d', 'u'):
        for y in range(0, 7):
            c.set(15, y, K)
        c.set(14, 7, mad[2])
        c.set(14, 8, mad[2])
        c.set(14, 9, K)
    # detalhe: cipó (mata) ou rachaduras em brasa (queimada)
    if tema == 'mata' and tipo in ('m', 'u'):
        c.set(5, 7, t['cipo'])
        c.set(5, 8, t['cipo'])
        c.set(6, 9, t['cipo'])
        c.set(6, 10, cor('#6cc94a'))
    if tema == 'queimada':
        c.set(4, 2, cor('#ff7a2a'))
        c.set(5, 3, cor('#ec4a2a'))
        c.set(11, 2, cor('#ffb03a'))
    return c


def tiles_plataforma(tema):
    return [('%s_plataforma_%s' % (tema, tp), tile_plataforma(tema, tp)) for tp in ('e', 'm', 'd', 'u')]


def tile_espinho(tema, fase=0):
    """
    Espinhos grandes e pontudos (mata: espinheiro; queimada: galhos
    carbonizados com pontas em brasa). Ocupam a parte de baixo do tile.
    """
    c = Canvas(16, 16)
    if tema == 'mata':
        luz, meio, sombra = cor('#fff4d6'), cor('#e2cfa0'), cor('#a88e62')
        base, base_s = cor('#2f6a34'), cor('#1e4a26')
    else:
        luz, meio, sombra = cor('#6a5e62'), cor('#4e4448'), cor('#2e2626')
        base, base_s = cor('#2e2626'), cor('#1a1416')
    # três espinhos triangulares
    for (cx, topo, meia) in ((3.5, 5, 3.0), (8.5, 3, 3.4), (13.0, 6, 2.8)):
        for y in range(topo, 14):
            t = (y - topo) / float(14 - topo)
            w = meia * t
            for x in range(int(cx - w - 0.5), int(cx + w + 0.5) + 1):
                if abs(x + 0.5 - cx) <= w + 0.35:
                    rel = (x + 0.5 - cx)
                    cc = meio
                    if rel < -w * 0.35:
                        cc = luz
                    elif rel > w * 0.4:
                        cc = sombra
                    c.set(x, y, cc)
        # ponta
        pc = luz if tema == 'mata' else [cor('#ffdc4a'), cor('#ff9e2c'), cor('#ec4a2a')][(fase + int(cx)) % 3]
        c.set(int(cx), topo - 1, pc)
        c.set(int(cx), topo, pc)
    # base de ramos
    for y in range(12, 16):
        for x in range(16):
            c.set(x, y, base if (x + y) % 5 else base_s)
    c.hline(0, 15, 15, base_s)
    if tema == 'mata':
        c.set(6, 12, cor('#58bd48'))
        c.set(11, 12, cor('#58bd48'))
    else:
        c.set(6, 13, cor('#ff7a2a'))
        c.set(11, 14, cor('#ffb03a'))
    c.contorno(K)
    return c


def tiles_espinho(tema):
    if tema == 'mata':
        return [('mata_espinho_0', tile_espinho('mata'))]
    return [('queimada_espinho_%d' % i, tile_espinho('queimada', i)) for i in range(3)]


def tile_liquido(tema, topo, fase):
    """Água do rio (mata) ou poço de brasas (queimada). Animado."""
    c = Canvas(16, 16)
    if tema == 'mata':
        cores = [cor('#dff6ff'), cor('#5ec8e6'), cor('#2e8ac0'), cor('#1d5a8a')]
        alpha = 225
    else:
        cores = [cor('#fff6b8'), cor('#ffb03a'), cor('#ec4a2a'), cor('#8a1a22')]
        alpha = 255
    for y in range(16):
        for x in range(16):
            onda = math.sin((x + fase * 4) * (2 * math.pi / 16)) * 1.2
            if topo:
                sup = 4 + onda
                if y < sup:
                    continue
                if y < sup + 1.2:
                    cc = cores[0]
                elif y < sup + 3:
                    cc = cores[1]
                elif y < 11:
                    cc = cores[2]
                else:
                    cc = cores[3]
            else:
                cc = cores[3] if (x + y + fase) % 7 else cores[2]
                if tema == 'queimada' and (x * 3 + y * 5 + fase * 2) % 11 == 0:
                    cc = cores[1]
            c.set(x, y, com_alpha(cc, alpha))
    if topo:
        # brilhos
        for (bx, by) in ((3, 6), (11, 7)):
            c.set((bx + fase * 2) % 16, by, com_alpha(cores[0], alpha))
    return c


def tiles_liquido(tema):
    q = []
    for f in range(4):
        q.append(('%s_liquido_topo_%d' % (tema, f), tile_liquido(tema, True, f)))
    for f in range(4):
        q.append(('%s_liquido_fundo_%d' % (tema, f), tile_liquido(tema, False, f)))
    return q


# ------------------------------------------------------------ decorações pequenas
def deco_mata():
    q = []
    verde = [cor('#b4ec6a'), cor('#6cc94a'), cor('#3f9a3e'), cor('#2a6e34')]
    for v in range(3):
        c = Canvas(16, 16)
        rnd = random.Random(v * 13 + 1)
        for i in range(5 + v):
            x = rnd.randint(2, 13)
            h = rnd.randint(3, 6)
            for j in range(h):
                c.set(x + (j // 3) * (1 if i % 2 else -1), 15 - j, verde[1 if j < h - 1 else 0])
            c.set(x, 15, verde[2])
        c.contorno(K)
        q.append(('mata_deco_tufo_%d' % v, c))
    petalas = [cor('#ff5a6a'), cor('#ffd84a'), cor('#f4f0ff')]
    for v in range(3):
        c = Canvas(16, 16)
        caule_x = 7 + v % 2
        c.vline(caule_x, 9, 15, verde[2])
        c.set(caule_x - 1, 13, verde[1])
        c.set(caule_x + 1, 12, verde[1])
        p = petalas[v]
        for (dx, dy) in ((0, -1), (-1, 0), (1, 0), (0, 1)):
            c.set(caule_x + dx, 7 + dy, p)
        c.set(caule_x, 7, cor('#f08a2a'))
        if v == 1:
            c.vline(3, 11, 15, verde[2])
            c.set(3, 10, petalas[0])
        c.contorno(K)
        q.append(('mata_deco_flor_%d' % v, c))
    # bromélia
    c = Canvas(16, 16)
    for i, ang in enumerate((-60, -30, 0, 30, 60)):
        a = math.radians(ang - 90)
        for r in range(7):
            c.set(8 + math.cos(a) * r * 0.9, 15 + math.sin(a) * r, verde[1] if r < 5 else verde[0])
    c.rect(7, 8, 2, 3, cor('#ff4a6a'))
    c.set(7, 7, cor('#ffb0c0'))
    c.contorno(K)
    q.append(('mata_deco_bromelia', c))
    # samambaia
    c = Canvas(16, 16)
    for lado in (-1, 1):
        for r in range(8):
            x = 8 + lado * r
            y = 15 - int(r * 1.1) + (r * r) // 12
            c.set(x, y, verde[2])
            if r % 2 == 0:
                c.set(x, y - 1, verde[1])
    c.vline(8, 8, 15, verde[2])
    c.contorno(K)
    q.append(('mata_deco_samambaia', c))
    # cogumelo
    c = Canvas(16, 16)
    c.rect(7, 11, 2, 5, cor('#f4ecd8'))
    c.elipse(8, 10, 4, 2.5, cor('#e8452a'))
    c.set(6, 9, cor('#ffffff'))
    c.set(10, 10, cor('#ffffff'))
    c.contorno(K)
    q.append(('mata_deco_cogumelo', c))
    # pedra
    c = Canvas(16, 16)
    c.elipse(8, 13.5, 5, 3, cor('#9a928c'))
    c.elipse(7, 12.5, 3, 1.6, cor('#c4bcb4'))
    c.contorno(K)
    q.append(('mata_deco_pedra', c))
    # placa de aviso
    c = Canvas(16, 16)
    c.vline(8, 8, 15, cor('#7e5230'))
    c.rect(3, 3, 11, 6, cor('#d8a060'))
    c.hline(3, 13, 8, cor('#7e5230'))
    c.vline(8, 4, 6, cor('#c0392b'))
    c.set(8, 7, cor('#c0392b'))
    c.contorno(K)
    q.append(('mata_deco_placa', c))
    return q


def deco_queimada():
    q = []
    cinza = [cor('#9a9496'), cor('#6e6a6e'), cor('#4a4650'), cor('#2e2a34')]
    for v in range(2):
        c = Canvas(16, 16)
        rnd = random.Random(v * 7 + 3)
        for i in range(5):
            x = rnd.randint(2, 13)
            h = rnd.randint(2, 5)
            for j in range(h):
                c.set(x + (j // 2) * (1 if i % 2 else -1), 15 - j, cinza[2] if j < h - 1 else cinza[1])
        c.contorno(K)
        q.append(('queimada_deco_capim_%d' % v, c))
    # monte de cinzas com brasas
    c = Canvas(16, 16)
    c.elipse(8, 14.5, 6, 2.2, cinza[1])
    c.elipse(8, 13.8, 3.5, 1.4, cinza[0])
    c.set(6, 14, cor('#ff7a2a'))
    c.set(10, 15, cor('#ffb03a'))
    c.contorno(K)
    q.append(('queimada_deco_cinzas', c))
    # tocozinho carbonizado
    c = Canvas(16, 16)
    c.rect(5, 9, 6, 7, cor('#2e2424'))
    c.hline(5, 10, 9, cor('#4a3c38'))
    c.set(7, 11, cor('#ff7a2a'))
    c.set(9, 13, cor('#ec4a2a'))
    c.contorno(K)
    q.append(('queimada_deco_toco', c))
    # galho caído
    c = Canvas(16, 16)
    c.linha(2, 14, 13, 12, cor('#3a3034'))
    c.linha(2, 15, 13, 13, cor('#221a1e'))
    c.linha(9, 12, 11, 9, cor('#3a3034'))
    c.contorno(K)
    q.append(('queimada_deco_galho', c))
    # pedra chamuscada
    c = Canvas(16, 16)
    c.elipse(8, 13.5, 5, 3, cor('#55505a'))
    c.elipse(7, 12.5, 3, 1.6, cor('#7a7478'))
    c.contorno(K)
    q.append(('queimada_deco_pedra', c))
    return q


# ------------------------------------------------------------ árvores grandes
def arvore_ipe():
    """Ipê-amarelo: símbolo do Brasil (48x64)."""
    c = Canvas(48, 64)
    tronco = [cor('#6a4428'), cor('#4d2f25')]
    c.rect(21, 34, 6, 30, tronco[0])
    c.vline(21, 34, 63, tronco[1])
    c.linha(24, 40, 14, 28, tronco[0])
    c.linha(24, 38, 34, 26, tronco[0])
    c.linha(24, 37, 35, 25, tronco[0])
    amarelo = [cor('#fff08a'), cor('#ffd23a'), cor('#e8a020'), cor('#b87010')]
    rnd = random.Random(5)
    for (cx, cy, r) in ((24, 18, 13), (12, 24, 9), (36, 23, 10), (18, 12, 8), (31, 11, 8), (24, 28, 9)):
        def f(x, y, nx, ny, r=r):
            v = -ny * 0.7 - nx * 0.3 + rnd.random() * 0.35
            if v > 0.55:
                return amarelo[0]
            if v > 0.05:
                return amarelo[1]
            if v > -0.45:
                return amarelo[2]
            return amarelo[3]
        c.elipse_fn(cx, cy, r, r * 0.8, f)
    c.contorno(K)
    return c


def arvore_palmeira():
    """Palmeira juçara (32x64)."""
    c = Canvas(32, 64)
    for y in range(18, 64):
        x = 15 + int(math.sin(y / 9.0) * 1.5)
        c.hline(x, x + 2, y, cor('#8a6a48') if y % 4 else cor('#6a4e36'))
    verde = [cor('#a6e66c'), cor('#58bd48'), cor('#2e7a3a')]
    for ang in (-170, -140, -110, -70, -40, -10, 200):
        a = math.radians(ang)
        for r in range(15):
            x = 16 + math.cos(a) * r
            y = 17 + math.sin(a) * r * 0.55 + (r * r) / 30.0
            c.set(x, y, verde[1])
            c.set(x, y + 1, verde[2])
            if r % 3 == 0:
                c.set(x, y - 1, verde[0])
    c.elipse(16, 18, 2, 2, cor('#6a2a7a'))  # cacho de açaí
    c.contorno(K)
    return c


def arvore_copa():
    """Árvore da mata com copa verde (48x64)."""
    c = Canvas(48, 64)
    c.rect(20, 32, 8, 32, cor('#5a3826'))
    c.vline(20, 32, 63, cor('#3a2418'))
    c.vline(25, 34, 63, cor('#7a5236'))
    c.linha(24, 44, 12, 34, cor('#5a3826'))
    verde = [cor('#a6e66c'), cor('#58bd48'), cor('#3f9a3e'), cor('#2a6e34')]
    rnd = random.Random(11)
    for (cx, cy, r) in ((24, 20, 15), (10, 28, 9), (38, 27, 9), (24, 9, 9), (14, 14, 8), (34, 14, 8)):
        def f(x, y, nx, ny):
            v = -ny * 0.7 - nx * 0.3 + rnd.random() * 0.3
            if v > 0.55:
                return verde[0]
            if v > 0.0:
                return verde[1]
            if v > -0.45:
                return verde[2]
            return verde[3]
        c.elipse_fn(cx, cy, r, r * 0.82, f)
    c.contorno(K)
    return c


def arvore_bananeira():
    """Bananeira com cacho (32x48)."""
    c = Canvas(32, 48)
    c.rect(14, 20, 4, 28, cor('#8aa84a'))
    c.vline(14, 20, 47, cor('#5a7a34'))
    verde = [cor('#b4ec6a'), cor('#6cc94a'), cor('#3f9a3e')]
    for (ang, comp) in ((-160, 14), (-120, 12), (-60, 12), (-20, 14), (-90, 10)):
        a = math.radians(ang)
        for r in range(comp):
            x = 16 + math.cos(a) * r
            y = 19 + math.sin(a) * r * 0.7 + (r * r) / 40.0
            for w in range(-1, 2):
                c.set(x, y + w, verde[1] if w == 0 else verde[2])
            c.set(x, y - 2, verde[0] if r % 2 else None)
    for i in range(4):
        c.rect(18, 22 + i * 2, 3, 2, cor('#ffe066') if i % 2 == 0 else cor('#e8c040'))
    c.contorno(K)
    return c


def arvore_queimada(v):
    """Árvore queimada, com brasas nos galhos (48x64)."""
    c = Canvas(48, 64)
    carvao = [cor('#3a3034'), cor('#221a1e'), cor('#4e4448')]
    c.rect(20, 18, 7, 46, carvao[0])
    c.vline(20, 18, 63, carvao[1])
    c.vline(24, 20, 63, carvao[2])
    galhos = [((23, 30), (10, 18)), ((23, 26), (37, 12)), ((12, 21), (8, 10)), ((33, 16), (40, 8)),
              ((23, 20), (22, 4)), ((23, 40), (36, 32))]
    if v == 1:
        galhos = galhos[:4]
    for (a, b) in galhos:
        c.linha(a[0], a[1], b[0], b[1], carvao[0])
        c.linha(a[0] + 1, a[1], b[0] + 1, b[1], carvao[1])
    for (x, y) in ((22, 34), (25, 46), (21, 52), (11, 19), (36, 14)):
        c.set(x, y, cor('#ff7a2a'))
        c.set(x + 1, y, cor('#ec4a2a'))
    c.contorno(K)
    return c


def toco_grande():
    c = Canvas(32, 24)
    c.rect(8, 8, 16, 16, cor('#3a3034'))
    c.elipse(16, 8, 8, 2.5, cor('#5e4a44'))
    c.elipse(16, 8, 5, 1.5, cor('#7a5a4a'))
    c.linha(8, 22, 3, 23, cor('#3a3034'))
    c.linha(23, 22, 28, 23, cor('#3a3034'))
    c.set(13, 14, cor('#ff7a2a'))
    c.set(19, 18, cor('#ffb03a'))
    c.contorno(K)
    return c


def arbusto():
    c = Canvas(32, 16)
    verde = [cor('#a6e66c'), cor('#58bd48'), cor('#3f9a3e')]
    for (cx, cy, r) in ((9, 11, 6), (17, 9, 7), (25, 11, 6)):
        def f(x, y, nx, ny):
            return verde[0] if ny < -0.4 else (verde[1] if ny < 0.35 else verde[2])
        c.elipse_fn(cx, cy, r, r * 0.8, f)
    c.rect(4, 13, 24, 3, verde[2])
    c.set(12, 7, cor('#ff5a6a'))
    c.set(21, 6, cor('#ffd84a'))
    c.contorno(K)
    return c


def quadros_arvores():
    return [
        ('arvore_ipe', arvore_ipe()),
        ('arvore_copa', arvore_copa()),
        ('arvore_palmeira', arvore_palmeira()),
        ('arvore_bananeira', arvore_bananeira()),
        ('arvore_queimada_0', arvore_queimada(0)),
        ('arvore_queimada_1', arvore_queimada(1)),
        ('toco_grande', toco_grande()),
        ('arbusto', arbusto()),
    ]


# ------------------------------------------------------------ fundos (parallax)
L, A = 512, 256


def _gradiente(c, cores, y0=0, y1=A):
    n = len(cores) - 1
    for y in range(y0, y1):
        t = (y - y0) / max(1, (y1 - y0 - 1))
        i = min(n - 1, int(t * n))
        lt = t * n - i
        cc = misturar(cores[i], cores[i + 1], lt)
        # leve dithering entre faixas
        for x in range(L):
            c.set(x, y, cc)


def _silhueta(c, seed, base, amplitude, cor_corpo, cor_borda=None, oitavas=3, picos=False):
    f = ruido_periodico(seed, L, oitavas)
    for x in range(L):
        v = f(x)
        if picos:
            v = abs(v - 0.5) * 2
        topo = int(base - v * amplitude)
        for y in range(max(0, topo), A):
            c.set(x, y, cor_corpo)
        if cor_borda is not None and 0 <= topo < A:
            c.set(x, topo, cor_borda)
            if 0 <= topo + 1 < A and (x % 3):
                c.set(x, topo + 1, cor_borda)


def fundo_mata_ceu():
    c = Canvas(L, A)
    _gradiente(c, [cor('#3b3a78'), cor('#7a4a8c'), cor('#d0607a'), cor('#f59a58'), cor('#ffd58a')], 0, A)
    # sol grande no horizonte
    for y in range(A):
        for x in range(L):
            d = math.hypot(x - 360, y - 170)
            if d < 30:
                c.set(x, y, cor('#fff4c0') if d < 26 else cor('#ffe6a0'))
            elif d < 44:
                c.set(x, y, cor('#fff4c0', int(90 * (1 - (d - 30) / 14))))
    return c


def fundo_mata_nuvens():
    c = Canvas(L, A)
    rnd = random.Random(21)
    for i in range(9):
        cx = rnd.randint(0, L)
        cy = rnd.randint(30, 130)
        larg = rnd.randint(30, 60)
        for k in range(5):
            ox = cx + (k - 2) * larg / 4 + rnd.randint(-3, 3)
            oy = cy + rnd.randint(-3, 3)
            r = larg / 4 + rnd.randint(0, 6)
            for dx in (-L, 0, L):
                c.elipse(ox + dx, oy, r, r * 0.55, cor('#ffc9a8', 235))
                c.elipse(ox + dx, oy - 2, r * 0.8, r * 0.35, cor('#ffe4cc', 235))
        for dx in (-L, 0, L):
            c.rect(cx - larg / 2 + dx, cy + 2, larg, 3, cor('#e89a8a', 235))
    return c


def fundo_mata_serra():
    c = Canvas(L, A)
    _silhueta(c, 3, 170, 70, cor('#6a4c8c'), cor('#8a6aa8'), 3, picos=True)
    return c


def fundo_mata_morros():
    c = Canvas(L, A)
    _silhueta(c, 7, 200, 45, cor('#3d5e6e'), cor('#557a86'), 3)
    # silhuetas de araucárias e palmeiras
    rnd = random.Random(9)
    for i in range(14):
        x0 = rnd.randint(0, L)
        f = ruido_periodico(7, L, 3)
        topo = int(200 - f(x0) * 45)
        h = rnd.randint(22, 40)
        cc = cor('#34525f')
        for dx in (-L, 0, L):
            x = x0 + dx
            c.rect(x, topo - h, 2, h, cc)
            if i % 2 == 0:  # araucária (copa em "taça")
                for k in range(3):
                    y = topo - h + k * 5
                    c.hline(x - 8 + k * 2, x + 9 - k * 2, y, cc)
                    c.hline(x - 7 + k * 2, x + 8 - k * 2, y + 1, cc)
            else:  # palmeira
                for ang in (-150, -110, -70, -30):
                    a = math.radians(ang)
                    for r in range(10):
                        c.set(x + math.cos(a) * r, topo - h + math.sin(a) * r * 0.5 + r * r / 25.0, cc)
    return c


def fundo_mata_floresta():
    c = Canvas(L, A)
    f = ruido_periodico(13, L, 4)
    verde = [cor('#2c5a3a'), cor('#3a7046'), cor('#224a30')]
    for x in range(L):
        topo = int(212 - f(x) * 34)
        for y in range(max(0, topo), A):
            c.set(x, y, verde[2] if y > topo + 3 else verde[0])
        c.set(x, topo, verde[1])
    # copas redondas por cima
    rnd = random.Random(17)
    for i in range(22):
        cx = rnd.randint(0, L)
        topo = int(212 - f(cx) * 34)
        r = rnd.randint(9, 16)
        for dx in (-L, 0, L):
            c.elipse(cx + dx, topo + 4, r, r * 0.75, verde[0])
            c.elipse(cx + dx - r * 0.3, topo + 1, r * 0.5, r * 0.3, verde[1])
    # troncos
    for i in range(18):
        x = rnd.randint(0, L)
        for dx in (-L, 0, L):
            c.rect(x + dx, 222, 3, 34, cor('#1c3a26'))
    return c


def fundo_queimada_ceu():
    c = Canvas(L, A)
    _gradiente(c, [cor('#070a1c'), cor('#141a42'), cor('#262456'), cor('#4a2a4a'), cor('#7a3a3a')], 0, A)
    rnd = random.Random(31)
    for i in range(170):
        x, y = rnd.randint(0, L - 1), rnd.randint(0, 170)
        brilho = rnd.random()
        cc = cor('#ffffff') if brilho > 0.8 else (cor('#c8d0ff') if brilho > 0.4 else cor('#8890c0'))
        c.set(x, y, cc)
        if brilho > 0.93:
            c.set(x + 1, y, cor('#8890c0'))
            c.set(x - 1, y, cor('#8890c0'))
            c.set(x, y + 1, cor('#8890c0'))
            c.set(x, y - 1, cor('#8890c0'))
    # lua cheia com crateras
    for y in range(A):
        for x in range(L):
            d = math.hypot(x - 130, y - 60)
            if d < 22:
                cc = cor('#f4f0d6')
                if math.hypot(x - 124, y - 55) < 5 or math.hypot(x - 138, y - 66) < 4 or math.hypot(x - 133, y - 50) < 2.5:
                    cc = cor('#d8d2b0')
                c.set(x, y, cc)
            elif d < 34:
                c.set(x, y, cor('#f4f0d6', int(70 * (1 - (d - 22) / 12))))
    return c


def fundo_queimada_fumaca():
    c = Canvas(L, A)
    rnd = random.Random(41)
    for i in range(10):
        cx = rnd.randint(0, L)
        cy = rnd.randint(60, 170)
        larg = rnd.randint(40, 80)
        for k in range(6):
            ox = cx + (k - 3) * larg / 5 + rnd.randint(-4, 4)
            oy = cy + rnd.randint(-5, 5)
            r = larg / 5 + rnd.randint(0, 8)
            for dx in (-L, 0, L):
                c.elipse(ox + dx, oy, r, r * 0.5, cor('#5a5068', 110))
    return c


def fundo_queimada_serra():
    c = Canvas(L, A)
    _silhueta(c, 5, 180, 90, cor('#1c2440'), cor('#2e3a5e'), 3, picos=True)
    return c


def fundo_queimada_troncos():
    c = Canvas(L, A)
    _silhueta(c, 19, 215, 30, cor('#141826'), cor('#241e2e'), 2)
    rnd = random.Random(23)
    for i in range(20):
        x0 = rnd.randint(0, L)
        h = rnd.randint(50, 110)
        for dx in (-L, 0, L):
            x = x0 + dx
            c.rect(x, 215 - h, 3, h + 41, cor('#141826'))
            for k in range(2):
                gy = 215 - h + 10 + k * 18
                lado = 1 if (i + k) % 2 else -1
                c.linha(x + 1, gy, x + 1 + lado * 12, gy - 10, cor('#141826'))
    # brilho alaranjado de brasas no chão
    for x in range(L):
        for y in range(226, A):
            if rnd.random() < 0.012:
                c.set(x, y, cor('#ff7a2a'))
    return c


def fundo_queimada_perto():
    c = Canvas(L, A)
    f = ruido_periodico(29, L, 3)
    for x in range(L):
        topo = int(222 - f(x) * 26)
        for y in range(max(0, topo), A):
            c.set(x, y, cor('#0e0c14'))
    rnd = random.Random(37)
    for i in range(9):
        x0 = rnd.randint(0, L)
        for dx in (-L, 0, L):
            x = x0 + dx
            c.rect(x, 190, 8, 70, cor('#0e0c14'))
            c.linha(x + 7, 196, x + 16, 186, cor('#0e0c14'))
            c.set(x + 3, 200, cor('#ec4a2a'))
    return c


def fundos():
    return [
        ('fundo_mata_ceu', fundo_mata_ceu()),
        ('fundo_mata_nuvens', fundo_mata_nuvens()),
        ('fundo_mata_serra', fundo_mata_serra()),
        ('fundo_mata_morros', fundo_mata_morros()),
        ('fundo_mata_floresta', fundo_mata_floresta()),
        ('fundo_queimada_ceu', fundo_queimada_ceu()),
        ('fundo_queimada_fumaca', fundo_queimada_fumaca()),
        ('fundo_queimada_serra', fundo_queimada_serra()),
        ('fundo_queimada_troncos', fundo_queimada_troncos()),
        ('fundo_queimada_perto', fundo_queimada_perto()),
    ]
