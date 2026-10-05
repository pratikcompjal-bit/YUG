using UnityEngine;

/// <summary>Self-contained third-person movement, camera, and an original procedural pilgrim.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class PilgrimController : MonoBehaviour
{
    public bool inputEnabled = true;
    public Camera viewCamera;
    public Vector3 checkpoint;
    public Vector3 MoveInputOverride;
    public bool useMoveOverride;
    public float CameraYaw = 0f;
    public float CameraPitch = 20f;
    public float walkSpeed = 3.2f;
    public float runSpeed = 6f;
    public float jumpHeight = 1.2f;
    public float gravity = 22f;

    public bool IsGrounded { get { return motor != null && motor.isGrounded; } }
    public float CurrentSpeed { get { return planarVelocity.magnitude; } }
    public Transform AvatarRoot { get { return avatar; } }

    CharacterController motor;
    Transform avatar, body, leftHip, rightHip, leftKnee, rightKnee;
    Transform leftShoulder, rightShoulder, leftElbow, rightElbow, skirt, sashTail;
    Vector3 planarVelocity;
    float verticalVelocity, stride, movingBlend;
    bool cinematic, snapCamera = true, jumpRequested;
    Vector3 cinematicPosition, cinematicLook;
    const float CameraDistance = 8.6f;
    const int IgnorePlayerMask = ~(1 << 2);

    void Awake()
    {
        gameObject.layer = 2;
        motor = GetComponent<CharacterController>();
        motor.height = 1.8f;
        motor.radius = .32f;
        motor.center = new Vector3(0f, .9f, 0f);
        motor.stepOffset = .35f;
        motor.slopeLimit = 46f;
        motor.skinWidth = .035f;
        motor.minMoveDistance = 0f;
        checkpoint = transform.position;
        if (viewCamera == null) viewCamera = Camera.main;
        BuildAvatar();
    }

    void Update()
    {
        float dt = Mathf.Min(Time.deltaTime, .05f);
        if (dt <= 0f || motor == null || !motor.enabled) return;
        Vector3 desired = Vector3.zero;
        bool run = false;
        if (inputEnabled)
        {
            float orbit = (Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                        - (Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            CameraYaw += orbit * 88f * dt;
            CameraPitch += ((Input.GetKey(KeyCode.DownArrow) ? 1f : 0f)
                         - (Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)) * 45f * dt;
            if (Input.GetMouseButton(1))
            {
                CameraYaw += Input.GetAxisRaw("Mouse X") * 2.6f;
                CameraPitch -= Input.GetAxisRaw("Mouse Y") * 2f;
            }
            CameraPitch = Mathf.Clamp(CameraPitch, 8f, 55f);
            CameraYaw = Mathf.Repeat(CameraYaw, 360f);
            if (useMoveOverride)
            {
                desired = Vector3.ClampMagnitude(
                    new Vector3(MoveInputOverride.x, 0f, MoveInputOverride.z), 1f);
            }
            else
            {
                float x = (Input.GetKey(KeyCode.D) ? 1f : 0f)
                        - (Input.GetKey(KeyCode.A) ? 1f : 0f);
                float z = (Input.GetKey(KeyCode.W) ? 1f : 0f)
                        - (Input.GetKey(KeyCode.S) ? 1f : 0f);
                desired = Quaternion.Euler(0f, CameraYaw, 0f)
                        * Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);
            }
            run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
        float speed = run ? runSpeed : walkSpeed;
        planarVelocity = inputEnabled
            ? Vector3.MoveTowards(planarVelocity, desired * speed, 24f * dt)
            : Vector3.zero;
        if (motor.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        if (inputEnabled && motor.isGrounded && (jumpRequested || Input.GetKeyDown(KeyCode.Space)))
            verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
        jumpRequested = false;
        verticalVelocity = Mathf.Max(verticalVelocity - gravity * dt, -35f);
        CollisionFlags flags = motor.Move((planarVelocity + Vector3.up * verticalVelocity) * dt);
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
        if ((flags & CollisionFlags.Below) != 0 && verticalVelocity < 0f) verticalVelocity = -2f;

        if (planarVelocity.sqrMagnitude > .035f)
        {
            Quaternion facing = Quaternion.LookRotation(planarVelocity.normalized, Vector3.up);
            avatar.rotation = Quaternion.Slerp(avatar.rotation, facing, 1f - Mathf.Exp(-13f * dt));
        }
        AnimateAvatar(dt, run);
        if (transform.position.y < -18f) ResetToCheckpoint();
    }

    void LateUpdate()
    {
        if (viewCamera == null) { viewCamera = Camera.main; if (viewCamera == null) return; }
        float dt = Time.deltaTime;
        Transform lens = viewCamera.transform;
        if (cinematic)
        {
            float blend = 1f - Mathf.Exp(-3.5f * dt);
            lens.position = Vector3.Lerp(lens.position, cinematicPosition, blend);
            Vector3 direction = cinematicLook - lens.position;
            if (direction.sqrMagnitude > .001f)
                lens.rotation = Quaternion.Slerp(lens.rotation,
                    Quaternion.LookRotation(direction, Vector3.up), blend);
            snapCamera = false;
            return;
        }
        Vector3 focus = transform.position + Vector3.up * 1.25f;
        Vector3 boom = Quaternion.Euler(CameraPitch, CameraYaw, 0f) * Vector3.back * CameraDistance;
        Vector3 destination = ResolveCameraCollision(focus, focus + boom);
        Vector3 position = snapCamera ? destination
            : Vector3.Lerp(lens.position, destination, 1f - Mathf.Exp(-10f * dt));
        lens.position = ResolveCameraCollision(focus, position);
        Vector3 look = focus - lens.position;
        if (look.sqrMagnitude > .001f) lens.rotation = Quaternion.LookRotation(look, Vector3.up);
        snapCamera = false;
    }

    Vector3 ResolveCameraCollision(Vector3 from, Vector3 desired)
    {
        Vector3 delta = desired - from;
        float distance = delta.magnitude;
        if (distance < .001f) return desired;
        RaycastHit hit;
        if (Physics.SphereCast(from, .20f, delta / distance, out hit, distance,
            IgnorePlayerMask, QueryTriggerInteraction.Ignore))
            return from + delta / distance * Mathf.Max(.20f, hit.distance - .08f);
        return desired;
    }

    public void Teleport(Vector3 position)
    {
        bool enabledBefore = motor != null && motor.enabled;
        if (motor != null) motor.enabled = false;
        transform.position = position;
        if (motor != null) motor.enabled = enabledBefore;
        planarVelocity = Vector3.zero;
        jumpRequested = false;
        verticalVelocity = -2f;
        snapCamera = true;
    }

    public void RequestJump() { if (inputEnabled) jumpRequested = true; }

    public void ResetToCheckpoint() { Teleport(checkpoint); }

    public void SetCinematic(Vector3 pos, Vector3 look)
    {
        cinematicPosition = pos;
        cinematicLook = look;
        cinematic = true;
    }

    public void ClearCinematic()
    {
        cinematic = false;
        // Continue smoothly from the cinematic camera into the player's orbit.
        snapCamera = false;
    }

    void AnimateAvatar(float dt, bool run)
    {
        float speed = planarVelocity.magnitude;
        movingBlend = Mathf.MoveTowards(movingBlend, Mathf.Clamp01(speed / walkSpeed), dt * 7f);
        stride += speed * dt * 3.05f;
        float wave = Mathf.Sin(stride);
        float swing = wave * (run ? 34f : 26f) * movingBlend;
        bool airborne = !motor.isGrounded;
        leftHip.localRotation = Quaternion.Euler(airborne ? -16f : swing, 0f, 0f);
        rightHip.localRotation = Quaternion.Euler(airborne ? 18f : -swing, 0f, 0f);
        leftKnee.localRotation = Quaternion.Euler(airborne ? 30f :
            Mathf.Max(0f, -wave) * 32f * movingBlend, 0f, 0f);
        rightKnee.localRotation = Quaternion.Euler(airborne ? 42f :
            Mathf.Max(0f, wave) * 32f * movingBlend, 0f, 0f);
        leftShoulder.localRotation = Quaternion.Euler(-swing * .8f, 0f, 7f);
        rightShoulder.localRotation = Quaternion.Euler(swing * .8f, 0f, -7f);
        leftElbow.localRotation = Quaternion.Euler(-12f - movingBlend * 12f, 0f, 0f);
        rightElbow.localRotation = leftElbow.localRotation;
        float breath = Mathf.Sin(Time.time * 1.8f) * .004f;
        body.localPosition = new Vector3(0f,
            (airborne ? .015f : Mathf.Abs(Mathf.Cos(stride)) * .024f * movingBlend) + breath, 0f);
        body.localRotation = Quaternion.Euler(run && speed > .5f ? 5f : 0f, 0f,
            -wave * 1.5f * movingBlend);
        skirt.localRotation = Quaternion.Euler(swing * .10f, 0f, 0f);
        sashTail.localRotation = Quaternion.Euler(5f + movingBlend * 13f
            + Mathf.Sin(stride + .7f) * movingBlend * 8f, 0f, -8f);
    }

    void BuildAvatar()
    {
        Material skin = MakeMaterial("Pilgrim • warm umber", new Color(.24f, .115f, .065f));
        Material cloth = MakeMaterial("Pilgrim • saffron cotton", new Color(.78f, .39f, .095f));
        Material gold = MakeMaterial("Pilgrim • woven hem", new Color(.97f, .67f, .22f));
        Material teal = MakeMaterial("Pilgrim • river teal sash", new Color(.055f, .31f, .30f));
        Material hair = MakeMaterial("Pilgrim • charcoal hair", new Color(.045f, .033f, .024f));
        Material eye = MakeMaterial("Pilgrim • eyes", new Color(.095f, .055f, .026f));
        Material tilak = MakeMaterial("Pilgrim • sandalwood mark", new Color(.95f, .76f, .48f));

        avatar = Pivot("Pilgrim visual", transform, Vector3.zero);
        body = Pivot("Breathing body", avatar, Vector3.zero);
        Part("Abdomen", PrimitiveType.Sphere, body, new Vector3(0f, 1.055f, 0f),
            new Vector3(.36f, .36f, .24f), skin);
        Part("Chest", PrimitiveType.Sphere, body, new Vector3(0f, 1.295f, 0f),
            new Vector3(.48f, .41f, .285f), skin);
        Part("Neck", PrimitiveType.Cylinder, body, new Vector3(0f, 1.495f, 0f),
            new Vector3(.13f, .085f, .13f), skin);
        Part("Head", PrimitiveType.Sphere, body, new Vector3(0f, 1.665f, .005f),
            new Vector3(.285f, .35f, .28f), skin);
        Part("Nose", PrimitiveType.Sphere, body, new Vector3(0f, 1.655f, .145f),
            new Vector3(.055f, .073f, .075f), skin);
        for (int i = -1; i <= 1; i += 2)
        {
            Part("Ear", PrimitiveType.Sphere, body, new Vector3(i * .143f, 1.66f, .005f),
                new Vector3(.05f, .09f, .052f), skin);
            Part("Eye", PrimitiveType.Sphere, body, new Vector3(i * .061f, 1.695f, .127f),
                new Vector3(.027f, .025f, .015f), eye);
            Part("Sandal", PrimitiveType.Cube, BuildLeg(i < 0, skin),
                new Vector3(0f, -.435f, .042f), new Vector3(.13f, .05f, .23f), hair);
        }
        Part("Hair cap", PrimitiveType.Sphere, body, new Vector3(0f, 1.797f, -.012f),
            new Vector3(.282f, .145f, .279f), hair);
        Part("Hair bun", PrimitiveType.Sphere, body, new Vector3(0f, 1.78f, -.135f),
            new Vector3(.155f, .16f, .15f), hair);
        Part("Forehead mark", PrimitiveType.Cube, body, new Vector3(0f, 1.748f, .128f),
            new Vector3(.024f, .048f, .008f), tilak);

        BuildArm(true, skin);
        BuildArm(false, skin);
        skirt = Pivot("Dhoti", body, new Vector3(0f, .99f, 0f));
        RingCloth("Ochre dhoti", skirt, 0f, -.43f, .205f, .27f, .145f, .18f, cloth);
        RingCloth("Gold woven hem", skirt, -.393f, -.438f, .262f, .272f, .176f, .182f, gold);
        Part("Front pleat", PrimitiveType.Cube, skirt, new Vector3(.035f, -.24f, .179f),
            new Vector3(.125f, .39f, .018f), gold).localRotation = Quaternion.Euler(0f, 0f, -5f);
        Part("Waist tie", PrimitiveType.Sphere, body, new Vector3(0f, .987f, 0f),
            new Vector3(.438f, .075f, .306f), teal);
        Transform frontSash = Part("Sash front", PrimitiveType.Cube, body,
            new Vector3(0f, 1.255f, .145f), new Vector3(.125f, .51f, .038f), teal);
        frontSash.localRotation = Quaternion.Euler(0f, 0f, 28f);
        Transform backSash = Part("Sash back", PrimitiveType.Cube, body,
            new Vector3(0f, 1.255f, -.139f), new Vector3(.125f, .51f, .031f), teal);
        backSash.localRotation = Quaternion.Euler(0f, 0f, 28f);
        Part("Sash shoulder fold", PrimitiveType.Sphere, body,
            new Vector3(-.12f, 1.459f, 0f), new Vector3(.14f, .065f, .32f), teal);
        sashTail = Pivot("Loose sash end", body, new Vector3(.20f, .984f, -.07f));
        Part("Sash tail", PrimitiveType.Cube, sashTail,
            new Vector3(0f, -.18f, 0f), new Vector3(.115f, .37f, .025f), teal);
        Part("Sash tail trim", PrimitiveType.Cube, sashTail,
            new Vector3(0f, -.352f, -.001f), new Vector3(.117f, .023f, .027f), gold);
    }

    Transform BuildLeg(bool left, Material skin)
    {
        float sign = left ? -1f : 1f;
        Transform hip = Pivot(left ? "Left hip" : "Right hip", body, new Vector3(sign * .112f, .83f, 0f));
        Part("Thigh", PrimitiveType.Cylinder, hip, new Vector3(0f, -.185f, 0f),
            new Vector3(.14f, .185f, .14f), skin);
        Transform knee = Pivot("Knee", hip, new Vector3(0f, -.37f, 0f));
        Part("Knee joint", PrimitiveType.Sphere, knee, Vector3.zero, new Vector3(.14f, .14f, .14f), skin);
        Part("Calf", PrimitiveType.Cylinder, knee, new Vector3(0f, -.20f, 0f),
            new Vector3(.108f, .20f, .108f), skin);
        Part("Foot", PrimitiveType.Sphere, knee, new Vector3(0f, -.417f, .044f),
            new Vector3(.122f, .088f, .216f), skin);
        if (left) { leftHip = hip; leftKnee = knee; } else { rightHip = hip; rightKnee = knee; }
        return knee;
    }

    void BuildArm(bool left, Material skin)
    {
        float sign = left ? -1f : 1f;
        Transform shoulder = Pivot(left ? "Left shoulder" : "Right shoulder", body,
            new Vector3(sign * .245f, 1.40f, 0f));
        Part("Shoulder joint", PrimitiveType.Sphere, shoulder, Vector3.zero,
            new Vector3(.17f, .18f, .18f), skin);
        Part("Upper arm", PrimitiveType.Cylinder, shoulder, new Vector3(0f, -.135f, 0f),
            new Vector3(.115f, .135f, .115f), skin);
        Transform elbow = Pivot("Elbow", shoulder, new Vector3(0f, -.27f, 0f));
        Part("Elbow joint", PrimitiveType.Sphere, elbow, Vector3.zero,
            new Vector3(.12f, .12f, .12f), skin);
        Part("Forearm", PrimitiveType.Cylinder, elbow, new Vector3(0f, -.12f, 0f),
            new Vector3(.093f, .12f, .093f), skin);
        Part("Hand", PrimitiveType.Sphere, elbow, new Vector3(0f, -.277f, .007f),
            new Vector3(.09f, .13f, .062f), skin);
        if (left) { leftShoulder = shoulder; leftElbow = elbow; }
        else { rightShoulder = shoulder; rightElbow = elbow; }
    }

    static Material MakeMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
        Material m = new Material(shader) { name = name, color = color };
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", .12f);
        return m;
    }

    static Transform Pivot(string name, Transform parent, Vector3 position)
    {
        GameObject go = new GameObject(name);
        go.layer = 2;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        return go.transform;
    }

    static Transform Part(string name, PrimitiveType shape, Transform parent,
        Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.layer = 2;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) { collider.enabled = false; Object.Destroy(collider); }
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go.transform;
    }

    static void RingCloth(string name, Transform parent, float top, float bottom,
        float topX, float bottomX, float topZ, float bottomZ, Material material)
    {
        const int segments = 20;
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[segments * 6];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float pleat = i % 2 == 0 ? 1f : .956f;
            vertices[i * 2] = new Vector3(Mathf.Cos(angle) * topX, top, Mathf.Sin(angle) * topZ);
            vertices[i * 2 + 1] = new Vector3(Mathf.Cos(angle) * bottomX * pleat,
                bottom, Mathf.Sin(angle) * bottomZ * pleat);
            uv[i * 2] = new Vector2((float)i / segments, 1f);
            uv[i * 2 + 1] = new Vector2((float)i / segments, 0f);
            if (i == segments) continue;
            int t = i * 6, v = i * 2;
            triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
            triangles[t + 3] = v + 1; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
        }
        Mesh mesh = new Mesh { name = name, vertices = vertices, uv = uv, triangles = triangles };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Transform item = Pivot(name, parent, Vector3.zero);
        item.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        item.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
    }
}
