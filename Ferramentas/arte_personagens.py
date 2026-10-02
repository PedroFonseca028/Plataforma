# -*- coding: utf-8 -*-
"""
Personagens do jogo: o Curupira (jogador) e os inimigos
(Serra-Serra, Drone Madeireiro e Fagulha).
Cada função devolve uma lista de (nome, Canvas) com os quadros da animação.
"""
import math
import random
from pixelart import Canvas, cor, misturar, com_alpha

# ------------------------------------------------------------------ paleta
K = cor('#2a1a22')          # contorno
PELE = cor('#c67c4e')
PELE_S = cor('#955338')
PELE_H = cor('#e6a575')
OLHO_B = cor('#f7f3e8')
PUPILA = cor('#1d1a2c')
BOCA = cor('#6e2430')
FOGO = [cor('#fff6b8'), cor('#ffdc4a'), cor('#ff9e2c'), cor('#ec4a2a'), cor('#a52a36')]
FOLHA = cor('#58bd48')
FOLHA_S = cor('#2e7a3a')
FOLHA_H = cor('#a6e66c')


def _fogo_cor(calor):
    """calor 0..1 -> cor da chama (1 = mais quente/amarelo)."""
    if calor > 0.80:
        return FOGO[0]
    if calor > 0.58:
        return FOGO[1]
    if calor > 0.36:
        return FOGO[2]
    if calor > 0.16:
        return FOGO[3]
    return FOGO[4]


def desenhar_chamas(c, base_y, linguas, inclinacao, fase, cabeca):
    """
    Cabelo de fogo: pinta o topo e a nuca da cabeça com fogo e desenha
    línguas de chama subindo. `cabeca` = (cx, cy, rx, ry) da cabeça.
    """
    hcx, hcy, hrx, hry = cabeca
    calor = {}

    def aquecer(x, y, v):
        k = (int(x), int(y))
        if v > calor.get(k, 0):
            calor[k] = v

    # calota: topo da cabeça (acima dos olhos) e nuca (lado de trás)
    for y in range(int(hcy - hry - 1), int(hcy + hry + 1)):
        for x in range(int(hcx - hrx - 1), int(hcx + hrx + 1)):
            nx = (x + 0.5 - hcx) / hrx
            ny = (y + 0.5 - hcy) / hry
            if nx * nx + ny * ny > 1.0:
                continue
            topo = y < base_y + 1
            nuca = nx < -0.25 and y < hcy + 1.5
            if topo or nuca:
                aquecer(x, y, 0.30 + 0.30 * (1 - abs(nx)) + (0.12 if y < base_y - 1 else 0))

    # línguas de fogo
    for (xb, altura, larg) in linguas:
        altura = altura + (1 if (fase + int(xb)) % 3 == 0 else 0) - (1 if (fase + int(xb)) % 4 == 1 else 0)
        for i in range(int(altura) + 1):
            t = i / max(altura, 1)
            y = base_y - 1 - i
            desloc = -inclinacao * (t ** 1.4) + math.sin((fase * 1.7 + xb) + t * 3.0) * 0.6 * t
            centro = xb + desloc
            meia = max(0.5, (larg / 2.0) * (1 - t * 0.85))
            for x in range(int(centro - meia - 1), int(centro + meia + 2)):
                d = abs(x + 0.5 - centro) / meia
                if d <= 1.0:
                    aquecer(x, y, (1 - d) * 0.95 * (1 - t * 0.6) + 0.08)

    for (x, y), v in calor.items():
        c.set(x, y, _fogo_cor(v))

    # faísca solta acima (pisca conforme a fase)
    if fase % 2 == 0:
        c.set(hcx - inclinacao * 0.8 + (fase % 3) - 1, base_y - 9 - (fase % 2), FOGO[1])


