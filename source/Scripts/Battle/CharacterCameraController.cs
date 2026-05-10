using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CharacterCameraController : MonoBehaviour
{
    public Camera camera;
    public Canvas canvas;
    public VerticalLayoutGroup status_group;
    public VerticalLayoutGroup weapon_group;
    public Image hp_bar;
    public Image stamina_bar;
    public RawImage pointer;
    public TextMeshProUGUI assist_off_label;
    public TextMeshProUGUI assist_on_label;
    public GameObject keyboard_hints;
    public GameObject procon_hits;
    public GameObject target;

    public Color pointer_normal_color = new Color(1f, 1f, 1f, 156f / 255f);
    public Color pointer_lockon_color = new Color(1f, 0, 0, 156f / 255f);

    private Dictionary<int, GameObject> character_monitors;
    private Dictionary<int, GameObject> character_arrows;
    private Dictionary<int, GameObject> weapon_monitors;
    private Dictionary<int, GameObject> friend_monitors;

    // Start is called before the first frame update
    void Start()
    {
        //enemy_displays = new Dictionary<int, GameObject>();
        character_monitors = new Dictionary<int, GameObject>();
        character_arrows = new Dictionary<int, GameObject>();
        weapon_monitors = new Dictionary<int, GameObject>();
        friend_monitors = new Dictionary<int, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        CharacterModel character = GetComponent<CharacterModel>();

        if (character && camera && canvas)
        {
            // プレイヤーUI更新
            hp_bar.fillAmount = (float)character.hp / character.max_hp;
            stamina_bar.fillAmount = character.stamina / character.max_stamina;
            List<WeaponFeedback> weapons = character.GetWeaponFeedbacks();
            for(int i = 0; i < weapons.Count; i++)
            {
                WeaponFeedback weapon = weapons[i];
                if (!weapon_monitors.ContainsKey(weapon.GetHashCode()))
                {
                    GameObject _weapon_monitor = Instantiate(
                        (GameObject)Resources.Load("weapon_monitor"),
                        weapon_group.transform
                    );
                    weapon_monitors.Add(weapon.GetHashCode(), _weapon_monitor);
                }
                GameObject weapon_monitor_gameobject = weapon_monitors[weapon.GetHashCode()];
                WeaponMonitor weapon_monitor = weapon_monitor_gameobject.GetComponent<WeaponMonitor>();
                weapon_monitor.name_label.text = weapon.name;
                weapon_monitor.load_label.text = weapon.current_value + "/" + weapon.max_value;
                weapon_monitor.load_bar.fillAmount = (float)weapon.current_value / weapon.max_value;
            }

            List<int> existing_character_objs = new List<int>();
            foreach (int team in BattleSequenceController.GetAllTeams()) {
                foreach(CharacterModel _character in BattleSequenceController.GetCharacterModels(team))
                {
                    if(_character != character)
                    {
                        bool is_enemy = (_character.team != character.team);
                        bool is_target = (target == _character.gameObject);

                        if (!character_monitors.ContainsKey(_character.GetInstanceID()))
                        {
                            GameObject _character_monitor_obj = Instantiate((GameObject)Resources.Load("character_monitor"), canvas.transform);
                            character_monitors.Add(_character.GetInstanceID(), _character_monitor_obj);
                            _character_monitor_obj.transform.SetAsFirstSibling();
                        }

                        if (!character_arrows.ContainsKey(_character.GetInstanceID()))
                        {
                            GameObject _character_arrow_obj = Instantiate((GameObject)Resources.Load("character_arrow"), canvas.transform);
                            character_arrows.Add(_character.GetInstanceID(), _character_arrow_obj);
                            _character_arrow_obj.transform.SetAsFirstSibling();
                            _character_arrow_obj.GetComponent<CharacterArrow>().SetColor(is_enemy);
                        }

                        if (!is_enemy)
                        {
                            if (!friend_monitors.ContainsKey(_character.GetInstanceID()))
                            {
                                GameObject _friend_monitor_obj = Instantiate(
                                    (GameObject)Resources.Load("friend_monitor"),
                                    status_group.transform
                                );
                                friend_monitors.Add(_character.GetInstanceID(), _friend_monitor_obj);
                            }
                            GameObject friend_monitor_obj = friend_monitors[_character.GetInstanceID()];
                            FriendMonitor friend_monitor = friend_monitor_obj.GetComponent<FriendMonitor>();
                            friend_monitor.name_label.text = _character.display_name;
                            friend_monitor.hp_bar.fillAmount = (float)_character.hp / _character.max_hp;
                        }

                        GameObject character_monitor_obj = character_monitors[_character.GetInstanceID()];
                        GameObject character_arrow_obj = character_arrows[_character.GetInstanceID()];
                        CharacterMonitor character_monitor = character_monitor_obj.GetComponent<CharacterMonitor>();
                        
                        // 位置を設定
                        Vector3 character_position =_character.gameObject.GetCenterPoint();
                        Vector3 screenPos = camera.WorldToScreenPoint(character_position);
                        character_monitor_obj.SetActive((screenPos.z > 0));
                        screenPos.x = (screenPos.x - camera.scaledPixelWidth / 2) / canvas.scaleFactor;
                        screenPos.y = (screenPos.y - camera.scaledPixelHeight / 2) / canvas.scaleFactor;
                        screenPos.z = 0;
                        character_monitor_obj.transform.localPosition = screenPos;
                        character_monitor_obj.transform.rotation = Quaternion.LookRotation((character_position - camera.transform.position).normalized);

                        bool is_screen = true;
                        Vector3 arrow_upward = Vector3.zero;
                        if(screenPos.x < -camera.scaledPixelWidth / 2 / canvas.scaleFactor)
                        {
                            screenPos.x = -camera.scaledPixelWidth / 2 / canvas.scaleFactor;
                            arrow_upward.y = -1f;
                            is_screen = false;
                        }
                        else if(screenPos.x > camera.scaledPixelWidth / 2 / canvas.scaleFactor)
                        {
                            screenPos.x = camera.scaledPixelWidth / 2 / canvas.scaleFactor;
                            arrow_upward.y = 1f;
                            is_screen = false;
                        }
                        if(screenPos.y < -camera.scaledPixelHeight / 2 / canvas.scaleFactor || !character_monitor_obj.active)
                        {
                            screenPos.y = -camera.scaledPixelHeight / 2 / canvas.scaleFactor;
                            arrow_upward.x += 1f;
                            is_screen = false;
                        }
                        else if(screenPos.y > camera.scaledPixelHeight / 2 / canvas.scaleFactor)
                        {
                            screenPos.y = camera.scaledPixelHeight / 2 / canvas.scaleFactor;
                            arrow_upward.x -= 1f;
                            is_screen = false;
                        }
                        if (!character_monitor_obj.active)
                        {
                            screenPos.x *= -1;
                            arrow_upward.y *= -1;
                            is_screen = false;
                        }
                        character_arrow_obj.transform.localPosition = screenPos;
                        character_arrow_obj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, arrow_upward);
                        character_arrow_obj.SetActive(!is_screen);


                        character_monitor.name_label.text = _character.display_name;
                        character_monitor.hp_slider.value = (float)_character.hp / _character.max_hp;
                        character_monitor.distance_label.text = Vector3.Distance(character.gameObject.GetCenterPoint(), character_position).ToString("F2");
                        character_monitor.enemy_label.gameObject.SetActive(is_enemy);
                        character_monitor.friend_label.gameObject.SetActive(!is_enemy);
                        character_monitor.crosshair.color = (is_target) ? pointer_lockon_color : pointer_normal_color;

                        existing_character_objs.Add(_character.GetInstanceID());
                    }
                }
            }
            int[] character_monitor_keys = character_monitors.Keys.ToArray();
            foreach (int key in character_monitor_keys)
            {
                if (!existing_character_objs.Contains(key))
                {
                    GameObject gameObject = character_monitors[key];
                    character_monitors.Remove(key);
                    Destroy(gameObject);
                } 
            }
            int[] friend_monitor_keys = friend_monitors.Keys.ToArray();
            foreach (int key in friend_monitor_keys)
            {
                if (!existing_character_objs.Contains(key))
                {
                    GameObject gameObject = friend_monitors[key];
                    friend_monitors.Remove(key);
                    Destroy(gameObject);
                }
            }

            if (target)
            {
                pointer.color = pointer_lockon_color;
            }
            else {
                pointer.color = pointer_normal_color;
            }
        }

        
    }

    private void FixedUpdate()
    {
        

    }
}
