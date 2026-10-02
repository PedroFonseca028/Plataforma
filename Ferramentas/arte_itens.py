# -*- coding: utf-8 -*-
"""
Itens colecionáveis, checkpoints, efeitos visuais e ícones de interface.
"""
import math
from pixelart import Canvas, cor, misturar, com_alpha

K = cor('#2a1a22')
BRANCO = cor('#ffffff')
OURO = [cor('#fff7c2'), cor('#ffe066'), cor('#f5b62a'), cor('#c77a16'), cor('#7a4410')]
VERDE = cor('#58bd48')
VERDE_S = cor('#2e7a3a')
VERDE_H = cor('#a6e66c')
MARROM = cor('#8a5a3c')
MARROM_S = cor('#5a3826')


# ------------------------------------------------------------ semente
def semente(fase):
    """Semente Encantada: dourada, com um brotinho e um brilho que percorre."""
    c = Canvas(16, 16)
    cx, cy = 8.0, 9.5

    def f(x, y, nx, ny):
        v = -nx * 0.5 - ny * 0.6
        if v > 0.55:
            return OURO[0]
        if v > 0.15:
            return OURO[1]
        if v > -0.35:
            return OURO[2]
        return OURO[3]

    c.elipse_fn(cx, cy, 4.2, 5.0, f)
    # ponta da semente
    c.set(8, 4, OURO[2])
    c.set(7, 4, OURO[1])
    # brotinho
    c.set(8, 3, VERDE_S)
    c.set(9, 2, VERDE)
    c.set(10, 2, VERDE_H)
    c.set(7, 2, VERDE)
    c.set(6, 1, VERDE_H)
    # listra central
    for y in range(7, 13):
        c.set(8 if y < 11 else 9, y, OURO[3] if y % 2 else OURO[2])
    # brilho que passa (diagonal)
    faixa = [3, 5, 7, 9, 11, 20][fase % 6]
    for y in range(16):
        for x in range(16):
            if c.opaco(x, y) and abs((x + y) - faixa - 6) <= 0 and c.get(x, y) not in (VERDE, VERDE_S, VERDE_H):
                c.set(x, y, BRANCO)
    c.contorno(K)
    # faíscas ao redor
    pontos = [(2, 5), (13, 4), (13, 12), (2, 12), (8, 0), (14, 8)]
    px, py = pontos[fase % len(pontos)]
    c.set(px, py, OURO[0])
    return c


# ------------------------------------------------------------ frutas
def acai(fase):
    c = Canvas(16, 16)
    roxo = [cor('#b27ad6'), cor('#6e3a96'), cor('#4a2468'), cor('#2e1442')]
    bagas = [(6, 10), (10, 10), (8, 7), (8, 12), (5, 7), (11, 7)]
    for (bx, by) in bagas:
        def f(x, y, nx, ny):
            if nx < -0.2 and ny < -0.2:
                return roxo[0]
            if nx + ny > 0.6:
                return roxo[3]
            return roxo[1] if nx + ny < 0.1 else roxo[2]
        c.elipse_fn(bx, by, 2.2, 2.2, f)
    # cabinho
    c.linha(8, 5, 9, 2, MARROM)
    c.set(10, 2, VERDE)
    c.set(11, 1, VERDE_H)
    brilho = [(5, 6), (9, 6), (7, 9), (4, 6)][fase % 4]
    c.set(brilho[0], brilho[1], BRANCO)
    c.contorno(K)
    return c


def caju(fase):
    c = Canvas(16, 16)
    # pedúnculo (a "fruta" do caju): amarelo -> vermelho
    def f(x, y, nx, ny):
        t = (ny + 1) / 2
        base = misturar(cor('#ffd84a'), cor('#e8452a'), min(1, max(0, t * 1.2 - 0.1 + nx * 0.25)))
        if nx < -0.4 and ny < 0:
            return misturar(base, BRANCO, 0.35)
        return base
    c.elipse_fn(8, 7.5, 4.3, 4.8, f)
    c.elipse_fn(8, 4.5, 3.2, 2.5, f)
    # castanha pendurada
    c.elipse(8.5, 13.2, 2.4, 1.7, cor('#9a8a6a'))
    c.set(7, 13, cor('#c4b48e'))
    c.set(10, 14, cor('#6a5a42'))
    # folhinha
    c.set(8, 1, VERDE_S)
    c.set(9, 0, VERDE)
    brilho = [(6, 5), (6, 6), (7, 5), (6, 4)][fase % 4]
    c.set(brilho[0], brilho[1], BRANCO)
    c.contorno(K)
    return c