def _membro(c, x0, y0, x1, y1, cor_membro, grossura=2):
    passos = int(max(abs(x1 - x0), abs(y1 - y0))) + 1
    for i in range(passos + 1):
        t = i / max(passos, 1)
        x = x0 + (x1 - x0) * t
        y = y0 + (y1 - y0) * t
        for g in range(grossura):
            c.set(x + g, y, cor_membro)


def _pe_para_tras(c, ax, ay, cor_pe, virado=False):
    """Pé do Curupira: os dedos apontam para TRÁS (para a esquerda quando ele olha para a direita)."""
    # calcanhar à direita (x = ax+1), dedos à esquerda
    c.hline(ax - 2, ax + 1, ay, cor_pe)
    c.hline(ax - 1, ax + 1, ay - 1, cor_pe)
    c.set(ax - 2, ay, PELE_H if cor_pe == PELE else cor_pe)  # ponta dos dedos


def curupira(pose):
    """
    pose: dicionário com
      bob      - deslocamento vertical do corpo (0 ou 1)
      pernas   - [(ax, ay), (ax, ay)] tornozelo da perna de trás e da frente
      bracos   - [(hx, hy), (hx, hy)] mão do braço de trás e da frente
      chamas   - (linguas, inclinacao, fase)
      rosto    - 'normal' | 'feliz' | 'dor' | 'assobio' | 'susto'
    """
    c = Canvas(32, 32)
    b = pose.get('bob', 0)
    (bx, by), (fx, fy) = pose['pernas']
    (hbx, hby), (hfx, hfy) = pose['bracos']
    rosto = pose.get('rosto', 'normal')

    # ---- braço de trás (mais escuro, atrás do corpo)
    _membro(c, 12, 19 + b, hbx, hby, PELE_S, 2)
    c.set(hbx, hby + 1, PELE_S)

    # ---- perna de trás
    _membro(c, 13, 25 + b, bx, by - 1, PELE_S, 2)
    _pe_para_tras(c, bx, by, PELE_S)

    # ---- perna da frente
    _membro(c, 17, 25 + b, fx, fy - 1, PELE, 2)
    _pe_para_tras(c, fx, fy, PELE)

    # ---- tronco
    for y in range(18 + b, 24 + b):
        for x in range(12, 20):
            cc = PELE
            if x == 12 or y == 18 + b:
                cc = PELE_S
            elif x in (17, 18) and y in (19 + b, 20 + b):
                cc = PELE_H
            c.set(x, y, cc)
    c.set(12, 18 + b, None)
    c.set(19, 18 + b, None)
    # colar de sementes (açaí e guaraná)
    for i, x in enumerate(range(13, 19)):
        c.set(x, 18 + b, cor('#d8423a') if i % 2 == 0 else cor('#ffd24a'))
    c.set(15, 19 + b, cor('#6a2a7a'))

    # ---- saia de folhas
    for y in range(22 + b, 26 + b):
        for x in range(11, 21):
            cc = FOLHA
            if y == 22 + b:
                cc = FOLHA_H if x % 2 == 0 else FOLHA
            if x <= 12 or y == 25 + b:
                cc = FOLHA_S
            c.set(x, y, cc)
    for x in range(11, 21, 2):  # pontas das folhas
        c.set(x, 26 + b, FOLHA_S)
    c.set(15, 23 + b, FOLHA_S)  # nervura
    c.set(18, 24 + b, FOLHA_H)

    # ---- cabeça
    def pele_cabeca(x, y, nx, ny):
        if nx < -0.55 or ny > 0.62:
            return PELE_S
        if nx > 0.2 and ny < -0.1:
            return PELE_H
        return PELE

    cabeca = (15.8, 12.5 + b, 5.4, 5.0)
    c.elipse_fn(cabeca[0], cabeca[1], cabeca[2], cabeca[3], pele_cabeca)
    # nariz
    c.set(21, 13 + b, PELE)

    # ---- cabelo de fogo (cobre topo e nuca)
    linguas, incl, fase = pose['chamas']
    desenhar_chamas(c, 9 + b, linguas, incl, fase, cabeca)
    # orelha (na frente da nuca em chamas)
    c.set(13, 12 + b, PELE_S)
    c.set(13, 13 + b, PELE_S)
    c.set(12, 13 + b, PELE_S)

    # ---- rosto
    ey = 11 + b
    if rosto == 'dor':
        # olho em "X"
        c.set(18, ey, PUPILA); c.set(20, ey, PUPILA)
        c.set(19, ey + 1, PUPILA)
        c.set(18, ey + 2, PUPILA); c.set(20, ey + 2, PUPILA)
        c.hline(18, 20, 15 + b, BOCA)
        c.set(19, 16 + b, BOCA)
    else:
        c.rect(18, ey, 2, 3, OLHO_B)
        c.set(20, ey, OLHO_B)
        c.set(20, ey + 1, OLHO_B)
        if rosto == 'susto':
            c.set(19, ey + 1, PUPILA)
        else:
            c.set(19, ey + 1, PUPILA)
            c.set(19, ey + 2, PUPILA)
            c.set(20, ey + 1, PUPILA)
        c.hline(18, 20, ey - 1, FOGO[3])  # sobrancelha (fogo!)
        if rosto == 'feliz':
            c.hline(18, 20, 15 + b, BOCA)
            c.set(17, 14 + b, BOCA)
            c.set(21, 14 + b, BOCA)
        elif rosto == 'assobio':
            c.set(20, 15 + b, BOCA)
            c.set(21, 15 + b, BOCA)
        else:
            c.hline(19, 20, 15 + b, BOCA)
            c.set(18, 14 + b, BOCA)
        c.set(17, 14 + b, cor('#d9876a'))  # bochecha

    # ---- braço da frente (por cima do corpo)
    _membro(c, 19, 19 + b, hfx, hfy, PELE, 2)
    c.set(hfx + 1, hfy + 1, PELE_H)

    c.contorno(K)
    return c


