using UnityEngine;
using ExtensionMethods;
using UnityEngine.InputSystem;

public enum CAMERA_MODE
{
    FREE_ANGLE,
    LOCK_TARGET
}

public class PlayerController : MonoBehaviour
{
    public const int TEAM = 1;
    public CharacterModel player_character;
    public float free_angle_rotate_speed = 1.0f;
    public float free_angle_focus_distance = 30.0f;
    public float double_input_time = 0.3f;
    public Vector3 lockon_assist_area = new Vector3 (100.0f, 100.0f, 120.0f);

    private int target_index = 0;

    private Vector2 previous_raw_direction = Vector2.zero;
    private Vector2 double_input_direction = Vector2.zero;
    private float double_input_count = 0.0f;

    private CAMERA_MODE camera_mode = CAMERA_MODE.FREE_ANGLE;
    private Quaternion camera_rotation = Quaternion.identity;
    private float camera_rotation_x = 0.0f;
    private float camera_rotation_y = 0.0f;
    private Vector3 camera_offset = new Vector3(0, 2, -5);
    private bool lockon_assist = false;
    private bool init_look_at = true;
    private bool isPause = false;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        BattleSequenceController.playerController = this;
    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = Vector3.zero;

        float move_x = 0.0f;
        float move_y = 0.0f;
        float raw_move_x = 0.0f;
        float raw_move_y = 0.0f;
        float pointer_x = 0.0f;
        float pointer_y = 0.0f;
        bool main_attack = false;
        bool sub_attack = false;
        bool jump_down = false;
        bool double_jump_down = false;
        bool fly = false;
        bool camera_mode_chanage = false;
        bool lockon_assist_onoff = false;
        bool reset_look_at = init_look_at;
        //bool change_target = false;
        bool axis_hit = false; // 方向変更（0に戻すも含む）
        bool double_input_move = false;
        bool pause = false;
        bool hasGamepad = (Gamepad.current != null);

        if (BattleSequenceController.GetSceneState() == SCENE_STATE.PLAYING)
        {
            // 入力
            move_x = Input.GetAxis("Horizontal");
            move_y = Input.GetAxis("Vertical");
            raw_move_x = Input.GetAxisRaw("Horizontal");
            raw_move_y = Input.GetAxisRaw("Vertical");
            pointer_x = Input.GetAxis("Mouse X");
            pointer_y = Input.GetAxis("Mouse Y");
            main_attack = Input.GetMouseButton(0);
            sub_attack = Input.GetMouseButton(1);
            jump_down = Input.GetKeyDown(KeyCode.Space);
            fly = Input.GetKey(KeyCode.Space);
            // camera_mode_chanage = Input.GetKeyDown(KeyCode.R);
            lockon_assist_onoff = Input.GetKeyDown(KeyCode.LeftShift);
            pause = Input.GetKeyDown(KeyCode.Return);

            if (hasGamepad)
            {
                Vector2 left_stick = Gamepad.current.leftStick.value;
                if (left_stick != Vector2.zero)
                {
                    move_x = left_stick.x;
                    move_y = left_stick.y;
                    raw_move_x = Mathf.Round(left_stick.x);
                    raw_move_y = Mathf.Round(left_stick.y);
                }
                Vector2 right_stick = Gamepad.current.rightStick.value;
                if(right_stick != Vector2.zero)
                {
                    pointer_x = right_stick.x;
                    pointer_y = right_stick.y;
                }
                main_attack = main_attack || Gamepad.current.rightShoulder.isPressed;
                sub_attack = sub_attack || Gamepad.current.rightTrigger.isPressed;
                jump_down = jump_down || Gamepad.current.leftTrigger.wasPressedThisFrame;
                fly = fly || Gamepad.current.leftTrigger.isPressed;
                lockon_assist_onoff = lockon_assist_onoff || Gamepad.current.yButton.wasPressedThisFrame;
                pause = pause || Gamepad.current.startButton.wasPressedThisFrame;
            }


            Vector2 raw_move = new Vector2(raw_move_x, raw_move_y);
            axis_hit = (raw_move != previous_raw_direction);

            double_input_count = Mathf.Min(double_input_count + Time.deltaTime, double_input_time + 1.0f);
            if (axis_hit)
            {
                if(raw_move == double_input_direction && double_input_count <= double_input_time)
                {
                    double_input_move = true;
                }
                else if(raw_move != Vector2.zero)
                {
                    double_input_direction = raw_move;
                    double_input_count = 0.0f;
                }
            }
            previous_raw_direction = raw_move;
        }

