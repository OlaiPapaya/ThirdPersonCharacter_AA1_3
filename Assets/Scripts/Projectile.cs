using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float disappearTime = 5f;
    public Vector3 forceMin = new Vector3(-1, -1, 50);
    public Vector3 forceMax = new Vector3(1, 1, 100);
    public LayerMask layers;
    public float collisionForceMultiplier = 2f;
    public float radius = .1f;
    [HideInInspector]
    public Rigidbody rb;
    [SerializeField] private GameObject decalPrefab;
    [SerializeField] private List<ParticleSystem> particles;
    [SerializeField] private TrailRenderer trail;

    Vector3 lastPos;
    void Start()
    {
        
    }

    public void Init()
    {
        foreach (ParticleSystem particle in particles)
        {
            particle.Stop();
            particle.Clear();
        }

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (trail == null) trail = GetComponent<TrailRenderer>();
        trail.Clear();

        if (!rb) rb = GetComponent<Rigidbody>();

        // Push the new transform to the physics body BEFORE applying force
        Physics.SyncTransforms();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddRelativeForce(new Vector3(
            Random.Range(forceMin.x, forceMax.x),
            Random.Range(forceMin.y, forceMax.y),
            Random.Range(forceMin.z, forceMax.z)));

        lastPos = transform.position;

        foreach (ParticleSystem particle in particles)
            particle.Play();
    }

    private void FixedUpdate()
    {
        Vector3 dir = transform.position - lastPos;

        Debug.DrawRay(lastPos, dir, Color.blue, disappearTime);

        RaycastHit hit;

        if (Physics.SphereCast(lastPos, radius, dir, out hit, dir.magnitude, layers))
        {
            Hitted(hit);
        }

        lastPos = transform.position;
    }

    void Hitted(RaycastHit hit)
    {
        if (hit.rigidbody)
        {
            hit.rigidbody.AddForceAtPosition(rb.linearVelocity * rb.mass * collisionForceMultiplier, this.transform.position);
        }
        GameObject decal = Instantiate(decalPrefab, hit.point, Quaternion.identity, hit.transform);
        decal.transform.forward = hit.normal;
        Destroy(decal, 5);
    }
}
