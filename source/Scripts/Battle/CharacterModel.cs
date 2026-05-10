using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;


public class WeaponFeedback {
    public string name;
    public string icon;
    public int max_value;
    public int current_value;
}

abstract public class CharacterModel : MonoBehaviour
{
    [Header("チーム（味方か敵かを判別）")]
    public int team;
    public string display_name = "display_name";
    [Header("最大HP")]
    public int max_hp = 200;
    [Header("最大スタミナ")]
    public float max_stamina = 100.0f;
    [Header("スタミナ回復秒速")]
    public float stamina_recover_speed = 10.0f;
    [Header("最高速度(距離/秒)")]
    public float max_speed = 10.0f;
    [Header("最高速度到達時間(秒)")]
    public float time_to_peak = 0.4f;
    [Header("ステップ距離")]
    public float step_distance = 5.0f;
    [Header("ステップ動作時間(秒)")]
    public float step_action_time = 0.3f;
    [Header("ステップ消耗スタミナ")]
    public float step_use_stamina = 25.0f;
    [Header("飛行消耗スタミナ(秒)")]
    public float fly_use_stamina = 25.0f;
    [Header("回転速度(角度/秒)")]
    public float max_rotate_speed = 180.0f;
    [Header("前進開始角度")]
    public float start_move_angle = 30.0f;
    [Header("ジャンプ動作時間(秒)")]
    public float jump_action_time = 0.3f;
    [Header("着地必要速度")]
    public float need_land_action_velocity = 1.0f;
    [Header("着地動作時間(秒)")]
    public float land_action_time = 0.3f;
    [Header("死亡動作時間(秒)")]
    public float destory_action_time = 1.0f;

    public List<string> audio_clip_names;
    public List<AudioClip> audio_clips;
    public AudioSource audio_source;

    public int hp;
    public float stamina;
    public Vector3 look_at = Vector3.zero;

    protected Rigidbody rb;
    protected List<WeaponFeedback> weapon_feedbacks;
    protected Animator animator;
    protected GameObject small_explosion;
    protected Collider collider;

    protected Vector3 move_direction = Vector3.zero;
    protected Vector3 step_direction = Vector3.zero;
    protected bool move_step = false;
    protected bool move_jump = false;
    protected bool move_fly = false;
    protected bool on_land = false;

    protected float jump_progress = 1.0f;
    protected float land_progress = 1.0f;
    protected float step_progress = 1.0f;
    protected bool lock_action = false;
    protected bool destroying = false;
    protected float destory_progress = 1.0f;

    // Start is called before the first frame update
    virtual public void Start()
    {
        weapon_feedbacks = new List<WeaponFeedback>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider>();

        hp = max_hp;
        stamina = max_stamina;

        BattleSequenceController.RegisterCharacter(this);
    }

    virtual public void Update()
    {
        
        if (hp <= 0 && !destroying) {
            destroying = true;
            destory_progress = 0.0f;
            BattleSequenceController.DeregisterCharacter(this);
            small_explosion = Instantiate((GameObject)Resources.Load("SmallExplosion"), gameObject.GetCenterPoint(), transform.rotation);
        }
    }

    virtual public void FixedUpdate()
    {
        BeforeUpdate();
        AfterUpdate();
    }

    virtual protected void BeforeUpdate()
    {
        if (destroying)
        {
            if (destory_progress >= 1.0f)
            {

                Instantiate((GameObject)Resources.Load("BigExplosion"), gameObject.GetCenterPoint(), transform.rotation);

                Destroy(this.gameObject);
                if (small_explosion)
                {
                    Destroy(small_explosion);
                }
            }
            lock_action = true;
            lock_action = false;
            move_jump = false;
            move_step = false;
            move_fly = false;
            jump_progress = 1.0f;
            destory_progress += Time.fixedDeltaTime / destory_action_time;
        }
        on_land = false;
        if (collider)
        {
            Vector3 foot_point = collider.bounds.center;
            foot_point.y -= collider.bounds.extents.y;
            int layer = 1 << LayerMask.NameToLayer("static");
            foreach (Collider collider in Physics.OverlapBox(foot_point, new Vector3(collider.bounds.extents.x, 0.1f, collider.bounds.extents.z), transform.rotation, layer))
            {
                on_land = true;
                break;
            }
        }

        Vector3 direction = move_direction;

        if (jump_progress < 1.0f)
        {
            lock_action = true;
            jump_progress += Time.fixedDeltaTime / jump_action_time;
        }
        if (step_progress < 1.0f)
        {
            lock_action = true;
            rb.useGravity = false;
            float t = step_progress.EaseInOutQuad();
            Quaternion rotation = Quaternion.LookRotation(step_direction.normalized);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, t);
            Vector3 max_velocity = step_direction.normalized * (step_distance / step_action_time) * t;
            Vector3 add_velocity = (max_velocity - rb.velocity);
            rb.velocity += add_velocity;

            step_progress += Time.fixedDeltaTime / step_action_time;
            jump_progress = 1.0f;
        }
        else
        {
            rb.useGravity = true;
        }
        if (on_land && land_progress < 1.0f)
        {
            lock_action = true;
            land_progress += Time.fixedDeltaTime / land_action_time;
        }

