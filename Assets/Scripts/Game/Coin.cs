using UnityEngine;

/// <summary>
/// Moeda coletável. Gira e flutua no lugar, e é coletada quando o kart encosta nela.
/// O collider da moeda é um trigger, então o kart passa direto sem ser empurrado.
/// </summary>
public class Coin : MonoBehaviour
{
    [Header("Animação")]
    public float spinSpeed = 140f;   // graus por segundo
    public float bobHeight = 0.12f;  // quanto ela sobe e desce
    public float bobSpeed = 2.5f;

    private Transform visual;
    private float phase;
    private bool collected;

    /// <summary>Define qual filho é a parte visual (a que flutua).</summary>
    public void Init(Transform visualTransform)
    {
        visual = visualTransform;
        phase = Random.value * Mathf.PI * 2f; // moedas não ficam todas sincronizadas
    }

    void Update()
    {
        // Tempo sem escala: as moedas continuam animando na tela inicial (jogo congelado).
        transform.Rotate(0f, spinSpeed * Time.unscaledDeltaTime, 0f, Space.World);

        if (visual != null)
        {
            float y = Mathf.Sin(Time.unscaledTime * bobSpeed + phase) * bobHeight;
            visual.localPosition = new Vector3(0f, y, 0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (other.GetComponentInParent<KartController>() == null) return;

        collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.CollectCoin();

        Destroy(gameObject);
    }
}
