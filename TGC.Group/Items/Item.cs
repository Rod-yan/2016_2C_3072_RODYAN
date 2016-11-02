using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Textures;

namespace TGC.Group.Items
{
    class Item
    {
        private int Id;
        private string Name;
        private int Weight;
        private TgcTexture Icon;

        public Item(int id, string name, int weight, TgcTexture texture)
        {
            this.Id = id;
            this.Name = name;
            this.Weight = weight;
            this.Icon = texture;
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
