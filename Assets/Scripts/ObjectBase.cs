using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    public float force = 10f;
    public float stopFactor = 4f;
    public Manager.Mode mode;
    public bool isDragging = false;
    public GameObject targetObj = null;
    public float maxSpeed = 10f;

    Vector3 moveDir;
    Vector3 dragMouseStartPos;
    Vector3 lastMousePos;
    Vector3 worldDelta;
    protected Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;

        if(Manager.Instance != null)
        {
            Manager.Instance.ModeChanged += OnModeChanged;
            OnModeChanged(mode, Manager.Instance.currentMode);
        }
    }

    private void OnDestroy()
    {
        if(Manager.Instance != null)
        {
            Manager.Instance.ModeChanged -= OnModeChanged;
        }
    }

    private void OnModeChanged(Manager.Mode oldMode, Manager.Mode newMode)
    {
        mode = newMode;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            if(rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = maxSpeed * rb.velocity.normalized;
            }
        }

        if (mode == Manager.Mode.FreeMode)
        {
            DragMove();
        }
    }

    protected virtual bool TrySelectTarget()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            if (raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("TargetObject"))
            {
                targetObj = raycastHit.collider.gameObject;
                if (targetObj.TryGetComponent<Rigidbody>(out _))
                {
                    return true;
                }
            }
        }
        targetObj = null;
        return false;
    }

    protected void DragMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (TrySelectTarget())
            {
                isDragging = true;
                dragMouseStartPos = Input.mousePosition;
                lastMousePos = Input.mousePosition;
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
            if (targetObj!=null && targetObj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(worldDelta.normalized * force / stopFactor, ForceMode.Impulse);
            }
            isDragging = false;
            targetObj = null;
        }
    }
}
