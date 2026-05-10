using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// シーン進行状態
public enum SCENE_STATE
{
    BEFORE_START,
    READY,
    PLAYING,
    PAUSED,
    WIN,
    LOSE,
    RESULT
};

// =======================================================
// BattleSequenceController
// -------------------------------------------------------
// バトル全体の進行管理クラス
//
// シーン状態管理、キャラクター管理、敵リスト生成、
// 勝敗判定、一時停止、リザルト遷移などを担当する
// =======================================================

public class BattleSequenceController : MonoBehaviour
{
    // プレイヤー制御
    static private PlayerController _playerController;

    // リザルト制御
    static private ResultController _resultController;

    // 現在のシーン状態
    static private SCENE_STATE scene_state = SCENE_STATE.BEFORE_START;

    // 地面レイヤー番号
    static private int ground_layer_number;

    // チームごとのキャラクター一覧
    static private Dictionary<int, List<CharacterModel>> teams;

    // チームごとの敵一覧
    static private Dictionary<int, List<CharacterModel>> enemies;

    // 一時停止状態
    static private bool _isPause = false;

    // ラベル演出時間
    private float fade_duration = 0.17f;
    private float label_duration = 0.83f;

    private float fade_t = 0.0f;
    private float label_t = 0.0f;

    // 経過時間
    protected float time_count = 0.0f;

    // UI
    public GameObject label_ready;
    public GameObject label_mission_start;
    public GameObject label_mission_complete;

    public GameObject lose_canvas;
    public GameObject pauseCanvas;

    // デバッグ用
    public bool debug_result = false;

    // =======================================================
    // static property
    // =======================================================

    static public PlayerController playerController
    {
        set { _playerController = value; }
        get { return _playerController; }
    }

    static public ResultController resultController
    {
        set { _resultController = value; }
        get { return _resultController; }
    }

    static public bool isPause
    {
        get { return _isPause; }
        set { _isPause = value; }
    }

    // 現在のシーン状態取得
    static public SCENE_STATE GetSceneState()
    {
        return scene_state;
    }

    // プレイヤーキャラクター取得
    static public CharacterModel getPlayerCharacter()
    {
        return playerController.player_character;
    }

    // 地面レイヤー取得
    static public int GetGroundLayerNumber()
    {
        return ground_layer_number;
    }

    // =======================================================
    // キャラクター登録
    // =======================================================

    static public void RegisterCharacter(CharacterModel character)
    {
        if (teams == null)
        {
            teams = new Dictionary<int, List<CharacterModel>>();
        }

        if (!teams.ContainsKey(character.team))
        {
            teams.Add(character.team, new List<CharacterModel>());
        }

        teams[character.team].Add(character);

        UpdateEnemyList();
    }

    // キャラクター削除
    static public void DeregisterCharacter(CharacterModel character)
    {
        if (teams == null)
        {
            teams = new Dictionary<int, List<CharacterModel>>();
        }

        if (teams.ContainsKey(character.team))
        {
            teams[character.team].Remove(character);
        }

        UpdateEnemyList();
    }

    // 指定チームから見た敵一覧取得
    static public CharacterModel[] GetEnemies(int self_team)
    {
        if (enemies == null)
        {
            enemies = new Dictionary<int, List<CharacterModel>>();
        }

        if (!enemies.ContainsKey(self_team))
        {
            enemies.Add(self_team, new List<CharacterModel>());
        }

        return enemies[self_team].ToArray();
    }

    // 全チーム取得
    static public int[] GetAllTeams()
    {
        return teams.Keys.ToArray();
    }

    // チーム所属キャラクター取得
    static public CharacterModel[] GetCharacterModels(int team)
    {
        if (teams.ContainsKey(team))
        {
            return teams[team].ToArray();
        }

        return null;
    }

    // =======================================================
    // 敵一覧更新
    // =======================================================

    static private void UpdateEnemyList()
    {
        if (teams != null &&
            playerController != null &&
            playerController.player_character != null)
        {
            Dictionary<int, List<CharacterModel>> _enemies =
                new Dictionary<int, List<CharacterModel>>();

            int[] team_numbers = teams.Keys.ToArray();

            foreach (KeyValuePair<int, List<CharacterModel>> team_char in teams)
            {
                foreach (int team_number in team_numbers)
                {
                    if (team_number != team_char.Key)
                    {
                        if (!_enemies.ContainsKey(team_number))
                        {
                            _enemies.Add(
                                team_number,
                                new List<CharacterModel>()
                            );
                        }

                        _enemies[team_number].AddRange(team_char.Value);
                    }
                }
            }

            enemies = _enemies;
        }
    }

    void Start()
    {
        scene_state = SCENE_STATE.BEFORE_START;

        ground_layer_number =
            LayerMask.NameToLayer("ground");

        if (teams == null)
        {
            teams = new Dictionary<int, List<CharacterModel>>();
        }

        if (enemies == null)
        {
            enemies = new Dictionary<int, List<CharacterModel>>();
        }

        UpdateEnemyList();
    }

    // =======================================================
    // ラベル演出
    // =======================================================

