using System;using System.Drawing;using System.IO;using System.Windows.Forms;
namespace PandaLauncher {
public class GroupDialog : Form {
 public string GroupName,ShortcutName,ColorKey;TextBox groupName=new TextBox(),shortcutName=new TextBox();CheckBox closeEnabled=new CheckBox();bool customName,updating;RadioButton[] choices=new RadioButton[5];Label error=new Label();
 public GroupDialog(Group group,string currentShortcut,bool createShortcut,Action<string,string,string> commit):this(group,currentShortcut,(name,color,enabled,close)=>commit(name,name,color)){}
 public GroupDialog(Group group,string currentClose,Action<string,string,bool,string> commit){
  Text="Paramètres du launcher";ClientSize=new Size(680,570);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;MinimizeBox=false;StartPosition=FormStartPosition.CenterParent;
  var root=new FlowLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(22),FlowDirection=FlowDirection.TopDown,WrapContents=false};Controls.Add(root);
  root.Controls.Add(new Label{Text="Nom du launcher",AutoSize=true});groupName.Width=604;groupName.MaxLength=90;groupName.Text=group.Name;root.Controls.Add(groupName);
  root.Controls.Add(new Label{Text="Utilisé pour l'onglet et le raccourci sur le Bureau.",AutoSize=true});
  root.Controls.Add(new Label{Text="Couleur de l'icône",AutoSize=true,Margin=new Padding(0,16,0,4)});
  var icons=new FlowLayoutPanel{Width=610,Height=125,WrapContents=false};root.Controls.Add(icons);
  for(int i=0;i<5;i++){var option=new Panel{Width=114,Height=122,Margin=new Padding(0,0,7,0)};int index=i;var picture=new PictureBox{Image=Theme.RunLogo(GroupLinks.Colors[i]),SizeMode=PictureBoxSizeMode.Zoom,Size=new Size(100,86),Location=new Point(7,0),Cursor=Cursors.Hand};choices[i]=new RadioButton{Text=GroupLinks.Labels[i],AutoSize=true,Location=new Point(8,91),Checked=group.Icon==GroupLinks.Colors[i]};choices[i].CheckedChanged+=(s,e)=>{if(choices[index].Checked)for(int j=0;j<5;j++)if(j!=index)choices[j].Checked=false;};picture.Click+=(s,e)=>choices[index].Checked=true;option.Controls.Add(picture);option.Controls.Add(choices[i]);icons.Controls.Add(option);}
  closeEnabled.Text="Créer aussi un raccourci pour fermer les applications";closeEnabled.AutoSize=true;closeEnabled.Margin=new Padding(0,14,0,6);closeEnabled.Checked=group.CloseShortcutEnabled??(currentClose!=null);root.Controls.Add(closeEnabled);
  var closeLabel=new Label{Text="Nom du raccourci de fermeture",AutoSize=true};root.Controls.Add(closeLabel);shortcutName.Width=604;shortcutName.MaxLength=100;root.Controls.Add(shortcutName);
  string existing=group.CloseShortcutName??(currentClose==null?null:Path.GetFileNameWithoutExtension(currentClose));customName=existing!=null&&(group.CloseNameCustom||existing!=GroupLinks.DefaultCloseName(group.Name));shortcutName.Text=customName?existing:GroupLinks.DefaultCloseName(group.Name);
  Action refresh=()=>{closeLabel.Visible=shortcutName.Visible=closeEnabled.Checked;};closeEnabled.CheckedChanged+=(s,e)=>refresh();refresh();
  groupName.TextChanged+=(s,e)=>{if(!customName){updating=true;shortcutName.Text=GroupLinks.DefaultCloseName(groupName.Text.Trim());updating=false;}};
  shortcutName.TextChanged+=(s,e)=>{if(!updating)customName=shortcutName.Text!=GroupLinks.DefaultCloseName(groupName.Text.Trim());};
  root.Controls.Add(new Label{Text="Enregistrer conserve les options. Le bouton des raccourcis les applique au Bureau.",AutoSize=false,Size=new Size(610,40),Margin=new Padding(0,10,0,0)});
  error.AutoSize=false;error.Size=new Size(605,44);root.Controls.Add(error);
  var buttons=new FlowLayoutPanel{Width=610,Height=48};root.Controls.Add(buttons);
  buttons.Controls.Add(Theme.Button("Enregistrer",()=>{try{if(String.IsNullOrWhiteSpace(groupName.Text))throw new IOException("Indiquez un nom de launcher.");GroupLinks.ValidateName(groupName.Text.Trim());if(closeEnabled.Checked)GroupLinks.ValidateName(shortcutName.Text.Trim());GroupName=groupName.Text.Trim();ShortcutName=shortcutName.Text.Trim();ColorKey=group.Icon;for(int i=0;i<5;i++)if(choices[i].Checked)ColorKey=GroupLinks.Colors[i];commit(GroupName,ColorKey,closeEnabled.Checked,ShortcutName);DialogResult=DialogResult.OK;}catch(Exception ex){error.Text=ex.Message;}}));buttons.Controls.Add(Theme.Button("Annuler",()=>DialogResult=DialogResult.Cancel));Theme.Apply(this);error.ForeColor=Color.FromArgb(182,48,65);Theme.Primary((Button)buttons.Controls[0]);
 }
}
}


