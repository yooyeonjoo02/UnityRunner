using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class Record
{
    public string name;
    public float time;

    public Record(string name, float time)
    {
        this.name = name;
        this.time = time;
    }
}

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI currentRecordText;
    public TextMeshProUGUI recordListText;
    public float gameSpeed = 10f;

    private float time;
    private bool isGameOver = false;

    private List<Record> records = new List<Record>();

    void Start()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        ShowRecordList();
    }

    void Update()
    {
        if (!isGameOver)
        {
            time += Time.deltaTime;
            timerText.text = time.ToString("F1");
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        gameOverPanel.SetActive(true);
        currentRecordText.text = "Record: " + time.ToString("F1") + " sec";
    }

    public void SaveRecord()
    {
        string playerName = nameInputField.text;

        if (playerName == "")
            playerName = "Player";

        LoadFromPrefs();

        records.Add(new Record(playerName, time));

        records.Sort((a, b) => b.time.CompareTo(a.time));

        if (records.Count > 5)
            records.RemoveAt(records.Count - 1);

        SaveToPrefs();
        ShowRecordList();
    }

    private void SaveToPrefs()
    {
        PlayerPrefs.SetInt("RecordCount", records.Count);

        for (int i = 0; i < records.Count; i++)
        {
            PlayerPrefs.SetString("Name_" + i, records[i].name);
            PlayerPrefs.SetFloat("Time_" + i, records[i].time);
        }

        PlayerPrefs.Save();
    }

    private void LoadFromPrefs()
    {
        records.Clear();

        int count = PlayerPrefs.GetInt("RecordCount", 0);

        for (int i = 0; i < count; i++)
        {
            string savedName = PlayerPrefs.GetString("Name_" + i, "Player");
            float savedTime = PlayerPrefs.GetFloat("Time_" + i, 0f);

            records.Add(new Record(savedName, savedTime));
        }
    }

    private void ShowRecordList()
    {
        LoadFromPrefs();

        string text = "Best Record\n";

        if (records.Count == 0)
        {
            text += "No Record";
        }
        else
        {
            for (int i = 0; i < records.Count; i++)
            {
                text += (i + 1) + ". "
                      + records[i].name + " - "
                      + records[i].time.ToString("F1") + " sec\n";
            }
        }

        recordListText.text = text;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}