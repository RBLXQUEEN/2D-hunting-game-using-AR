using UnityEngine;
using UnityEngine.InputSystem;

public class Movements : MonoBehaviour
{
    InputAction MoveAction;
    Rigidbody2D myrigidbody2D;
    ScoreManager scoreManager;
    [SerializeField] float torqueAmount=1f;
    [SerializeField] float basespeed=15f;
    [SerializeField] float boostspeed=20f;
    SurfaceEffector2D surface;
    public int flip=0;
    float total_rotation;
    public float prev_rotations;
    void Start()
    {
        MoveAction = InputSystem.actions.FindAction("Move");
        myrigidbody2D = GetComponent<Rigidbody2D>();
        
        surface = FindFirstObjectByType<SurfaceEffector2D>();
        scoreManager=FindFirstObjectByType<ScoreManager>();
        
        prev_rotations = transform.rotation.eulerAngles.z;
    }
    Vector2 moveVector;
    bool playcontrol=true;

    
    void Update()
    {
        if (playcontrol)
        {
            rotatePlayer();
            boost();
            calculateflips();   
        }
        else
        {
            playcontrol=false;
        }
        
    }
    void rotatePlayer()
    {
        float ctorque = 0f;
        moveVector=MoveAction.ReadValue<Vector2>();
        if (moveVector.x < 0)
        {
            ctorque = torqueAmount;
            myrigidbody2D.AddTorque(ctorque);
        }
        if(moveVector.x > 0)    
        {
            ctorque = -torqueAmount;
            myrigidbody2D.AddTorque(ctorque);
        }
    }
    void boost()
    {
        if (moveVector.y > 0)
        {
            surface.speed=boostspeed;
        }
        else if (moveVector.y == 0)
        {
            surface.speed=basespeed;
        }
       
    }
    public void Disablecontrols()
    {
        playcontrol=false;
    }
    void calculateflips()
    {
        
        float current_rotations = transform.rotation.eulerAngles.z;
        total_rotation += Mathf.DeltaAngle(prev_rotations,current_rotations);
        prev_rotations=current_rotations;
        if(total_rotation>=340 || total_rotation <= -340)
        {
            ++flip;
            total_rotation=0;
            scoreManager.AddScore(100);

        }
         
    }

    public void powerbooster(PowerupSO power)
    {
        if (power.Getpowertype() == "speed")
        {
            basespeed+=power.Getvaluechange();
            boostspeed+=power.Getvaluechange();
        }
    }
    public void powerrotator(PoweruprotationSO power)
    {
        if (power.Getpowertype() == "torque")
        {
            torqueAmount += power.Gettorquechange();
        }
    }
    
    public void disablepowerup(PowerupSO power)
    {
        if (power.Getpowertype() == "speed")
        {
            basespeed-=power.Getvaluechange();
            boostspeed-=power.Getvaluechange();
        }
    }
    public void disablepowerrot(PoweruprotationSO power)
    {
        if(power.Getpowertype() == "torque")
        {
            torqueAmount-= power.Gettorquechange();
        }
    }
}
