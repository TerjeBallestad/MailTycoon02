using UnityEngine;

public class MoveTransform : MonoBehaviour, IMoveVelocity {
    [SerializeField] private float speed = 4;
    private Vector3 direction;

    public void SetDirection (Vector3 direction) {
        this.direction = direction;
    }

    public void SetSpeed (float speed) {
        this.speed = speed;
    }

    private void Update () {
        transform.position += direction * speed * Time.deltaTime;
    }
}