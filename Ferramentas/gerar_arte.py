# -*- coding: utf-8 -*-
"""
Gerador da arte original do jogo "Curupira: Guardião da Mata".

Gera, dentro de Assets/Resources/Sprites:
  * as folhas de sprites (PNG),
  * os arquivos .meta do Unity (Sprite Mode = Multiple, 16 pixels por
    unidade, filtro Point, sem compressão) já fatiados,
  * o manifest.json com a posição/pivô de cada sprite (usado pelo jogo
    para montar os sprites em tempo de execução).

Uso (na raiz do projeto):  py Ferramentas/gerar_arte.py
"""
import hashlib
import json
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import arte_cenario as ac      # noqa: E402
import arte_itens as ai        # noqa: E402
import arte_personagens as ap  # noqa: E402

RAIZ = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SAIDA = os.path.join(RAIZ, "Assets", "Resources", "Sprites")
PPU = 16


def guid_de(texto):
    """GUID determinístico (32 hex) para o .meta não mudar entre execuções."""
    return hashlib.md5(("curupira:" + texto).encode("utf-8")).hexdigest()


def id_interno(nome_folha, nome_sprite):
    """fileID determinístico (positivo, 63 bits) para cada sprite fatiado."""
    h = hashlib.md5((nome_folha + "/" + nome_sprite).encode("utf-8")).digest()
    return int.from_bytes(h[:8], "little") & 0x3FFFFFFFFFFFFFFF


MARGEM = 1  # pixels extras (copiados da borda) em volta de cada sprite: evita "linhas" entre tiles


def empacotar(sprites, largura_max=256):
    """
    Empacota sprites de tamanhos variados em prateleiras. Cada sprite ganha
    MARGEM px de cada lado. Devolve a posição (canto sup. esquerdo do
    conteúdo) de cada sprite e o tamanho final da textura (potência de 2).
    """
    x = y = 0
    altura_linha = 0
    pos = []
    largura_usada = 0
    for nome, canvas, pivo, borda in sprites:
        w = canvas.w + 2 * MARGEM
        h = canvas.h + 2 * MARGEM
        if x + w > largura_max:
            x = 0
            y += altura_linha
            altura_linha = 0
        pos.append((x + MARGEM, y + MARGEM))
        x += w
        largura_usada = max(largura_usada, x)
        altura_linha = max(altura_linha, h)
    altura = y + altura_linha

    def pot(v):
        p = 1
        while p < v:
            p *= 2
        return p

    return pos, pot(largura_usada), pot(altura)


