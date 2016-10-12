using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using TGC.Group.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Terrain;
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
        private bool BoundingBox;
        private TgcPlane terrain;
        private List<TgcMesh> models;
        private TgcSkyBox skybox;

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
            var loader = new TgcSceneLoader();

            models = new List<TgcMesh>();

            //Crear suelo
            var terrainTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\grass.jpg");
            var terrainSizeMultiplier = Game.Default.Config_MapSizeMultiplier;
            if (terrainSizeMultiplier <= 0)
                terrainSizeMultiplier = 1;
            terrain = new TgcPlane(new Vector3(), new Vector3(6000 * terrainSizeMultiplier, 0f, 6000 * terrainSizeMultiplier), TgcPlane.Orientations.XZplane, terrainTexture, 50f, 50f);

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
            var template = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\ArbolSelvatico2\\ArbolSelvatico2-TgcScene.xml").Meshes[0];
            while (iterator <= Game.Default.Config_01_TreesVolume)
            {
                var size = numberGenerator.Next(4, 15);
                models.Add(template.createMeshInstance("01_Arbol_" + iterator, new Vector3(positionX, 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), new Vector3(0, positionX, 0),
                new Vector3(size, size, size)));
                positionX = positionX + (int)Math.Ceiling(terrain.Size.X / Game.Default.Config_01_TreesVolume);
                iterator = iterator + 1;
            }

            //Modelo 2: Roca - template
            template = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Roca\\Roca-TgcScene.xml").Meshes[0];
            iterator = 1;
            positionX = 1;
            while (iterator <= Game.Default.Config_02_RocksVolume)
            {
                var size = numberGenerator.Next(1,10);
                models.Add(template.createMeshInstance("02_Roca_" + iterator, new Vector3(positionX, 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), new Vector3(positionX, positionX, 0),
                new Vector3(size/2,size/2,size/2)));
                positionX = positionX + (int)Math.Ceiling(terrain.Size.X / Game.Default.Config_02_RocksVolume);
                iterator = iterator + 1;
            }

            //Modelo 3: Pasto - template
            template = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pasto\\Pasto-TgcScene.xml").Meshes[0];
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
            template = loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Arbusto\\Arbusto-TgcScene.xml").Meshes[0];
            iterator = 1;
            positionX = 1;
            while (iterator <= Game.Default.Config_04_BushesVolume)
            {
                var size = numberGenerator.Next(1, 3);
                models.Add(template.createMeshInstance("04_Arusto_" + iterator, new Vector3(positionX, 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), new Vector3(0, positionX, 0),
                new Vector3(size, size, size)));
                positionX = positionX + (int)Math.Ceiling(terrain.Size.X / Game.Default.Config_04_BushesVolume);
                iterator = iterator + 1;
            }

            //Posición de la camara.
            Camara = new TgcFpsCamera(Input);
        }

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

            foreach (var mesh in models)
            {
                mesh.AlphaBlendEnable = true;
            }

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
                mesh.render();
                if (BoundingBox)
                {
                    mesh.BoundingBox.render();
                }
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
            foreach (var mesh in models)
            {
                mesh.dispose();
            }
        }
    }
}