def guarana(fase):
    """Guaraná: a casca vermelha se abre e mostra o 'olho' (lenda indígena)."""
    c = Canvas(16, 16)
    frutos = [(5.5, 9.5), (10.5, 9.5), (8, 5.5)]
    for i, (fx, fy) in enumerate(frutos):
        def f(x, y, nx, ny):
            if nx < -0.3 and ny < -0.3:
                return cor('#ff7a5a')
            if nx + ny > 0.7:
                return cor('#8a1a22')
            return cor('#d8322e')
        c.elipse_fn(fx, fy, 3.0, 3.0, f)
        # parte branca + semente preta ("olho")
        c.elipse(fx, fy + 0.3, 1.8, 1.6, cor('#f7f3e8'))
        c.set(int(fx), int(fy + 0.3), cor('#1a1420'))
        if (i + fase) % 4 == 0:
            c.set(int(fx) - 1, int(fy) - 1, BRANCO)
    c.set(8, 1, VERDE_S)
    c.set(9, 1, VERDE)
    c.set(10, 0, VERDE_H)
    c.contorno(K)
    return c


def coracao(fase, tamanho=None):
    """Coração da Mata (vida extra): coração vermelho com uma folha."""
    c = Canvas(16, 16)
    esc = tamanho if tamanho is not None else [1.0, 1.08, 1.16, 1.08][fase % 4]
    cx, cy = 8.0, 8.5
    for y in range(16):
        for x in range(16):
            px = (x + 0.5 - cx) / (5.2 * esc)
            py = (y + 0.5 - cy) / (5.2 * esc)
            py = -py
            # equação clássica do coração
            v = (px * px + py * py - 0.55) ** 3 - px * px * (py ** 3) * 1.2
            if v <= 0:
                cc = cor('#e8323e')
                if px < -0.25 and py > 0.15:
                    cc = cor('#ff8a8a')
                elif px + (-py) > 0.55:
                    cc = cor('#9a1a2e')
                c.set(x, y, cc)
    c.set(5, 5, BRANCO)
    # folhinha no topo
    c.set(9, 3, VERDE_S)
    c.set(10, 2, VERDE)
    c.set(11, 2, VERDE_H)
    c.set(11, 1, VERDE)
    c.contorno(K)
    return c


# ------------------------------------------------------------ broto
def broto(fase):
    """Broto que nasce onde a semente foi recuperada (5 estágios)."""
    c = Canvas(16, 16)
    # montinho de terra
    c.elipse(8, 15, 4, 1.6, MARROM)
    c.hline(5, 11, 14, cor('#a8744a'))
    if fase >= 1:
        c.vline(8, 15 - (2 + fase * 2), 14, VERDE_S)
    if fase >= 2:
        c.set(9, 12, VERDE)
        c.set(10, 11, VERDE_H)
        c.set(7, 13, VERDE)
    if fase >= 3:
        c.set(6, 10, VERDE)
        c.set(5, 9, VERDE_H)
        c.set(7, 10, VERDE)
        c.set(10, 8, VERDE)
        c.set(11, 7, VERDE_H)
        c.set(9, 8, VERDE)
    if fase >= 4:
        # florzinha amarela de ipê
        topo = 15 - (2 + fase * 2)
        for (dx, dy) in ((0, -1), (-1, 0), (1, 0), (0, 1)):
            c.set(8 + dx, topo + dy, cor('#ffd84a'))
        c.set(8, topo, cor('#f08a2a'))
    c.contorno(K)
    return c


