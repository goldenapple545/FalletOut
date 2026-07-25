using UnityEditor;
using UnityEngine;

namespace CodeBase.Gameplay.Car.Editor
{
    [CustomEditor(typeof(PrometeoCarController))]
    [System.Serializable]
    public class PrometeoEditor : UnityEditor.Editor{

        enum displayFieldType {DisplayAsAutomaticFields, DisplayAsCustomizableGUIFields}
        displayFieldType DisplayFieldType;

        private PrometeoCarController prometeo;
        private SerializedObject SO;
        //
        //
        //CAR SETUP
        //
        //
        private SerializedProperty maxSpeed;
        private SerializedProperty maxReverseSpeed;
        private SerializedProperty accelerationMultiplier;
        private SerializedProperty maxSteeringAngle;
        private SerializedProperty steeringSpeed;
        private SerializedProperty brakeForce;
        private SerializedProperty decelerationMultiplier;
        private SerializedProperty handbrakeDriftMultiplier;
        private SerializedProperty bodyMassCenter;
        //
        //
        //WHEELS VARIABLES
        //
        //
        private SerializedProperty frontLeftCollider;
        private SerializedProperty frontRightCollider;
        private SerializedProperty rearLeftCollider;
        private SerializedProperty rearRightCollider;
        //
        //
        //
        //
        //
        //
        //SPEED TEXT (UI) VARIABLES
        //
        //
        private SerializedProperty useSounds;
        private SerializedProperty carEngineSound;
        private SerializedProperty tireScreechSound;
        //
        //
        //TOUCH CONTROLS VARIABLES
        //
        //
        private SerializedProperty useTouchControls;
        private SerializedProperty throttleButton;
        private SerializedProperty reverseButton;
        private SerializedProperty turnRightButton;
        private SerializedProperty turnLeftButton;
        private SerializedProperty handbrakeButton;

        private void OnEnable(){
            prometeo = (PrometeoCarController)target;
            SO = new SerializedObject(target);

            maxSpeed = SO.FindProperty("maxSpeed");
            maxReverseSpeed = SO.FindProperty("maxReverseSpeed");
            accelerationMultiplier = SO.FindProperty("accelerationMultiplier");
            maxSteeringAngle = SO.FindProperty("maxSteeringAngle");
            steeringSpeed = SO.FindProperty("steeringSpeed");
            brakeForce = SO.FindProperty("brakeForce");
            decelerationMultiplier = SO.FindProperty("decelerationMultiplier");
            handbrakeDriftMultiplier = SO.FindProperty("handbrakeDriftMultiplier");
            bodyMassCenter = SO.FindProperty("bodyMassCenter");

            frontLeftCollider = SO.FindProperty("frontLeftCollider");
            frontRightCollider = SO.FindProperty("frontRightCollider");
            rearLeftCollider = SO.FindProperty("rearLeftCollider");
            rearRightCollider = SO.FindProperty("rearRightCollider");

            useSounds = SO.FindProperty("useSounds");
            carEngineSound = SO.FindProperty("carEngineSound");
            tireScreechSound = SO.FindProperty("tireScreechSound");

            useTouchControls = SO.FindProperty("useTouchControls");
            throttleButton = SO.FindProperty("throttleButton");
            reverseButton = SO.FindProperty("reverseButton");
            turnRightButton = SO.FindProperty("turnRightButton");
            turnLeftButton = SO.FindProperty("turnLeftButton");
            handbrakeButton = SO.FindProperty("handbrakeButton");

        }

        public override void OnInspectorGUI(){

            SO.Update();

            GUILayout.Space(25);
            GUILayout.Label("CAR SETUP", EditorStyles.boldLabel);
            GUILayout.Space(10);
            //
            //
            //CAR SETUP
            //
            //
            //
            maxSpeed.intValue = EditorGUILayout.IntSlider("Max Speed:", maxSpeed.intValue, 20, 190);
            maxReverseSpeed.intValue = EditorGUILayout.IntSlider("Max Reverse Speed:", maxReverseSpeed.intValue, 10, 120);
            accelerationMultiplier.intValue = EditorGUILayout.IntSlider("Acceleration Multiplier:", accelerationMultiplier.intValue, 1, 10);
            maxSteeringAngle.intValue = EditorGUILayout.IntSlider("Max Steering Angle:", maxSteeringAngle.intValue, 10, 45);
            steeringSpeed.floatValue = EditorGUILayout.Slider("Steering Speed:", steeringSpeed.floatValue, 0.1f, 1f);
            brakeForce.intValue = EditorGUILayout.IntSlider("Brake Force:", brakeForce.intValue, 100, 600);
            decelerationMultiplier.intValue = EditorGUILayout.IntSlider("Deceleration Multiplier:", decelerationMultiplier.intValue, 1, 10);
            handbrakeDriftMultiplier.intValue = EditorGUILayout.IntSlider("Drift Multiplier:", handbrakeDriftMultiplier.intValue, 1, 10);
            EditorGUILayout.PropertyField(bodyMassCenter, new GUIContent("Mass Center of Car: "));

            //
            //
            //WHEELS
            //
            //

            GUILayout.Space(25);
            GUILayout.Label("WHEELS", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(frontLeftCollider, new GUIContent("Front Left Collider: "));

            EditorGUILayout.PropertyField(frontRightCollider, new GUIContent("Front Right Collider: "));

            EditorGUILayout.PropertyField(rearLeftCollider, new GUIContent("Rear Left Collider: "));

            EditorGUILayout.PropertyField(rearRightCollider, new GUIContent("Rear Right Collider: "));

            //
            //
            //EFFECTS
            //
            //

            GUILayout.Space(25);

            //
            //
            //UI
            //
            //

            GUILayout.Space(25);

            //
            //
            //SOUNDS
            //
            //

            GUILayout.Space(25);
            GUILayout.Label("SOUNDS", EditorStyles.boldLabel);
            GUILayout.Space(10);

            useSounds.boolValue = EditorGUILayout.BeginToggleGroup("Use sounds (car sounds)?", useSounds.boolValue);
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(carEngineSound, new GUIContent("Car Engine Sound: "));
            EditorGUILayout.PropertyField(tireScreechSound, new GUIContent("Tire Screech Sound: "));

            EditorGUILayout.EndToggleGroup();

            //
            //
            //TOUCH CONTROLS
            //
            //

            GUILayout.Space(25);
            GUILayout.Label("TOUCH CONTROLS", EditorStyles.boldLabel);
            GUILayout.Space(10);

            useTouchControls.boolValue = EditorGUILayout.BeginToggleGroup("Use touch controls (mobile devices)?", useTouchControls.boolValue);
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(throttleButton, new GUIContent("Throttle Button: "));
            EditorGUILayout.PropertyField(reverseButton, new GUIContent("Brakes/Reverse Button: "));
            EditorGUILayout.PropertyField(turnLeftButton, new GUIContent("Turn Left Button: "));
            EditorGUILayout.PropertyField(turnRightButton, new GUIContent("Turn Right Button: "));
            EditorGUILayout.PropertyField(handbrakeButton, new GUIContent("Handbrake Button: "));

            EditorGUILayout.EndToggleGroup();

            //END

            GUILayout.Space(10);
            SO.ApplyModifiedProperties();

        }

    }
}
