using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public enum Mode { CreativeMode, FreeMode };
    public static Manager Instance { get; private set; } //任何代码都可以读取，但是仅能在manager中进行修改

    Mode startMode = Mode.FreeMode;
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
        curMode = startMode;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleMode();
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
    }

    private void EnterFreeMode()
    {
        Debug.Log("进入自由模式");
    }

    public bool IsMode(Mode mode)
    {
        return currentMode == mode;
    }
}
