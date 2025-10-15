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
        //AudioSource‚Ìæ“¾
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // ƒQ[ƒ€ƒvƒŒƒC’†‚Ì‚İ”½‰
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
        Debug.Log("‰¹‚ ‚è");
        isAttack = true;

        Destroy(gameObject, deleteTime);
    }
}
