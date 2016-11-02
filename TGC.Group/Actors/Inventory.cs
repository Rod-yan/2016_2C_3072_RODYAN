using System.Collections.Generic;
using TGC.Group.Items;

namespace TGC.Group.Actors
{
    class Inventory
    {
        private List<Item> Items;
        private int Weight;

        public Inventory(){
            Items = new List<Item>(20);
            Weight = 0;
        }

        public void AddItem(Item item)
        {
            this.Items.Add(item);
            SetWeight(this.Weight + item.GetWeight());
        }

        public void RemoveItem(Item item)
        {
            this.Items.Remove(item);
            SetWeight(this.Weight - item.GetWeight());
        }

        public int GetFreeSpace()
        {
            return (this.Items.Capacity - this.Items.Count);
        }

        public Item GetItemByID(int itemId)
        {
            return Items.Find(item => itemId == item.GetId());
        }

        // Setters & Getters
        public void SetWeight(int value) { Weight = value; }

        public int GetWeight() { return Weight; }
        public List<Item> GetItems() { return Items; }
    }
}
