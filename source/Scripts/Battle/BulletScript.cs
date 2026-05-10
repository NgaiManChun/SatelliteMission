using ExtensionMethods;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float speed = 10.0f;
    public float effective_distance = 100.0f;
    public int power = 1;
    public GameObject owner;

    private Rigidbody rigidbody;
    private bool shooted = false;
    private float distance_count = 0.0f;
    private Vector3 prevous_position = Vector3.zero;
    private bool hitted = false;
    private Vector3 hitted_position = Vector3.zero;


    // public Quaternion 

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        prevous_position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        

    }

    private void FixedUpdate()
    {
        if (hitted)
        {
            rigidbody.position = hitted_position;
            Destroy(this.gameObject);
        }

        if (!shooted && !hitted)
        {
            rigidbody.velocity = transform.forward * speed;
            shooted = true;
        }


        if (distance_count >= effective_distance)
        {
            Destroy(this.gameObject);
        }

        distance_count += Vector3.Distance(prevous_position, transform.position);
        prevous_position = transform.position;

    }

    private void handleHit(Vector3 position, GameObject gameObject) {
        GameObject HitEffect = (GameObject)Resources.Load("HitEffect");
        Instantiate(HitEffect, position, Quaternion.Inverse(transform.rotation));

        

        CharacterModel char_model = gameObject.GetComponent<CharacterModel>();
        if (!char_model && gameObject.transform.root != null)
        {
            char_model = gameObject.transform.root.GetComponent<CharacterModel>();
        }
        
        if (char_model) {
            bool sameTeam = (char_model.team == owner.GetTeam());
            char_model.GotHit(power, sameTeam);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        bool isOwner = (other.gameObject == owner);
        bool sameOwner = (other.gameObject.GetOwner() == owner);
        

        if (!isOwner && !sameOwner) {
            hitted = true;
            hitted_position = other.ClosestPointOnBounds(transform.position);
            handleHit(hitted_position, other.gameObject);
        }
        
    }

}
