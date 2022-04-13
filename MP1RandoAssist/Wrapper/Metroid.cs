using System;
using System.Drawing;

namespace Wrapper
{
    public class Metroid
    {
        public virtual long IGT() { return 0; }
        public String IGTAsStr()
        {
            long __IGT = IGT();
            return String.Format("{0:00}:{1:00}:{2:00}.{3:000}", __IGT / (60 * 60 * 1000), (__IGT / (60 * 1000)) % 60, (__IGT / 1000) % 60, __IGT % 1000);
        }
        public virtual uint CurrentSuitVisual { get; set; }
        public virtual uint CurrentWorld { get; }
        public virtual uint CurrentArea { get; }
        public virtual bool IsInSaveStationRoom { get; }
        public bool IsIngame() { return IGT() > 16; }
        public virtual bool IsMorphed() { return false; }
        public virtual bool IsSwitchingState() { return false; }
        public virtual int GetHealth() { return 0; }
        public virtual int GetBaseHealth() { return 99; }
        public virtual int GetEtankCapacity() { return 100; }
        public int GetMaxHealth() { return GetPickupCount("Energy Tanks") * GetEtankCapacity() + GetBaseHealth(); }
        public virtual void SetHealth(int health) { }
        public virtual bool HasPickup(String pickup) { return false; }
        public virtual int GetPickupCount(String pickup) { return 0; }
        public virtual void SetPickupCount(String pickup, int count) { }
        public virtual int GetAmmo(String pickup) { return 0; }
        public virtual void SetAmmo(String pickup, int ammo) { }
    }
}