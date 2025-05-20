using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float fadeSpeed = 1f;
    public float duration = 1.2f;

    private TextMeshProUGUI text;
    private Color originalColor;

    void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
        originalColor = text.color;
    }

    public void Initialize(string message, Color color)
    {
        text.text = message;
        text.color = color;
        originalColor = color;

        Destroy(gameObject, duration); // Auto-destroy
    }

    void Update()
    {
        // Move up in world space
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out
        Color c = text.color;
        c.a -= fadeSpeed * Time.deltaTime;
        text.color = c;
    }

    void LateUpdate()
    {
        // Always face camera
        if (Camera.main)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // Correct backward facing text
        }
    }
}