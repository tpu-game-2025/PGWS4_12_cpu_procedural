using UnityEngine;

public class MovePoint : MonoBehaviour
{
    [SerializeField] Vector3 RangeMin = Vector3.zero;
    [SerializeField] Vector3 RangeMax = Vector3.one;
    [SerializeField] float deleteTime = 5f;

    Vector3 velocity;

    void Start()
    {
        velocity.x = UnityEngine.Random.Range(RangeMin.x, RangeMax.x);
        velocity.y = UnityEngine.Random.Range(RangeMin.y, RangeMax.y);
        velocity.z = UnityEngine.Random.Range(RangeMin.z, RangeMax.z);

        Destroy(gameObject, deleteTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += velocity * Time.deltaTime;
    }
}
