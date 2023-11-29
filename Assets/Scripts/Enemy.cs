using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] int speed = 5;
    public int nextIndex = 0;
    public List<Vector3> path;
    public int health;

    void FixedUpdate() {
        transform.position = Vector2.MoveTowards(transform.position, path[nextIndex], speed * Time.deltaTime);
        if(Vector2.Distance(transform.position, path[nextIndex]) < 0.1f){
            
            if(nextIndex < path.Count - 1){
                nextIndex++;
            } else {
                Destroy(gameObject);
            }
        }
    }
}
