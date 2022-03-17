using UnityEngine;

[RequireComponent(typeof(BarsEffect))]
public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private float distanceAway;
    [SerializeField] private float distanceUp;
    [SerializeField] private float smooth;
    [SerializeField] private Transform follow;

    [SerializeField] private float widescreen = 0.2f;
    [SerializeField] private float targetingTime = 0.5f;
    
    [Header("First Person")]
    [SerializeField] private float firstPersonLookSpeed = 1.5f;
    [SerializeField] private Vector2 firstPersonXAxisClamp = new Vector2(-70f, 70f);
    [SerializeField] private float fpsRotationDegresPerSecond = 120f;


    private Vector3 lookDir;
    private Vector3 targetPosition;
    private BarsEffect barEffect;
    private CameraPosition firstPersonCamPos;

    private CameraState camState = CameraState.Behind;
    public CameraState CamState { get => camState; }

    private float xAxisRot = 0f;
    private float lookWeight;
    

    // Smoothing and damping
    private Vector3 velocityCamSmooth = Vector3.zero;
    [Header("Smooth Values")]
    [SerializeField] private float camSmoothDampTime = 0.1f;

    // Start is called before the first frame update
    private void Start()
    {
        lookDir = follow.forward;
        barEffect = GetComponent<BarsEffect>();

        firstPersonCamPos = new CameraPosition();
        firstPersonCamPos.Init(
            "First Person Camera",
            new Vector3(0f, 1.6f, 0.2f),
            new GameObject().transform,
            follow.transform);
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        float rightX = Input.GetAxis("Horizontal");
        float rightY = Input.GetAxis("Vertical");
        float leftX = Input.GetAxis("Mouse X");
        float leftY = -Input.GetAxis("Mouse Y");

        Vector3 characterOffset = follow.position + new Vector3(0f, distanceUp, 0f);
        Vector3 lookAt = characterOffset;

        // Determine camera state
        if (Input.GetButton("Target"))
        {
            barEffect.Coverage = Mathf.SmoothStep(barEffect.Coverage, widescreen, targetingTime * Time.deltaTime);
            camState = CameraState.Target;
        }
        else
        {
            barEffect.Coverage = Mathf.SmoothStep(barEffect.Coverage, 0f, targetingTime * Time.deltaTime);
            camState = camState == CameraState.Target ? CameraState.Behind : camState;

            // First Person
            if (Input.GetKeyDown(KeyCode.Mouse2) && !follow.GetComponent<PlayerController>().IsInLocomotion())
            {
                // Reset look before entering the first person  mode
                xAxisRot = 0;
                lookWeight = 0f;
                camState = CameraState.FirstPerson;
            }

            // Behind the back
            if (camState == CameraState.FirstPerson && Input.GetKeyDown(KeyCode.Mouse1))
            {
                camState = CameraState.Behind;
            }
            
        }



        // Doesn't work for now
        // VVVVVVVVVV THIS NEEDS  TO BE MOVED OVER TO THE PLAYER CONTROLLER!!! VVVVVVVVVV
        // Set the Look At Weight amount to use look at IK vs using the head's animation
        follow.GetComponent<Animator>().SetLookAtWeight(lookWeight);



        // Execute camera state
        switch (camState)
        {
            case CameraState.Behind:
                ResetCamera();

                // Calculate direction from camera to player, kill Y, and
                // normalize to give a valid direction with unit magnitude.
                lookDir = characterOffset - transform.position;
                lookDir.y = 0;
                lookDir.Normalize();
                Debug.DrawRay(transform.position, lookDir, Color.green);

                // Setting the target position to be the correct offset from the follow
                targetPosition = characterOffset + follow.up * distanceUp - lookDir * distanceAway;

                Debug.DrawRay(follow.position, follow.up * distanceUp, Color.red);
                Debug.DrawRay(follow.position, -1f * follow.forward * distanceAway, Color.blue);
                Debug.DrawLine(follow.position, targetPosition, Color.magenta);
                break;
            case CameraState.FirstPerson:
                // Look Up and Down
                //  Calculate the amount of rotation and apply to the firstPersonCamPos GameObject
                xAxisRot += (leftY * firstPersonLookSpeed);
                xAxisRot = Mathf.Clamp(xAxisRot, firstPersonXAxisClamp.x, firstPersonXAxisClamp.y);
                firstPersonCamPos.XForm.localRotation = Quaternion.Euler(xAxisRot, 0f, 0f);

                // Superimpose firstPersonCamPos GameObject's rotation on camera
                Quaternion rotationShift = Quaternion.FromToRotation(transform.forward, firstPersonCamPos.XForm.forward);
                transform.rotation = rotationShift * transform.rotation;



                // Doesn't work for now
                // VVVVVVVVVV THIS NEEDS  TO BE MOVED OVER TO THE PLAYER CONTROLLER!!! VVVVVVVVVV
                // Move character model's head
                follow.GetComponent<Animator>().SetLookAtPosition(firstPersonCamPos.XForm.position + firstPersonCamPos.XForm.forward);
                // Update the weight of the lookAt
                lookWeight = Mathf.Lerp(lookWeight, 1f, Time.deltaTime * firstPersonLookSpeed);
                // AAAAAAAAAA THIS NEEDS  TO BE MOVED OVER TO THE PLAYER CONTROLLER!!! AAAAAAAAAA




                // Looking  left and right
                Vector3 rotationAmout = Vector3.Lerp(Vector3.zero, new Vector3(0f, fpsRotationDegresPerSecond * (leftX < 0f ? -1f : 1f), 0f), Mathf.Abs(leftX));
                Quaternion deltaRotation = Quaternion.Euler(rotationAmout * Time.deltaTime);
                follow.rotation = follow.rotation * deltaRotation;

                // Move camera to firstPersonCamPos
                targetPosition = firstPersonCamPos.XForm.position;

                // Smooth the look direction towards firstPersonCamPos when entering first person mode
                lookAt = Vector3.Lerp(targetPosition + follow.forward, transform.position + transform.forward, camSmoothDampTime * Time.deltaTime);

                // Choose lookAttarget based on distance
                lookAt = Vector3.Lerp(transform.position + transform.forward, lookAt, Vector3.Distance(transform.position, firstPersonCamPos.XForm.position));

                break;
            case CameraState.Target:
                ResetCamera();

                // Setting the target position
                targetPosition = characterOffset + follow.up * distanceUp - follow.forward * distanceAway;
                break;
            case CameraState.Free:
                break;
            default:
                break;
        }

        // Moves the camera closer to the player when it hits a wall
        CompensateForWalls(characterOffset);

        // Making a smooth transition between it's current position and the position it wants to be in
        SmoothPosition(transform.position, targetPosition);

        // Make sure the camera is looking the right way
        transform.LookAt(lookAt);
    }

    private void SmoothPosition(Vector3 fromPos, Vector3 toPos)
    {
        // Making a smooth transition between camera's current position and the position it wants to be in
        transform.position = Vector3.SmoothDamp(fromPos, toPos, ref velocityCamSmooth, camSmoothDampTime);
    }

    private void CompensateForWalls(Vector3 fromObject)
    {
        Debug.DrawLine(fromObject, targetPosition, Color.cyan);

        // Compensate for walls between camera
        RaycastHit wallHit;
        if (Physics.Linecast(fromObject, targetPosition, out wallHit))
        {
            Debug.DrawRay(wallHit.point, Vector3.left, Color.red);
            targetPosition = new Vector3(wallHit.point.x, targetPosition.y, wallHit.point.z);
        }
    }

    private void ResetCamera()
    {
        lookWeight = Mathf.Lerp(lookWeight, 0f, Time.deltaTime * firstPersonLookSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime);
    }
}

public struct CameraPosition
{
    // Position to align camera to
    private Vector3 position;
    // Transform used for rotation
    private Transform xForm;

    public Vector3 Position {
        get => position;
        set {
            position = value;
        }
    }
    public Transform XForm
    {
        get => xForm;
        set
        {
            xForm = value;
        }
    }

    public void Init(string camName, Vector3 pos, Transform transform, Transform parent)
    {
        position = pos;
        xForm = transform;
        xForm.name = camName;
        xForm.parent = parent;
        xForm.localPosition = position;
    }
}

public enum CameraState
{
    Behind,
    FirstPerson,
    Target,
    Free
}
