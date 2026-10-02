# -*- coding: utf-8 -*-
"""
Mini biblioteca de pixel art usada pelo gerador de arte do jogo
"Curupira: Guardião da Mata".

Tudo é desenhado "pixel a pixel" por código, para que a arte seja
100% original do projeto.
"""
import math
import random
from PIL import Image


def cor(hexa, a=255):
    """Converte '#rrggbb' em tupla RGBA."""
    hexa = hexa.lstrip('#')
    return (int(hexa[0:2], 16), int(hexa[2:4], 16), int(hexa[4:6], 16), a)


def misturar(c1, c2, t):
    """Interpola duas cores RGBA (t=0 -> c1, t=1 -> c2)."""
    return tuple(int(round(c1[i] + (c2[i] - c1[i]) * t)) for i in range(4))


def com_alpha(c, a):
    return (c[0], c[1], c[2], int(a))


class Canvas:
    """Tela RGBA simples. Origem (0,0) no canto superior esquerdo."""

    def __init__(self, w, h):
        self.w = w
        self.h = h
        self.p = [[None] * w for _ in range(h)]

    # ------------------------------------------------------------ básico
    def dentro(self, x, y):
        return 0 <= x < self.w and 0 <= y < self.h

    def get(self, x, y):
        x, y = int(x), int(y)
        if self.dentro(x, y):
            return self.p[y][x]
        return None

    def opaco(self, x, y):
        c = self.get(x, y)
        return c is not None and c[3] >= 128

    def set(self, x, y, c):
        x, y = int(math.floor(x)), int(math.floor(y))
        if not self.dentro(x, y):
            return
        if c is None:
            self.p[y][x] = None
            return
        if c[3] >= 255 or self.p[y][x] is None:
            self.p[y][x] = c
        else:
            # mistura alpha "por cima"
            base = self.p[y][x]
            a = c[3] / 255.0
            na = min(255, int(base[3] + c[3] * (1 - base[3] / 255.0)))
            self.p[y][x] = (
                int(base[0] * (1 - a) + c[0] * a),
                int(base[1] * (1 - a) + c[1] * a),
                int(base[2] * (1 - a) + c[2] * a),
                na,
            )

    def set_se_vazio(self, x, y, c):
        if self.get(x, y) is None:
            self.set(x, y, c)

    def rect(self, x, y, w, h, c):
        for yy in range(int(y), int(y + h)):
            for xx in range(int(x), int(x + w)):
                self.set(xx, yy, c)

    def hline(self, x0, x1, y, c):
        for x in range(int(min(x0, x1)), int(max(x0, x1)) + 1):
            self.set(x, y, c)

    def vline(self, x, y0, y1, c):
        for y in range(int(min(y0, y1)), int(max(y0, y1)) + 1):
            self.set(x, y, c)

    def linha(self, x0, y0, x1, y1, c):
        x0, y0, x1, y1 = int(round(x0)), int(round(y0)), int(round(x1)), int(round(y1))
        dx, dy = abs(x1 - x0), -abs(y1 - y0)
        sx = 1 if x0 < x1 else -1
        sy = 1 if y0 < y1 else -1
        err = dx + dy
        while True:
            self.set(x0, y0, c)
            if x0 == x1 and y0 == y1:
                break
            e2 = 2 * err
            if e2 >= dy:
                err += dy
                x0 += sx
            if e2 <= dx:
                err += dx
                y0 += sy

    def elipse(self, cx, cy, rx, ry, c):
        """Elipse preenchida com centro em (cx, cy) - coordenadas de pixel."""
        for y in range(int(cy - ry - 1), int(cy + ry + 2)):
            for x in range(int(cx - rx - 1), int(cx + rx + 2)):
                dx = (x + 0.5 - cx) / max(rx, 0.01)
                dy = (y + 0.5 - cy) / max(ry, 0.01)
                if dx * dx + dy * dy <= 1.0:
                    self.set(x, y, c)

    def elipse_fn(self, cx, cy, rx, ry, fn):
        """Elipse onde a cor é decidida por fn(x, y, nx, ny) (nx,ny em -1..1)."""
        for y in range(int(cy - ry - 1), int(cy + ry + 2)):
            for x in range(int(cx - rx - 1), int(cx + rx + 2)):
                nx = (x + 0.5 - cx) / max(rx, 0.01)
                ny = (y + 0.5 - cy) / max(ry, 0.01)
                if nx * nx + ny * ny <= 1.0:
                    c = fn(x, y, nx, ny)
                    if c is not None:
                        self.set(x, y, c)

    def poligono(self, pts, c):
        """Preenche polígono (lista de (x,y)) por scanline."""
        ys = [p[1] for p in pts]
        for y in range(int(min(ys)), int(max(ys)) + 1):
            yc = y + 0.5
            xs = []
            n = len(pts)
            for i in range(n):
                x0, y0 = pts[i]
                x1, y1 = pts[(i + 1) % n]
                if (y0 <= yc < y1) or (y1 <= yc < y0):
                    xs.append(x0 + (yc - y0) * (x1 - x0) / (y1 - y0))
            xs.sort()
            for i in range(0, len(xs) - 1, 2):
                for x in range(int(math.ceil(xs[i] - 0.5)), int(math.floor(xs[i + 1] - 0.5)) + 1):
                    self.set(x, y, c)

    def ascii(self, x0, y0, linhas, paleta):
        """Desenha arte ASCII. '.' ou ' ' = transparente."""
        for j, linha in enumerate(linhas):
            for i, ch in enumerate(linha):
                if ch in ('.', ' '):
                    continue
                if ch in paleta:
                    self.set(x0 + i, y0 + j, paleta[ch])

    # ------------------------------------------------------- operações
    def contorno(self, c, diagonais=False, limiar=128):
        """Adiciona contorno de 1px ao redor dos pixels opacos."""
        novos = []
        for y in range(self.h):
            for x in range(self.w):
                atual = self.p[y][x]
                if atual is not None and atual[3] >= limiar:
                    continue
                viz = [(1, 0), (-1, 0), (0, 1), (0, -1)]
                if diagonais:
                    viz += [(1, 1), (-1, -1), (1, -1), (-1, 1)]
                for dx, dy in viz:
                    v = self.get(x + dx, y + dy)
                    if v is not None and v[3] >= limiar and v != c:
                        novos.append((x, y))
                        break
        for x, y in novos:
            self.p[y][x] = c

    def colar(self, outra, x0, y0):
        for y in range(outra.h):
            for x in range(outra.w):
                c = outra.p[y][x]
                if c is not None:
                    self.set(x0 + x, y0 + y, c)

    def espelhar(self):
        n = Canvas(self.w, self.h)
        for y in range(self.h):
            n.p[y] = list(reversed(self.p[y]))
        return n

    def copia(self):
        n = Canvas(self.w, self.h)
        n.p = [list(l) for l in self.p]
        return n

    def deslocar(self, dx, dy):
        n = Canvas(self.w, self.h)
        for y in range(self.h):
            for x in range(self.w):
                c = self.p[y][x]
                if c is not None:
                    n.set(x + dx, y + dy, c)
        return n

    def trocar_cor(self, mapa):
        for y in range(self.h):
            for x in range(self.w):
                c = self.p[y][x]
                if c is not None and c in mapa:
                    self.p[y][x] = mapa[c]

    def tingir(self, fn):
        for y in range(self.h):
            for x in range(self.w):
                c = self.p[y][x]
                if c is not None:
                    self.p[y][x] = fn(c, x, y)

    def imagem(self):
        img = Image.new("RGBA", (self.w, self.h), (0, 0, 0, 0))
        px = img.load()
        for y in range(self.h):
            for x in range(self.w):
                c = self.p[y][x]
                if c is not None:
                    px[x, y] = c
        return img


def ruido_valor(seed):
    """Retorna uma função de ruído 1D suave (value noise) determinística."""
    rnd = random.Random(seed)
    tabela = [rnd.random() for _ in range(1024)]

    def f(x):
        i = int(math.floor(x))
        t = x - i
        a = tabela[i % 1024]
        b = tabela[(i + 1) % 1024]
        t = t * t * (3 - 2 * t)
        return a + (b - a) * t

    return f


def ruido_periodico(seed, periodo, oitavas=3):
    """Ruído 1D periódico (para fundos que repetem na horizontal)."""
    rnd = random.Random(seed)
    camadas = []
    for o in range(oitavas):
        n = max(2, int(periodo / (64 / (2 ** o))))
        camadas.append(([rnd.random() for _ in range(n)], n, 0.5 ** o))

    def f(x):
        total = 0.0
        peso = 0.0
        for tab, n, amp in camadas:
            u = (x / periodo) * n
            i = int(math.floor(u))
            t = u - i
            a = tab[i % n]
            b = tab[(i + 1) % n]
            t = t * t * (3 - 2 * t)
            total += (a + (b - a) * t) * amp
            peso += amp
        return total / peso

    return f
