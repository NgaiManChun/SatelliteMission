using System.Collections.Generic;
using UnityEngine;

namespace ExtensionMethods
{

    public class CharacterDistanceComparer : IComparer<CharacterModel>
    {
        public Vector3 origin = Vector3.zero;
        public int Compare(CharacterModel x, CharacterModel y)
        {
            return Vector3.Distance(origin, x.gameObject.GetCenterPoint()).CompareTo(Vector3.Distance(origin, y.gameObject.GetCenterPoint()));
        }
    }

    public static class Vector3Extensions {
        public static Vector3 bezier2(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float f0 = Mathf.Pow(1 - t, 2);
            float f1 = 2 * (1 - t) * t;
            float f2 = Mathf.Pow(t, 2);

            Vector3 result =
                p0 * f0 +
                p1 * f1 +
                p2 * f2;
            return result;
        }

        public static Vector3 bezier3(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
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

    public static class GameObjectExtensions
    {
        public static int GetTeam(this GameObject gameObject)
        {
            CharacterModel character = gameObject.GetComponent<CharacterModel>();
            if (character)
            {
                return character.team;
            }
            return 0;
        }

        public static Vector3 GetCenterPoint(this GameObject gameObject)
        {
            Vector3 position = gameObject.transform.position;
            Transform center_position = gameObject.transform.Find("center_position");
            if (center_position)
            {
                position = center_position.position;
            }
            return position;
        }

        public static float EaseInOutQuad(this float t)
        {
            return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
        }

        public static GameObject GetOwner(this GameObject gameObject)
        {
            BulletScript bulletScript = gameObject.GetComponent<BulletScript>();
            if (bulletScript)
            {
                return bulletScript.owner;
            }
            MissileScript missileScript = gameObject.GetComponent<MissileScript>();
            if (missileScript)
            {
                return missileScript.owner;
            }
            return null;
        }

        public static bool SetOwner(this GameObject gameObject, GameObject owner)
        {
            BulletScript bulletScript = gameObject.GetComponent<BulletScript>();
            if (bulletScript)
            {
                bulletScript.owner = owner;
                return true;
            }
            MissileScript missileScript = gameObject.GetComponent<MissileScript>();
            if (missileScript)
            {
                missileScript.owner = owner;
                return true;
            }
            return false;
        }
    }

}
