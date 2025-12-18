using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Animator anim;
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        
    }
}
