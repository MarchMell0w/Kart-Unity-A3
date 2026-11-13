using UnityEngine;

public class UnderBridgeIndicator : MonoBehaviour
{
    [Header("3D Marker Settings")]
    public Color markerColor = Color.yellow; // cor da seta
    public float heightAboveCar = 1.5f; // altura
    public float markerSize = 0.6f; // tamanho da seta

    private Transform markerTransform;
    private BoxCollider boxCol; // colisão da seta
    private Renderer carRenderer; // coiso que renderiza pro carro

    void Start()
    {
        boxCol = GetComponent<BoxCollider>();
        if (boxCol == null) boxCol = GetComponentInChildren<BoxCollider>();
        carRenderer = GetComponentInChildren<Renderer>();

        // Cria a esfera 3d no mundo
        GameObject markerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        markerObj.name = "CarTopMarker";

        // Sem colisão
        Destroy(markerObj.GetComponent<Collider>());

        markerTransform = markerObj.transform;
        markerTransform.localScale = new Vector3(markerSize, markerSize * 0.4f, markerSize);

        // Renderiza o material pra aparecer acima de pontes e ficar sempre visivel
        Renderer markerRend = markerObj.GetComponent<Renderer>();
        Shader guiShader = Shader.Find("GUI/Text Shader");
        if (guiShader != null)
        {
            Material xrayMat = new Material(guiShader);
            xrayMat.color = markerColor;
            markerRend.material = xrayMat;
        }
        else
        {
            markerRend.material.color = markerColor;
        }
    }

    void LateUpdate()
    {
        if (markerTransform == null) return;

        // Prende posição no topo do centro do carro
        Vector3 trueCenter = transform.position;
        if (boxCol != null)
            trueCenter = transform.TransformPoint(boxCol.center);
        else if (carRenderer != null)
            trueCenter = carRenderer.bounds.center;

        markerTransform.position = trueCenter + (Vector3.up * heightAboveCar);
    }
}