using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //if all checkpoints are passed
        if (GameObject.Find("Checkpoint") == null) {

            Destroy(gameObject);

            //if not the last scene
            if (SceneManager.GetActiveScene().buildIndex < 2) {
                SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
