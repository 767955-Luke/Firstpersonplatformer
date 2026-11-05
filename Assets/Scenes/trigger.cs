using Unity.VisualScripting;
using UnityEngine;

public class trigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected!");
        }
    }
}

