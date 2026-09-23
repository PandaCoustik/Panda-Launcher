using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace PandaLauncher {
public class Entry {
 public string Id {get;set;} public string Name {get;set;} public string Target {get;set;}
 public bool Enabled {get;set;} public int Position {get;set;} public string Arguments {get;set;}
 public string WorkingDirectory {get;set;} public int DelayMs {get;set;}
 public bool SkipRunning {get;set;} public bool Administrator {get;set;}
 public int WaitMode {get;set;} public int WaitTimeoutSeconds {get;set;}
 public Entry() { Id=Guid.NewGuid().ToString(); Enabled=true; SkipRunning=true; Arguments=""; WorkingDirectory="";WaitTimeoutSeconds=60; }
}
public class Configuration { public int Version=1; public List<Entry> Entries=new List<Entry>(); }
public static class Entries {
 static string Normalize(string path) {try{return Path.GetFullPath(Environment.ExpandEnvironmentVariables(path??"")).TrimEnd('\\','/');}catch{return path??"";}}
 static string TargetKey(Entry entry) {
  string target=entry.Target,arguments=entry.Arguments??"";
  if(String.Equals(Path.GetExtension(target),".lnk",StringComparison.OrdinalIgnoreCase)&&File.Exists(target)) {
   object shell=null,link=null;try{shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));dynamic s=shell;link=s.CreateShortcut(target);dynamic l=link;if(!String.IsNullOrWhiteSpace((string)l.TargetPath))target=(string)l.TargetPath;if(String.IsNullOrEmpty(arguments))arguments=(string)l.Arguments;}catch{}finally{if(link!=null)Marshal.FinalReleaseComObject(link);if(shell!=null)Marshal.FinalReleaseComObject(shell);}
  }
  var key=Normalize(target);
  if(new[]{"cmd","powershell","pwsh","wscript","cscript"}.Contains(Path.GetFileNameWithoutExtension(target).ToLowerInvariant()))key+="|"+arguments.Trim();
  return key;
 }
 public static Entry FindDuplicate(IEnumerable<Entry> list,Entry candidate) {
  string key=TargetKey(candidate);return list.FirstOrDefault(e=>e.Id!=candidate.Id&&(String.Equals(Normalize(e.Target),Normalize(candidate.Target),StringComparison.OrdinalIgnoreCase)||String.Equals(TargetKey(e),key,StringComparison.OrdinalIgnoreCase)));
 }
 // Slot is the boundary before a row, or Count for the end of the list.
 public static int MoveToSlot(Configuration config,string id,int slot) {
  int from=config.Entries.FindIndex(e=>e.Id==id);if(from<0||slot<0||slot>config.Entries.Count)return -1;
  int to=slot>from?slot-1:slot;if(to==from)return from;var entry=config.Entries[from];config.Entries.RemoveAt(from);config.Entries.Insert(to,entry);for(int i=0;i<config.Entries.Count;i++)config.Entries[i].Position=i;return to;
 }
}
public class Store {
 public readonly string PathName;
 public Store(string path) {PathName=path;}
 public static string Root {get {return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PandaCoustik","PandaLauncher");}}
 public Configuration Read() {
  if(!File.Exists(PathName)) return new Configuration();
  try {
   var c=new JavaScriptSerializer().Deserialize<Configuration>(File.ReadAllText(PathName));
   if(c==null||c.Version!=1||c.Entries==null) throw new Exception("Format ou version non reconnu.");
   var ids=new HashSet<string>();
   foreach(var e in c.Entries) if(e==null||String.IsNullOrWhiteSpace(e.Id)||!ids.Add(e.Id)||String.IsNullOrWhiteSpace(e.Name)||String.IsNullOrWhiteSpace(e.Target)||!Path.IsPathRooted(e.Target)||e.DelayMs<0||e.DelayMs>600000||e.WaitMode<0||e.WaitMode>2||e.WaitTimeoutSeconds<1||e.WaitTimeoutSeconds>600) throw new Exception("Entrée invalide.");
   c.Entries=c.Entries.OrderBy(e=>e.Position).ToList(); return c;
  } catch(Exception ex) {throw new IOException("Configuration illisible, conservée sans modification : "+PathName+"\n"+ex.Message,ex);}
 }
 public void Save(Configuration c) {
  Directory.CreateDirectory(Path.GetDirectoryName(PathName));
  for(int i=0;i<c.Entries.Count;i++) c.Entries[i].Position=i;
  string tmp=PathName+"."+Guid.NewGuid().ToString("N")+".tmp";
  try {var data=System.Text.Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(c));
   using(var f=new FileStream(tmp,FileMode.CreateNew,FileAccess.Write,FileShare.None)) {f.Write(data,0,data.Length);f.Flush(true);}
   if(File.Exists(PathName)) File.Replace(tmp,PathName,PathName+".bak"); else File.Move(tmp,PathName);
  } finally {if(File.Exists(tmp)) File.Delete(tmp);}
 }
}
public static class Journal {
 public static void Write(string text) {try {Directory.CreateDirectory(Store.Root); string p=Path.Combine(Store.Root,"launcher.log"); if(File.Exists(p)&&new FileInfo(p).Length>1048576) {File.Copy(p,p+".old",true);File.Delete(p);} File.AppendAllText(p,DateTime.Now.ToString("s")+" "+text+Environment.NewLine);} catch {} }
}
public class Resolved {public string Target,Arguments,Directory; public bool CanDetectRunning;}
public static class Shortcuts {
 public static bool Supported(string path) {return new[]{".exe",".lnk",".cmd",".bat",".ps1"}.Contains(Path.GetExtension(path).ToLowerInvariant());}
 public static Resolved Resolve(Entry e) {
  var r=new Resolved {Target=e.Target,Arguments=e.Arguments??"",Directory=e.WorkingDirectory??""};
  if(!File.Exists(e.Target)) throw new IOException("Fichier introuvable : "+e.Target);
  if(String.Equals(Path.GetExtension(e.Target),".lnk",StringComparison.OrdinalIgnoreCase)) {
   object shell=null,link=null;
   try {shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));dynamic s=shell;link=s.CreateShortcut(e.Target);dynamic l=link;
    r.Target=(string)l.TargetPath; if(String.IsNullOrEmpty(e.Arguments)) r.Arguments=(string)l.Arguments;
    if(String.IsNullOrEmpty(e.WorkingDirectory)) r.Directory=(string)l.WorkingDirectory;
   } finally {if(link!=null) Marshal.FinalReleaseComObject(link);if(shell!=null) Marshal.FinalReleaseComObject(shell);}
  }
  r.Target=Environment.ExpandEnvironmentVariables(r.Target??"");
  if(!File.Exists(r.Target)) throw new IOException("Cible introuvable ou raccourci non pris en charge : "+r.Target);
  string ext=Path.GetExtension(r.Target).ToLowerInvariant();
  if(!new[]{".exe",".cmd",".bat",".ps1"}.Contains(ext)) throw new IOException("Cible non prise en charge : choisissez un .exe, .cmd, .bat ou .ps1, directement ou via un .lnk.");
  r.CanDetectRunning=ext==".exe"&&!new[]{"cmd","powershell","pwsh","wscript","cscript"}.Contains(Path.GetFileNameWithoutExtension(r.Target).ToLowerInvariant());
  if(String.IsNullOrWhiteSpace(r.Directory)) r.Directory=Path.GetDirectoryName(r.Target);
  r.Directory=Environment.ExpandEnvironmentVariables(r.Directory);
  if(!System.IO.Directory.Exists(r.Directory)) throw new IOException("Dossier de travail introuvable : "+r.Directory);
  // Never allow OBS launch flags to start a broadcast or recording.
  if(Path.GetFileNameWithoutExtension(r.Target).StartsWith("obs",StringComparison.OrdinalIgnoreCase)&&System.Text.RegularExpressions.Regex.IsMatch(r.Arguments,@"--start(streaming|recording|replaybuffer|virtualcam)\b",System.Text.RegularExpressions.RegexOptions.IgnoreCase)) throw new IOException("Les arguments de démarrage automatique OBS ne sont pas autorisés.");
  return r;
 }
 public static string Create(string exe,string destination) {
  object shell=null,link=null;try {shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));dynamic s=shell;link=s.CreateShortcut(destination);dynamic l=link;l.TargetPath=exe;l.Arguments="--launch";l.WorkingDirectory=Path.GetDirectoryName(exe);l.Description="Ouvrir les logiciels configurés dans Panda Launcher";l.IconLocation=LaunchIcon(exe);l.Save();return destination;}finally{if(link!=null)Marshal.FinalReleaseComObject(link);if(shell!=null)Marshal.FinalReleaseComObject(shell);}
 }
 static string LaunchIcon(string exe) {
  using(var source=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("launch.ico")) {
   if(source==null)return exe+",0";
   string path=Path.Combine(Store.Root,"icons","panda-launch-v2.ico");System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path));
   using(var memory=new MemoryStream()){source.CopyTo(memory);var bytes=memory.ToArray();if(!File.Exists(path)||!File.ReadAllBytes(path).SequenceEqual(bytes))File.WriteAllBytes(path,bytes);}return path+",0";
  }
 }
}
public class LaunchLock : IDisposable {
 private Mutex mutex;private bool owned;
 public LaunchLock(string name) {mutex=new Mutex(false,name);try {owned=mutex.WaitOne(0);}catch(AbandonedMutexException){owned=true;}}
 public bool Acquired {get{return owned;}}
 public void Dispose(){if(owned)mutex.ReleaseMutex();mutex.Dispose();}
}
public static class Launcher {
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] static extern int GetPackageFamilyName(IntPtr process,ref uint length,System.Text.StringBuilder family);
 public static string PackageFamily(int id){IntPtr h=OpenProcess(0x1000,false,id);if(h==IntPtr.Zero)return null;try{uint n=512;var b=new System.Text.StringBuilder((int)n);return GetPackageFamilyName(h,ref n,b)==0?b.ToString():null;}finally{CloseHandle(h);}}
 public const string LockName=@"Local\PandaCoustik.PandaLauncher.Sequence.v1";
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool QueryFullProcessImageName(IntPtr h,int flags,System.Text.StringBuilder text,ref int size);
 [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr OpenProcess(int access,bool inherit,int id);
 [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr h);
 [DllImport("user32.dll",SetLastError=true)] static extern IntPtr SendMessageTimeout(IntPtr hwnd,uint message,IntPtr wparam,IntPtr lparam,uint flags,uint timeout,out IntPtr result);
 [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
 public static string ProcessPath(int id){IntPtr h=OpenProcess(0x1000,false,id);if(h==IntPtr.Zero)return null;try{var b=new System.Text.StringBuilder(32768);int n=b.Capacity;return QueryFullProcessImageName(h,0,b,ref n)?Path.GetFullPath(b.ToString()):null;}finally{CloseHandle(h);}}
 public static bool ResponsiveWindow(string target){var identity=ExecutionIdentity.Read(target);foreach(var name in identity.ProcessNames)foreach(var p in Process.GetProcessesByName(name))using(p){try{if(!identity.Matches(ProcessPath(p.Id),identity.Family==null?null:PackageFamily(p.Id)))continue;IntPtr hwnd=p.MainWindowHandle,result;if(hwnd!=IntPtr.Zero&&IsWindowVisible(hwnd)&&SendMessageTimeout(hwnd,0,IntPtr.Zero,IntPtr.Zero,2,200,out result)!=IntPtr.Zero)return true;}catch(InvalidOperationException){}catch(System.ComponentModel.Win32Exception){}}return false;}
 public static async Task<bool> WaitForReady(Resolved target,int mode,int timeoutSeconds,Action<string> progress){
  if(mode<1||mode>2||timeoutSeconds<1||timeoutSeconds>600)throw new IOException("Paramètres d’attente invalides.");
  if(!target.CanDetectRunning)throw new IOException("Attente indisponible pour un script ou un interpréteur. Utilisez un délai fixe.");
  var timer=Stopwatch.StartNew();int stable=0;while(timer.Elapsed.TotalSeconds<timeoutSeconds){bool ready=await Task.Run(()=>{bool uncertain;return IsRunning(target.Target,out uncertain)&&(mode==1||ResponsiveWindow(target.Target));});stable=ready?stable+1:0;if(stable>=(mode==2?2:1))return true;progress("Attente "+(mode==1?"du processus":"d’une fenêtre réactive")+" · "+(int)timer.Elapsed.TotalSeconds+" / "+timeoutSeconds+" s");await Task.Delay(400);}return false;
 }
 public static bool IsRunning(string target,out bool uncertain) {
  uncertain=false;var identity=ExecutionIdentity.Read(target);
  foreach(var name in identity.ProcessNames)foreach(var p in Process.GetProcessesByName(name)) using(p) {
   try{string path=ProcessPath(p.Id);if(path==null){uncertain=true;continue;}if(identity.Matches(path,identity.Family==null?null:PackageFamily(p.Id)))return true;}catch(InvalidOperationException){}catch(System.ComponentModel.Win32Exception){uncertain=true;}
 }return false;
 }
 public static ProcessStartInfo BuildStart(Resolved r,bool administrator) {
  string target=r.Target,arguments=r.Arguments;string ext=Path.GetExtension(target).ToLowerInvariant();
  if(ext==".cmd"||ext==".bat") {arguments="/d /s /c \"\""+target+"\" "+arguments+"\"";target=Path.Combine(Environment.SystemDirectory,"cmd.exe");}
  else if(ext==".ps1") {arguments="-NoLogo -NoProfile -File \""+target+"\" "+arguments;target=Path.Combine(Environment.SystemDirectory,@"WindowsPowerShell\v1.0\powershell.exe");}
  var start=new ProcessStartInfo(target,arguments){WorkingDirectory=r.Directory,UseShellExecute=true};if(administrator)start.Verb="runas";return start;
 }
 public static async Task<int> Run(Configuration c,Action<Entry,string,bool> report) {
  int errors=0;var list=c.Entries.Where(e=>e.Enabled).OrderBy(e=>e.Position).ToList();
  for(int i=0;i<list.Count;i++) {var e=list[i];try {
   var r=Shortcuts.Resolve(e);bool uncertain=false;
   if(e.WaitMode!=0&&!r.CanDetectRunning)throw new IOException("Attente indisponible pour cette cible. Choisissez un délai fixe dans les options avancées.");
   if(e.SkipRunning&&r.CanDetectRunning&&IsRunning(r.Target,out uncertain)) report(e,"Déjà ouvert",false);
   else if(e.SkipRunning&&uncertain) throw new IOException("Détection incertaine : accès au processus refusé. Désactivez « Ne pas relancer » pour autoriser son lancement.");
   else {using(var p=Process.Start(BuildStart(r,e.Administrator))){} report(e,r.CanDetectRunning?"Demande de lancement envoyée":"Demande envoyée · détection indisponible pour ce script",false);}
   if(e.WaitMode!=0){if(!await WaitForReady(r,e.WaitMode,e.WaitTimeoutSeconds,state=>report(e,state,false)))throw new IOException("Délai d’attente dépassé ("+e.WaitTimeoutSeconds+" s). La suite du groupe n’a pas été lancée.");report(e,e.WaitMode==2?"Logiciel lancé · fenêtre réactive détectée":"Logiciel lancé · processus détecté",false);}
  }catch(Exception ex){errors++;report(e,"Erreur : "+ex.Message,true);Journal.Write(e.Name+" : "+ex.Message);if(e.WaitMode!=0){for(int j=i+1;j<list.Count;j++)report(list[j],"Non lancé : attente de « "+e.Name+" » non satisfaite",true);return errors;}}
   if(i<list.Count-1&&e.DelayMs>0) await Task.Delay(e.DelayMs);
  }return errors;
}
}
public enum RuntimeKind {Running,Stopped,Unknown,Unsupported,Error}
public class RuntimeState {public RuntimeKind Kind;public string Text;}
public static class RuntimeProbe {
 public static RuntimeState Read(Entry entry){try{var r=Shortcuts.Resolve(entry);if(!r.CanDetectRunning)return new RuntimeState{Kind=RuntimeKind.Unsupported,Text="État non vérifiable (script)"};bool uncertain;bool running=Launcher.IsRunning(r.Target,out uncertain);return new RuntimeState{Kind=running?RuntimeKind.Running:uncertain?RuntimeKind.Unknown:RuntimeKind.Stopped,Text=running?"Logiciel lancé · processus détecté":uncertain?"État inconnu · accès refusé":"Non ouvert"};}catch(Exception ex){return new RuntimeState{Kind=RuntimeKind.Error,Text=ex.Message};}}
}
}
