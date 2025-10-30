using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f; //camera movement speed
    public bool useSmoothMovement = true;
    public float smoothSpeed = 5f; //camera speed changes smoothly
    float minX = -50f, maxX = 50f;
    float minY = 0f, maxY = 50f;
    float minZ = -50f, maxZ = 50f;

    public float rotationSpeed = 3f;
    public bool enableRotation = true;

    public float zoomSpeed = 10f;
    public float minZoom = 2f, maxZoom = 10f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float targetZoom;
    private float curZoom;
    private Vector3 lastMousePos;

    private bool isRotating = false;

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        curZoom = transform.position.y;
        targetZoom = curZoom;
        ClampPosition();
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleMouseInput();
        HandleZoom();
    }

    void HandleMovement()
    {
        Vector3 movement = Vector3.zero;
        Vector3 camForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        // WASD 移动
        if (Input.GetKey(KeyCode.W))
            movement += camForward;
        if (Input.GetKey(KeyCode.S))
            movement -= camForward;
        if (Input.GetKey(KeyCode.A))
            movement -= camRight;
        if (Input.GetKey(KeyCode.D))
            movement += camRight;

        // QE 上下移动
        if (Input.GetKey(KeyCode.Q))
            movement += Vector3.down;
        if (Input.GetKey(KeyCode.E))
            movement += Vector3.up;

        if (movement != Vector3.zero)
        {
            movement.Normalize();
            targetPosition += movement * moveSpeed * Time.deltaTime;
            ClampPosition();
        }

        if (useSmoothMovement)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            lastMousePos = Input.mousePosition;
            Cursor.visible = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
            Cursor.visible = true;
        }

        if (isRotating)
        {
            HandleRotation();
        }
    }

    void HandleRotation()
    {
        //定义鼠标移动的位移差，x,y旋转与位移正交
        //存储当前的欧拉角，对其分别加上x，y方向的旋转，限制欧拉角x到-180~180，并且其x应在-89~89
        //将欧拉角转换为四元组到旋转目标中，赋值上一帧的鼠标位置
        Vector3 mouseDelta = Input.mousePosition - lastMousePos;
        float rotationX = -mouseDelta.y * rotationSpeed * 0.1f;
        float rotationY = mouseDelta.x * rotationSpeed * 0.1f;

        Vector3 curEulerAngle = targetRotation.eulerAngles;
        curEulerAngle.x += rotationX;
        curEulerAngle.y += rotationY;

        if (curEulerAngle.x > 180)
        {
            curEulerAngle.x -= 360;
        }
        curEulerAngle.x = Mathf.Clamp(curEulerAngle.x, -89f, 89f);

        targetRotation = Quaternion.Euler(curEulerAngle);
        lastMousePos = Input.mousePosition;

        transform.rotation = targetRotation;
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            Vector3 zoomDirection = transform.forward * scroll * zoomSpeed;
            targetPosition += zoomDirection;
            ClampPosition();
        }
    }

    void ClampPosition()
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);
    }
}
