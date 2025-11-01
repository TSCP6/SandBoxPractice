using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public GameObject panel; 
    public TextMeshProUGUI modeText; //显示当前模式
    public TextMeshProUGUI pauseCondition;
    public TextMeshProUGUI timeScaleCondition;

    private bool isPaused = false;
    private float[] timeScale = { 1f, 2f};
    private int curTimeIndex = -1;
    private float lastTimeScale = 1f;

    public enum Mode { CreativeMode, FreeMode };
    public static Manager Instance { get; private set; } //任何代码都可以读取，但是仅能在manager中进行修改

    [SerializeField]
    private Mode startMode = Mode.FreeMode;
    Mode curMode;

    public Mode currentMode
    {
        get => curMode;
        private set //将值修改为value右值
        {
            if (curMode != value)
            {
                Mode oldMode = curMode;
                curMode = value;
                OnModeChanged(oldMode, curMode);
            }
        }
    }

    public delegate void ModeChangeHandler(Mode oldMode, Mode newMode);
    public event ModeChangeHandler ModeChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentMode = startMode;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleMode();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleTimeScale();
        }
    }

    //切换到newMode
    public void SwitchMode(Mode newMode)
    {
        if (currentMode == newMode)
        {
            Debug.Log($"已经处于 {newMode} 模式");
            return;
        }
        currentMode = newMode;
    }

    //在两种模式间切换
    public void ToggleMode()
    {
        Mode newMode = currentMode == Mode.CreativeMode ? Mode.FreeMode : Mode.CreativeMode;
        SwitchMode(newMode);
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            lastTimeScale = Time.timeScale;
            Time.timeScale = 0;
            pauseCondition.text = "Off";
        }
        else
        {
            Time.timeScale = lastTimeScale;
            pauseCondition.text = "On";
        }
    }

    void ToggleTimeScale()
    {
        curTimeIndex = (curTimeIndex + 1) % timeScale.Length;

        float newTimeScale = timeScale[curTimeIndex];

        if (!isPaused)
        {
            Time.timeScale = newTimeScale;
        }

        lastTimeScale = newTimeScale;

        timeScaleCondition.text = "×" + Time.timeScale;
    }

    private void OnModeChanged(Mode oldMode, Mode newMode)
    {
        Debug.Log($"模式切换: {oldMode} -> {newMode}");
        ModeChanged?.Invoke(oldMode, newMode); //将模式变化通知给订阅者

        switch (newMode)
        {
            case Mode.FreeMode:
                EnterFreeMode();
                break;
            case Mode.CreativeMode:
                EnterCreativeMode();
                break;
        }
    }

    private void EnterCreativeMode()
    {
        Debug.Log("进入创造模式");
        if (modeText != null) modeText.text = "Creative Mode";
    }

    private void EnterFreeMode()
    {
        Debug.Log("进入自由模式");
        if (modeText != null) modeText.text = "Free Mode";
    }

    public bool IsMode(Mode mode)
    {
        return currentMode == mode;
    }
}
