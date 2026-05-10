using ExtensionMethods;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum RESULT_STATE
{
    BEFORE_START,
    CAMERA_TRANSITION,
    BG_TRANSITION,
    TIME_TRANSITION,
    RANK_TRANSITION,
    INPUT_WAIT,
    END
};

public class ResultController : MonoBehaviour
{
    public Transform character_position;
    public Camera result_camera;
    public Canvas result_canvas;
    public GameObject result_canvas_layout;
    public TextMeshProUGUI time_txt;
    public TextMeshProUGUI rank_txt;
    public Transform camera_start_transform;
    public float camera_start_field_of_view = 60.0f;
    public Transform camera_final_transform;
    public float camera_final_field_of_view = 12.0f;
    public Transform bezier_control_point;
    public float camera_transition_duration = 1.0f;
    public float bg_transition_duration = 0.3f;
    public float time_transition_duration = 1.0f;
    public float rank_transition_duration = 1.0f;

    public bool debug_result = false;

    private float camera_transition_t = 0.0f;
    private float bg_transition_t = 0.0f;
    private float time_transition_t = 0.0f;
    private float rank_transition_t = 0.0f;

    private RESULT_STATE state = RESULT_STATE.BEFORE_START;

    // Start is called before the first frame update
    void Start()
    {
        BattleSequenceController.resultController = this;
    }

    // Update is called once per frame
    void Update()
    {

        if (state == RESULT_STATE.CAMERA_TRANSITION)
        {
            result_camera.transform.position = Vector3Extensions.bezier2(
                                                    camera_start_transform.position,
                                                    bezier_control_point.position,
                                                    camera_final_transform.position,
                                                    camera_transition_t
                                                );
            result_camera.transform.rotation = Quaternion.Slerp(camera_start_transform.rotation, camera_final_transform.rotation, camera_transition_t);
            result_camera.fieldOfView = camera_start_field_of_view + (camera_final_field_of_view - camera_start_field_of_view) * camera_transition_t;
            if (camera_transition_t == 1.0f) {
                state = RESULT_STATE.BG_TRANSITION;
            }
            camera_transition_t = (camera_transition_t > 1.0f)?1.0f: camera_transition_t + Time.deltaTime / camera_transition_duration;
        }
        else if (state == RESULT_STATE.BG_TRANSITION)
        {
            result_canvas_layout.transform.localScale = new Vector3(bg_transition_t, 1, 1);
            if (bg_transition_t == 1.0f)
            {
                state = RESULT_STATE.INPUT_WAIT;
            }
            bg_transition_t = (bg_transition_t > 1.0f) ? 1.0f : bg_transition_t + Time.deltaTime / bg_transition_duration;
        }
        else if (state == RESULT_STATE.TIME_TRANSITION)
        {

        }
        else if (state == RESULT_STATE.RANK_TRANSITION)
        {

        }
        else if (state == RESULT_STATE.INPUT_WAIT)
        {
            if (debug_result)
            {
                StartProcedure(0, "-");
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            
        }
    }

    private void FixedUpdate()
    {
        
    }

    public RESULT_STATE GetState()
    {
        return state;
    }

    public void StartProcedure(float clear_time, string rank)
    {
        result_canvas.gameObject.SetActive(true);
        TimeSpan timeSpan = TimeSpan.FromSeconds(clear_time);

        // 00:00形式にフォーマット
        string formattedTime = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        time_txt.text = formattedTime;
        rank_txt.text = rank;

        result_camera.transform.position = camera_start_transform.position;
        result_camera.transform.rotation = camera_start_transform.rotation;
        result_camera.fieldOfView = camera_start_field_of_view;

        CharacterModel player_character = BattleSequenceController.getPlayerCharacter();
        player_character.transform.position = character_position.position;
        player_character.transform.rotation = character_position.rotation;

        result_camera.gameObject.SetActive(true);
        player_character.GetComponent<CharacterCameraController>().camera.gameObject.SetActive(false);

        player_character.Win();
        state = RESULT_STATE.CAMERA_TRANSITION;
        camera_transition_t = 0.0f;
        bg_transition_t = 0.0f;
        time_transition_t = 0.0f;
        rank_transition_t = 0.0f;
    }

    public void OnButtonClick(string id)
    {
        if(id == "return")
        {
            SceneManager.LoadScene("title");
        }
        else if(id == "reload")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
