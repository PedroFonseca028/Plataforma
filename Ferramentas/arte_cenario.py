# -*- coding: utf-8 -*-
"""
Cenário do jogo "Rumo ao Hexa": tiles (com auto-tiling de 16 combinações),
plataformas, perigos, mar/fosso animados, decorações e as camadas de fundo
(parallax) da praia e do estádio.
"""
import math
import random
from pixelart import Canvas, cor, misturar, com_alpha, ruido_periodico
from arte_itens import micro_texto, largura_micro

K = cor('#1e1622')
BRANCO = cor('#ffffff')
VERDE = cor('#1f9d48')
AMARELO = cor('#ffd21f')
AZUL = cor('#2557c9')
CELESTE = cor('#62b8ec')

CIMA, DIREITA, BAIXO, ESQUERDA = 1, 2, 4, 8

AREIA = [cor('#fff0c4'), cor('#f6dd9c'), cor('#e6c27c'), cor('#c99c58'), cor('#9a7038')]
GRAMA = [cor('#9be86a'), cor('#5cc84a'), cor('#3a9a3a'), cor('#257a32')]
TERRA = [cor('#6a4a30'), cor('#563c28'), cor('#46301f'), cor('#2e2016')]
CONCRETO = [cor('#d4d8e2'), cor('#a8aebd'), cor('#848a9a'), cor('#5e6474')]


def _bordas(c, mascara, escuro, mais_escuro):
    aberto_dir = not (mascara & DIREITA)
    aberto_baixo = not (mascara & BAIXO)
    aberto_esq = not (mascara & ESQUERDA)
    if aberto_baixo:
        for x in range(16):
            c.set(x, 13, escuro)
            c.set(x, 14, mais_escuro)
            c.set(x, 15, K)
    if aberto_esq:
        for y in range(16):
            c.set(1, y, escuro)
            c.set(0, y, K)
    if aberto_dir:
        for y in range(16):
            c.set(14, y, escuro)
            c.set(15, y, K)
    if aberto_baixo and aberto_esq:
        c.set(0, 15, None)
        c.set(1, 14, K)
    if aberto_baixo and aberto_dir:
        c.set(15, 15, None)
        c.set(14, 14, K)


def tile_areia(mascara, variante=0):
    rnd = random.Random(mascara * 31 + variante * 977 + 5)
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            v = rnd.random()
            cc = AREIA[2]
            if v < 0.12:
                cc = AREIA[3]
            elif v < 0.30:
                cc = AREIA[1]
            c.set(x, y, cc)
    # conchinhas e pedrinhas enterradas
    for _ in range(variante * 2):
        px, py = rnd.randint(3, 11), rnd.randint(7, 12)
        if variante == 1:
            c.set(px, py, cor('#ffd0d8'))
            c.set(px + 1, py, cor('#f4a8b8'))
        else:
            c.rect(px, py, 2, 1, cor('#b8b0a8'))
    _bordas(c, mascara, AREIA[3], AREIA[4])
    if not (mascara & CIMA):
        # areia seca e clarinha por cima, com borda ondulada
        for x in range(16):
            fundo = 3 + (1 if (x + mascara) % 5 == 0 else 0) + (1 if x % 6 == 2 else 0)
            for y in range(0, fundo + 1):
                cc = AREIA[1] if y > 0 else AREIA[0]
                if y == fundo:
                    cc = AREIA[2]
                c.set(x, y, cc)
        for x in range(0, 16, 4):
            c.set(x + (mascara % 3), 1, AREIA[0])
        c.hline(0, 15, 0, AREIA[0])
        if not (mascara & ESQUERDA):
            for y in range(0, 5):
                c.set(0, y, K)
            c.set(0, 0, None)
        if not (mascara & DIREITA):
            for y in range(0, 5):
                c.set(15, y, K)
            c.set(15, 0, None)
    return c


def tile_gramado(mascara, variante=0):
    rnd = random.Random(mascara * 17 + variante * 571 + 9)
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            v = rnd.random()
            cc = TERRA[1]
            if v < 0.10:
                cc = TERRA[2]
            elif v < 0.17:
                cc = TERRA[0]
            c.set(x, y, cc)
    for _ in range(1 + variante):
        px, py = rnd.randint(2, 12), rnd.randint(6, 13)
        c.rect(px, py, 2, 2, TERRA[3])
    _bordas(c, mascara, TERRA[2], TERRA[3])
    if not (mascara & CIMA):
        for x in range(16):
            faixa = GRAMA[1] if x < 8 else GRAMA[2]   # listras do corte da grama
            fundo = 4 + (1 if (x * 7 + mascara) % 5 == 0 else 0)
            for y in range(0, fundo + 1):
                cc = faixa
                if y == 0:
                    cc = GRAMA[0] if x < 8 else GRAMA[1]
                if y == fundo:
                    cc = GRAMA[3]
                c.set(x, y, cc)
        for x in range(1, 16, 3):
            c.set(x, 0, None if (x + mascara) % 2 else c.get(x, 0))
        for x in range(16):
            if c.get(x, 0) is None:
                c.set(x, 1, GRAMA[0] if x < 8 else GRAMA[1])
        if not (mascara & ESQUERDA):
            for y in range(0, 6):
                c.set(0, y, K)
            c.set(0, 0, None)
        if not (mascara & DIREITA):
            for y in range(0, 6):
                c.set(15, y, K)
            c.set(15, 0, None)
    return c


