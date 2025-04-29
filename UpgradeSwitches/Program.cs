using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System.ComponentModel.Design;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Windows.Input;

//sshConnect zdroj:
//https://stackoverflow.com/questions/30883237/how-to-run-commands-on-ssh-server-in-c
//Extreme switch update zdroj:
//https://documentation.extremenetworks.com/exos_32.3/GUID-40C25AA2-D2FE-4715-B4CA-2B7137629CA3.shtml
//"summitX-32.7.1.9-patch1-68.xos"

namespace SSHCommandSender
{
    internal class Program
    {
        private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true, AllowTrailingCommas = true};
        private string configFileName = "config.json";
        private SshClient sshclient;
        private static LogWriter logWriter;
        private Config config;
        private bool testOnly;
        private string? arg0;
        private string? arg1;

        private string sshPwd = "";

        //Hlavni spousteci bod programu (spusti metodu Start)
        static void Main(string[] args)
        {
            Program program = new();
            program.Start(args);
        }

        //Start
        public void Start(string[] args) {
            
            logWriter = new LogWriter(); //vytvoreni instance Loggeru (zaroven vypisuje na obrazovku)

            //============================ Nacteni konfigurace ze souboru config.json ============================
            try
            {
                if (File.Exists(configFileName)) //existuje config.json ?
                {
                    var json = File.ReadAllText(configFileName); //precit JSON

                    if (json != null) //kdyz soubor neni prazdny
                    {
                        Console.WriteLine("Obsah souboru config.json:");
                        Console.WriteLine("--------------------------");
                        Console.WriteLine(json.ToString());
                        config = JsonSerializer.Deserialize<Config>(json, _options); //Namapuj na objekt
                        if (config == null)
                        {
                            Log("Configurace se nezdarila (config = null)", ConsoleColor.Red);
                            return;
                        }
                    }
                    else
                    {
                        Log("Soubor config.json je prazdny", ConsoleColor.Red);
                        return;
                    }
                }
                else
                {
                    Log("Soubor config.json nenalezen", ConsoleColor.Red);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString(), ConsoleColor.Red);
                return;
            }

            //=============================== Nacteni parametru test nebo run a sshPasword, nebo vyzva k rucnimu zadani ==================================
            if (args.Length > 0 )  
            {
                arg0 = args[0];
                if (args.Length > 1)
                {
                    arg1 = args[1];
                }
            }

            //volba test nebo run z parametru
            if (!string.IsNullOrEmpty(arg0)){
                if (arg0 == "test")
                {
                    testOnly = true;
                    Log("Zvolen rezim TEST", ConsoleColor.Yellow);
                }
                else if (arg0 == "run")
                {
                    testOnly = false;
                    Log("Zvolen rezim RUN", ConsoleColor.Yellow);
                }
                else
                {
                    Log("Nespravny prvni parametr [ test | run ]", ConsoleColor.Red);
                    arg0 = null; //pokud se nepovede, vynulujeme a jedem dal
                }
            }

            //pokud neni test nebo run spravne zadano parametrem, zeptej se
            if(string.IsNullOrEmpty(arg0))
            {
                Log("Zadej run nebo test", ConsoleColor.Yellow);
                string result = Console.ReadLine();
                if (!String.IsNullOrEmpty(result)) {
                    if (result == "test")
                    {
                        testOnly = true;
                        Log("Zvolen rezim TEST", ConsoleColor.Yellow);
                    }
                    else if (result == "run")
                    {
                        testOnly = false;
                        Log("Zvolen rezim RUN", ConsoleColor.Yellow);
                    } else { 
                        Log("Nespravny prvni parametr [ test | run ] takze konec.", ConsoleColor.Red);
                        return;
                    }
                }
                else { 
                    Log("Nespravny prvni parametr [ test | run ]  takze konec.", ConsoleColor.Red);
                    return;
                }
            }

            //SSH heslo - z parametru nebo vyzva
            if(!string.IsNullOrEmpty(arg1)) //pokud je zadano ssh heslo v parametru
            {
                sshPwd = arg1;
            }
            else if (!String.IsNullOrEmpty(config.sshPwd)) //nebo pokud je zadano v JSONu
            {
                sshPwd = config.sshPwd;
            }
            else //nebo heslo zadame rucne
            {
                Console.WriteLine("Enter SSH password:");
                sshPwd = Console.ReadLine();
                if (String.IsNullOrEmpty(sshPwd)) //pokud ho nezadame, so what, bude prazdny
                {
                    sshPwd = "";
                }
            }

            //==================================== projed vsechny IP z JSONu, pripoj ssh a odesli prikazy ====================================
            foreach (string switchIp in config.sshIpList) {
                
                Log($"------ Pripojuji se SSH k IP: {switchIp} ------", ConsoleColor.Yellow);
                
                try //zachyceni pripadne chyby
                {
                    //Pripojeni SSH
                    var connInfo = new Renci.SshNet.PasswordConnectionInfo(switchIp, 22, config.sshUserName, sshPwd);
                    var sshClient = new Renci.SshNet.SshClient(connInfo);
                    sshClient.Connect();

                    //Vytvorteni a vypis prikazu 1
                    string? command1 = ReplaceSshParams(config.sshCommand1); //nahradi prislusne parametry
                    if(!string.IsNullOrEmpty(command1)) 
                    {
                            SendCommand(sshClient, command1, testOnly);  // Odeslani prikazu + vypis vysledku (RUN)    
                    }

                    //Vytvorteni a vypis prikazu 1
                    string? command2 = ReplaceSshParams(config.sshCommand2); //nahradi prislusne parametry
                    if (!string.IsNullOrEmpty(command2))
                    {
                        SendCommand(sshClient, command2, testOnly);  // Odeslani prikazu + vypis vysledku (RUN)    
                    }

                    //Vytvorteni a vypis prikazu 1
                    string? command3 = ReplaceSshParams(config.sshCommand3); //nahradi prislusne parametry
                    if (!string.IsNullOrEmpty(command3))
                    {
                        SendCommand(sshClient, command3, testOnly);  // Odeslani prikazu + vypis vysledku (RUN)    
                    }

                    //Vytvorteni a vypis prikazu 1
                    string? command4 = ReplaceSshParams(config.sshCommand4); //nahradi prislusne parametry
                    if (!string.IsNullOrEmpty(command4))
                    {
                        SendCommand(sshClient, command4, testOnly);  // Odeslani prikazu + vypis vysledku (RUN)    
                    }
                    sshClient.Disconnect(); //SSH konec
                }
                catch (Exception ex) {
                    Log(ex.Message, ConsoleColor.Red); //vypis pripadne chyby
                }
                Console.WriteLine();
            }
        }

