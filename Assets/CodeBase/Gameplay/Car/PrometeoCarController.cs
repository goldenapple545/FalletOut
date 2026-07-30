/*
MESSAGE FROM CREATOR: This script was coded by Mena. You can use it in your games either these are commercial or
personal projects. You can even add or remove functions as you wish. However, you cannot sell copies of this
script by itself, since it is originally distributed as a free product.
I wish you the best for your project. Good luck!

P.S: If you need more cars, you can check my other vehicle assets on the Unity Asset Store, perhaps you could find
something useful for your game. Best regards, Mena.
*/

using System;
using CodeBase.Gameplay.Car.Input;
using UnityEngine;
using CodeBase.Gameplay.Car.Input;
using CodeBase.Gameplay.Car.Input.Prediction;
using FishNet.Component.Prediction;
using FishNet.Object.Prediction;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CodeBase.Gameplay.Car
{
  public class PrometeoCarController : MonoBehaviour
  {

    //CAR SETUP

    [SerializeField] private VehicleStats stats;

    //WHEELS

    /*
      The following variables are used to store the wheels' data of the car. We need both the mesh-only game objects and wheel
      collider components of the wheels. The wheel collider components and 3D meshes of the wheels cannot come from the same
      game object; they must be separate game objects.
      */
    public WheelCollider frontLeftCollider;
    [Space(10)]
    public WheelCollider frontRightCollider;
    [Space(10)]
    public WheelCollider rearLeftCollider;
    [Space(10)]
    public WheelCollider rearRightCollider;

    //SOUNDS

    [Space(20)]
    //[Header("Sounds")]
    [Space(10)]
    //The following variable lets you to set up sounds for your car such as the car engine or tire screech sounds.
    public bool useSounds = false;
    public AudioSource carEngineSound; // This variable stores the sound of the car engine.
    public AudioSource tireScreechSound; // This variable stores the sound of the tire screech (when the car is drifting).
    float initialCarEngineSoundPitch; // Used to store the initial pitch of the car engine sound.

    //CAR DATA

    [HideInInspector]
    public float carSpeed;
    [HideInInspector]
    public bool isDrifting;
    [HideInInspector]
    public bool isTractionLocked;
    
    public float LocalVelocityX => localVelocityX;
    public float LocalVelocityZ => localVelocityZ;
    
    public float SteeringAxis => steeringAxis;
    public float ThrottleAxis => throttleAxis;
    public float DriftingAxis => driftingAxis;
    public bool IsDrifting => isDrifting;
    public bool IsTractionLocked => isTractionLocked;

    //PRIVATE VARIABLES

    /*
      IMPORTANT: The following variables should not be modified manually since their values are automatically given via script.
      */
    Rigidbody carRigidbody; // Stores the car's rigidbody.
    float steeringAxis; // Used to know whether the steering wheel has reached the maximum value. It goes from -1 to 1.
    float throttleAxis; // Used to know whether the throttle has reached the maximum value. It goes from -1 to 1.
    float driftingAxis;
    float localVelocityZ;
    float localVelocityX;
    bool deceleratingCar;
    bool touchControlsSetup = false;
    /*
      The following variables are used to store information about sideways friction of the wheels (such as
      extremumSlip,extremumValue, asymptoteSlip, asymptoteValue and stiffness). We change this values to
      make the car to start drifting.
      */
    WheelFrictionCurve FLwheelFriction;
    float FLWextremumSlip;
    WheelFrictionCurve FRwheelFriction;
    float FRWextremumSlip;
    WheelFrictionCurve RLwheelFriction;
    float RLWextremumSlip;
    WheelFrictionCurve RRwheelFriction;
    float RRWextremumSlip;
    
    private bool _canReactOnInput;
    private float _nextBoostTime;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    public void InitializeManual()
    {
        Initialize();
    }

    public void SetReactOnInput(bool value)
    {
        if (!value)
        {
            _canReactOnInput = true;
            
            UpdateSteering(0, 0);
            UpdateDrive(0, 0);
            UpdateHandbrake(false, 0);
        }
        
        _canReactOnInput = value;
    }

    private void Initialize()
    {
        //In this part, we set the 'carRigidbody' value with the Rigidbody attached to this
        //gameObject. Also, we define the center of mass of the car with the Vector3 given
        //in the inspector.
        carRigidbody = gameObject.GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = stats.bodyMassCenter;

        //Initial setup to calculate the drift value of the car. This part could look a bit
        //complicated, but do not be afraid, the only thing we're doing here is to save the default
        //friction values of the car wheels so we can set an appropiate drifting value later.
        FLwheelFriction = new WheelFrictionCurve();
        FLwheelFriction.extremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLWextremumSlip = frontLeftCollider.sidewaysFriction.extremumSlip;
        FLwheelFriction.extremumValue = frontLeftCollider.sidewaysFriction.extremumValue;
        FLwheelFriction.asymptoteSlip = frontLeftCollider.sidewaysFriction.asymptoteSlip;
        FLwheelFriction.asymptoteValue = frontLeftCollider.sidewaysFriction.asymptoteValue;
        FLwheelFriction.stiffness = frontLeftCollider.sidewaysFriction.stiffness;
        FRwheelFriction = new WheelFrictionCurve();
        FRwheelFriction.extremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRWextremumSlip = frontRightCollider.sidewaysFriction.extremumSlip;
        FRwheelFriction.extremumValue = frontRightCollider.sidewaysFriction.extremumValue;
        FRwheelFriction.asymptoteSlip = frontRightCollider.sidewaysFriction.asymptoteSlip;
        FRwheelFriction.asymptoteValue = frontRightCollider.sidewaysFriction.asymptoteValue;
        FRwheelFriction.stiffness = frontRightCollider.sidewaysFriction.stiffness;
        RLwheelFriction = new WheelFrictionCurve();
        RLwheelFriction.extremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLWextremumSlip = rearLeftCollider.sidewaysFriction.extremumSlip;
        RLwheelFriction.extremumValue = rearLeftCollider.sidewaysFriction.extremumValue;
        RLwheelFriction.asymptoteSlip = rearLeftCollider.sidewaysFriction.asymptoteSlip;
        RLwheelFriction.asymptoteValue = rearLeftCollider.sidewaysFriction.asymptoteValue;
        RLwheelFriction.stiffness = rearLeftCollider.sidewaysFriction.stiffness;
        RRwheelFriction = new WheelFrictionCurve();
        RRwheelFriction.extremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRWextremumSlip = rearRightCollider.sidewaysFriction.extremumSlip;
        RRwheelFriction.extremumValue = rearRightCollider.sidewaysFriction.extremumValue;
        RRwheelFriction.asymptoteSlip = rearRightCollider.sidewaysFriction.asymptoteSlip;
        RRwheelFriction.asymptoteValue = rearRightCollider.sidewaysFriction.asymptoteValue;
        RRwheelFriction.stiffness = rearRightCollider.sidewaysFriction.stiffness;

        // We save the initial pitch of the car engine sound.
        if (carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
        }

        if (useSounds)
        {
            InvokeRepeating("CarSounds", 0f, 0.1f);
        }
        else if (!useSounds)
        {
            if (carEngineSound != null)
            {
                carEngineSound.Stop();
            }

            if (tireScreechSound != null)
            {
                tireScreechSound.Stop();
            }
        }
    }

    void Update()
    {
      CalculateCarData();
    }

    private void CalculateCarData()
    {
      // We determine the speed of the car.
      carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
      // Save the local velocity of the car in the x axis. Used to know if the car is drifting.
      localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
      // Save the local velocity of the car in the z axis. Used to know if the car is going forward or backwards.
      localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;
    }
    
    public void SimulateTick(
    VehicleInput input,
    PredictionRigidbody predictionRigidbody,
    float tickDelta,
    ReplicateState state)
    {
        UpdatePhysicsData();

        UpdateSteering(input.Steering, tickDelta);
        UpdateDrive(input.Throttle, tickDelta);
        UpdateHandbrake(input.Handbrake, tickDelta);
        UpdateBoost(input.Boost, tickDelta);
    }

    public void ApplyPredictionState(VehicleReconcileData data)
    {
        steeringAxis = data.SteeringAxis;
        throttleAxis = data.ThrottleAxis;
        driftingAxis = data.DriftingAxis;
        isDrifting = data.IsDrifting;
        isTractionLocked = data.IsTractionLocked;

        ApplySteeringAngle();
        ApplyDriftFriction();
    }

    private void UpdatePhysicsData()
    {
        carSpeed =
            (2f * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60f) /
            1000f;

        Vector3 localVelocity =
            transform.InverseTransformDirection(carRigidbody.linearVelocity);

        localVelocityX = localVelocity.x;
        localVelocityZ = localVelocity.z;
    }

    private void UpdateSteering(float steeringInput, float tickDelta)
    {
        if (!_canReactOnInput) return;
        
        steeringAxis = Mathf.MoveTowards(
            steeringAxis,
            Mathf.Clamp(steeringInput, -1f, 1f),
            10f * stats.steeringSpeed * tickDelta);

        ApplySteeringAngle();
    }

    private void ApplySteeringAngle()
    {
        float steeringAngle = steeringAxis * stats.maxSteeringAngle;

        frontLeftCollider.steerAngle = Mathf.Lerp(
            frontLeftCollider.steerAngle,
            steeringAngle,
            stats.steeringSpeed);

        frontRightCollider.steerAngle = Mathf.Lerp(
            frontRightCollider.steerAngle,
            steeringAngle,
            stats.steeringSpeed);
    }

    private void UpdateDrive(float throttleInput, float tickDelta)
    {
        if (!_canReactOnInput) return;
        
        float desiredThrottle = Mathf.Clamp(throttleInput, -1f, 1f);

        float throttleChangeSpeed =
            desiredThrottle == 0f
                ? 15f
                : 10f;

        throttleAxis = Mathf.MoveTowards(
            throttleAxis,
            desiredThrottle,
            throttleChangeSpeed * tickDelta);

        UpdateDriftFlag();

        if (Mathf.Approximately(throttleAxis, 0f))
        {
            SetMotorTorque(0f);
            ApplyCoasting(tickDelta);
            return;
        }

        if (throttleAxis > 0f && localVelocityZ < -1f)
        {
            SetMotorTorque(0f);
            SetBrakeTorque(stats.brakeForce);
            ApplyCoasting(tickDelta);
            return;
        }

        if (throttleAxis < 0f && localVelocityZ > 1f)
        {
            SetMotorTorque(0f);
            SetBrakeTorque(stats.brakeForce);
            ApplyCoasting(tickDelta);
            return;
        }

        bool isForward = throttleAxis > 0f;
        float speedLimit = isForward ? stats.maxSpeed : stats.maxReverseSpeed;

        if (Mathf.Abs(carSpeed) >= speedLimit)
        {
            SetMotorTorque(0f);
            return;
        }

        SetBrakeTorque(0f);

        float motorTorque =
            stats.accelerationMultiplier * throttleAxis;

        SetMotorTorque(motorTorque);
    }

    private void ApplyCoasting(float tickDelta)
    {
        float damping =
            1f / (1f + 0.025f * stats.decelerationMultiplier);

        // Применяем тот же damping с учётом длительности tick.
        float referenceDelta = 0.02f;
        float multiplier = Mathf.Pow(damping, tickDelta / referenceDelta);

        carRigidbody.linearVelocity *= multiplier;

        if (carRigidbody.linearVelocity.magnitude < 0.25f)
            carRigidbody.linearVelocity = Vector3.zero;
    }

    private void UpdateHandbrake(bool isHandbrakePressed, float tickDelta)
    {
        if (!_canReactOnInput) return;
        
        isTractionLocked = isHandbrakePressed;

        float targetDrift = isHandbrakePressed ? 1f : 0f;
        float driftSpeed = isHandbrakePressed ? 1f : 1f / 1.5f;

        driftingAxis = Mathf.MoveTowards(
            driftingAxis,
            targetDrift,
            driftSpeed * tickDelta);

        UpdateDriftFlag();
        ApplyDriftFriction();
    }
    
    private void UpdateBoost(bool InputBoost, float TickDelta)
    {
        if (!_canReactOnInput) return;
        
        if (InputBoost && Time.time >= _nextBoostTime)
        {
            carRigidbody.AddForce(Vector3.up * stats.boostForce, ForceMode.Impulse);
            _nextBoostTime = Time.time + stats.boostCooldown;
        }
    }

    private void ApplyDriftFriction()
    {
        float driftMultiplier = Mathf.Lerp(
            1f,
            stats.handbrakeDriftMultiplier,
            driftingAxis);

        FLwheelFriction.extremumSlip =
            FLWextremumSlip * driftMultiplier;

        FRwheelFriction.extremumSlip =
            FRWextremumSlip * driftMultiplier;

        RLwheelFriction.extremumSlip =
            RLWextremumSlip * driftMultiplier;

        RRwheelFriction.extremumSlip =
            RRWextremumSlip * driftMultiplier;

        frontLeftCollider.sidewaysFriction = FLwheelFriction;
        frontRightCollider.sidewaysFriction = FRwheelFriction;
        rearLeftCollider.sidewaysFriction = RLwheelFriction;
        rearRightCollider.sidewaysFriction = RRwheelFriction;
    }

    private void UpdateDriftFlag()
    {
        isDrifting = Mathf.Abs(localVelocityX) > 2.5f;
    }

    private void SetMotorTorque(float value)
    {
        frontLeftCollider.motorTorque = value;
        frontRightCollider.motorTorque = value;
        rearLeftCollider.motorTorque = value;
        rearRightCollider.motorTorque = value;
    }

    private void SetBrakeTorque(float value)
    {
        frontLeftCollider.brakeTorque = value;
        frontRightCollider.brakeTorque = value;
        rearLeftCollider.brakeTorque = value;
        rearRightCollider.brakeTorque = value;
    }

    
    public void CarSounds(){

      if(useSounds){
        try{
          if(carEngineSound != null){
            float engineSoundPitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / 25f);
            carEngineSound.pitch = engineSoundPitch;
          }
          if((isDrifting) || (isTractionLocked && Mathf.Abs(carSpeed) > 12f)){
            if(!tireScreechSound.isPlaying){
              tireScreechSound.Play();
            }
          }else if((!isDrifting) && (!isTractionLocked || Mathf.Abs(carSpeed) < 12f)){
            tireScreechSound.Stop();
          }
        }catch(Exception ex){
          Debug.LogWarning(ex);
        }
      }else if(!useSounds){
        if(carEngineSound != null && carEngineSound.isPlaying){
          carEngineSound.Stop();
        }
        if(tireScreechSound != null && tireScreechSound.isPlaying){
          tireScreechSound.Stop();
        }
      }

    }
  }
}
