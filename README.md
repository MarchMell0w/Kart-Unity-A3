# Turbo Kart

Jogo de kart em 3D feito na Unity para a A3 de Blender + Unity. O carro e a pista foram modelados no Blender e importados como FBX.

**Versão da Unity:** 6000.6.7f1 (URP 17.6)

## Como jogar

1. Abra a pasta do projeto no Unity Hub e abra a cena `Assets/Scenes/SampleScene.unity`.
2. Aperte Play. A tela inicial mostra o nome do jogo e o botão **START** (ou aperte ENTER).
3. Dirija com **W A S D** ou as **setas** do teclado.
4. Colete todas as moedas espalhadas pela pista. O cronômetro conta o tempo da partida e o contador mostra quantas moedas você já pegou.
5. Ao pegar a última moeda o tempo para e aparece a tela de fim de jogo, com o botão **JOGAR NOVAMENTE**.

## Estrutura do projeto

```
Assets/
  Art/
    Models/      modelos 3D do Blender (carros e pista) em FBX
    Materials/   materiais
  Input/         asset do Input System
  Scenes/        SampleScene (cena do jogo)
  Scripts/
    Gameplay/    KartController, CenterOfMassHelper, UnderBridgeIndicator
    Game/        GameManager, GameUI, Coin, CoinSpawner, CoinLayout
  Settings/      configurações do URP e perfis de volume
  TutorialInfo/  material do template URP (pode ser ignorado)
Packages/        pacotes do projeto (manifest.json)
ProjectSettings/ configurações do Unity
```

## Scripts

**Gameplay**

- `KartController`: movimento do kart com Rigidbody, curva, inclinação em rampas e gravidade.
- `CenterOfMassHelper`: ajusta o centro de massa e mostra gizmos (não está ligado a nenhum objeto na cena).
- `UnderBridgeIndicator`: marcador amarelo sobre o carro para vê-lo quando passa embaixo da ponte.

**Game**

- `GameManager`: estados do jogo (menu, jogando, fim), cronômetro e contagem de moedas. Fica no objeto `GameManager` da cena.
- `GameUI`: tela inicial, HUD (tempo e moedas) e tela final.
- `Coin`: moeda que gira e é coletada quando o kart encosta.
- `CoinSpawner`: cria as moedas na pista quando a cena começa.
- `CoinLayout`: lista com as posições das moedas. Para mudar o percurso, edite, adicione ou remova linhas dessa lista.
