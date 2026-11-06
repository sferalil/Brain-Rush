
using UnityEngine;

public class AvatarController : MonoBehaviour
{
    public float speed = 5f;

    public float leftLimit = -6f;
    public float rightLimit = 6f;

    void Update()
    {
        float direction = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            direction = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            direction = 1f;
        }

        float newX = transform.position.x +
                     direction * speed * Time.deltaTime;

        newX = Mathf.Clamp(newX, leftLimit, rightLimit);

        transform.position = new Vector3(
            newX,
            transform.position.y,
            transform.position.z
        );

        if (direction < 0)
        {
            transform.rotation = Quaternion.Euler(0, -10, 0);
        }
        else if (direction > 0)
        {
            transform.rotation = Quaternion.Euler(0, 10, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
