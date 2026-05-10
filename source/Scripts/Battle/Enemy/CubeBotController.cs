using ExtensionMethods;
using UnityEngine;

public enum CUBE_BOT_PATTREN_TYPE
{
    ATTACK_PLAYER,
    ATTACK_RADAR,
    ATTACK_NEAR,
    DEFENSE_AREA
}

public class CubeBotController : MonoBehaviour
{
    public CharacterModel self_character;
    public float attack_start_distance = 60.0f;
    public Vector3 defense_position = Vector3.zero;
    public float defense_moving_range_min = 5.0f;
    public float defense_moving_range_max = 30.0f;
    public float attack_moving_range_min = 5.0f;
    public float attack_moving_range_max = 80.0f;
    public CUBE_BOT_PATTREN_TYPE pattren_type = CUBE_BOT_PATTREN_TYPE.ATTACK_NEAR;
    public float positioning_limit_time = 20.0f;
    private Collider collider;
    private Vector3 destination = Vector3.zero;
    private GameObject target;
    private bool find_next_destination = true;

    private float last_positioning = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        self_character = GetComponent<CharacterModel>();
        collider = GetComponent<Collider>();

    }

    // Update is called once per frame
    void Update()
    {
        
        CharacterModel[] enemies = BattleSequenceController.GetEnemies(self_character.team);
        if (enemies.Length > 0)
        {
            Vector3 self_position = gameObject.GetCenterPoint();
            CharacterDistanceComparer distance_comparer = new CharacterDistanceComparer();
            distance_comparer.origin = self_position;
            System.Array.Sort(enemies, distance_comparer);

            if (Vector3.Distance(self_position, destination) < collider.bounds.size.z || last_positioning > positioning_limit_time)
            {
                find_next_destination = true;
            }

            if (pattren_type == CUBE_BOT_PATTREN_TYPE.ATTACK_NEAR)
            {
                target = enemies[0].gameObject;
            }
            else if (pattren_type == CUBE_BOT_PATTREN_TYPE.ATTACK_PLAYER)
            {
                if (BattleSequenceController.playerController.player_character)
                {
                    target = BattleSequenceController.playerController.player_character.gameObject;
                }
            }
            else if (pattren_type == CUBE_BOT_PATTREN_TYPE.ATTACK_RADAR)
            {
                target = enemies[0].gameObject;
                foreach (CharacterModel enemy in enemies)
                {
                    if (enemy is RadarDish)
                    {
                        target = enemy.gameObject;
                        break;
                    }
                }
            }
            else if (pattren_type == CUBE_BOT_PATTREN_TYPE.DEFENSE_AREA)
            {
                target = enemies[0].gameObject;
            }
            if (target)
            {
                Vector3 target_position = target.GetCenterPoint();
                if (find_next_destination)
                {
                    int layer = 1 << LayerMask.NameToLayer("static");
                    layer = layer | (1 << LayerMask.NameToLayer("wall"));
                    if (pattren_type == CUBE_BOT_PATTREN_TYPE.DEFENSE_AREA)
                    {
                        destination = ReflectStatic(self_position, defense_position + Random.onUnitSphere * Random.Range(defense_moving_range_min, defense_moving_range_max), layer);

                    }
                    else
                    {
                        destination = ReflectStatic(self_position, target_position + Random.onUnitSphere * Random.Range(attack_moving_range_min, attack_moving_range_max), layer);
                    }
                    find_next_destination = false;
                    last_positioning += Time.deltaTime;
                }

                if (BattleSequenceController.GetSceneState() == SCENE_STATE.PLAYING)
                {
                    if (self_character.GetWeaponFeedbacks()[0].current_value > 1.0f && Vector3.Distance(self_position, target_position) < attack_start_distance)
                    {
                        self_character.Attack("main", target_position, target);
                    }
                }
            }
            

            self_character.SetMoveDirection((destination - self_position).normalized);
        }


    }

    private Vector3 ReflectStatic(Vector3 origin, Vector3 destination, int layer, int try_limit = 30)
    {
        if(try_limit > 0)
        {
            foreach (RaycastHit hit in Physics.RaycastAll(origin, (destination - origin).normalized, Vector3.Distance(origin, destination), layer))
            {

                destination = hit.point + Vector3.Reflect(destination - hit.point, hit.normal);
                destination = ReflectStatic(origin, destination, layer, try_limit - 1);
                break;
            }
        }
        return destination;
    }

}
