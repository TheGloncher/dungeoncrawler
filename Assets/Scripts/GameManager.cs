using System.Collections;
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

    public int playerHP;
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
            Instance.StartCoroutine(ShowEndAndRestart());
        }
    }

    private IEnumerator ShowEndAndRestart()
    {
        // Wait until BattleManager is active and ready
        yield return new WaitForSeconds(0.5f);
        BattleManager manager = FindObjectOfType<BattleManager>();

        while (manager == null)
        {
            yield return new WaitForSeconds(0.2f);
            manager = FindObjectOfType<BattleManager>();
        }

        manager.dialogue.text = "Congratulations, you have reached the end.";
        yield return manager.StartCoroutine(manager.WaitForContinue());

        currentStageIndex = 0;
        SceneManager.LoadScene(battleScenes[0]);
    }

    public void RestartGame()
    {
        currentStageIndex = 0;
        
        recruitedParty.Clear(); // ? Clear the actual party list
        SceneManager.LoadScene("MainMenu");
    }
}