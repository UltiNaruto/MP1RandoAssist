namespace Wrapper.Prime
{
    public class Prime : Metroid
    {
        protected const int CONST_ARTIFACT_TEMPLE_AREA_IDX = 16;

        protected const long OFF_CGAMESTATE_WORLDLAYERSTATE_ARRAY = 0x94;
        protected const long OFF_CGAMESTATE_PLAYTIME = 0xA0;
        protected const long OFF_CPLAYER = 0x84C;
        protected const long OFF_CPLAYER_MORPHSTATE = 0x2F4;
        protected const long OFF_CWORLD = 0x850;
        protected const long OFF_CWORLD_MLVL = 0x8;
        protected const long OFF_CWORLD_MREAID = 0x68;
        protected const long OFF_CWORLD_MLVLID = 0x6C;
        protected const long OFF_CWORLD_MORPHSTATE = 0x2F0;
        protected const long OFF_CPLAYERSTATE = 0x8B8;
        protected const long OFF_HEALTH = 0x0C;
        protected const long OFF_CRITICAL_HEALTH = OFF_HEALTH + 4;
        protected const long OFF_CURRENT_SUIT_VISUAL = 0x20;
        protected const long OFF_POWERBEAM_OBTAINED = 0x2C;
        protected const long OFF_ICEBEAM_OBTAINED = 0x34;
        protected const long OFF_WAVEBEAM_OBTAINED = 0x3C;
        protected const long OFF_PLASMABEAM_OBTAINED = 0x44;
        protected const long OFF_MISSILES = 0x48;
        protected const long OFF_MAX_MISSILES = OFF_MISSILES + 4;
        protected const long OFF_SCANVISOR_OBTAINED = 0x54;
        protected const long OFF_MORPHBALLBOMBS_OBTAINED = 0x5C;
        protected const long OFF_POWERBOMBS = 0x60;
        protected const long OFF_MAX_POWERBOMBS = OFF_POWERBOMBS + 4;
        protected const long OFF_FLAMETHROWER_OBTAINED = 0x6C;
        protected const long OFF_THERMALVISOR_OBTAINED = 0x74;
        protected const long OFF_CHARGEBEAM_OBTAINED = 0x7C;
        protected const long OFF_SUPERMISSILE_OBTAINED = 0x84;
        protected const long OFF_GRAPPLEBEAM_OBTAINED = 0x8C;
        protected const long OFF_XRAYVISOR_OBTAINED = 0x94;
        protected const long OFF_ICESPREADER_OBTAINED = 0x9C;
        protected const long OFF_SPACEJUMPBOOTS_OBTAINED = 0xA4;
        protected const long OFF_MORPHBALL_OBTAINED = 0xAC;
        protected const long OFF_COMBATVISOR_OBTAINED = 0xB4;
        protected const long OFF_BOOSTBALL_OBTAINED = 0xBC;
        protected const long OFF_SPIDERBALL_OBTAINED = 0xC4;
        protected const long OFF_POWERSUIT_OBTAINED = 0xCC;
        protected const long OFF_GRAVITYSUIT_OBTAINED = 0xD4;
        protected const long OFF_VARIASUIT_OBTAINED = 0xDC;
        protected const long OFF_PHAZONSUIT_OBTAINED = 0xE4;
        protected const long OFF_ENERGYTANKS_OBTAINED = 0xEC;
        protected const long OFF_ENERGYREFILL_OBTAINED = 0xFC;
        protected const long OFF_WAVEBUSTER_OBTAINED = 0x10C;
        protected const long OFF_ARTIFACT_OF_TRUTH_OBTAINED = 0x114;
        protected const long OFF_ARTIFACT_OF_STRENGTH_OBTAINED = 0x11C;
        protected const long OFF_ARTIFACT_OF_ELDER_OBTAINED = 0x124;
        protected const long OFF_ARTIFACT_OF_WILD_OBTAINED = 0x12C;
        protected const long OFF_ARTIFACT_OF_LIFEGIVER_OBTAINED = 0x134;
        protected const long OFF_ARTIFACT_OF_WARRIOR_OBTAINED = 0x13C;
        protected const long OFF_ARTIFACT_OF_CHOZO_OBTAINED = 0x144;
        protected const long OFF_ARTIFACT_OF_NATURE_OBTAINED = 0x14C;
        protected const long OFF_ARTIFACT_OF_SUN_OBTAINED = 0x154;
        protected const long OFF_ARTIFACT_OF_WORLD_OBTAINED = 0x15C;
        protected const long OFF_ARTIFACT_OF_SPIRIT_OBTAINED = 0x164;
        protected const long OFF_ARTIFACT_OF_NEWBORN_OBTAINED = 0x16C;

        protected virtual long CPlayer { get; }
        protected virtual long CWorld { get; }
        protected virtual long CGameState { get; }
        protected virtual long CPlayerState { get; }
        protected virtual long CWorldLayerState(uint world, uint area_id) { return 0; }
        protected virtual long _IGT { get; }

        protected virtual int MorphState { get; set; }

        protected virtual int Health { get; set; }
        protected virtual int Missiles { get; set; }
        protected virtual int MaxMissiles { get; set; }
        protected virtual int PowerBombs { get; set; }
        protected virtual int MaxPowerBombs { get; set; }
        protected virtual int MaxEnergyTanks { get; set; }

        protected bool HaveEnergyTanks
        {
            get
            {
                return MaxEnergyTanks > 0;
            }
        }

        protected virtual int MorphBallBombs { get; set; }
        protected int MaxMorphBallBombs => 3;

        protected virtual bool HaveIceBeam { get; set; }
        protected virtual bool HaveWaveBeam { get; set; }
        protected virtual bool HavePlasmaBeam { get; set; }

        protected bool HaveMissiles
        {
            get
            {
                return MaxMissiles > 0;
            }
        }

        protected virtual bool HaveMorphBallBombs { get; set; }

        protected bool HavePowerBombs
        {
            get
            {
                return MaxPowerBombs > 0;
            }
        }

        protected virtual bool HaveFlamethrower { get; set; }
        protected virtual bool HaveScanVisor { get; set; }
        protected virtual bool HaveThermalVisor { get; set; }
        protected virtual bool HaveChargeBeam { get; set; }
        protected virtual bool HaveSuperMissile { get; set; }
        protected virtual bool HaveGrappleBeam { get; set; }
        protected virtual bool HaveXRayVisor { get; set; }
        protected virtual bool HaveIceSpreader { get; set; }
        protected virtual bool HaveSpaceJumpBoots { get; set; }
        protected virtual bool HaveMorphBall { get; set; }
        protected virtual bool HaveBoostBall { get; set; }
        protected virtual bool HaveSpiderBall { get; set; }
        protected virtual bool HaveGravitySuit { get; set; }
        protected virtual bool HaveVariaSuit { get; set; }
        protected virtual bool HavePhazonSuit { get; set; }
        protected virtual bool HaveWavebuster { get; set; }
        protected virtual bool HaveArtifact(int index) { return false; }
        protected virtual void SetArtifact(int index, bool obtained) { }

        public override long IGT()
        {
            return this._IGT;
        }

        int GetLayerCount(uint world, uint area_id)
        {
            long CLayerState = CWorldLayerState(world, area_id);
            if (CLayerState == 0)
                return 0;
            return GCMem.ReadInt32(CLayerState);
        }

        bool IsLayerEnabled(uint world, uint area_id, uint layer_id)
        {
            long CLayerState = CWorldLayerState(world, area_id);
            if (CLayerState == 0)
                return false;
            uint layer_state = GCMem.ReadUInt32(CLayerState + 0xC);
            layer_state &= (uint)(1 << (int)layer_id);
            if (layer_id > 0)
                layer_state >>= (int)layer_id;
            return layer_state == 1;
        }

        void EnableLayer(uint world, uint area_id, uint layer_id)
        {
            long CLayerState = CWorldLayerState(world, area_id);
            if (CLayerState == 0)
                return;
            uint layer_state = GCMem.ReadUInt32(CLayerState + 0xC);
            GCMem.WriteUInt32(CLayerState + 0xC, layer_state | (uint)(1 << (int)layer_id));
        }

        void DisableLayer(uint world, uint area_id, uint layer_id)
        {
            long CLayerState = CWorldLayerState(world, area_id);
            if (CLayerState == 0)
                return;
            uint layer_state = GCMem.ReadUInt32(CLayerState + 0xC);
            GCMem.WriteUInt32(CLayerState + 0xC, layer_state & ~(uint)(1 << (int)layer_id));
        }

        public override bool IsInSaveStationRoom
        {
            get
            {
                if (CurrentWorld == 0xC13B09D1) // Impact Crater
                {
                    return CurrentArea == 0x00;   // Entrance
                }
                else if (CurrentWorld == 0x3EF8237C) // Magmoor Caverns
                {
                    return CurrentArea == 0x03 || // Save Station Magmoor A
                           CurrentArea == 0x1C;   // Save Station Magmoor B
                }
                else if (CurrentWorld == 0xB1AC4D65) // Phazon Mines
                {
                    return CurrentArea == 0x04 || // Save Station Mines A
                           CurrentArea == 0x1E || // Save Station Mines B
                           CurrentArea == 0x22;   // Save Station Mines C
                }
                else if (CurrentWorld == 0x83F6FF6F) // Chozo Ruins
                {
                    return CurrentArea == 0x16 || // Save Station 1
                           CurrentArea == 0x27 || // Save Station 2
                           CurrentArea == 0x3B;   // Save Station 3
                }
                else if (CurrentWorld == 0x39F2DE28) // Tallon Overworld
                {
                    return CurrentArea == 0x00 || // Landing Site
                           CurrentArea == 0x1C;   // Save Station in Crashed Frigate
                }
                else if (CurrentWorld == 0xA8BE6291) // Phendrana Drifts
                {
                    return CurrentArea == 0x04 || // Save Station B
                           CurrentArea == 0x11 || // Save Station A
                           CurrentArea == 0x21 || // Save Station D
                           CurrentArea == 0x2D;   // Save Station C
                }

                return false;
            }
        }

        public override bool IsMorphed()
        {
            return (MorphState & 1) == 1;
        }

        public override bool IsSwitchingState()
        {
            return MorphState > 1;
        }

        public override bool HasPickup(string pickup)
        {
            switch(pickup)
            {
                case "Energy Tanks":
                    return HaveEnergyTanks;
                case "Missiles":
                    return HaveMissiles;
                case "Morph Ball":
                    return HaveMorphBall;
                case "Morph Ball Bombs":
                    return HaveMorphBallBombs;
                case "Boost Ball":
                    return HaveBoostBall;
                case "Spider Ball":
                    return HaveSpiderBall;
                case "Power Bombs":
                    return HavePowerBombs;
                case "Space Jump Boots":
                    return HaveSpaceJumpBoots;
                case "Varia Suit":
                    return HaveVariaSuit;
                case "Gravity Suit":
                    return HaveGravitySuit;
                case "Phazon Suit":
                    return HavePhazonSuit;
                case "Scan Visor":
                    return HaveScanVisor;
                case "Thermal Visor":
                    return HaveThermalVisor;
                case "XRay Visor":
                    return HaveXRayVisor;
                case "Wave Beam":
                    return HaveWaveBeam;
                case "Ice Beam":
                    return HaveIceBeam;
                case "Plasma Beam":
                    return HavePlasmaBeam;
                case "Charge Beam":
                    return HaveChargeBeam;
                case "Grapple Beam":
                    return HaveGrappleBeam;
                case "Super Missile":
                    return HaveSuperMissile;
                case "Wavebuster":
                    return HaveWavebuster;
                case "Ice Spreader":
                    return HaveIceSpreader;
                case "Flamethrower":
                    return HaveFlamethrower;
                case "Truth":
                    if (GetLayerCount(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX) == 24)
                        return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 23);
                    else
                        return !IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 18);
                case "Strength":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 2);
                case "Elder":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 3);
                case "Wild":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 4);
                case "Lifegiver":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 5);
                case "Warrior":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 6);
                case "Chozo":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 7);
                case "Nature":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 8);
                case "Sun":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 9);
                case "World":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 10);
                case "Spirit":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 11);
                case "Newborn":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 12);
                default:
                    return false;
            }
        }

        public override int GetPickupCount(string pickup)
        {
            switch (pickup)
            {
                case "Energy Tanks":
                    return MaxEnergyTanks;
                case "Missiles":
                    return MaxMissiles;
                case "Morph Ball":
                    return HaveMorphBall ? 1 : 0;
                case "Morph Ball Bombs":
                    return HaveMorphBallBombs ? 1 : 0;
                case "Boost Ball":
                    return HaveBoostBall ? 1 : 0;
                case "Spider Ball":
                    return HaveSpiderBall ? 1 : 0;
                case "Power Bombs":
                    return MaxPowerBombs;
                case "Space Jump Boots":
                    return HaveSpaceJumpBoots ? 1 : 0;
                case "Varia Suit":
                    return HaveVariaSuit ? 1 : 0;
                case "Gravity Suit":
                    return HaveGravitySuit ? 1 : 0;
                case "Phazon Suit":
                    return HavePhazonSuit ? 1 : 0;
                case "Scan Visor":
                    return HaveScanVisor ? 1 : 0;
                case "Thermal Visor":
                    return HaveThermalVisor ? 1 : 0;
                case "XRay Visor":
                    return HaveXRayVisor ? 1 : 0;
                case "Wave Beam":
                    return HaveWaveBeam ? 1 : 0;
                case "Ice Beam":
                    return HaveIceBeam ? 1 : 0;
                case "Plasma Beam":
                    return HavePlasmaBeam ? 1 : 0;
                case "Charge Beam":
                    return HaveChargeBeam ? 1 : 0;
                case "Grapple Beam":
                    return HaveGrappleBeam ? 1 : 0;
                case "Super Missile":
                    return HaveSuperMissile ? 1 : 0;
                case "Wavebuster":
                    return HaveWavebuster ? 1 : 0;
                case "Ice Spreader":
                    return HaveIceSpreader ? 1 : 0;
                case "Flamethrower":
                    return HaveFlamethrower ? 1 : 0;
                case "Truth":
                    if(GetLayerCount(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX) == 24)
                        return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 23) ? 1 : 0;
                    else
                        return !IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 18) ? 1 : 0;
                case "Strength":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 2) ? 1 : 0;
                case "Elder":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 3) ? 1 : 0;
                case "Wild":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 4) ? 1 : 0;
                case "Lifegiver":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 5) ? 1 : 0;
                case "Warrior":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 6) ? 1 : 0;
                case "Chozo":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 7) ? 1 : 0;
                case "Nature":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 8) ? 1 : 0;
                case "Sun":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 9) ? 1 : 0;
                case "World":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 10) ? 1 : 0;
                case "Spirit":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 11) ? 1 : 0;
                case "Newborn":
                    return IsLayerEnabled(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 12) ? 1 : 0;
                default:
                    return 0;
            }
        }

        public override int GetHealth()
        {
            return Health;
        }

        public override void SetHealth(int health)
        {
            Health = health;
        }

        public override void SetPickupCount(string pickup, int count)
        {
            switch (pickup)
            {
                case "Missiles":
                    MaxMissiles = count;
                    break;
                case "Morph Ball":
                    HaveMorphBall = count > 0;
                    break;
                case "Morph Ball Bombs":
                    HaveMorphBallBombs = count > 0;
                    break;
                case "Boost Ball":
                    HaveBoostBall = count > 0;
                    break;
                case "Spider Ball":
                    HaveSpiderBall = count > 0;
                    break;
                case "Power Bombs":
                    MaxPowerBombs = count;
                    break;
                case "Space Jump Boots":
                    HaveSpaceJumpBoots = count > 0;
                    break;
                case "Varia Suit":
                    HaveVariaSuit = count > 0;
                    UpdateSuitVisual();
                    break;
                case "Gravity Suit":
                    HaveGravitySuit = count > 0;
                    UpdateSuitVisual();
                    break;
                case "Phazon Suit":
                    HavePhazonSuit = count > 0;
                    UpdateSuitVisual();
                    break;
                case "Scan Visor":
                    HaveScanVisor = count > 0;
                    break;
                case "Thermal Visor":
                    HaveThermalVisor = count > 0;
                    break;
                case "XRay Visor":
                    HaveXRayVisor = count > 0;
                    break;
                case "Wave Beam":
                    HaveWaveBeam = count > 0;
                    break;
                case "Ice Beam":
                    HaveIceBeam = count > 0;
                    break;
                case "Plasma Beam":
                    HavePlasmaBeam = count > 0;
                    break;
                case "Charge Beam":
                    HaveChargeBeam = count > 0;
                    break;
                case "Grapple Beam":
                    HaveGrappleBeam = count > 0;
                    break;
                case "Super Missile":
                    HaveSuperMissile = count > 0;
                    break;
                case "Wavebuster":
                    HaveWavebuster = count > 0;
                    break;
                case "Ice Spreader":
                    HaveIceSpreader = count > 0;
                    break;
                case "Flamethrower":
                    HaveFlamethrower = count > 0;
                    break;
                case "Truth":
                    SetArtifact(0, count > 0);
                    if (count > 0)
                    {
                        if(GetLayerCount(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX) == 24)
                            EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 23);
                        /*else
                            DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 18);*/
                    }
                    else
                    {
                        if (GetLayerCount(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX) == 24)
                            DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 23);
                        /*else
                            EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 18);*/
                    }
                    break;
                case "Strength":
                    SetArtifact(1, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 2);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 2);
                    break;
                case "Elder":
                    SetArtifact(2, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 3);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 3);
                    break;
                case "Wild":
                    SetArtifact(3, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 4);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 4);
                    break;
                case "Lifegiver":
                    SetArtifact(4, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 5);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 5);
                    break;
                case "Warrior":
                    SetArtifact(5, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 6);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 6);
                    break;
                case "Chozo":
                    SetArtifact(6, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 7);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 7);
                    break;
                case "Nature":
                    SetArtifact(7, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 8);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 8);
                    break;
                case "Sun":
                    SetArtifact(8, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 9);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 9);
                    break;
                case "World":
                    SetArtifact(9, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 10);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 10);
                    break;
                case "Spirit":
                    SetArtifact(10, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 11);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 11);
                    break;
                case "Newborn":
                    SetArtifact(11, count > 0);
                    if (count > 0)
                        EnableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 12);
                    else
                        DisableLayer(0x39F2DE28, CONST_ARTIFACT_TEMPLE_AREA_IDX, 12);
                    break;
            }
        }

        public override int GetAmmo(string pickup)
        {
            switch (pickup)
            {
                case "Morph Ball Bombs":
                    return MorphBallBombs;
                case "Missiles":
                    return Missiles;
                case "Power Bombs":
                    return PowerBombs;
                default:
                    return 0;
            }
        }

        public override void SetAmmo(string pickup, int ammo)
        {
            switch (pickup)
            {
                case "Morph Ball Bombs":
                    MorphBallBombs = ammo;
                    break;
                case "Missiles":
                    Missiles = ammo;
                    break;
                case "Power Bombs":
                    PowerBombs = ammo;
                    break;
            }
        }

        void UpdateSuitVisual()
        {
            CurrentSuitVisual &= 4;
            if (HaveVariaSuit)
                CurrentSuitVisual = 2;
            if (HaveGravitySuit)
                CurrentSuitVisual = 1;
            if (HavePhazonSuit)
                CurrentSuitVisual = 3;
        }
    }
}
