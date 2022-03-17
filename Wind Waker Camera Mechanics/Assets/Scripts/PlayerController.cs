using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float directionDampTime = 0.25f;
    [SerializeField] private float directionSpeed = 3f;
    [SerializeField] private float rotationDegreePerSecond = 120f;
    [SerializeField] private ThirdPersonCamera cam;

    private Animator animator;

    private float direction = 0f;
    private float speed = 0f;
    private float horizontal = 0f;
    private float vertical = 0f;

    private AnimatorStateInfo stateInfo;

    // Hashes
    private int m_LocomotionId = 0;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        // Hash all animation names
        m_LocomotionId = Animator.StringToHash("Base Layer.Locomotion");
    }

    // Update is called once per frame
    void Update()
    {
        if (cam.CamState == CameraState.FirstPerson)
            return;

        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        // Translate controls stick coordinates  into world/cam/character space
        StickToWorldspace(transform, cam.transform);

        animator.SetFloat("Speed", speed);
        animator.SetFloat("Direction", direction, directionDampTime, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (IsInLocomotion() && ((direction >= 0 && horizontal >= 0) || (direction < 0 && horizontal < 0)))
        {
            Vector3 rotationAmount = Vector3.Lerp(Vector3.zero, new Vector3(0f, rotationDegreePerSecond * (horizontal < 0f ? -1f : 1f), 0f), Mathf.Abs(horizontal));
            Quaternion deltaRotation = Quaternion.Euler(rotationAmount * Time.deltaTime);
            transform.rotation = (transform.rotation * deltaRotation);
        }
    }

    public void StickToWorldspace(Transform root, Transform camera)
    {
        Vector3 rootDirection = root.forward;

        Vector3 stickDirection = new Vector3(horizontal, 0, vertical);

        speed = stickDirection.sqrMagnitude;

        // Get camera rotation
        Vector3 CameraDirection = camera.forward;
        CameraDirection.y = 0.0f; // kill Y
        Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(CameraDirection));

        // Convert joystick input in Worldspace coordinates
        Vector3 moveDirection = referentialShift * stickDirection;
        Vector3 axisSign = Vector3.Cross(moveDirection, rootDirection);

        Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2f, root.position.z), moveDirection, Color.green);
        Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2f, root.position.z), rootDirection, Color.magenta);
        Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2f, root.position.z), stickDirection, Color.blue);
        Debug.DrawRay(new Vector3(root.position.x, root.position.y + 2.5f, root.position.z), axisSign, Color.red);

        float angleRootToMove = Vector3.Angle(rootDirection, moveDirection) * (axisSign.y >= 0 ? -1f : 1f);

        angleRootToMove /= 180f;

        direction = angleRootToMove * directionSpeed;
    }

    public bool IsInLocomotion() =>
        stateInfo.fullPathHash == m_LocomotionId;
}
