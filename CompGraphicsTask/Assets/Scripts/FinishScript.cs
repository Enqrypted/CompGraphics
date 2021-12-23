using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishScript : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject FinishText, StartText;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "player") {
            StartText.SetActive(false);
            FinishText.SetActive(true);
            Destroy(this.gameObject);
        }
    }
}
