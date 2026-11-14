using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,      // tela inicial, jogo congelado
    Playing,   // corrida rolando, cronômetro contando
    Finished   // todas as moedas coletadas
}

/// <summary>
/// Controla o fluxo da partida: tela inicial, cronômetro, moedas e fim de jogo.
/// Fica em um único GameObject ("GameManager") dentro da cena.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Moedas")]
    [Tooltip("Cria as moedas automaticamente ao longo da pista quando a cena começa.")]
    public bool spawnCoins = true;

    public GameState State { get; private set; } = GameState.Menu;
    public float ElapsedTime { get; private set; }
    public int CoinsCollected { get; private set; }
    public int TotalCoins { get; private set; }

    // Quando o jogador clica em "Jogar novamente" a cena recarrega e pula a tela inicial.
    private static bool skipMenuOnNextLoad;

    void Awake()
    {
        Instance = this;

        // A interface é desenhada por outro script no mesmo objeto.
        if (!TryGetComponent(out GameUI _))
            gameObject.AddComponent<GameUI>();

        // Congela o jogo até o jogador apertar Start.
        Time.timeScale = 0f;

        if (skipMenuOnNextLoad)
        {
            skipMenuOnNextLoad = false;
            StartGame();
        }
    }

    void Start()
    {
        if (spawnCoins)
        {
            GameObject coinsParent = new GameObject("Moedas");
            TotalCoins = CoinSpawner.SpawnAll(coinsParent.transform);
        }
    }

    void Update()
    {
        // Time.deltaTime já é zero com o jogo congelado, mas a checagem deixa a regra explícita.
        if (State == GameState.Playing)
            ElapsedTime += Time.deltaTime;
    }

    void OnDestroy()
    {
        // Garante que o tempo volte ao normal ao sair do Play Mode.
        Time.timeScale = 1f;
        if (Instance == this) Instance = null;
    }

    public void StartGame()
    {
        if (State == GameState.Playing) return;

        State = GameState.Playing;
        ElapsedTime = 0f;
        CoinsCollected = 0;
        Time.timeScale = 1f;
    }

    public void CollectCoin()
    {
        if (State != GameState.Playing) return;

        CoinsCollected++;

        if (TotalCoins > 0 && CoinsCollected >= TotalCoins)
            FinishGame();
    }

    private void FinishGame()
    {
        State = GameState.Finished;
        Time.timeScale = 0f; // o cronômetro para junto com o jogo
    }

    public void RestartGame()
    {
        skipMenuOnNextLoad = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Formata segundos como MM:SS.cc (centésimos).</summary>
    public static string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(Mathf.Max(0f, seconds) * 100f);
        int centis = total % 100;
        int secs = (total / 100) % 60;
        int mins = total / 6000;
        return mins.ToString("00") + ":" + secs.ToString("00") + "." + centis.ToString("00");
    }
}
