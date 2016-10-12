using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SkeletalAnimation;

namespace TGC.Group.Actor
{
    class Actor : TgcSkeletalLoader
    {
        private Inventory Inventory;
        private float Health { get; set; }
        private float Stamina { get; set; }
        private bool Thirsty { get; set; }
        private bool Hungry { get; set; }
        private bool Tired { get; set; }
        private bool Cooled { get; set; }
        private bool Heated { get; set; }
        private int Weight { get; set; }

        public Actor()
        {
            this.Health = 100;
            this.Stamina = 100;
            this.Hungry = false;
            this.Tired = false;
            this.Thirsty = false;
            this.Cooled = false;
            this.Heated = false;
            this.Weight = 0;
            this.Inventory = new Inventory();
        }

        // Setters & Getters
        public void SetHealth(float value) { Health = value; }
        public void SetStamina(float value) { Stamina = value; }
        public void SetThirstStatus(bool value) { Thirsty = value; }
        public void SetHungerStatus(bool value) { Hungry = value; }
        public void SetFatigueStatus(bool value) { Tired = value; }
        public void SetColdStatus(bool value) { Cooled = value; }
        public void SetHeatStatus(bool value) { Heated = value; }
        public void SetWeight(int value) { Weight = value; }
        public void SetWeight()
        {
            Weight = Inventory.GetWeight();
        }
        
        public float GetHealth() { return Health; }
        public float GetStamina() { return Stamina; }
        public bool GetThirstStatus() { return Thirsty; }
        public bool GetHungerStatus() { return Hungry; }
        public bool GetFatigueStatus() { return Tired; }
        public bool GetColdStatus() { return Cooled; }
        public bool GetHeatStatus() { return Heated; }
        public int GetWeight() { return Weight; }
    }
}
