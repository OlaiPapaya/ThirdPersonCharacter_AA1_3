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
    public Transform bulletsContainer;
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
    private InputSystem_Actions input;
    private float lastShotTime = float.MinValue;
    private float lastReloadTime = float.MinValue;
    private void Start()
    {
        input = new();
        input.Player.Enable();

        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, positionRecover * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, originalRotation, rotationRecover * Time.deltaTime);

        PlaceRealAim();

        HandleFire();
    }
    void PlaceRealAim()
    {
        // These references cannot be null
        if (!canvasTransform || !realAim || !firePoint) return;

        Vector3 pointPosition;

        // Raycast from the weapon fire point to its forward direction
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, maxFireDistance))
        {
            // If the raycast hit something, that's where we're pointing.
            pointPosition = hit.point;
        }
        else
        {
            // If not, we'll just point forward, to the max shooting distance.
            pointPosition = firePoint.position + firePoint.forward * maxFireDistance;
        }

        // We convert the world position into a screen position.
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, pointPosition);

        // We convert the screen position to the local UI object's coordinate system,
        // checking if the conversion worked.
        // https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/recttransformutility/screenpointtoworldpointinrectangle
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform, screenPoint, null, out Vector2 anchoredPoint))
            return;

        // Once we have the local UI's coordinate system position, we assign it:
        realAim.anchoredPosition = anchoredPoint;
    }
    void HandleFire()
    {
        if (input.Player.Attack.IsPressed() &&
            Time.time - lastShotTime > fireRate &&
            Time.time - lastReloadTime > reloadTime)
        {
            Fire();
            lastShotTime = Time.time;
            clipCurrent--;
            if (clipCurrent <= 0)
            {
                Reload();
            }
        }
        if (input.Player.Reload.WasPressedThisFrame())
            Reload();
    }
    void Reload()
    {
        clipCurrent = clipMax;
        lastReloadTime = Time.time;
    }
    public void Fire()
    {
        Destroy(Instantiate(bullet, firePoint.position, firePoint.rotation, bulletsContainer == null ? null : bulletsContainer), 10);
        onFire.Invoke();
        StartCoroutine(Knockback_Corutine());
    }
    IEnumerator Knockback_Corutine()
    {
        yield return null;
        transform.localPosition -= new Vector3(Random.Range(-knockbackPosition.x, knockbackPosition.x), Random.Range(0, knockbackPosition.y), Random.Range(-knockbackPosition.z, -knockbackPosition.z * .5f));
        transform.localEulerAngles -= new Vector3(Random.Range(knockbackRotation.x * 0.5f, knockbackRotation.x), Random.Range(-knockbackRotation.y, knockbackRotation.y), Random.Range(-knockbackRotation.z, knockbackRotation.z));
    }
    private void OnDestroy()
    {
        input.Player.Disable();
    }
}
