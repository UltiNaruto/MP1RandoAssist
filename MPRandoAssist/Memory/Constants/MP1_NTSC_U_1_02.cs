using System;

namespace Prime.Memory.Constants
{
    internal class MP1_NTSC_U_1_02 : _MP1
    {
        internal const long OFF_CGAMEGLOBALOBJECTS = 0x4587F8;
        internal const long OFF_CGAMESTATE = OFF_CGAMEGLOBALOBJECTS + 0x134;
        internal const long OFF_CSTATEMANAGER = 0x45B208;
        internal const long OFF_MORPHBALLBOMBS_COUNT = 0x458D78;

        internal override long CGameState
        {
            get
            {
                long result = Dolphin.ReadUInt32(GC.RAMBaseAddress + OFF_CGAMESTATE);
                if (result < GC.RAMBaseAddress)
                    return -1;
                return result;
            }
        }

        internal override long CPlayerState
        {
            get
            {
                long result = Dolphin.ReadUInt32(GC.RAMBaseAddress + OFF_CSTATEMANAGER + OFF_CPLAYERSTATE);
                if (result < GC.RAMBaseAddress)
                    return -1;
                result = Dolphin.ReadUInt32(result);
                if (result < GC.RAMBaseAddress)
                    return -1;
                return result;
            }
        }

        internal override long CWorld
        {
            get
            {
                long result = Dolphin.ReadUInt32(GC.RAMBaseAddress + OFF_CSTATEMANAGER + OFF_CWORLD);
                if (result < GC.RAMBaseAddress)
                    return -1;
                return result;
            }
        }

        internal override uint CurrentWorld
        {
            get
            {
                if (CWorld == -1)
                    return UInt32.MaxValue;
                return Dolphin.ReadUInt32(CWorld + OFF_WORLD_ID);
            }
        }

        internal override uint CurrentRoom
        {
            get
            {
                if (CWorld == -1)
                    return UInt32.MaxValue;
                return Dolphin.ReadUInt32(CWorld + OFF_ROOM_ID);
            }
        }

        internal override ushort Health
        {
            get
            {
                if (CPlayerState == -1)
                    return 0;
                return (ushort)Dolphin.ReadFloat32(CPlayerState + OFF_HEALTH);
            }
            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteFloat32(CPlayerState + OFF_HEALTH, (float)value);
            }
        }

        internal override ushort MaxHealth
        {
            get
            {
                if (CPlayerState == -1)
                    return 0;
                return (ushort)(Dolphin.ReadUInt32(CPlayerState + OFF_ENERGYTANKS_OBTAINED) * 100 + 99);
            }
        }

        internal override long IGT
        {
            get
            {
                if (CGameState == -1)
                    return -1;
                return (long)(Dolphin.ReadFloat64(CGameState + OFF_PLAYTIME) * 1000);
            }

            set
            {
                if (CGameState == -1)
                    return;
                Dolphin.WriteFloat64(CGameState + OFF_PLAYTIME, (double)value / 1000.0);
            }
        }

        internal override string IGTAsStr
        {
            get
            {
                if (IGT == -1)
                    return "00:00:00.000";
                return String.Format("{0:00}:{1:00}:{2:00}.{3:000}", IGT / (60 * 60 * 1000), (IGT / (60 * 1000)) % 60, (IGT / 1000) % 60, IGT % 1000);
            }
        }

