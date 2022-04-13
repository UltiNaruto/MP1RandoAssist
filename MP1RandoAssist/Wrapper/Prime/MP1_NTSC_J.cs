using System;

namespace Wrapper.Prime
{
    internal class MP1_NTSC_J : Prime
    {
        protected const long OFF_CGAMEGLOBALOBJECTS = 0x80440618;
        protected const long OFF_CGAMESTATE = OFF_CGAMEGLOBALOBJECTS + 0x134;
        protected const long OFF_CSTATEMANAGER = 0x80443030;
        protected const long OFF_MORPHBALLBOMBS_COUNT = 0x80440BA0;
        protected const long OFF_BASEHEALTH = 0x805939F8;
        protected const long OFF_ENERGYTANK_CAPACITY = OFF_BASEHEALTH - 4;

        protected override long CWorld
        {
            get
            {
                return GCMem.ReadUInt32(OFF_CSTATEMANAGER + OFF_CWORLD);
            }
        }

        protected override long CGameState
        {
            get
            {
                return GCMem.ReadUInt32(OFF_CGAMESTATE);
            }
        }

        protected override long CPlayerState
        {
            get
            {
                long result = GCMem.ReadUInt32(OFF_CSTATEMANAGER + OFF_CPLAYERSTATE);
                if (result == 0)
                    return 0;
                return GCMem.ReadUInt32(result);
            }
        }

        protected override long CWorldLayerState(uint world, uint area_id)
        {
            if (CGameState == 0)
                return 0;
            long result = GCMem.ReadUInt32(CGameState + OFF_CGAMESTATE_WORLDLAYERSTATE_ARRAY);
            if (result == 0)
                return 0;
            for (int i = 0; i < 7; i++)
            {
                if (GCMem.ReadUInt32(result) == world)
                {
                    result = GCMem.ReadUInt32(result + 0x14);
                    if (result == 0)
                        return 0;
                    result = GCMem.ReadUInt32(result);
                    if (result == 0)
                        return 0;
                    if (area_id >= GCMem.ReadUInt32(result + 0x4))
                        return 0;
                    result = GCMem.ReadUInt32(result + 0xC);
                    if (result == 0)
                        return 0;
                    return result + area_id * 0x10;
                }
                result += 0x18;
            }
            return 0;
        }

        protected override long _IGT
        {
            get
            {
                if (CGameState == 0)
                    return 0;
                return (long)(GCMem.ReadFloat64(CGameState + OFF_CGAMESTATE_PLAYTIME) * 1000);
            }
        }

        protected override int MorphState
        {
            get
            {
                if (CPlayer == 0)
                    return 3;
                return GCMem.ReadInt32(CPlayer + OFF_CPLAYER_MORPHSTATE + 0x10);
            }

            set
            {
                if (CPlayer == 0)
                    return;
                GCMem.WriteInt32(CPlayer + OFF_CPLAYER_MORPHSTATE + 0x10, value);
            }
        }

        public override uint CurrentWorld
        {
            get
            {
                if (CWorld == 0)
                    return UInt32.MaxValue;
                return GCMem.ReadUInt32(CWorld + OFF_CWORLD_MLVL);
            }
        }

        public override uint CurrentArea
        {
            get
            {
                if (CWorld == 0)
                    return UInt32.MaxValue;
                return GCMem.ReadUInt32(CWorld + OFF_CWORLD_MREAID);
            }
        }

        protected override int Health
        {
            get
            {
                if (CPlayerState == 0)
                    return 0;
                return (int)GCMem.ReadFloat32(CPlayerState + OFF_HEALTH);
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteFloat32(CPlayerState + OFF_HEALTH, value);
            }
        }

        public override int GetBaseHealth()
        {
            return (int)GCMem.ReadFloat32(OFF_BASEHEALTH);
        }

        public override int GetEtankCapacity()
        {
            return (int)GCMem.ReadFloat32(OFF_ENERGYTANK_CAPACITY);
        }

        public override uint CurrentSuitVisual
        {
            get
            {
                if (CPlayerState == 0)
                    return 0;
                return GCMem.ReadUInt32(CPlayerState + OFF_CURRENT_SUIT_VISUAL);
            }
            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteUInt32(CPlayerState + OFF_CURRENT_SUIT_VISUAL, value);
            }
        }

