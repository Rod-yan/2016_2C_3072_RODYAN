using Microsoft.DirectX;
using System.IO;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Group.Camara;
using TGC.Group.Form;
using TGC.Group.Actors;
using TGC.Group.Collision;
using TGC.Group.Items;
using TGC.Core.Input;
using TGC.Core.Fog;
using TGC.Core.Shaders;
using TGC.Core.Direct3D;
using TGC.Core.Utils;
using TGC.Core.Text;
using TGC.Core.BoundingVolumes;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Collision;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Terrain;
using TGC.Core.SkeletalAnimation;
using System;

namespace TGC.Group.Model
{
    /// <summary>
    ///     Ejemplo para implementar el TP.
    ///     Inicialmente puede ser renombrado o copiado para hacer m·s ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        private readonly List<TgcBoundingAxisAlignBox> objetosColisionables = new List<TgcBoundingAxisAlignBox>();
        private bool BoundingBox;
        private bool selected;
        private Random numberGenerator = new Random();
        private Microsoft.DirectX.Direct3D.Effect effect;
        private TgcFog fog;
        private EjemploDefaultHelpForm helpForm;
        private TgcPlane terrain;
        private TgcPlane healthBar;
        private TgcPlane staminaBar;
        private TgcPlane coldIcon;
        private TgcPlane thirstIcon;
        private TgcPlane hungerIcon;
        private TgcPlane fatigueIcon;
        private TgcPlane overweightIcon;
        private TgcPlane inventoryBoard;
        private List<TgcMesh> models;
        private List<TgcMesh> inventoryHUD = new List<TgcMesh>();
        private List<TgcMesh> combinedItems = new List<TgcMesh>(2);
        private TgcSkyBox skybox;
        private TgcSkeletalMesh character;
        private SphereCollisionManager collisionManager;
        private TgcBoundingSphere characterSphere;
        private TgcThirdPersonCamera camaraInterna;
        private TgcPickingRay pickingRay;
        private TgcMesh selectedMesh;
        private Item selectedItem;
        private TgcText2D inventoryText;
        private TgcText2D weightText;
        private TgcText2D healthText;
        private TgcText2D staminaText;
        private TgcText2D youAreDeadText;
        private TgcText2D youSurvivedText;
        private TgcText2D godModeText;
        private Vector3 collisionPoint;
        private Actor actor = new Actor();
        private float jumping = 0;
        private int weatherIndex = 1; // Soleado por defecto (1), Nublado (2), Tormenta de nieve (3)
        private int itemId = 1;
        private bool showHUD = true;
        private bool godMode = false;
        private bool showInventory = false;
        private bool fogEnabled = false;
        private int screenWidth = Game.Default.Config_ScreenWidth;
        private int screenHeigh = Game.Default.Config_ScreenHeigh;

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        /// <param name="mediaDir">Ruta donde esta la carpeta con los assets</param>
        /// <param name="shadersDir">Ruta donde esta la carpeta con los shaders</param>
        public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

        public void changeWeather(int weatherIndex)
        {
            skybox.Center = new Vector3(0, 0, 0);
            skybox.Size = new Vector3(terrain.Size.X + 4000, terrain.Size.Y + 4000, terrain.Size.Z + 4000);
            switch (weatherIndex)
            {
                //Sunny
                case 1:
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Up, MediaDir + "\\Textures\\Skybox_sunny\\skybox_sunny_up.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Down, MediaDir + "\\Textures\\Skybox_sunny\\skybox_sunny_down.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Left, MediaDir + "\\Textures\\Skybox_sunny\\skybox_sunny_left.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Right, MediaDir + "\\Textures\\Skybox_sunny\\skybox_sunny_right.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Front, MediaDir + "\\Textures\\Skybox_sunny\\skybox_sunny_front.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Back, MediaDir + "\\Textures\\Skybox_sunny\\skybox_sunny_back.png");
                    terrain.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device,
                        MediaDir + "\\Textures\\grass.jpg"));

                    fogEnabled = false;

                    actor.SetColdStatus(false);

                    if (numberGenerator.Next(1, 10) == 5)
                        actor.SetThirstStatus(true);

                    break;
                //Cloudy
                case 2:
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Up, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_up.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Down, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_down.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Left, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_left.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Right, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_right.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Front, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_front.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Back, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_back.jpg");

                    fogEnabled = false;