        if (!BattleSequenceController.isPause)
        {
            if (camera_mode_chanage)
            {
                camera_mode = (camera_mode == CAMERA_MODE.FREE_ANGLE) ? CAMERA_MODE.LOCK_TARGET : CAMERA_MODE.FREE_ANGLE;
            }
            if (lockon_assist_onoff)
            {
                lockon_assist = !lockon_assist;
            }

            CharacterCameraController characterCameraController = (player_character) ? player_character.GetComponent<CharacterCameraController>() : null;
            Camera camera = (characterCameraController) ? characterCameraController.camera : null;
            if (camera)
            {
                characterCameraController.keyboard_hints.SetActive(!hasGamepad);
                characterCameraController.procon_hits.SetActive(hasGamepad);

                characterCameraController.assist_off_label.gameObject.SetActive(!lockon_assist);
                characterCameraController.assist_on_label.gameObject.SetActive(lockon_assist);

                CharacterModel[] enemies = BattleSequenceController.GetEnemies(player_character.team);
                GameObject target = null;
                Vector3 look_at = Vector3.zero;
                init_look_at = false;
                if (reset_look_at)
                {
                    Vector3 player_position = player_character.gameObject.GetCenterPoint();
                    camera_offset = player_character.transform.rotation * camera_offset;

                }
                if (camera_mode == CAMERA_MODE.FREE_ANGLE)
                {
                    Vector3 pointer_move = new Vector3(-pointer_y * Options.camera_speed, pointer_x * Options.camera_speed, 0);
                    pointer_move = camera.transform.TransformDirection(pointer_move);
                    Vector3 player_position = player_character.gameObject.GetCenterPoint();
                    camera_offset = Quaternion.Euler(pointer_move) * camera_offset;
                    camera.transform.position = player_position + camera_offset;
                    camera.transform.rotation = Quaternion.LookRotation(player_position - camera.transform.position) * Quaternion.Euler(-15, 0, 0);

                    look_at = camera.transform.position + camera.transform.forward * free_angle_focus_distance;

                    float distance = -1.0f;
                    foreach (CharacterModel enemy in enemies)
                    {
                        Vector3 enemy_position = enemy.gameObject.GetCenterPoint();
                        Vector3 screenPos = camera.WorldToScreenPoint(enemy_position);
                        screenPos.x = (screenPos.x - camera.scaledPixelWidth / 2) / characterCameraController.canvas.scaleFactor;
                        screenPos.y = (screenPos.y - camera.scaledPixelHeight / 2) / characterCameraController.canvas.scaleFactor;
                        if (screenPos.z > 0 && Mathf.Abs(screenPos.x) < lockon_assist_area.x / 2 && Mathf.Abs(screenPos.y) < lockon_assist_area.y / 2)
                        {
                            if (screenPos.z <= lockon_assist_area.z)
                            {
                                if (distance < 0.0f || screenPos.z < distance)
                                {
                                    target = enemy.gameObject;
                                    if (lockon_assist)
                                    {
                                        look_at = enemy.gameObject.GetCenterPoint();
                                    }
                                    distance = screenPos.z;
                                }
                            }
                        }
                    }
                }
                else if (camera_mode == CAMERA_MODE.LOCK_TARGET)
                {

                    if (enemies.Length > 0)
                    {
                        Vector3 player_position = player_character.gameObject.GetCenterPoint();
                        Vector3 target_position = enemies[0].gameObject.GetCenterPoint();
                        Quaternion rotation = Quaternion.LookRotation(target_position - player_position);
                        camera.transform.position = player_character.transform.position + rotation * camera_offset;
                        camera.transform.LookAt(target_position);
                        look_at = target_position;
                        target = enemies[0].gameObject;
                    }

                }
                player_character.look_at = look_at;
                characterCameraController.target = target;

                Vector3 move_direction = camera.transform.TransformDirection(new Vector3(move_x, 0, move_y));

                // 主武器攻撃
                if (main_attack)
                {
                    player_character.Attack("main", player_character.look_at, target);
                }
                if (sub_attack)
                {
                    player_character.Attack("sub", player_character.look_at, target);
                }

                // 移動
                player_character.SetMoveDirection(move_direction);

                if (double_input_move)
                {
                    player_character.Step();
                }
                else if (jump_down)
                {
                    player_character.Jump();
                }
                if (fly)
                {
                    player_character.Moveflying();
                }

            }
        }

        

        if (pause)
        {
            BattleSequenceController.isPause = !BattleSequenceController.isPause;
        }

    }

    public int getTargetIndex() { 
        return target_index;
    }
}
