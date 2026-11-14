using UnityEngine;

/// <summary>
/// Cria as moedas ao longo da pista a partir da lista do CoinLayout.
///
/// Os pontos do CoinLayout estão no espaço da malha da pista (Blender). Para levar cada ponto
/// até o mundo da Unity, o spawner descobre sozinho como o modelo foi orientado na importação:
/// ele testa as combinações possíveis de eixos e escolhe a que faz os raycasts acertarem o
/// asfalto na altura esperada. Depois disso cada moeda é encostada no chão com um raycast.
/// </summary>
public static class CoinSpawner
{
    // Nome do objeto raiz da pista na cena (prefab do pista_corrida.fbx).
    private const string TrackRootName = "pista_corrida";

    // Posição da pista na cena, usada só se o objeto acima não for encontrado.
    private static readonly Vector3 FallbackTrackPosition = new Vector3(6.4952626f, 0.17996472f, 0.98196036f);

    // Cada opção é (sinalX, sinalY): mundo = raiz.TransformPoint(sinalX * x, z, sinalY * y).
    // A primeira é o resultado esperado de um FBX do Blender (Z para cima) importado na Unity.
    private static readonly Vector2[] AxisOptions =
    {
        new Vector2(-1f, -1f),
        new Vector2( 1f,  1f),
        new Vector2( 1f, -1f),
        new Vector2(-1f,  1f)
    };

    private const float RayStartHeight = 2f;   // o raycast começa um pouco acima do ponto
    private const float RayLength = 6f;
    private const float HoverHeight = 0.6f;    // altura da moeda em relação ao asfalto
    private const float TriggerRadius = 0.9f;  // raio de coleta (o kart tem cerca de 1,6 x 1,0 m)
    private const float ProbeTolerance = 0.02f; // diferença máxima de altura na calibração (metros)

    private static Material coinMaterial;

    /// <summary>Cria todas as moedas e devolve quantas foram colocadas.</summary>
    public static int SpawnAll(Transform parent)
    {
        Matrix4x4 trackToWorld = GetTrackMatrix();
        Vector2 axes = DetectAxes(trackToWorld, out int bestScore);

        // Se nenhum raycast acertou a pista (colliders desligados, por exemplo),
        // as moedas são colocadas na altura calculada, sem encostar no chão.
        bool useGroundRaycast = bestScore > 0;
        if (!useGroundRaycast)
            Debug.LogWarning("CoinSpawner: não encontrei o collider da pista. Moedas colocadas sem raycast.");

        int count = 0;
        foreach (Vector3 meshPoint in CoinLayout.MeshPoints)
        {
            Vector3 world = ToWorld(trackToWorld, axes, meshPoint);

            if (useGroundRaycast)
            {
                if (!TryFindGround(world + Vector3.up * RayStartHeight, out RaycastHit hit))
                    continue; // ponto fora da pista, ignora
                world = hit.point;
            }

            count++;
            CreateCoin(parent, world + Vector3.up * HoverHeight, count);
        }

        return count;
    }

    // ---------------------------------------------------------------- conversão de coordenadas

    private static Matrix4x4 GetTrackMatrix()
    {
        GameObject root = GameObject.Find(TrackRootName);
        return root != null ? root.transform.localToWorldMatrix : Matrix4x4.Translate(FallbackTrackPosition);
    }

    private static Vector3 ToWorld(Matrix4x4 trackToWorld, Vector2 axes, Vector3 meshPoint)
    {
        return trackToWorld.MultiplyPoint3x4(new Vector3(axes.x * meshPoint.x, meshPoint.z, axes.y * meshPoint.y));
    }

    private static Vector2 DetectAxes(Matrix4x4 trackToWorld, out int bestScore)
    {
        Vector2 best = AxisOptions[0];
        bestScore = -1;

        foreach (Vector2 option in AxisOptions)
        {
            int score = ScoreAxes(trackToWorld, option);
            if (score > bestScore)
            {
                bestScore = score;
                best = option;
            }
        }

        return best;
    }

    // Quantos pontos de calibração acertam o asfalto na altura exata do modelo.
    // Só a orientação correta acerta todos; as outras acertam zero ou quase.
    private static int ScoreAxes(Matrix4x4 trackToWorld, Vector2 axes)
    {
        int score = 0;

        foreach (Vector3 probe in CoinLayout.ProbePoints)
        {
            Vector3 world = ToWorld(trackToWorld, axes, probe);
            if (!TryFindGround(world + Vector3.up * RayStartHeight, out RaycastHit hit))
                continue;

            if (Mathf.Abs(hit.point.y - world.y) <= ProbeTolerance)
                score++;
        }

        return score;
    }

    // ---------------------------------------------------------------- raycast

    // Acha a superfície mais alta abaixo de "from", ignorando triggers, o kart e outras moedas.
    private static bool TryFindGround(Vector3 from, out RaycastHit result)
    {
        result = default;
        bool found = false;
        float closest = float.MaxValue;

        RaycastHit[] hits = Physics.RaycastAll(from, Vector3.down, RayLength, ~0, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.GetComponentInParent<KartController>() != null) continue;
            if (hit.collider.GetComponentInParent<Coin>() != null) continue;

            if (hit.distance < closest)
            {
                closest = hit.distance;
                result = hit;
                found = true;
            }
        }

        return found;
    }

    // ---------------------------------------------------------------- criação da moeda

    private static void CreateCoin(Transform parent, Vector3 position, int index)
    {
        GameObject root = new GameObject("Moeda_" + index.ToString("00"));
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        SphereCollider trigger = root.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = TriggerRadius;

        // Disco dourado em pé (cilindro deitado de lado, girando em volta do eixo Y).
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        Object.Destroy(visual.GetComponent<Collider>()); // só o trigger da raiz deve existir
        visual.transform.SetParent(root.transform, false);
        visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        visual.transform.localScale = new Vector3(0.7f, 0.05f, 0.7f);
        visual.GetComponent<Renderer>().sharedMaterial = GetCoinMaterial(visual.GetComponent<Renderer>());

        Coin coin = root.AddComponent<Coin>();
        coin.Init(visual.transform);
    }

    // Um material só para todas as moedas (a base vem do material padrão do render pipeline).
    private static Material GetCoinMaterial(Renderer baseRenderer)
    {
        if (coinMaterial != null) return coinMaterial;

        coinMaterial = new Material(baseRenderer.sharedMaterial) { name = "Moeda (runtime)" };
        Color gold = new Color(1f, 0.82f, 0.1f);

        if (coinMaterial.HasProperty("_BaseColor")) coinMaterial.SetColor("_BaseColor", gold);
        if (coinMaterial.HasProperty("_Color")) coinMaterial.SetColor("_Color", gold);
        if (coinMaterial.HasProperty("_Metallic")) coinMaterial.SetFloat("_Metallic", 0.85f);
        if (coinMaterial.HasProperty("_Smoothness")) coinMaterial.SetFloat("_Smoothness", 0.75f);

        if (coinMaterial.HasProperty("_EmissionColor"))
        {
            coinMaterial.EnableKeyword("_EMISSION");
            coinMaterial.SetColor("_EmissionColor", gold * 0.35f);
        }

        return coinMaterial;
    }
}
