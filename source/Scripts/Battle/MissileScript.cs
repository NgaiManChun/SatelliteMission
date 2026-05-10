using ExtensionMethods;
using System.Collections.Generic;
using UnityEngine;

public class MissileScript : MonoBehaviour
{
    public float speed = 10.0f;
    public float effective_distance = 100.0f;
    public int power = 1;
    public float vel = 0.5f;
    public float adjust_load_time = 0.15f;
    public float explosion_raduis = 1.0f;
    public GameObject owner;
    public GameObject target;

    private Rigidbody rigidbody;
    private bool shooted = false;
    private float distance_count = 0.0f;
    private Vector3 prevous_position = Vector3.zero;
    private bool hitted = false;
    private Vector3 hitted_position = Vector3.zero;

    private float adjust_load = 1.0f;
    private int adjust_count = 0;

    private Vector3 explosion_debugs_position = Vector3.zero;

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
            rigidbody.AddForce(transform.forward * speed / 2 * Time.fixedDeltaTime, ForceMode.Impulse);
            shooted = true;
        }
        else
        {
            if (target)
            {
                if (adjust_load >= 1.0f)
                {
                    adjust_load = 0.0f;
                    Vector3 target_position = target.GetCenterPoint();

                    float distance = Vector3.Distance(target_position, transform.position);

                    target_position += Random.insideUnitSphere * distance * vel;
                    transform.LookAt(target_position);
                }
            }
            rigidbody.AddForce(transform.forward * speed * Time.fixedDeltaTime, ForceMode.Force);
            adjust_load += Time.fixedDeltaTime / adjust_load_time;
            adjust_load = Mathf.Min(1.0f, adjust_load);
        }

        if (distance_count >= effective_distance)
        {
            Destroy(this.gameObject);
        }

        distance_count += Vector3.Distance(prevous_position, transform.position);
        prevous_position = transform.position;

    }
    private void handleHit(Vector3 position, GameObject gameObject)
    {
        GameObject SmallExplosion = (GameObject)Resources.Load("SmallExplosion");
        Instantiate(SmallExplosion, position, transform.rotation);

        List<CharacterModel> characters = new List<CharacterModel>();
        CharacterModel character = getCharacterModel(gameObject);
        if (character)
        {
            characters.Add(character);
        }
        
        Collider[] colliders = Physics.OverlapSphere(position, explosion_raduis);
        foreach (Collider collider in colliders)
        {
            CharacterModel _character = getCharacterModel(collider.gameObject);
            if (_character && !characters.Contains(_character))
            {
                characters.Add(_character);
            }
        }
        foreach(CharacterModel _character in characters)
        {
            bool sameTeam = (_character.team == owner.GetTeam());
            if (!sameTeam)
            {
                _character.GotHit(power, sameTeam);
            }
        }
    }

    private CharacterModel getCharacterModel(GameObject gameObject)
    {
        CharacterModel char_model = gameObject.GetComponent<CharacterModel>();
        if (!char_model && gameObject.transform.root != null)
        {
            char_model = gameObject.transform.root.GetComponent<CharacterModel>();
        }
        return char_model;
    }

    private void OnTriggerEnter(Collider other)
    {
        bool isOwner = (other.gameObject == owner);
        bool sameOwner = (other.gameObject.GetOwner() == owner);

        if (!isOwner && !sameOwner)
        {
            hitted = true;
            hitted_position = other.ClosestPointOnBounds(transform.position);
            handleHit(hitted_position, other.gameObject);
        }

    }
}
