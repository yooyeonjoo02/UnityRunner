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
    public float gameSpeed = 10f;   // 게임 속도 관리
    public float acceleration = 1f;  // 초당 속도 증가량

    private float time;
    private bool isGameOver = false;

    private List<Record> records = new List<Record>();

    public static GameManager Instance;

    // Ground 3개의 라인 X 좌표 정보
    public float[] lanes = { -3f, 0f, 3f };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
            // 1. 시간에 따른 속도 상승 
            gameSpeed += acceleration * Time.deltaTime;

            // 2. 시간 및 속도에 비례한 점수 상승 
            //score += gameSpeed * Time.deltaTime;
            
            // UI 업데이트 (기존 timerText 혹은 새로운 scoreText 사용)
            // if (scoreText != null)
            //     scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
            
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

    #region 게임 아이템 로직

    // 시간 가속 아이템 효과 (FastItem에서 호출)
    public void ApplyTimeScaleEffect(float multiplier, float duration)
    {
        StartCoroutine(TimeScaleRoutine(multiplier, duration));
    }
    private System.Collections.IEnumerator TimeScaleRoutine(float multiplier, float duration)
    {
        Time.timeScale = multiplier;
        
        // Time.timeScale이 변했으므로 실제 흐르는 시간을 기준으로 대기해야 함
        yield return new WaitForSecondsRealtime(duration); 
        
        // 원래대로 복구 (게임오버 상태가 아닐 때만)
        if (!isGameOver) 
        {
            Time.timeScale = 1f;
        }
    }

    

    #endregion
}