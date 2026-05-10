using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

// =======================================================
// CharacterCameraController
// -------------------------------------------------------
// プレイヤー用HUD・ターゲットUI管理クラス
//
// HP、スタミナ、武器情報、敵味方マーカー、
// 画面外矢印、ロックオン表示などを更新する
// =======================================================

public class CharacterCameraController : MonoBehaviour
{
    public Camera camera;
    public Canvas canvas;

    // 味方ステータスUI
    public VerticalLayoutGroup status_group;

    // 武器UI
    public VerticalLayoutGroup weapon_group;

    // プレイヤーUI
    public Image hp_bar;
    public Image stamina_bar;

    // 中央ポインタ
    public RawImage pointer;

    // ロックオン補助表示
    public TextMeshProUGUI assist_off_label;
    public TextMeshProUGUI assist_on_label;

    // 入力ヒント
    public GameObject keyboard_hints;
    public GameObject procon_hits;

    // 現在ターゲット
    public GameObject target;

    // 通常時ポインタ色
    public Color pointer_normal_color =
        new Color(1f, 1f, 1f, 156f / 255f);

    // ロックオン時ポインタ色
    public Color pointer_lockon_color =
        new Color(1f, 0, 0, 156f / 255f);

    // キャラクターUI管理
    private Dictionary<int, GameObject> character_monitors;

    // 画面外矢印UI管理
    private Dictionary<int, GameObject> character_arrows;

    // 武器UI管理
    private Dictionary<int, GameObject> weapon_monitors;

    // 味方UI管理
    private Dictionary<int, GameObject> friend_monitors;

    void Start()
    {
        character_monitors = new Dictionary<int, GameObject>();

        character_arrows = new Dictionary<int, GameObject>();

        weapon_monitors = new Dictionary<int, GameObject>();

        friend_monitors = new Dictionary<int, GameObject>();
    }

