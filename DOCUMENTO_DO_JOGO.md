# Rumo ao Hexa

**Jogo de plataforma 2D** feito em **Unity 6000.5.10f1** (URP 2D) para a atividade *"Plataforma — Vamos programar!!"*.
Toda a arte, a música e os efeitos sonoros são **originais**, gerados pelo próprio projeto: nenhum sprite foi baixado da internet.

---

## 1. História

Antes da grande final, os **argentinos roubaram as 5 estrelas da camisa amarelinha**.
O **craque canarinho**, de uniforme amarelo e calção azul, precisa recuperar as estrelas na **pelada da praia** e depois juntar as letras **H-E-X-A** na **final do Maracanã**, para o Brasil ser hexacampeão.

Os adversários usam o uniforme da Argentina (listras azul-celeste e branco, calção azul-marinho).
O craque não pisa em ninguém para vencer: a arma dele é a **bola**. Cada argentino atingido leva **cartão vermelho e some**.

## 2. Como jogar

As instruções também aparecem direto na **tela inicial**.

| Tecla (teclado)        | Gamepad          | Ação |
|------------------------|------------------|------|
| A / D  ou  ← / →       | Analógico / D-pad | Andar |
| Espaço / Z             | A (Sul)          | Pular (segurar = pulo mais alto) |
| J / X                  | X (Oeste)        | **Chute rasteiro**: quica no chão e ricocheteia nas paredes |
| ↑ + J  ou  K / C       | Y (Norte)        | **Cavadinha**: bola alta, por cima dos obstáculos |
| Segurar J até a barra encher | Segurar X  | **Folha seca**: chute forte com curva que atravessa vários argentinos |
| J com a bola longe     | X                | Chama a bola de volta para o pé |
| ↓ + Espaço             | Baixo + A        | Descer da plataforma |
| Esc / P                | Start            | Pausar |
| M                      | —                | Liga/desliga o som |

**Objetivo**
- **Fase 1:** recuperar as **5 estrelas** (cada uma tem um ano: 1958, 1962, 1970, 1994 e 2002).
- **Fase 2:** juntar as letras **H, E, X e A**.

Uma **seta amarela** perto do craque aponta para o item obrigatório mais próximo quando ele está longe.

## 3. Requisitos da atividade × o que foi feito

| Requisito | Como está no jogo |
|---|---|
| Jogo do tipo plataforma | Corrida, pulo com *coyote time*, *buffer* de pulo e pulo variável; plataformas atravessáveis por baixo e plataformas móveis |
| Tela inicial com orientações diretas de como jogar | Cena `Menu`: título, história em duas linhas, **tabela de controles** e dicas na própria tela, com os botões JOGAR e SAIR |
| Duas fases (cenas) | **Fase 1 – Pelada na Praia** (`Fase1`) e **Fase 2 – A Final no Maracanã** (`Fase2`) |
| Tratamento de colisão com as plataformas | Chão em Tilemap com **Tilemap Collider 2D + Composite Collider 2D + Rigidbody 2D (Static)**, igual ao passo a passo dos slides. Plataformas com **Platform Effector 2D** (atravessáveis por baixo), perigos com Box Collider 2D, mar e fosso com *trigger*. A bola também é um corpo físico e quica no chão e nas paredes |
| Fundo com parallax scrolling | Praia: 5 camadas (céu, nuvens com gaivotas, Pão de Açúcar e Cristo Redentor, mar com veleiros, orla com guarda-sóis). Estádio: céu com fogos, refletores, torcida animada, placas de publicidade e o **placar ao vivo**. Cada camada anda numa velocidade diferente em relação à câmera e se repete sem fim |
| Jogador, inimigos e itens com sprites animados | Craque: parado, correndo, pulo, queda, chute, carregando a folha seca, **embaixadinha**, dano e vitória. Argentinos: andar, arrancada, pulo, tonto e expulso. Itens: moeda girando, estrela brilhando, letras, coração, bandeirinha tremulando. Tiles animados: mar, fosso, ouriços e sinalizadores |
| Itens colecionáveis | Moedas (+10), coração (+1 vida), estrelas e letras (obrigatórias, +250) |
| Pontuação | Pontos no HUD, combos (DOBLETE +100, HAT-TRICK +300, GOLEADA +500), bônus de +1000 por fase e **recorde salvo** (PlayerPrefs) |
| Cena final informando vitória ou derrota, com botão para reiniciar | Cena `FimDeJogo`: **VITÓRIA!** (o craque ergue a taça sob confete) ou **DERROTA...**, com pontos, itens, moedas, argentinos expulsos, recorde e os botões **JOGAR NOVAMENTE** e **MENU PRINCIPAL** |
| Perde se os inimigos machucarem o jogador e as vidas chegarem a zero | 3 vidas (máximo 5). Encostar num argentino, num ouriço ou num sinalizador, ou cair no mar ou no fosso, tira 1 vida. Com 0 vidas: Derrota |
| Ganha se coletar todos os itens predeterminados | 5 estrelas na Fase 1 levam à Fase 2; as 4 letras H-E-X-A na Fase 2 dão a Vitória |

