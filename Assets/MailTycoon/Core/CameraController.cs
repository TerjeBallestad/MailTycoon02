using UnityEngine;

public class CameraController : MonoBehaviour {
    public float PanSpeed = 20f;
    public float PanBorderThickness = 10f;
    public Vector2 PanLimit;

    void Update () {
        Vector3 position = transform.position;

        if (Input.GetKey ("w") || Input.mousePosition.y >= Screen.height - PanBorderThickness) {
            position.y += PanSpeed * Time.deltaTime;
        }
        if (Input.GetKey ("s") || Input.mousePosition.y <= PanBorderThickness) {
            position.y -= PanSpeed * Time.deltaTime;
        }
        if (Input.GetKey ("d") || Input.mousePosition.x >= Screen.width - PanBorderThickness) {
            position.x += PanSpeed * Time.deltaTime;
        }
        if (Input.GetKey ("a") || Input.mousePosition.x <= PanBorderThickness) {
            position.x -= PanSpeed * Time.deltaTime;
        }

        // position.x = Mathf.Clamp(position.x, position  )

        transform.position = position;
    }
}