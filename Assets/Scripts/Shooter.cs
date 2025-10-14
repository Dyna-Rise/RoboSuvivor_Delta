using System.Collections;using UnityEngine;public class Shooter : MonoBehaviour{
    public GameObject gate;         // Gate�I�u�W�F�N�g�̏��
    public GameObject bulletPrefab; // PlayerBullet�̃v���n�u���
    public float shootSpeed = 100f; // ��΂��́i�O�����j
    public int shotPower = 10;      // �c�e��
    public float recoverySeconds = 3.0f; // 1���񕜂ɂ�����b��
    public float upSpeed = 2.0f; //Bullet�̍���

    Transform player;   // �v���C���[��Transform���
    bool possibleShoot; // ���ˉ\�t���O
    Camera cam;         // �J�������

    void Start()    {
        // �v���C���[���̎擾
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // �q�I�u�W�F�N�g "Gate" ���擾
        gate = player.Find("Gate").gameObject;

        // ���C���J�����擾
        cam = Camera.main;

        // �����x��Ĕ��ˉ\�ɂ���
        Invoke(nameof(ShootEnabled), 0.5f);    }

    void Update()    {
        // �Q�[���v���C���̂ݔ���
        if (GameManager.gameState != GameState.playing) return;        if (Input.GetMouseButtonDown(0) && possibleShoot)        {            Shot();        }    }

    // ���ˉ\�ɂ���
    void ShootEnabled()    {        possibleShoot = true;    }

    // ���ˏ���
    void Shot()    {        if (player == null || shotPower <= 0) return;

        // Gate�̈ʒu�E��]����e�𐶐�
        Quaternion bulletRot = gate.transform.rotation * Quaternion.Euler(90f, 0f, 0f);        GameObject obj = Instantiate(bulletPrefab, gate.transform.position, bulletRot);

        // Rigidbody�擾���đO���֔���
        Rigidbody rbody = obj.GetComponent<Rigidbody>();        Vector3 shootDir = cam.transform.forward * shootSpeed;
        shootDir.y += upSpeed;

        // Rigidbody�Œe���΂�
        rbody.AddForce(shootDir, ForceMode.Impulse);

        // �e������
        ConsumePower();    }

    // �e�̏���
    void ConsumePower()    {        shotPower--;        StartCoroutine(RecoverPower());    }

    // �e�����[�h�R���[�`��
    IEnumerator RecoverPower()    {        yield return new WaitForSeconds(recoverySeconds);        shotPower++;    }}