## 4. Personagens

- **Craque canarinho (jogador):** camisa amarela com gola verde, calção azul e meião branco. Parado com a bola no pé, ele **faz embaixadinhas**.
- **Zagueiro argentino:** forte e barbudo; patrulha a área e vira na parede ou na beirada.
- **Lateral argentino:** cabelo comprido com faixa; anda devagar, mas quando vê o craque na mesma altura dá uma **arrancada** na direção dele.
- **Cabeceador argentino:** cabelão crespo; vive **pulando para cabecear**. O chute rasteiro passa por baixo dele no alto do pulo; a cavadinha pega ele no ar.

### Mecânicas de futebol
- **A bola é de verdade:** um corpo físico que quica, rola e ricocheteia. Depois de um tempo (ou ao apertar J de novo) ela volta sozinha para o pé, e o craque também pode ir buscá-la encostando nela.
- **3 tipos de chute:** rasteiro (J), cavadinha (↑+J ou K) e **folha seca** (segurar J até a barra encher; a bola sai dourada, faz a curva e atravessa todos os argentinos no caminho).
- **Cartão vermelho:** o argentino atingido pela bola leva cartão vermelho, é empurrado e **some numa nuvem**. Só a bola expulsa: pular na cabeça só deixa o adversário **tonto** por alguns segundos.
- **Narrador e combos:** frases como "QUE CHUTAÇO!", "DE CAVADINHA!" e "FOLHA SECA! QUE ISSO!". Expulsar vários com a mesma bola dá DOBLETE, HAT-TRICK ou GOLEADA.
- **Placar ao vivo (Fase 2):** o telão do estádio marca **BRA x ARG**. Cada argentino expulso é gol do Brasil e cada vida perdida é gol da Argentina.
- **Bandeirinha de escanteio:** ponto de controle. Quando o craque passa, ela sobe verde e amarela; se ele cair no mar ou no fosso, volta para lá.

## 5. Fases

- **Fase 1 – Pelada na Praia** (152 × 18 tiles): areia, deck de madeira, **ouriços-do-mar**, mar com uma **prancha de surfe** que leva o craque de um lado para o outro, coqueiros, guarda-sóis, castelo de areia e trave de futebol de areia. Ao fundo: Pão de Açúcar com o bondinho e o Cristo Redentor.
- **Fase 2 – A Final no Maracanã** (166 × 20 tiles): à noite, com gramado listrado, **arquibancadas de concreto com cadeiras coloridas**, travessões como plataformas, **sinalizadores acesos** jogados no campo, o fosso do estádio, torcida pulando, fogos e o placar ao vivo. Usa **luzes 2D do URP**: luz global de estádio e uma luz que acompanha o craque.

Os mapas ficam em `Assets/Resources/Fases/Fase1.txt` e `Fase2.txt` e **podem ser editados em qualquer editor de texto** (a legenda está no começo de cada arquivo). O chão escolhe sozinho a borda certa (*auto-tiling* com 16 combinações de vizinhos).

## 6. Testes

São **3 grupos de testes** (Unity Test Framework / NUnit):

| Onde | O que testa |
|---|---|
| `Assets/Tests/EditMode/Logica` | Regras sem Unity: pontuação, vidas e recorde; os 3 chutes e a barra da folha seca; quando a bola volta e quando é dominada; combos; narrador; leitura dos mapas e **se todos os itens, corações, moedas e bandeirinhas são alcançáveis** com o pulo do craque; e se a fonte pixelada tem todas as letras dos textos |
| `Assets/Tests/EditMode/Unity` | Recursos do jogo dentro do Unity: todos os sprites e animações existem; as 4 cenas estão na Build na ordem certa; as camadas de física; a bola quica no chão mas não no jogador; todos os sons e músicas são gerados. E o **projeto 2D** (`TestesProjeto2D`): editor em modo 2D, URP com **2D Renderer**, câmeras ortográficas, nenhum componente 3D nas cenas, nenhuma física 3D no código, sprites importados como Sprite e a versão do URP igual à do editor 6000.5.10f1 |
| `Assets/Tests/PlayMode` | Jogando de verdade: o chute rasteiro expulsa e o argentino some; a folha seca atravessa dois e dá DOBLETE; pisar na cabeça só deixa tonto; encostar tira vida; a bola volta para o pé; moeda soma pontos; pegar as 5 estrelas leva à Fase 2; H-E-X-A dá vitória; perder as vidas dá derrota, e **JOGAR NOVAMENTE** reinicia; cair no mar volta para o início; o placar marca o gol; o menu tem as instruções e o botão JOGAR funciona; a pausa para o tempo |

