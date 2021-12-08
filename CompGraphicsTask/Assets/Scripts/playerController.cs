using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(CharacterController))]

public class playerController : MonoBehaviour
{
    float worldYRot = 0;
    float worldXRot = 0;

    float bodyYRot = 0;
    float targBodyYRot = 0;

    float gravity = 97f;
    float gravityAcceleration = 0f;

    bool isJumping = false;

    GameObject camPivot;
    // Start is called before the first frame update
    void Start()
    {

        

        camPivot = new GameObject();
        camPivot.transform.parent = gameObject.transform;
        camPivot.transform.localPosition = new Vector3(0,1.5f,0);
        GameObject cam = GameObject.FindGameObjectWithTag("MainCamera");
        cam.transform.parent = camPivot.transform;
        cam.transform.localPosition = new Vector3(0, 0, 3f);
        cam.transform.rotation = Quaternion.Euler(0, 180f, 0);

        Cube head = camPivot.AddComponent<Cube>();
        head.color = new Color(.5f, .5f, .7f);
        head.cubeSize = new Vector3(.75f, .75f, .75f);
        head.Generate();

    }

    float lerp(float a, float b, float x) {
        return a + (b - a) * x;
    }

    // Update is called once per frame
    void Update()
    {

        CharacterController characterController = gameObject.GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        worldXRot = Mathf.Clamp(worldXRot + Input.GetAxis("Mouse Y") * 2, -90, 90);
        worldYRot += Input.GetAxis("Mouse X")*2;
        camPivot.transform.rotation = Quaternion.Euler(worldXRot, worldYRot, 0);

        Vector3 forward = -camPivot.transform.forward;
        forward.y = 0;

        Vector3 sideways = -camPivot.transform.right;
        sideways.y = 0;

        bodyYRot = lerp(bodyYRot, targBodyYRot, 5*Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, bodyYRot, 0);

        Vector3 moveDirection = new Vector3();

        if (Input.GetKey(KeyCode.W))
        {
            targBodyYRot = worldYRot;
            moveDirection += forward.normalized * Time.deltaTime * 5;
        }

        if (Input.GetKey(KeyCode.S))
        {
            targBodyYRot = worldYRot;
            moveDirection += -forward.normalized * Time.deltaTime * 5;
        }

        if (Input.GetKey(KeyCode.A))
        {
            targBodyYRot = worldYRot;
            moveDirection += -sideways.normalized * Time.deltaTime * 5;
        }
        
        if (Input.GetKey(KeyCode.D))
        {
            targBodyYRot = worldYRot;
            moveDirection += sideways.normalized * Time.deltaTime * 5;
        }

        if (characterController.isGrounded)
        {
            gravityAcceleration = 0;
            isJumping = false;
        }
        else
        {
            gravityAcceleration += Time.deltaTime * 1f;
        }

        if (Input.GetKey(KeyCode.Space)) {
            isJumping = true;
        }


        if (isJumping) {
            moveDirection += new Vector3(0, 20 * Time.deltaTime, 0);
        }
        

        
        moveDirection.y -= (gravity*gravityAcceleration) * Time.deltaTime;
        characterController.Move(moveDirection);
    }
}
