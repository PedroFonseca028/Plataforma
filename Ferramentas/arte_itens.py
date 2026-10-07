# -*- coding: utf-8 -*-
"""
Itens, bola, bandeirinha, placar, efeitos visuais e ícones de interface
do jogo "Rumo ao Hexa".
"""
import math
from pixelart import Canvas, cor, misturar, com_alpha

K = cor('#1e1622')
BRANCO = cor('#ffffff')
OURO = [cor('#fff7c2'), cor('#ffe066'), cor('#f5b62a'), cor('#c77a16'), cor('#7a4410')]
VERDE = cor('#1f9d48')
VERDE_S = cor('#13703a')
VERDE_H = cor('#5fd47e')
AMARELO = cor('#ffd21f')
AZUL = cor('#2557c9')
AZUL_S = cor('#173a8f')
CELESTE = cor('#62b8ec')

# ------------------------------------------------------------ micro fonte (3x5) para placas e placar
MICRO = {
    'A': ["###", "#.#", "###", "#.#", "#.#"], 'B': ["##.", "#.#", "##.", "#.#", "##."],
    'C': ["###", "#..", "#..", "#..", "###"], 'D': ["##.", "#.#", "#.#", "#.#", "##."],
    'E': ["###", "#..", "##.", "#..", "###"], 'F': ["###", "#..", "##.", "#..", "#.."],
    'G': ["###", "#..", "#.#", "#.#", "###"], 'H': ["#.#", "#.#", "###", "#.#", "#.#"],
    'I': ["###", ".#.", ".#.", ".#.", "###"], 'L': ["#..", "#..", "#..", "#..", "###"],
    'M': ["#.#", "###", "###", "#.#", "#.#"], 'N': ["##.", "#.#", "#.#", "#.#", "#.#"],
    'O': ["###", "#.#", "#.#", "#.#", "###"], 'P': ["###", "#.#", "###", "#..", "#.."],
    'Q': ["###", "#.#", "#.#", "##.", ".##"],
    'R': ["##.", "#.#", "##.", "#.#", "#.#"], 'S': ["###", "#..", "###", "..#", "###"],
    'T': ["###", ".#.", ".#.", ".#.", ".#."], 'U': ["#.#", "#.#", "#.#", "#.#", "###"],
    'V': ["#.#", "#.#", "#.#", "#.#", ".#."], 'X': ["#.#", "#.#", ".#.", "#.#", "#.#"],
    '!': [".#.", ".#.", ".#.", "...", ".#."], ' ': ["...", "...", "...", "...", "..."],
    '6': ["###", "#..", "###", "#.#", "###"], '0': ["###", "#.#", "#.#", "#.#", "###"],
}


def micro_texto(c, x, y, texto, cc):
    """Escreve com a micro fonte 3x5 (1 px de espaço). Devolve a largura."""
    faltando = [ch for ch in texto if ch not in MICRO]
    if faltando:
        raise ValueError("micro fonte sem as letras %r (texto %r)" % (faltando, texto))
    x0 = x
    for ch in texto:
        g = MICRO.get(ch, MICRO[' '])
        for j, linha in enumerate(g):
            for i, p in enumerate(linha):
                if p == '#':
                    c.set(x + i, y + j, cc)
        x += 4
    return x - x0 - 1


def largura_micro(texto):
    return len(texto) * 4 - 1


# ------------------------------------------------------------ moeda
def moeda(fase):
    """Moeda de ouro girando (6 quadros)."""
    c = Canvas(16, 16)
    larguras = [5.6, 4.6, 2.8, 1.0, 2.8, 4.6]
    rx = larguras[fase % 6]
    cx, cy, ry = 8.0, 8.0, 5.6

    def f(x, y, nx, ny):
        borda = nx * nx + ny * ny > 0.62
        if borda:
            return OURO[3] if nx > 0.2 or ny > 0.3 else OURO[2]
        v = -nx * 0.6 - ny * 0.5
        return OURO[0] if v > 0.45 else (OURO[1] if v > -0.2 else OURO[2])

    c.elipse_fn(cx, cy, rx, ry, f)
    if rx > 3:
        # estrelinha gravada no meio
        c.set(8, 6, OURO[3]); c.hline(7, 9, 7, OURO[3]); c.set(7, 8, OURO[3]); c.set(9, 8, OURO[3])
    if fase % 6 == 3:
        c.vline(8, 3, 12, OURO[3])
    c.contorno(K)
    if fase % 6 == 0:
        c.set(5, 4, BRANCO)
    return c