**Como rodar no Unity:** menu **Window › General › Test Runner** (ou **Rumo ao Hexa › Rodar os testes**), abas *EditMode* e *PlayMode*, botão **Run All**.

**Sem abrir o Unity** (só as regras, com o .NET 8 SDK instalado), na raiz do projeto:

```
dotnet test Ferramentas/TestesSemUnity
```

## 7. Como abrir e rodar

O projeto é **2D** e foi feito para o **Unity 6000.5.10f1** (Unity 6.5):
- criado a partir do template oficial **Universal 2D** (`com.unity.template.universal-2d`);
- editor em **modo 2D** (*Project Settings › Editor › Default Behavior Mode = 2D*); ao abrir o projeto, a aba Scene também fica em 2D (atalho: **Rumo ao Hexa › Aba Scene em 2D**);
- renderização com o **URP 17.5.0** (a versão que vem com o 6000.5.10f1) usando o **2D Renderer** e as luzes 2D, definido em *Graphics* e em todos os níveis de *Quality*;
- câmeras **ortográficas**, sem skybox; física **2D** (Rigidbody 2D, Collider 2D, Tilemap Collider 2D); sprites e Tilemaps em pixel art (16 px por unidade, filtro Point).

Os módulos `com.unity.modules.*` listados em `Packages/manifest.json` (inclusive os de física 3D e terreno) fazem parte do template 2D do próprio Unity e são dependências de outros pacotes, como o URP. O jogo não usa nenhum deles.

1. Abra a pasta no **Unity Hub** com o editor **6000.5.10f1**.
2. A cena `Menu` abre automaticamente (ou use o menu **Rumo ao Hexa › Abrir cena Menu**).
3. Aperte **Play**. Também dá para dar Play direto em `Fase1` ou `Fase2`.
4. Para gerar um executável: *File › Build Profiles*. As 4 cenas já estão na Scene List (Menu, Fase1, Fase2, FimDeJogo).

## 8. Estrutura do projeto

```
Assets/
  Scenes/            Menu, Fase1, Fase2, FimDeJogo
  Resources/
    Fases/           mapas das fases em texto
    Sprites/         folhas de sprites PNG (16 px por unidade, filtro Point) + manifest.json
  Scripts/
    Logica/          regras puras, sem Unity (estado da partida, chutes, bola, mapas, narrador, textos)
    Nucleo/          Sessao, Entrada (Input System), Arte, Transicao, Constantes
    Fase/            ConstrutorFase (monta a fase), GerenciadorFase (regras), Placar, CameraSeguir, parallax...
    Jogador/         Jogador (movimento, chutes, embaixadinha) e Bola (física, volta para o pé, expulsões)
    Inimigos/        Inimigo (base), Zagueiro, Lateral, Cabeceador
    Itens/           Coletavel (moeda, coração, estrela, letra) e Bandeirinha
    Efeitos/         animações, partículas, cartão vermelho, confete, textos de pontos
    Interface/       HUD, menu, tela final, pausa, fonte pixelada própria
    Som/             Sintetizador (gera os sons e as músicas) e SomJogo
    Editor/          atalhos do menu "Rumo ao Hexa"
  Tests/             EditMode (Logica e Unity) e PlayMode
Ferramentas/
  gerar_arte.py      gera toda a arte (py Ferramentas/gerar_arte.py, precisa de Python + Pillow)
  TestesSemUnity/    projeto .NET que roda os testes das regras sem o Unity
```

**Detalhes técnicos**
- As cenas contêm a câmera, a Global Light 2D e um objeto principal. O `ConstrutorFase` cria, em tempo de execução, o **Grid com os Tilemaps**, o jogador e a bola, os argentinos, os itens, o fundo, o placar e o HUD a partir do mapa em texto. Na aba *Scene*, o formato da fase aparece como pré-visualização (Gizmos) mesmo sem dar Play.
- As **regras** ficam separadas em `Assets/Scripts/Logica` (assembly `RumoAoHexa.Logica`, sem dependência do Unity), por isso são testadas até fora do Unity.
- Animações por troca de quadros (`AnimadorSprite`) e tiles animados com um `TileBase` próprio (`TileAnimado`).
- **Input System** (novo), como o projeto está configurado; a UI é navegável por mouse, teclado e gamepad.
- Música e sons **sintetizados por código** (estilo 8 bits): chute, folha seca, cartão vermelho, combo, moeda e outros.
- Texto com **fonte pixelada própria** (com acentos do português), sem precisar de TextMeshPro.
- Camadas de física: Chao, Jogador, Inimigo, Item, Perigo, Plataforma e **Bola**.

## 9. Créditos

Arte, música, efeitos sonoros e código originais criados para este projeto.
Dupla: _[nome 1]_ e _[nome 2]_ (o texto de créditos da tela inicial pode ser editado no objeto **Menu Principal**, campo *Creditos*).
