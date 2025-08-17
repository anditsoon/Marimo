using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SchoolMoveManager : MonoBehaviour
{
    private List<GameObject> players = new List<GameObject>();

    public void startMovableCoroutine()
    {
        StartCoroutine(movableCoroutine());
    }

    private IEnumerator movableCoroutine()
    {
        yield return new WaitForSeconds(6f);
        MoveControl(true);
    }

    private void MoveControl(bool movable)
    {
        foreach (GameObject obj in players)
        {
            obj.GetComponent<PlayerMove>().Movable = movable;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            players.Add(other.gameObject);
        }
    }
}