# chamas "padrão": (x_base, altura, largura)
CHAMAS = [(10.5, 5, 3.4), (13.0, 7, 4.2), (16.0, 8, 4.4), (19.0, 5, 3.2)]


def quadros_curupira():
    q = []
    # --- parado (respiração + chama tremulando)
    for i in range(4):
        b = 1 if i in (1, 2) else 0
        q.append(('curupira_parado_%d' % i, curupira({
            'bob': b,
            'pernas': [(14, 31), (18, 31)],
            'bracos': [(11, 23 + b), (20, 23 + b)],
            'chamas': (CHAMAS, 1.2, i),
        })))
    # --- correndo (6 quadros)
    passos = [
        ((10, 30), (21, 31), (10, 22), (22, 20), 0),
        ((12, 28), (19, 31), (11, 23), (21, 21), 1),
        ((15, 27), (17, 31), (12, 23), (20, 22), 1),
        ((21, 31), (10, 30), (14, 21), (18, 23), 0),
        ((19, 31), (12, 28), (13, 22), (19, 23), 1),
        ((17, 31), (15, 27), (12, 22), (20, 22), 1),
    ]
    for i, (pb, pf, mb, mf, b) in enumerate(passos):
        q.append(('curupira_correr_%d' % i, curupira({
            'bob': b,
            'pernas': [pb, pf],
            'bracos': [(mb[0], mb[1] + b), (mf[0], mf[1] + b)],
            'chamas': (CHAMAS, 3.2, i),
        })))
    # --- pulo (subindo)
    for i in range(2):
        q.append(('curupira_pulo_%d' % i, curupira({
            'bob': 0,
            'pernas': [(12, 29), (19, 28)],
            'bracos': [(9, 15), (22, 14)],
            'chamas': (CHAMAS, 1.0 + i, i + 1),
            'rosto': 'feliz',
        })))
    # --- queda
    for i in range(2):
        q.append(('curupira_queda_%d' % i, curupira({
            'bob': 0,
            'pernas': [(13, 31), (19, 31)],
            'bracos': [(8, 17), (23, 16)],
            'chamas': (CHAMAS, -1.5 - i, i + 2),
            'rosto': 'susto',
        })))
    # --- dano
    q.append(('curupira_dano_0', curupira({
        'bob': 0,
        'pernas': [(11, 30), (16, 31)],
        'bracos': [(8, 15), (22, 15)],
        'chamas': (CHAMAS, 2.5, 5),
        'rosto': 'dor',
    })))
    # --- assobio (mão na boca)
    for i in range(2):
        q.append(('curupira_assobio_%d' % i, curupira({
            'bob': i,
            'pernas': [(14, 31), (18, 31)],
            'bracos': [(11, 23 + i), (21, 15 + i)],
            'chamas': (CHAMAS, 0.8, i * 2),
            'rosto': 'assobio',
        })))
    # --- vitória (braços para cima)
    for i in range(2):
        q.append(('curupira_vitoria_%d' % i, curupira({
            'bob': i,
            'pernas': [(13, 31), (19, 31)],
            'bracos': [(9, 12 + i), (22, 12 + i)],
            'chamas': (CHAMAS, 0.0, i * 3),
            'rosto': 'feliz',
        })))
    return q


