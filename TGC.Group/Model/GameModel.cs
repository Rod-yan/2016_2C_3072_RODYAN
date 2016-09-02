using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;
using System.Drawing;
using TGC.Group.Camara;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Utils;
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
            var model1_MaxCount = Game.Default.Config_TreesVolume;
            var model1_Iterator = 1;

            var model2_MaxCount = Game.Default.Config_RocksVolume;
            var model2_Iterator = 1;

            var model3_MaxCount = Game.Default.Config_GrassVolume;
            var model3_Iterator = 1;

            models = new List<TgcMesh>();
            var numberGenerator = new Random();

            //Crear suelo
            var terrainTexture = TgcTexture.createTexture(D3DDevice.Instance.Device,
                 MediaDir + "\\Textures\\grass_2.jpg");
            terrain = new TgcPlane(new Vector3(0, 0, 0), new Vector3(6000, 0f, 6000), TgcPlane.Orientations.XZplane, terrainTexture);

            //Modelo 1, template
            var model1_Loader = new TgcSceneLoader(); 
            var model1_Scene = model1_Loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pino\\Pino-TgcScene.xml");
            var model1_Template = model1_Scene.Meshes[0];

            while (model1_Iterator <= model1_MaxCount)
            {
                models.Add(model1_Template.createMeshInstance("Arbol_"+ model1_Iterator, new Vector3(numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.X)), 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), Vector3.Empty,
                new Vector3(1, 1, 1)));
                model1_Iterator = model1_Iterator + 1;
            }

            //Modelo 2, template
            var model2_Loader = new TgcSceneLoader();
            var model2_Scene = model2_Loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Roca\\Roca-TgcScene.xml");
            var model2_Template = model2_Scene.Meshes[0];

            while (model2_Iterator <= model2_MaxCount)
            {
                models.Add(model2_Template.createMeshInstance("Roca_" + model1_Iterator, new Vector3(numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.X)), 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), Vector3.Empty,
                new Vector3(1, 1, 1)));
                model2_Iterator = model2_Iterator + 1;
            }

            //Modelo 3, template
            var model3_Loader = new TgcSceneLoader();
            var model3_Scene = model3_Loader.loadSceneFromFile(MediaDir + "MeshCreator\\Meshes\\Vegetacion\\Pasto\\Pasto-TgcScene.xml");
            var model3_Template = model3_Scene.Meshes[0];

            while (model3_Iterator <= model3_MaxCount)
            {
                models.Add(model3_Template.createMeshInstance("Pasto_" + model1_Iterator, new Vector3(numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.X)), 0, numberGenerator.Next(0, (int)Math.Ceiling(terrain.Size.Z))), Vector3.Empty,
                new Vector3(1, 1, 1)));
                model3_Iterator = model3_Iterator + 1;
            }

            //Posición de la camara.
            Camara = new TgcRotationalCamera(new Vector3((terrain.Size.X * 0.5f), 20, (terrain.Size.Z * 0.5f)), 100, Input);
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la lógica de computo del modelo, así como también verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        public override void Update()
        {
            PreUpdate();

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
            foreach (var mesh in models)
            {
                mesh.dispose();
            }
        }
    }
}