 using UnityEngine;
using System.Collections;

/* Example script to apply trauma to the camera or any game object */
public class TraumaInducer : MonoBehaviour 
{
    [Tooltip("Maximum distance in which objects are affected by this TraumaInducer")]
    public float Range = 45;

    public void Impose(float MaximumStress = 0.2f)
    {
        /* Find all gameobjects in the scene and loop through them until we find all the nearvy stress receivers */
        var targets = UnityEngine.Object.FindObjectsOfType<GameObject>();
        for(int i = 0; i < targets.Length; ++i)
        {
            var receiver = targets[i].GetComponent<StressReceiver>();
            if(receiver == null) continue;
            float distance = Vector3.Distance(transform.position, targets[i].transform.position);
            /* Apply stress to the object, adjusted for the distance */
            if(distance > Range) continue;
            float distance01 = Mathf.Clamp01(distance / Range);
            float stress = (1 - Mathf.Pow(distance01, 2)) * MaximumStress;
            receiver.InduceStress(stress);
        }
    }
}