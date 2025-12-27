using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerUI : MonoBehaviour
{
    public static TimerUI Instance { get; private set; }

    [Header("Warning Settings")]
    [SerializeField] private float warningTime = 10f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;

    private TextMeshProUGUI timerText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(transform.root.gameObject);

        timerText = GetComponent<TextMeshProUGUI>();
        Hide();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Hide();
    }

    public void DisplayTime(float timeLeft)
    {
        if (timerText == null)
            return;

        timeLeft = Mathf.Max(timeLeft, 0f);

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
        timerText.enabled = true;

        timerText.color = timeLeft <= warningTime ? warningColor : normalColor;
    }

    public void Hide()
    {
        if (timerText == null)
            return;

        timerText.text = "";
        timerText.enabled = false;
    }
}