        internal override uint CurrentSuitVisual
        {
            get
            {
                if (CPlayerState == -1)
                    return 0;
                return Dolphin.ReadUInt32(CPlayerState + OFF_CURRENT_SUIT_VISUAL);
            }
            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_CURRENT_SUIT_VISUAL, value);
            }
        }

        internal override uint MorphBallBombs
        {
            get
            {
                if (CGameState == -1)
                    return 0;
                return Dolphin.ReadUInt32(GC.RAMBaseAddress + OFF_MORPHBALLBOMBS_COUNT);
            }

            set
            {
                if (CGameState == -1)
                    return;
                Dolphin.WriteUInt32(GC.RAMBaseAddress + OFF_MORPHBALLBOMBS_COUNT, value);
            }
        }

        internal override uint MaxMorphBallBombs
        {
            get
            {
                return 3;
            }
        }

        internal override uint Missiles
        {
            get
            {
                if (CPlayerState == -1)
                    return 0;
                return Dolphin.ReadUInt32(CPlayerState + OFF_MISSILES);
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_MISSILES, (byte)value);
            }
        }

        internal override uint MaxMissiles
        {
            get
            {
                if (CPlayerState == -1)
                    return 0;
                return Dolphin.ReadUInt32(CPlayerState + OFF_MAX_MISSILES);
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_MAX_MISSILES, (byte)value);
            }
        }

        internal override bool HaveScanVisor
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_SCANVISOR_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_SCANVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override uint PowerBombs
        {
            get
            {
                if (CPlayerState == -1)
                    return 0;
                return Dolphin.ReadUInt32(CPlayerState + OFF_POWERBOMBS);
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_POWERBOMBS, (byte)value);
            }
        }

        internal override uint MaxPowerBombs
        {
            get
            {
                if (CPlayerState == -1)
                    return 0;
                return Dolphin.ReadUInt32(CPlayerState + OFF_MAX_POWERBOMBS);
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_MAX_POWERBOMBS, (byte)value);
            }
        }

        internal override bool HaveIceBeam
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_ICEBEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_ICEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveWaveBeam
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_WAVEBEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_WAVEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HavePlasmaBeam
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_PLASMABEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_PLASMABEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveMorphBallBombs
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_MORPHBALLBOMBS_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_MORPHBALLBOMBS_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveFlamethrower
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_FLAMETHROWER_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_FLAMETHROWER_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveThermalVisor
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_THERMALVISOR_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_THERMALVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveChargeBeam
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_CHARGEBEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_CHARGEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveSuperMissile
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_SUPERMISSILE_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_SUPERMISSILE_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveGrappleBeam
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_GRAPPLEBEAM_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_GRAPPLEBEAM_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveXRayVisor
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_XRAYVISOR_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_XRAYVISOR_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveIceSpreader
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_ICESPREADER_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_ICESPREADER_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveSpaceJumpBoots
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_SPACEJUMPBOOTS_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_SPACEJUMPBOOTS_OBTAINED, (byte)(value ? 1 : 0));
                Dolphin.WriteUInt32(CPlayerState + OFF_SPACEJUMPBOOTS_OBTAINED + 4, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveMorphBall
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_MORPHBALL_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_MORPHBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveBoostBall
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_BOOSTBALL_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_BOOSTBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveSpiderBall
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_SPIDERBALL_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_SPIDERBALL_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveGravitySuit
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_GRAVITYSUIT_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_GRAVITYSUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveVariaSuit
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_VARIASUIT_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_VARIASUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HavePhazonSuit
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_PHAZONSUIT_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_PHAZONSUIT_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool HaveWavebuster
        {
            get
            {
                if (CPlayerState == -1)
                    return false;
                return Dolphin.ReadUInt32(CPlayerState + OFF_WAVEBUSTER_OBTAINED) > 0;
            }

            set
            {
                if (CPlayerState == -1)
                    return;
                Dolphin.WriteUInt32(CPlayerState + OFF_WAVEBUSTER_OBTAINED, (byte)(value ? 1 : 0));
            }
        }

        internal override bool Artifacts(int index)
        {
            if (CPlayerState == -1)
                return false;
            if (index < 0)
                throw new Exception("Index can't be negative");
            switch (index)
            {
                case 0:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_TRUTH_OBTAINED) > 0;
                case 1:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_STRENGTH_OBTAINED) > 0;
                case 2:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_ELDER_OBTAINED) > 0;
                case 3:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_WILD_OBTAINED) > 0;
                case 4:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_LIFEGIVER_OBTAINED) > 0;
                case 5:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_WARRIOR_OBTAINED) > 0;
                case 6:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_CHOZO_OBTAINED) > 0;
                case 7:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_NATURE_OBTAINED) > 0;
                case 8:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_SUN_OBTAINED) > 0;
                case 9:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_WORLD_OBTAINED) > 0;
                case 10:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_SPIRIT_OBTAINED) > 0;
                case 11:
                    return Dolphin.ReadUInt32(CPlayerState + OFF_ARTIFACT_OF_NEWBORN_OBTAINED) > 0;
                default:
                    throw new Exception("There are no artifacts past the 12th artifact");
            }
        }
    }
}
