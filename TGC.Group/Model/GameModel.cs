using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Group.Camara;
using TGC.Group.Actors;
using TGC.Group.Collision;
using TGC.Core.Direct3D;
using TGC.Core.Utils;
using TGC.Core.BoundingVolumes;
using TGC.Core.Example;
using TGC.Core.Geometry;
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
        private bool BoundingBox;
        private TgcPlane terrain;
        private List<TgcMesh> models;
        private TgcSkyBox skybox;
        private TgcSkeletalMesh character;
        private SphereCollisionManager collisionManager;
        private TgcBoundingSphere characterSphere;
        private TgcThirdPersonCamera camaraInterna;

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
            var numberGenerator = new Random();
            var sceneLoader = new TgcSceneLoader();
            var skeletalLoader = new TgcSkeletalLoader();

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
                           MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\StandBy-TgcSkeletalAnim.xml"
                       });

            //Configurar animacion inicial
            character.playAnimation("StandBy", true);
            //Escalarlo porque es muy pequeño
            character.AutoTransformEnable = true;
            character.Position = new Vector3(terrain.Size.X / 2f, 2000, terrain.Size.Z / 2f);
            character.Scale = new Vector3(2.6f, 2.6f, 2.6f);
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

            //Crear skybox
            skybox = new TgcSkyBox();
            skybox.Center = new Vector3(0, 0, 0);
            skybox.Size = new Vector3(terrain.Size.X + 4000, terrain.Size.Y + 4000, terrain.Size.Z + 4000);
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Up, MediaDir + "\\Textures\\skybox_up.png");
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Down, MediaDir + "\\Textures\\skybox_down.png");
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Left, MediaDir + "\\Textures\\skybox_left.png");
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Right, MediaDir + "\\Textures\\skybox_right.png");
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Front, MediaDir + "\\Textures\\skybox_front.png");
            skybox.setFaceTexture(TgcSkyBox.SkyFaces.Back, MediaDir + "\\Textures\\skybox_back.png");
            skybox.SkyEpsilon = 25f;
            skybox.Init();

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
            //Camara = new TgcFpsCamera(character.Position, 1f , 0, Input);
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

        float velocidadCaminar = 7.5f;
        float velocidadRotacion = 80f;
        float jumping = 0;
        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
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

            //Si hubo rotacion
            if (rotating)
            {
                //Rotar personaje y la camara, hay que multiplicarlo por el tiempo transcurrido para no atarse a la velocidad el hardware
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                character.rotateY(rotAngle);
                camaraInterna.rotateY(rotAngle);
            }

            //Si hubo desplazamiento
            if (moving)
            {
                //Activar animacion de caminando
                character.playAnimation("Walk", true);
            }

            //Si no se esta moviendo, activar animacion de Parado
            else
            {
                character.playAnimation("StandBy", true);
            }

            //Vector de movimiento
            var movementVector = Vector3.Empty;
            if (moving)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movementVector = new Vector3(FastMath.Sin(character.Rotation.Y) * moveForward, jump,
                    FastMath.Cos(character.Rotation.Y) * moveForward);
            }

            //Mover personaje con detección de colisiones, sliding y gravedad
            var realMovement = collisionManager.moveCharacter(characterSphere, movementVector, objetosColisionables);
            character.move(realMovement);
            //Hacer que la camara siga al personaje en su nueva posicion
            camaraInterna.Target = character.Position;

            //Capturar Input teclado
            if (Input.keyPressed(Key.Z))
            {
                BoundingBox = !BoundingBox;
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

            //Render personaje
            character.animateAndRender(ElapsedTime);

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