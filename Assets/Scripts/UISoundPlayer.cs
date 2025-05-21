using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class UISoundPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public AudioSource AudioSource;
    public AudioClip MoveSfx;
    public AudioClip SelectSfx;
    public AudioSource MusicSource;
    public AudioClip battleTheme;
    private GameObject _lastSelected;

    private void Start()
    {
        MusicSource.volume = 0.3f; 
        MusicSource.clip = battleTheme;
        MusicSource.loop = true;
        MusicSource.Play();
    }
    void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        // If selection changed (e.g., via controller/D-pad)
        if (current != null && current != _lastSelected)
        {
            AudioSource.PlayOneShot(MoveSfx);
            _lastSelected = current;
        }

        // Detect submit (e.g., A or Enter key)
        if (Input.GetButtonDown("Submit"))
        {
            AudioSource.PlayOneShot(SelectSfx);
        }
    }
}