# ======================================================================
# INIMIGOS
# ======================================================================
LARANJA = cor('#f28a2e')
LARANJA_S = cor('#b3521f')
LARANJA_H = cor('#ffc15a')
METAL = cor('#aeb6c0')
METAL_S = cor('#6c7480')
METAL_H = cor('#e4eaf0')
ESCURO = cor('#3a3844')
ESCURO_H = cor('#5c5a6a')
VERMELHO = cor('#ff3b3b')
VERMELHO_S = cor('#a3202a')


def _olho_bravo(c, x, y, tipo='normal', olhando=1):
    """Olho grande de desenho animado (4x4) com sobrancelha brava."""
    c.rect(x, y, 4, 4, OLHO_B)
    c.set(x, y, None)
    c.set(x + 3, y, None)
    c.set(x, y + 3, None)
    c.set(x + 3, y + 3, None)
    if tipo == 'tonto':
        c.set(x + 1, y + 1, PUPILA)
        c.set(x + 2, y + 2, PUPILA)
        c.set(x + 2, y + 1, PUPILA)
        c.set(x + 1, y + 2, PUPILA)
        c.set(x + 1, y + 1, OLHO_B)
    elif tipo == 'x':
        c.set(x, y, PUPILA)
        c.set(x + 3, y + 3, PUPILA)
        c.set(x + 3, y, PUPILA)
        c.set(x, y + 3, PUPILA)
        c.set(x + 1, y + 1, PUPILA)
        c.set(x + 2, y + 2, PUPILA)
        c.set(x + 2, y + 1, PUPILA)
        c.set(x + 1, y + 2, PUPILA)
    else:
        px = x + (2 if olhando > 0 else 0)
        c.rect(px, y + 1, 2, 2, PUPILA)
    if tipo == 'normal':
        # sobrancelha brava (inclinada para dentro)
        c.linha(x - 1, y - 2, x + 3, y - 1, K)