    private bool TransLabel(
        float fade_duration,
        float stop_duration,
        float deltaTime)
    {
        if (label_t <= 1.0f)
        {
            if (fade_t < 1.0f)
            {
                fade_t += deltaTime / fade_duration;
            }
            else
            {
                label_t += deltaTime / stop_duration;
            }
        }
        else
        {
            if (fade_t > 0.0f)
            {
                fade_t -= deltaTime / fade_duration;
            }
            else
            {
                return true;
            }
        }

        fade_t = Mathf.Min(fade_t, 1.0f);
        fade_t = Mathf.Max(fade_t, 0.0f);

        return false;
    }

    virtual public void Update()
    {
        // =======================================================
        // 開始待機
        // =======================================================

        if (scene_state == SCENE_STATE.BEFORE_START)
        {
            bool ready =
                !!playerController &&
                !!resultController;

            if (ready)
            {
                // READY音声
                if (playerController.player_character.audio_clips.Count > 10)
                {
                    playerController.player_character.audio_source.PlayOneShot(
                        playerController.player_character.audio_clips[10]
                    );
                }

                scene_state = SCENE_STATE.READY;
            }
        }

        // =======================================================
        // READY演出
        // =======================================================

        else if (scene_state == SCENE_STATE.READY)
        {
            if (TransLabel(fade_duration, label_duration, Time.deltaTime))
            {
                fade_t = 0.0f;
                label_t = 0.0f;

                label_ready.SetActive(false);

                scene_state = SCENE_STATE.PLAYING;
            }

            label_ready.transform.localScale =
                new Vector3(1, fade_t, 1);
        }

        // =======================================================
        // プレイ中
        // =======================================================

        else if (scene_state == SCENE_STATE.PLAYING)
        {
            // ミッション開始ラベル
            if (label_mission_start.activeSelf)
            {
                if (TransLabel(fade_duration, label_duration, Time.deltaTime))
                {
                    fade_t = 0.0f;
                    label_t = 0.0f;

                    label_mission_start.SetActive(false);
                }

                label_mission_start.transform.localScale =
                    new Vector3(1, fade_t, 1);
            }

            // 敗北判定
            if (CheckLose())
            {
                Time.timeScale = 0.5f;

                scene_state = SCENE_STATE.LOSE;
            }

            // 勝利判定
            else if (CheckWin())
            {
                Time.timeScale = 0.5f;

                scene_state = SCENE_STATE.WIN;
            }

            time_count += Time.deltaTime;

            // =======================================================
            // 一時停止
            // =======================================================

            if (isPause)
            {
                pauseCanvas.SetActive(true);

                Time.timeScale = 0.0f;

                Cursor.visible = true;

                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                pauseCanvas.SetActive(false);

                Time.timeScale = 1.0f;

                Cursor.visible = false;

                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        // =======================================================
        // 勝利演出
        // =======================================================

        else if (scene_state == SCENE_STATE.WIN)
        {
            if (TransLabel(fade_duration, label_duration, Time.deltaTime))
            {
                fade_t = 0.0f;
                label_t = 0.0f;

                label_mission_complete.SetActive(false);

                Time.timeScale = 1.0f;

                scene_state = SCENE_STATE.RESULT;
            }

            label_mission_complete.transform.localScale =
                new Vector3(1, fade_t, 1);
        }

        // =======================================================
        // 敗北
        // =======================================================

        else if (scene_state == SCENE_STATE.LOSE)
        {
            Time.timeScale = 1.0f;

            lose_canvas.SetActive(true);

            Cursor.visible = true;

            Cursor.lockState = CursorLockMode.None;
        }

        // =======================================================
        // リザルト
        // =======================================================

        else if (scene_state == SCENE_STATE.RESULT)
        {
            RESULT_STATE result_state =
                resultController.GetState();

            if (result_state == RESULT_STATE.BEFORE_START)
            {
                resultController.StartProcedure(
                    time_count,
                    GetRank()
                );
            }
            else if (result_state == RESULT_STATE.END)
            {

            }
        }
    }

    // 勝利条件
    virtual protected bool CheckWin()
    {
        if (debug_result)
        {
            return true;
        }

        return GetEnemies(playerController.player_character.team).Length == 0;
    }

    // 敗北条件
    virtual protected bool CheckLose()
    {
        return playerController.player_character.hp <= 0.0f;
    }

    // ランク取得
    virtual protected string GetRank()
    {
        return "--";
    }

    private void OnDestroy()
    {
        playerController = null;
        resultController = null;

        scene_state = SCENE_STATE.BEFORE_START;

        teams.Clear();
        enemies.Clear();
    }

    public bool GetIsPause()
    {
        return isPause;
    }

    public void SetIsPause(bool value)
    {
        isPause = value;
    }

    // タイトルへ戻る
    public void ReturnToTitle()
    {
        SceneManager.LoadScene("title");
    }

    // ポーズメニュー操作
    public void PauseMenuButton(string id)
    {
        if (id == "resume")
        {
            isPause = false;
        }
        else if (id == "return")
        {
            isPause = false;

            Time.timeScale = 1.0f;

            SceneManager.LoadScene("title");
        }
    }
}