        protected override int Missiles
        {
            get
            {
                if (CPlayerState == 0)
                    return 0;
                return GCMem.ReadInt32(CPlayerState + OFF_MISSILES);
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_MISSILES, value);
            }
        }

        protected override int MaxMissiles
        {
            get
            {
                if (CPlayerState == 0)
                    return 0;
                return GCMem.ReadInt32(CPlayerState + OFF_MAX_MISSILES);
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_MAX_MISSILES, value);
            }
        }

        protected override int PowerBombs
        {
            get
            {
                if (CPlayerState == 0)
                    return 0;
                return GCMem.ReadInt32(CPlayerState + OFF_POWERBOMBS);
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_POWERBOMBS, value);
            }
        }

        protected override int MaxPowerBombs
        {
            get
            {
                if (CPlayerState == 0)
                    return 0;
                return GCMem.ReadInt32(CPlayerState + OFF_MAX_POWERBOMBS);
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_MAX_POWERBOMBS, value);
            }
        }

        protected override int MaxEnergyTanks
        {
            get
            {
                if (CPlayerState == 0)
                    return 0;
                return GCMem.ReadInt32(CPlayerState + OFF_ENERGYTANKS_OBTAINED);
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_ENERGYTANKS_OBTAINED, value);
            }
        }

        protected override int MorphBallBombs
        {
            get => GCMem.ReadInt32(OFF_MORPHBALLBOMBS_COUNT);
            set => GCMem.WriteInt32(OFF_MORPHBALLBOMBS_COUNT, value);
        }

        protected override bool HaveIceBeam
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_ICEBEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_ICEBEAM_OBTAINED - 4, value ? 1 : 0);
                GCMem.WriteInt32(CPlayerState + OFF_ICEBEAM_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveWaveBeam
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_WAVEBEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_WAVEBEAM_OBTAINED - 4, value ? 1 : 0);
                GCMem.WriteInt32(CPlayerState + OFF_WAVEBEAM_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HavePlasmaBeam
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_PLASMABEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_PLASMABEAM_OBTAINED - 4, value ? 1 : 0);
                GCMem.WriteInt32(CPlayerState + OFF_PLASMABEAM_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveMorphBallBombs
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_MORPHBALLBOMBS_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_MORPHBALLBOMBS_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveFlamethrower
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_FLAMETHROWER_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_FLAMETHROWER_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveScanVisor
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_SCANVISOR_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_SCANVISOR_OBTAINED - 4, value ? 1 : 0);
                GCMem.WriteInt32(CPlayerState + OFF_SCANVISOR_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveThermalVisor
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_THERMALVISOR_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_THERMALVISOR_OBTAINED - 4, value ? 1 : 0);
                GCMem.WriteInt32(CPlayerState + OFF_THERMALVISOR_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveChargeBeam
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_CHARGEBEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_CHARGEBEAM_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveSuperMissile
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_SUPERMISSILE_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_SUPERMISSILE_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveGrappleBeam
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_GRAPPLEBEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_GRAPPLEBEAM_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveXRayVisor
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_XRAYVISOR_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_XRAYVISOR_OBTAINED - 4, value ? 1 : 0);
                GCMem.WriteInt32(CPlayerState + OFF_XRAYVISOR_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveIceSpreader
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_ICESPREADER_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_ICESPREADER_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveSpaceJumpBoots
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_SPACEJUMPBOOTS_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_SPACEJUMPBOOTS_OBTAINED - 4, value ? 1 : 0);
                GCMem.WriteInt32(CPlayerState + OFF_SPACEJUMPBOOTS_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveMorphBall
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_MORPHBALL_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_MORPHBALL_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveBoostBall
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_BOOSTBALL_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_BOOSTBALL_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveSpiderBall
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_SPIDERBALL_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_SPIDERBALL_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveGravitySuit
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_GRAVITYSUIT_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_GRAVITYSUIT_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveVariaSuit
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_VARIASUIT_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_VARIASUIT_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HavePhazonSuit
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_PHAZONSUIT_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_PHAZONSUIT_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveWavebuster
        {
            get
            {
                if (CPlayerState == 0)
                    return false;
                return GCMem.ReadInt32(CPlayerState + OFF_WAVEBUSTER_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == 0)
                    return;
                GCMem.WriteInt32(CPlayerState + OFF_WAVEBUSTER_OBTAINED, value ? 1 : 0);
            }
        }

        protected override bool HaveArtifact(int index)
        {
            if (CPlayerState == 0)
                return false;
            if (index < 0)
                throw new Exception("Index can't be negative");
            switch (index)
            {
                case 0:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_TRUTH_OBTAINED) > 0;
                case 1:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_STRENGTH_OBTAINED) > 0;
                case 2:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_ELDER_OBTAINED) > 0;
                case 3:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_WILD_OBTAINED) > 0;
                case 4:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_LIFEGIVER_OBTAINED) > 0;
                case 5:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_WARRIOR_OBTAINED) > 0;
                case 6:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_CHOZO_OBTAINED) > 0;
                case 7:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_NATURE_OBTAINED) > 0;
                case 8:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_SUN_OBTAINED) > 0;
                case 9:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_WORLD_OBTAINED) > 0;
                case 10:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_SPIRIT_OBTAINED) > 0;
                case 11:
                    return GCMem.ReadInt32(CPlayerState + OFF_ARTIFACT_OF_NEWBORN_OBTAINED) > 0;
                default:
                    throw new Exception("There are no artifacts past the 12th artifact");
            }
        }

        protected override void SetArtifact(int index, bool obtained)
        {
            if (CPlayerState == 0)
                return;
            if (index < 0)
                throw new Exception("Index can't be negative");
            switch (index)
            {
                case 0:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_TRUTH_OBTAINED, obtained ? 1 : 0);
                    break;
                case 1:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_STRENGTH_OBTAINED, obtained ? 1 : 0);
                    break;
                case 2:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_ELDER_OBTAINED, obtained ? 1 : 0);
                    break;
                case 3:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_WILD_OBTAINED, obtained ? 1 : 0);
                    break;
                case 4:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_LIFEGIVER_OBTAINED, obtained ? 1 : 0);
                    break;
                case 5:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_WARRIOR_OBTAINED, obtained ? 1 : 0);
                    break;
                case 6:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_CHOZO_OBTAINED, obtained ? 1 : 0);
                    break;
                case 7:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_NATURE_OBTAINED, obtained ? 1 : 0);
                    break;
                case 8:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_SUN_OBTAINED, obtained ? 1 : 0);
                    break;
                case 9:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_WORLD_OBTAINED, obtained ? 1 : 0);
                    break;
                case 10:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_SPIRIT_OBTAINED, obtained ? 1 : 0);
                    break;
                case 11:
                    GCMem.WriteInt32(CPlayerState + OFF_ARTIFACT_OF_NEWBORN_OBTAINED, obtained ? 1 : 0);
                    break;
                default:
                    throw new Exception("There are no artifacts past the 12th artifact");
            }
        }
    }
}
