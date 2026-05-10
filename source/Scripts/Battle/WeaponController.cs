using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class WeaponController : MonoBehaviour
{
    abstract public void fire(Vector3 aiming_to, GameObject target);

    abstract public void stop();
}
