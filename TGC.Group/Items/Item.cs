using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Textures;
using TGC.Core.SceneLoader;
using TGC.Core.Geometry;

namespace TGC.Group.Items
{
    class Item : TgcMesh
    {
        private int Id;
        private int Weight;
        private TgcTexture Icon;

        public Item(int id, string name, int weight, TgcTexture icon)
        {
            this.Id = id;
            this.Name = name;
            this.Weight = weight;
            this.Icon = icon;
            this.AlphaBlendEnable = true;
        }

        // Setters & Getters
        public void SetId(int value) { Id = value; }
        public void SetName(string value) { Name = value; }
        public void SetWeight(int value) { Weight = value; }
        public void SetIcon(TgcTexture value) { Icon = value; }

        public int GetId() { return Id; }
        public string GetName() { return Name; }
        public int GetWeight() { return Weight; }
        public TgcTexture GetIcon() { return Icon; }
    }
}
