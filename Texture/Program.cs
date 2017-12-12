using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Urho;
using Urho.Actions;
using Urho.SharpReality;
using Urho.Shapes;
using Urho.Resources;
using Urho.Gui;
using System.Diagnostics;
using Urho.Physics;

namespace Texture
{
    internal class Program
    {
        [MTAThread]
        static void Main()
        {
            var appViewSource = new UrhoAppViewSource<HelloWorldApplication>(new ApplicationOptions("Data"));
            appViewSource.UrhoAppViewCreated += OnViewCreated;
            CoreApplication.Run(appViewSource);
        }

        static void OnViewCreated(UrhoAppView view)
        {
            view.WindowIsSet += View_WindowIsSet;
        }

        static void View_WindowIsSet(Windows.UI.Core.CoreWindow coreWindow)
        {
            // you can subscribe to CoreWindow events here
        }
    }

    public class HelloWorldApplication : StereoApplication
    {
        Node boardNode;
        Node pion1Node;
        Node pion2Node;
        Node diceNode;
        Node textNode, textNode2;
        float xi = -0.40f;
        float yi = -0.44f;
        float zi = -1f;
        //float i = 0.001f;
        List<Vector2> boardModel = new List<Vector2>();
        Vector2 initPos = new Vector2(-0.40f, -0.40f);
        int currentPos1 = 0;
        int currentPos2 = 0;
        float d = 0.075f;
        int curPlay1 = 1;
        

        //float initScale = 0.15f;


        public HelloWorldApplication(ApplicationOptions opts) : base(opts) { }

        protected override async void Start()
        {
            // Create a basic scene, see StereoApplication
            base.Start();
            initBoard();

            // Enable input
            EnableGestureManipulation = true;
            EnableGestureTapped = true;

            //board
            boardNode = Scene.CreateChild();
            boardNode.Position = new Vector3(0, 0, 2f); //1.5m away
            boardNode.SetScale(0f); //D=30cm
            boardNode.Scale = new Vector3(0.4f, 0.4f,0.4f);
            //boardNode.Rotation = new Quaternion((160.0f), 30.0f, 30.0f);
            // Scene has a lot of pre-configured components, such as Cameras (eyes), Lights, etc.
            DirectionalLight.Brightness = 1f;
            DirectionalLight.Node.SetDirection(new Vector3(-1, 0, 0.5f));

            //Sphere is just a StaticModel component with Sphere.mdl as a Model.
            var board = boardNode.CreateComponent<StaticModel>();
            board.Model = CoreAssets.Models.Box;
            board.Material = Material.FromImage("Textures/Board.jpg");
            //==========================================
            
            diceNode = boardNode.CreateChild();
            diceNode.SetScale(0.27f); //27% of the Earth's size
            diceNode.Position = new Vector3(1.2f, 0, 0);

            //var dice = diceNode.CreateComponent<StaticModel>();
            //dice.Model = ResourceCache.GetModel("Models/dadu1.mdl");
            //dice.SetMaterial(ResourceCache.GetMaterial("Materials/Transparant_Plastic.001.xml"));
            //=============================================
            pion1Node = boardNode.CreateChild();
            pion1Node.SetScale(0.04f);
            pion1Node.Position = new Vector3(initPos.X, initPos.Y, -1);
            pion1Node.Rotation = new Quaternion((-60.0f), 270.0f, 90.0f);

            var pion1 = pion1Node.CreateComponent<StaticModel>();
            pion1.Model = ResourceCache.GetModel("Models/King.mdl");
            
            //===============================================
             pion2Node = boardNode.CreateChild();
            pion2Node.SetScale(0.04f);
            pion2Node.Position = new Vector3(initPos.X, initPos.Y, -1);
            pion2Node.Rotation = new Quaternion((-60.0f), 270.0f, 90.0f);

            var pion2 = pion2Node.CreateComponent<StaticModel>();
            pion2.Model = ResourceCache.GetModel("Models/Queen.mdl");
            pion2.SetMaterial(ResourceCache.GetMaterial("Materials/kobochan.003.xml"));

            //=======================================
            var rumahNode = boardNode.CreateChild();
            rumahNode.SetScale(0.10f);
            rumahNode.Position = new Vector3(1f, -3.9f, 1f);
            rumahNode.Rotation = new Quaternion((40.0f), 0.0f, 180.0f);

            var rumah = rumahNode.CreateComponent<StaticModel>();
            rumah.Model = ResourceCache.GetModel("Models/Rumah.mdl");
            rumah.SetMaterial(ResourceCache.GetMaterial("Materials/kobochan.003.xml"));

            //======================================
            var hotelNode = boardNode.CreateChild();
            hotelNode.SetScale(0.1f);
            hotelNode.Position = new Vector3(1f, 1f, -1f);
            hotelNode.Rotation = new Quaternion((40.0f), 0.0f, 180.0f);

            var hotel = hotelNode.CreateComponent<StaticModel>();
            hotel.Model = ResourceCache.GetModel("Models/Hotel.mdl");
            hotel.SetMaterial(ResourceCache.GetMaterial("Materials/Plastic.xml"));

            textNode = boardNode.CreateChild();
            var text3D = textNode.CreateComponent<Text3D>();
            text3D.HorizontalAlignment = HorizontalAlignment.Center;
            text3D.VerticalAlignment = VerticalAlignment.Center;
            text3D.ViewMask = 0x80000000;
            text3D.Text = "Dadu";
            text3D.SetFont(CoreAssets.Fonts.AnonymousPro, 36);
            text3D.SetColor(Color.Black);
            textNode.Translate(new Vector3(-0.1f, 0.1f, -1f));

            textNode2 = boardNode.CreateChild();
            var text3D2 = textNode2.CreateComponent<Text3D>();
            text3D2.HorizontalAlignment = HorizontalAlignment.Center;
            text3D2.VerticalAlignment = VerticalAlignment.Center;
            text3D2.ViewMask = 0x80000000;
            text3D2.Text = ".";
            text3D2.SetFont(CoreAssets.Fonts.AnonymousPro, 36);
            text3D2.SetColor(Color.Black);
            textNode2.Translate(new Vector3(0.1f, 0.1f, -1f));

            // Run a few actions to spin the Earth, the Moon and the clouds.

            //boardNode.RunActions(new RepeatForever(new RotateBy(duration: 1f, deltaAngleX: 2, deltaAngleY: 0, deltaAngleZ: 0)));
            //await TextToSpeech("Hello world from UrhoSharp!");

            // More advanced samples can be found here:
            // https://github.com/xamarin/urho-samples/tree/master/HoloLens

            await RegisterCortanaCommands(new Dictionary<string, Action> {
               // { "he", () =>pion1Node.Position =  new Vector3 (xi *1, yi += 0.08f, zi * 1)}, //atas
                //{ "people", () =>pion1Node.Position =  new Vector3 (x +=0.08f, y, z *1)}, //kanan
                //{ "room", () =>pion1Node.Position =  new Vector3 (x *1, y -= 0.08f, z *1)}, //bawah
                //{ "wall", () =>pion1Node.Position =  new Vector3 (x -=0.08f, y , z *1)}, //kiri
                { "one", () =>curPlay1 = 1 },
                { "up", () =>curPlay1 = 2 },
                //{ "go", () =>pion1Node.Position *=0.45f }
            });



        }

