using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GenericGun : MonoBehaviour
{
    [Header("Weapon")]
    public int clipMax = 30;
    public int clipCurrent = 30;
    public bool automatic = true;
    [Min(1f/60f)]
    public float fireRate = 0.1f;
    public float reloadTime = 0.5f;
    [Header("Firing")]
    public UnityEvent onFire;
    public Transform firePoint;
    public GameObject bullet;
    public float maxFireDistance = 1000f;
    [Header("Animation")]
    public float positionRecover;
    public float rotationRecover;
    public Vector3 knockbackPosition;
    public Vector3 knockbackRotation;
    [Header("UI")]
    public RectTransform realAim;
    public RectTransform canvasTransform;
    Vector3 originalPosition;
    Quaternion originalRotation;
    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, positionRecover * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, rotationRecover * Time.deltaTime);

        PlaceRealAim();
    }
    public void PlaceRealAim()
    {
        // These references cannot be null
        if (!canvasTransform || !realAim || !firePoint) return;

        Vector3 pointPosition;

        // If the raycast doesn't hit anything, we don't move the RealAim object
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, maxFireDistance))
        {
            pointPosition = hit.point;
        }
        else
        {
            pointPosition = firePoint.position + firePoint.forward * maxFireDistance;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, pointPosition);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform, screenPoint, null, out Vector2 anchoredPoint))
            return;

        realAim.anchoredPosition = anchoredPoint;
    }
    public void Fire()
    {
        Destroy(Instantiate(bullet, firePoint.position, firePoint.rotation), 10);
        onFire.Invoke();
        StartCoroutine(Knockback_Corutine());
    }
    IEnumerator Knockback_Corutine()
    {
        yield return null;
        transform.localPosition -= new Vector3(Random.Range(-knockbackPosition.x, knockbackPosition.x), Random.Range(0, knockbackPosition.y), Random.Range(-knockbackPosition.z, -knockbackPosition.z * .5f));
        transform.localEulerAngles -= new Vector3(Random.Range(knockbackRotation.x * 0.5f, knockbackRotation.x), Random.Range(-knockbackRotation.y, knockbackRotation.y), Random.Range(-knockbackRotation.z, knockbackRotation.z));
    }
}
