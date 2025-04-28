using Renci.SshNet;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Text;

//zdroj:
//https://stackoverflow.com/questions/30883237/how-to-run-commands-on-ssh-server-in-c

namespace UpgradeSwitches
{
    internal class Program
    {
        private ShellStream stream;
        private SshClient sshclient;
        private List<string> switches = new List<string>();
        private string switchesListFileName = "switches-list.txt";

        private string tftpServerIP = "0.0.0.0";
        private string imageName = "summitX-32.7.1.9-patch1-68.xos";
        private string userName = "sshtest";
        private string password = "sshtest";
        private string slot = "primary";

        static void Main(string[] args)
        {
            Log("\n\nPouziti: (zadne prepinace, jen stringy s mezerami bez zavorek v nasledujicim poradi", ConsoleColor.Yellow);
            Log("UpgradeSwitches [Tftp IP] [Image Name] [User Name] [Password]\n", ConsoleColor.Yellow);
            Log("Soubor se seznamem switchu: switches-list.txt (co radek to IP adresa) \n\n", ConsoleColor.Yellow);
            Program program = new();
            program.Start(args);
        }


        public void Start(string[] args) {

            if (args.Length == 4)       //Nacteni parametru
            {
                tftpServerIP = args[0];
                imageName = args[1];
                userName = args[2];
                password = args[3];
            } else                      //Parametry jsou spatne, pouzity vychozi hodnoty
            {
                Log("Chybny pocet parametru nebo nezadane parametry.", ConsoleColor.Red);
                Log("Byly pouzity vychozi hodnoty.");
            }

            Log("Parametry:", ConsoleColor.Cyan);
            Log($"Tftp IP: {tftpServerIP}", ConsoleColor.Yellow);
            Log($"Image Name: {imageName}", ConsoleColor.Yellow);
            Log($"User Name: {userName}", ConsoleColor.Yellow);
            
            if (!File.Exists(switchesListFileName)) //Sobor se seznamem switchu nenalezen
            {
                Log($"Nenalezen soubor {switchesListFileName}\n\n", ConsoleColor.Red);
                return;
            }

            //Nacteni seznamu switchu ze souboru
            var switchesListFileContent = File.ReadAllLines(switchesListFileName);
            switches = new List<string>(switchesListFileContent);

            foreach (string switchIp in switches) {
                Log($"\nSwitch IP: {switchIp}", ConsoleColor.Yellow);
                try //zachyceni pripadne chyby
                {
                    //Pripojeni SSH
                    var connInfo = new Renci.SshNet.PasswordConnectionInfo(switchIp, 22, userName, password);
                    var sshClient = new Renci.SshNet.SshClient(connInfo);
                    sshClient.Connect();

                    //Vytvorteni a odeslani prikazu
                    string commandText = $"download image tftp://{tftpServerIP}/Extreme/{imageName} "; // partition primary install reboot
                    //https://documentation.extremenetworks.com/exos_32.3/GUID-40C25AA2-D2FE-4715-B4CA-2B7137629CA3.shtml

                    //string commandText = $"dir"; // Slot ????
                    Log($"{commandText}", ConsoleColor.Cyan);
                    using (var command = sshClient.CreateCommand(commandText))
                    {
                        var output = command.Execute();
                        if(command.ExitStatus !=0)
                        {
                            Log($"ExitStatus:{command.ExitStatus}", ConsoleColor.Red);
                            Log($"Error:{command.Error}", ConsoleColor.Red);
                        }
                        Log($"{output}", ConsoleColor.Green);
                        
                    }
                    sshClient.Disconnect();
                }
                catch (Exception ex) {
                    Log(ex.Message, ConsoleColor.Red);
                }
            }
        }

        //Logovaci funkce
        private static void Log(string txt, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(txt);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void Log(string txt)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(txt);
        }

    }
}

/*
SshCommand sc = sshclient.CreateCommand("mkdir ____TESTDIR");
sc.Execute();
string answer = sc.Result;
Console.WriteLine(answer);
*/

