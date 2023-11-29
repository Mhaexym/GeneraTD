using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] int amount;
    [SerializeField] int cooldown;
    [SerializeField] Enemy enemyPrefab;
    [SerializeField] GameObject _startNode;
    [SerializeField] GameObject _endNode;
    private GameObject[] path;
    private List<Vector3> pathpositions = new List<Vector3>();
    public List<GameObject> enemies;
    public int enemiesAlive = 0;
    public float totaltime;
    public float time;
    private float timeStore;

    void Start(){
        timeStore = time;
        totaltime = amount * time;
        path = GameObject.FindGameObjectsWithTag("Path");
        foreach(var node in path){
            pathpositions.Add(node.transform.position);
        }


    }
    void Update(){
        SpawnEnemy();
        if(totaltime <= 0){
            CheckEnemies();
        }
    }
    
    //Fix this to make sure it doesnt eat GPU for breakfast
    void SpawnEnemy(){
        if(totaltime > 0){
            if(time > 0){
                time -= Time.deltaTime;
            } 
            else {
                GameObject currentEnemy = Instantiate(enemyPrefab.gameObject,pathpositions[0],Quaternion.identity);
                enemies.Add(currentEnemy);
                enemiesAlive++;
                currentEnemy.GetComponent<Enemy>().path = pathpositions;
                totaltime -= timeStore;
                time = timeStore;
            }
        }      
    }
    //Fix this to make sure it doesnt eat GPU for breakfast
    // Maybe have it so that PathManager will update the end position immediately
    // By adding a collider to the EndPosition and removing it from the previous one.
    // If the enemy then hits something (OnCollisionEnter), we check if hits the EndPosition and act accordingly. (no update)
    // Maybe even having that code run in the EndPosition is more than enough.
    // That does mean that either the projectiles or the enemy should have the collision detection for hits, and this
    // brings up the question whether we're going to have more enemies, or more projectiles at any given time (where i think the first is true)
    //So, rebranding:
    // Enemy: should move along nodes, have hp, have a drop, and have simple weaknesses code.
    // Projectile: should have animation, point of origin and target, damage type and amount based on enemies weaknesses on collision. 
    // EndPosition: should have a simple rigidbody checking for collision with enemies, if so: destroy the enemy and decrease correct amount of lives. 
    // Spawner: should know which enemies to spawn, spawn them, and destroy itself. There should only ever be 1 spawner. 
    
    // Spawner should really only contain an list or amount (if its only a single type) of enemies to spawn, and
    // destroy itself when it is done spawning those. 
    void CheckEnemies(){
        if(enemies.Count > 0){
            foreach(var enemy in enemies){
                if(enemy == null)
                    enemiesAlive--;
            }
        }
    }
}