# ------------------------------------------------------------ estrela da camisa
def _estrela_poligono(cx, cy, r_ext, r_int, rot=-90):
    pts = []
    for i in range(10):
        r = r_ext if i % 2 == 0 else r_int
        a = math.radians(rot + i * 36)
        pts.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    return pts


def estrela(fase):
    """Estrela dourada da camisa (6 quadros, com um brilho que passa)."""
    c = Canvas(16, 16)
    pts = _estrela_poligono(8.0, 8.6, 7.4, 3.1)
    c.poligono(pts, OURO[1])
    # sombreado: metade de baixo/direita mais escura
    for y in range(16):
        for x in range(16):
            px = c.get(x, y)
            if px is None:
                continue
            if x + y > 18:
                c.set(x, y, OURO[2])
            if x + y < 11:
                c.set(x, y, OURO[0])
    # brilho que percorre a estrela
    faixa = [2, 6, 10, 14, 18, 40][fase % 6]
    for y in range(16):
        for x in range(16):
            if c.get(x, y) is not None and abs((x + y) - faixa) <= 0:
                c.set(x, y, BRANCO)
    c.contorno(K)
    faiscas = [(1, 2), (14, 1), (15, 12), (1, 13), (8, 0), (14, 6)]
    px, py = faiscas[fase % len(faiscas)]
    c.set(px, py, OURO[0])
    return c


