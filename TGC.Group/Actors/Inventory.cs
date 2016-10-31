using System.Collections.Generic;
using TGC.Group.Items;

namespace TGC.Group.Actors
{
    class Inventory
    {
        private List<Item> Items;
        private int Weight;

        public Inventory(){
            Items = new List<Item>(10);
            Weight = 0;
        }

        public void AddItem(Item item)
        {
            this.Items.Add(item);
        }

        // Setters & Getters
        public void SetWeight(int value) { Weight = value; }
        public void SetWeight()
        {
            Weight = 0;
            foreach (var item in Items)
            {
                Weight = Weight + item.GetWeight();
            }
        }

        public int GetWeight() { return Weight; }
        public Item GetItem(int itemId)
        {
            return Items.Find(item => itemId == item.GetId());
        }
    }
}
