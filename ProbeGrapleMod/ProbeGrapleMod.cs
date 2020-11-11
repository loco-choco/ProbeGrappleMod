using UnityEngine;
using IMOWAAnotations;

namespace PGM
{

    public class ProbeGrapleMod : MonoBehaviour
    {

        private GameObject grapplePoint;
        public LayerMask whatIsGrappleable;


        public LineRenderer lr;
        private float lineThicness;
        private Color lineColor;

        public Transform player;
        public Transform modCamera;
        public Transform ship;



        private bool isGrappling;
        private float grapleRadius;
        private float elasticConstant;
        private float frictionConstant;
        private float originalLenght;

        GUIStyle aparenciaDoTexto;


        [IMOWAModInnit("PlayerBody", "Awake", modName = "Probe Graple Mod")]
        public static void ModInnit(string porOndeTaInicializando)
        {

            Debug.Log("ProbeGrapleMod foi iniciado em "+ porOndeTaInicializando);
            GameObject.FindGameObjectsWithTag("Player")[0].AddComponent<ProbeGrapleMod>();
            Debug.Log("O script do mod foi colocado no 'Player' ");

        }

        void Start()
        {

            modCamera = gameObject.FindWithRequiredTag("MainCamera").camera.transform;
            player = gameObject.FindWithRequiredTag("Player").transform;
            ship = gameObject.FindWithRequiredTag("Ship").transform;

            isGrappling = false;


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

            //Talvez n seja necessario
            aparenciaDoTexto.font.material = new Material(Shader.Find("Diffuse"))
            {
                color = Color.gray
            };
        }


        void FixedUpdate()
        {
            //Logica da física de 'esfera'
            if (isGrappling)
            {

                float playerDistance = (player.position - grapplePoint.transform.position).magnitude;
                if (grapleRadius <= playerDistance)
                {
                    //Física

                    Vector3 direcao = grapplePoint.transform.position - player.position;

                    Vector3 forcaElastica = (playerDistance - grapleRadius) * elasticConstant * direcao.normalized * 0.80f;//o 80% é energia perdida em calor etc...

                    Vector3 forcaDeFriccao = (grapplePoint.rigidbody.velocity - player.gameObject.rigidbody.velocity) * frictionConstant;

                    player.gameObject.GetAttachedOWRigidbody().AddForce(forcaElastica + forcaDeFriccao); //Alguem me ajuda plssss

                    //Visual
                    lr.SetWidth(lineThicness * grapleRadius / playerDistance, lineThicness * grapleRadius / playerDistance);

                    lr.material.color = lineColor - new Color(0f, 1 - playerDistance / grapleRadius, 0f);
                }

                else
                {
                    lr.SetWidth(lineThicness, lineThicness);
                }




            }
        }

        float tempoPassado = 0f;
        readonly float tempoParaAlterar = 0.25f;

        

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                if (isGrappling)
                {
                    Debug.Log("Stop grapling");
                    StopGrapple();

                    lr.SetWidth(lineThicness, lineThicness);

                }
                else
                {
                    Debug.Log("Sart grapling");
                    StartGrapple(player, 2f);
                }
            }


            tempoPassado += Time.deltaTime;

            if (isGrappling && tempoPassado >= tempoParaAlterar)
            {
                Debug.Log("Pronto para uzar");

                if (Input.GetKey(KeyCode.T))
                {
                    grapleRadius += 0.25f;
                    Debug.Log("Crescendo");
                    tempoPassado = 0f;
                    tempoDesdeOUltimoTexto = 0f;
                }

                if (Input.GetKey(KeyCode.Y) && grapleRadius >= 0.75f)
                {
                    grapleRadius -= 0.25f;
                    Debug.Log("Diminuindo");
                    tempoPassado = 0f;
                    tempoDesdeOUltimoTexto = 0f;
                }


            }
        }

        void LateUpdate()
        {
            DrawRope();
        }

        float tempoDesdeOUltimoTexto = 0f;
        readonly float tempoDoTexto = 3f;

        void OnGUI()
        {
            if (tempoDesdeOUltimoTexto <= tempoDoTexto && isGrappling)
                GUI.Box(new Rect(559f, 519f, 680f, 93f), $"{grapleRadius} m", aparenciaDoTexto);

            tempoDesdeOUltimoTexto += Time.deltaTime;
        }

        /// <summary>
        /// Call whenever we want to start a grapple
        /// </summary>
        /// 
        void StartGrapple(Transform alvo, float ropeLenght, float ropeStrenght = 0.008f, float friction = 0.03125f)
        {
            if (Physics.Raycast(modCamera.position, modCamera.forward, out RaycastHit hit, ropeLenght, OWLayerMask.GetPhysicalMask()))
            {
                Vector3 grapplePointPosition = hit.point;

                grapplePoint = GameObject.CreatePrimitive(PrimitiveType.Cube);

                grapplePoint.collider.enabled = false;

                grapplePoint.transform.name = "grapplingPointMod";
                grapplePoint.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                grapplePoint.renderer.material.color = new Color(72f, 45f, 128f);


                grapplePoint.transform.position = grapplePointPosition;
                grapplePoint.transform.parent = hit.transform;

                grapplePoint.AddComponent<Rigidbody>();
                grapplePoint.rigidbody.isKinematic = true;

                grapleRadius = ropeLenght;
                originalLenght = ropeLenght;

                elasticConstant = ropeStrenght;

                frictionConstant = friction;

                isGrappling = true;


                lr.SetVertexCount(2);
            }
        }


        /// <summary>
        /// Call whenever we want to stop a grapple
        /// </summary>
        void StopGrapple()
        {
            lr.SetVertexCount(0);
            Destroy(grapplePoint);
            isGrappling = false;
            // Destroy(joint);

        }

        private Vector3 currentGrapplePosition;

        void DrawRope()
        {
            if (!isGrappling) return;

            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint.transform.position, Time.deltaTime * 32f);

            lr.SetPosition(0, player.position);
            lr.SetPosition(1, currentGrapplePosition);


        }

        public Vector3 GetGrapplePoint()
        {
            return grapplePoint.transform.position;
        }


    }
}