# ------------------------------------------------------------ letras H E X A
LETRAS_5x7 = {
    'H': ["#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
    'E': ["#####", "#....", "#....", "####.", "#....", "#....", "#####"],
    'X': ["#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#"],
    'A': [".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
}


def letra(ch, fase):
    """Medalha verde com a letra amarela (4 quadros: o brilho gira em volta)."""
    c = Canvas(16, 16)

    def f(x, y, nx, ny):
        d = nx * nx + ny * ny
        if d > 0.72:
            return AMARELO
        if d > 0.55:
            return AZUL
        return VERDE if nx + ny > -0.6 else VERDE_H

    c.elipse_fn(8, 8, 7.4, 7.4, f)
    g = LETRAS_5x7[ch]
    pix = set((6 + i, 5 + j) for j, l in enumerate(g) for i, p in enumerate(l) if p == '#')
    for (x, y) in pix:
        if (x + 1, y + 1) not in pix:
            c.set(x + 1, y + 1, VERDE_S)   # sombra da letra
    for (x, y) in pix:
        c.set(x, y, AMARELO)
    # brilho girando na borda
    a = math.radians(-135 + fase * 90)
    bx, by = 8 + math.cos(a) * 6.2, 8 + math.sin(a) * 6.2
    c.set(bx, by, BRANCO)
    c.set(bx + 1, by, cor('#fff7c2'))
    c.contorno(K)
    return c


# ------------------------------------------------------------ coração
def coracao(fase, tamanho=None):
    c = Canvas(16, 16)
    esc = tamanho if tamanho is not None else [1.0, 1.08, 1.16, 1.08][fase % 4]
    cx, cy = 8.0, 8.5
    for y in range(16):
        for x in range(16):
            px = (x + 0.5 - cx) / (5.4 * esc)
            py = -(y + 0.5 - cy) / (5.4 * esc)
            v = (px * px + py * py - 0.55) ** 3 - px * px * (py ** 3) * 1.2
            if v <= 0:
                cc = cor('#e8323e')
                if px < -0.25 and py > 0.15:
                    cc = cor('#ff8a8a')
                elif px + (-py) > 0.55:
                    cc = cor('#9a1a2e')
                c.set(x, y, cc)
    c.set(5, 5, BRANCO)
    c.contorno(K)
    return c


# ------------------------------------------------------------ bandeirinha de escanteio (checkpoint)
def bandeirinha(estado, fase=0):
    c = Canvas(16, 32)
    # mastro branco com listras
    for y in range(4, 32):
        cc = cor('#f4f4f4') if (y // 3) % 2 == 0 else cor('#c9c9d6')
        c.set(4, y, cc)
        c.set(5, y, cor('#c9c9d6'))
    c.rect(3, 30, 4, 2, cor('#8a8a96'))
    c.set(4, 3, cor('#ffe066'))
    if estado == 'desligada':
        # pano cinza pendurado
        for y in range(4, 13):
            larg = 3 if y < 11 else 2
            c.hline(6, 6 + larg, y, cor('#8a8894') if y % 3 else cor('#6e6c78'))
    else:
        # bandeira verde e amarela tremulando
        for x in range(6, 16):
            onda = int(round(math.sin((x - 6) * 0.9 + fase * 2.2) * 1.2))
            for y in range(4, 11):
                yy = y + onda
                cc = VERDE
                dx = abs(x - 10.5) / 4.5
                dy = abs(y - 7.0) / 3.0
                if dx + dy < 0.85:
                    cc = AMARELO
                if dx * dx + dy * dy < 0.12:
                    cc = AZUL
                c.set(x, yy, cc)
    c.contorno(K)
    return c


# ------------------------------------------------------------ bola
def bola():
    """Bola de futebol 8x8 (meio tile)."""
    c = Canvas(8, 8)
    preto = cor('#24202c')
    cinza = cor('#c8ccd8')
    c.elipse_fn(4, 4, 3.8, 3.8, lambda x, y, nx, ny: cinza if nx + ny > 0.7 else BRANCO)
    # pentágono central e gomos
    c.rect(3, 3, 2, 2, preto)
    c.set(4, 2, preto)
    for (x, y) in ((1, 1), (6, 1), (0, 5), (7, 4), (4, 7), (3, 7)):
        c.set(x, y, preto)
    c.set(2, 2, cor('#ffffff'))
    c.contorno(K)
    return c


def bola_ui():
    """Bola grande para o HUD (16x16)."""
    c = Canvas(16, 16)
    preto = cor('#24202c')
    cinza = cor('#c8ccd8')
    c.elipse_fn(8, 8, 7, 7, lambda x, y, nx, ny: cinza if nx + ny > 0.8 else BRANCO)
    c.poligono([(8, 5), (11, 7.2), (10, 10.6), (6, 10.6), (5, 7.2)], preto)
    for pts in ([(8, 1), (10, 1.5), (8, 3)], [(1.5, 6), (3, 4.5), (3, 8)], [(14.5, 6), (13, 4.5), (13, 8)],
                [(4, 13.5), (6, 12.5), (6, 15)], [(12, 13.5), (10, 12.5), (10, 15)]):
        c.poligono(pts, preto)
    c.set(5, 3, BRANCO)
    c.contorno(K)
    return c


def rastro():
    c = Canvas(4, 4)
    c.rect(1, 0, 2, 4, BRANCO)
    c.rect(0, 1, 4, 2, BRANCO)
    return c


def confete():
    c = Canvas(3, 2)
    c.rect(0, 0, 3, 2, BRANCO)
    return c


# ------------------------------------------------------------ cartão vermelho
def cartao():
    c = Canvas(8, 11)
    c.rect(1, 0, 6, 9, cor('#e8232e'))
    c.vline(1, 0, 8, cor('#ff6a6a'))
    c.vline(6, 1, 8, cor('#a3121e'))
    c.hline(1, 6, 8, cor('#a3121e'))
    c.contorno(K)
    return c


# ------------------------------------------------------------ taça
def taca():
    """Taça dourada genérica (16x24, pivô embaixo)."""
    c = Canvas(16, 26)

    def ouro(x, y, nx, ny):
        v = -nx * 0.7 - ny * 0.3
        return OURO[0] if v > 0.5 else (OURO[1] if v > -0.1 else (OURO[2] if v > -0.6 else OURO[3]))

    c.elipse_fn(8, 7, 6, 6.5, ouro)
    c.rect(2, 1, 13, 3, None)
    c.hline(2, 13, 1, OURO[1])
    c.hline(2, 13, 2, OURO[0])
    # alças
    for x0 in (1, 14):
        c.vline(x0, 3, 9, OURO[2])
        c.set(x0, 10, OURO[3])
    # haste e base verde
    c.rect(7, 13, 2, 6, OURO[2])
    c.set(7, 13, OURO[1])
    c.rect(5, 18, 6, 2, OURO[1])
    c.rect(3, 20, 10, 5, VERDE)
    c.hline(3, 12, 20, VERDE_H)
    c.hline(3, 12, 24, VERDE_S)
    c.hline(4, 11, 22, AMARELO)
    # estrelinha gravada
    c.set(8, 6, OURO[3]); c.hline(7, 9, 7, OURO[3]); c.set(7, 8, OURO[3]); c.set(9, 8, OURO[3])
    c.set(4, 5, BRANCO)
    c.contorno(K)
    return c


# ------------------------------------------------------------ placar do estádio
def placar():
    """Telão do placar (96x40). Os números são desenhados pelo jogo com os sprites led_N."""
    c = Canvas(96, 40)
    moldura = [cor('#3a3e4c'), cor('#5c6276'), cor('#262832')]
    c.rect(0, 0, 96, 40, moldura[0])
    c.hline(0, 95, 0, moldura[1])
    c.hline(0, 95, 39, moldura[2])
    c.rect(3, 3, 90, 34, cor('#0a0c14'))
    # pontinhos de LED apagados
    for y in range(4, 36, 2):
        for x in range(4, 92, 2):
            c.set(x, y, cor('#141826'))
    # bandeirinhas e siglas
    # Brasil
    c.rect(7, 6, 10, 7, VERDE)
    c.poligono([(12, 6.6), (16.4, 9.5), (12, 12.4), (7.6, 9.5)], AMARELO)
    c.rect(11, 8, 2, 3, AZUL)
    micro_texto(c, 20, 7, "BRA", AMARELO)
    # Argentina
    c.rect(79, 6, 10, 7, CELESTE)
    c.hline(79, 88, 8, BRANCO)
    c.hline(79, 88, 9, BRANCO)
    c.hline(79, 88, 10, BRANCO)
    c.set(84, 9, cor('#ffd21f'))
    micro_texto(c, 65, 7, "ARG", cor('#a8dcff'))
    # "X" no meio
    for i in range(5):
        c.set(46 + i, 22 + i, cor('#ff9e2c'))
        c.set(50 - i, 22 + i, cor('#ff9e2c'))
    c.contorno(K)
    return c


LED = {
    '0': ["#####", "#...#", "#...#", "#...#", "#...#", "#...#", "#####"],
    '1': ["..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###."],
    '2': ["#####", "....#", "....#", "#####", "#....", "#....", "#####"],
    '3': ["#####", "....#", "....#", ".####", "....#", "....#", "#####"],
    '4': ["#...#", "#...#", "#...#", "#####", "....#", "....#", "....#"],
    '5': ["#####", "#....", "#....", "#####", "....#", "....#", "#####"],
    '6': ["#####", "#....", "#....", "#####", "#...#", "#...#", "#####"],
    '7': ["#####", "....#", "...#.", "..#..", "..#..", "..#..", "..#.."],
    '8': ["#####", "#...#", "#...#", "#####", "#...#", "#...#", "#####"],
    '9': ["#####", "#...#", "#...#", "#####", "....#", "....#", "#####"],
}


def led(digito):
    c = Canvas(5, 7)
    for j, linha in enumerate(LED[digito]):
        for i, p in enumerate(linha):
            if p == '#':
                c.set(i, j, cor('#ffb03a'))
    return c


# ------------------------------------------------------------ efeitos
def brilho(fase):
    c = Canvas(16, 16)
    r = [2, 4, 6, 3][fase % 4]
    col = [BRANCO, cor('#fff6b8'), cor('#ffe066'), cor('#ffe066')][fase % 4]
    for i in range(-r, r + 1):
        a = 255 if abs(i) < r else 160
        c.set(8 + i, 8, com_alpha(col, a))
        c.set(8, 8 + i, com_alpha(col, a))
    if fase in (1, 2):
        for d in (1, 2):
            for (sx, sy) in ((1, 1), (-1, -1), (1, -1), (-1, 1)):
                c.set(8 + d * sx, 8 + d * sy, com_alpha(col, 180))
    return c


def poeira(fase):
    """Nuvenzinha de areia/poeira levantada pelos pés."""
    c = Canvas(16, 16)
    r = [2.0, 3.2, 4.0, 4.4][fase % 4]
    a = [230, 200, 140, 70][fase % 4]
    base = cor('#f0e2c0', a)
    c.elipse(8, 11, r, r * 0.7, base)
    c.elipse(5, 12, r * 0.6, r * 0.45, base)
    c.elipse(11, 12, r * 0.6, r * 0.45, base)
    return c


def fumaca(fase):
    c = Canvas(16, 16)
    r = [2.5, 3.5, 4.5, 5.0][fase % 4]
    a = [210, 170, 120, 60][fase % 4]
    c.elipse(8, 9, r, r * 0.85, cor('#d8d6e0', a))
    c.elipse(7, 8, r * 0.6, r * 0.5, cor('#ffffff', a))
    return c


def tontura(fase):
    """Estrelinhas girando em volta da cabeça de quem está tonto."""
    c = Canvas(8, 8)
    amarelo = cor('#ffe066')
    if fase == 0:
        c.set(3, 1, amarelo)
        c.hline(1, 5, 3, amarelo)
        c.set(2, 4, amarelo)
        c.set(4, 4, amarelo)
        c.set(3, 2, amarelo)
        c.set(3, 3, BRANCO)
        c.set(1, 5, amarelo)
        c.set(5, 5, amarelo)
    else:
        c.set(3, 2, amarelo)
        c.hline(2, 4, 3, amarelo)
        c.set(3, 4, amarelo)
        c.set(3, 3, BRANCO)
    c.contorno(K)
    return c


def seta():
    """Seta que aponta para o item obrigatório mais próximo."""
    c = Canvas(16, 16)
    c.poligono([(3, 6), (9, 6), (9, 3), (14, 8), (9, 13), (9, 10), (3, 10)], AMARELO)
    c.hline(4, 9, 7, cor('#fff7c2'))
    c.contorno(K)
    return c


def anel():
    """Anel branco (onda de choque da folha seca)."""
    c = Canvas(64, 64)
    for y in range(64):
        for x in range(64):
            d = math.hypot(x + 0.5 - 32, y + 0.5 - 32)
            if 27.5 <= d <= 31.5:
                a = 255 if 28.5 <= d <= 30.5 else 120
                c.set(x, y, cor('#ffffff', a))
            elif 24 <= d < 27.5:
                c.set(x, y, cor('#ffffff', int(60 * (d - 24) / 3.5)))
    return c


def luz_suave():
    c = Canvas(32, 32)
    for y in range(32):
        for x in range(32):
            d = math.hypot(x + 0.5 - 16, y + 0.5 - 16) / 16.0
            if d < 1:
                a = int(255 * (1 - d) ** 2)
                if a > 0:
                    c.set(x, y, cor('#ffffff', a))
    return c


# ------------------------------------------------------------ interface
def ui_coracao(cheio):
    if cheio:
        return coracao(0, 1.0)
    c = Canvas(16, 16)
    base = coracao(0, 1.0)
    for y in range(16):
        for x in range(16):
            px = base.get(x, y)
            if px is None:
                continue
            c.set(x, y, cor('#1a1420') if px == K else cor('#4a3a4a'))
    return c


def ui_botao():
    """Botão verde com borda amarela (9-slice, borda de 6 px)."""
    c = Canvas(48, 16)
    for y in range(16):
        for x in range(48):
            cc = VERDE
            if y in (1, 2):
                cc = VERDE_H
            if y in (12, 13):
                cc = VERDE_S
            c.set(x, y, cc)
    for x in range(48):
        c.set(x, 0, K)
        c.set(x, 15, K)
        c.set(x, 14, cor('#0c4a24'))
    for y in range(16):
        c.set(0, y, K)
        c.set(47, y, K)
    for x in range(2, 46):
        c.set(x, 3 if x % 2 else 3, com_alpha(AMARELO, 120))
    for (x, y) in ((3, 4), (44, 4), (3, 10), (44, 10)):
        c.set(x, y, AMARELO)
    for (x, y) in ((0, 0), (47, 0), (0, 15), (47, 15)):
        c.set(x, y, None)
    return c


def ui_painel():
    """Painel azul-escuro com moldura verde e amarela (9-slice, borda de 12 px)."""
    c = Canvas(48, 48)
    fundo = cor('#101a3c', 238)
    for y in range(48):
        for x in range(48):
            c.set(x, y, fundo)
    camadas = [K, VERDE_S, VERDE, AMARELO, VERDE_S]
    for i, cc in enumerate(camadas):
        for x in range(i, 48 - i):
            c.set(x, i, cc)
            c.set(x, 47 - i, cc)
        for y in range(i, 48 - i):
            c.set(i, y, cc)
            c.set(47 - i, y, cc)
    # estrelinhas nos cantos
    for (x, y) in ((2, 2), (45, 2), (2, 45), (45, 45)):
        c.set(x, y, BRANCO)
    return c


def ui_tecla():
    c = Canvas(16, 16)
    for y in range(16):
        for x in range(16):
            cc = cor('#e8e4dc')
            if y >= 12:
                cc = cor('#9a948c')
            if y <= 1:
                cc = cor('#ffffff')
            c.set(x, y, cc)
    for x in range(16):
        c.set(x, 0, K)
        c.set(x, 15, K)
    for y in range(16):
        c.set(0, y, K)
        c.set(15, y, K)
    for (x, y) in ((0, 0), (15, 0), (0, 15), (15, 15)):
        c.set(x, y, None)
    return c


def ui_branco():
    c = Canvas(4, 4)
    c.rect(0, 0, 4, 4, BRANCO)
    return c


# ------------------------------------------------------------ listas
def quadros_itens():
    q = []
    for i in range(6):
        q.append(('moeda_%d' % i, moeda(i)))
    for i in range(6):
        q.append(('estrela_%d' % i, estrela(i)))
    for ch in 'HEXA':
        for i in range(4):
            q.append(('letra_%s_%d' % (ch.lower(), i), letra(ch, i)))
    for i in range(4):
        q.append(('coracao_%d' % i, coracao(i)))
    for i in range(4):
        q.append(('brilho_%d' % i, brilho(i)))
    for i in range(4):
        q.append(('poeira_%d' % i, poeira(i)))
    for i in range(4):
        q.append(('fumaca_%d' % i, fumaca(i)))
    q.append(('seta', seta()))
    q.append(('bola_ui', bola_ui()))
    return q


def quadros_pequenos():
    q = [('bola', bola()), ('rastro', rastro()), ('confete', confete()), ('cartao', cartao())]
    for i in range(2):
        q.append(('tontura_%d' % i, tontura(i)))
    for d in '0123456789':
        q.append(('led_%s' % d, led(d)))
    return q


def quadros_bandeirinha():
    return [('bandeirinha_desligada', bandeirinha('desligada')),
            ('bandeirinha_ligada_0', bandeirinha('ligada', 0)),
            ('bandeirinha_ligada_1', bandeirinha('ligada', 1)),
            ('bandeirinha_ligada_2', bandeirinha('ligada', 2))]
