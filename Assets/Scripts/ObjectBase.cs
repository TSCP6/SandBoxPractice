using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    public float force = 10f;
    public float stopFactor = 4f;
    public Manager.Mode mode;

    bool isDragging = false;
    Vector3 moveDir;
    Vector3 dragMouseStartPos;
    Vector3 lastMousePos;
    Vector3 worldDelta;
    Camera mainCamera;
    GameObject targetObj = null;

    // Start is called before the first frame update
    void Start()
    {
        mode = Manager.Mode.FreeMode;
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (mode == Manager.Mode.FreeMode)
        {
            DragMove();
        }
    }

    void DragMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                targetObj = raycastHit.collider.gameObject;
                if (targetObj.layer == LayerMask.NameToLayer("TargetObject"))
                {
                    targetObj = raycastHit.collider.gameObject;
                    if (targetObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
                    {
                        isDragging = true;
                        dragMouseStartPos = Input.mousePosition;
                        lastMousePos = Input.mousePosition;
                    }
                    else
                    {
                        targetObj = null;
                    }
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging && targetObj != null)
        {
            Vector3 curMousePos = Input.mousePosition;
            Vector3 mouseDelta = curMousePos - lastMousePos;

            if (mouseDelta.magnitude > 0.1f)
            {
                float distance = Vector3.Distance(mainCamera.transform.position, targetObj.transform.position);

                worldDelta = mainCamera.ScreenToWorldPoint(new Vector3(mouseDelta.x, mouseDelta.y, distance))
                                   - mainCamera.ScreenToWorldPoint(new Vector3(0, 0, distance));

                if (targetObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    rb.AddForce(worldDelta.normalized * force , ForceMode.Force);
                }

            }
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (targetObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(worldDelta.normalized * force / stopFactor, ForceMode.Impulse);
            }
            isDragging = false;
            targetObj = null;
        }
    }
}
