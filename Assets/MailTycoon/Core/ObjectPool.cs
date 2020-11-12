using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPool<T> : MonoBehaviour where T : Component {

    public T prefab;
    private Queue<T> objects = new Queue<T> ();

    public T Get () {
        if (objects.Count == 0)
            AddObjects (1);
        var newObject = objects.Dequeue ();
        newObject.gameObject.SetActive (true);
        return newObject;
    }

    public void ReturnToPool (T objectToReturn) {
        objectToReturn.gameObject.SetActive (false);
        objectToReturn.transform.SetParent (transform);
        objects.Enqueue (objectToReturn);

    }

    public void AddObjects (int count) {
        for (int i = 0; i < count; i++) {
            var newObject = GameObject.Instantiate (prefab);
            newObject.transform.position = Vector3.zero;
            newObject.transform.SetParent (transform);
            newObject.gameObject.SetActive (false);
            objects.Enqueue (newObject);
        }
    }
}