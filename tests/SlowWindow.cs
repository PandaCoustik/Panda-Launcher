using System;using System.IO;using System.Threading;using System.Windows.Forms;using System.Drawing;
class SlowWindow {
 [STAThread]static void Main(string[] args){int delay=int.Parse(args[1]),hold=int.Parse(args[2]);Thread.Sleep(delay);if(args.Length>3&&args[3]=="headless"){File.WriteAllText(args[0],"ready");Thread.Sleep(hold);return;}using(var form=new Form{Text="Panda test",ShowInTaskbar=false,StartPosition=FormStartPosition.Manual,Location=new Point(-2000,-2000),Size=new Size(160,90)})using(var timer=new System.Windows.Forms.Timer{Interval=hold}){form.Shown+=(s,e)=>{File.WriteAllText(args[0],DateTime.UtcNow.ToString("O"));timer.Start();if(args.Length>3&&args[3]=="hidden")form.Hide();};bool automatic=false;form.FormClosing+=(s,e)=>{if(args.Length>3&&args[3]=="refuse"&&!automatic)e.Cancel=true;};timer.Tick+=(s,e)=>{automatic=true;form.Close();};Application.Run(form);}}
}

