using System;using System.IO;using System.Linq;using System.Drawing;using System.Reflection;using System.Windows.Forms;using System.Runtime.InteropServices;using PandaLauncher;
class RegressionV3 {
 static int count;
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);Console.WriteLine("OK : "+text);count++;}
 static object Field(object o,string name){return o.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(o);}
 static object Call(object o,string name,params object[] args){return o.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(o,args);}
 static void Drop(MainForm f,DataGridView grid,string id,Point point){f.GetType().GetField("dragId",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(f,id);var data=new DataObject();data.SetData(typeof(string),id);var screen=grid.PointToScreen(point);Call(f,"HandleDrop",grid,new DragEventArgs(data,0,screen.X,screen.Y,DragDropEffects.Move,DragDropEffects.Move));}
 [STAThread]static void Main(string[] args){Application.EnableVisualStyles();string root=Path.GetFullPath(args[0]);Directory.CreateDirectory(root);string script=Path.Combine(root,"Panda été.cmd");File.WriteAllText(script,"@echo off\r\n");var original=new Entry{Name="PANDAURA",Target=script,Enabled=false};var list=new[]{original};
 Check(Entries.FindDuplicate(list,new Entry{Name="Autre nom",Target=script.ToUpperInvariant()})==original,"doublon malgré renommage, casse et désactivation");
 Check(Entries.FindDuplicate(list,original)==null,"modification de sa propre entrée autorisée");
 Check(Entries.FindDuplicate(list,new Entry{Name="PANDAURA",Target=Path.Combine(root,"autre.cmd")})==null,"homonyme avec cible différente autorisé");
 object shell=Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));dynamic sh=shell;string link=Path.Combine(root,"Panda.lnk"),alias=Path.Combine(root,"Alias.lnk");foreach(string path in new[]{link,alias}){dynamic l=sh.CreateShortcut(path);l.TargetPath=script;l.Save();Marshal.FinalReleaseComObject(l);}Marshal.FinalReleaseComObject(shell);
 Check(Entries.FindDuplicate(list,new Entry{Name="Lien",Target=link})==original,"raccourci et script direct reconnus comme doublon");
 Check(Entries.FindDuplicate(new[]{new Entry{Name="Premier lien",Target=link}},new Entry{Name="Second lien",Target=alias})!=null,"deux raccourcis distincts vers une même cible");
 var store=new CatalogStore(Path.Combine(root,"config.json"));var catalog=new Catalog();var config=catalog.Add("Test");config.Entries.Add(original);config.Entries.Add(new Entry{Name="Deux",Target=Path.Combine(root,"deux.exe")});config.Entries.Add(new Entry{Name="Trois",Target=Path.Combine(root,"trois.exe")});store.Save(catalog);
 using(var f=new MainForm(store)){f.Show();Application.DoEvents();var grid=(DataGridView)Field(f,"grid");
  var last=grid.GetRowDisplayRectangle(2,false);Drop(f,grid,original.Id,new Point(150,last.Bottom-3));Check(store.Read().Selected.Entries.Last().Id==original.Id,"dépôt vers le bas enregistré");
  var first=grid.GetRowDisplayRectangle(0,false);Drop(f,grid,original.Id,new Point(150,first.Top+2));Check(store.Read().Selected.Entries.First().Id==original.Id,"dépôt vers le haut enregistré");
  Drop(f,grid,original.Id,new Point(150,first.Top+2));Check(store.Read().Selected.Entries.Count==3&&store.Read().Selected.Entries.First().Id==original.Id,"dépôt à la même place sans duplication");
  Check(store.Read().Selected.Entries.Select((e,i)=>e.Position==i).All(x=>x),"positions persistées après glisser-déposer");
  Check(((string)Call(f,"ValidateEntry",new Entry{Name="Doublon",Target=link})).Contains("désactivée"),"message de validation identifie le doublon désactivé");
  var buttons=((FlowLayoutPanel)Field(f,"commands")).Controls.OfType<Button>().Take(2).ToArray();
  foreach(var button in buttons)foreach(string state in new[]{"normal","survol","appui","désactivé"}){button.Enabled=state!="désactivé";if(state=="survol")Call(button,"OnMouseEnter",EventArgs.Empty);if(state=="appui")Call(button,"OnMouseDown",new MouseEventArgs(MouseButtons.Left,1,5,5,0));using(var b=new Bitmap(button.Width,button.Height)){button.DrawToBitmap(b,new Rectangle(Point.Empty,button.Size));Check(b.GetPixel(0,0).ToArgb()==button.Parent.BackColor.ToArgb()&&b.GetPixel(button.Width-1,button.Height-1).ToArgb()==button.Parent.BackColor.ToArgb(),"angles propres "+button.Text+" / "+state);}Call(button,"OnMouseLeave",EventArgs.Empty);button.Enabled=true;}
  f.Close();
 }
 Console.WriteLine(count+" vérifications v3 réussies.");
 }
}
