// Assets/Project/Dev3_Combat/Scripts/EnemyStub.cs
using HonVietThuThanh.Shared;
using HVTThanh.Combat;
using UnityEngine;

public class EnemyStub : MonoBehaviour, IDamageable, ITargetable
{
    public float hp = 50f;
    public float speed = 2f;

    public Vector3 GetPosition() => transform.position;
    public bool IsAlive() => hp > 0;

    public void TakeDamage(float amount)
    {
        hp -= amount;
        Debug.Log($"Enemy nhận {amount} damage, còn {hp} HP");
        if (hp <= 0)
        {
            Debug.Log("Enemy chết!");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Di chuyển thẳng về phía base (trục Z âm)
        transform.position += Vector3.back * speed * Time.deltaTime;

        // Tới cuối lane thì log và tự xoá
        if (transform.position.z < -10f)
        {
            Debug.Log("Enemy tới base!");
            Destroy(gameObject);
        }
    }
}