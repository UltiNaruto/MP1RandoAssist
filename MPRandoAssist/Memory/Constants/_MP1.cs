namespace Prime.Memory.Constants
{
    internal abstract class _MP1
    {
        internal const long OFF_PLAYTIME = 0xA0;
        internal const long OFF_CWORLD = 0x850;
        internal const long OFF_ROOM_ID = 0x68;
        internal const long OFF_WORLD_ID = 0x6C;
        internal const long OFF_CPLAYERSTATE = 0x8B8;
        internal const long OFF_HEALTH = 0x0C;
        internal const long OFF_CRITICAL_HEALTH = OFF_HEALTH + 4;
        internal const long OFF_POWERBEAM_OBTAINED = 0x2C;
        internal const long OFF_ICEBEAM_OBTAINED = 0x34;
        internal const long OFF_WAVEBEAM_OBTAINED = 0x3C;
        internal const long OFF_PLASMABEAM_OBTAINED = 0x44;
        internal const long OFF_MISSILES = 0x48;
        internal const long OFF_MAX_MISSILES = OFF_MISSILES + 4;
        internal const long OFF_SCANVISOR_OBTAINED = 0x54;
        internal const long OFF_MORPHBALLBOMBS_OBTAINED = 0x5C;
        internal const long OFF_POWERBOMBS = 0x60;
        internal const long OFF_MAX_POWERBOMBS = OFF_POWERBOMBS + 4;
        internal const long OFF_FLAMETHROWER_OBTAINED = 0x6C;
        internal const long OFF_THERMALVISOR_OBTAINED = 0x74;
        internal const long OFF_CHARGEBEAM_OBTAINED = 0x7C;
        internal const long OFF_SUPERMISSILE_OBTAINED = 0x84;
        internal const long OFF_GRAPPLEBEAM_OBTAINED = 0x8C;
        internal const long OFF_XRAYVISOR_OBTAINED = 0x94;
        internal const long OFF_ICESPREADER_OBTAINED = 0x9C;
        internal const long OFF_SPACEJUMPBOOTS_OBTAINED = 0xA0;
        internal const long OFF_MORPHBALL_OBTAINED = 0xAC;
        internal const long OFF_COMBATVISOR_OBTAINED = 0xB4;
        internal const long OFF_BOOSTBALL_OBTAINED = 0xBC;
        internal const long OFF_SPIDERBALL_OBTAINED = 0xC4;
        internal const long OFF_POWERSUIT_OBTAINED = 0xCC;
        internal const long OFF_GRAVITYSUIT_OBTAINED = 0xD4;
        internal const long OFF_VARIASUIT_OBTAINED = 0xDC;
        internal const long OFF_PHAZONSUIT_OBTAINED = 0xE4;
        internal const long OFF_ENERGYTANKS_OBTAINED = 0xEC;
        internal const long OFF_ENERGYREFILL_OBTAINED = 0xFC;
        internal const long OFF_WAVEBUSTER_OBTAINED = 0x10C;
        internal const long OFF_ARTIFACT_OF_TRUTH_OBTAINED = 0x114;
        internal const long OFF_ARTIFACT_OF_STRENGTH_OBTAINED = 0x11C;
        internal const long OFF_ARTIFACT_OF_ELDER_OBTAINED = 0x124;
        internal const long OFF_ARTIFACT_OF_WILD_OBTAINED = 0x12C;
        internal const long OFF_ARTIFACT_OF_LIFEGIVER_OBTAINED = 0x134;
        internal const long OFF_ARTIFACT_OF_WARRIOR_OBTAINED = 0x13C;
        internal const long OFF_ARTIFACT_OF_CHOZO_OBTAINED = 0x144;
        internal const long OFF_ARTIFACT_OF_NATURE_OBTAINED = 0x14C;
        internal const long OFF_ARTIFACT_OF_SUN_OBTAINED = 0x154;
        internal const long OFF_ARTIFACT_OF_WORLD_OBTAINED = 0x15C;
        internal const long OFF_ARTIFACT_OF_SPIRIT_OBTAINED = 0x164;
        internal const long OFF_ARTIFACT_OF_NEWBORN_OBTAINED = 0x16C;

        internal abstract long CGameState { get; }
        internal abstract long CPlayerState { get; }
        internal abstract long CWorld { get; }
        internal abstract uint CurrentWorld { get; }
        internal abstract uint CurrentRoom { get; }
        internal abstract ushort Health { get; set; }
        internal abstract ushort MaxHealth { get; }
        internal abstract long IGT { get; set; }
        internal abstract string IGTAsStr { get; }

        internal abstract uint MorphBallBombs { get; set; }
        internal abstract uint MaxMorphBallBombs { get; }
        internal abstract uint Missiles { get; set; }
        internal abstract uint MaxMissiles { get; set; }
        internal abstract uint PowerBombs { get; set; }
        internal abstract uint MaxPowerBombs { get; set; }
        internal abstract bool HaveIceBeam { get; set; }
        internal abstract bool HaveWaveBeam { get; set; }
        internal abstract bool HavePlasmaBeam { get; set; }
        internal abstract bool HaveMorphBallBombs { get; set; }
        internal abstract bool HaveFlamethrower { get; set; }
        internal abstract bool HaveThermalVisor { get; set; }
        internal abstract bool HaveChargeBeam { get; set; }
        internal abstract bool HaveSuperMissile { get; set; }
        internal abstract bool HaveGrappleBeam { get; set; }
        internal abstract bool HaveXRayVisor { get; set; }
        internal abstract bool HaveIceSpreader { get; set; }
        internal abstract bool HaveSpaceJumpBoots { get; set; }
        internal abstract bool HaveMorphBall { get; set; }
        internal abstract bool HaveBoostBall { get; set; }
        internal abstract bool HaveSpiderBall { get; set; }
        internal abstract bool HaveGravitySuit { get; set; }
        internal abstract bool HaveVariaSuit { get; set; }
        internal abstract bool HavePhazonSuit { get; set; }
        internal abstract bool HaveWavebuster { get; set; }
        internal abstract bool Artifacts(int index);

        internal bool IsInSaveStationRoom
        {
            get
            {
                if (CurrentWorld == 0x0A) // Impact Crater
                {
                    return CurrentRoom == 0x00;   // Entrance
                }
                else if (CurrentWorld == 0x11) // Magmoor Caverns
                {
                    return CurrentRoom == 0x03 || // Save Station Magmoor A
                           CurrentRoom == 0x1C;   // Save Station Magmoor B
                }
                else if (CurrentWorld == 0x13) // Phazon Mines
                {
                    return CurrentRoom == 0x04 || // Save Station Mines A
                           CurrentRoom == 0x1E || // Save Station Mines B
                           CurrentRoom == 0x22;   // Save Station Mines C
                }
                else if (CurrentWorld == 0x18) // Chozo Ruins
                {
                    return CurrentRoom == 0x16 || // Save Station 1
                           CurrentRoom == 0x27 || // Save Station 2
                           CurrentRoom == 0x3B;   // Save Station 3
                }
                else if (CurrentWorld == 0x19) // Tallon Overworld
                {
                    return CurrentRoom == 0x00 || // Landing Site
                           CurrentRoom == 0x1C;   // Save Station in Crashed Frigate
                }
                else if (CurrentWorld == 0x1B) // Phendrana Drifts
                {
                    return CurrentRoom == 0x04 || // Save Station B
                           CurrentRoom == 0x11 || // Save Station A
                           CurrentRoom == 0x21 || // Save Station D
                           CurrentRoom == 0x2D;   // Save Station C
                }

                return false;
            }
        }

        internal bool IsAlive
        {
            get
            {
                return Health > 0;
            }
        }
    }
}