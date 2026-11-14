using UnityEngine;

/// <summary>
/// Interface do jogo: tela inicial (nome do jogo + botão Start), HUD com cronômetro e contador
/// de moedas, e tela de fim de jogo. Usa o sistema de GUI imediata (OnGUI), então não precisa
/// de Canvas, EventSystem nem de nenhum objeto extra na cena.
/// </summary>
public class GameUI : MonoBehaviour
{
    private const string GameTitle = "TURBO KART";
    private const float DesignHeight = 1080f; // a interface é desenhada em 1080p e escala para a tela

    private static readonly Color Gold = new Color(1f, 0.82f, 0.1f);
    private static readonly Color DarkText = new Color(0.16f, 0.1f, 0f);

    private bool ready;
    private GUIStyle titleStyle, subtitleStyle, hintStyle, panelStyle, buttonStyle;
    private GUIStyle hudLabelStyle, hudValueStyle, bigValueStyle;
    private Texture2D panelTex, buttonTex, buttonHoverTex, buttonActiveTex, coinIcon;

    void OnDestroy()
    {
        Destroy(panelTex); Destroy(buttonTex); Destroy(buttonHoverTex);
        Destroy(buttonActiveTex); Destroy(coinIcon);
    }

    void OnGUI()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (!ready) BuildResources();

        float scale = Screen.height / DesignHeight;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float w = Screen.width / scale;
        float h = DesignHeight;