    void Update()
    {
        CharacterModel character = GetComponent<CharacterModel>();

        if (character && camera && canvas)
        {
            // =======================================================
            // プレイヤーUI更新
            // =======================================================

            hp_bar.fillAmount =
                (float)character.hp / character.max_hp;

            stamina_bar.fillAmount =
                character.stamina / character.max_stamina;

            // =======================================================
            // 武器UI更新
            // =======================================================

            List<WeaponFeedback> weapons =
                character.GetWeaponFeedbacks();

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponFeedback weapon = weapons[i];

                // 武器UI生成
                if (!weapon_monitors.ContainsKey(weapon.GetHashCode()))
                {
                    GameObject _weapon_monitor =
                        Instantiate(
                            (GameObject)Resources.Load("weapon_monitor"),
                            weapon_group.transform
                        );

                    weapon_monitors.Add(
                        weapon.GetHashCode(),
                        _weapon_monitor
                    );
                }

                GameObject weapon_monitor_gameobject =
                    weapon_monitors[weapon.GetHashCode()];

                WeaponMonitor weapon_monitor =
                    weapon_monitor_gameobject.GetComponent<WeaponMonitor>();

                weapon_monitor.name_label.text =
                    weapon.name;

                weapon_monitor.load_label.text =
                    weapon.current_value + "/" + weapon.max_value;

                weapon_monitor.load_bar.fillAmount =
                    (float)weapon.current_value / weapon.max_value;
            }

            // 現在存在しているキャラクター一覧
            List<int> existing_character_objs =
                new List<int>();

            // =======================================================
            // 全キャラクターUI更新
            // =======================================================

            foreach (int team in BattleSequenceController.GetAllTeams())
            {
                foreach (CharacterModel _character in BattleSequenceController.GetCharacterModels(team))
                {
                    if (_character != character)
                    {
                        bool is_enemy =
                            (_character.team != character.team);

                        bool is_target =
                            (target == _character.gameObject);

                        // =======================================================
                        // キャラクターマーカー生成
                        // =======================================================

                        if (!character_monitors.ContainsKey(_character.GetInstanceID()))
                        {
                            GameObject _character_monitor_obj =
                                Instantiate(
                                    (GameObject)Resources.Load("character_monitor"),
                                    canvas.transform
                                );

                            character_monitors.Add(
                                _character.GetInstanceID(),
                                _character_monitor_obj
                            );

                            _character_monitor_obj.transform.SetAsFirstSibling();
                        }

                        // =======================================================
                        // 画面外矢印生成
                        // =======================================================

                        if (!character_arrows.ContainsKey(_character.GetInstanceID()))
                        {
                            GameObject _character_arrow_obj =
                                Instantiate(
                                    (GameObject)Resources.Load("character_arrow"),
                                    canvas.transform
                                );

                            character_arrows.Add(
                                _character.GetInstanceID(),
                                _character_arrow_obj
                            );

                            _character_arrow_obj.transform.SetAsFirstSibling();

                            _character_arrow_obj
                                .GetComponent<CharacterArrow>()
                                .SetColor(is_enemy);
                        }

                        // =======================================================
                        // 味方ステータスUI生成
                        // =======================================================

                        if (!is_enemy)
                        {
                            if (!friend_monitors.ContainsKey(_character.GetInstanceID()))
                            {
                                GameObject _friend_monitor_obj =
                                    Instantiate(
                                        (GameObject)Resources.Load("friend_monitor"),
                                        status_group.transform
                                    );

                                friend_monitors.Add(
                                    _character.GetInstanceID(),
                                    _friend_monitor_obj
                                );
                            }

                            GameObject friend_monitor_obj =
                                friend_monitors[_character.GetInstanceID()];

                            FriendMonitor friend_monitor =
                                friend_monitor_obj.GetComponent<FriendMonitor>();

                            friend_monitor.name_label.text =
                                _character.display_name;

                            friend_monitor.hp_bar.fillAmount =
                                (float)_character.hp / _character.max_hp;
                        }

                        GameObject character_monitor_obj =
                            character_monitors[_character.GetInstanceID()];

                        GameObject character_arrow_obj =
                            character_arrows[_character.GetInstanceID()];

                        CharacterMonitor character_monitor =
                            character_monitor_obj.GetComponent<CharacterMonitor>();

                        // =======================================================
                        // キャラクター位置計算
                        // =======================================================

                        Vector3 character_position =
                            _character.gameObject.GetCenterPoint();

                        Vector3 screenPos =
                            camera.WorldToScreenPoint(character_position);

                        character_monitor_obj.SetActive((screenPos.z > 0));

                        screenPos.x =
                            (screenPos.x - camera.scaledPixelWidth / 2) /
                            canvas.scaleFactor;

                        screenPos.y =
                            (screenPos.y - camera.scaledPixelHeight / 2) /
                            canvas.scaleFactor;

                        screenPos.z = 0;

                        character_monitor_obj.transform.localPosition =
                            screenPos;

                        character_monitor_obj.transform.rotation =
                            Quaternion.LookRotation(
                                (character_position - camera.transform.position).normalized
                            );

                        // =======================================================
                        // 画面外矢印処理
                        // =======================================================

                        bool is_screen = true;

                        Vector3 arrow_upward = Vector3.zero;

                        if (screenPos.x < -camera.scaledPixelWidth / 2 / canvas.scaleFactor)
                        {
                            screenPos.x =
                                -camera.scaledPixelWidth / 2 / canvas.scaleFactor;

                            arrow_upward.y = -1f;

                            is_screen = false;
                        }
                        else if (screenPos.x > camera.scaledPixelWidth / 2 / canvas.scaleFactor)
                        {
                            screenPos.x =
                                camera.scaledPixelWidth / 2 / canvas.scaleFactor;

                            arrow_upward.y = 1f;

                            is_screen = false;
                        }

                        if (screenPos.y < -camera.scaledPixelHeight / 2 / canvas.scaleFactor || !character_monitor_obj.active)
                        {
                            screenPos.y =
                                -camera.scaledPixelHeight / 2 / canvas.scaleFactor;

                            arrow_upward.x += 1f;

                            is_screen = false;
                        }
                        else if (screenPos.y > camera.scaledPixelHeight / 2 / canvas.scaleFactor)
                        {
                            screenPos.y =
                                camera.scaledPixelHeight / 2 / canvas.scaleFactor;

                            arrow_upward.x -= 1f;

                            is_screen = false;
                        }

                        // カメラ背面にいる場合は反転
                        if (!character_monitor_obj.active)
                        {
                            screenPos.x *= -1;

                            arrow_upward.y *= -1;

                            is_screen = false;
                        }

                        character_arrow_obj.transform.localPosition =
                            screenPos;

                        character_arrow_obj.transform.localRotation =
                            Quaternion.LookRotation(
                                Vector3.forward,
                                arrow_upward
                            );

                        character_arrow_obj.SetActive(!is_screen);

                        // =======================================================
                        // UI情報更新
                        // =======================================================

                        character_monitor.name_label.text =
                            _character.display_name;

                        character_monitor.hp_slider.value =
                            (float)_character.hp / _character.max_hp;

                        character_monitor.distance_label.text =
                            Vector3.Distance(
                                character.gameObject.GetCenterPoint(),
                                character_position
                            ).ToString("F2");

                        character_monitor.enemy_label.gameObject.SetActive(is_enemy);

                        character_monitor.friend_label.gameObject.SetActive(!is_enemy);

                        character_monitor.crosshair.color =
                            (is_target)
                            ? pointer_lockon_color
                            : pointer_normal_color;

                        existing_character_objs.Add(
                            _character.GetInstanceID()
                        );
                    }
                }
            }

            // =======================================================
            // 存在しなくなったキャラクターUI削除
            // =======================================================

            int[] character_monitor_keys =
                character_monitors.Keys.ToArray();

            foreach (int key in character_monitor_keys)
            {
                if (!existing_character_objs.Contains(key))
                {
                    GameObject gameObject =
                        character_monitors[key];

                    character_monitors.Remove(key);

                    Destroy(gameObject);
                }
            }

            // =======================================================
            // 存在しなくなった味方UI削除
            // =======================================================

            int[] friend_monitor_keys =
                friend_monitors.Keys.ToArray();

            foreach (int key in friend_monitor_keys)
            {
                if (!existing_character_objs.Contains(key))
                {
                    GameObject gameObject =
                        friend_monitors[key];

                    friend_monitors.Remove(key);

                    Destroy(gameObject);
                }
            }

            // =======================================================
            // 中央ポインタ色変更
            // =======================================================

            if (target)
            {
                pointer.color = pointer_lockon_color;
            }
            else
            {
                pointer.color = pointer_normal_color;
            }
        }
    }

    private void FixedUpdate()
    {

    }
}