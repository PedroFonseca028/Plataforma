# Curupira: Guardião da Mata

**Jogo de plataforma 2D** feito em **Unity 6000.5.10f1** (URP 2D) para a atividade *"Plataforma — Vamos programar!!"*.
Toda a arte, a música e os efeitos sonoros são **originais**, gerados pelo próprio projeto — não há sprites baixados de sites.

---

## 1. História

Na Mata Atlântica vive o **Curupira**, o menino de cabelos de fogo e **pés virados para trás** do folclore brasileiro, guardião das florestas.
As máquinas da **Madeireira Serra-Tudo** invadiram a mata e espalharam as **Sementes Encantadas** que mantêm a floresta viva.
O Curupira precisa recuperar todas as sementes: primeiro na mata ao entardecer e depois na área queimada, durante a noite — onde cada semente recuperada **faz a floresta renascer**.

## 2. Como jogar

| Tecla (teclado)            | Gamepad       | Ação |
|----------------------------|---------------|------|
| A / D  ou  ← / →           | Analógico / D-pad | Andar |
| Espaço / W / ↑ / Z         | A (Sul)       | Pular (segurar = pulo mais alto) |
| J / X / K / Shift          | X (Oeste) / Y | **Assobio mágico** |
| S + Espaço                 | Baixo + A     | Descer da plataforma de madeira |
| Esc / P                    | Start         | Pausar |
| M                          | —             | Liga/desliga o som |

**Objetivo:** coletar **todas as Sementes Encantadas** de cada fase (8 na Fase 1 e 10 na Fase 2).
Uma **seta dourada** perto do Curupira aponta para a semente mais próxima quando ela está longe.

## 3. Requisitos da atividade × o que foi feito

| Requisito | Como está no jogo |
|---|---|
| Jogo do tipo plataforma | Corrida, pulo com *coyote time*, *buffer* de pulo, pulo variável, plataformas atravessáveis e móveis |
| Duas fases | **Fase 1 – A Mata ao Entardecer** e **Fase 2 – A Noite da Queimada** (cenas `Fase1` e `Fase2`) |
| Tratamento de colisão com as plataformas | Tilemap do chão com **Tilemap Collider 2D + Composite Collider 2D + Rigidbody 2D (Static)** (igual ao slide), plataformas com **Platform Effector 2D** (one-way), espinhos e paredes com Box Collider 2D |
| Fundo com parallax scrolling | 5 camadas por fase (céu, nuvens/fumaça, serra, morros, floresta), cada uma com velocidade diferente em relação à câmera e repetição infinita |
| Jogador, inimigos e itens com sprites animados | Curupira (parado, correr, pulo, queda, dano, assobio, vitória), 3 inimigos (andar/voar, tonto, esmagado), itens com brilho e balanço, totens acesos, água/brasas animadas no Tilemap |
| Itens colecionáveis | Semente Encantada (obrigatória, +100), açaí (+10), caju (+25), guaraná (+50), coração (+1 vida) |
| Pontuação | Pontos no HUD, bônus de +500 por fase, **recorde salvo** (PlayerPrefs) |
| Cena final com vitória/derrota e botão para reiniciar | Cena `FimDeJogo`: mostra **VITÓRIA!** ou **DERROTA...**, pontos, sementes, recorde e os botões **JOGAR NOVAMENTE** e **MENU PRINCIPAL** |
| Perde se os inimigos machucarem e as vidas chegarem a zero | 3 vidas (máx. 5). Encostar em inimigo/espinho ou cair no rio/brasas tira 1 vida. Com 0 vidas → Derrota |
| Ganha se coletar todos os itens predeterminados | Coletou todas as sementes da Fase 1 → Fase 2; todas da Fase 2 → Vitória |

## 4. Personagens e inimigos

- **Curupira (jogador):** cabelo de fogo animado e pés para trás — ao correr ele deixa **pegadas apontando para o lado contrário**, como na lenda, que diz que ele engana os caçadores assim.
- **Serra-Serra:** motosserra-robô que patrulha e dá meia-volta nas paredes e beiradas. Pode ser pisada (+100).
- **Drone Madeireiro:** voa de um lado para o outro balançando. Pode ser pisado (+150).
- **Fagulha (Fase 2):** brasa viva que salta sem parar. **Queima até se for pisada** — primeiro é preciso deixá-la tonta com o assobio; aí dá para apagá-la (+200).

