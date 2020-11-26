using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneHubController : MonoBehaviour
{

    private Animator _animator;


    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            _animator.SetBool("IsOpen", true);
        } else if (Input.GetKeyDown(KeyCode.H))
        {
            _animator.SetBool("IsOpen", false);
        }
    }
}