def meta_textura(guid, nome_folha, sprites_info, largura, altura, multiplo=True, pivo_unico=(0.5, 0.5),
                 borda_unica=(0, 0, 0, 0)):
    """Gera o texto do .meta (TextureImporter) no formato do Unity 6."""
    linhas = []
    add = linhas.append
    add("fileFormatVersion: 2")
    add("guid: %s" % guid)
    add("TextureImporter:")
    add("  internalIDToNameTable: []")
    add("  externalObjects: {}")
    add("  serializedVersion: 13")
    add("  mipmaps:")
    add("    mipMapMode: 0")
    add("    enableMipMap: 0")
    add("    sRGBTexture: 1")
    add("    linearTexture: 0")
    add("    fadeOut: 0")
    add("    borderMipMap: 0")
    add("    mipMapsPreserveCoverage: 0")
    add("    alphaTestReferenceValue: 0.5")
    add("    mipMapFadeDistanceStart: 1")
    add("    mipMapFadeDistanceEnd: 3")
    add("  bumpmap:")
    add("    convertToNormalMap: 0")
    add("    externalNormalMap: 0")
    add("    heightScale: 0.25")
    add("    normalMapFilter: 0")
    add("    flipGreenChannel: 0")
    add("  isReadable: 0")
    add("  streamingMipmaps: 0")
    add("  streamingMipmapsPriority: 0")
    add("  vTOnly: 0")
    add("  ignoreMipmapLimit: 0")
    add("  grayScaleToAlpha: 0")
    add("  generateCubemap: 6")
    add("  cubemapConvolution: 0")
    add("  seamlessCubemap: 0")
    add("  textureFormat: 1")
    add("  maxTextureSize: 2048")
    add("  textureSettings:")
    add("    serializedVersion: 2")
    add("    filterMode: 0")
    add("    aniso: 1")
    add("    mipBias: 0")
    add("    wrapU: 1")
    add("    wrapV: 1")
    add("    wrapW: 1")
    add("  nPOTScale: 0")
    add("  lightmap: 0")
    add("  compressionQuality: 50")
    add("  spriteMode: %d" % (2 if multiplo else 1))
    add("  spriteExtrude: 1")
    add("  spriteMeshType: 0")
    add("  alignment: 9")
    add("  spritePivot: {x: %s, y: %s}" % (_f(pivo_unico[0]), _f(pivo_unico[1])))
    add("  spritePixelsToUnits: %d" % PPU)
    add("  spriteBorder: {x: %d, y: %d, z: %d, w: %d}" % borda_unica)
    add("  spriteGenerateFallbackPhysicsShape: 1")
    add("  alphaUsage: 1")
    add("  alphaIsTransparency: 1")
    add("  spriteTessellationDetail: -1")
    add("  textureType: 8")
    add("  textureShape: 1")
    add("  singleChannelComponent: 0")
    add("  flipbookRows: 1")
    add("  flipbookColumns: 1")
    add("  maxTextureSizeSet: 0")
    add("  compressionQualitySet: 0")
    add("  textureFormatSet: 0")
    add("  ignorePngGamma: 0")
    add("  applyGammaDecoding: 0")
    add("  swizzle: 50462976")
    add("  cookieLightType: 0")
    add("  platformSettings:")
    for alvo in ("DefaultTexturePlatform", "Standalone", "WebGL", "Android"):
        add("  - serializedVersion: 4")
        add("    buildTarget: %s" % alvo)
        add("    maxTextureSize: 2048")
        add("    resizeAlgorithm: 0")
        add("    textureFormat: -1")
        add("    textureCompression: 0")
        add("    compressionQuality: 50")
        add("    crunchedCompression: 0")
        add("    allowsAlphaSplitting: 0")
        add("    overridden: 0")
        add("    ignorePlatformSupport: 0")
        add("    androidETC2FallbackOverride: 0")
        add("    forceMaximumCompressionQuality_BC6H_BC7: 0")
    add("  spriteSheet:")
    add("    serializedVersion: 2")
    if multiplo:
        add("    sprites:")
        for info in sprites_info:
            add("    - serializedVersion: 2")
            add("      name: %s" % info["nome"])
            add("      rect:")
            add("        serializedVersion: 2")
            add("        x: %d" % info["x"])
            add("        y: %d" % info["y_unity"])
            add("        width: %d" % info["w"])
            add("        height: %d" % info["h"])
            add("      alignment: 9")
            add("      pivot: {x: %s, y: %s}" % (_f(info["px"]), _f(info["py"])))
            add("      border: {x: %d, y: %d, z: %d, w: %d}" % tuple(info["borda"]))
            add("      customData: ")
            add("      outline: []")
            add("      physicsShape: []")
            add("      tessellationDetail: -1")
            add("      bones: []")
            add("      spriteID: %s" % guid_de(nome_folha + "#" + info["nome"]))
            add("      internalID: %d" % info["id"])
            add("      vertices: []")
            add("      indices: ")
            add("      edges: []")
            add("      weights: []")
    else:
        add("    sprites: []")
    add("    outline: []")
    add("    customData: ")
    add("    physicsShape: []")
    add("    bones: []")
    add("    spriteID: %s" % (guid_de(nome_folha + "#main") if not multiplo else ""))
    add("    internalID: 0")
    add("    vertices: []")
    add("    indices: ")
    add("    edges: []")
    add("    weights: []")
    add("    secondaryTextures: []")
    add("    spriteCustomMetadata:")
    add("      entries: []")
    add("    nameFileIdTable:")
    if multiplo and sprites_info:
        for info in sprites_info:
            add("      %s: %d" % (info["nome"], info["id"]))
    else:
        linhas[-1] = "    nameFileIdTable: {}"
    add("  mipmapLimitGroupName: ")
    add("  pSDRemoveMatte: 0")
    add("  userData: ")
    add("  assetBundleName: ")
    add("  assetBundleVariant: ")
    return "\n".join(linhas) + "\n"


