using UnityEngine;
using CAMOWA;
using System.Collections;
using System;
using DIMOWAModLoader;

namespace PGM
{

    public class ProbeGrapleMod : MonoBehaviour
    {
        public ClientDebuggerSide Debugger { get; private set; }

        private GameObject grapplePoint;
        public LayerMask whatIsGrappleable;

        public LineRenderer lr;
        private float lineThicness;
        private Color lineColor;

        public Transform player;
        public Transform modCamera;
        public Transform ship;

        //Assets importados
        private AudioClip throwClip;
        private Mesh hookMesh;
        private AudioSource wavPlayer;

        public bool IsGrappling { get; set; }

        private float grapleRadius = 5f;
        private float elasticConstant;
        private float frictionConstant;
        private float originalLenght;

        GUIStyle aparenciaDoTexto;
        
        [IMOWAModInnit("Probe Graple Mod", 1, 2)]
        public static void ModInnit(string porOndeTaInicializando)
        {
            Debug.Log("ProbeGrapleMod foi iniciado em "+ porOndeTaInicializando);
            GameObject.FindGameObjectsWithTag("Player")[0].AddComponent<ProbeGrapleMod>();
            Debug.Log("O script do mod foi colocado no 'Player' ");
        }


        private IEnumerator ImportAllFiles()
        {
            WWW wwwImportThrowClip = new WWW(WWWHelper.RelativePathToUrl(@"PGMAssets\Fuoooo.wav"));
            yield return wwwImportThrowClip;
            throwClip = wwwImportThrowClip.audioClip;
            yield break;
        }

        void Start()
        {
            Debugger = GameObject.Find("DIMOWALevelLoaderHandler").GetComponent<ClientDebuggerSide>();

            WWWHelper wwwHelper = new WWWHelper();
            StartCoroutine(ImportAllFiles());
            ObjImporter objImporter = new ObjImporter();
            //        \OuterWilds_Alpha_1_2_Data\OuterWilds_Alpha_1_2_Data\Assets\PGMAssets
            hookMesh = objImporter.ImportFile(@"PGMAssets\grapleHookModel.obj");

            modCamera = gameObject.FindWithRequiredTag("MainCamera").camera.transform;
            player = gameObject.FindWithRequiredTag("Player").transform;
            ship = gameObject.FindWithRequiredTag("Ship").transform;
            
            try
            {
                wavPlayer = gameObject.FindWithRequiredTag("Player").GetComponent<AudioSource>();
            }
            catch(Exception e)
            {
                Debugger.SendLog($"Erro ao pegar o AudioSource do player: {e}");
            }
            if(wavPlayer == null)
            {
                wavPlayer = gameObject.FindWithRequiredTag("Player").AddComponent<AudioSource>();
            }
            Debug.Log($"O wav player é {wavPlayer != null}");


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
        bool ropeBroke = false;
        void FixedUpdate()
        {
            if (grapplePoint != null)
            {

                float playerDistance = (player.position - grapplePoint.transform.position).magnitude;

                if(playerDistance >= grapleRadius + 25f)
                {
                    Debugger.SendLog("A corda se partiu");
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
                    
                    if(IsGrappling)
                        player.gameObject.GetAttachedOWRigidbody().AddForce(forcaElastica + forcaDeFriccao); //Alguem me ajuda plssss

                    else
                        grapplePoint.gameObject.GetAttachedOWRigidbody().AddForce( -(forcaElastica + forcaDeFriccao/3)  );

                    //Visual
                    lr.SetWidth(lineThicness * grapleRadius / playerDistance, lineThicness * grapleRadius / playerDistance);

                    lr.material.color = lineColor - new Color(0f, 1 - playerDistance / grapleRadius, 0f);
                }
                else
                    lr.SetWidth(lineThicness, lineThicness);
                
            }
        }

        float tempoPassado = 0f;
        readonly float tempoParaAlterar = 0.25f;

        

        void Update()
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

        void LateUpdate()
        {
            DrawRope();
        }

        float tempoDesdeOUltimoTexto = 0f;
        readonly float tempoDoTexto = 3f;
        bool showGUI = false;
        void OnGUI()
        {
            if (tempoDesdeOUltimoTexto <= tempoDoTexto && showGUI)
                GUI.Box(new Rect(559f, 519f, 680f, 93f), $"{grapleRadius} m", aparenciaDoTexto);
            else
                showGUI = false;

            tempoDesdeOUltimoTexto += Time.deltaTime;
        }
        
        void StartGrapple(Transform alvo, float ropeLenght, float ropeStrenght = 0.008f, float friction = 0.00651f)
        {
            grapplePoint = BasicOWRigidbodyGO.SimplestBoxOWObject(Vector3.one);
            grapplePoint.rigidbody.mass = 0.001f;
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

        void StopGrapple()
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

        void DrawRope()
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
