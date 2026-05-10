using ExtensionMethods;
using UnityEngine;


public class Satellite_Gun_Individual
{
    public GunController gun;
    public Vector3 fire_offset;
    public Vector3 return_start_position;
    public Quaternion return_start_rotation;
    public SatelliteController satelliteController;
}

public class SatelliteGunController : WeaponController
{
    private enum GUN_STATE
    {
        INIT,
        READY,
        FIRE_START,
        FIRE,
        FIRE_END
    };
    public string name = "Satellite Guns";
    [Header("òAåãGameobject")]
    public GunController[] guns;
    [Header("î≠éÀéûÇÃèeÇÃà íu")]
    public Vector3[] fire_offsets;
    [Header("ç≈ëÂécíeêî")]
    public int max_load_value = 120;
    [Header("écíeâÒïúïbë¨")]
    public int load_speed = 4;
    [Header("ç≈ëÂòAéÀêî")]
    public int max_continued_fire = 21;
    [Header("àÍî≠è¡ñ’íeêî")]
    public int use_load = 1;

    public GameObject owner;
    
    public float return_pos_time = 0.2f;
    public float startup_time = 1.0f;
    private float startup_count = 0.0f;
    public float current_load = 0;
    public WeaponFeedback weapon_feedback;

    public Satellite_Gun_Individual[] gun_inds = new Satellite_Gun_Individual[2];

    private GUN_STATE state = GUN_STATE.INIT;
    private float return_t = 0.0f;
    private Vector3 aiming_to = Vector3.zero;
    private GameObject target;
    
    // Start is called before the first frame update
    void Start()
    {

        gun_inds = new Satellite_Gun_Individual[guns.Length];

        for (int i = 0; i < guns.Length; i++) {
            gun_inds[i] = new Satellite_Gun_Individual();
            gun_inds[i].gun = guns[i];
            gun_inds[i].fire_offset = fire_offsets[i];
            gun_inds[i].return_start_position = guns[i].transform.position;
            gun_inds[i].return_start_rotation = guns[i].transform.rotation;
            SatelliteController satelliteController = guns[i].GetComponent<SatelliteController>();
            if (satelliteController)
            {
                satelliteController.target = this.gameObject;
                satelliteController.start();
                gun_inds[i].satelliteController = satelliteController;
            }
            guns[i].SetSatelliteGunController(this);
            guns[i].transform.parent = null;
            guns[i].owner = this.gameObject;
        }

        current_load = max_load_value;

        weapon_feedback = new WeaponFeedback();
        weapon_feedback.name = name; //"Cthugha/Ithaqua";
        weapon_feedback.max_value = max_load_value;
        weapon_feedback.current_value = Mathf.FloorToInt(current_load);


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        weapon_feedback.current_value = Mathf.FloorToInt(current_load);

        if (state == GUN_STATE.INIT)
        {
            foreach (Satellite_Gun_Individual gun_ind in gun_inds)
            {
                gun_ind.gun.gameObject.GetComponent<Renderer>().material.color = Color.Lerp(Color.clear, Color.white, startup_count / startup_time);
                gun_ind.gun.transform.parent = null;
            }
            if (startup_count > startup_time)
            {
                state = GUN_STATE.READY;
            }
            startup_count += Time.fixedDeltaTime;
        }
        if (state == GUN_STATE.FIRE_START)
        {
            return_t = 0.0f;
            foreach (Satellite_Gun_Individual gun_ind in gun_inds)
            {
                if (gun_ind.satelliteController)
                    gun_ind.satelliteController.stop();
                gun_ind.return_start_position = gun_ind.gun.transform.position;
                gun_ind.return_start_rotation = gun_ind.gun.transform.rotation;
            }
            state = GUN_STATE.FIRE;
        }
        else if (state == GUN_STATE.FIRE_END)
        {
            return_t = 0.0f;
            foreach (Satellite_Gun_Individual gun_ind in gun_inds)
            {
                gun_ind.gun.stop();
                SatelliteController satelliteController = gun_ind.gun.GetComponent<SatelliteController>();
                if (gun_ind.satelliteController)
                    gun_ind.satelliteController.start();

            }
            state = GUN_STATE.READY;
        }
        else if (state == GUN_STATE.FIRE)
        {
            if (return_t >= 1f)
            {
                foreach (Satellite_Gun_Individual gun_ind in gun_inds)
                {
                    
                    gun_ind.gun.transform.position = transform.position + Quaternion.LookRotation(aiming_to - transform.position) * gun_ind.fire_offset;
                    gun_ind.gun.transform.LookAt(aiming_to);
                    gun_ind.gun.transform.position += gun_ind.gun.getVelPosition();
                    gun_ind.gun.transform.rotation *= gun_ind.gun.getVelRotation();
                    if (current_load >= use_load)
                    {
                        gun_ind.gun.fire();
                    }
                    else {
                        gun_ind.gun.stop();
                    }
                }
            }
            else
            {
                foreach (Satellite_Gun_Individual gun_ind in gun_inds)
                {
                    Vector3 final_position = transform.position + Quaternion.LookRotation(aiming_to - transform.position) * gun_ind.fire_offset;
                    Quaternion final_rotation = Quaternion.FromToRotation(final_position, aiming_to);
                    float t = return_t.EaseInOutQuad();
                    gun_ind.gun.transform.position = Vector3.Slerp(gun_ind.return_start_position, final_position, t);
                    gun_ind.gun.transform.rotation = Quaternion.Slerp(gun_ind.return_start_rotation, final_rotation, t);
                }
            }

        }

        if (state == GUN_STATE.READY) {
            float load_amount = load_speed * Time.fixedDeltaTime;
            if (current_load + load_amount > max_load_value)
            {
                current_load = max_load_value;
            }
            else
            {
                current_load += load_amount;
            }
        }

        return_t += Time.fixedDeltaTime / return_pos_time;
    }

    override public void fire(Vector3 aiming_to, GameObject target)
    {
        this.aiming_to = aiming_to;
        this.target = target;
        if (state == GUN_STATE.READY) {
            state = GUN_STATE.FIRE_START;
        }
    }

    override public void stop()
    {
        this.aiming_to = Vector3.zero;
        this.target = null;
        if (state == GUN_STATE.FIRE)
        {
            state = GUN_STATE.FIRE_END;
        }
    }

    private void OnDestroy()
    {
        foreach (GunController gun in guns)
        {
            if (gun != null && gun.gameObject != null) {
                Destroy(gun.gameObject);
            }
        }
    }

}
