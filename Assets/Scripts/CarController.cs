using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D frontTire; // Reference to the front tire Rigidbody2D
    [SerializeField] private Rigidbody2D backTire;  // Reference to the back tire Rigidbody2D
    [SerializeField] private Rigidbody2D carRigidBody; // Reference to the car's main Rigidbody2D
    [SerializeField] private float speed = 15f;    // Speed multiplier for tire movement
    [SerializeField] private float carTorque = 300f; // Torque applied to the car's body for rotation
    [SerializeField] private float maxSpeed = 120f; // Maximum speed of the car

    private float movement = 0f; // Player's movement input
    private float fuel = 1f; // Fuel level (1 = full fuel)
    private float fuelConsumption = 0.1f; // Fuel consumption rate
    private float idleFuelConsumption = 0.03f;


    public float Fuel
    {
        get => fuel;
        set { fuel = Mathf.Clamp01(value); } // Ensure fuel is between 0 and 1
    }

    //public bool moveStop = false; // Stops the car when true
    public Vector3 StartPos { get; set; } // Stores the car's starting position

    private void Update()
    {
        // Disable input processing if the game is over
        if (GameManager.Instance.isDie)
        {
            movement = Mathf.MoveTowards(movement, 0f, 0.02f);
            return; // Exit the method to prevent further processing
        }
        else if (GameManager.Instance.GasBtnPressed)
        {
            movement += 0.009f;
            movement = Mathf.Clamp(movement, -1f, 1f); // Clamp movement between -1 and 1
        }
        else if (GameManager.Instance.BrakeBtnPressed)
        {
            movement -= 0.009f;
            movement = Mathf.Clamp(movement, -1f, 1f);
        }
        else
        {
            // Gradually return movement to 0 when no button is pressed
            movement = Mathf.MoveTowards(movement, 0f, 0.02f);
        }
        // Consume fuel based on movement
        GameManager.Instance.FuelConsume();
    }

    private void FixedUpdate()
    {
        // Prevent the car from moving backward beyond its starting position
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, StartPos.x, Mathf.Infinity),
            transform.position.y
        );

        if (movement == 0 || fuel <= 0)
        {
            // Stop tires when not moving or out of fuel
            backTire.angularVelocity = Mathf.MoveTowards(backTire.angularVelocity, 0f, 7000f * Time.fixedDeltaTime);
            frontTire.angularVelocity = Mathf.MoveTowards(frontTire.angularVelocity, 0f, 7000f * Time.fixedDeltaTime);
        }
        else
        {
            // Apply torque to the tires for movement
            float currentSpeed = carRigidBody.velocity.magnitude; // Calculate current speed

            if (currentSpeed < maxSpeed) // Check if speed is below the maximum
            {
                backTire.AddTorque(-movement * speed * Time.fixedDeltaTime);
                frontTire.AddTorque(-movement * speed * Time.fixedDeltaTime);
            }

            // Apply torque to the car body for rotation
            carRigidBody.AddTorque(movement * carTorque * Time.fixedDeltaTime);
        }

        // Stop the car completely when the game ends
        /*      if (GameManager.Instance.isDie && moveStop)
              {
                  carRigidBody.velocity = Vector2.zero;
                  carRigidBody.angularVelocity = 0f;
              }*/

        // Reduce fuel as the car moves
        if (Mathf.Abs(movement) > 0f)
        {
            fuel -= fuelConsumption * Mathf.Abs(movement) * Time.fixedDeltaTime; // Higher consumption when moving
        }
        else
        {
            fuel -= idleFuelConsumption * Time.fixedDeltaTime; // Slower consumption when idle
        }

        fuel = Mathf.Clamp01(fuel); // Keep fuel level between 0 and 1
    }
}