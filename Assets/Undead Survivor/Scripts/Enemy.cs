using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHelath;
    public Rigidbody2D target;
    public RuntimeAnimatorController[] animeCon;

    bool isLive;

    Rigidbody2D rigid;
    Collider2D coll;

    SpriteRenderer sprite;
    Animator anime;
    WaitForFixedUpdate wait;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anime = GetComponent<Animator>();
        wait = new WaitForFixedUpdate();
    }

    void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
        {
            return;
        }

        if (!isLive || anime.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
        {
            return;
        }

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (!GameManager.instance.isLive)
        {
            return;
        }

        if (!isLive)
        {
            return;
        }
        
        sprite.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        sprite.sortingOrder = 2;
        anime.SetBool("Dead", false);
        health = maxHelath;
    }

    public void Init(SpawnData data)
    {
        anime.runtimeAnimatorController = animeCon[data.spriteType];
        speed = data.speed;
        maxHelath = data.health;
        health = data.health;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive)
        {
            return;
        }

        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack());
        if(health > 0)
        {
            anime.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            sprite.sortingOrder = 1;
            anime.SetBool("Dead", true);
            GameManager.instance.kill++;
            GameManager.instance.GetExp();

            if(GameManager.instance.isLive)
            {
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
            }
        }
    }

    IEnumerator KnockBack()
    {
        yield return wait; // 하나의 물리 프레임 딜레이
        Vector3 plaeyrPos = GameManager.instance.player.transform.position;
        Vector3 dirVector = transform.position - plaeyrPos;
        rigid.AddForce(dirVector.normalized * 3, ForceMode2D.Impulse);
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }

}
