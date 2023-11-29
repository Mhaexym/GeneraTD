using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RoundManager : MonoBehaviour
{
    //NOT SHOWING DESIRED BEHAVIOUR: SHOULD SPAWN 10 ENEMIES (CURRENTLY) BEFORE DELETING SPAWNER. 
    [SerializeField] private List<Spawner> spawners;
    private Spawner currentspawner;
    public static RoundManager ins;
    private int roundnumber = 0;
    public float timeBetweenRounds = 3;
    private float timeLeft;

    void Awake() {
        currentspawner = spawners[roundnumber];
        timeLeft = timeBetweenRounds;
    }
    void Update()
    {
        if(timeLeft <= 0){
            if(CheckRoundOver()){
                Debug.Log("Round is over");
                roundnumber++;
                InitiateNextRound();
            }
            timeLeft = timeBetweenRounds;
        }
        else{
            timeLeft -= Time.deltaTime;
        }
    }

    void InitiateNextRound()
    {   
        GameObject previousspawner = GameObject.FindGameObjectWithTag("Spawner");
        Destroy(previousspawner);
        if(roundnumber < spawners.Count)
        {
            currentspawner = spawners[roundnumber];
            currentspawner = Instantiate(spawners[roundnumber], Vector3.zero, Quaternion.identity);
        }
        else{
            roundnumber--;
            currentspawner = Instantiate(spawners[roundnumber], Vector3.zero, Quaternion.identity);
        }   
    }
    bool CheckRoundOver()
    {   
        if(GameObject.FindGameObjectWithTag("Spawner") != null){
            if(currentspawner.GetComponent<Spawner>().enemiesAlive > 0){
                return false;
            }
        }
        return true;
    }
}
