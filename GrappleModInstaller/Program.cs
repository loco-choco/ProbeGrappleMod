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
            

           
                Console.WriteLine("Instalando o mod | Instaling the mod: GrappleMod");

                Patcher p = new Patcher("Assembly-CSharp.dll");

                Instruction[] opcodesGrappleModInnit = {

                Instruction.Create(OpCodes.Ldstr   ,  "ProbeGrapleMod foi iniciado | was started"), 

                Instruction.Create(OpCodes.Call,p.BuildCall(typeof(UnityEngine.Debug), "Log" , typeof(void) , new[]{ typeof(object) })),

                 Instruction.Create(OpCodes.Ldstr   ,  "PlayerBody"), 

                Instruction.Create(OpCodes.Call, p.BuildCall(typeof(PGM.ProbeGrapleMod), "ModInnit", typeof(void), new[] { typeof(string) }))

               
                };

            
                int[] indicesDoMod =
                {

                    0,
                    1,
                    2,
                    3
                    

                
                };

            
            Target target = new Target()
            {
                Namespace = "",
                Class = "PlayerBody",
                Method = "Awake",
                Instructions = opcodesGrappleModInnit,
                Indices = indicesDoMod,
                InsertInstructions = true

                };

                

            try
            {
                p.Patch(target);

                Console.WriteLine("Patching foi um sucesso, salvando agora. . . | Patching was a success, saving now. . .");
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Erro no Patching | Patching error: {exp}");
            }


            try
            {
                p.Save("Assembly-CSharp-ModLoaded.dll");
                Console.WriteLine("Mod Salvado com Sucesso :: ) | Mod saving was successfull :: )");
            }
            catch (Exception exp)
            {
                Console.WriteLine($"Erro no Saving | Saving error: {exp}");
                Console.WriteLine("O mod não foi possivel de ser instalado | It wasn't possible to save the mod");
            }

            

                Console.Read();
        }
    }
}
