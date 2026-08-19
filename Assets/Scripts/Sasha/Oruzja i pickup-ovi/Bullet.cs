using UnityEngine;
using System;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float lifeTime;
    public int damage = 10;
    private System.Random rand = new System.Random();

    void Start()
    {
        damage += rand.Next(5);
        Invoke("UnistavanjeBulleta", lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void UnistavanjeBulleta()
    {
        Destroy(gameObject);
    }
}