ASSENTOS = [cor('#ffd21f'), cor('#1f9d48'), cor('#2557c9'), cor('#f4f4f4')]


def tile_degrau(mascara, variante=0):
    """Concreto da arquibancada, com cadeirinhas coloridas no topo."""
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            cc = CONCRETO[1]
            if y % 8 == 7:
                cc = CONCRETO[2]       # emenda dos degraus
            elif y % 8 == 0:
                cc = CONCRETO[0]
            c.set(x, y, cc)
    if variante:
        c.set(5 + variante, 10, CONCRETO[3])
        c.set(6 + variante, 11, CONCRETO[3])
    _bordas(c, mascara, CONCRETO[2], CONCRETO[3])
    if not (mascara & CIMA):
        # faixa amarela de segurança e fileira de cadeiras
        c.hline(0, 15, 5, CONCRETO[0])
        c.hline(0, 15, 6, AMARELO)
        for x in range(16):
            for y in range(0, 5):
                c.set(x, y, None)
        for i in range(4):
            cc = ASSENTOS[(i + mascara + variante) % 4]
            x0 = i * 4
            c.rect(x0, 1, 3, 3, cc)
            c.hline(x0, x0 + 2, 4, misturar(cc, K, 0.4))
            c.set(x0, 1, misturar(cc, BRANCO, 0.4))
        if not (mascara & ESQUERDA):
            for y in range(5, 8):
                c.set(0, y, K)
        if not (mascara & DIREITA):
            for y in range(5, 8):
                c.set(15, y, K)
        c.contorno(K)
    return c


def tiles_chao(tema):
    q = []
    if tema == 'praia':
        for m in range(16):
            q.append(('praia_chao_%d' % m, tile_areia(m, 0)))
        q.append(('praia_chao_15b', tile_areia(15, 1)))
        q.append(('praia_chao_15c', tile_areia(15, 2)))
    else:
        for m in range(16):
            q.append(('estadio_chao_%d' % m, tile_gramado(m, 0)))
        q.append(('estadio_chao_15b', tile_gramado(15, 1)))
        q.append(('estadio_chao_15c', tile_gramado(15, 2)))
        for m in range(16):
            q.append(('estadio_degrau_%d' % m, tile_degrau(m, 0)))
        q.append(('estadio_degrau_15b', tile_degrau(15, 1)))
        q.append(('estadio_degrau_15c', tile_degrau(15, 2)))
    return q


# ------------------------------------------------------------ plataformas
def tile_deck(tipo):
    """Deck de madeira da praia (atravessável por baixo). tipo: e, m, d, u."""
    mad = [cor('#e8c48c'), cor('#c89a5c'), cor('#94683a'), cor('#6a4626')]
    c = Canvas(16, 16)
    for y in range(0, 6):
        for x in range(16):
            cc = mad[1]
            if y == 0:
                cc = mad[0]
            if y >= 4:
                cc = mad[2]
            if (x + y * 5) % 11 == 0 and 0 < y < 4:
                cc = mad[2]
            c.set(x, y, cc)
    c.hline(0, 15, 6, K)
    if tipo == 'm':
        c.vline(8, 1, 4, mad[2])
    # amarração de corda
    if tipo in ('m', 'u'):
        for y in range(1, 5):
            c.set(4, y, cor('#efe2c0') if y % 2 else cor('#c8b48a'))
    if tipo in ('e', 'u'):
        for y in range(0, 7):
            c.set(0, y, K)
        c.vline(2, 7, 10, mad[3])
        c.set(2, 11, K)
    if tipo in ('d', 'u'):
        for y in range(0, 7):
            c.set(15, y, K)
        c.vline(13, 7, 10, mad[3])
        c.set(13, 11, K)
    return c


def tile_travessao(tipo):
    """Travessão (barra branca) com a rede pendurada embaixo."""
    c = Canvas(16, 16)
    branco = [cor('#ffffff'), cor('#e4e6ee'), cor('#b4b8c8')]
    for x in range(16):
        c.set(x, 0, K)
        c.set(x, 1, branco[0])
        c.set(x, 2, branco[1])
        c.set(x, 3, branco[2])
        c.set(x, 4, K)
    rede = cor('#ffffff', 110)
    for y in range(5, 14):
        for x in range(16):
            if (x + y) % 4 == 0 or (x - y) % 4 == 0:
                if y < 13 or x % 2 == 0:
                    c.set(x, y, rede)
    if tipo in ('e', 'u'):
        c.vline(0, 0, 4, K)
        c.set(1, 1, branco[1])
    if tipo in ('d', 'u'):
        c.vline(15, 0, 4, K)
        c.set(14, 3, branco[2])
    return c


def prancha():
    """Prancha de surfe (plataforma móvel da praia, 3 tiles de largura). A parte de cima fica no topo do tile."""
    c = Canvas(48, 16)
    for x in range(1, 47):
        t = abs(x - 23.5) / 23.0
        meia = 3.0 * (1 - t ** 3) ** 0.5
        for y in range(0, 7):
            if abs(y - 3.0) <= meia:
                cc = AMARELO
                if abs(x - 23.5) < 2:
                    cc = VERDE          # faixa central
                elif y >= 4:
                    cc = misturar(AMARELO, K, 0.25)
                c.set(x, y, cc)
    for x in range(6, 42, 7):
        c.set(x, 2, cor('#fff7a8'))
    # quilha
    c.rect(6, 7, 3, 2, cor('#2557c9'))
    c.contorno(K)
    return c