# ------------------------------------------------------------ totem
def totem(estado, fase=0):
    """Totem de madeira: checkpoint. Acende os olhos quando ativado."""
    c = Canvas(16, 32)
    madeira = cor('#9a6a40')
    madeira_s = cor('#6a4428')
    madeira_h = cor('#c8925a')
    c.rect(4, 8, 8, 24, madeira)
    c.vline(4, 8, 31, madeira_s)
    c.vline(11, 8, 31, madeira_s)
    c.vline(6, 9, 30, madeira_h)
    # entalhes (faixas)
    for y in (15, 23):
        c.hline(4, 11, y, madeira_s)
    # asas/folhas no topo
    c.rect(1, 9, 3, 2, VERDE_S)
    c.rect(12, 9, 3, 2, VERDE_S)
    c.set(0, 8, VERDE)
    c.set(15, 8, VERDE)
    # rosto de cima
    aceso = estado == 'ligado'
    olho = cor('#ffe066') if aceso else cor('#3a2a1a')
    c.rect(5, 11, 2, 2, olho)
    c.rect(9, 11, 2, 2, olho)
    c.hline(6, 9, 14, madeira_s)
    # rosto de baixo
    c.rect(5, 18, 2, 1, olho if aceso else madeira_s)
    c.rect(9, 18, 2, 1, olho if aceso else madeira_s)
    c.rect(6, 20, 4, 2, madeira_s)
    # chama no topo quando aceso
    if aceso:
        alt = 6 + (fase % 2)
        for i in range(alt):
            larg = max(0, 2 - i // 2)
            cc = [cor('#fff6b8'), cor('#ffdc4a'), cor('#ff9e2c'), cor('#ec4a2a')][min(3, i // 2)]
            for dx in range(-larg, larg + 1):
                c.set(8 + dx + (1 if (i > 3 and fase % 2) else 0), 7 - i, cc)
    else:
        c.rect(6, 6, 4, 2, madeira_s)
    c.contorno(K)
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
            c.set(8 + d, 8 + d, com_alpha(col, 180))
            c.set(8 - d, 8 - d, com_alpha(col, 180))
            c.set(8 + d, 8 - d, com_alpha(col, 180))
            c.set(8 - d, 8 + d, com_alpha(col, 180))
    return c


def poeira(fase):
    c = Canvas(16, 16)
    r = [2.0, 3.2, 4.0, 4.4][fase % 4]
    a = [230, 200, 140, 70][fase % 4]
    base = cor('#e8dcc8', a)
    c.elipse(8, 11, r, r * 0.7, base)
    c.elipse(5, 12, r * 0.6, r * 0.45, base)
    c.elipse(11, 12, r * 0.6, r * 0.45, base)
    return c


def fumaca(fase):
    c = Canvas(16, 16)
    r = [2.5, 3.5, 4.5, 5.0][fase % 4]
    a = [200, 170, 120, 60][fase % 4]
    c.elipse(8, 9, r, r * 0.85, cor('#8a8894', a))
    c.elipse(7, 8, r * 0.6, r * 0.5, cor('#b8b6c2', a))
    return c


def estrela_tonta(fase):
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


def nota(fase):
    c = Canvas(8, 8)
    n = cor('#fff6b8')
    if fase == 0:
        c.vline(4, 1, 5, n)
        c.hline(4, 6, 1, n)
        c.rect(2, 5, 3, 2, n)
    else:
        c.vline(2, 1, 5, n)
        c.vline(6, 1, 4, n)
        c.hline(2, 6, 1, n)
        c.rect(1, 5, 2, 2, n)
        c.rect(5, 4, 2, 2, n)
    c.contorno(K)
    return c


def pegada():
    """Pegada do Curupira: aponta para trás, para enganar os caçadores!"""
    c = Canvas(8, 8)
    p = cor('#4a3020', 170)
    # calcanhar à direita, dedos à esquerda
    c.hline(2, 5, 4, p)
    c.hline(3, 5, 5, p)
    c.set(1, 3, p)
    c.set(2, 3, p)
    c.set(1, 5, p)
    return c


def brasa(fase):
    c = Canvas(4, 4)
    if fase == 0:
        c.rect(1, 1, 2, 2, cor('#ffdc4a'))
        c.set(1, 1, cor('#fff6b8'))
    else:
        c.rect(1, 1, 2, 2, cor('#ff7a2a'))
        c.set(2, 2, cor('#ec4a2a'))
    return c


def folha(fase):
    c = Canvas(8, 8)
    if fase == 0:
        c.hline(2, 5, 3, VERDE)
        c.hline(3, 6, 4, VERDE_S)
        c.set(1, 3, VERDE_H)
    else:
        c.vline(3, 2, 5, VERDE)
        c.vline(4, 3, 6, VERDE_S)
        c.set(3, 1, VERDE_H)
    return c


def seta():
    """Seta que aponta para a semente mais próxima."""
    c = Canvas(16, 16)
    ouro = cor('#ffe066')
    c.poligono([(3, 6), (9, 6), (9, 3), (14, 8), (9, 13), (9, 10), (3, 10)], ouro)
    c.hline(4, 9, 7, cor('#fff7c2'))
    c.contorno(K)
    return c


def anel():
    """Anel do assobio (onda sonora), branco com transparência."""
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
    """Halo radial suave (usado como brilho atrás de coisas mágicas)."""
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
            if px == K:
                c.set(x, y, cor('#1a1420'))
            else:
                c.set(x, y, cor('#4a3a4a'))
    return c


def ui_semente():
    return semente(5)


def ui_assobio():
    c = Canvas(16, 16)
    n = cor('#fff6b8')
    # "ondas" de som + nota
    c.vline(7, 3, 10, n)
    c.hline(7, 10, 3, n)
    c.hline(7, 10, 4, n)
    c.vline(10, 3, 9, n)
    c.rect(5, 10, 3, 2, n)
    c.rect(8, 9, 3, 2, n)
    for (x, y) in ((12, 5), (13, 7), (12, 9)):
        c.set(x, y, cor('#a6e66c'))
    for (x, y) in ((2, 5), (1, 7), (2, 9)):
        c.set(x, y, cor('#a6e66c'))
    c.contorno(K)
    return c


def ui_botao():
    """Botão de madeira (9-slice, borda de 6 px)."""
    c = Canvas(48, 16)
    for y in range(16):
        for x in range(48):
            cc = cor('#9a6a40')
            if y in (1, 2):
                cc = cor('#c8925a')
            if y in (12, 13):
                cc = cor('#6a4428')
            if (x * 7 + y * 3) % 23 == 0 and 3 < y < 11:
                cc = cor('#865a36')
            c.set(x, y, cc)
    for x in range(48):
        c.set(x, 0, K)
        c.set(x, 15, K)
        c.set(x, 14, cor('#4a2e1c'))
    for y in range(16):
        c.set(0, y, K)
        c.set(47, y, K)
    # pregos
    for (x, y) in ((3, 4), (44, 4), (3, 10), (44, 10)):
        c.set(x, y, cor('#e4eaf0'))
    c.set(0, 0, None)
    c.set(47, 0, None)
    c.set(0, 15, None)
    c.set(47, 15, None)
    return c


def ui_painel():
    """Painel escuro com moldura de madeira (9-slice, borda de 12 px)."""
    c = Canvas(48, 48)
    fundo = cor('#1c1626', 235)
    for y in range(48):
        for x in range(48):
            c.set(x, y, fundo)
    moldura = cor('#8a5a3c')
    for i in range(5):
        cc = [K, cor('#6a4428'), moldura, cor('#c8925a'), cor('#6a4428')][i]
        for x in range(i, 48 - i):
            c.set(x, i, cc)
            c.set(x, 47 - i, cc)
        for y in range(i, 48 - i):
            c.set(i, y, cc)
            c.set(47 - i, y, cc)
    # folhas nos cantos
    for (x, y) in ((1, 1), (43, 1), (1, 43), (43, 43)):
        c.rect(x, y, 4, 4, VERDE)
        c.set(x + 1, y + 1, VERDE_H)
        c.set(x + 3, y + 3, VERDE_S)
    return c


def ui_tecla():
    """Tecla (9-slice, borda de 5 px) para a tela 'Como jogar'."""
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
    c.set(0, 0, None)
    c.set(15, 0, None)
    c.set(0, 15, None)
    c.set(15, 15, None)
    return c


def ui_branco():
    c = Canvas(4, 4)
    c.rect(0, 0, 4, 4, BRANCO)
    return c


def quadros_itens():
    q = []
    for i in range(6):
        q.append(('semente_%d' % i, semente(i)))
    for i in range(4):
        q.append(('acai_%d' % i, acai(i)))
    for i in range(4):
        q.append(('caju_%d' % i, caju(i)))
    for i in range(4):
        q.append(('guarana_%d' % i, guarana(i)))
    for i in range(4):
        q.append(('coracao_%d' % i, coracao(i)))
    for i in range(5):
        q.append(('broto_%d' % i, broto(i)))
    for i in range(4):
        q.append(('brilho_%d' % i, brilho(i)))
    for i in range(4):
        q.append(('poeira_%d' % i, poeira(i)))
    for i in range(4):
        q.append(('fumaca_%d' % i, fumaca(i)))
    q.append(('seta', seta()))
    return q


def quadros_pequenos():
    q = []
    for i in range(2):
        q.append(('estrela_%d' % i, estrela_tonta(i)))
    for i in range(2):
        q.append(('nota_%d' % i, nota(i)))
    q.append(('pegada', pegada()))
    for i in range(2):
        q.append(('folha_%d' % i, folha(i)))
    return q


def quadros_totem():
    return [('totem_desligado', totem('desligado')),
            ('totem_ligado_0', totem('ligado', 0)),
            ('totem_ligado_1', totem('ligado', 1))]
