using UnityEngine;
using System.Collections;

public class BombScript : MonoBehaviour {
    ParticleSystem bombSyst;
    GameObject self;
	// Use this for initialization
	void OnTriggerEnter () {
        bombSyst.Play();
       
       // self.a
	}
	
	
}
