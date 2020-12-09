using CAMOWA;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace DropableItemTest
{
    public class DropableItem : MonoBehaviour
    {

        List<GameObject> listOfGO; 

        private Transform playerCamera;

        Texture2D cubeTexture;

         IEnumerator Start()
        {
            

            playerCamera = gameObject.FindWithRequiredTag("MainCamera").camera.transform;
            listOfGO = new List<GameObject>();
            
            
            //Não achei outra forma senão essa, não tem StartCoroutine nessa versão da Unity, então terá que ser feito de maneira manual
            //Mas terá coisas para facilitar o processo
            WWW www = new WWW(RelativePathToUrl(@"Assets\gameing.png"));
            yield return www;
            cubeTexture = www.texture;
        }
        

        string PathToUrl(string filePath)
        {
            return "file:///" + filePath.Replace(" ","%20");
        }

        string RelativePathToUrl(string relativeFilePath)
        {
            return PathToUrl(Application.dataPath + @"\" + relativeFilePath);
        }

        [IMOWAModInnit("Dropable Item Test",1,2)]
        public static void ModInnit(string porOndeTaInicializando)
        {

            Debug.Log($"{porOndeTaInicializando} foi iniciado em PlayerBody");
            GameObject.FindGameObjectsWithTag("Player")[0].AddComponent<DropableItem>();
            Debug.Log("O script do mod foi colocado no 'Player' ");

        }

        

        private IEnumerator WWWImageImport(string fileUrl)
        {
            WWW www = new WWW(fileUrl);
            yield return www;
            cubeTexture = www.texture;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                //uma unidade a frente da onde a camera ta olhando
                SpawnItem(playerCamera.position + playerCamera.forward);
                LaunchHook(listOfGO[0], playerCamera.forward, playerCamera.position + playerCamera.forward);
            }

        }

        void SpawnItem(Vector3 worldPositionOfSpawn)
        {
            listOfGO.Insert(0, BasicOWRigidbodyGO.SimplestBoxOWObject(Vector3.one));
            listOfGO[0].tag = "Probe";
            listOfGO[0].GetComponent<Rigidbody>().mass = 0.0001f;
            listOfGO[0].renderer.material.mainTexture = cubeTexture;
            listOfGO[0].transform.position = worldPositionOfSpawn;

        }

        GameObject SimplestBoxOWObject(Vector3 cubeSize , Vector3 colliderSize) 
        {
            GameObject simplestGO = GameObject.CreatePrimitive(PrimitiveType.Cube);

            simplestGO.transform.localScale = cubeSize;
            
            simplestGO.AddComponent<Rigidbody>().mass = 0.0001f;

            simplestGO.AddComponent<OWRigidbody>();

            GameObject simplestGODetector = new GameObject();
            simplestGODetector.GetComponent<Transform>().parent = simplestGO.transform;
            simplestGODetector.AddComponent<MeshFilter>().mesh = simplestGO.GetComponent<MeshFilter>().mesh;
            simplestGODetector.AddComponent<BoxCollider>().size = colliderSize;
            simplestGODetector.AddComponent<MultiFieldDetector>();
            


            return simplestGO;
        }

        private void LaunchHook(GameObject hook, Vector3 velocityVector, Vector3 globalPosition)
        {

            hook.transform.position = globalPosition;
            hook.GetAttachedOWRigidbody().AddVelocityChange(velocityVector);

        }

    }
}
