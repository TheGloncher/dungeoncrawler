using JetBrains.Annotations;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class BattleHUD : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI HpText;
    public Image HpSlider;
    public GameObject HUD;

    public RawImage rawImage; //  Drag your HUD RawImage here
    public Texture2D normalTexture;
    public Texture2D highlightedTexture;

    private Coroutine highlightRoutine;


    public void StartHighlight()
    {
        if (highlightRoutine != null)
            StopCoroutine(highlightRoutine);
        highlightRoutine = StartCoroutine(BlinkHighlight());
    }

    public void StopHighlight()
    {
        if (highlightRoutine != null)
            StopCoroutine(highlightRoutine);

        if (rawImage != null && normalTexture != null)
            rawImage.texture = normalTexture;
    }

    private IEnumerator BlinkHighlight()
    {
        while (true)
        {
            if (rawImage != null)
                rawImage.texture = highlightedTexture;

            yield return new WaitForSeconds(0.5f);

            if (rawImage != null)
                rawImage.texture = normalTexture;

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void SetHUD(Character character)
    {
        this.gameObject.SetActive(true);
        NameText.text = character.CharacterName;
        HpText.text = character.CurrentHP + "/" + character.MaxHP;
        float lifebarAmount = HpSlider.fillAmount = (float)character.CurrentHP / character.MaxHP;
        HpSlider.fillAmount = lifebarAmount;
       
       
    }

    public void Shake(float damageAmount)
    {
        ScreenShake shake = HUD.GetComponent<ScreenShake>();
        if (shake != null)
        {
            Debug.Log($"Shaking HUD with magnitude: {damageAmount}");
            shake.TriggerShake(damageAmount*5);
        }
        else
        {
            Debug.LogWarning(" No ScreenShake component found on HUD GameObject!");
        }
    }


}
