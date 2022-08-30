using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleCharacterController : MonoBehaviour
{
    public Text nameTxt;

    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    public Animation character;
    public AudioSource audioSource;
    public AudioClip run;

    private Vector3 target;
    private float moveSpeed = 1;

    void Start()
    {
        target = transform.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * moveSpeed);
        Vector3 targetDirection = target - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, 10 * Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    public void Walk(Vector3 target)
    {
        this.target = target;
        moveSpeed = 2;
        audioSource.Stop();
        character.CrossFade("Walk", .1f);
        StopCoroutine("PlayIdleWhenCharacterReachesTarget");
        StartCoroutine("PlayIdleWhenCharacterReachesTarget");
    }

    public void Run(Vector3 target)
    {
        this.target = target;
        moveSpeed = 5;
        audioSource.clip = run;
        audioSource.Play();
        character.CrossFade("Run", .1f);
        StopCoroutine("PlayIdleWhenCharacterReachesTarget");
        StartCoroutine("PlayIdleWhenCharacterReachesTarget");
    }

    IEnumerator PlayIdleWhenCharacterReachesTarget()
    {
        while (Vector3.Distance(transform.position, target) > 0) { yield return null; }
        yield return new WaitForSeconds(.25f);
        character.CrossFade("Idle", .1f);
        audioSource.Stop();
    }

}
