using System;
using System.IO;
using System.Threading;
class Probe {
 static void Main(string[] args) {File.WriteAllLines(args[0],new[]{Environment.CurrentDirectory,String.Join("|",args)});if(args.Length>1&&args[1]=="wait")Thread.Sleep(3500);}
}