        protected void initBoard()
        {
            boardModel.Add(new Vector2(initPos.X, initPos.Y));
            float x, y;
            x = initPos.X;
            y = initPos.Y;
            for (int i = 0; i<40; i++)
            {
                if (i == 10)
                {
                    y += 0.005f;
                    x += 0.005f;
                }
                else if (i == 20)
                {
                    x += 0.005f;
                    y += 0.005f;

                }
                else if (i == 30)
                {
                    y -= 0.005f;
                    x -= 0.005f;
                }
                else if (i == 0)
                {
                    x -= 0.005f;
                    y -= 0.005f;
                }
                
                if (i<10)
                {
                    //
                    y += d;

                }
                else if (i < 20)
                {
                    //
                    x += d;
                }
                else if (i < 30)
                {
                    //
                    y -= d;
                }
                else if (i < 40)
                {
                    //
                    x -= d;
                }
                boardModel.Add(new Vector2(x, y));
            }

        }


        protected void moveTo(int n, int pId)
        {
            Debug.WriteLine(n);
            if (pId == 1)
            {
                pion1Node.Position = new Vector3(boardModel[40].X, boardModel[40].Y, zi);
                currentPos1 = n;
                
            }
            else if (pId == 2)
            {
                pion2Node.Position = new Vector3(boardModel[n].X, boardModel[n].Y, zi);
                currentPos2 = n;
            }
        }

        
        

        // For HL optical stabilization (optional)
        public override Vector3 FocusWorldPoint => boardNode.WorldPosition;

        //Handle input:

        Vector3 earthPosBeforeManipulations;
        public override void OnGestureManipulationStarted() => earthPosBeforeManipulations = boardNode.Position;
        public override void OnGestureManipulationUpdated(Vector3 relativeHandPosition) =>
            boardNode.Position = relativeHandPosition + earthPosBeforeManipulations;

        int dice = 0;
        int dice2 = 0;
        public override void OnGestureTapped() {
            
            Random rand = new Random();
            dice = rand.Next(1, 7);
            dice2 = rand.Next(1, 7);

            textNode.GetComponent<Text3D>().Text = dice.ToString();
            Debug.WriteLine(dice);

            Debug.WriteLine("dadu1: " + dice);
            if (curPlay1 ==1 )
                moveTo(currentPos1 + dice, curPlay1);
            else if (curPlay1 == 2)
                moveTo(currentPos2 + dice, curPlay1);

            textNode2.GetComponent<Text3D>().Text = dice2.ToString();
            Debug.WriteLine(dice2);

            Debug.WriteLine("dadu2: " + dice2);
            if (curPlay1 == 1)
                moveTo(currentPos1 + dice2, curPlay1);
            else if (curPlay1 == 2)
                moveTo(currentPos2 + dice2, curPlay1);
        }
        public override void OnGestureDoubleTapped() {
            
        }
        
        
        
        }
}