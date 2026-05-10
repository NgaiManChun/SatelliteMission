using ExtensionMethods;
using UnityEngine;

public class SatelliteController : MonoBehaviour
{

    public GameObject target;

    public Vector3 position_offset = Vector3.zero;
    public float cycle_period = 1.0f;
    public float radius = 2.0f;
    public float rotate_period = 1.0f;
    public float time_offset = 0.0f;
    public float return_pos_time = 0.5f;
    public bool active = false;

    private float time_count = 0.0f;
    private float return_t = 0.0f;
    private Vector3 return_start_position;
    private Quaternion return_start_rotation;
    private Renderer render;

    // Start is called before the first frame update
    void Start()
    {
        render = GetComponentInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (active && target)
        {
            Vector3 center = target.GetCenterPoint() + position_offset;
            /*
            if(target.GetComponentInChildren<Renderer>())
            {
                Bounds bounds = target.GetComponentInChildren<Renderer>().bounds;
                center = bounds.center;
            }
            */

            float cycle_t = ((time_count + time_offset) % cycle_period) / cycle_period;
            float x = radius * Mathf.Cos(Mathf.PI * cycle_t * 360 / 180);
            float y = 0;
            float z = radius * Mathf.Sin(Mathf.PI * cycle_t * 360 / 180);

            float rotate_t = ((time_count + time_offset) % rotate_period) / rotate_period;
            float angle = 360.0f * rotate_t;

            Vector3 local_pos = Quaternion.Euler(0, 0, angle) * new Vector3(x, y, z);
            Vector3 world_pos = center + local_pos;

            if (return_t < 1.0f)
            {
                world_pos = Vector3.Lerp(transform.position, world_pos, return_t);

                return_t += Time.fixedDeltaTime / return_pos_time;
            }


            // world_pos.y = (world_pos.y < 0.5f) ? 0.5f : world_pos.y;

            transform.LookAt(world_pos, transform.up);
            transform.position = world_pos;
            
        }

        time_count += Time.fixedDeltaTime;
    }

    public void start() {
        return_t = 0.0f;
        return_start_position = transform.position;
        return_start_rotation = transform.rotation;
        active = true;
    }

    public void stop() {
        active = false;
    }
}
