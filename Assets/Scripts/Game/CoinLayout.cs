using UnityEngine;

/// <summary>
/// Posições das moedas ao longo da pista.
///
/// Cada ponto está nas coordenadas da malha "pista_corrida.fbx" (o mesmo espaço do Blender):
///   x e y = posição no plano da pista (vista de cima), z = altura em relação ao chão.
/// O CoinSpawner converte esses pontos para o mundo da Unity e encosta cada moeda no asfalto
/// usando raycast, então pequenas diferenças de altura são corrigidas automaticamente.
///
/// Para mudar o desenho do percurso basta editar, adicionar ou remover linhas desta lista.
/// </summary>
public static class CoinLayout
{
    public static readonly Vector3[] MeshPoints =
    {
        new Vector3(-12.425f, -1.225f, 0.002f),
        new Vector3(-8.800f, -1.950f, 0.405f),
        new Vector3(-5.280f, -1.950f, 1.922f),
        new Vector3(-9.000f, -5.950f, 0.186f),
        new Vector3(-1.760f, -1.950f, 2.400f),
        new Vector3(-12.075f, -10.325f, 0.000f),
        new Vector3(-6.100f, -5.950f, 0.859f),
        new Vector3(-8.275f, -10.975f, 0.000f),
        new Vector3(-3.200f, -5.950f, 1.732f),
        new Vector3(0.000f, -6.000f, 0.000f),
        new Vector3(1.760f, -1.950f, 2.400f),
        new Vector3(3.200f, -5.950f, 1.717f),
        new Vector3(6.100f, -5.950f, 0.845f),
        new Vector3(9.000f, -5.950f, 0.177f),
        new Vector3(5.280f, -1.950f, 1.903f),
        new Vector3(12.775f, -5.875f, 0.000f),
        new Vector3(16.475f, -4.875f, 0.000f),
        new Vector3(8.800f, -1.950f, 0.377f),
        new Vector3(17.875f, -1.325f, 0.000f),
        new Vector3(12.225f, -0.675f, 0.000f),
        new Vector3(17.975f, 2.475f, 0.000f),
        new Vector3(12.475f, 3.125f, 0.000f),
        new Vector3(17.875f, 6.275f, 0.000f),
        new Vector3(16.875f, 9.975f, 0.000f),
        new Vector3(9.025f, 4.875f, 0.000f),
        new Vector3(13.375f, 11.725f, 0.000f),
        new Vector3(5.075f, 4.925f, 0.000f),
        new Vector3(8.375f, 11.975f, 0.000f),
        new Vector3(1.325f, 4.175f, 0.000f),
        new Vector3(-0.325f, 11.975f, 0.000f),
        new Vector3(-11.125f, 11.625f, 0.000f),
        new Vector3(-14.225f, 9.425f, 0.000f),
        new Vector3(-14.975f, 5.675f, 0.000f),
        new Vector3(-14.675f, 1.875f, 0.000f),
        new Vector3(0.075f, 0.525f, 0.000f),
    };

    /// <summary>
    /// Pontos de calibração, usados só pelo CoinSpawner para descobrir como o modelo da pista
    /// foi orientado na Unity. Cada ponto está sobre o asfalto e, ao mesmo tempo, fora dele se
    /// os eixos forem espelhados ou girados, então só a orientação correta acerta todos.
    /// </summary>
    public static readonly Vector3[] ProbePoints =
    {
        new Vector3(4.175f, 5.325f, 0f),
        new Vector3(19.275f, 6.975f, 0f),
        new Vector3(15.675f, 12.525f, 0f),
        new Vector3(10.425f, 4.125f, 0f),
        new Vector3(7.625f, 6.275f, 0f),
        new Vector3(18.275f, 10.225f, 0f),
        new Vector3(6.475f, 3.725f, 0f),
        new Vector3(2.575f, 3.625f, 0f),
        new Vector3(8.425f, 4.375f, 0f),
    };
}