def serra_serra(fase, estado='andar'):
    """Serra-Serra: motosserra-robô da madeireira (anda sobre esteira)."""
    c = Canvas(32, 32)
    achatar = estado == 'esmagado'
    topo = 23 if achatar else 16
    # esteira
    c.rect(5, 27, 19, 5, ESCURO)
    c.rect(6, 26, 17, 1, ESCURO)
    for i, wx in enumerate((8, 14, 20)):
        c.elipse(wx + 0.5, 29.5, 2.2, 2.2, ESCURO_H)
        ang = (fase + i) % 2
        if ang == 0:
            c.set(wx, 29, METAL_S)
            c.set(wx + 1, 30, METAL_S)
        else:
            c.set(wx + 1, 29, METAL_S)
            c.set(wx, 30, METAL_S)
    for x in range(6, 23):
        if (x + fase) % 3 == 0:
            c.set(x, 31, ESCURO_H)
    # corpo (motor laranja)
    for y in range(topo, 27):
        for x in range(5, 22):
            cc = LARANJA
            if y == topo or (y == topo + 1 and x > 7):
                cc = LARANJA_H
            if x == 5 or y >= 25:
                cc = LARANJA_S
            c.set(x, y, cc)
    c.set(5, topo, None)
    c.set(21, topo, None)
    if not achatar:
        # alça
        c.hline(9, 16, 13, ESCURO)
        c.vline(9, 13, 15, ESCURO)
        c.vline(16, 13, 15, ESCURO)
        # escapamento soltando fumaça
        c.rect(6, 12, 2, 4, ESCURO_H)
        if estado == 'andar' and fase % 2 == 0:
            c.set(6, 10, cor('#9a9aa6'))
            c.set(7, 9, cor('#c8c8d2'))
    # sabre da serra
    sy = 24 if achatar else 19
    c.rect(21, sy, 9, 4, METAL)
    c.hline(21, 29, sy, METAL_H)
    c.hline(21, 29, sy + 3, METAL_S)
    c.set(30, sy + 1, METAL)
    c.set(30, sy + 2, METAL_S)
    # dentes (a corrente anda)
    for x in range(21, 31):
        if (x + fase) % 2 == 0:
            c.set(x, sy - 1, METAL_H)
            c.set(x, sy + 4, METAL_S)
    # olho e boca
    if achatar:
        _olho_bravo(c, 11, 23, 'x')
    else:
        tipo = 'tonto' if estado == 'tonto' else 'normal'
        _olho_bravo(c, 12, 18, tipo, 1)
        # grade da boca
        c.rect(12, 23, 6, 2, ESCURO)
        for x in range(12, 18, 2):
            c.set(x, 23, METAL_H)
    c.contorno(K)
    return c


def drone(fase, estado='voar'):
    """Drone Madeireiro: vigia voador da madeireira."""
    c = Canvas(32, 32)
    queda = estado == 'esmagado'
    dy = 3 if queda else 0
    # braços
    c.hline(5, 11, 16 + dy, ESCURO)
    c.hline(21, 27, 16 + dy, ESCURO)
    # hélices (borrão animado)
    tamanhos = [6, 4, 2, 4] if estado == 'voar' else [3, 3, 3, 3]
    t = tamanhos[fase % 4]
    for hx in (7.5, 24.5):
        c.vline(int(hx), 14 + dy, 15 + dy, ESCURO_H)
        if not queda:
            for x in range(int(hx - t), int(hx + t) + 1):
                c.set(x, 13 + dy, com_alpha(METAL_H, 230 if abs(x - hx) < t - 1 else 150))
        else:
            c.linha(hx - 3, 12 + dy, hx + 2, 14 + dy, METAL_S)

    def corpo(x, y, nx, ny):
        if ny < -0.45:
            return METAL_H
        if ny > 0.45:
            return METAL_S
        return METAL

    c.elipse_fn(16, 19 + dy, 7, 4.5, corpo)
    # antena com luz piscando
    if not queda:
        c.vline(16, 12, 14, ESCURO)
        c.set(16, 11, VERMELHO if fase % 2 == 0 else VERMELHO_S)
    # lente (olho)
    if queda:
        _olho_bravo(c, 14, 17 + dy, 'x')
    else:
        c.elipse(16, 20, 2.6, 2.6, VERMELHO_S if estado == 'tonto' else VERMELHO)
        if estado == 'tonto':
            c.set(15, 19, OLHO_B)
            c.set(17, 21, OLHO_B)
        else:
            c.set(15, 19, OLHO_B)
            c.linha(12, 16, 15, 17, K)
            c.linha(20, 16, 17, 17, K)
    # garra
    c.set(14, 24 + dy, ESCURO)
    c.set(18, 24 + dy, ESCURO)
    c.set(13, 25 + dy, ESCURO)
    c.set(19, 25 + dy, ESCURO)
    c.contorno(K)
    return c


