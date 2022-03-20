using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float directionDampTime = 0.25f;
    [SerializeField] private float speedDampTime = 0.05f;
    [SerializeField] private float directionSpeed = 3f;
    [SerializeField] private float rotationDegreePerSecond = 120f;
    [SerializeField] private ThirdPersonCamera cam;

    private Animator animator;

    private float direction = 0f;
    private float horizontal = 0f;
    private float vertical = 0f;
    private float charAngle = 0f;

    private float speed = 0f;
    public float Speed { get => speed; }

    public float LocomotionThreshold { get => 0.2f; }

    private AnimatorStateInfo stateInfo;
    private AnimatorTransitionInfo transInfo;

    // Hashes
    private int m_LocomotionId;
    private int m_LocomotionPivotLId;
    private int m_LocomotionPivotRId;
    private int m_LocomotionPivotLTransId;
    private int m_LocomotionPivotRTransId;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        // Hash all animation names
        m_LocomotionId = Animator.StringToHash("Base Layer.Locomotion");
        m_LocomotionPivotLId = Animator.StringToHash("Base Layer.LocomotionPivotL");
        m_LocomotionPivotRId = Animator.StringToHash("Base Layer.LocomotionPivotR");
        m_LocomotionPivotLTransId = Animator.StringToHash("Base Layer.Locomotion -> Base Layer.LocomotionPivotL");
        m_LocomotionPivotRTransId = Animator.StringToHash("Base Layer.Locomotion -> Base Layer.LocomotionPivotR");
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.CamState == CameraState.FirstPerson)
            return;

        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        transInfo = animator.GetAnimatorTransitionInfo(0);

        if (IsInPivot())
            return;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        charAngle = 0f;
        direction = 0f;

        // Translate controls stick coordinates  into world/cam/character space
        StickToWorldspace();

        // Resets direction when pivoting to smooth animations
        if (IsInPivot() || charAngle < -90 || charAngle > 90)
        {
            direction = 0;
        }

        animator.SetFloat("Speed", speed, speedDampTime, Time.deltaTime);
        animator.SetFloat("Direction", direction, directionDampTime, Time.deltaTime);

        if (speed > LocomotionThreshold && !IsInPivot())
        {
            animator.SetFloat("Angle", charAngle);
        }

        if (speed < LocomotionThreshold && Mathf.Abs(horizontal) < 0.05f)
        {
            animator.SetFloat("Direction", 0f);
        }
    }

    private void FixedUpdate()
    {
        if (IsInLocomotion() && !IsInPivot() && ((direction >= 0 && horizontal >= 0) || (direction < 0 && horizontal < 0)))
        {
            Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, rotationDegreePerSecond * (horizontal < 0f ? -1f : 1f), 0f), Mathf.Abs(horizontal));
            Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
            transform.rotation = (transform.rotation * deltaRotation);
        }
    }

    public void StickToWorldspace()
    {
        Vector3 stickDirection = Vector3.forward * vertical + Vector3.right * horizontal;// new Vector3(horizontal, 0, vertical);

        stickDirection.y = 0;

        speed = stickDirection.magnitude;

        // Get camera rotation
        Vector3 CameraForward = cam.transform.forward;
        CameraForward.y = 0.0f; // kill Y
        Vector3 CameraRight = cam.transform.right;
        CameraRight.y = 0.0f; // kill Y

        // Boo quaternions suck!
        //Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(CameraDirection));

        // Convert joystick input in Worldspace coordinates
        // So much simpler and it works!
        Vector3 moveDirection = CameraForward * vertical + CameraRight * horizontal; // referentialShift * stickDirection;

        Vector3 axisSign = Vector3.Cross(moveDirection, transform.forward);

        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z), moveDirection, Color.green);
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z), transform.forward, Color.magenta);
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z), stickDirection, Color.blue);
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 2.5f, transform.position.z), axisSign, Color.red);

        float angleRootToMove = Vector3.Angle(transform.forward , moveDirection) * (axisSign.y >= 0 ? -1f : 1f);

        if (!IsInPivot())
        {
            charAngle = angleRootToMove;
        }

        angleRootToMove /= 180f;

        direction = angleRootToMove * directionSpeed;
    }

    public bool IsInLocomotion() =>
        stateInfo.fullPathHash == m_LocomotionId;

    /// <summary>
    /// THE PROBLEM! Even tho the angle is higher that 80 or lower than -80 
    /// it still returns fall for allot of frames allowing the animation to break
    /// </summary>
    /// <returns></returns>
    public bool IsInPivot() =>
        stateInfo.fullPathHash == m_LocomotionPivotLId
        || stateInfo.fullPathHash == m_LocomotionPivotRId
        || transInfo.fullPathHash == m_LocomotionPivotLTransId
        || transInfo.fullPathHash == m_LocomotionPivotRTransId;
}