def tiles_plataforma(tema):
    fn = tile_deck if tema == 'praia' else tile_travessao
    return [('%s_plataforma_%s' % (tema, tp), fn(tp)) for tp in ('e', 'm', 'd', 'u')]


# ------------------------------------------------------------ perigos
def ourico(fase):
    """Ouriço-do-mar: bola roxa cheia de espinhos (2 quadros: os espinhos mexem)."""
    c = Canvas(16, 16)
    corpo = [cor('#5a2a6e'), cor('#3a1a4a'), cor('#8a4aa8')]
    for i in range(14):
        a = math.radians(-180 + i * (180 / 13.0) + (6 if fase else 0) * (1 if i % 2 else -1))
        comp = 6.5 if i % 2 == 0 else 5.0
        for r in range(3, int(comp) + 1):
            x = 8 + math.cos(a) * r
            y = 12.5 + math.sin(a) * r
            c.set(x, y, cor('#2a1030') if r < comp - 1 else cor('#d8b8f0'))
    c.elipse_fn(8, 12.5, 4.2, 3.4, lambda x, y, nx, ny: corpo[2] if nx < -0.2 and ny < -0.2 else
                (corpo[1] if nx + ny > 0.6 else corpo[0]))
    c.hline(3, 12, 15, corpo[1])
    c.set(6, 11, cor('#c890e8'))
    c.contorno(K)
    return c


def sinalizador(fase):
    """Dois sinalizadores acesos jogados no gramado (3 quadros de fogo)."""
    c = Canvas(16, 16)
    fogo = [cor('#ffffff'), cor('#fff6b8'), cor('#ffdc4a'), cor('#ff9e2c'), cor('#ec4a2a')]
    rnd = random.Random(fase * 13 + 1)
    for (bx, alt) in ((4, 9), (11, 11)):
        # bastão vermelho
        c.rect(bx - 1, 12, 3, 4, cor('#c0202e'))
        c.vline(bx - 1, 12, 15, cor('#ff5a5a'))
        c.hline(bx - 1, bx + 1, 12, cor('#2a2a30'))
        h = alt + (fase % 3) - 1
        for i in range(h):
            t = i / float(h)
            larg = max(0.6, 2.4 * (1 - t))
            centro = bx + math.sin(t * 5 + fase * 2.1) * t * 1.6
            for x in range(int(centro - larg - 1), int(centro + larg + 2)):
                d = abs(x + 0.5 - centro) / larg
                if d <= 1.0:
                    calor = (1 - d) * (1 - t * 0.7)
                    cc = fogo[0] if calor > 0.75 else (fogo[1] if calor > 0.55 else (fogo[2] if calor > 0.35 else (fogo[3] if calor > 0.15 else fogo[4])))
                    c.set(x, 11 - i, cc)
        # faíscas
        c.set(bx + rnd.randint(-3, 3), 11 - h - rnd.randint(0, 2), fogo[2])
    return c


def tiles_espinho(tema):
    if tema == 'praia':
        return [('praia_espinho_%d' % i, ourico(i)) for i in range(2)]
    return [('estadio_espinho_%d' % i, sinalizador(i)) for i in range(3)]


def tile_liquido(tema, topo, fase):
    """Mar da praia ou água do fosso do estádio (4 quadros)."""
    c = Canvas(16, 16)
    if tema == 'praia':
        cores = [cor('#f2fdff'), cor('#6ad8e8'), cor('#2aa8d0'), cor('#1878a8')]
        alpha = 230
    else:
        cores = [cor('#cfe9e0'), cor('#3e8a80'), cor('#24605e'), cor('#143c40')]
        alpha = 240
    for y in range(16):
        for x in range(16):
            onda = math.sin((x + fase * 4) * (2 * math.pi / 16)) * 1.2
            if topo:
                sup = 4 + onda
                if y < sup:
                    continue
                if y < sup + 1.3:
                    cc = cores[0]
                elif y < sup + 3:
                    cc = cores[1]
                elif y < 11:
                    cc = cores[2]
                else:
                    cc = cores[3]
            else:
                cc = cores[3] if (x + y + fase) % 7 else cores[2]
            c.set(x, y, com_alpha(cc, alpha))
    if topo:
        for (bx, by) in ((3, 7), (11, 8)):
            c.set((bx + fase * 2) % 16, by, com_alpha(cores[0], alpha))
        if tema == 'praia':
            # espuminha
            for x in range(0, 16, 5):
                c.set((x + fase * 3) % 16, 4, com_alpha(BRANCO, 200))
    return c


def tiles_liquido(tema):
    q = []
    for f in range(4):
        q.append(('%s_liquido_topo_%d' % (tema, f), tile_liquido(tema, True, f)))
    for f in range(4):
        q.append(('%s_liquido_fundo_%d' % (tema, f), tile_liquido(tema, False, f)))
    return q


