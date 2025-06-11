using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class PartyMemberData
    {
        public string prefabName;
        public int currentHP;

        public PartyMemberData(string prefabName, int currentHP)
        {
            this.prefabName = prefabName;
            this.currentHP = currentHP;
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
        }
    }

    public static GameManager Instance;

    public int playerHP = 10;
    public string playerPrefabName = "Player"; // ? Now it's in GameManager
    public List<PartyMemberData> recruitedParty = new List<PartyMemberData>(); // Here too

    public int currentStageIndex = 0;

    public string[] battleScenes = { "Battle_1", "Battle_2", "Battle_3", "Battle_4" };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadNextStage()
    {
        currentStageIndex++;
        if (currentStageIndex < battleScenes.Length)
        {
            SceneManager.LoadScene(battleScenes[currentStageIndex]);
        }
        else
        {
            Debug.Log("Congratulations, you have reached the end.");
            SceneManager.LoadScene("MainMenu"); // or a Victory screen
        }
    }

    public void RestartGame()
    {
        currentStageIndex = 0;
        playerHP = 10;
        recruitedParty.Clear(); // ? Clear the actual party list
        SceneManager.LoadScene("MainMenu");
    }
}