using UnityEngine;
using System;

public class Projektil : MonoBehaviour
{
    public float speed;
    public float lifeTime;
    public int damage = 20;
    private System.Random rand = new System.Random();

    void Start()
    {
        damage += rand.Next(10);
        Invoke("UnistavanjeProjektila", lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    void UnistavanjeProjektila()
    {
        // Uklonjena linija za Instantiate(explosion...)
        Destroy(gameObject);
    }
}