# -*- coding: utf-8 -*-
"""
Personagens do jogo "Rumo ao Hexa": o craque canarinho (jogador, uniforme
amarelo) e os argentinos (zagueiro, lateral e cabeceador, uniforme listrado
de azul-celeste e branco com calção azul-marinho).

Todos usam o mesmo boneco de futebolista (32x32, olhando para a direita,
pés na última linha); cada pose diz onde ficam as mãos e os pés.
Cada função devolve uma lista de (nome, Canvas) com os quadros da animação.
"""
import math
from pixelart import Canvas, cor, misturar

K = cor('#1e1622')          # contorno
OLHO_B = cor('#f7f3e8')
PUPILA = cor('#1d1a2c')
BOCA = cor('#6e2430')

# ------------------------------------------------------------------ uniformes
BRASIL = {
    'camisa': cor('#ffd21f'), 'camisa_s': cor('#d9a400'), 'camisa_h': cor('#fff27a'),
    'listra': None,
    'gola': cor('#1f9d48'),
    'calcao': cor('#2557c9'), 'calcao_s': cor('#173a8f'),
    'meiao': cor('#f4f4f4'), 'meiao_s': cor('#c9c9d6'), 'faixa_meiao': cor('#1f9d48'),
    'chuteira': cor('#20202a'), 'chuteira_h': cor('#3fd06a'),
}

ARGENTINA = {
    'camisa': cor('#f6f8ff'), 'camisa_s': cor('#c6cde0'), 'camisa_h': cor('#ffffff'),
    'listra': cor('#62b8ec'), 'listra_s': cor('#3a8cc4'),
    'gola': cor('#1d2a5e'),
    'calcao': cor('#1d2a5e'), 'calcao_s': cor('#111a3c'),
    'meiao': cor('#f4f4f4'), 'meiao_s': cor('#c9c9d6'), 'faixa_meiao': cor('#62b8ec'),
    'chuteira': cor('#20202a'), 'chuteira_h': cor('#e8e8f0'),
}

PELES = {
    'morena': (cor('#b9794f'), cor('#8c5536'), cor('#d99a6c')),
    'clara': (cor('#f0c49c'), cor('#c9946c'), cor('#ffdcbc')),
    'media': (cor('#d9a074'), cor('#a8724c'), cor('#f0bf94')),
}

CABELOS = {
    'preto': (cor('#1c1820'), cor('#3a3440')),
    'castanho': (cor('#5a3418'), cor('#7e4e2a')),
    'loiro': (cor('#e8b84a'), cor('#ffd77a')),
}


def _segmento(c, x0, y0, x1, y1, cor_fn, grossura=2):
    """Linha grossa onde a cor depende de t (0 no começo, 1 no fim)."""
    passos = int(max(abs(x1 - x0), abs(y1 - y0))) + 1
    for i in range(passos + 1):
        t = i / max(passos, 1)
        x = x0 + (x1 - x0) * t
        y = y0 + (y1 - y0) * t
        cc = cor_fn(t)
        for g in range(grossura):
            c.set(x + g, y, cc)


def _perna(c, quadril, tornozelo, kit, pele, sombra=False):
    """Coxa e canela (pele em cima, meião embaixo) e a chuteira apontando para a frente."""
    pele_c = pele[1] if sombra else pele[0]
    meiao = kit['meiao_s'] if sombra else kit['meiao']
    hx, hy = quadril
    ax, ay = tornozelo

    def cor_t(t):
        if t < 0.38:
            return pele_c
        if t < 0.46:
            return kit['faixa_meiao']
        return meiao

    _segmento(c, hx, hy, ax, ay - 1, cor_t, 2)
    # chuteira (3 px para a frente, 2 de altura)
    ch = kit['chuteira']
    c.hline(ax - 1, ax + 2, ay, ch)
    c.hline(ax, ax + 2, ay - 1, ch)
    c.set(ax + 2, ay, kit['chuteira_h'])


def _braco(c, ombro, mao, kit, pele, sombra=False):
    pele_c = pele[1] if sombra else pele[0]
    manga = kit['camisa_s'] if sombra else (kit['listra'] or kit['camisa'])

    def cor_t(t):
        return manga if t < 0.3 else pele_c

    _segmento(c, ombro[0], ombro[1], mao[0], mao[1], cor_t, 2)
    c.set(mao[0] + 1, mao[1] + 1, pele[2] if not sombra else pele[1])