### Mecânica especial: o Assobio do Curupira
Na lenda, o Curupira assobia para confundir quem invade a mata. No jogo, o assobio solta uma onda sonora que deixa **todas as máquinas próximas tontas por 3,5 s** (param, ficam inofensivas e podem ser pisadas). Recarga de 4 s, mostrada no ícone do canto inferior esquerdo.

### A mata renasce
Cada semente recuperada faz nascer um broto. Na **Fase 2**, o chão queimado em volta volta a ficar **verde e florido** em ondas; ao pegar a última semente, a fase inteira renasce.

## 5. Fases

- **Fase 1 – A Mata ao Entardecer** (160 × 18 tiles): céu de pôr do sol, serra, araucárias e ipês. Tem rio (cair = perder vida), espinheiros, plataforma móvel sobre o rio e 2 totens de checkpoint.
- **Fase 2 – A Noite da Queimada** (170 × 20 tiles): noite com lua e estrelas, árvores carbonizadas e poços de brasas. Usa **luzes 2D do URP**: a luz global fica azulada e fraca, o cabelo do Curupira ilumina ao redor e as sementes brilham.

Os mapas ficam em `Assets/Resources/Fases/Fase1.txt` e `Fase2.txt` e **podem ser editados em qualquer editor de texto** (a legenda está no começo de cada arquivo). O chão escolhe sozinho a borda correta (*auto-tiling* com 16 combinações de vizinhos).

## 6. Como abrir e rodar

1. Abra a pasta no **Unity Hub** com o editor **6000.5.10f1**.
2. A cena `Menu` abre automaticamente (ou use o menu **Curupira › Abrir cena Menu**).
3. Aperte **Play**. Também dá para dar Play direto em `Fase1` ou `Fase2`.
4. Para gerar um executável: *File › Build Profiles* — as 4 cenas já estão na Scene List (Menu, Fase1, Fase2, FimDeJogo).

## 7. Estrutura do projeto

```
Assets/
  Scenes/            Menu, Fase1, Fase2, FimDeJogo
  Resources/
    Fases/           mapas das fases em texto
    Sprites/         folhas de sprites PNG (fatiadas, 16 px por unidade, filtro Point) + manifest.json
  Scripts/
    Nucleo/          Sessao (pontos/vidas entre cenas), Entrada (Input System), Arte, Transicao, Constantes
    Fase/            ConstrutorFase (monta a fase), GerenciadorFase (regras), CameraSeguir, CamadaParallax, Fundo...
    Jogador/         Jogador (movimento, pulo, assobio, dano)
    Inimigos/        Inimigo (base), SerraSerra, Drone, Fagulha
    Itens/           Coletavel, Totem
    Efeitos/         AnimadorSprite, partículas, textos de pontos, anel do assobio
    Interface/       HUD, menus, fonte pixelada própria (TextoPixel/FontePixel)
    Som/             Sintetizador (gera sons e músicas) e SomJogo
    Editor/          atalhos do menu "Curupira"
Ferramentas/         scripts Python que geraram a arte (gerar_arte.py)
```

**Detalhes técnicos**
- As cenas contêm a câmera, a Global Light 2D e um objeto principal; o `ConstrutorFase` cria em tempo de execução o **Grid com os Tilemaps**, jogador, inimigos, itens, fundo e HUD a partir do mapa em texto. Na aba *Scene*, o formato da fase aparece como pré-visualização (Gizmos) mesmo sem dar Play.
- Animações por troca de quadros (`AnimadorSprite`); tiles animados com um `TileBase` próprio (`TileAnimado`).
- **Input System** (novo), como o projeto está configurado; UI com `InputSystemUIInputModule`, navegável por mouse, teclado e gamepad.
- Música e sons **sintetizados por código** (estilo 8 bits). A trilha da Fase 1 é um **baião** (zabumba, triângulo e baixo); a da Fase 2 é misteriosa, em Ré menor.
- Texto com **fonte pixelada própria** (com acentos do português), sem precisar de TextMeshPro.
- Camadas de física: Chao, Jogador, Inimigo, Item, Perigo, Plataforma.
- Para mudar a arte: edite os PNGs em `Assets/Resources/Sprites` ou rode `py Ferramentas/gerar_arte.py` (precisa de Python + Pillow).

## 8. Créditos

Arte, música, efeitos sonoros e código originais criados para este projeto.
Dupla: _[nome 1]_ e _[nome 2]_ (o texto de créditos do menu pode ser editado no objeto **Menu Principal**, campo *Creditos*).
