using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

public class BattleSequenceController : MonoBehaviour
{

    static private PlayerController _playerController;
    static private ResultController _resultController;

    static private SCENE_STATE scene_state = SCENE_STATE.BEFORE_START;
    static private int ground_layer_number;
    static private Dictionary<int, List<CharacterModel>> teams;
    static private Dictionary<int, List<CharacterModel>> enemies;
    static private bool _isPause = false;

    private float fade_duration = 0.17f;
    private float label_duration = 0.83f;
    private float fade_t = 0.0f;
    private float label_t = 0.0f;

    protected float time_count = 0.0f;

    public GameObject label_ready;
    public GameObject label_mission_start;
    public GameObject label_mission_complete;
    public GameObject lose_canvas;
    public GameObject pauseCanvas;
    public bool debug_result = false;
    


    static public PlayerController playerController {  
        set { _playerController = value; }
        get { return _playerController; } 
    }

    static public ResultController resultController { 
        set { _resultController = value; }
        get { return _resultController; } 
    }

    static public bool isPause
    {
        get { return _isPause; }
        set { _isPause = value; }
    }

    static public SCENE_STATE GetSceneState() { 
        return scene_state;
    }


    static public CharacterModel getPlayerCharacter() {
        return playerController.player_character;
    }

    static public int GetGroundLayerNumber()
    {
        return ground_layer_number;
    }

    static public void RegisterCharacter(CharacterModel character) {
        if(teams == null)
        {
            teams = new Dictionary<int, List<CharacterModel>>();
        }
        if (!teams.ContainsKey(character.team)) {
            teams.Add(character.team, new List<CharacterModel>());
        }
        teams[character.team].Add(character);
        UpdateEnemyList();
    }

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

    static public CharacterModel[] GetEnemies(int self_team) {
        if(enemies == null)
        {
            enemies = new Dictionary<int, List<CharacterModel>>();
        }
        if (!enemies.ContainsKey(self_team))
        {
            enemies.Add(self_team, new List<CharacterModel>());
        }
        return enemies[self_team].ToArray();
    }

    static public int[] GetAllTeams()
    {
        return teams.Keys.ToArray();
    }

    static public CharacterModel[] GetCharacterModels(int team) {
        if (teams.ContainsKey(team)){
            return teams[team].ToArray();
        }
        return null;
    }

    static private void UpdateEnemyList()
    {
        if (teams != null && playerController != null && playerController.player_character != null)
        {
            Dictionary<int, List<CharacterModel>> _enemies = new Dictionary<int, List<CharacterModel>>();
            int[] team_numbers = teams.Keys.ToArray();
            foreach (KeyValuePair<int, List<CharacterModel>> team_char in teams)
            {
                foreach(int team_number in team_numbers)
                {
                    if(team_number != team_char.Key)
                    {
                        if (!_enemies.ContainsKey(team_number))
                        {
                            _enemies.Add(team_number, new List<CharacterModel>());
                        }
                        _enemies[team_number].AddRange(team_char.Value);
                    }
                    
                }
            }
            enemies = _enemies;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        scene_state = SCENE_STATE.BEFORE_START;
        ground_layer_number = LayerMask.NameToLayer("ground");
        if (teams == null) {
            teams = new Dictionary<int, List<CharacterModel>>();
        }
        if(enemies == null)
        {
            enemies = new Dictionary<int, List<CharacterModel>>();
        }
        UpdateEnemyList();
    }

    private bool TransLabel(float fade_duration, float stop_duration, float deltaTime)
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

    // Update is called once per frame
    virtual public void Update()
    {
        if (scene_state == SCENE_STATE.BEFORE_START) {
            bool ready = !!playerController && !!resultController;

            if (ready)
            {
                if(playerController.player_character.audio_clips.Count > 10)
                {
                    playerController.player_character.audio_source.PlayOneShot(playerController.player_character.audio_clips[10]);
                }
                
                scene_state = SCENE_STATE.READY;
            }

        }
        else if (scene_state == SCENE_STATE.READY)
        {
            if (TransLabel(fade_duration, label_duration, Time.deltaTime)) {
                fade_t = 0.0f;
                label_t = 0.0f;
                label_ready.SetActive(false);
                scene_state = SCENE_STATE.PLAYING;
            }
            label_ready.transform.localScale = new Vector3(1, fade_t, 1);
        }
        else if (scene_state == SCENE_STATE.PLAYING)
        {
            if (label_mission_start.activeSelf)
            {
                if (TransLabel(fade_duration, label_duration, Time.deltaTime))
                {
                    fade_t = 0.0f;
                    label_t = 0.0f;
                    label_mission_start.SetActive(false);
                }
                label_mission_start.transform.localScale = new Vector3(1, fade_t, 1);
            }

            if (CheckLose())
            {
                Time.timeScale = 0.5f;
                scene_state = SCENE_STATE.LOSE;
            }
            else if (CheckWin())
            {
                Time.timeScale = 0.5f;
                scene_state = SCENE_STATE.WIN;
            }
            time_count += Time.deltaTime;
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
            label_mission_complete.transform.localScale = new Vector3(1, fade_t, 1);
        }
        else if (scene_state == SCENE_STATE.LOSE)
        {
            Time.timeScale = 1.0f;
            lose_canvas.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (scene_state == SCENE_STATE.RESULT)
        {
            RESULT_STATE result_state = resultController.GetState();
            if (result_state == RESULT_STATE.BEFORE_START)
            {
                resultController.StartProcedure(time_count, GetRank());
            }
            else if (result_state == RESULT_STATE.END)
            {

            }

        }
        
    }

    virtual protected bool CheckWin()
    {
        if (debug_result)
        {
            return true;
        }
        return GetEnemies(playerController.player_character.team).Length == 0;
    }

    virtual protected bool CheckLose()
    {
        return playerController.player_character.hp <= 0.0f;
    }

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

    public void ReturnToTitle()
    {
        SceneManager.LoadScene("title");
    }

    public void PauseMenuButton(string id)
    {
        if(id == "resume")
        {
            isPause = false;
        }
        else if(id == "return")
        {
            isPause = false;
            Time.timeScale = 1.0f;
            SceneManager.LoadScene("title");
        }
    }
}
