using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPointHandler : MonoBehaviour
{
    private float duration = 1f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void movePointTo(Vector3 targetPosition)
    {
        StopAllCoroutines();
        StartCoroutine(moveTo(targetPosition));
    }

    IEnumerator moveTo(Vector3 targetPosition)
    {
        // source: https://gamedevbeginner.com/how-to-move-objects-in-unity/
        float timeElapsed = 0;
        Vector3 startPosition = transform.position;
        while (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }


}
