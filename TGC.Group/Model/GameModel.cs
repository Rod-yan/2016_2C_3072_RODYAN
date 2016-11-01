using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Group.Camara;
using TGC.Group.Actors;
using TGC.Group.Collision;
using TGC.Group.Items;
using TGC.Core.Input;
using TGC.Core.Direct3D;
using TGC.Core.Utils;
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
    ///     Inicialmente puede ser renombrado o copiado para hacer más ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar el modelo que instancia GameForm <see cref="Form.GameForm.InitGraphics()" />
    ///     line 97.
    /// </summary>
    public class GameModel : TgcExample
    {
        private readonly List<TgcBoundingAxisAlignBox> objetosColisionables = new List<TgcBoundingAxisAlignBox>();
        private int itemId = 1;
        private bool BoundingBox;
        private bool showInventory;
        private bool selected;
        private TgcPlane terrain;
        private List<TgcMesh> models;
        private TgcSkyBox skybox;
        private TgcSkeletalMesh character;
        private SphereCollisionManager collisionManager;
        private TgcBoundingSphere characterSphere;
        private TgcThirdPersonCamera camaraInterna;
        private TgcPickingRay pickingRay;
        private TgcMesh selectedMesh;
        private Vector3 collisionPoint;
        private Actor actor = new Actor();
        private float velocidadCaminar = 15f;
        private float velocidadRotacion = 150f;
        private float jumping = 0;
        private int weatherIndex = 1;

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

                    actor.SetThirstStatus(true);
                    //actor.SetStamina()
                    break;
                //Cloudy
                case 2:
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Up, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_up.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Down, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_down.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Left, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_left.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Right, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_right.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Front, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_front.jpg");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Back, MediaDir + "\\Textures\\Skybox_cloudy\\skybox_cloudy_back.jpg");
                    break;
                //Blizzard
                case 3:
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Up, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_up.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Down, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_down.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Left, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_left.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Right, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_right.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Front, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_front.png");
                    skybox.setFaceTexture(TgcSkyBox.SkyFaces.Back, MediaDir + "\\Textures\\Skybox_blizzard\\skybox_blizzard_back.png");
                    break;
            }
            skybox.SkyEpsilon = 25f;
            skybox.Init();
            return;
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aquí todo el código de inicialización: cargar modelos, texturas, estructuras de optimización, todo
        ///     procesamiento que podemos pre calcular para nuestro juego.
        ///     Borrar el codigo ejemplo no utilizado.
        /// </summary>
        public override void Init()
        {
            var iterator = 1;
            var positionX = 1;
            var sceneLoader = new TgcSceneLoader();
            var skeletalLoader = new TgcSkeletalLoader();
            var numberGenerator = new Random();

            skybox = new TgcSkyBox();
            models = new List<TgcMesh>();
            
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
            
            //Creo todos los items del juego e inicializo inventario del actor
            var item = new Item(itemId, "Agua", 1, 5, TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\water-icon.png"));
            itemId = itemId + 1;
            actor.GetInventory().AddItem(item);

            /*item = new Item(itemId, "Madera", 1, TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\water-icon.png"));*/

            //Iniciarlizar PickingRay
            pickingRay = new TgcPickingRay(Input);
            selected = false;

            //Inicializar estado climatico
            changeWeather(weatherIndex);
            
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
            
            //Posición de la camara.
            camaraInterna = new TgcThirdPersonCamera(character.Position, new Vector3(0, 100, 0), 100, -400);
            Camara = camaraInterna;
            
            //Almacenar volumenes de colision del escenario
            objetosColisionables.Clear();
            objetosColisionables.Add(terrain.BoundingBox);
            foreach (var mesh in models)
            {
                if (!((mesh.Name.Contains("03_Pasto_")) || (mesh.Name.Contains("04_Arbusto_"))))
                    objetosColisionables.Add(mesh.BoundingBox);
            }
        }        

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            var numberGenerator = new Random();
            PreUpdate();

            D3DDevice.Instance.Device.Transform.Projection =
                    Matrix.PerspectiveFovLH(D3DDevice.Instance.FieldOfView,
                    D3DDevice.Instance.AspectRatio,
                    D3DDevice.Instance.ZNearPlaneDistance,
                    D3DDevice.Instance.ZFarPlaneDistance * Game.Default.Config_MapSizeMultiplier);
            skybox.Center = Camara.Position;

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;
            float jump = 0;
            float tiredFactor = actor.GetStamina() / 100;

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
            //Acciones según actividad
            if (moving)
            {
                character.playAnimation("Walk", true);
                if (actor.GetStamina() >= 0)
                {
                    actor.SetStamina(actor.GetStamina() - (1 / actor.GetStamina() * 4));
                }
            }
            else
            {
                character.playAnimation("StandBy", true);
                if ((actor.GetStamina() < 100) && (actor.GetStamina() >= 0))
                {
                    actor.SetStamina(actor.GetStamina() + 40 + ElapsedTime);
                }
            }

            //Vector de movimiento
            var movementVector = Vector3.Empty;
            if (moving)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movementVector = new Vector3(FastMath.Sin(character.Rotation.Y) * moveForward * tiredFactor, jump,
                    FastMath.Cos(character.Rotation.Y) * moveForward * tiredFactor);
            }

            //Mover personaje con detección de colisiones, sliding y gravedad
            var realMovement = collisionManager.moveCharacter(characterSphere, movementVector, objetosColisionables);
            character.move(realMovement);

            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = character.Position;
            camaraInterna.UpdateCamera(ElapsedTime);

            //Probabilidad de que cambie el estado climatico de 1:3000
            if (numberGenerator.Next(1, 2000) == 1)
            {
                weatherIndex = numberGenerator.Next(1, 4);
                changeWeather(weatherIndex);
            }

            //Fix Stamina, Health del actor
            if (actor.GetStamina() > 100)
                actor.SetStamina(100);
            if (actor.GetHealth() > 100)
                actor.SetHealth(100);
            if (actor.GetStamina() < 0)
                actor.SetStamina(0);
            if (actor.GetHealth() < 0)
                actor.SetHealth(0);

            //Capturar Input teclado
            if (Input.keyPressed(Key.Z))
            {
                BoundingBox = !BoundingBox;
            }
            if (Input.keyPressed(Key.I))
            {
                showInventory = !showInventory;
            }
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aquí todo el código referido al renderizado.
        ///     Borrar todo lo que no haga falta.
        /// </summary>
        public override void Render()
        {
            PreRender();

            terrain.render();
            skybox.render();

            //Si hacen clic con el mouse, ver si hay colision RayAABB
            if (Input.buttonPressed(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //Actualizar Ray de colision en base a posicion del mouse
                pickingRay.updateRay();

                //Testear Ray contra el AABB de todos los meshes
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

            foreach (var mesh in models)
            {
                mesh.Transform =
                Matrix.Scaling(mesh.Scale)
                            * Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z)
                            * Matrix.Translation(mesh.Position);
                mesh.AlphaBlendEnable = true;
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
                    //Remuevo el mesh. TODO: Asignar a inventario de usuario conjunto de elementos segun tipo de mesh seleccionado y destruido
                    models.Remove(selectedMesh);
                    objetosColisionables.Remove(selectedMesh.BoundingBox);
                }
                else
                {
                    selectedMesh = null;
                    selected = false;
                }
            }

            /*(if (showInventory)
            {
                inventory.render();
            }*/

            //Render personaje
            character.animateAndRender(ElapsedTime);
            DrawText.drawText("Health: " + actor.GetHealth(), 5, 20, System.Drawing.Color.Red);
            DrawText.drawText("Stamina: "+actor.GetStamina(), 5, 40, System.Drawing.Color.Red);
            if (actor.GetFatigueStatus())
            {
                DrawText.drawText("TIRED", 5, 60, System.Drawing.Color.Red);
            }
            
            PostRender();
        }

        /// <summary>
        ///     Se llama cuando termina la ejecución del ejemplo.
        ///     Hacer Dispose() de todos los objetos creados.
        ///     Es muy importante liberar los recursos, sobretodo los gráficos ya que quedan bloqueados en el device de video.
        /// </summary>
        public override void Dispose()
        {
            terrain.dispose();
            skybox.dispose();
            character.dispose();
            foreach (var mesh in models)
            {
                mesh.dispose();
            }
        }
    }
}