def _f(v):
    s = ("%.4f" % v).rstrip("0").rstrip(".")
    return s if s else "0"


def meta_pasta(guid):
    return ("fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
            "  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % guid)


def meta_texto(guid):
    return ("fileFormatVersion: 2\nguid: %s\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n"
            "  assetBundleName: \n  assetBundleVariant: \n" % guid)


def salvar_folha(nome_folha, sprites, manifest, largura_max=256):
    """sprites: lista de (nome, canvas, (pivoX, pivoY), (bordaL, bordaB, bordaR, bordaT))."""
    from PIL import Image
    pos, W, H = empacotar(sprites, largura_max)
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    infos = []
    for (nome, canvas, pivo, borda), (x, y) in zip(sprites, pos):
        im = canvas.imagem()
        # extrusão: repete as bordas do sprite na margem
        if MARGEM > 0:
            ext = Image.new("RGBA", (canvas.w + 2 * MARGEM, canvas.h + 2 * MARGEM), (0, 0, 0, 0))
            ext.paste(im, (MARGEM, MARGEM))
            for m in range(MARGEM):
                ext.paste(im.crop((0, 0, canvas.w, 1)), (MARGEM, m))
                ext.paste(im.crop((0, canvas.h - 1, canvas.w, canvas.h)), (MARGEM, MARGEM + canvas.h + m))
            for m in range(MARGEM):
                ext.paste(ext.crop((MARGEM, 0, MARGEM + 1, ext.height)), (m, 0))
                ext.paste(ext.crop((MARGEM + canvas.w - 1, 0, MARGEM + canvas.w, ext.height)), (MARGEM + canvas.w + m, 0))
            img.paste(ext, (x - MARGEM, y - MARGEM))
        else:
            img.alpha_composite(im, (x, y))
        infos.append({
            "nome": nome, "x": x, "y_unity": H - y - canvas.h, "w": canvas.w, "h": canvas.h,
            "px": pivo[0], "py": pivo[1], "borda": list(borda), "id": id_interno(nome_folha, nome),
        })
    caminho = os.path.join(SAIDA, nome_folha + ".png")
    img.save(caminho)
    with open(caminho + ".meta", "w", encoding="utf-8", newline="\n") as f:
        f.write(meta_textura(guid_de("Sprites/" + nome_folha + ".png"), nome_folha, infos, W, H, True))
    manifest["folhas"].append({
        "folha": nome_folha,
        "sprites": [{"n": i["nome"], "x": i["x"], "y": i["y_unity"], "w": i["w"], "h": i["h"],
                     "px": i["px"], "py": i["py"], "b": i["borda"]} for i in infos],
    })
    print("  %-22s %4dx%-4d %3d sprites" % (nome_folha + ".png", W, H, len(infos)))


def salvar_imagem_unica(nome, canvas, manifest, pivo=(0.5, 0.5)):
    """Imagens grandes (fundos do parallax): Sprite Mode = Single."""
    img = canvas.imagem()
    caminho = os.path.join(SAIDA, nome + ".png")
    img.save(caminho)
    with open(caminho + ".meta", "w", encoding="utf-8", newline="\n") as f:
        f.write(meta_textura(guid_de("Sprites/" + nome + ".png"), nome, [], canvas.w, canvas.h, False, pivo))
    manifest["folhas"].append({
        "folha": nome,
        "sprites": [{"n": nome, "x": 0, "y": 0, "w": canvas.w, "h": canvas.h,
                     "px": pivo[0], "py": pivo[1], "b": [0, 0, 0, 0]}],
    })
    print("  %-22s %4dx%-4d (fundo)" % (nome + ".png", canvas.w, canvas.h))


def main():
    os.makedirs(SAIDA, exist_ok=True)
    # metas das pastas Resources e Resources/Sprites
    for pasta in ("Assets/Resources", "Assets/Resources/Sprites"):
        caminho = os.path.join(RAIZ, *pasta.split("/")) + ".meta"
        if not os.path.exists(caminho):
            with open(caminho, "w", encoding="utf-8", newline="\n") as f:
                f.write(meta_pasta(guid_de(pasta)))

    manifest = {"ppu": PPU, "folhas": []}
    pe = (0.5, 0.0)       # pivô nos pés (personagens)
    centro = (0.5, 0.5)
    sem = (0, 0, 0, 0)

    print("Gerando folhas de sprites...")
    salvar_folha("curupira", [(n, c, pe, sem) for n, c in ap.quadros_curupira()], manifest)
    salvar_folha("inimigos", [(n, c, pe, sem) for n, c in ap.quadros_inimigos()], manifest)

    itens = [(n, c, centro, sem) for n, c in ai.quadros_itens()]
    itens += [(n, c, centro, sem) for n, c in ai.quadros_pequenos()]
    itens += [('brasa_%d' % i, ai.brasa(i), centro, sem) for i in range(2)]
    itens += [(n, c, (0.5, 0.0), sem) for n, c in ai.quadros_totem()]
    itens += [('anel', ai.anel(), centro, sem), ('luz_suave', ai.luz_suave(), centro, sem)]
    salvar_folha("itens", itens, manifest)

    for tema in ("mata", "queimada"):
        t = [(n, c, centro, sem) for n, c in ac.tiles_chao(tema)]
        t += [(n, c, centro, sem) for n, c in ac.tiles_plataforma(tema)]
        t += [(n, c, centro, sem) for n, c in ac.tiles_espinho(tema)]
        t += [(n, c, centro, sem) for n, c in ac.tiles_liquido(tema)]
        deco = ac.deco_mata() if tema == "mata" else ac.deco_queimada()
        t += [(n, c, centro, sem) for n, c in deco]
        salvar_folha("tiles_" + tema, t, manifest)

    salvar_folha("arvores", [(n, c, (0.5, 0.0), sem) for n, c in ac.quadros_arvores()], manifest)

    ui = [
        ('ui_coracao_cheio', ai.ui_coracao(True), centro, sem),
        ('ui_coracao_vazio', ai.ui_coracao(False), centro, sem),
        ('ui_semente', ai.ui_semente(), centro, sem),
        ('ui_assobio', ai.ui_assobio(), centro, sem),
        ('ui_botao', ai.ui_botao(), centro, (6, 6, 6, 6)),
        ('ui_painel', ai.ui_painel(), centro, (12, 12, 12, 12)),
        ('ui_tecla', ai.ui_tecla(), centro, (5, 5, 5, 5)),
        ('ui_branco', ai.ui_branco(), centro, (1, 1, 1, 1)),
    ]
    salvar_folha("ui", ui, manifest)

    for nome, canvas in ac.fundos():
        salvar_imagem_unica(nome, canvas, manifest)

    caminho = os.path.join(SAIDA, "manifest.json")
    with open(caminho, "w", encoding="utf-8", newline="\n") as f:
        json.dump(manifest, f, ensure_ascii=False, separators=(",", ":"))
    if not os.path.exists(caminho + ".meta"):
        with open(caminho + ".meta", "w", encoding="utf-8", newline="\n") as f:
            f.write(meta_texto(guid_de("Sprites/manifest.json")))
    total = sum(len(f["sprites"]) for f in manifest["folhas"])
    print("Pronto: %d folhas, %d sprites." % (len(manifest["folhas"]), total))


if __name__ == "__main__":
    main()