# ------------------------------------------------------------ decorações pequenas (em cima do chão)
def deco_praia():
    q = []
    # concha
    c = Canvas(16, 16)
    for i in range(5):
        a = math.radians(-160 + i * 35)
        c.linha(8, 15, 8 + math.cos(a) * 4, 15 + math.sin(a) * 4, cor('#ffc8d4') if i % 2 else cor('#f49ab0'))
    c.elipse(8, 13.5, 3.5, 2, cor('#ffd8e0'))
    c.contorno(K)
    q.append(('praia_deco_concha', c))
    # estrela-do-mar
    c = Canvas(16, 16)
    pts = []
    for i in range(10):
        r = 4.6 if i % 2 == 0 else 2.0
        a = math.radians(-90 + i * 36)
        pts.append((8 + math.cos(a) * r, 12 + math.sin(a) * r * 0.75))
    c.poligono(pts, cor('#ff8a3a'))
    c.set(8, 11, cor('#ffc07a'))
    c.set(6, 12, cor('#ffc07a'))
    c.contorno(K)
    q.append(('praia_deco_estrelamar', c))
    # baldinho com pazinha
    c = Canvas(16, 16)
    c.poligono([(4, 9), (11, 9), (10, 15), (5, 15)], cor('#e8323e'))
    c.hline(4, 11, 9, cor('#ff7a7a'))
    c.linha(5, 9, 7, 6, K)
    c.linha(10, 9, 8, 6, K)
    c.linha(12, 15, 14, 8, cor('#ffd21f'))
    c.rect(13, 6, 2, 3, cor('#ffd21f'))
    c.contorno(K)
    q.append(('praia_deco_balde', c))
    # chinelos
    c = Canvas(16, 16)
    for (x0, cc) in ((2, cor('#1f9d48')), (8, cor('#ffd21f'))):
        c.rect(x0, 14, 6, 2, cc)
        c.set(x0, 14, None)
        c.set(x0 + 3, 13, cor('#2557c9'))
        c.set(x0 + 2, 12, cor('#2557c9'))
        c.set(x0 + 4, 12, cor('#2557c9'))
    c.contorno(K)
    q.append(('praia_deco_chinelo', c))
    # coco com canudinho
    c = Canvas(16, 16)
    c.elipse(8, 12, 4.4, 3.6, cor('#5cb83a'))
    c.elipse(7, 11, 2, 1.4, cor('#9be86a'))
    c.linha(9, 9, 12, 3, cor('#ff5a8a'))
    c.contorno(K)
    q.append(('praia_deco_coco', c))
    # capim da restinga
    c = Canvas(16, 16)
    rnd = random.Random(4)
    for i in range(6):
        x = rnd.randint(3, 12)
        h = rnd.randint(3, 7)
        for j in range(h):
            c.set(x + (j // 3) * (1 if i % 2 else -1), 15 - j, cor('#7ac24a') if j < h - 1 else cor('#b4ec6a'))
    c.contorno(K)
    q.append(('praia_deco_capim', c))
    # pedra
    c = Canvas(16, 16)
    c.elipse(8, 13.5, 5, 3, cor('#a8a098'))
    c.elipse(7, 12.5, 3, 1.6, cor('#d0c8c0'))
    c.contorno(K)
    q.append(('praia_deco_pedra', c))
    return q


def deco_estadio():
    q = []
    for v in range(2):
        c = Canvas(16, 16)
        rnd = random.Random(v * 13 + 1)
        for i in range(5 + v):
            x = rnd.randint(2, 13)
            h = rnd.randint(2, 4)
            for j in range(h):
                c.set(x, 15 - j, GRAMA[1] if j < h - 1 else GRAMA[0])
        q.append(('estadio_deco_tufo%s' % 'ab'[v], c))
    # cone de treino
    c = Canvas(16, 16)
    c.poligono([(8, 7), (11, 15), (5, 15)], cor('#ff7a1a'))
    c.hline(6, 10, 12, BRANCO)
    c.hline(4, 12, 15, cor('#d85a10'))
    c.contorno(K)
    q.append(('estadio_deco_cone', c))
    # garrafinha de água
    c = Canvas(16, 16)
    c.rect(7, 9, 3, 7, cor('#9ad8ff', 220))
    c.rect(7, 11, 3, 2, cor('#2557c9'))
    c.rect(7, 8, 3, 1, cor('#f4f4f4'))
    c.contorno(K)
    q.append(('estadio_deco_garrafa', c))
    # copinho amassado
    c = Canvas(16, 16)
    c.poligono([(5, 12), (10, 11), (11, 15), (6, 15)], BRANCO)
    c.hline(6, 10, 13, cor('#e8323e'))
    c.contorno(K)
    q.append(('estadio_deco_copo', c))
    # papel picado
    c = Canvas(16, 16)
    for (x, y, cc) in ((3, 14, AMARELO), (6, 15, VERDE), (9, 14, AZUL), (12, 15, BRANCO), (8, 13, AMARELO)):
        c.rect(x, y, 2, 1, cc)
    q.append(('estadio_deco_papel', c))
    return q


# ------------------------------------------------------------ decorações grandes (pivô embaixo)
def coqueiro():
    c = Canvas(32, 64)
    for y in range(16, 64):
        x = 14 + int(math.sin(y / 10.0) * 2)
        c.hline(x, x + 2, y, cor('#a07a50') if y % 4 else cor('#7a5a3a'))
    verde = [cor('#b4ec6a'), cor('#5cb83a'), cor('#2e7a3a')]
    for ang in (-175, -145, -115, -65, -35, -5, 200, -90):
        a = math.radians(ang)
        comp = 14 if ang != -90 else 8
        for r in range(comp):
            x = 16 + math.cos(a) * r
            y = 15 + math.sin(a) * r * 0.55 + (r * r) / 28.0
            c.set(x, y, verde[1])
            c.set(x, y + 1, verde[2])
            if r % 3 == 0:
                c.set(x, y - 1, verde[0])
    for (x, y) in ((14, 17), (17, 17), (15.5, 19)):
        c.elipse(x, y, 1.6, 1.6, cor('#6a8a2a'))
    c.contorno(K)
    return c


def guarda_sol():
    c = Canvas(32, 40)
    c.vline(16, 10, 39, cor('#e8e8f0'))
    c.vline(17, 10, 39, cor('#a8aebd'))
    cores = [AMARELO, VERDE]
    for y in range(4, 14):
        for x in range(2, 31):
            nx = (x + 0.5 - 16) / 14.5
            ny = (y + 0.5 - 13) / 9.5
            if nx * nx + ny * ny <= 1.0 and y < 13:
                gomo = int((math.atan2(ny, nx) + math.pi) / (math.pi / 6))
                cc = cores[gomo % 2]
                if y == 12:
                    cc = misturar(cc, K, 0.3)
                c.set(x, y, cc)
    for x in range(3, 30, 4):
        c.set(x, 13, AMARELO if (x // 4) % 2 else VERDE)
    c.contorno(K)
    return c


def trave(largura=48, altura=32, areia=False):
    """Trave de futebol vista de lado-frente, com rede."""
    c = Canvas(largura, altura)
    poste = [cor('#ffffff'), cor('#c9ccd8')]
    topo = 4
    rede = cor('#ffffff', 120)
    for y in range(topo + 2, altura):
        for x in range(4, largura - 4):
            if (x + y) % 4 == 0 or (x - y) % 4 == 0:
                c.set(x, y, rede)
    for y in range(topo, altura):
        c.set(2, y, poste[0]); c.set(3, y, poste[1])
        c.set(largura - 4, y, poste[0]); c.set(largura - 3, y, poste[1])
    c.hline(2, largura - 3, topo, poste[0])
    c.hline(2, largura - 3, topo + 1, poste[1])
    if areia:
        c.rect(0, altura - 2, 6, 2, cor('#e6c27c'))
        c.rect(largura - 6, altura - 2, 6, 2, cor('#e6c27c'))
    c.contorno(K)
    return c


def quiosque():
    c = Canvas(48, 48)
    # telhado de palha
    for y in range(4, 18):
        meia = 6 + (y - 4) * 1.4
        for x in range(int(24 - meia), int(24 + meia)):
            cc = cor('#d8b060') if (x + y) % 3 else cor('#b08a40')
            if y == 17:
                cc = cor('#8a6a30')
            c.set(x, y, cc)
    for x in range(6, 42, 3):
        c.vline(x, 18, 19, cor('#b08a40'))
    # postes e balcão
    c.rect(8, 18, 2, 30, cor('#8a6238'))
    c.rect(38, 18, 2, 30, cor('#8a6238'))
    c.rect(6, 32, 36, 16, cor('#3fa0d8'))
    c.hline(6, 41, 32, cor('#8ad0ff'))
    for x in range(6, 42, 6):
        c.vline(x, 33, 47, cor('#2a7ab0'))
    # placa "COCO"
    c.rect(16, 22, 16, 7, BRANCO)
    micro_texto(c, 17, 23, "COCO", cor('#1f9d48'))
    # cocos no balcão
    for x in (12, 17, 31, 36):
        c.elipse(x, 30, 2.4, 2, cor('#5cb83a'))
    c.contorno(K)
    return c


def castelo():
    c = Canvas(32, 24)
    areia = [cor('#f6dd9c'), cor('#e6c27c'), cor('#c99c58')]
    c.rect(4, 14, 24, 10, areia[1])
    c.rect(8, 8, 6, 6, areia[1])
    c.rect(18, 8, 6, 6, areia[1])
    c.rect(13, 4, 6, 10, areia[0])
    for x in (4, 8, 12, 19, 23, 27):
        c.set(x, 13, None)
    for x in (8, 10, 12, 18, 20, 22):
        c.set(x, 7, areia[1])
    c.vline(16, 0, 4, K)
    c.rect(17, 0, 4, 2, cor('#1f9d48'))
    c.set(18, 0, AMARELO)
    c.rect(15, 18, 3, 6, areia[2])
    c.contorno(K)
    return c


def placa():
    c = Canvas(48, 16)
    c.rect(0, 0, 48, 13, cor('#1a2a6c'))
    c.rect(2, 2, 44, 9, cor('#2557c9'))
    texto = "BRASIL HEXA"
    micro_texto(c, 24 - largura_micro(texto) // 2, 4, texto, AMARELO)
    c.vline(4, 13, 15, cor('#5e6474'))
    c.vline(43, 13, 15, cor('#5e6474'))
    c.contorno(K)
    return c


def banco_reservas():
    c = Canvas(48, 28)
    # cobertura de acrílico
    for y in range(2, 14):
        for x in range(2, 46):
            nx = (x + 0.5 - 24) / 22.0
            ny = (y + 0.5 - 14) / 12.0
            if nx * nx + ny * ny <= 1.0:
                c.set(x, y, cor('#bfe6ff', 120))
    c.hline(2, 45, 14, cor('#8a8f9e'))
    c.vline(3, 14, 27, cor('#5e6474'))
    c.vline(44, 14, 27, cor('#5e6474'))
    # cadeiras azuis
    for i in range(6):
        x0 = 6 + i * 6
        c.rect(x0, 17, 5, 5, AZUL)
        c.rect(x0, 22, 5, 2, cor('#173a8f'))
        c.vline(x0 + 2, 24, 27, cor('#5e6474'))
    c.contorno(K)
    return c


def camera_tv():
    c = Canvas(16, 32)
    c.rect(2, 8, 11, 7, cor('#2a2e3a'))
    c.rect(12, 9, 4, 5, cor('#444a5a'))
    c.set(15, 11, cor('#8ad0ff'))
    c.rect(4, 6, 5, 2, cor('#444a5a'))
    c.set(3, 9, cor('#ff3a3a'))
    for (x1, x0) in ((7, 2), (7, 13), (7, 7)):
        c.linha(7, 15, x0, 31, cor('#5e6474'))
    c.contorno(K)
    return c


def sacola_bolas():
    """Três bolas reservas empilhadas na beira do campo."""
    c = Canvas(16, 16)
    preto = cor('#24202c')
    for (x, y) in ((4.5, 12.5), (11.5, 12.5), (8, 7)):
        c.elipse_fn(x, y, 3.4, 3.4, lambda xx, yy, nx, ny: cor('#c8ccd8') if nx + ny > 0.7 else BRANCO)
        c.rect(int(x) - 1, int(y) - 1, 2, 2, preto)
        c.set(int(x) - 2, int(y) - 2, BRANCO)
    c.contorno(K)
    return c


def quadros_decoracao():
    return [
        ('praia_coqueiro', coqueiro()),
        ('praia_guarda_sol', guarda_sol()),
        ('praia_trave', trave(48, 32, areia=True)),
        ('praia_quiosque', quiosque()),
        ('praia_castelo', castelo()),
        ('estadio_trave', trave(48, 32)),
        ('estadio_placa', placa()),
        ('estadio_banco', banco_reservas()),
        ('estadio_camera', camera_tv()),
        ('estadio_sacola', sacola_bolas()),
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
        for x in range(L):
            c.set(x, y, cc)


def _nuvem(c, cx, cy, larg, rnd, cor_corpo, cor_sombra):
    for k in range(5):
        ox = cx + (k - 2) * larg / 4 + rnd.randint(-3, 3)
        oy = cy + rnd.randint(-3, 3)
        r = larg / 4 + rnd.randint(0, 6)
        for dx in (-L, 0, L):
            c.elipse(ox + dx, oy + 2, r, r * 0.5, cor_sombra)
            c.elipse(ox + dx, oy, r, r * 0.55, cor_corpo)


def fundo_praia_ceu():
    c = Canvas(L, A)
    _gradiente(c, [cor('#2a7ed6'), cor('#4ea2ea'), cor('#86c8f6'), cor('#c4e8ff'), cor('#e8f8ff')], 0, A)
    for y in range(A):
        for x in range(L):
            d = math.hypot(x - 410, y - 52)
            if d < 16:
                c.set(x, y, cor('#fffbe0'))
            elif d < 40:
                c.set(x, y, cor('#fff6c8', int(110 * (1 - (d - 16) / 24))))
    return c


def fundo_praia_nuvens():
    c = Canvas(L, A)
    rnd = random.Random(21)
    for i in range(7):
        _nuvem(c, rnd.randint(0, L), rnd.randint(8, 52), rnd.randint(34, 60), rnd,
               cor('#ffffff', 240), cor('#cfe4f6', 240))
    # gaivotas
    for i in range(7):
        x, y = rnd.randint(0, L - 1), rnd.randint(30, 120)
        g = cor('#3a4250')
        c.set(x - 2, y - 1, g); c.set(x - 1, y, g); c.set(x, y, g); c.set(x + 1, y, g); c.set(x + 2, y - 1, g)
    return c


def _morro(c, cx, base, largura, altura, corpo, luz, forma=2.0):
    """Morro arredondado (tipo Pão de Açúcar): perfil de domo íngreme."""
    for x in range(int(cx - largura), int(cx + largura) + 1):
        t = abs(x - cx) / float(largura)
        if t > 1:
            continue
        h = altura * (1 - t ** forma) ** 0.5
        topo = int(base - h)
        for y in range(topo, base + 40):
            xx = x % L
            cc = corpo
            if x < cx - largura * 0.15 and y < topo + 6:
                cc = luz
            c.set(xx, y, cc)


def fundo_praia_morros():
    c = Canvas(L, A)
    base = 160
    nevoa = cor('#8fb6c4')
    corpo = cor('#5e8c98')
    escuro = cor('#4a7480')
    luz = cor('#7aa6b0')
    # serra ao fundo (bem clarinha)
    f = ruido_periodico(5, L, 3)
    for x in range(L):
        topo = int(base - 18 - f(x) * 40)
        for y in range(topo, base + 40):
            c.set(x, y, nevoa)
    # Corcovado com o Cristo
    _morro(c, 330, base, 70, 92, escuro, corpo, 1.3)
    cx, topo = 330, base - 92
    c.rect(cx - 1, topo - 9, 3, 9, cor('#e8eef2'))       # corpo do Cristo
    c.hline(cx - 5, cx + 5, topo - 7, cor('#e8eef2'))     # braços abertos
    c.set(cx, topo - 10, cor('#e8eef2'))
    c.rect(cx - 2, topo - 1, 5, 2, cor('#c8d0d8'))
    # Dois Irmãos
    _morro(c, 455, base, 26, 58, corpo, luz, 1.6)
    _morro(c, 488, base, 22, 46, escuro, corpo, 1.6)
    # Morro da Urca e Pão de Açúcar
    _morro(c, 72, base, 34, 44, corpo, luz, 2.2)
    _morro(c, 128, base, 30, 86, escuro, corpo, 3.0)
    # bondinho (cabos e cabines)
    x0, y0, x1, y1 = 82, base - 44, 124, base - 85
    c.linha(x0, y0, x1, y1, cor('#2a3a44'))
    for t in (0.35, 0.7):
        bx = int(x0 + (x1 - x0) * t)
        by = int(y0 + (y1 - y0) * t)
        c.rect(bx - 1, by + 1, 3, 2, cor('#e8323e'))
    return c


def fundo_praia_mar():
    c = Canvas(L, A)
    horizonte = 158
    _gradiente(c, [cor('#1f6fae'), cor('#2a90c0'), cor('#3fb6c8'), cor('#6ad8d8')], horizonte, A)
    rnd = random.Random(33)
    # ondinhas
    for i in range(380):
        y = rnd.randint(horizonte + 2, A - 1)
        x = rnd.randint(0, L - 1)
        comp = 2 + (y - horizonte) // 18
        for k in range(comp):
            c.set((x + k) % L, y, cor('#b8ecf4', 200))
    c.hline(0, L - 1, horizonte, cor('#a8d8ec'))
    # veleiros no horizonte
    for x in (60, 200, 300, 470):
        c.vline(x, horizonte - 9, horizonte - 1, cor('#5a5048'))
        c.poligono([(x + 1, horizonte - 9), (x + 6, horizonte - 2), (x + 1, horizonte - 2)], BRANCO)
        c.hline(x - 3, x + 5, horizonte, cor('#5a5048'))
    # ilhota
    c.elipse(250, horizonte + 1, 16, 5, cor('#4a7480'))
    return c


def fundo_praia_orla():
    """Faixa de areia ao fundo com guarda-sóis, barracas e coqueiros."""
    c = Canvas(L, A)
    topo_areia = 200
    rnd = random.Random(47)
    for y in range(topo_areia, A):
        for x in range(L):
            c.set(x, y, AREIA[1] if (x * 7 + y * 3) % 23 else AREIA[2])
    c.hline(0, L - 1, topo_areia, AREIA[0])
    # espuma da beira do mar
    for x in range(L):
        if (x // 6) % 3 != 0:
            c.set(x, topo_areia - 1, cor('#f2fdff', 220))
    cores = [cor('#e8323e'), AMARELO, VERDE, AZUL, cor('#ff8a3a'), BRANCO]
    # coqueiros
    for i in range(6):
        x0 = rnd.randint(0, L)
        for dx in (-L, 0, L):
            x = x0 + dx
            for y in range(112, topo_areia + 4):
                c.set(x + int(math.sin(y / 9.0) * 2), y, cor('#7a5a3a'))
            for ang in (-170, -140, -110, -70, -40, -10):
                a = math.radians(ang)
                for r in range(12):
                    c.set(x + math.cos(a) * r, 112 + math.sin(a) * r * 0.5 + r * r / 26.0, cor('#3a8a3a'))
    # guarda-sóis e barracas
    for i in range(16):
        x0 = rnd.randint(0, L)
        cc = cores[i % len(cores)]
        for dx in (-L, 0, L):
            x = x0 + dx
            alto = 56 + (i % 3) * 4
            c.vline(x, topo_areia - alto + 6, topo_areia + 3, cor('#e8e8f0'))
            for y in range(topo_areia - alto, topo_areia - alto + 7):
                meia = (y - (topo_areia - alto - 1)) * 1.4
                c.hline(x - meia, x + meia, y, cc if (y % 2) else misturar(cc, BRANCO, 0.3))
    # cadeirinhas e banhistas (pontinhos)
    for i in range(30):
        x = rnd.randint(0, L - 1)
        c.rect(x, topo_areia + 4, 2, 2, cores[rnd.randint(0, len(cores) - 1)])
    return c


def fundo_estadio_ceu():
    c = Canvas(L, A)
    _gradiente(c, [cor('#05081a'), cor('#0c1236'), cor('#1a2052'), cor('#2e2c66'), cor('#4a3a72')], 0, A)
    rnd = random.Random(31)
    for i in range(140):
        x, y = rnd.randint(0, L - 1), rnd.randint(0, 140)
        b = rnd.random()
        c.set(x, y, cor('#ffffff') if b > 0.8 else (cor('#c8d0ff') if b > 0.4 else cor('#7880b0')))
    # fogos de artifício
    for (fx, fy, r, cc) in ((90, 50, 16, AMARELO), (260, 34, 12, cor('#5fd47e')), (420, 60, 18, cor('#8ad0ff'))):
        for k in range(16):
            a = k * math.pi / 8
            for d in (r * 0.5, r * 0.75, r):
                c.set(fx + math.cos(a) * d, fy + math.sin(a) * d, com_alpha(cc, 255 if d == r else 170))
        c.set(fx, fy, BRANCO)
    return c


def fundo_estadio_refletores():
    c = Canvas(L, A)
    for tx in (70, 250, 430):
        # feixe de luz
        for y in range(46, 230):
            meia = (y - 46) * 0.55
            for x in range(int(tx - meia), int(tx + meia)):
                if (x + y) % 2 == 0:
                    c.set(x % L, y, cor('#fff6c8', 26))
        # torre treliçada
        for y in range(40, 236):
            c.set(tx - 3, y, cor('#3a3e52'))
            c.set(tx + 3, y, cor('#3a3e52'))
            if y % 8 == 0:
                c.linha(tx - 3, y, tx + 3, y + 8, cor('#2a2e40'))
        # painel de lâmpadas com brilho
        for y in range(18, 46):
            for x in range(tx - 24, tx + 25):
                d = math.hypot(x - tx, (y - 32) * 1.6)
                if d < 40:
                    c.set(x % L, y, cor('#fff6c8', int(70 * (1 - d / 40))))
        c.rect(tx - 14, 24, 29, 14, cor('#2a2e40'))
        for j in range(3):
            for i in range(6):
                c.rect(tx - 12 + i * 5, 26 + j * 4, 3, 2, cor('#fffbe0'))
    return c


def _torcida(fase):
    """Arquibancada lotada (fase 0/1: a torcida pula)."""
    c = Canvas(L, A)
    rnd = random.Random(77)
    topo, base = 92, 218
    # marquise (cobertura em anel)
    for y in range(78, topo):
        for x in range(L):
            c.set(x, y, cor('#d4d8e2') if y < 82 else cor('#8a90a2'))
    c.hline(0, L - 1, topo, K)
    # concreto da arquibancada
    for y in range(topo + 1, base):
        for x in range(L):
            c.set(x, y, cor('#2a2e44') if (y // 6) % 2 else cor('#24283c'))
    # torcedores (cabeça + camisa), em fileiras
    paleta_br = [AMARELO, AMARELO, VERDE, AZUL, BRANCO, cor('#ffe680')]
    paleta_ar = [CELESTE, BRANCO, CELESTE, cor('#1d2a5e')]
    peles = [cor('#b9794f'), cor('#f0c49c'), cor('#d9a074'), cor('#8c5536')]
    for fila, y in enumerate(range(topo + 4, base - 4, 6)):
        for x in range(1, L - 1, 3):
            argentino = 300 <= x < 360
            pal = paleta_ar if argentino else paleta_br
            camisa = pal[rnd.randint(0, len(pal) - 1)]
            pele = peles[rnd.randint(0, len(peles) - 1)]
            pula = (fase == 1 and (x // 3 + fila) % 3 == 0 and not argentino)
            dy = -1 if pula else 0
            c.rect(x, y + 2 + dy, 2, 2, camisa)
            c.set(x, y + dy, pele)
            c.set(x + 1, y + dy, pele)
            if pula and (x // 3) % 2 == 0:
                c.set(x - 1, y - 1 + dy, pele)   # braço levantado
    # bandeirões
    for (bx, by, larg, alt) in ((30, 120, 40, 26), (170, 140, 46, 30), (420, 118, 44, 28)):
        onda = 1 if fase else 0
        for y in range(by, by + alt):
            for x in range(bx, bx + larg):
                yy = y + (onda if (x // 8) % 2 else 0)
                cc = VERDE
                dx = abs(x - (bx + larg / 2.0)) / (larg / 2.0)
                dy = abs(y - (by + alt / 2.0)) / (alt / 2.0)
                if dx + dy < 0.9:
                    cc = AMARELO
                if dx * dx + dy * dy < 0.16:
                    cc = AZUL
                c.set(x, yy, cc)
    # faixa "HEXA"
    c.rect(250, 104, 40, 9, BRANCO)
    micro_texto(c, 256, 106, "HEXA!", VERDE)
    # a torcida fica na sombra da marquise: escurece para destacar o campo
    c.tingir(lambda p, x, y: (int(p[0] * 0.72), int(p[1] * 0.72), int(p[2] * 0.78), p[3]) if y > 92 else p)
    return c


def fundo_estadio_torcida(fase):
    return _torcida(fase)


def fundo_estadio_placas():
    """Placas de publicidade na beira do campo (camada mais próxima)."""
    c = Canvas(L, A)
    topo, alt = 134, 16
    # muro do fosso
    for y in range(topo + alt, A):
        for x in range(L):
            c.set(x, y, CONCRETO[2] if (y + x // 16) % 5 else CONCRETO[3])
    textos = ["BRASIL", "HEXA", "GOL!", "TORCIDA", "CRAQUE", "VAI BRASIL!"]
    cores = [(cor('#1f9d48'), AMARELO), (cor('#2557c9'), BRANCO), (cor('#ffd21f'), cor('#1f9d48')),
             (cor('#e8323e'), BRANCO)]
    x = 0
    i = 0
    while x < L:
        larg = 84
        fundo, letra = cores[i % len(cores)]
        larg = min(larg, L - x)
        c.rect(x, topo, larg, alt, fundo)
        c.hline(x, x + larg - 1, topo, misturar(fundo, BRANCO, 0.4))
        c.vline(x, topo, topo + alt - 1, K)
        t = textos[i % len(textos)]
        if larg > largura_micro(t) + 6:
            micro_texto(c, x + (larg - largura_micro(t)) // 2, topo + 6, t, letra)
        x += larg
        i += 1
    c.hline(0, L - 1, topo + alt, K)
    return c


def fundos():
    return [
        ('fundo_praia_ceu', fundo_praia_ceu()),
        ('fundo_praia_nuvens', fundo_praia_nuvens()),
        ('fundo_praia_morros', fundo_praia_morros()),
        ('fundo_praia_mar', fundo_praia_mar()),
        ('fundo_praia_orla', fundo_praia_orla()),
        ('fundo_estadio_ceu', fundo_estadio_ceu()),
        ('fundo_estadio_refletores', fundo_estadio_refletores()),
        ('fundo_estadio_torcida_0', fundo_estadio_torcida(0)),
        ('fundo_estadio_torcida_1', fundo_estadio_torcida(1)),
        ('fundo_estadio_placas', fundo_estadio_placas()),
    ]
