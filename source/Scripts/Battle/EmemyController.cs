using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;

public class EmemyController : MonoBehaviour
{
    CharacterModel self_character;

    private Vector3 p0;
    private Vector3 p1;
    private Vector3 p2;
    private Vector3 p3;
    private float t = 2.0f;
    private float distance = 0.0f;
    private float height = 0.0f;

    private Collider collider;
    private LineRenderer lineRenderer;

    private bool can_main_attack = true;
    private ACTION_PATTERN action_pattern = ACTION_PATTERN.POSITION;
    private enum ACTION_PATTERN
    {
        POSITION,
        WANDERING,
        ATTACK
    }

    // Start is called before the first frame update
    void Start()
    {
        self_character = GetComponent<CharacterModel>();
        lineRenderer = GetComponent<LineRenderer>();
        collider = GetComponent<Collider>();

        p0 = transform.position;
        p1 = transform.position;
        p2 = transform.position;
        p3 = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        CharacterModel player = BattleSequenceController.getPlayerCharacter();
        if (player)
        {

            if (action_pattern == ACTION_PATTERN.POSITION)
            {
                float outer_distance = Random.Range(5f, 50f);
                p0 = player.transform.position + Random.onUnitSphere * outer_distance;
                Vector3 p1 = gameObject.GetCenterPoint();
                Vector3 p2 = p0;
                List<Vector3> positions = new List<Vector3>();
                positions.Add(p1);

                LayerMask layerMask = 1 << LayerMask.NameToLayer("ground");
                bool hitted = false;
                do
                {
                    //hitted = false;
                    //RaycastHit hit;
                    //if(Physics.Raycast(p1, (p2 - p1).normalized, out hit, Vector3.Distance(p1, p2), layerMask))
                    //{
                    //    positions.Add(hit.point);
                    //    p1 = hit.point;
                    //    p2 = hit.point + Vector3.Reflect(p2 - hit.point, hit.normal);
                    //    hitted = true;
                    //}
                    hitted = false;
                    foreach (RaycastHit hit in Physics.RaycastAll(p1, (p2 - p1).normalized, Vector3.Distance(p1, p2)))
                    {
                        if (hit.collider.gameObject.isStatic)
                        {

                            positions.Add(hit.point);
                            p1 = hit.point;
                            p2 = hit.point + Vector3.Reflect(p2 - hit.point, hit.normal);
                            hitted = true;
                            break;
                        }
                    }
                } while (hitted);
                p0 = p2;

                positions.Add(p2);
                //lineRenderer.positionCount = positions.Count;
                //lineRenderer.SetPositions(positions.ToArray());

                //p0.y = (p0.y < height / 2) ? height / 2 : p0.y;
                action_pattern = ACTION_PATTERN.WANDERING;
            }

            if (can_main_attack)
            {
                float distance = Vector3.Distance(player.gameObject.GetCenterPoint(), gameObject.GetCenterPoint());
                if (distance < 30.0f)
                {
                    foreach (WeaponFeedback weaponFeedback in self_character.GetWeaponFeedbacks())
                    {
                        if (weaponFeedback.current_value > 1.0f)
                        {
                            action_pattern = ACTION_PATTERN.ATTACK;
                        }

                    }

                }
            }

            
            if (action_pattern == ACTION_PATTERN.WANDERING)
            {
                self_character.SetMoveDirection((p0 - gameObject.GetCenterPoint()).normalized);
                if(Vector3.Distance(gameObject.GetCenterPoint(), p0) < collider.bounds.size.z)
                {
                    can_main_attack = true;
                    action_pattern = ACTION_PATTERN.POSITION;
                }
            }
            else if(action_pattern == ACTION_PATTERN.ATTACK)
            {
                self_character.Attack("main", player.gameObject.GetCenterPoint(), player.gameObject);
                action_pattern = ACTION_PATTERN.POSITION;
                foreach (WeaponFeedback weaponFeedback in self_character.GetWeaponFeedbacks())
                {
                    if (weaponFeedback.current_value < 1.0f)
                    {
                        can_main_attack = false;
                    }

                }
                
            }

            
            //main_attack = false;
            //float distance = Vector3.Distance(player.gameObject.GetCenterPoint(), gameObject.GetCenterPoint());

            //if (distance < 30.0f)
            //{
            //    foreach (WeaponFeedback weaponFeedback in self_character.GetWeaponFeedbacks())
            //    {
            //        if (weaponFeedback.current_value > 1.0f)
            //        {
            //            main_attack = true;
            //        }

            //        if (main_attack)
            //        {
            //            self_character.Attack("main", player.gameObject.GetCenterPoint(), player.gameObject);
            //        }

            //    }

            //}
            if (false)
            {

                //if(Vector3.Distance(gameObject.GetCenterPoint(), p0) < 3.0f)
                //{
                //    float inner_distance = Random.Range(1f, 5f);
                //    float outer_distance = Random.Range(5f, 20f);

                //    p0 = player.transform.position + Random.onUnitSphere * outer_distance;
                //    p0.y = (p0.y < height / 2) ? height / 2 : p0.y;
                //}

                
                //self_character.SetMoveDirection((p0 - gameObject.GetCenterPoint()).normalized);




                //if (t > 1.0f)
                //{
                //    t = 0.0f;
                //    distance = 0.0f;
                //    height = this.GetComponent<Collider>().bounds.size.y;

                //    // “Z‚í‚è‚Â‚­
                //    p0 = transform.position;
                //    p1 = p0 - p2;

                //    float inner_distance = Random.Range(1f, 5f);
                //    float outer_distance = Random.Range(5f, 10f);

                //    float p2_d = inner_distance + ((p1.y < 0) ? -p1.y : 0f);
                //    p2 = player.transform.position + Random.onUnitSphere * p2_d;
                //    p2.y = (p2.y < 0) ? -p2.y : p2.y;
                //    p3 = player.transform.position + Random.insideUnitSphere * outer_distance;
                //    p3.y = (p3.y < 0) ? -p3.y : p3.y;

                //    // transform.position + Random.onUnitSphere * move_speed * 20f;

                //    float accuracy = 1.0f / 10.1f;
                //    Vector3 prevous_p = p0;
                //    for (float t = accuracy; t <= 1.0f; t += accuracy)
                //    {
                //        Vector3 cur_p = bezier3(p0, p1, p2, p3, t);
                //        distance += Vector3.Distance(prevous_p, cur_p);
                //        prevous_p = cur_p;
                //    }
                //}
                //Vector3 position = bezier3(p0, p1, p2, p3, t);
                //position.y = (position.y < height / 2) ? height / 2 : position.y;

                //self_character.SetMoveDirection((position - gameObject.GetCenterPoint()).normalized);
                ////transform.position = position;

                //t += Time.deltaTime / (distance / self_character.max_speed);
            }

        }
        

        /*
        CharacterModel player = SceneController.getPlayerCharacter();
        if (player)
        {
            
        }
        */

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.isStatic)
        {
            //action_pattern = ACTION_PATTERN.POSITION;
        }
    }

    public Vector3 bezier3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        float f0 = Mathf.Pow(1 - t, 3);
        float f1 = 3 * Mathf.Pow(1 - t, 2) * t;
        float f2 = 3 * (1 - t) * Mathf.Pow(t, 2);
        float f3 = Mathf.Pow(t, 3);

        Vector3 result =
            p0 * f0 +
            p1 * f1 +
            p2 * f2 +
            p3 * f3;
        return result;
    }
}
