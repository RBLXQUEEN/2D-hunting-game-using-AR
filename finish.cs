using UnityEngine;
using UnityEngine.SceneManagement;


public class finish : MonoBehaviour
{
    [SerializeField] ParticleSystem finishparticles;
   void OnTriggerEnter2D(Collider2D other)
    {
        int Layerindex = LayerMask.NameToLayer("player");

        if (other.gameObject.layer == Layerindex)
        {
            Debug.Log("Level Completed!!!!!!");
            finishparticles.Play();
            Invoke("reload",2f);
            
                                                                
        }
        
        
    }
    void reload()
    {
        SceneManager.LoadScene(0);
    }
}