        switch (gm.State)
        {
            case GameState.Menu:
                DrawMenu(gm, w, h);
                break;
            case GameState.Playing:
                DrawHud(gm, w);
                break;
            case GameState.Finished:
                DrawHud(gm, w);
                DrawFinished(gm, w, h);
                break;
        }
    }

    // ---------------------------------------------------------------- telas

    private void DrawMenu(GameManager gm, float w, float h)
    {
        DrawOverlay(w, h, 0.65f);

        DrawOutlinedLabel(new Rect(0f, h * 0.17f, w, 170f), GameTitle, titleStyle, Gold, Color.black, 5f);
        GUI.Label(new Rect(0f, h * 0.17f + 175f, w, 60f), "Colete todas as moedas o mais rápido que puder!", subtitleStyle);

        bool start = GUI.Button(new Rect((w - 420f) / 2f, h * 0.52f, 420f, 120f), "START", buttonStyle);
        start |= KeyPressed(KeyCode.Return) || KeyPressed(KeyCode.KeypadEnter) || KeyPressed(KeyCode.Space);

        GUI.Label(new Rect(0f, h * 0.52f + 140f, w, 40f), "Pressione ENTER ou clique em START", hintStyle);
        GUI.Label(new Rect(0f, h * 0.88f, w, 40f), "Controles: W A S D ou setas do teclado", hintStyle);

        if (start) gm.StartGame();
    }

    private void DrawHud(GameManager gm, float w)
    {
        // Cronômetro (centro, no topo)
        Rect timer = new Rect((w - 320f) / 2f, 24f, 320f, 96f);
        GUI.Box(timer, GUIContent.none, panelStyle);
        GUI.Label(new Rect(timer.x, timer.y + 8f, timer.width, 26f), "TEMPO", hudLabelStyle);
        GUI.Label(new Rect(timer.x, timer.y + 32f, timer.width, 56f), GameManager.FormatTime(gm.ElapsedTime), hudValueStyle);

        // Contador de moedas (canto direito, no topo)
        Rect coins = new Rect(w - 340f - 30f, 24f, 340f, 96f);
        GUI.Box(coins, GUIContent.none, panelStyle);
        GUI.DrawTexture(new Rect(coins.x + 22f, coins.y + 20f, 56f, 56f), coinIcon);
        GUI.Label(new Rect(coins.x + 90f, coins.y + 8f, coins.width - 100f, 26f), "MOEDAS", hudLabelStyle);
        GUI.Label(new Rect(coins.x + 90f, coins.y + 32f, coins.width - 100f, 56f),
                  gm.CoinsCollected + " / " + gm.TotalCoins, hudValueStyle);
    }

    private void DrawFinished(GameManager gm, float w, float h)
    {
        DrawOverlay(w, h, 0.6f);

        DrawOutlinedLabel(new Rect(0f, h * 0.20f, w, 150f), "PARABÉNS!", titleStyle, Gold, Color.black, 5f);
        GUI.Label(new Rect(0f, h * 0.20f + 160f, w, 60f),
                  "Você coletou as " + gm.TotalCoins + " moedas", subtitleStyle);

        GUI.Label(new Rect(0f, h * 0.20f + 240f, w, 30f), "SEU TEMPO", hudLabelStyle);
        GUI.Label(new Rect(0f, h * 0.20f + 270f, w, 100f), GameManager.FormatTime(gm.ElapsedTime), bigValueStyle);

        bool again = GUI.Button(new Rect((w - 560f) / 2f, h * 0.62f, 560f, 120f), "JOGAR NOVAMENTE", buttonStyle);
        again |= KeyPressed(KeyCode.Return) || KeyPressed(KeyCode.KeypadEnter) || KeyPressed(KeyCode.R);

        GUI.Label(new Rect(0f, h * 0.62f + 140f, w, 40f), "Pressione ENTER ou R", hintStyle);

        if (again) gm.RestartGame();
    }

    // ---------------------------------------------------------------- helpers de desenho

    private static bool KeyPressed(KeyCode key)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == key)
        {
            e.Use();
            return true;
        }
        return false;
    }

    private static void DrawOverlay(float w, float h, float alpha)
    {
        GUI.color = new Color(0f, 0f, 0f, alpha);
        GUI.DrawTexture(new Rect(0f, 0f, w, h), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    // Texto com contorno: desenha o texto preto deslocado nos 8 lados e o texto colorido por cima.
    private static void DrawOutlinedLabel(Rect rect, string text, GUIStyle style, Color color, Color outline, float size)
    {
        Color original = style.normal.textColor;

        style.normal.textColor = outline;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                GUI.Label(new Rect(rect.x + dx * size, rect.y + dy * size, rect.width, rect.height), text, style);
            }
        }

        style.normal.textColor = color;
        GUI.Label(rect, text, style);
        style.normal.textColor = original;
    }

    // ---------------------------------------------------------------- recursos (estilos e texturas)

    private void BuildResources()
    {
        panelTex = MakeRoundedTexture(64, 18, new Color(0f, 0f, 0f, 0.55f));
        buttonTex = MakeRoundedTexture(64, 22, Gold);
        buttonHoverTex = MakeRoundedTexture(64, 22, new Color(1f, 0.9f, 0.4f));
        buttonActiveTex = MakeRoundedTexture(64, 22, new Color(0.88f, 0.64f, 0.05f));
        coinIcon = MakeCoinIcon(64);

        titleStyle = MakeLabel(130, FontStyle.Bold, Gold);
        subtitleStyle = MakeLabel(38, FontStyle.Normal, Color.white);
        hintStyle = MakeLabel(26, FontStyle.Normal, new Color(1f, 1f, 1f, 0.7f));
        hudLabelStyle = MakeLabel(22, FontStyle.Bold, new Color(1f, 1f, 1f, 0.7f));
        hudValueStyle = MakeLabel(44, FontStyle.Bold, Color.white);
        bigValueStyle = MakeLabel(96, FontStyle.Bold, Color.white);

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            border = new RectOffset(18, 18, 18, 18),
            padding = new RectOffset(0, 0, 0, 0)
        };
        SetBackground(panelStyle, panelTex, panelTex, panelTex);

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 52,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            border = new RectOffset(22, 22, 22, 22)
        };
        SetBackground(buttonStyle, buttonTex, buttonHoverTex, buttonActiveTex);
        buttonStyle.normal.textColor = buttonStyle.hover.textColor =
            buttonStyle.active.textColor = buttonStyle.focused.textColor = DarkText;

        ready = true;
    }

    private static GUIStyle MakeLabel(int fontSize, FontStyle fontStyle, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = fontStyle,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = false
        };
        style.normal.textColor = color;
        return style;
    }

    private static void SetBackground(GUIStyle style, Texture2D normal, Texture2D hover, Texture2D active)
    {
        style.normal.background = normal;
        style.hover.background = hover;
        style.active.background = active;
        style.focused.background = normal;
    }

    // Retângulo com cantos arredondados (usado como 9-slice nos painéis e botões).
    private static Texture2D MakeRoundedTexture(int size, int radius, Color color)
    {
        Texture2D tex = NewTexture(size);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                Vector2 nearest = new Vector2(Mathf.Clamp(p.x, radius, size - radius), Mathf.Clamp(p.y, radius, size - radius));
                float alpha = Mathf.Clamp01(radius - Vector2.Distance(p, nearest) + 0.5f);
                pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // Ícone de moeda dourada para o contador.
    private static Texture2D MakeCoinIcon(int size)
    {
        Texture2D tex = NewTexture(size);
        Color[] pixels = new Color[size * size];
        float center = (size - 1) / 2f;
        float radius = size / 2f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(radius - d + 0.5f);

                Color c;
                if (d > radius * 0.78f)
                    c = new Color(0.78f, 0.52f, 0.05f);          // borda
                else
                {
                    float shine = Mathf.Clamp01(((x - y) / (float)size + 0.5f)); // brilho no canto de cima
                    c = Color.Lerp(new Color(1f, 0.78f, 0.1f), new Color(1f, 0.95f, 0.6f), shine * 0.6f);
                }

                c.a = alpha;
                pixels[y * size + x] = c;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private static Texture2D NewTexture(int size)
    {
        return new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };
    }
}