                    if (numberGenerator.Next(1, 5) == 3)
                        actor.SetColdStatus(true);

                    break;
                //Blizzard
                case 3:
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Up, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_up.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Down, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_down.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Left, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_left.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Right, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_right.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Front, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_front.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Back, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_back.png");
                    terrain.setTexture(TgcTexture.createTexture(D3DDevice.Instance.Device,
                        MediaDir + "\\Textures\\snow.jpg"));

                    fogEnabled = false;

                    actor.SetColdStatus(true);

                    if (numberGenerator.Next(1, 20) == 10)
                        actor.SetThirstStatus(true);

                    break;
            }
            skybox.SkyEpsilon = 25f;
            skybox.Init();

            if (numberGenerator.Next(1, 10) == 5)
                actor.SetHungerStatus(true);

            return;
        }

        public void inventoryInitializer()
        {
            //Inicializo inventario del actor
            var item = new Item(itemId, "00_Water", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\Water_icon.png"));
            itemId = itemId + 1;
            actor.GetInventory().AddItem(item);

            item = new Item(itemId, "00_Apple", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\Apple_icon.png"));
            itemId = itemId + 1;
            actor.GetInventory().AddItem(item);

            item = new Item(itemId, "00_Water", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\Water_icon.png"));
            itemId = itemId + 1;
            actor.GetInventory().AddItem(item);

            item = new Item(itemId, "00_Leather", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\Leather_icon.png"));
            itemId = itemId + 1;
            actor.GetInventory().AddItem(item);

            return;
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquÌ todo el cÛdigo de inicializaciÛn: cargar modelos, texturas, estructuras de optimizaciÛn, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            var iterator = 1;
            var positionX = 1;
            var sceneLoader = new TgcSceneLoader();
            var skeletalLoader = new TgcSkeletalLoader();

            skybox = new TgcSkyBox();
            models = new List<TgcMesh>();

            var helpRtf = File.ReadAllText(MediaDir + "\\help.rtf");
            helpForm = new EjemploDefaultHelpForm(helpRtf);

            //Cargar Shader personalizado
            effect = TgcShaders.loadEffect(ShadersDir + "TgcFogShader.fx");
            fog = new TgcFog();

            //Crear suelo
            var terrainTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\grass.jpg");
            
            var terrainSizeMultiplier = Game.Default.Config_MapSizeMultiplier;
            if (terrainSizeMultiplier <= 0)
                terrainSizeMultiplier = 1;
            terrain = new TgcPlane(new Vector3(), new Vector3(6000 * terrainSizeMultiplier, 0f, 6000 * terrainSizeMultiplier), TgcPlane.Orientations.XZplane, terrainTexture, 50f, 50f);

            //Cargar personaje con animaciones
            character =
                       skeletalLoader.loadMeshAndAnimationsFromFile(
                       MediaDir + "SkeletalAnimations\\BasicHuman\\BasicHuman-TgcSkeletalMesh.xml",
                       MediaDir + "SkeletalAnimations\\BasicHuman\\",
                       new[]
                       {
                           MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Walk-TgcSkeletalAnim.xml",
                           MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\StandBy-TgcSkeletalAnim.xml",
                           MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Jump-TgcSkeletalAnim.xml"
                       });
            
            //Configurar animacion inicial
            character.playAnimation("StandBy", true);
            character.AutoTransformEnable = true;
            character.Position = new Vector3(terrain.Size.X / 2f, 2000, terrain.Size.Z / 2f);
            character.Scale = new Vector3(2.5f, 2.5f, 2.5f);
            character.rotateY(Geometry.DegreeToRadian(180f));
            
            //BoundingSphere que va a usar el personaje
            character.AutoUpdateBoundingBox = false;
            characterSphere = new TgcBoundingSphere(character.BoundingBox.calculateBoxCenter(),
                character.BoundingBox.calculateBoxRadius());
            
            //Crear manejador de colisiones
            collisionManager = new SphereCollisionManager();
            collisionManager.GravityEnabled = true;
            collisionManager.GravityForce = new Vector3(0, -10, 0);
            collisionManager.SlideFactor = 0;

            //Inventario inicial del actor
            inventoryInitializer();

            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay(Input);
            selected = false;

            //Inicializar estado climatico
            changeWeather(weatherIndex);

            // Inicializar texturas y par·metros de HUD
            var barTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\healthbar.jpg");
            healthBar = new TgcPlane(new Vector3(), new Vector3(100, 2, 0), TgcPlane.Orientations.XYplane, barTexture, 1f, 1f);

            barTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\staminabar.jpg");
            staminaBar = new TgcPlane(new Vector3(), new Vector3(100, 2, 0), TgcPlane.Orientations.XYplane, barTexture, 1f, 1f);

            barTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\Cold_icon.png");
            coldIcon = new TgcPlane(new Vector3(), new Vector3(10, 10, 0), TgcPlane.Orientations.XYplane, barTexture, 1f, 1f);
            coldIcon.AlphaBlendEnable = true;

            barTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\Thirst_icon.png");
            thirstIcon = new TgcPlane(new Vector3(), new Vector3(10, 10, 0), TgcPlane.Orientations.XYplane, barTexture, 1f, 1f);
            thirstIcon.AlphaBlendEnable = true;

            barTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\Hunger_icon.png");
            hungerIcon = new TgcPlane(new Vector3(), new Vector3(10, 10, 0), TgcPlane.Orientations.XYplane, barTexture, 1f, 1f);
            hungerIcon.AlphaBlendEnable = true;

            barTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\Fatigue_icon.png");
            fatigueIcon = new TgcPlane(new Vector3(), new Vector3(10, 10, 0), TgcPlane.Orientations.XYplane, barTexture, 1f, 1f);
            fatigueIcon.AlphaBlendEnable = true;

            barTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\Overweight_icon.png");
            overweightIcon = new TgcPlane(new Vector3(), new Vector3(10, 10, 0), TgcPlane.Orientations.XYplane, barTexture, 1f, 1f);
            overweightIcon.AlphaBlendEnable = true;

            inventoryBoard = new TgcPlane(new Vector3(),
                                          new Vector3(250, 250, 0), TgcPlane.Orientations.XYplane,
                                          TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                                   MediaDir + "\\Textures\\inventoryboard.jpg"), 1f, 1f
                                         );
            //Texto de inventario
            inventoryText = new TgcText2D();
            inventoryText.Position = new Point((int)(screenWidth * 0.84), (int)(screenHeigh * 0.84));
            inventoryText.Size = new Size(500, 100);
            inventoryText.changeFont(new System.Drawing.Font("TimesNewRoman", 14, FontStyle.Regular));
            inventoryText.Color = Color.Yellow;
            inventoryText.Align = TgcText2D.TextAlign.LEFT;

            //Texto de Peso
            weightText = new TgcText2D();
            weightText.Position = new Point((int)(screenWidth * 0.84), (int)(screenHeigh * 0.86));
            weightText.Size = new Size(500, 100);
            weightText.changeFont(new System.Drawing.Font("TimesNewRoman", 14, FontStyle.Regular));
            weightText.Color = Color.Yellow;
            weightText.Align = TgcText2D.TextAlign.LEFT;

            //Texto de Salud
            healthText = new TgcText2D();
            healthText.Position = new Point((int)(screenWidth * 0.078), (int)(screenHeigh * 0.84));
            healthText.Size = new Size(500, 100);
            healthText.changeFont(new System.Drawing.Font("TimesNewRoman", 14, FontStyle.Regular));
            healthText.Color = Color.Red;
            healthText.Align = TgcText2D.TextAlign.LEFT;

            //Texto de Aguante
            staminaText = new TgcText2D();
            staminaText.Position = new Point((int)(screenWidth * 0.078), (int)(screenHeigh * 0.86));
            staminaText.Size = new Size(500, 100);
            staminaText.changeFont(new System.Drawing.Font("TimesNewRoman", 14, FontStyle.Regular));
            staminaText.Color = Color.LightGreen;
            staminaText.Align = TgcText2D.TextAlign.LEFT;

            //Texto de Personaje Muerto
            youAreDeadText = new TgcText2D();
            youAreDeadText.Position = new Point((int)(screenWidth * 0.2604), (int)(screenHeigh * 0.463));
            youAreDeadText.Size = new Size(500, 100);
            youAreDeadText.changeFont(new System.Drawing.Font("TimesNewRoman", 52, FontStyle.Regular));
            youAreDeadText.Color = Color.Tomato;
            youAreDeadText.Align = TgcText2D.TextAlign.LEFT;

            //Texto de Juego Ganado
            youSurvivedText = new TgcText2D();
            youSurvivedText.Position = new Point((int)(screenWidth * 0.2604), (int)(screenHeigh * 0.463));
            youSurvivedText.Size = new Size(500, 100);
            youSurvivedText.changeFont(new System.Drawing.Font("TimesNewRoman", 52, FontStyle.Regular));
            youSurvivedText.Color = Color.LightSeaGreen;
            youSurvivedText.Align = TgcText2D.TextAlign.LEFT;

            //Texto de GodMode
            godModeText = new TgcText2D();
            godModeText.Position = new Point((int)(screenWidth * 0.46875), (int)(screenHeigh * 0.0185));
            godModeText.Size = new Size(500, 100);
            godModeText.changeFont(new System.Drawing.Font("TimesNewRoman", 14, FontStyle.Regular));
            godModeText.Color = Color.Blue;
            godModeText.Align = TgcText2D.TextAlign.LEFT;

            //Modelo 1: Arbol - template
            var template = sceneLoader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\ArbolSelvatico2\\ArbolSelvatico2-TgcScene.xml").Meshes[0];
            
            while (iterator <= Game.Default.Config_01_TreesVolume)
            {
                var size = numberGenerator.Next(4, 15);
                models.Add(template.createMeshInstance("01_Arbol_" + iterator, new Vector3(positionX, 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), new Vector3(0, positionX, 0),
                new Vector3(size, size, size)));
                positionX = positionX + (int)Math.Ceiling(terrain.Size.X / Game.Default.Config_01_TreesVolume);
                iterator = iterator + 1;
            }

            //Modelo 2: Roca - template
            template = sceneLoader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Roca\\Roca-TgcScene.xml").Meshes[0];
            iterator = 1;
            positionX = 1;
            while (iterator <= Game.Default.Config_02_RocksVolume)
            {
                var size = numberGenerator.Next(1, 10);
                models.Add(template.createMeshInstance("02_Roca_" + iterator, new Vector3(positionX, 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), new Vector3(positionX, positionX, 0),
                new Vector3(size / 2, size / 2, size / 2)));
                positionX = positionX + (int)Math.Ceiling(terrain.Size.X / Game.Default.Config_02_RocksVolume);
                iterator = iterator + 1;
            }
            
            //Modelo 3: Pasto - template
            template = sceneLoader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pasto\\Pasto-TgcScene.xml").Meshes[0];
            iterator = 1;
            positionX = 1;
            while (iterator <= Game.Default.Config_03_GrassVolume)
            {
                models.Add(template.createMeshInstance("03_Pasto_" + iterator, new Vector3(positionX, 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), new Vector3(0, positionX, 0),
                new Vector3(1, 1, 1)));
                positionX = positionX + (int)Math.Ceiling(terrain.Size.X / Game.Default.Config_03_GrassVolume);
                iterator = iterator + 1;
            }
            
            //Modelo 4: Arbusto - template
            template = sceneLoader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Arbusto\\Arbusto-TgcScene.xml").Meshes[0];
            iterator = 1;
            positionX = 1;
            while (iterator <= Game.Default.Config_04_BushesVolume)
            {
                var size = numberGenerator.Next(1, 3);
                models.Add(template.createMeshInstance("04_Arbusto_" + iterator, new Vector3(positionX, 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), new Vector3(0, positionX, 0),
                new Vector3(size, size, size)));
                positionX = positionX + (int)Math.Ceiling(terrain.Size.X / Game.Default.Config_04_BushesVolume);
                iterator = iterator + 1;
            }
            
            //PosiciÛn de la camara.
            camaraInterna = new TgcThirdPersonCamera(character.Position, new Vector3(0, 100, 0), 100, -400);
            Camara = camaraInterna;
            
            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            objetosColisionables.Add(terrain.BoundingBox);
            foreach (var mesh in models)
            {
                if (!((mesh.Name.Contains("03_Pasto_")) || (mesh.Name.Contains("04_Arbusto_"))))
                {
                    objetosColisionables.Add(mesh.BoundingBox);
                }
            }
        }        

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lÛgica de computo del modelo, asÌ como tambiÈn verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            float velocidadCaminar = Game.Default.Config_WalkingSpeed;
            float velocidadRotacion = 150f;

            PreUpdate();

            D3DDevice.Instance.Device.Transform.Projection =
                    Matrix.PerspectiveFovLH(1.2f,
                                            D3DDevice.Instance.AspectRatio,
                                            D3DDevice.Instance.ZNearPlaneDistance,
                                            D3DDevice.Instance.ZFarPlaneDistance * Game.Default.Config_MapSizeMultiplier
                                            );

            skybox.Center = Camara.Position;

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;
            var columnCount = 1;
            int iconOffsetX = -110;
            int iconOffsetY = 280;
            float jump = 0;
            float thirstEffect = 0;
            float hungerEffect = 0;
            float coldEffect = 0;
            float tiredFactor = actor.GetStamina() / 35 ;

            //Mostrar inventario
            if (Input.keyPressed(Key.I))
            {
                showInventory = !showInventory;
            }

            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
            }

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
            }
            
            //Izquierda
            if (Input.keyDown(Key.A))
            {
                rotate = -velocidadRotacion;
                rotating = true;
            }

            //Jump
            if (Input.keyUp(Key.Space) && jumping < 15)
            {
                jumping = 15;
            }
            if (Input.keyUp(Key.Space) || jumping > 0)
            {
                jumping -= 15 * ElapsedTime;
                jump = jumping;
                moving = true;
            }

            if (actor.GetInventory().GetWeight() >= Game.Default.Config_MaxWeight)
            {
                moving = false;
            }

            if (actor.GetStamina() <= 40)
            {
                actor.SetFatigueStatus(true);
                if (actor.GetStamina() <= 0)
                {
                    moving = false;
                }
            }
            else
            {
                actor.SetFatigueStatus(false);
            }

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                character.rotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);
            }
            //Acciones seg˙n actividad
            if (moving && !showInventory)
            {
                character.playAnimation("Walk", true);
                if (actor.GetStamina() >= 0)
                {
                    actor.SetStamina(actor.GetStamina() - (0.6f / (actor.GetStamina() + actor.GetInventory().GetWeight())));
                }
            }
            else
            {
                character.playAnimation("StandBy", true);
                if ((actor.GetStamina() < 100) && (actor.GetStamina() >= 0))
                {
                    actor.SetStamina(actor.GetStamina() + 5 * ElapsedTime);
                }
            }

            //Vector de movimiento
            var movementVector = Vector3.Empty;
            if (moving && !showInventory && (actor.GetHealth() > 0))
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movementVector = new Vector3(FastMath.Sin(character.Rotation.Y) * moveForward * tiredFactor, jump,
                    FastMath.Cos(character.Rotation.Y) * moveForward * tiredFactor);
            }

            //Mover personaje con detecciÛn de colisiones, sliding y gravedad
            var realMovement = collisionManager.moveCharacter(characterSphere, movementVector, objetosColisionables);
            character.move(realMovement);

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = character.Position;
            camaraInterna.UpdateCamera(ElapsedTime);

            //Probabilidad de que cambie el estado climatico de 1:3000
            if ((numberGenerator.Next(1, 4000) == 2500) && (actor.GetHealth() > 0))
            {
                weatherIndex = numberGenerator.Next(1, 4);
                changeWeather(weatherIndex);
            }

            //Efectos adversos
            if (actor.GetColdStatus())
            {
                coldEffect = 0.008f;
            }
            if (actor.GetHungerStatus())
            {
                hungerEffect = 0.004f;
            }
            if (actor.GetThirstStatus())
            {
                thirstEffect = 0.004f;
            }

            //Modificadores a la salud del actor
            actor.SetHealth(actor.GetHealth() - thirstEffect - hungerEffect - coldEffect);

            //Fix Stamina, Health del actor
            if (actor.GetStamina() > 100)
                actor.SetStamina(100);
            if (actor.GetHealth() > 100)
                actor.SetHealth(100);
            if (actor.GetStamina() < 0)
                actor.SetStamina(0);
            if (actor.GetHealth() <= 0)
            {
                actor.SetHealth(0);
                actor.SetStamina(0);
            }

            // HUD
            healthBar.Origin = new Vector3(character.Position.X - 50, character.Position.Y + 140, character.Position.Z);
            healthBar.Size = new Vector3(actor.GetHealth(), 2, 0);
            healthBar.updateValues();

            staminaBar.Origin = new Vector3(character.Position.X - 50, character.Position.Y + 135, character.Position.Z);
            staminaBar.Size = new Vector3(actor.GetStamina(), 2, 0);
            staminaBar.updateValues();

            coldIcon.Origin = new Vector3(character.Position.X - 45, character.Position.Y + 145, character.Position.Z);
            coldIcon.updateValues();

            thirstIcon.Origin = new Vector3(character.Position.X - 25, character.Position.Y + 145, character.Position.Z);
            thirstIcon.updateValues();

            hungerIcon.Origin = new Vector3(character.Position.X -5, character.Position.Y + 145, character.Position.Z);
            hungerIcon.updateValues();

            fatigueIcon.Origin = new Vector3(character.Position.X + 15, character.Position.Y + 145, character.Position.Z);
            fatigueIcon.updateValues();

            overweightIcon.Origin = new Vector3(character.Position.X + 35, character.Position.Y + 145, character.Position.Z);
            overweightIcon.updateValues();

            inventoryBoard.Origin = new Vector3(character.Position.X - 120, character.Position.Y + 50, character.Position.Z - 99f);
            inventoryBoard.updateValues();

            //Doy formato al inventario
            if (showInventory && (actor.GetHealth() > 0))
            {
                inventoryHUD.Clear();
                foreach (var item in actor.GetInventory().GetItems())
                {
                    if (columnCount == 7)
                    {
                        iconOffsetY = iconOffsetY - 40;
                        iconOffsetX = -110;
                        columnCount = 1;
                    }

                    var itemIcon = new TgcPlane(new Vector3(character.Position.X + iconOffsetX, character.Position.Y + iconOffsetY, character.Position.Z - 100f), new Vector3(10, 10, 0), TgcPlane.Orientations.XYplane, item.GetIcon(), 1f, 1f);
                    iconOffsetX = iconOffsetX + 40;
                    columnCount = columnCount + 1;

                    inventoryHUD.Add(itemIcon.toMesh(item.GetId() + ""));
                }
            }

            //Actualizo textos de status
            healthText.Text = "ÅHEALTH: " + (int)actor.GetHealth();
            staminaText.Text = "ÅSTAMINA: " + (int)actor.GetStamina();
            inventoryText.Text = "ÅINVENTORY: "+actor.GetInventory().GetItems().Count+" / 20";
            weightText.Text = "ÅWEIGHT: " + actor.GetInventory().GetWeight() + "Kg / 25Kg";
            youSurvivedText.Text = "ÅYOU SURVIVED!";
            youAreDeadText.Text = "ÅYOU ARE DEAD";
            godModeText.Text = "ÅGODMODE";

            //Capturar Input teclado para activar o desactivar boundingBox
            if (Input.keyPressed(Key.Z))
            {
                BoundingBox = !BoundingBox;
            }
            //Capturar Input teclado para activar o desactivar HUD
            if (Input.keyPressed(Key.H))
            {
                showHUD = !showHUD;
            }
            //Capturar Input teclado para activar o desactivar Godmode
            if (Input.keyPressed(Key.G))
            {
                godMode = !godMode;
            }
            if (godMode)
            {
                actor.SetStamina(100);
                actor.SetHealth(100);
                actor.SetColdStatus(false);
                actor.SetFatigueStatus(false);
                actor.SetHungerStatus(false);
                actor.SetThirstStatus(false);
                actor.GetInventory().SetWeight(0);
                collisionManager.GravityEnabled = false;
            }
            else
            {
                actor.GetInventory().ResetWeight();
                collisionManager.GravityEnabled = true;
            }
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquÌ todo el cÛdigo referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            PreRender();
            ClearTextures();

            fog.Enabled = fogEnabled;
            fog.StartDistance = 100;
            fog.EndDistance = 10000;
            fog.Density = 0.0025f;
            fog.Color = Color.LightGray;

            if (fog.Enabled)
            {
                // Cargamos las variables de shader, color del fog.
                fog.updateValues();
                effect.SetValue("ColorFog", fog.Color.ToArgb());
                effect.SetValue("CameraPos", TgcParserUtils.vector3ToFloat4Array(Camara.Position));
                effect.SetValue("StartFogDistance", fog.StartDistance);
                effect.SetValue("EndFogDistance", fog.EndDistance);
                effect.SetValue("Density", fog.Density);
            }

            terrain.render();
            skybox.render();

            //Si hacen clic con el mouse, ver si hay colision RayAABB
            if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT) || Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {
                //Actualizar Ray de colision en base a posicion del mouse
                pickingRay.updateRay();

                //Testear Ray contra el AABB de todos los meshes
                if (showInventory)
                {
                    foreach (var item in inventoryHUD)
                    {
                        var aabb = item.BoundingBox;

                        //Ejecutar test, si devuelve true se carga el punto de colision collisionPoint
                        selected = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, aabb, out collisionPoint);
                        if (selected)
                        {
                            selectedMesh = item;
                            selectedItem = actor.GetInventory().GetItemByID(Int32.Parse(selectedMesh.Name));
                            if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_RIGHT))
                            {
                                if (combinedItems.Count == 2)
                                    combinedItems.Clear();

                                combinedItems.Add(selectedMesh);
                                selectedMesh = null;
                                selectedItem = null;
                                selected = false;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    foreach (var mesh in models)
                    {
                        var aabb = mesh.BoundingBox;

                        //Ejecutar test, si devuelve true se carga el punto de colision collisionPoint
                        selected = TgcCollisionUtils.intersectRayAABB(pickingRay.Ray, aabb, out collisionPoint);
                        if (selected)
                        {
                            selectedMesh = mesh;
                            break;
                        }
                    }
                }
            }

            foreach (var mesh in models)
            {
                mesh.Transform =
                Matrix.Scaling(mesh.Scale)
                            * Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z)
                            * Matrix.Translation(mesh.Position);
                mesh.AlphaBlendEnable = true;

                if (fog.Enabled)
                {
                    mesh.Effect = effect;
                    mesh.Technique = "RenderScene";
                }
                else
                {
                    mesh.Effect = TgcShaders.Instance.TgcMeshShader;
                    mesh.Technique = "DIFFUSE_MAP";
                }

                mesh.render();

                if (BoundingBox)
                {
                    mesh.BoundingBox.render();
                }
            }

            if (selected)
            {
                if ((Math.Abs((selectedMesh.BoundingBox.Position.X - character.Position.X)) < 300) && (Math.Abs((selectedMesh.BoundingBox.Position.Z - character.Position.Z)) < 300))
                {
                    //Agrego items del objeto seleccionado al inventario del actor
                    if (actor.GetInventory().GetFreeSpace() != 0)
                    {
                        int itemsObtained;

                        if (actor.GetInventory().GetFreeSpace() >= 3)
                        {
                            itemsObtained = numberGenerator.Next(0, 4);
                        }
                        else
                        {
                            itemsObtained = numberGenerator.Next(0, actor.GetInventory().GetFreeSpace() + 1);
                        }
                        int pickItemType;
                        Item item;
                        int iterator = 0;

                        if (selectedMesh.Name.Contains("01_Arbol_"))
                        {
                            while (iterator < itemsObtained)
                            {
                                pickItemType = numberGenerator.Next(1, 6);
                                switch (pickItemType)
                                {
                                    case 1:
                                        item = new Item(itemId, "00_Water", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Water_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                    case 2:
                                        item = new Item(itemId, "00_Charcoal", 2, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Charcoal_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                    case 3:
                                        item = new Item(itemId, "00_Wood", 2, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Wood_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                    case 4:
                                        item = new Item(itemId, "00_Fiber", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Cloth_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                    case 5:
                                        item = new Item(itemId, "00_Apple", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Apple_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                }
                                itemId = itemId + 1;
                                iterator = iterator + 1;
                            }
                        }
                        else if (selectedMesh.Name.Contains("02_Roca_"))
                        {
                            while (iterator < itemsObtained)
                            {
                                pickItemType = numberGenerator.Next(1, 3);
                                switch (pickItemType)
                                {
                                    case 1:
                                        item = new Item(itemId, "00_Stone", 2, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Stone_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                    case 2:
                                        item = new Item(itemId, "00_Metal", 3, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Metal_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                }
                                itemId = itemId + 1;
                                iterator = iterator + 1;
                            }
                        }
                        else if (selectedMesh.Name.Contains("03_Pasto_"))
                        {
                            while (iterator < itemsObtained)
                            {
                                pickItemType = numberGenerator.Next(1, 3);
                                switch (pickItemType)
                                {
                                    case 1:
                                        item = new Item(itemId, "00_Water", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Water_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                    case 2:
                                        item = new Item(itemId, "00_Fiber", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Cloth_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                }
                                itemId = itemId + 1;
                                iterator = iterator + 1;
                            }
                        }
                        else if (selectedMesh.Name.Contains("04_Arbusto_"))
                        {
                            while (iterator < itemsObtained)
                            {
                                pickItemType = numberGenerator.Next(1, 5);
                                switch (pickItemType)
                                {
                                    case 1:
                                        item = new Item(itemId, "00_Water", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Water_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                    case 2:
                                        item = new Item(itemId, "00_Fiber", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Cloth_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                    case 3:
                                        item = new Item(itemId, "00_Corn", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Corn_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                    case 4:
                                        item = new Item(itemId, "00_Leather", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                                                        MediaDir + "\\Textures\\Leather_icon.png"));
                                        actor.GetInventory().AddItem(item);
                                        break;
                                }
                                itemId = itemId + 1;
                                iterator = iterator + 1;
                            }
                        }
                    }

                    if (showInventory)
                    {
                        if (selectedItem.GetName().Contains("00_Water"))
                        {
                            actor.SetThirstStatus(false);
                            actor.SetStamina(actor.GetStamina() + 15);
                        }
                        else if (selectedItem.GetName().Contains("00_Fiber") || selectedItem.GetName().Contains("00_Leather"))
                        {
                            actor.SetColdStatus(false);
                        }
                        else if (selectedItem.GetName().Contains("00_Apple") || selectedItem.GetName().Contains("00_Corn"))
                        {
                            actor.SetHungerStatus(false);
                            actor.SetHealth(actor.GetHealth() + 15);
                        }

                        inventoryHUD.Remove(selectedMesh);
                        actor.GetInventory().RemoveItem(selectedItem);
                    }
                    else
                    {
                        models.Remove(selectedMesh);
                        objetosColisionables.Remove(selectedMesh.BoundingBox);
                    }
                }

                selectedMesh = null;
                selectedItem = null;
                selected = false;
            }

            //Render personaje
            character.animateAndRender(ElapsedTime);

            //Aviso de Juego Ganado
            if (models.Count == 0)
            {
                youSurvivedText.render();
                actor.SetHealth(100);
            }

            //Aviso de Personaje Muerto
            if (actor.GetHealth() <= 0)
            {
                youAreDeadText.render();
            }

            //Aviso de GodMode
            if (godMode)
            {
                godModeText.render();
            }

            //Muestro Pantalla de ayuda si toca tecla P
            if (Input.keyPressed(Key.P))
            {
                helpForm.ShowDialog();
            }

            //Render del HUD e Inventario
            if (showHUD)
            {
                if (showInventory && (actor.GetHealth() > 0))
                {
                    inventoryBoard.render();
                    foreach (var item in inventoryHUD)
                    {
                        item.Transform =
                        Matrix.Scaling(item.Scale)
                                    * Matrix.RotationYawPitchRoll(item.Rotation.Y, item.Rotation.X, item.Rotation.Z)
                                    * Matrix.Translation(item.Position);
                        item.AlphaBlendEnable = true;
                        if (combinedItems.Contains(item))
                        {
                            item.setColor(Color.DarkBlue);
                            item.UpdateMeshTransform();
                        }
                        item.render();
                    }
                }
                healthBar.render();
                staminaBar.render();
                inventoryText.render();
                weightText.render();
                healthText.render();
                staminaText.render();
                if (actor.GetColdStatus())
                    coldIcon.render();
                if (actor.GetThirstStatus())
                    thirstIcon.render();
                if (actor.GetHungerStatus())
                    hungerIcon.render();
                if (actor.GetFatigueStatus())
                    fatigueIcon.render();
                if (actor.GetInventory().GetWeight() >= Game.Default.Config_MaxWeight)
                    overweightIcon.render();
            }
            
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecuciÛn del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gr·ficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            terrain.dispose();
            healthBar.dispose();
            staminaBar.dispose();
            inventoryText.Dispose();
            weightText.Dispose();
            healthText.Dispose();
            godModeText.Dispose();
            staminaText.Dispose();
            youAreDeadText.Dispose();
            youSurvivedText.Dispose();
            coldIcon.dispose();
            thirstIcon.dispose();
            hungerIcon.dispose();
            fatigueIcon.dispose();
            overweightIcon.dispose();
            inventoryBoard.dispose();
            skybox.dispose();
            character.dispose();
            helpForm.Dispose();
            foreach (var mesh in models)
            {
                mesh.dispose();
            }
            foreach (var item in inventoryHUD)
            {
                item.dispose();
            }
        }
    }
}