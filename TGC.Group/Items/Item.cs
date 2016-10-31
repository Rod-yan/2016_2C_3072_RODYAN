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
        private int Quantity;
        private TgcTexture Icon;
        private List<int> Category;

        public Item(int id, string name, int weight, int quantity, TgcTexture texture)
        {
            this.Id = id;
            this.Name = name;
            this.Weight = weight * quantity;
            this.Quantity = quantity;
            this.Icon = texture;
            this.Category = new List<int>();
        }

        // Setters & Getters
        public void SetId(int value) { Id = value; }
        public void SetName(string value) { Name = value; }
        public void SetWeight(int value) { Weight = value * this.Quantity; }
        public void SetQuantity(int value) { Quantity = value; }
        public void SetIcon(TgcTexture value) { Icon = value; }
        public void SetCategory(List<int> value) { Category = value; }

        public int GetId() { return Id; }
        public string GetName() { return Name; }
        public int GetWeight() { return Weight; }
        public int GetQuantity() { return Quantity; }
        public TgcTexture GetIcon() { return Icon; }
        public List<int> GetCategory() { return Category; }
    }
}
