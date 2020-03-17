﻿namespace Prime.Memory.Constants
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
        internal const long OFF_POWERBEAM_OBTAINED = 0x2F;
        internal const long OFF_ICEBEAM_OBTAINED = 0x37;
        internal const long OFF_WAVEBEAM_OBTAINED = 0x3F;
        internal const long OFF_PLASMABEAM_OBTAINED = 0x47;
        internal const long OFF_MISSILES = 0x4B;
        internal const long OFF_MAX_MISSILES = OFF_MISSILES + 4;
        internal const long OFF_SCANVISOR_OBTAINED = 0x57;
        internal const long OFF_MORPHBALLBOMBS_OBTAINED = 0x5F;
        internal const long OFF_POWERBOMBS = 0x63;
        internal const long OFF_MAX_POWERBOMBS = OFF_POWERBOMBS + 4;
        internal const long OFF_FLAMETHROWER_OBTAINED = 0x6F;
        internal const long OFF_THERMALVISOR_OBTAINED = 0x77;
        internal const long OFF_CHARGEBEAM_OBTAINED = 0x7F;
        internal const long OFF_SUPERMISSILE_OBTAINED = 0x87;
        internal const long OFF_GRAPPLEBEAM_OBTAINED = 0x8F;
        internal const long OFF_XRAYVISOR_OBTAINED = 0x97;
        internal const long OFF_ICESPREADER_OBTAINED = 0x9F;
        internal const long OFF_SPACEJUMPBOOTS_OBTAINED = 0xA7;
        internal const long OFF_MORPHBALL_OBTAINED = 0xAF;
        internal const long OFF_COMBATVISOR_OBTAINED = 0xB7;
        internal const long OFF_BOOSTBALL_OBTAINED = 0xBF;
        internal const long OFF_SPIDERBALL_OBTAINED = 0xC7;
        internal const long OFF_POWERSUIT_OBTAINED = 0xCF;
        internal const long OFF_GRAVITYSUIT_OBTAINED = 0xD7;
        internal const long OFF_VARIASUIT_OBTAINED = 0xDF;
        internal const long OFF_PHAZONSUIT_OBTAINED = 0xE7;
        internal const long OFF_ENERGYTANKS_OBTAINED = 0xEF;
        internal const long OFF_ENERGYREFILL_OBTAINED = 0xFF;
        internal const long OFF_WAVEBUSTER_OBTAINED = 0x10F;
        internal const long OFF_ARTIFACT_OF_TRUTH_OBTAINED = 0x117;
        internal const long OFF_ARTIFACT_OF_STRENGTH_OBTAINED = 0x11F;
        internal const long OFF_ARTIFACT_OF_ELDER_OBTAINED = 0x127;
        internal const long OFF_ARTIFACT_OF_WILD_OBTAINED = 0x12F;
        internal const long OFF_ARTIFACT_OF_LIFEGIVER_OBTAINED = 0x137;
        internal const long OFF_ARTIFACT_OF_WARRIOR_OBTAINED = 0x13F;
        internal const long OFF_ARTIFACT_OF_CHOZO_OBTAINED = 0x147;
        internal const long OFF_ARTIFACT_OF_NATURE_OBTAINED = 0x14F;
        internal const long OFF_ARTIFACT_OF_SUN_OBTAINED = 0x157;
        internal const long OFF_ARTIFACT_OF_WORLD_OBTAINED = 0x15F;
        internal const long OFF_ARTIFACT_OF_SPIRIT_OBTAINED = 0x167;
        internal const long OFF_ARTIFACT_OF_NEWBORN_OBTAINED = 0x16F;

        internal abstract long CGameState { get; }
        internal abstract long CPlayerState { get; }
        internal abstract long CWorld { get; }
        internal abstract uint CurrentWorld { get; }
        internal abstract uint CurrentRoom { get; }
        internal abstract ushort Health { get; set; }
        internal abstract ushort MaxHealth { get; }
        internal abstract long IGT { get; }
        internal abstract string IGTAsStr { get; }

        internal abstract int MorphBallBombs { get; set; }
        internal abstract int MaxMorphBallBombs { get; }
        internal abstract int Missiles { get; set; }
        internal abstract int MaxMissiles { get; set; }
        internal abstract int PowerBombs { get; set; }
        internal abstract int MaxPowerBombs { get; set; }
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
    }
}