
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using InfimaGames.LowPolyShooterPack;

public class PlayerMoves : MonoBehaviour
{
     [SerializeField]private float speed = 5f;
     [SerializeField]private float mouseSensitivity = 1f;
     [SerializeField]private Camera playerCamera;

     [SerializeField]private float fireRange = 100f;
     [SerializeField]private float fireInterval = 0.1f;
     
     [SerializeField]private LayerMask shootableLayer;
     [SerializeField]private ParticleSystem muzzleFlash;
     [SerializeField]private Animator weaponAnimator;

     [SerializeField] private Transform wpRoot;
    [SerializeField] private float wpBlockDist = 0.8f;
    [SerializeField] private Vector3 blockedWpOffset = new Vector3(0f, -0.2f, -0.25f);
    [SerializeField] private float wpSwaySpeed = 12f;

    private Vector3 originWpLocalPosition;

     private bool isFiring = false;
     private bool isSprinting = false;
     private float nextFireTime;//발사 간격

     
    
     private CharacterController controller;
     private Vector2 lookInput;
     private Vector2 moveInput;

     private Vector3 velocity;
    private float xRotation;

    private Animator animator;
     
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (muzzleFlash == null)
            muzzleFlash = GetComponentInChildren<ParticleSystem>(true);

        if (weaponAnimator == null)
            weaponAnimator = GetComponentInChildren<Animator>(true);

        if (wpRoot == null && weaponAnimator != null)
            wpRoot = weaponAnimator.transform;

        if (wpRoot != null)
            originWpLocalPosition = wpRoot.localPosition;
        animator = weaponAnimator;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (muzzleFlash != null)
            muzzleFlash.Stop();
    }

    // Update is called once per frame
    void Update()
    {
         HandleLook();
         HandleSprint();
         HandleMove();
         HandleFire();  
         HandleReload();
         HandleWeaponCollision();   
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
        float currentSpeed;
        if(isSprinting)
            currentSpeed = speed * 2f;
        else
            currentSpeed = speed;
        controller.Move(moveDir * currentSpeed * Time.deltaTime);
        
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
     public void OnFire(InputAction.CallbackContext context)
     {
         if (context.started)
             isFiring = true;
         else if (context.canceled)
             isFiring = false;

         if (isFiring)
             HandleFire();
     }
        private void HandleFire()
        {
            if(!isFiring)
                return;

            if (Time.time < nextFireTime)
                return;

            nextFireTime = Time.time + fireInterval;

            if(animator != null)
                animator.SetTrigger("Fire");

            if (muzzleFlash != null)
            {
                muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                muzzleFlash.Play();
            }

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Debug.DrawRay(ray.origin, ray.direction * fireRange, Color.red, 1f);
            if (Physics.Raycast(ray, out RaycastHit hit, fireRange, shootableLayer))
            {
                Debug.Log($"Hit: {hit.collider.name}");

            }
        }

        //sprint 메서드
        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
                isSprinting = true;  
            else if (context.canceled)
                isSprinting = false;  
        }
        private void HandleSprint()
        {
            isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        }
        public void OnReload(InputAction.CallbackContext context)
        {
            if (context.started)
                HandleReload();
        }
        private void HandleReload()
        {
            //재장전 애니메이션 트리거
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (animator != null)
                    animator.SetTrigger("Reload");
                Debug.Log("Reloading...");
            }
        }
       
        private void HandleWeaponCollision()
        {
            if (wpRoot == null)
                return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, wpBlockDist))
            {
                Vector3 targetPos = originWpLocalPosition + blockedWpOffset;
                wpRoot.localPosition = Vector3.Lerp(wpRoot.localPosition, targetPos, Time.deltaTime * wpSwaySpeed);
            }
            else
            {
                wpRoot.localPosition = Vector3.Lerp(wpRoot.localPosition, originWpLocalPosition, Time.deltaTime * wpSwaySpeed);
            }
        }
    

 
        
}
