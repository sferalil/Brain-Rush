using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnswerCube : MonoBehaviour
{
    public float speed = 5f;

    // Panel o'lchamlari kodning ichida (Inspector/prefabdagi eski qiymatlar ta'sir qilmaydi).
    // Kamera yuqori chegarasi spawn nuqtasida y ~ 2.9, shuning uchun panel yuqori cheti shundan oshmasin.
    private const float PanelWidth = 5.3f;      // yo'laklar orasi ~5.4, oshirmang
    private const float PanelHeight = 3.2f;     // 3-4 qatorgacha sig'adi
    private const float GapAboveCube = 0.15f;
    private const float MinFontSize = 1f;
    private const float MaxFontSize = 8f;
    private const float BorderThickness = 0.12f;
    private static readonly Color PanelColor = new Color(0.07f, 0.08f, 0.12f, 1f);

    private bool isCorrect;
    private bool hasHit;
    private Collider playerCollider;

    private Transform panel;
    private TextMeshPro label;
    private Material borderMaterial;
    private readonly List<Material> createdMaterials = new List<Material>();
    private Camera cam;
    private Vector3 panelOffset;

    void Awake()
    {
        // Kub ichidagi eski yozuvni o'chiramiz (endi javob panelda ko'rinadi).
        TMP_Text oldFaceText = GetComponentInChildren<TMP_Text>(true);
        if (oldFaceText != null)
            oldFaceText.gameObject.SetActive(false);

        BuildPanel();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerCollider = player.GetComponent<Collider>();
    }

    public void SetColor(Color color)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = color;

        // Panel chekkasi kub bilan bir xil rangda: o'yinchi kubni rang bo'yicha topadi.
        if (borderMaterial != null)
            borderMaterial.color = color;
    }

    // Matnli javoblar uchun (barcha kategoriyalar).
    public void SetAnswer(string answer, bool correct)
    {
        isCorrect = correct;

        if (label != null)
            label.text = answer;
    }

    // Eski kod bilan moslik uchun raqamli overload ham qoldirildi.
    public void SetAnswer(int answer, bool correct)
    {
        SetAnswer(answer.ToString(), correct);
    }

    void Update()
    {
        if (hasHit) return;

        transform.Translate(Vector3.back * speed * Time.deltaTime);

        if (playerCollider == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerCollider = player.GetComponent<Collider>();
        }

        if (playerCollider != null)
        {
            Collider cubeCollider = GetComponent<Collider>();
            if (cubeCollider != null && cubeCollider.bounds.Intersects(playerCollider.bounds))
            {
                hasHit = true;

                MathGame mathGame = FindFirstObjectByType<MathGame>();
                if (mathGame != null)
                    mathGame.CheckAnswer(isCorrect);

                Destroy(gameObject);
            }
        }
    }

    void LateUpdate()
    {
        if (panel == null) return;

        if (cam == null) cam = Camera.main;

        panel.position = transform.position + panelOffset;

        // Billboard: panel doim kamera bilan bir xil burchakda turadi.
        if (cam != null)
            panel.rotation = cam.transform.rotation;
    }

    void OnDestroy()
    {
        if (panel != null)
            Destroy(panel.gameObject);

        foreach (Material m in createdMaterials)
            if (m != null) Destroy(m);
    }

    // ---------------------------------------------------------------
    // Panelni kod orqali yasaymiz, shuning uchun prefabni o'zgartirish shart emas.
    // ---------------------------------------------------------------
    private void BuildPanel()
    {
        panelOffset = new Vector3(0f, transform.lossyScale.y * 0.5f + GapAboveCube + PanelHeight * 0.5f, 0f);

        GameObject root = new GameObject("AnswerLabel");
        panel = root.transform;
        panel.position = transform.position + panelOffset;

        // Chekka (kub rangida) va fon
        borderMaterial = CreateQuad("Border", panel,
            new Vector3(PanelWidth + BorderThickness * 2f, PanelHeight + BorderThickness * 2f, 1f),
            0.01f, Color.white);
        CreateQuad("Background", panel, new Vector3(PanelWidth, PanelHeight, 1f), 0f, PanelColor);

        // Matn
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(panel, false);
        textGO.transform.localPosition = new Vector3(0f, 0f, -0.02f);

        label = textGO.AddComponent<TextMeshPro>();
        label.rectTransform.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        label.margin = new Vector4(0.08f, 0.05f, 0.08f, 0.05f);
        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Bold;
        label.color = Color.white;
        label.textWrappingMode = TextWrappingModes.Normal;   // uzun javob keyingi qatorga o'tadi
        label.overflowMode = TextOverflowModes.Overflow;
        label.enableAutoSizing = true;                       // sig'maguncha matn kichrayadi
        label.fontSizeMin = MinFontSize;
        label.fontSizeMax = MaxFontSize;
        label.lineSpacing = -25f;               // qatorlar zichroq: ko'proq qator sig'adi
        label.characterSpacing = -3f;           // harflar bir oz zichroq: shrift kattaroq chiqadi
        label.outlineWidth = 0.12f;
        label.outlineColor = new Color32(0, 0, 0, 255);
        label.text = "";
    }

    private Material CreateQuad(string quadName, Transform parent, Vector3 scale, float z, Color color)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = quadName;

        Collider col = quad.GetComponent<Collider>();
        if (col != null) Destroy(col);

        quad.transform.SetParent(parent, false);
        quad.transform.localPosition = new Vector3(0f, 0f, z);
        quad.transform.localScale = scale;

        Renderer rend = quad.GetComponent<Renderer>();
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;

        // Unlit shader bo'lsa shuni, bo'lmasa kubning o'z materialini nusxalab ishlatamiz.
        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
        Material mat;
        if (unlit != null)
        {
            mat = new Material(unlit);
        }
        else
        {
            Renderer cubeRend = GetComponent<Renderer>();
            mat = new Material(cubeRend != null ? cubeRend.sharedMaterial : rend.sharedMaterial);
        }

        mat.color = color;
        rend.material = mat;
        createdMaterials.Add(mat);
        return mat;
    }
}