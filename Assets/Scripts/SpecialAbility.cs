using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class SpecialAbility : ObjectBase
{
    public float range = 5f;
    public float specialForce = 5f;
    public float stopTime = 0.5f;
    public enum Ability { attract, repulsion };
    public Ability ability = Ability.attract;

    private Coroutine stopObj = null;
    Collider[] neighborObjs;

    protected override void Update()
    {
        HandleSpecialAbility();
    }

    private void HandleSpecialAbility()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (TrySelectTarget())
            {
                isDragging = true;
                if (gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
                {
                    //如果正在停止，终止
                    if (stopObj != null)
                    {
                        StopCoroutine(stopObj);
                        stopObj = null;
                    }
                    rb.isKinematic = false;
                }
            }
        }

        if (isDragging)
        {
            DragMove();
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                if (stopObj != null)
                {
                    StopCoroutine(stopObj);
                }
                stopObj = StartCoroutine(SmoothStop(rb));
            }
            targetObj = null;
        }

        Vector3 center = transform.position;
        neighborObjs = Physics.OverlapSphere(center, range);
        foreach (Collider collider in neighborObjs)
        {
            if (collider.gameObject.tag == "CommonOBJ")
            {
                ApplyForce(ability, collider, center);
            }
        }
        if (mode == Manager.Mode.FreeMode)
            ChangeSpecial();
    }

    IEnumerator SmoothStop(Rigidbody rb)
    {
        float elapsedTime = 0f;
        Vector3 initialVelocity = rb.velocity;
        while (elapsedTime < Time.deltaTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / stopTime;
            t = 1 - Mathf.Pow(1 - t, 3);
            rb.velocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
            yield return null;
        }

        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        stopObj = null;
    }

    protected override bool TrySelectTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject.tag == "SpecialOBJ")
            {
                targetObj = hit.collider.gameObject;
                return true;
            }
        }
        targetObj = null;
        return false;
    }

    void ApplyForce(Ability ability, Collider collider, Vector3 center)
    {
        collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb);
        Vector3 forceDir = (center - collider.transform.position).normalized;
        if (ability == Ability.attract)
        {
            rb.AddForce(specialForce * forceDir);
        }
        else if (ability == Ability.repulsion)
        {
            rb.AddForce(-specialForce * forceDir);
        }
    }

    void ChangeSpecial()
    {
        if (targetObj != null && targetObj.tag == "SpecialOBJ" && Input.GetKeyDown(KeyCode.R))
        {
            if (targetObj.TryGetComponent<Renderer>(out Renderer renderer))
            {
                if (ability == Ability.attract)
                {
                    ability = Ability.repulsion;
                    renderer.material.color = Color.white;
                }
                else if (ability == Ability.repulsion)
                {
                    ability = Ability.attract;
                    renderer.material.color = Color.black;
                }
            }
        }
    }
}
