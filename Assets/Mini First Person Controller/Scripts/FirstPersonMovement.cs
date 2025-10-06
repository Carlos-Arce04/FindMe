using System.Collections.Generic;
using UnityEngine;


public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;

    [Header("Running")]
    public bool canRun = true;
    public bool canMove = true;
    public bool IsRunning { get; private set; }
    public float runSpeed = 9;
    public KeyCode runningKey = KeyCode.LeftShift;

    [Header("Stamina")]
    public float maxStamina = 10.0f;
    public float staminaRegenDelay = 4.0f;
    public float staminaRegenRate = 2.0f;

    // --- UI --- barra de estamina
    [Header("UI")]
    public ProgressBar staminaBar;

    Rigidbody rigidbody;
    public List<System.Func<float>> speedOverrides = new List<System.Func<float>>();
    
    private float currentStamina;
    private float staminaRegenTimer;
    private bool isExhausted = false;


    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        currentStamina = maxStamina;
    }
    
    void Update()
    {
        if (!canMove) return;

        HandleStamina();

        // --- UI ---  barra de estamina
        if (staminaBar != null)
        {
            // Calculo de estamina como un porcentaje (de 0 a 100)
            float staminaPercentage = (currentStamina / maxStamina) * 100;
            
            // pasamos ese porcentaje a la variable pública "BarValue" del script del asset
            staminaBar.BarValue = staminaPercentage;
        }

        
        IsRunning = canRun && Input.GetKey(runningKey) && !isExhausted;

        float targetMovingSpeed = IsRunning ? runSpeed : speed;
        if (speedOverrides.Count > 0)
        {
            targetMovingSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        Vector2 targetVelocity = new Vector2(Input.GetAxis("Horizontal") * targetMovingSpeed, Input.GetAxis("Vertical") * targetMovingSpeed);
        
        ApplyMovement(targetVelocity);
    }

    
    void FixedUpdate() { }

    void ApplyMovement(Vector2 targetVelocity)
    {
        if(rigidbody != null)
        {
            rigidbody.linearVelocity = transform.rotation * new Vector3(targetVelocity.x, rigidbody.linearVelocity.y, targetVelocity.y);
        }
    }

    void HandleStamina()
    {
        bool isTryingToRun = Input.GetKey(runningKey);
        bool isMoving = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).sqrMagnitude > 0;

        //recarga estamina mientras camina
        if (isTryingToRun && isMoving && !isExhausted)
        {
            if (currentStamina > 0.0f)
            {
                currentStamina -= Time.deltaTime;
            }
            
            if (currentStamina <= 0.0f)
            {
                currentStamina = 0.0f;
                isExhausted = true;
            }
            
            staminaRegenTimer = 0.0f;
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                if (staminaRegenTimer < staminaRegenDelay)
                {
                    staminaRegenTimer += Time.deltaTime;
                }
                else
                {
                    currentStamina += staminaRegenRate * Time.deltaTime;
                    currentStamina = Mathf.Min(currentStamina, maxStamina);

                    if (currentStamina >= maxStamina)
                    {
                        isExhausted = false;
                    }
                }
            }
        }
    }
}