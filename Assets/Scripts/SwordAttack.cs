using UnityEngine;
using UnityEngine.Audio;

public class SwordAttack : MonoBehaviour
{
    public GameObject swordCollider;
    public GameObject swordPrefab;
    public float deleteTime = 0.5f;

    bool isAttack;
    Transform player;

    AudioSource audioSource;
    public AudioClip se_Sword;

    void Start()
    {
        //AudioSource�̎擾
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // �Q�[���v���C���̂ݔ���
        if (GameManager.gameState != GameState.playing) return;

        if (Input.GetMouseButtonDown(1) && isAttack)
        {
            Sword();
        }
    }

    void Sword()
    {
        if (isAttack) return;
        if (player == null) return;

        //SE
        audioSource.PlayOneShot(se_Sword);
        Debug.Log("������");
        isAttack = true;

        Destroy(gameObject, deleteTime);
    }
}
