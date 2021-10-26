using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class PedestrianAI : MonoBehaviour
{
    private Animator anim;
    private GameObject[] destinationLocations;
    UnityEngine.AI.NavMeshAgent agent;
    [SerializeField] bool isMale = true;
   

    // Start is called before the first frame update
    void Start()
    {
        SetRandomSpeed();
        GetGender();
        SetRandomProperties();
        
    }

    // Update is called once per frame
    void Update()
    {
        SetCorrectAnimation();
        SetCorrectDestination();
    }

    // Now there is only one animation triggered, walk- and idle-anim are not yet in use
    void SetCorrectAnimation()
    {
        anim.SetTrigger("IsWalking");
    }

    void SetCorrectDestination()
    {
        if(agent.remainingDistance < 1)
        {
            agent.SetDestination(destinationLocations[Random.Range(0, destinationLocations.Length)].transform.position);
        }
    }
    
    // Set speed multiplier for a random speed of the pedestrians
    void SetRandomSpeed()
    {
        float sm = Random.Range(0.4f, 1.5f);

        destinationLocations = GameObject.FindGameObjectsWithTag("Destination");
        agent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.SetDestination(destinationLocations[Random.Range(0, destinationLocations.Length)].transform.position);
        anim = this.GetComponent<Animator>();

        anim.SetFloat("WalkingOffset", Random.Range(0, 1.0f));
        anim.SetFloat("SpeedMultiplier", sm);
        agent.speed *= sm;
    }
    void SetRandomProperties()
    {
        //Change scale randomly
        Vector3 randomScale = GetComponent<Transform>().localScale;
        float randomFloatMale = Random.Range(0.95f, 1.05f);
        float randomFloatFemale = Random.Range(0.9f, 1.00f);
          
        if (isMale)
        {
            Vector3 randomVector = new Vector3(randomFloatMale, randomFloatMale, randomFloatMale);
            this.GetComponent<Transform>().localScale = randomVector;
            this.GetComponent<NavMeshAgent>().height *= randomFloatMale;
        }
        else
        {
            Vector3 randomVector = new Vector3(randomFloatFemale, randomFloatFemale, randomFloatFemale);
            this.GetComponent<Transform>().localScale = randomVector;
            this.GetComponent<NavMeshAgent>().height *= randomFloatFemale;
        }
               

    }

    void GetGender()
    {
        if (this.tag == "Male")
        {
            isMale = true;
        }
        else
        {
            isMale = false;
        }
    }
}
