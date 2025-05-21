using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuLogic : MonoBehaviour
{
    [SerializeField] private Button mainMenuUI; // Reference to the main menu UI GameObject
    private GameObject _lastSelected;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(SelectMainButtonNextFrame());
    }

    private IEnumerator SelectMainButtonNextFrame()
    {
        yield return null; // wait one frame

        EventSystem.current.SetSelectedGameObject(mainMenuUI.gameObject);
        _lastSelected = mainMenuUI.gameObject;
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected);
        }
        else
        {
            _lastSelected = EventSystem.current.currentSelectedGameObject;
        }
    }

    public void StartButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Battle_1");
    }
}