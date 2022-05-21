using UnityEngine;
using CAMOWA;
using CAMOWA.FileImporting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using BepInEx;

namespace PGM
{
    [BepInDependency("locochoco.plugins.CAMOWA", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("Locochoco.OWA.ProbeGrapple", "Probe Grapple", "1.1.1")]
    [BepInProcess("OuterWilds_Alpha_1_2.exe")]
    public class ProbeGrapleMod : BaseUnityPlugin
    {
        private GameObject grapplePoint;
        public LayerMask whatIsGrappleable;

        public LineRenderer lr;
        private float lineThicness;
        private Color lineColor;

        public Transform player;
        public Transform modCamera;
        public Transform ship;

        //Assets importados
        private static AudioClip throwClip;
        private static Mesh hookMesh;
        private AudioSource wavPlayer;

        public bool IsGrappling { get; set; }

        private float grapleRadius = 5f;
        private float elasticConstant;
        private float frictionConstant;
        private float originalLenght;

        private GUIStyle aparenciaDoTexto;

        private static string gamePath;
        public static string DllExecutablePath
        {
            get
            {
                if (string.IsNullOrEmpty(gamePath))
                    gamePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return gamePath;
            }

            private set { }
        }

        public static void GetFiles()
        {
            if (Application.loadedLevel == 1)
            {
                if (hookMesh == null)
                {
                    hookMesh = FileImporter.ImportOBJMesh(Path.Combine(DllExecutablePath, "grapleHookModel.obj"));
                }
                if (throwClip == null)
                {
                    try
                    {
                        throwClip = FileImporter.ImportWAVAudio(Path.Combine(DllExecutablePath, "Fuoooo.wav"));
                    }
                    catch { Debug.Log("Erro ao ler o audio"); }
                }
            }
        }

        private void Awake()
        {
            Debug.Log("ProbeGrapleMod was started");
            Debug.Log($"The mod script has been placed in the '{this.gameObject.name}'");
            SceneLoading.OnSceneLoad += SceneLoading_OnSceneLoad;
        }

        private void SceneLoading_OnSceneLoad(int sceneId)
        {
            if (sceneId == 1)
            {
                GetFiles();
                Initialize();
            }
        }

        private void Initialize()
        {
            if (Application.loadedLevel == 1)
            {
                modCamera = gameObject.FindWithRequiredTag("MainCamera").camera.transform;
                player = gameObject.FindWithRequiredTag("Player").transform;
                ship = gameObject.FindWithRequiredTag("Ship").transform;

                try
                {
                    wavPlayer = gameObject.FindWithRequiredTag("Player").GetComponent<AudioSource>();
                }
                catch (Exception e)
                {
                    Debug.Log($"Error getting AudioSource from player: {e}");
                }
                if (wavPlayer == null)
                {
                    wavPlayer = gameObject.FindWithRequiredTag("Player").AddComponent<AudioSource>();
                }
                Debug.Log($"The wav player {(wavPlayer != null ? "exists" : "does not exist")}");


                IsGrappling = false;

                lr = player.gameObject.AddComponent<LineRenderer>();

                lineThicness = 0.08f;
                lr.SetWidth(lineThicness, lineThicness);

                lr.material = new Material(Shader.Find("Diffuse"));

                lineColor = new Color(1f, 1f, 0.36f, 1f);
                lr.material.color = lineColor;
                lr.SetVertexCount(0);


                aparenciaDoTexto = new GUIStyle
                {
                    fontSize = 72
                };

                aparenciaDoTexto.normal.textColor = Color.gray;
            }
        }

        private bool ropeBroke = false;
        private void FixedUpdate()
        {
            if (Application.loadedLevel == 1)
            {
                if (grapplePoint != null)
                {

                    float playerDistance = (player.position - grapplePoint.transform.position).magnitude;

                    if (playerDistance >= grapleRadius + 25f)
                    {
                        Debug.Log("the rope broke");
                        StopGrapple();
                        lr.SetWidth(lineThicness, lineThicness);
                        ropeBroke = true;
                    }

                    if (grapleRadius <= playerDistance && !ropeBroke)
                    {
                        //Física

                        Vector3 direcao = grapplePoint.transform.position - player.position;

                        Vector3 forcaElastica = (playerDistance - grapleRadius) * elasticConstant * direcao.normalized * 0.80f;//o 80% é energia perdida em calor etc...

                        Vector3 forcaDeFriccao = (grapplePoint.rigidbody.velocity - player.gameObject.rigidbody.velocity) * frictionConstant;

                        if (IsGrappling)
                            player.gameObject.GetAttachedOWRigidbody().AddForce(forcaElastica + forcaDeFriccao); //Alguem me ajuda plssss

                        else
                            grapplePoint.gameObject.GetAttachedOWRigidbody().AddForce(-(forcaElastica + forcaDeFriccao / 3));

                        //Visual
                        lr.SetWidth(lineThicness * grapleRadius / playerDistance, lineThicness * grapleRadius / playerDistance);

                        lr.material.color = lineColor - new Color(0f, 1 - playerDistance / grapleRadius, 0f);
                    }
                    else
                        lr.SetWidth(lineThicness, lineThicness);

                }
            }
        }

        private float tempoPassado = 0f;
        private readonly float tempoParaAlterar = 0.25f;



        private void Update()
        {
            if (Application.loadedLevel == 1)
            {
                if (Input.GetKeyDown(KeyCode.G))
                {
                    if (grapplePoint != null)
                    {
                        StopGrapple();

                        lr.SetWidth(lineThicness, lineThicness);

                    }
                    else if(grapplePoint == null)
                    {
                        ropeBroke = false;
                        StartGrapple(player, grapleRadius);
                    }
                }
            

                tempoPassado += Time.deltaTime;

                if (tempoPassado >= tempoParaAlterar)
                {
                    if (Input.GetKey(KeyCode.T) && grapleRadius < 100f)
                    {
                        grapleRadius += 0.25f;
                        tempoPassado = 0f;
                        tempoDesdeOUltimoTexto = 0f;
                        showGUI = true;
                    }
                    else if (Input.GetKey(KeyCode.Y) && grapleRadius >= 0.75f)
                    {
                        grapleRadius -= 0.25f;
                        tempoPassado = 0f;
                        tempoDesdeOUltimoTexto = 0f;
                        showGUI = true;
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (Application.loadedLevel == 1)
                DrawRope();
        }

        private float tempoDesdeOUltimoTexto = 0f;
        private readonly float tempoDoTexto = 3f;
        private bool showGUI = false;
        private void OnGUI()
        {
            if (Application.loadedLevel == 1)
            {
                if (tempoDesdeOUltimoTexto <= tempoDoTexto && showGUI)
                    GUI.Box(new Rect(559f, 519f, 680f, 93f), $"{grapleRadius} m", aparenciaDoTexto);
                else
                    showGUI = false;

                tempoDesdeOUltimoTexto += Time.deltaTime;
            }
        }


        public static GameObject SimplestBoxOWObject(float mass = 1)
        {
            Vector3 cubeSize = Vector3.one;
            Vector3 colliderSize = Vector3.one;
            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            primitive.transform.localScale = cubeSize;
            primitive.AddComponent<Rigidbody>().mass = mass;
            primitive.AddComponent<OWRigidbody>();
            GameObject gameObject = new GameObject();
            gameObject.GetComponent<Transform>().parent = primitive.transform;
            gameObject.AddComponent<MeshFilter>().mesh = primitive.GetComponent<MeshFilter>().mesh;
            gameObject.AddComponent<BoxCollider>().size = colliderSize;
            gameObject.AddComponent<MultiFieldDetector>();
            return primitive;
        }

        private void StartGrapple(Transform alvo, float ropeLenght, float ropeStrenght = 0.008f, float friction = 0.00651f)
        {
            grapplePoint = SimplestBoxOWObject(0.001f);
            grapplePoint.transform.parent = transform.root;
            grapplePoint.AddComponent<HookAnchor>().HookManager = gameObject.GetComponent<ProbeGrapleMod>();
            grapplePoint.transform.name = "grapplingPointMod";
            grapplePoint.GetComponent<MeshFilter>().mesh = hookMesh;
            grapplePoint.renderer.material.color = new Color(0.25f, 0.25f, 0.25f,1f);

            grapleRadius = ropeLenght;
            originalLenght = ropeLenght;
            elasticConstant = ropeStrenght;
            frictionConstant = friction;

            LaunchHook(grapplePoint, modCamera.forward*5, modCamera.transform.position + modCamera.forward );
            lr.SetVertexCount(2);
            wavPlayer.PlayOneShot(throwClip,0.7f);
           
        }

        private void StopGrapple()
        {
            lr.SetVertexCount(0);
            for(int i = 0; i < grapplePoint.transform.childCount; i++)
            {
                Destroy(grapplePoint.transform.GetChild(i).gameObject);
            }
            Destroy(grapplePoint);
            IsGrappling = false;
        }

        private Vector3 currentGrapplePosition;

        private void DrawRope()
        {
            if (grapplePoint == null) return;

            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint.transform.position, Time.deltaTime * 32f);
            lr.SetPosition(0, player.position);
            lr.SetPosition(1, currentGrapplePosition);
        }

        public Vector3 GetGrapplePoint()
        {
            return grapplePoint.transform.position;
        }

        private void LaunchHook(GameObject hook, Vector3 velocityVector, Vector3 globalPosition)
        {
            hook.transform.position = globalPosition;
            hook.GetAttachedOWRigidbody().AddLocalVelocityChange(velocityVector);
        }
    }
}