        if ((!lock_action || jump_progress < 1.0f) && move_step && stamina > step_use_stamina)
        {
            stamina = Mathf.Max(stamina - step_use_stamina, 0.0f);
            step_progress = 0.0f;
            step_direction = direction;
        }

        if (!lock_action)
        {
            if (move_jump && stamina > 0.0f)
            {
                jump_progress = 0.0f;
            }

            // 移動する方法体を向ける（上下は除外）
            if (direction != Vector3.zero)
            {
                float rotate_angle = Vector3.Angle(transform.forward, direction * 2.0f);
                float delta_rotate_speed = max_rotate_speed * Time.fixedDeltaTime;
                float t = 1.0f;
                if (delta_rotate_speed < rotate_angle)
                {
                    t = delta_rotate_speed / rotate_angle;
                }
                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, t);

                // 体の回転角度が前進開始角度以上の時移動させない
                if (rotate_angle > start_move_angle)
                {
                    direction.x = 0.0f;
                    direction.z = 0.0f;
                }
            }

            if (move_fly && stamina > 0.0f)
            {
                float fly_use_stamina_amount = fly_use_stamina * Time.fixedDeltaTime;
                if (stamina - fly_use_stamina_amount < 0)
                {
                    stamina = 0;
                }
                else
                {
                    stamina -= fly_use_stamina_amount;
                }
                rb.useGravity = false;
                direction.y = 1.0f;
            }
            else
            {
                rb.useGravity = true;
            }

            // 一定速度で移動
            Vector3 max_velocity = direction.normalized * max_speed;
            Vector3 add_velocity = (max_velocity - rb.velocity) * Time.fixedDeltaTime / time_to_peak;
            rb.velocity += add_velocity;
        }

        if (!on_land && rb.velocity.y < -need_land_action_velocity)
        {
            land_progress = 0.0f;
        }
        if (on_land)
        {
            float stamina_recover_amount = stamina_recover_speed * Time.fixedDeltaTime;
            if (stamina + stamina_recover_amount > max_stamina)
            {
                stamina = max_stamina;
            }
            else
            {
                stamina += stamina_recover_amount;
            }
        }
    }

    virtual protected void AfterUpdate()
    {
        if(animator != null)
        {
            animator.SetFloat("jump_action_time", jump_action_time);
            animator.SetFloat("land_action_time", land_action_time);
            animator.SetBool("move", (move_direction != Vector3.zero));
            animator.SetFloat("jump_progress", jump_progress);
            animator.SetFloat("velocity_v", rb.velocity.y);
            animator.SetFloat("land_progress", land_progress);
            animator.SetFloat("step_progress", step_progress);
            animator.SetBool("on_land", on_land);
        }
        

        lock_action = false;
        move_jump = false;
        move_step = false;
        move_fly = false;
    }

    virtual public void SetMoveDirection(Vector3 move_direction) {
        this.move_direction = move_direction.normalized;
        this.move_direction.y = 0;
    }

    virtual public void Step() {
        move_step = true;
    }

    virtual public void Jump()
    {
        move_jump = true;
    }

    virtual public void Moveflying()
    {
        move_fly = true;
    }

    virtual public void GotHit(int power, bool sameTeam)
    {
        if (!sameTeam)
        {
            hp = Mathf.Max(hp - power, 0);
        }
    }

    virtual public void Win()
    {
        animator.SetTrigger("win");
    }

    public List<WeaponFeedback> GetWeaponFeedbacks() {
        return weapon_feedbacks;
    }

    abstract public void Attack(string id, Vector3 aiming_to, GameObject target);

}

