using ExtensionMethods;
using UnityEngine;

public class GunController : MonoBehaviour
{

    private enum GUN_STATE
    {
        READY,
        FIRE_START,
        FIRE,
        FIRE_END
    };

    public GameObject owner;
    [Header("秒間発数")]
    public float rate_of_fire = 7.0f;
    [Header("位置ブレ")]
    public float position_vel = 0.04f;
    [Header("角度ブレ")]
    public float rotation_vel = 4.0f;

    

    private GUN_STATE state = GUN_STATE.READY;
    private Transform gunpoint;
    private float next_fire_processed = 1.0f;
    private Vector3 vel_accumulated_position = Vector3.zero;
    private Quaternion vel_accumulated_rotation = Quaternion.identity;
    private Vector3 vel_position = Vector3.zero;
    private Quaternion vel_rotation = Quaternion.identity;
    private SatelliteGunController satelliteGunController;

    // Start is called before the first frame update
    void Start()
    {
        gunpoint = transform.Find("gunpoint");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (state == GUN_STATE.FIRE_START)
        {
            vel_accumulated_position = Vector3.zero;
            vel_accumulated_rotation = Quaternion.identity;
            state = GUN_STATE.FIRE;
        }
        else if (state == GUN_STATE.FIRE_END)
        {
            transform.Find("MuzzleFlash").gameObject.SetActive(false);
            transform.position -= vel_accumulated_position;
            transform.rotation = transform.rotation * Quaternion.Inverse(vel_accumulated_rotation);
            vel_position = Vector3.zero;
            vel_rotation = Quaternion.identity;
            state = GUN_STATE.READY;
        }
        else if (state == GUN_STATE.FIRE)
        {
            transform.Find("MuzzleFlash").gameObject.SetActive(true);
            if (next_fire_processed >= 1.0f)
            {
                next_fire_processed = 0.0f;
                if (satelliteGunController.current_load - satelliteGunController.use_load < 0)
                {
                    satelliteGunController.current_load = 0;
                }
                else
                {
                    satelliteGunController.current_load -= satelliteGunController.use_load;
                }
                GameObject bullet = (GameObject)Resources.Load("bullet");
                bullet.SetOwner(owner);
                Vector3 init_position = gunpoint.position;
                Quaternion ro = Quaternion.LookRotation(transform.forward);
                Instantiate(bullet, init_position, ro);
            }
        }
        next_fire_processed = Mathf.Min(next_fire_processed + rate_of_fire * Time.fixedDeltaTime, 1.0f);
    }
    public Vector3 getVelPosition() {
        return new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * position_vel / 2;
    }

    public Quaternion getVelRotation() {
        return Quaternion.Euler(new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * rotation_vel / 2);
    }

    public void fire() {
        
        if (state == GUN_STATE.READY)
        {
            state = GUN_STATE.FIRE_START;
        }
    }

    public void stop()
    {
        if (state == GUN_STATE.FIRE)
        {
            state = GUN_STATE.FIRE_END;
        }
    }

    public void SetSatelliteGunController(SatelliteGunController satelliteGunController) { 
        this.satelliteGunController = satelliteGunController;
    }

}
