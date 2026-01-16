using UnityEngine;

public class HacerseHijoDePalo : MonoBehaviour
{
    [SerializeField] private GameObject mano;
    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("Mano"))
        {
           
            transform.SetParent(mano.transform); 
            transform.localPosition=new Vector3(-0.526f,0.123f,0.107f);
            transform.localRotation = Quaternion.Euler(-207.359f,184.937f,-90.41f);
        }
    }
}

-0.332   -0.248 -0.527
    -170.021  -44.949  0.241
Vector3(-0.332999021,-0.248126999,-0.527649999)
Vector3(350.021179,135.050873,240.544006)