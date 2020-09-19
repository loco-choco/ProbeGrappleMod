using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnpatch;


namespace GrappleModInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            

           
                Console.WriteLine("Instalando o mod: GrappleMod");

                Patcher p = new Patcher("teste.dll");//Colocar o nome do DLL do jogo

                Instruction[] opcodesGrappleModInnit = {

                Instruction.Create(OpCodes.Ldstr   ,  "ProbeGrapleMod foi iniciado"), // ldarg.0

                Instruction.Create(OpCodes.Call,p.BuildCall(typeof(UnityEngine.Debug), "Log" , typeof(void) , new[]{ typeof(object) })),

                 Instruction.Create(OpCodes.Ldstr   ,  "PlayerBody"), // ldarg.0

                Instruction.Create(OpCodes.Call, p.BuildCall(typeof(PGM.ProbeGrapleMod), "ModInnit", typeof(void), new[] { typeof(string) }))

               
                };

            
                int[] indicesDoMod =
                {

                    0,
                    1,
                    2,
                    3
                    

                
                };

                //Descobrir uma maneira dessas instruções serem colocadas e não tomarem o lugar
                Target target = new Target()
                {
                    Namespace = "",
                    Class = "PlayerBody",
                    Method = "Awake",
                    Instructions = opcodesGrappleModInnit,
                    Indices = indicesDoMod
                };

                

            try
            {
                p.Patch(target, true);

                Console.WriteLine("Patching foi um sucesso, salvando agora. . .");
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Erro no Patching: {exp}");
            }


            try
            {
                p.Save("Assembly-CSharp-ModLoaded.dll");
                Console.WriteLine("Mod Salvado com Sucesso :: )");
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Erro no Saving: {exp}");
                Console.WriteLine("O mod não foi possivel de ser instalado");
            }

            

                Console.Read();
        }
    }
}
