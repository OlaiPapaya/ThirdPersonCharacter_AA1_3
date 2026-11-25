using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletsPool : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;

    private Stack<GameObject> pooledObjects = new();
    private List<GameObject> existingObjects = new();

    private List<KeyValuePair<GameObject, float>> timeToPoolObjects = new();

    private void Update()
    {
        for (int i = 0; i < timeToPoolObjects.Count; i++)
        {
            KeyValuePair<GameObject, float> tmp = new(
                timeToPoolObjects[i].Key, timeToPoolObjects[i].Value - Time.deltaTime);

            timeToPoolObjects[i] = tmp;

            if (tmp.Value <= 0)
            {
                DestroyBullet(timeToPoolObjects[i].Key);
                timeToPoolObjects.RemoveAt(i);
                i--;
            }
        }
    }

    public GameObject CreateBullet(Vector3 position, Quaternion rotation)
    {
        GameObject newObj;
        if (pooledObjects.Count <= 0)
        {
            // If there aren't inactive bullets that can be activated,
            // we create a new one:
            newObj = Instantiate(bulletPrefab, position, rotation, transform);
        }
        else
        {
            // If there are inactive bullets to be activated, we use those ones:
            newObj = pooledObjects.Pop();
            newObj.transform.position = position;
            newObj.transform.rotation = rotation;
            newObj.SetActive(true);
        }
        existingObjects.Add(newObj);

        Projectile bulletScript = newObj.GetComponentInChildren<Projectile>();
        if (bulletScript != null) bulletScript.Init();

        return newObj;
    }

    public void DestroyBullet(GameObject obj)
    {
        // Return the bullet to the inactive bullets stack:
        if (!existingObjects.Contains(obj)) return;
        existingObjects.Remove(obj);
        obj.SetActive(false);
        pooledObjects.Push(obj);
    }

    public void TimedDestroyBullet(GameObject obj, float time)
    {
        // Schedule a bullet to be deactivated later:
        KeyValuePair<GameObject, float> tmp = new KeyValuePair<GameObject, float>(obj, time);
        timeToPoolObjects.Add(tmp);
    }
}
