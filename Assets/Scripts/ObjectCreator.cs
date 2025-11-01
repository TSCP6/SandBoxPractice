using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectCreator : MonoBehaviour
{
    private bool onlyInCreativeMode = true; //只在创造模式中实现
    private bool canCreate = false;

    public GameObject[] prefabs = new GameObject[3]; //三个预制体
    private int curPrefabIndex = 0; //预制体索引

    public Material previewMaterial; //预览透明材质
    public Color previewColor = new Color(1f, 1f, 1f, 0.4f);
    public Vector3 defaultPos = new Vector3(0, 4f, 0);
    public float heightAdjustSpeed = 2f;
    private GameObject previewObject;
    private float heightOffset = 0f;

    public Color lineColor = Color.white;
    public float lineWidth = 0.05f;
    private LineRenderer dropIndicator; //下落射线的指示线

    public LayerMask groundLayer;
    public float maxDropDistance = 100f; //最远检测距离

    private Camera mainCamera;
    private bool isPreviewActive = false; //当前预览是否启用

    void Start()
    {
        mainCamera = Camera.main;

        CreateDropIndicator(); //创建下落指示线

        // 订阅模式切换事件
        if (Manager.Instance != null)
        {
            Manager.Instance.ModeChanged += OnModeChanged;

            // 根据初始模式设置状态
            UpdateCreatorState(Manager.Instance.currentMode);
        }
    }

    void OnDestroy()
    {
        // 取消订阅事件
        if (Manager.Instance != null)
        {
            Manager.Instance.ModeChanged -= OnModeChanged;
        }
    }

    void Update()
    {
        if (!canCreate) return;

        // 只在创造模式下允许创建物体
        if (onlyInCreativeMode && !Manager.Instance.IsMode(Manager.Mode.CreativeMode))
        {
            HidePreview(); //隐藏预览
            return;
        }

        HandlePrefabSelection(); //切换预制体

        HandleHeight(); //处理高度

        UpdatePreview(); //更新预制体

        HandleObjectCreation(); //创建物体
    }

    void HandleHeight()
    {
        if (previewObject == null || !isPreviewActive) return;

        float adjustment = 0f;
        if (Input.GetKey(KeyCode.UpArrow))
            adjustment += heightAdjustSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow)) 
            adjustment -= heightAdjustSpeed * Time.deltaTime;
        if(adjustment != 0f)
        {
            heightOffset += adjustment;
        }
    }

    void HandlePrefabSelection() //按下主键盘或小键盘123调用选择012
    {
        if(Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectPrefab(0);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SelectPrefab(1);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SelectPrefab(2);
        }
    }

    //当索引合法且预制体不为空，赋值当前索引，创建预览
    void SelectPrefab(int index) 
    {
        if (index >= 0 && index < prefabs.Length && prefabs[index] != null)
        {
            curPrefabIndex = index;
            CreatePreviewObject();
        }
    }

    //删除旧预览（如果现有预览物体不为空，销毁），如果当前索引的预制体为空，返回
    //实例化预览物体，取名；获取刚体并禁用重力，启用运动学；获取并禁用所有预制体的子碰撞体
    //设置透明材质，preview active
    void CreatePreviewObject()
    {
        Vector3 position = defaultPos;
        if(previewObject != null)
        {
            position = previewObject.transform.position;
            Destroy(previewObject);
        }
        if (prefabs[curPrefabIndex] == null)
        {
            return;
        }

        previewObject = Instantiate(prefabs[curPrefabIndex], position, Quaternion.identity);
        previewObject.name = "PreviewObject";

        if(previewObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if(previewObject.TryGetComponent<BombAbility>(out BombAbility bomb))
        {
            bomb.enabled = false;
        }

        Collider[] collider = previewObject.GetComponentsInChildren<Collider>();
        foreach(Collider col in collider)
        {
            col.enabled = false;
        }

        SetPreviewMaterial(previewObject);
        isPreviewActive = true;
    }

    //获取物体的所有renderer，每一个renderer，获取所有子材质，对每一个材质
    //如果预览材质不为空，使用材质，并修改颜色；将renderer的材质赋值
    void SetPreviewMaterial(GameObject obj)
    {
        Renderer[] renderer = obj.GetComponentsInChildren<Renderer>();
        foreach(Renderer render in renderer)
        {
            Material[] materials = render.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                if(previewMaterial != null)
                {
                    materials[i] = previewMaterial;
                    materials[i].color = previewColor;
                }
            }
            render.materials = materials;
        }
    }

    //如果预览没有开启或预览物体为空，则创建预览物体
    //如果预览物体为空，隐藏预览，返回
    //获取鼠标在世界空间的位置，检测鼠标指向的位置，将预览物体放在鼠标位置，并设置为启用，更新下落线
    void UpdatePreview()
    {
        if(!isPreviewActive || previewObject == null)
        {
            CreatePreviewObject();
            if (previewObject == null)
            {
                HidePreview();
                return;
            }
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 targetPos = hit.point + defaultPos + new Vector3(0, heightOffset, 0);
            previewObject.transform.position = targetPos;
            previewObject.SetActive(true);

            UpdateDropIndicator(targetPos);
        }
    }


    void CreateDropIndicator()
    {
        GameObject lineObj = new GameObject("DropIndicator");
        lineObj.transform.SetParent(transform);

        dropIndicator = lineObj.AddComponent<LineRenderer>();
        dropIndicator.startWidth = lineWidth;
        dropIndicator.endWidth = lineWidth;
        dropIndicator.material = new Material(Shader.Find("Sprites/Default"));
        dropIndicator.startColor = lineColor;
        dropIndicator.endColor = lineColor;
        dropIndicator.positionCount = 2;
        dropIndicator.enabled = false;
    }

    //indicator为空，返回；向下发射射线，绘制两点之间的线
    void UpdateDropIndicator(Vector3 startPos)
    {
        if (dropIndicator == null)
        {
            return;
        }

        Ray downRay = new Ray(startPos, Vector3.down);

        if(Physics.Raycast(downRay, out RaycastHit hit, maxDropDistance, groundLayer))
        {
            dropIndicator.SetPosition(0, startPos);
            dropIndicator.SetPosition(1, hit.point);
            dropIndicator.enabled = true;
        }
        else
        {
            dropIndicator.enabled = false;
        }
    }

    //点击左键且预览不为空，创建物体
    void HandleObjectCreation()
    {
        if(Input.GetMouseButtonDown(0) && previewObject != null)
        {
            CreateObjectAtPreviewPosition();
        }
    }

    //预览物体为空，返回；获取下落位置；实例化物体
    void CreateObjectAtPreviewPosition()
    {
        if (prefabs[curPrefabIndex] == null)
        {
            return;
        }

        GameObject newObj = Instantiate(prefabs[curPrefabIndex], previewObject.transform.position, Quaternion.identity);
        newObj.name = prefabs[curPrefabIndex].name;
    }

    //如果预览物体和预览线都不为空，隐藏，不启用预览
    void HidePreview()
    {
        if(previewObject != null)
            previewObject.SetActive(false);
        if(dropIndicator != null)
            dropIndicator.enabled = false;
        isPreviewActive = false;
    }

    private void OnModeChanged(Manager.Mode oldMode, Manager.Mode newMode)
    {
        UpdateCreatorState(newMode);
    }

    private void UpdateCreatorState(Manager.Mode mode)
    {
        if (onlyInCreativeMode)
        {
            canCreate = mode == Manager.Mode.CreativeMode;
        }
    }
}
