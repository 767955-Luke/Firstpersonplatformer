using UnityEngine;



public class MoveAndSpin : MonoBehaviour

{

    public float speed = 5f;

    public float spinSpeed = 80f;



    void Update()

    {

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);

    }

}