def _tronco(c, x0, y0, largura, altura, kit):
    for y in range(y0, y0 + altura):
        for x in range(x0, x0 + largura):
            cc = kit['camisa']
            if kit['listra'] is not None and ((x - x0) // 2) % 2 == 1:
                cc = kit['listra']
            if x == x0:
                cc = kit['camisa_s'] if kit['listra'] is None or ((x - x0) // 2) % 2 == 0 else kit['listra_s']
            elif x >= x0 + largura - 2 and y < y0 + 3 and kit['listra'] is None:
                cc = kit['camisa_h']
            c.set(x, y, cc)
    # gola em "V"
    meio = x0 + largura // 2
    c.set(meio, y0, kit['gola'])
    c.set(meio - 1, y0, kit['gola'])
    c.set(meio + 1, y0, kit['gola'])
    c.set(meio, y0 + 1, kit['gola'])
    # escudinho (só no Brasil)
    if kit is BRASIL:
        c.set(x0 + largura - 3, y0 + 2, cor('#1f9d48'))
        c.set(x0 + largura - 3, y0 + 3, cor('#2557c9'))


def _calcao(c, x0, y0, largura, kit, passo=0):
    for y in range(y0, y0 + 3):
        for x in range(x0, x0 + largura):
            cc = kit['calcao_s'] if (x == x0 or y == y0 + 2) else kit['calcao']
            c.set(x, y, cc)
    # perna do calção aberta conforme o passo
    if passo:
        c.set(x0 - 1, y0 + 2, kit['calcao_s'])
        c.set(x0 + largura, y0 + 2, kit['calcao'])


def _cabelo(c, estilo, cabeca, cores, fase=0):
    cx, cy, rx, ry = cabeca
    escuro, claro = cores
    if estilo == 'curto':
        for y in range(int(cy - ry - 1), int(cy - 1)):
            for x in range(int(cx - rx - 1), int(cx + rx + 1)):
                nx = (x + 0.5 - cx) / rx
                ny = (y + 0.5 - cy) / ry
                if nx * nx + ny * ny <= 1.15 and (ny < -0.35 or nx < -0.55):
                    c.set(x, y, escuro)
        # topete
        c.set(cx + 1, cy - ry - 1, escuro)
        c.set(cx + 2, cy - ry - 1, escuro)
        c.set(cx, cy - ry - 2, claro)
    elif estilo == 'raspado':
        for x in range(int(cx - rx), int(cx + rx)):
            y = int(cy - ry + (abs(x + 0.5 - cx) / rx) ** 2 * 2)
            c.set(x, y, escuro)
    elif estilo == 'comprido':
        for y in range(int(cy - ry - 1), int(cy + ry - 1)):
            for x in range(int(cx - rx - 1), int(cx + rx + 1)):
                nx = (x + 0.5 - cx) / rx
                ny = (y + 0.5 - cy) / ry
                if nx * nx + ny * ny <= 1.2 and (ny < -0.3 or nx < -0.35):
                    c.set(x, y, escuro)
        # rabo de cavalo balançando
        bal = [0, 1, 0, -1][fase % 4]
        for i in range(5):
            c.set(cx - rx - 1 - i * 0.6, cy - 1 + i + (bal if i > 2 else 0), escuro)
            c.set(cx - rx - i * 0.6, cy - 1 + i, claro if i == 0 else escuro)
    elif estilo == 'crespo':
        for y in range(int(cy - ry - 4), int(cy)):
            for x in range(int(cx - rx - 2), int(cx + rx + 2)):
                nx = (x + 0.5 - cx) / (rx + 1.8)
                ny = (y + 0.5 - (cy - 2)) / (ry + 1.5)
                if nx * nx + ny * ny <= 1.0 and (y < cy - 1.5 or nx < -0.5):
                    cc = escuro if (x + y) % 3 else claro
                    c.set(x, y, cc)


def _rosto(c, cabeca, expressao, brava, pele):
    cx, cy, rx, ry = cabeca
    ex = int(cx + 1)
    ey = int(cy - 1)
    if expressao == 'tonto':
        # olho em espiral
        c.set(ex, ey, PUPILA); c.set(ex + 1, ey, PUPILA)
        c.set(ex + 1, ey + 1, PUPILA); c.set(ex, ey + 2, PUPILA)
        c.set(ex + 2, ey + 1, OLHO_B); c.set(ex, ey + 1, OLHO_B)
        c.hline(ex - 1, ex + 2, int(cy + 3), BOCA)
        c.set(ex + 2, int(cy + 2), BOCA)
        return
    if expressao == 'susto':
        c.rect(ex, ey - 1, 2, 3, OLHO_B)
        c.set(ex + 1, ey, PUPILA)
        c.rect(ex, int(cy + 2), 2, 2, BOCA)
        c.linha(ex - 1, ey - 3, ex + 2, ey - 2, K)
        return
    if expressao == 'dor':
        c.set(ex, ey, PUPILA); c.set(ex + 2, ey, PUPILA)
        c.set(ex + 1, ey + 1, PUPILA)
        c.set(ex, ey + 2, PUPILA); c.set(ex + 2, ey + 2, PUPILA)
        c.hline(ex, ex + 2, int(cy + 3), BOCA)
        return
    # olho normal (2x2 com pupila na frente)
    c.rect(ex, ey, 2, 2, OLHO_B)
    c.set(ex + 1, ey, PUPILA)
    c.set(ex + 1, ey + 1, PUPILA)
    if expressao == 'baixo':
        c.set(ex + 1, ey, OLHO_B)
        c.set(ex, ey + 1, PUPILA)
    if brava:
        c.linha(ex - 1, ey - 2, ex + 2, ey - 1, K)
    else:
        c.hline(ex, ex + 1, ey - 2, K)
    my = int(cy + 3)
    if expressao == 'feliz':
        c.hline(ex - 1, ex + 2, my, BOCA)
        c.set(ex - 2, my - 1, BOCA)
        c.set(ex + 3, my - 1, BOCA)
    elif expressao == 'grito':
        c.rect(ex, my - 1, 2, 2, BOCA)
    elif brava:
        c.hline(ex - 1, ex + 1, my, BOCA)
        c.set(ex + 2, my + 1, BOCA)
    else:
        c.hline(ex, ex + 1, my, BOCA)
    # bochecha / nariz
    c.set(int(cx + rx - 0.5), int(cy + 0.5), pele[0])


def futebolista(pose, kit, pele='morena', cabelo=('curto', 'preto'), largura=8, brava=False, extra=None):
    """
    pose: dicionário com
      bob    - deslocamento vertical do corpo (0, 1 ou 2)
      pernas - [(ax, ay) de trás, (ax, ay) da frente]: tornozelos
      bracos - [(hx, hy) de trás, (hx, hy) da frente]: mãos
      rosto  - normal | feliz | dor | susto | tonto | baixo | grito
      incl   - inclinação do tronco para a frente (0 ou 1 px)
      fase   - quadro (para cabelo balançando)
    """
    c = Canvas(32, 32)
    p = PELES[pele]
    b = pose.get('bob', 0)
    incl = pose.get('incl', 0)
    (bx, by), (fx, fy) = pose['pernas']
    (hbx, hby), (hfx, hfy) = pose['bracos']
    x0 = 16 - largura // 2
    topo_tronco = 14 + b
    quadril_y = 22 + b

    # braço de trás (mais escuro, atrás do corpo)
    _braco(c, (x0 + incl, topo_tronco + 1), (hbx, hby), kit, p, sombra=True)
    # perna de trás e da frente
    _perna(c, (x0 + 1, quadril_y + 2), (bx, by), kit, p, sombra=True)
    _perna(c, (x0 + largura - 3, quadril_y + 2), (fx, fy), kit, p)
    # calção e camisa
    _calcao(c, x0, quadril_y, largura, kit, passo=abs(fx - bx) > 6)
    _tronco(c, x0 + incl, topo_tronco, largura, quadril_y - topo_tronco, kit)
    # pescoço e cabeça
    c.rect(15 + incl, topo_tronco - 1, 2, 1, p[1])
    cabeca = (16.2 + incl, 9.0 + b, 4.6, 4.4)

    def pele_cabeca(x, y, nx, ny):
        if nx < -0.55 or ny > 0.62:
            return p[1]
        if nx > 0.25 and ny < -0.1:
            return p[2]
        return p[0]

    c.elipse_fn(cabeca[0], cabeca[1], cabeca[2], cabeca[3], pele_cabeca)
    c.set(int(cabeca[0] + cabeca[2]), int(cabeca[1] + 0.5), p[0])   # nariz
    c.set(int(cabeca[0] - 2), int(cabeca[1] + 0.5), p[1])           # orelha
    estilo, tom = cabelo
    _cabelo(c, estilo, cabeca, CABELOS[tom], pose.get('fase', 0))
    _rosto(c, cabeca, pose.get('rosto', 'normal'), brava, p)
    if extra == 'faixa':
        c.hline(int(cabeca[0] - 4), int(cabeca[0] + 3), int(cabeca[1] - 3), cor('#e8e8f0'))
    if extra == 'barba':
        for x in range(int(cabeca[0] - 1), int(cabeca[0] + 4)):
            c.set(x, int(cabeca[1] + 3), CABELOS['castanho'][0])
            if x % 2:
                c.set(x, int(cabeca[1] + 2), CABELOS['castanho'][0])
    # braço da frente (por cima do corpo)
    _braco(c, (x0 + largura - 2 + incl, topo_tronco + 1), (hfx, hfy), kit, p)
    c.contorno(K)
    return c


# ======================================================================
# O CRAQUE CANARINHO
# ======================================================================
def craque(pose):
    return futebolista(pose, BRASIL, 'morena', ('curto', 'preto'))


def quadros_jogador():
    q = []
    # --- parado (respirando)
    for i in range(4):
        b = 1 if i in (1, 2) else 0
        q.append(('craque_parado_%d' % i, craque({
            'bob': b, 'pernas': [(13, 31), (18, 31)], 'bracos': [(11, 22 + b), (21, 22 + b)], 'fase': i})))
    # --- correndo (6 quadros)
    passos = [
        ((9, 30), (21, 31), (10, 21), (22, 19), 0),
        ((11, 28), (19, 31), (11, 22), (21, 20), 1),
        ((14, 27), (17, 31), (12, 22), (20, 21), 1),
        ((21, 31), (9, 30), (13, 20), (19, 22), 0),
        ((19, 31), (11, 28), (12, 21), (20, 22), 1),
        ((17, 31), (14, 27), (11, 21), (21, 21), 1),
    ]
    for i, (pb, pf, mb, mf, b) in enumerate(passos):
        q.append(('craque_correr_%d' % i, craque({
            'bob': b, 'pernas': [pb, pf], 'bracos': [(mb[0], mb[1] + b), (mf[0], mf[1] + b)],
            'incl': 1, 'fase': i})))
    # --- pulo e queda
    for i in range(2):
        q.append(('craque_pulo_%d' % i, craque({
            'pernas': [(11, 28), (20, 27 + i)], 'bracos': [(9, 13), (23, 12 + i)], 'rosto': 'feliz', 'fase': i})))
    for i in range(2):
        q.append(('craque_queda_%d' % i, craque({
            'pernas': [(12, 31), (20, 30)], 'bracos': [(8, 16 + i), (24, 15 + i)], 'rosto': 'susto', 'fase': i})))
    # --- chute: preparação, batida (perna esticada) e acompanhamento
    q.append(('craque_chute_0', craque({
        'pernas': [(15, 31), (8, 26)], 'bracos': [(19, 18), (12, 20)], 'incl': 0})))
    q.append(('craque_chute_1', craque({
        'pernas': [(14, 31), (25, 27)], 'bracos': [(9, 18), (22, 20)], 'incl': 1, 'rosto': 'grito'})))
    q.append(('craque_chute_2', craque({
        'pernas': [(14, 31), (24, 21)], 'bracos': [(8, 17), (21, 22)], 'incl': 0, 'rosto': 'feliz'})))
    # --- carregando a folha seca (perna lá atrás, tremendo)
    for i in range(2):
        q.append(('craque_carregar_%d' % i, craque({
            'bob': i, 'pernas': [(16, 31), (6 + i, 24)], 'bracos': [(21, 16 + i), (13, 19)], 'rosto': 'grito'})))
    # --- embaixadinha: o pé sobe (quadro 0) e desce
    for i in range(4):
        pe = (21, 25) if i == 0 else ((20, 28) if i in (1, 3) else (19, 30))
        q.append(('craque_embaixadinha_%d' % i, craque({
            'bob': 0, 'pernas': [(14, 31), pe], 'bracos': [(10, 19), (22, 19)], 'rosto': 'baixo', 'fase': i})))
    # --- dano
    q.append(('craque_dano_0', craque({
        'pernas': [(10, 30), (16, 31)], 'bracos': [(8, 14), (22, 14)], 'rosto': 'dor'})))
    # --- vitória (braços para cima)
    for i in range(2):
        q.append(('craque_vitoria_%d' % i, craque({
            'bob': i, 'pernas': [(13, 31), (19, 31)], 'bracos': [(7, 7 + i), (25, 6 + i)], 'rosto': 'feliz'})))
    return q


# ======================================================================
# OS ARGENTINOS
# ======================================================================
def zagueiro(pose):
    """Zagueiro: forte, barbudo e de cabelo raspado."""
    return futebolista(pose, ARGENTINA, 'clara', ('raspado', 'castanho'), largura=9, brava=True, extra='barba')


def lateral(pose):
    """Lateral: rápido, cabelo comprido com faixa."""
    return futebolista(pose, ARGENTINA, 'media', ('comprido', 'loiro'), largura=8, brava=True, extra='faixa')


def cabeceador(pose):
    """Cabeceador: cabelão crespo, vive pulando para cabecear."""
    return futebolista(pose, ARGENTINA, 'morena', ('crespo', 'preto'), largura=8, brava=True)


ANDAR = [
    ((11, 31), (20, 31), (10, 22), (22, 21), 0),
    ((13, 30), (18, 31), (11, 22), (21, 22), 1),
    ((19, 31), (12, 31), (12, 21), (20, 22), 0),
    ((17, 31), (14, 30), (11, 22), (21, 21), 1),
]


def _quadros_argentino(prefixo, fn, pose_extra=None):
    q = []
    for i, (pb, pf, mb, mf, b) in enumerate(ANDAR):
        pose = {'bob': b, 'pernas': [pb, pf], 'bracos': [(mb[0], mb[1] + b), (mf[0], mf[1] + b)], 'fase': i}
        if pose_extra:
            pose.update(pose_extra)
        q.append(('%s_andar_%d' % (prefixo, i), fn(pose)))
    for i in range(2):
        q.append(('%s_tonto_%d' % (prefixo, i), fn({
            'bob': i, 'pernas': [(12 + i, 31), (19 - i, 31)], 'bracos': [(9, 19 + i), (23, 18 - i)],
            'rosto': 'tonto', 'fase': i})))
    q.append(('%s_expulso_0' % prefixo, fn({
        'pernas': [(12, 31), (19, 31)], 'bracos': [(7, 13), (25, 13)], 'rosto': 'susto'})))
    return q


def quadros_argentinos():
    q = []
    q += _quadros_argentino('zagueiro', zagueiro)
    # lateral: corre inclinado (arrancada)
    corrida = [
        ((8, 30), (22, 31), (9, 20), (23, 18), 0),
        ((11, 28), (19, 31), (10, 21), (22, 19), 1),
        ((22, 31), (8, 30), (12, 19), (19, 21), 0),
        ((19, 31), (11, 28), (11, 20), (21, 21), 1),
    ]
    q += _quadros_argentino('lateral', lateral)
    for i, (pb, pf, mb, mf, b) in enumerate(corrida):
        q.append(('lateral_correr_%d' % i, lateral({
            'bob': b, 'pernas': [pb, pf], 'bracos': [(mb[0], mb[1] + b), (mf[0], mf[1] + b)],
            'incl': 1, 'rosto': 'grito', 'fase': i})))
    # cabeceador: agachado, subindo (cabeçada) e descendo
    q += _quadros_argentino('cabeceador', cabeceador)
    q.append(('cabeceador_pulo_0', cabeceador({
        'bob': 2, 'pernas': [(11, 31), (20, 31)], 'bracos': [(10, 24), (22, 24)]})))
    q.append(('cabeceador_pulo_1', cabeceador({
        'bob': 0, 'pernas': [(13, 29), (18, 30)], 'bracos': [(9, 12), (23, 11)], 'rosto': 'grito'})))
    q.append(('cabeceador_pulo_2', cabeceador({
        'bob': 0, 'pernas': [(12, 31), (19, 30)], 'bracos': [(8, 17), (24, 16)]})))
    return q
