using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Collections;

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

    public float spawnZ = 200f; // Spawner에서 사용할 생성 위치 z값

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
            // 시간에 따른 속도 상승 
            gameSpeed += acceleration * Time.deltaTime;
            
            time += Time.deltaTime;
            timerText.text = time.ToString("F2");
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        gameOverPanel.SetActive(true);
        currentRecordText.text = "Record : " + time.ToString("F2") + " sec";
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
                      + records[i].time.ToString("F2") + " sec\n";
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

    // 속도 증감 아이템 효과 (FastItem, SlowItem에서 호출)
    public void ApplySpeedItemEffect(float amount, float duration)
    {
        StartCoroutine(SpeedChangeRoutine(amount, duration));
    }

    private IEnumerator SpeedChangeRoutine(float amount, float duration)
    {
        // 1. 속도를 즉시 n만큼 증가/감소
        gameSpeed += amount;
        
        yield return new WaitForSeconds(duration);
        
        // 2. m초 후 증가/감소시켰던 n만큼 정확히 원상 복구
        // (그동안 진행된 가속도(acceleration) 곡선은 그대로 부드럽게 유지됨)
        gameSpeed -= amount;
    }

    
    [HideInInspector] public bool isGhostMode = false;
    [HideInInspector] public float currentGhostAlpha = 1.0f;
    // 고스트 아이템 효과 (GhostItem에서 사용)
    public IEnumerator GhostModeRoutine(float duration, Action callBack)
    {
        
        isGhostMode = true;
        // 깜빡임 시작 전까지의 일반 반투명 지속 시간 (예: 5초 - 2초 = 3초)
        float normalDuration = duration - 2f;
        if (normalDuration < 0) normalDuration = 0;

        // 1. 일반 반투명 상태
        currentGhostAlpha = 0.3f;
        yield return new WaitForSeconds(normalDuration);

        // 2. 풀리기 2초 전 깜빡임 (PingPong 효과)
        float flickerTimer = 0f;
        while (flickerTimer < 2f)
        {
            flickerTimer += Time.deltaTime;
            
            // 시간이 지날수록 주파수(Frequency)가 5에서 25로 상승하여 점점 빠르게 깜빡임
            float frequency = Mathf.Lerp(5f, 25f, flickerTimer / 2f);
            
            // Mathf.PingPong을 사용해 0.3(투명) ~ 0.8(진해짐) 사이를 왕복
            currentGhostAlpha = Mathf.Lerp(0.3f, 0.8f, Mathf.PingPong(flickerTimer * frequency, 1f));
            
            yield return null; // 매 프레임 업데이트
        }

        // 3. 지속시간 종료 후 원상 복구
        callBack?.Invoke();
        isGhostMode = false;
        currentGhostAlpha = 1.0f;
    }

    #endregion
}