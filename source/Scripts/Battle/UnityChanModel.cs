using ExtensionMethods;
using UnityEngine;

public class UnityChanModel : CharacterModel
{

    public float missile_max_load = 10.0f;
    public float missile_fire_rate = 5.0f;
    public float missile_recover_rate = 1.0f;
    public string missile_name = "Missile";

    private SatelliteGunController satelliteGun;
    private bool moving_attack_main = false;
    private bool moving_attack_sub = false;
    private bool main_attack_audio_played = false;
    private bool sub_attack_audio_played = false;
    private Vector3 aiming_to;
    private GameObject target;
    private float missile_current_load = 0.0f;
    private float missile_cooldown_progress = 1.0f;
    private WeaponFeedback missile_feedback;

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        satelliteGun = GetComponent<SatelliteGunController>();
        satelliteGun.owner = this.gameObject;
        if (satelliteGun.weapon_feedback != null)
        {
            weapon_feedbacks.Add(satelliteGun.weapon_feedback);
        }

        missile_current_load = missile_max_load;
        missile_feedback = new WeaponFeedback();
        missile_feedback.name = missile_name;
        missile_feedback.max_value = Mathf.FloorToInt(missile_max_load);
        missile_feedback.current_value = Mathf.FloorToInt(missile_current_load);
        weapon_feedbacks.Add(missile_feedback);
    }
    public override void Update()
    {
        base.Update();
        if (!weapon_feedbacks.Contains(satelliteGun.weapon_feedback))
        {
            weapon_feedbacks.Add(satelliteGun.weapon_feedback);
        }
    }

    override protected void BeforeUpdate()
    {

        if (destroying && destory_progress == 0.0f)
        {
            audio_source.PlayOneShot(audio_clips[12]);
        }
        base.BeforeUpdate();
        
        if (destroying)
        {
            moving_attack_main = false;
            moving_attack_sub = false;
        }
        if (moving_attack_main)
        {
            if (!main_attack_audio_played && satelliteGun.current_load == 0)
            {
                audio_source.PlayOneShot(audio_clips[9]);
                main_attack_audio_played = true;
            }
            satelliteGun.fire(aiming_to, target);
        }
        else
        {
            main_attack_audio_played = false;
            satelliteGun.stop();
        }

        if (moving_attack_sub)
        {
            if (target && missile_cooldown_progress >= 1.0f && missile_current_load >= 1.0f)
            {
                GameObject missile = (GameObject)Resources.Load("missile");
                missile.GetComponent<MissileScript>().target = target;
                missile.GetComponent<MissileScript>().owner = gameObject;
                missile.gameObject.SetOwner(gameObject);
                Vector3 rand = transform.TransformDirection(new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(1.0f, 2.0f), 0));
                Vector3 init_position = gameObject.GetCenterPoint() + rand;
                Instantiate(missile, init_position, Quaternion.LookRotation(rand.normalized));
                missile_cooldown_progress = 0.0f;
                missile_current_load -= 1.0f;

                if (!sub_attack_audio_played)
                {
                    audio_source.PlayOneShot(audio_clips[Random.Range(0, 5)]);
                    sub_attack_audio_played = true;
                }

            }
            
        }
        else
        {
            sub_attack_audio_played = false;
        }
    }

    protected override void AfterUpdate()
    {
        base.AfterUpdate();
        if (audio_source && !audio_source.isPlaying)
        {
            audio_source.clip = null;
        }
        moving_attack_main = false;
        moving_attack_sub = false;

        missile_current_load = Mathf.Min(missile_current_load + missile_recover_rate * Time.fixedDeltaTime, missile_max_load);
        missile_cooldown_progress = Mathf.Min(missile_cooldown_progress + missile_fire_rate * Time.fixedDeltaTime, 1.0f);
        missile_feedback.current_value = Mathf.FloorToInt(missile_current_load);
    }

    public override void Win()
    {
        foreach(GunController gun in satelliteGun.guns)
        {
            gun.gameObject.SetActive(false);
        }
        audio_source.PlayOneShot(audio_clips[11]);
        base.Win();
    }

    override public void Attack(string id, Vector3 aiming_to, GameObject target)
    {

        if (id == "main")
        {
            moving_attack_main = true;
            this.aiming_to = aiming_to;
            this.target = target;
        }
        else if(id == "sub")
        {
            moving_attack_sub = true;
            this.aiming_to = aiming_to;
            this.target = target;
        }
    }

    public override void GotHit(int power, bool sameTeam)
    {
        base.GotHit(power, sameTeam);
        audio_source.PlayOneShot(audio_clips[Random.Range(5, 9)]);
    }
}
