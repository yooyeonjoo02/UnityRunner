using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine.UI;

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
    [Header("Core UI")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI currentRecordText;
    public TextMeshProUGUI recordListText;

    [Header("Item Feedback UI")]
    // 속도 증감 텍스트 UI
    public TextMeshProUGUI speedFeedbackText;
    public CanvasGroup speedFeedbackCanvasGroup;
    public RectTransform speedFeedbackRect;

    // 쉴드 UI
    public GameObject shieldActiveIcon;
    public GameObject shieldBrokenIcon;
    
    // 고스트 UI
    public Image ghostCooldownImage; // 초기 Item Type이 Filled로 설정되어 있어야 함
    public GameObject ghostItem;
    public GameObject ghostBackground;


    [Header("Game Settings")]
    public float gameSpeed = 10f;   // 게임 속도 관리
    public float acceleration = 1f;  // 초당 속도 증가량

    public float spawnZ = 200f; // Spawner에서 사용할 생성 위치 z값

    // 고스트 쿨타임 제어용 변수
    [HideInInspector] public bool isGhostMode = false;
    private float ghostTimer = 0f;
    private float ghostDurationMax = 0f;
    private Coroutine ghostCoroutine;

    // 속도 피드백 코루틴 추적 변수
    private Coroutine speedFeedbackCoroutine;
    
    // 게임 시스템 용 변수
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
        
        if (speedFeedbackCanvasGroup != null)
            speedFeedbackCanvasGroup.alpha = 0f;
        
        if (ghostCooldownImage != null)
            ghostCooldownImage.fillAmount = 0f;

        
        UpdateShieldUI(false);

        ShowRecordList();

        if(ghostBackground != null)
            ghostBackground.SetActive(true);
        if(ghostItem != null)
            ghostItem.SetActive(true);
    }

    void Update()
    {
        if (!isGameOver)
        {
            // 시간에 따른 속도 상승 
            gameSpeed += acceleration * Time.deltaTime;
            
            float speedRatio = gameSpeed / 10f; // UI의 카운트 속도가 실제 게임 스피드에 비례하여 증감

            time += Time.deltaTime * speedRatio; 
            timerText.text = time.ToString("F2");
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        gameOverPanel.SetActive(true);
        currentRecordText.text = "Record : " + time.ToString("F2") + " 점";
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

        string text = ""; // "Best Record\n" 제목은 UI 상단에 고정 텍스트로 배치

        if (records.Count == 0)
        {
            text = "1. - - -\n2. - - -\n3. - - -";
        }
        else
        {
            // 상위 3개만 표기, 나머지는 '-' 처리
            for (int i = 0; i < 3; i++)
            {
                if (i < records.Count)
                {
                    text += (i + 1) + ". " + records[i].name + " - " + records[i].time.ToString("F2") + " 점\n";
                }
                else
                {
                    text += (i + 1) + ". - - -\n";
                }
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
    public void ApplySpeedItemEffect(float amount, float duration, bool isFast)
    {
        // 로직 코루틴
        StartCoroutine(SpeedChangeRoutine(amount, duration));

        // 시각적 코루틴 (기존 코루틴을 덮어 쓰도록 하여 애니메이션 filkering 문제 해결)
        if (speedFeedbackCoroutine != null)
        {
            StopCoroutine(speedFeedbackCoroutine);
        }
        // 새로운 애니메이션을 시작하고 변수에 저장하여 추적
        speedFeedbackCoroutine = StartCoroutine(ShowSpeedFeedbackRoutine(isFast));
    }

    // 공용 - 속도 변경 코루틴
    private IEnumerator SpeedChangeRoutine(float amount, float duration)
    {
        // 1. 속도를 즉시 n만큼 증가/감소
        gameSpeed += amount;
        
        yield return new WaitForSeconds(duration);
        
        // 2. m초 후 증가/감소시켰던 n만큼 정확히 원상 복구
        // 그동안 진행된 가속도(acceleration) 곡선은 그대로 부드럽게 유지됨
        // 지속시간 동안에 또 아이템을 먹어서 변경되더라도 연계적으로 원래 상태로 복구됨
        gameSpeed -= amount;
    }

    // 속도 텍스트 애니메이션
    private IEnumerator ShowSpeedFeedbackRoutine(bool isFast)
    {
        if (speedFeedbackText == null || speedFeedbackCanvasGroup == null) yield break;

        speedFeedbackText.text = isFast ? "속도 증가" : "속도 감소";
        
        // 이동 방향 설정 (증가: 아래에서 위 / 감소: 위에서 아래)
        float startY = isFast ? -50f : 50f;
        float targetY = 0f;
        
        // Fade In (0.3초)
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            speedFeedbackCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
            speedFeedbackRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(startY, targetY, t / 0.3f));
            yield return null;
        }

        // 유지 (2초)
        yield return new WaitForSeconds(2f);

        // Fade Out (0.3초)
        t = 0f;
        float endY = isFast ? 50f : -50f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            speedFeedbackCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
            speedFeedbackRect.anchoredPosition = new Vector2(0f, Mathf.Lerp(targetY, endY, t / 0.3f));
            yield return null;
        }
    }

    // 쉴드 UI 상태 업데이트
    public void UpdateShieldUI(bool isActive)
    {
        if (shieldActiveIcon != null) shieldActiveIcon.SetActive(isActive);
        if (shieldBrokenIcon != null) shieldBrokenIcon.SetActive(!isActive);
    }

    // 고스트 모드 쿨타임 제어 (기존의 반투명 깜빡임 로직 삭제 및 타이머 로직 개편)
    public void ActivateGhostMode(float duration, Action callBack)
    {
        ghostDurationMax = duration;
        ghostTimer = duration; // 갱신 (이미 실행 중이라도 타이머만 최대로 덮어씌움)

        if (ghostCoroutine == null)
        {
            ghostCoroutine = StartCoroutine(GhostTimerRoutine(callBack));
        }
    }

    
    // 고스트 아이템 효과 (GhostItem에서 사용)
    private IEnumerator GhostTimerRoutine(Action callBack)
    {
        isGhostMode = true;

        while (ghostTimer > 0f)
        {
            ghostTimer -= Time.deltaTime;
            
            // UI 원형 쿨타임 업데이트
            if (ghostCooldownImage != null)
            {
                ghostCooldownImage.fillAmount = ghostTimer / ghostDurationMax;
            }
            yield return null;
        }

        isGhostMode = false;
        ghostCoroutine = null;
        
        // if (PlayerController.Instance != null)
        // {
        //     PlayerController.Instance.GhostModeAfter(); // 플레이어 상태 동기화
        // }

        // 콜백 함수로 플레이어 상태 동기화
        callBack?.Invoke();
    }

    // public IEnumerator GhostModeRoutine(float duration, Action callBack)
    // {
        
    //     isGhostMode = true;

    //     while (ghostTimer > 0f)
    //     {
    //         ghostTimer -= Time.deltaTime;
            
    //         // UI 원형 쿨타임 업데이트
    //         if (ghostCooldownImage != null)
    //         {
    //             ghostCooldownImage.fillAmount = ghostTimer / ghostDurationMax;
    //         }
    //         yield return null;
    //     }

    //     isGhostMode = false;
    //     ghostCoroutine = null;
        
    //     // if (PlayerController.Instance != null)
    //     // {
    //     //     PlayerController.Instance.GhostModeAfter(); 
    //     // }

    //     // 콜백 함수로 플레이어 상태 동기화
    //     callBack?.Invoke();
    //     isGhostMode = false;
    // }

    #endregion
}