using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy : MonoBehaviour
{
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private Transform currentWaypoint;
    public float movementSpeed = 5f;
    public float waitTime = 10f;
    public float fieldOfViewAngle = 90f;
    public float viewDistance = 10f;

    public float distanceThreshold = 1f; // Yeni public mesafe değeri

    private float timer = 0f;
    private bool isWaiting = false;
    private bool isPlayerDetected = false; // Player algılandı mı?

    private Animator animator;
    private Quaternion targetRotation;

    private void Start()
    {
        if (waypoints.Count > 0)
        {
            currentWaypoint = waypoints[currentWaypointIndex];
        }

        animator = GetComponent<Animator>();
    }

    private bool IsPlayerInSight()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, viewDistance); // Düşmanın etrafında görüş mesafesi içindeki tüm nesneleri alır

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                // Player tag'ine sahip nesneyi bulduk
                return true;
            }
        }

        return false; // Player tag'ine sahip nesne bulunamadı
    }

    private void Update()
    {
        isPlayerDetected = IsPlayerInSight(); // Player algılandı mı?

        if (!isPlayerDetected)
        {
            animator.SetBool("running", false);

            if (currentWaypoint != null)
            {
                if (!isWaiting)
                {
                    animator.SetBool("walking", true);

                    Vector3 targetPosition = new Vector3(currentWaypoint.position.x, transform.position.y, currentWaypoint.position.z);
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

                    Vector3 direction = targetPosition - transform.position;
                    float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    targetRotation = Quaternion.Euler(0f, angle, 0f);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);

                    if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
                    {
                        StartWaiting();
                    }
                }
                else
                {
                    animator.SetBool("walking", false);
                    animator.SetBool("idle", true);

                    timer += Time.deltaTime;

                    if (timer >= waitTime)
                    {
                        StopWaiting();
                        currentWaypointIndex++;

                        if (currentWaypointIndex >= waypoints.Count)
                        {
                            currentWaypointIndex = 0;
                        }

                        currentWaypoint = waypoints[currentWaypointIndex];
                    }
                }
            }
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                animator.SetBool("walking", false);
                animator.SetBool("idle", false);
                animator.SetBool("running", true);

                Vector3 playerDirection = player.transform.position - transform.position;
                float angleToPlayer = Vector3.Angle(playerDirection, transform.forward);

                if (angleToPlayer < fieldOfViewAngle * 0.5f && Vector3.Distance(transform.position, player.transform.position) <= viewDistance)
                {
                    // Oyuncu görüş mesafesinde ve açısında
                    float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

                    if (distanceToPlayer > distanceThreshold)
                    {
                        // Düşmanın hedef pozisyonunu belirli bir mesafenin gerisine ayarla
                        Vector3 targetPosition = player.transform.position - playerDirection.normalized * distanceThreshold;

                        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

                        // Player'a doğru bakma
                        Quaternion lookRotation = Quaternion.LookRotation(playerDirection);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 360f * Time.deltaTime);
                    }
                    else
                    {
                        animator.SetBool("walking", false);
                        animator.SetBool("idle", false);
                        animator.SetBool("running", false);
                        animator.SetBool("attack", true);
                        player.GetComponent<FPSController>().enabled = false;

                    }
                }
            }
        }
    }

    private void StartWaiting()
    {
        isWaiting = true;
        timer = 0f;
    }

    private void StopWaiting()
    {
        isWaiting = false;
        animator.SetBool("idle", false);
    }
}
