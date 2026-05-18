
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class PlayerMoves : MonoBehaviour
{
     [SerializeField]private float speed = 5f;
     [SerializeField]private float mouseSensitivity = 1f;
     [SerializeField]private Camera playerCamera;
     
    
     private CharacterController controller;
     private Vector2 lookInput;
     private Vector2 moveInput;

     private Vector3 velocity;
    private float xRotation;
     
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
         
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         HandleLook();
         HandleMove();
    }
    
    public void OnMove(InputAction.CallbackContext context)
     {
         moveInput = context.ReadValue<Vector2>();
     }
    public void OnLook(InputAction.CallbackContext context)
     {
         lookInput = context.ReadValue<Vector2>();
     }
     public void HandleMove()
     {
        Vector3 moveDir = transform.right * moveInput.x + 
                        transform.forward * moveInput.y;
        controller.Move(moveDir * speed * Time.deltaTime);
        
        /// 3. 지면 감지 후 중력 처리
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;  
        
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
     }
     public void HandleLook()
     {
         float mouseX = lookInput.x * mouseSensitivity;
         float mouseY = lookInput.y * mouseSensitivity;

         xRotation -= mouseY;
         xRotation = Mathf.Clamp(xRotation, -90f, 90f);
         
         playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

         transform.Rotate(Vector3.up * mouseX);
     }
    
      
}