def fagulha(fase, estado='viva'):
    """Fagulha: brasa viva e zangada que nasceu da queimada."""
    c = Canvas(32, 32)
    if estado == 'esmagado':
        # só sobra fumaça e uma brasinha
        for (x, y, r, g) in ((13, 26, 3, '#8a8894'), (18, 25, 3.5, '#a8a6b2'), (16, 22, 2.5, '#c6c4ce')):
            c.elipse(x, y, r, r * 0.8, cor(g))
        c.set(15, 29, FOGO[2])
        c.set(16, 29, FOGO[3])
        c.contorno(cor('#4a4852'))
        return c
    fraca = estado == 'tonto'
    oscila = [0, 1, 0, -1][fase % 4]
    cx = 16
    calor = {}
    for y in range(8, 32):
        for x in range(6, 27):
            # gota: círculo embaixo + ponta em cima
            dyc = (y + 0.5) - 24.5
            r = 6.5
            if dyc >= 0:
                d = math.hypot((x + 0.5 - cx) / r, dyc / 5.5)
            else:
                t = min(1.0, -dyc / 14.0)
                centro = cx + oscila * t * 3 + math.sin(fase + t * 5) * t * 1.2
                larg = r * (1 - t) ** 0.9
                if larg <= 0.3:
                    continue
                d = abs(x + 0.5 - centro) / larg
            if d <= 1.0:
                calor[(x, y)] = (1 - d) * (0.9 - 0.25 * max(0, -dyc) / 14.0) + 0.1
    for (x, y), v in calor.items():
        if fraca:
            v *= 0.55
        c.set(x, y, _fogo_cor(v))
    # línguas extras
    for lx, alt in ((11, 4), (21, 5)):
        for i in range(alt):
            c.set(lx + (oscila if i > 1 else 0), 20 - i - (fase % 2), FOGO[3] if i > 1 else FOGO[2])
    # rosto
    for ox in (11, 17):
        c.rect(ox, 21, 3, 3, OLHO_B)
        if fraca:
            c.set(ox + 1, 22, PUPILA)
        else:
            c.set(ox + 1, 22, PUPILA)
            c.set(ox + 2, 22, PUPILA)
            c.set(ox + 1, 23, PUPILA)
    if not fraca:
        c.linha(10, 19, 13, 20, K)
        c.linha(22, 19, 19, 20, K)
        c.hline(14, 18, 26, BOCA)
        c.set(15, 27, BOCA)
        c.set(17, 27, BOCA)
    else:
        c.hline(15, 17, 26, BOCA)
    c.contorno(K)
    return c


def quadros_inimigos():
    q = []
    for i in range(4):
        q.append(('serra_andar_%d' % i, serra_serra(i, 'andar')))
    for i in range(2):
        q.append(('serra_tonto_%d' % i, serra_serra(i, 'tonto')))
    q.append(('serra_esmagado_0', serra_serra(0, 'esmagado')))
    for i in range(4):
        q.append(('drone_voar_%d' % i, drone(i, 'voar')))
    for i in range(2):
        q.append(('drone_tonto_%d' % i, drone(i, 'tonto')))
    q.append(('drone_esmagado_0', drone(0, 'esmagado')))
    for i in range(4):
        q.append(('fagulha_viva_%d' % i, fagulha(i, 'viva')))
    for i in range(2):
        q.append(('fagulha_tonto_%d' % i, fagulha(i, 'tonto')))
    q.append(('fagulha_esmagado_0', fagulha(0, 'esmagado')))
    return q
