using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class SpecialAbility : ObjectBase
{
    public float range = 5f;
    public float specialForce = 5f;
    public enum Ability { attract, repulsion };
    public Ability ability = Ability.attract;

    Collider[] neighborObjs;

    private void Update()
    {
        Vector3 center = transform.position;
        neighborObjs = Physics.OverlapSphere(center, range);
        AttractionAndRepulsion(center);
        ChangeSpecial();
    }

    void AttractionAndRepulsion(Vector3 center)
    {
        gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        rb.freezeRotation = true;
        if (isDragging == false || (Input.GetMouseButtonUp(0) && rb.isKinematic == false))
        {
            targetObj = null;
            rb.isKinematic = true;
        }
        if (Input.GetMouseButtonDown(0) && rb.isKinematic == true)
        {
            Physics.Raycast(ray, out RaycastHit hit);
            if (hit.collider.gameObject.tag == "SpecialOBJ")
                targetObj = hit.collider.gameObject;
            else targetObj = null;
            isDragging = true;
            rb.isKinematic = false;
        }
        foreach (Collider collider in neighborObjs)
        {
            if (collider.gameObject.tag == "CommonOBJ")
            {
                ApplyForce(ability, collider, center);
            }
        }
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
        if (Input.GetKeyDown(KeyCode.R) && targetObj.tag == "SpecialOBJ")
        {
            if(targetObj.TryGetComponent<Renderer>(out  Renderer renderer))
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
