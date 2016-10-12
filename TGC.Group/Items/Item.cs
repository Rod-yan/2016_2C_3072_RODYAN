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
        private int Weight;
        private TgcTexture Icon;

        public Item(int id, int weight)
        {
            this.Id = id;
            this.Weight = weight;
        }

        // Setters & Getters
        public void SetId(int value) { Id = value; }
        public void SetWeight(int value) { Weight = value; }
        public void SetIcon(TgcTexture texture) { Icon = texture; }

        public int GetId() { return Id; }
        public int GetWeight() { return Weight; }
        public TgcTexture GetIcon() { return Icon; }
    }
}
