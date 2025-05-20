using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float duration = 1f;
    public AnimationCurve curve;
    public bool start = false;

    private Coroutine _currentShake;

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            StartCoroutine(Shaking(1));
            start = false;
        }
    }
    IEnumerator Shaking(float magnitude)
    {

        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float strength = curve.Evaluate(elapsedTime / duration)*magnitude;
            transform.localPosition = startPosition + Random.insideUnitSphere * strength;
            yield return null;
        }
        transform.localPosition = startPosition;

    }
    public void TriggerShake(float magnitude = 1f)
    {
        if (_currentShake != null)
            StopCoroutine(_currentShake);

       _currentShake = StartCoroutine(Shaking(magnitude));
    }



}