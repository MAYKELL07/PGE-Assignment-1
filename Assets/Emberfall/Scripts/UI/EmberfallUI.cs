using UnityEngine;
using UnityEngine.SceneManagement;
namespace Emberfall {
public partial class EmberfallGame {
    GUIStyle parchment,slate,wood,slot; Font pixel;
    readonly Color brown=new Color(.29f,.20f,.16f);
    Texture2D UI(int id){var t=Resources.Load<Texture2D>("Interface/tile_"+id.ToString("0000"));if(t)t.filterMode=FilterMode.Point;return t;}
    GUIStyle Frame(int tile){return new GUIStyle{normal={background=UI(tile)},border=new RectOffset(10,10,10,10)};}
    void Styles(){if(label!=null)return;white=Texture2D.whiteTexture;pixel=Resources.Load<Font>("Interface/Pixel");
        label=new GUIStyle{font=pixel,fontSize=26,normal={textColor=brown}};
        small=new GUIStyle(label){fontSize=22};title=new GUIStyle(label){fontSize=64,alignment=TextAnchor.MiddleCenter};
        parchment=Frame(6);slate=Frame(3);wood=Frame(4);slot=Frame(13);
        button=new GUIStyle(wood){font=pixel,fontSize=30,alignment=TextAnchor.MiddleCenter,normal={background=UI(4),textColor=cream},hover={background=UI(17),textColor=Color.white},active={background=UI(1),textColor=amber}};
    }
    void Box(float x,float y,float w,float h,Color c){GUI.color=c;GUI.DrawTexture(new Rect(x,y,w,h),white);GUI.color=Color.white;}
    void Panel(float x,float y,float w,float h,GUIStyle style=null){GUI.Box(new Rect(x,y,w,h),GUIContent.none,style??parchment);}
    void Text(float x,float y,string s,GUIStyle style=null,float width=700){GUI.Label(new Rect(x,y,width,180),s,style??label);}
    void Center(float x,float y,float w,float h,string s,int size,Color color){GUI.Label(new Rect(x,y,w,h),s,new GUIStyle(label){fontSize=size,alignment=TextAnchor.MiddleCenter,normal={textColor=color}});}
    bool Button(float x,float y,float w,string s){bool pressed=GUI.Button(new Rect(x,y,w,54),s,button);if(pressed)ProjectAudio.Instance?.PlayClick();return pressed;}
    void Icon(int id,float x,float y,float size){var s=Sprite(id);GUI.DrawTexture(new Rect(x,y,size,size),s.texture,ScaleMode.ScaleToFit,true);}
    void Ribbon(float x,float y,float w,string text){GUI.DrawTexture(new Rect(x,y,48,64),UI(43));GUI.DrawTexture(new Rect(x+48,y,w-96,64),UI(44));GUI.DrawTexture(new Rect(x+w-48,y,48,64),UI(45));Center(x,y+1,w,54,text,30,cream);}
    void Bar(float x,float y,float w,float frac,Color color){Panel(x,y,w,20,slate);Box(x+6,y+6,(w-12)*Mathf.Clamp01(frac),8,color);}
    void Action(float x,int icon,string key,string name,float cooldown,System.Action action){Panel(x,615,80,78,slot);Icon(icon,x+20,625,40);Center(x+5,666,70,20,key,21,brown);if(cooldown>0){Box(x+9,623,62,42,new Color(.12f,.10f,.13f,.7f));Center(x,625,80,38,cooldown.ToString("0.0"),28,cream);}if(GUI.Button(new Rect(x,615,80,78),GUIContent.none,GUIStyle.none)&&cooldown<=0&&!paused&&!dialog&&!won&&!dead)action();Center(x-5,697,90,20,name,19,cream);}
    void OnGUI(){if(!hero)return;Styles();float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1280*scale)/2,(Screen.height-720*scale)/2,0),Quaternion.identity,Vector3.one*scale);
        if(started){
            Panel(22,20,320,113);Panel(35,32,74,78,slot);Icon(96,43,38,60);Text(120,32,"WARDEN  /  LV "+level,small);Bar(120,63,199,hp/MaxHP,new Color(.87f,.32f,.3f));Center(120,84,199,25,Mathf.CeilToInt(hp)+" / "+MaxHP+" HP",21,brown);Box(120,113,199*(float)xp/(level*60),4,new Color(.25f,.61f,.58f));
            Panel(946,20,312,140);Center(960,31,282,29,"RESTORE THE FLAME",26,brown);for(int i=0;i<3;i++){GUI.color=collected[i]?Color.white:new Color(.55f,.5f,.48f);Icon(56,974+i*45,72,32);GUI.color=Color.white;}Icon(101,1135,73,28);Text(1168,72,gold.ToString(),label,65);Center(960,115,282,26,sigils<3?"Find the three ancient sigils":"The northern throne awaits",21,brown);
            Panel(461,20,358,46,wood);Center(465,23,350,37,names[CurrentRoom],23,cream);
            Panel(1112,176,146,145,slate);Box(1140,267,89,3,new Color(.55f,.60f,.64f));Box(1183,208,3,61,new Color(.55f,.60f,.64f));for(int i=0;i<5;i++){float x=1185+centers[i].x*2.4f,y=278-centers[i].y*2.1f;Box(x-8,y-6,16,12,i==CurrentRoom?amber:i>0&&i<4&&collected[i-1]?teal:new Color(.48f,.56f,.60f));}Box(1183+hero.position.x*2.4f,276-hero.position.y*2.1f,4,4,Color.white);
            Action(449,106,"J / CLICK","SLASH",attackCD,Attack);Action(549,102,"SPACE","DODGE",dashCD,()=>{dashCD=1.15f;dashTime=.16f;invulnerable=.25f;dashDirection=facing;});Action(649,127,"Q","NOVA",spellCD,Cast);Action(749,115,"H  x"+potions,"HEAL",0,Heal);
            Center(24,665,230,36,"WASD / ARROWS  Move",22,cream);Center(1000,665,250,36,"E  Interact    ESC  Pause",22,cream);
            if(messageTime>0&&!dialog){Panel(300,555,680,43,wood);Center(315,557,650,37,message,22,cream);}
            foreach(var f in foes){if(!f.body||f.hp==f.maxHp)continue;Vector3 v=cam.WorldToScreenPoint(f.body.position+Vector3.up*(f.boss?1.4f:.7f));float x=(v.x-(Screen.width-1280*scale)/2)/scale,y=(Screen.height-v.y-(Screen.height-720*scale)/2)/scale;Bar(x-30,y,60,f.hp/f.maxHp,new Color(.85f,.3f,.3f));}
            foreach(var f in floaters){var v=cam.WorldToScreenPoint(f.pos);Center((v.x-(Screen.width-1280*scale)/2)/scale-75,(Screen.height-v.y-(Screen.height-720*scale)/2)/scale-25,150,30,f.text,28,f.color);}
            if(!paused&&!dead&&!won&&!dialog){string hint="";if(Vector2.Distance(hero.position,new Vector2(-2,1.5f))<2)hint="E  Talk to Keeper Orin";else if(Vector2.Distance(hero.position,new Vector2(2,1.5f))<1.8f)hint="E  Rest at the hearth";else foreach(var p in pickups)if(p.type>=2&&Vector2.Distance(hero.position,p.position)<1.5f)hint=p.type==2?"E  Claim sigil":"E  Open supply chest";if(hint!=""){Panel(440,502,400,43,wood);Center(450,505,380,34,hint,25,cream);}}
        }
        if(!started){Overlay();Panel(310,116,660,494);Ribbon(420,84,440,"A TINY TALE OF COURAGE");Center(345,166,590,96,"EMBERFALL",88,brown);Center(370,261,540,36,"THE LAST LIGHT OF THE KINGDOM",26,brown);
            Icon(106,428,329,48);Icon(56,613,329,48);Icon(124,798,329,48);Center(362,383,180,30,"MASTER YOUR BLADE",20,brown);Center(551,383,180,30,"CLAIM THREE SIGILS",20,brown);Center(738,383,180,30,"DEFEAT THE KING",20,brown);
            if(Button(475,438,330,"BEGIN ADVENTURE"))Begin();
            if(Button(475,502,330,"OPEN PHYSICS LAB"))SceneManager.LoadScene("PhysicsLab");
            Center(370,562,540,24,"WASD Move  /  J Slash  /  SPACE Dodge",21,brown);Center(370,587,540,20,"Art + interface + font by Kenney  /  CC0",18,brown);
        }
        if(dialog){Overlay();Panel(315,143,650,425);Ribbon(432,111,416,"KEEPER ORIN");Icon(84,350,202,78);Text(453,210,dialogText,new GUIStyle(label){fontSize=24,wordWrap=true},465);if(Button(350,472,290,"TEMPER BLADE / 20 GOLD"))dialogText=BuyUpgrade()?"Your blade burns brighter. Each strike now deals 10 more damage.\n\nWatch for the king's red wind-up, then dodge!":"You need 20 gold. Search the supply chests and defeated guardians.";if(Button(657,472,270,"BACK TO ADVENTURE"))dialog=false;}
        if(paused){Overlay();Panel(425,205,430,310);Ribbon(454,173,372,"AT REST");Icon(96,610,264,60);if(Button(490,360,300,"RESUME ADVENTURE"))paused=false;Center(445,445,390,32,"ESC also resumes your adventure",22,brown);}
        if(dead||won){Overlay();Panel(335,175,610,365);Ribbon(380,143,520,won?"THE FLAME RETURNS":"A FALLEN WARDEN");Icon(won?56:96,606,238,68);Center(355,321,570,40,won?"The hearth burns again. You brought the light home.":"The crypt claimed you. Rise and try again.",25,brown);Center(355,366,570,40,"LEVEL "+level+"    /    "+kills+" FOES    /    "+gold+" GOLD",26,brown);if(Button(490,438,300,won?"PLAY AGAIN":"TRY AGAIN"))Restart();}
    }
    void Overlay(){Box(-1000,-1000,3280,2720,new Color(.025f,.022f,.035f,.8f));}
}
}
