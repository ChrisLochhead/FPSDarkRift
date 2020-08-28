using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    [Header("References")]
    public CharacterController CharacterController;

    [Header("Shared Variables")]
    public float WalkSpeed;
    public float GravityConstant;
    public float JumpStrength;

    private Vector3 gravity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public PlayerUpdateData GetNextFrameData(PlayerInputData input, PlayerUpdateData currentUpdateData)
    {
        bool w = input.Keyinputs[0];
        bool a = input.Keyinputs[1];
        bool s = input.Keyinputs[2];
        bool d = input.Keyinputs[3];
        bool space = input.Keyinputs[4];
        bool left = input.Keyinputs[5];

        Vector3 rotation = input.LookDirection.eulerAngles;
        gravity = new Vector3(0, currentUpdateData.Gravity, 0);

        Vector3 movement = Vector3.zero;
        if (w)
        {
            movement += Vector3.forward;
        }
        if (a)
        {
            movement += Vector3.left;
        }
        if (s)
        {
            movement += Vector3.back;
        }
        if (d)
        {
            movement += Vector3.right;
        }

        movement = Quaternion.Euler(0, rotation.y, 0) * movement;
        movement.Normalize();
        movement = movement * WalkSpeed;

        movement = movement * Time.fixedDeltaTime;
        movement = movement + gravity * Time.fixedDeltaTime;

        Debug.Log("Position bfore" + transform.position);
        CharacterController.Move(new Vector3(0, -0.001f, 0));
        Debug.Log("Position then" + transform.position);
        if (CharacterController.isGrounded)
        {
            if (space)
            {
                gravity = new Vector3(0, JumpStrength, 0);
            }
        }
        else
        {
            gravity -= new Vector3(0, GravityConstant, 0);
        }
        Debug.Log("before position : " + transform.localPosition + "and movement : " + movement);
        CharacterController.Move(movement);
        Debug.Log("calculated position : " + transform.position);
        return new PlayerUpdateData(currentUpdateData.Id, gravity.y, transform.position, input.LookDirection);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
