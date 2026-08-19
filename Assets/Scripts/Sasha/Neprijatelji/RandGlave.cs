using UnityEngine;


public class RandGlave : MonoBehaviour
{
    private int tipGlave;
    public GameObject glava;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Sprite glava1;
    [SerializeField] private Sprite glava2;
    [SerializeField] private Sprite glava3;
    [SerializeField] private Sprite glava4;
    [SerializeField] private Sprite glava5;

    void Start()
    {
        //glava.transform.rotation = Quaternion.Euler(-180, -180, 0);
        tipGlave = Random.Range(1, 6);
        
        switch (tipGlave)
        {
            case 1:
                sprite.sprite = glava1;
                break;
            case 2:
                sprite.sprite = glava2;
                break;
            case 3:
                sprite.sprite = glava3;
                break;
            case 4:
                sprite.sprite = glava4;
                break;
            case 5:
                sprite.sprite = glava5;
                break;
        }
    }
}