        //funkce pro nahrazeni #sshParam# skutecnymi parametry z JSONU
        private string? ReplaceSshParams( string? commandText)
        {
            if(!string.IsNullOrEmpty(commandText)) {
                if (!string.IsNullOrEmpty(config.sshVariable1)) {
                    commandText = commandText.Replace("#sshParam1#", config.sshVariable1); 
                }
                if (!string.IsNullOrEmpty(config.sshVariable2))
                {
                    commandText = commandText.Replace("#sshParam2#", config.sshVariable2);
                }
                if (!string.IsNullOrEmpty(config.sshVariable3))
                {
                    commandText = commandText.Replace("#sshParam3#", config.sshVariable3);
                }
                if (!string.IsNullOrEmpty(config.sshVariable4))
                {
                    commandText = commandText.Replace("#sshParam4#", config.sshVariable4);
                }
                return commandText;
            } else
            {
                return null;
            }

        }


        //funkce na vytvoreni a exekuci prikazu v SSH
        private void SendCommand(SshClient sshClient, string commandText, bool test)
        {
            Log($"Prikaz v SSH:");
 
            if (test == true) // pokud je rezim TEST, vypise prikaz jen jako ECHO v konzoli ssh serveru
            {
                commandText = $"echo {commandText}";
            }

            Log($"{commandText}", ConsoleColor.Cyan); // Vypis odesilaneho prikazu

            Log($"Odpoved:");
            using (var command = sshClient.CreateCommand(commandText))
            {
                var output = command.Execute();
                if (command.ExitStatus != 0)
                {
                    Log($"ExitStatus:{command.ExitStatus}", ConsoleColor.Red);
                    Log($"Error:{command.Error}", ConsoleColor.Red);
                }
                Log($"{output}", ConsoleColor.Green);
            }
        }

        //Logovaci funkce
        private  void Log(string txt, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.Gray;
            logWriter.Log(txt);
        }

        private  void Log(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(txt);
            logWriter.Log(txt);
        }

    }

    //Trida pro namapovani JSON parametru
    public class Config
    {
        public string sshUserName { get; set; } = "";
        public string sshPwd { get; set; } = "";
        public string sshVariable1 { get; set; } = "";
        public string sshVariable2 { get; set; } = ""; 
        public string sshVariable3 { get; set; } = "";
        public string sshVariable4 { get; set; } = "";
        public string sshCommand1 { get; set; } = "";
        public string sshCommand2 { get; set; } = "";
        public string sshCommand3 { get; set; } = "";
        public string sshCommand4 { get; set; } = "";
        public List<string> sshIpList { get; set; } = new List<string>();
    }
    /* ------ json file format --------
   
    
    */
}

/*
SshCommand sc = sshclient.CreateCommand("mkdir ____TESTDIR");
sc.Execute();
string answer = sc.Result;
Console.WriteLine(answer);


using (var command = sshClient.CreateCommand(commandText))
                    {
                        var asyncExecute = command.BeginExecute();
                        command.OutputStream.CopyTo(Console.OpenStandardOutput());
                        command.EndExecute(asyncExecute);
                    }

*/

