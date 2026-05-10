using ExtensionMethods;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// =======================================================
// ResultController
// -------------------------------------------------------
// リザルト画面制御クラス
//
// カメラ演出、背景演出、クリアタイム表示、
// ランク表示、リトライ・タイトル遷移を管理する
// =======================================================

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
    // プレイヤー配置位置
    public Transform character_position;

    // リザルト用カメラ
    public Camera result_camera;

    public Canvas result_canvas;
    public GameObject result_canvas_layout;

    // UI表示
    public TextMeshProUGUI time_txt;
    public TextMeshProUGUI rank_txt;

    // カメラ開始位置
    public Transform camera_start_transform;
    public float camera_start_field_of_view = 60.0f;

    // カメラ終了位置
    public Transform camera_final_transform;
    public float camera_final_field_of_view = 12.0f;

    // ベジェ制御点
    public Transform bezier_control_point;

    // 演出時間
    public float camera_transition_duration = 1.0f;
    public float bg_transition_duration = 0.3f;
    public float time_transition_duration = 1.0f;
    public float rank_transition_duration = 1.0f;

    public bool debug_result = false;

    // 演出進行率
    private float camera_transition_t = 0.0f;
    private float bg_transition_t = 0.0f;
    private float time_transition_t = 0.0f;
    private float rank_transition_t = 0.0f;

    private RESULT_STATE state = RESULT_STATE.BEFORE_START;

    void Start()
    {
        BattleSequenceController.resultController = this;
    }

    void Update()
    {
        // =======================================================
        // カメラ演出
        // =======================================================

        if (state == RESULT_STATE.CAMERA_TRANSITION)
        {
            // ベジェ曲線でカメラ移動
            result_camera.transform.position =
                Vector3Extensions.bezier2(
                    camera_start_transform.position,
                    bezier_control_point.position,
                    camera_final_transform.position,
                    camera_transition_t
                );

            // カメラ回転補間
            result_camera.transform.rotation =
                Quaternion.Slerp(
                    camera_start_transform.rotation,
                    camera_final_transform.rotation,
                    camera_transition_t
                );

            // FOV補間
            result_camera.fieldOfView =
                camera_start_field_of_view +
                (camera_final_field_of_view - camera_start_field_of_view) *
                camera_transition_t;

            if (camera_transition_t == 1.0f)
            {
                state = RESULT_STATE.BG_TRANSITION;
            }

            camera_transition_t =
                (camera_transition_t > 1.0f)
                ? 1.0f
                : camera_transition_t + Time.deltaTime / camera_transition_duration;
        }

        // =======================================================
        // 背景演出
        // =======================================================

        else if (state == RESULT_STATE.BG_TRANSITION)
        {
            result_canvas_layout.transform.localScale =
                new Vector3(bg_transition_t, 1, 1);

            if (bg_transition_t == 1.0f)
            {
                state = RESULT_STATE.INPUT_WAIT;
            }

            bg_transition_t =
                (bg_transition_t > 1.0f)
                ? 1.0f
                : bg_transition_t + Time.deltaTime / bg_transition_duration;
        }

        else if (state == RESULT_STATE.TIME_TRANSITION)
        {

        }

        else if (state == RESULT_STATE.RANK_TRANSITION)
        {

        }

        // =======================================================
        // 入力待機
        // =======================================================

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

    // リザルト演出開始
    public void StartProcedure(float clear_time, string rank)
    {
        result_canvas.gameObject.SetActive(true);

        TimeSpan timeSpan =
            TimeSpan.FromSeconds(clear_time);

        // 00:00形式へ変換
        string formattedTime =
            $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

        time_txt.text = formattedTime;
        rank_txt.text = rank;

        // カメラ初期化
        result_camera.transform.position =
            camera_start_transform.position;

        result_camera.transform.rotation =
            camera_start_transform.rotation;

        result_camera.fieldOfView =
            camera_start_field_of_view;

        CharacterModel player_character =
            BattleSequenceController.getPlayerCharacter();

        // プレイヤーをリザルト位置へ移動
        player_character.transform.position =
            character_position.position;

        player_character.transform.rotation =
            character_position.rotation;

        // 通常カメラを無効化してリザルトカメラへ切り替え
        result_camera.gameObject.SetActive(true);

        player_character
            .GetComponent<CharacterCameraController>()
            .camera.gameObject.SetActive(false);

        // 勝利モーション再生
        player_character.Win();

        state = RESULT_STATE.CAMERA_TRANSITION;

        camera_transition_t = 0.0f;
        bg_transition_t = 0.0f;
        time_transition_t = 0.0f;
        rank_transition_t = 0.0f;
    }

    // ボタン入力
    public void OnButtonClick(string id)
    {
        // タイトルへ戻る
        if (id == "return")
        {
            SceneManager.LoadScene("title");
        }

        // 現在シーンを再読み込み
        else if (id == "reload")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}