using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpecialAbility;

public class BombAbility : ObjectBase
{
    public float bombForce = 100f;
    public float range = 5f;
    public float blinkTime = 3f;
    public float blinkInterval = 0.5f;

    Renderer bombRenderer;
    Collider[] neighborObjs;

    private void Start()
    {
        bombRenderer = GetComponent<Renderer>();
            StartCoroutine(BlinkAndExplode());
    }

    IEnumerator BlinkAndExplode()
    {
        float curTime = 0f;
        while (curTime < blinkTime)
        {
            if (bombRenderer != null)
            {
                bombRenderer.material.color = Color.red;
            }
            yield return new WaitForSeconds(blinkInterval);
            if (bombRenderer != null)
            {
                bombRenderer.material.color = Color.black;
            }
            yield return new WaitForSeconds(blinkInterval);
            curTime += 2 * blinkInterval;
        }

        Explode();
    }

    void Explode()
    {
        Vector3 center = transform.position;
        neighborObjs = Physics.OverlapSphere(center, range);
        foreach (Collider collider in neighborObjs)
        {
            if (collider.gameObject.tag == "CommonOBJ")
            {
                ApplyForce(collider, center);
            }
        }

        ObjectPool.Instance.Return(gameObject);
        Destroy(gameObject);
    }

    void ApplyForce(Collider collider, Vector3 center)
    {
        if (collider.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            Vector3 forceDir = (collider.transform.position - center).normalized;
            rb.AddForce(bombForce * forceDir, ForceMode.Impulse);
        }
    }
}
