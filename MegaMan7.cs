using System;
using System.Collections.Generic;
using System.Linq;
using ConnectorLib;
using CrowdControl.Common;
using JetBrains.Annotations;
using ConnectorType = CrowdControl.Common.ConnectorType;

namespace CrowdControl.Games.Packs
{
    [UsedImplicitly]
    public class MegaMan7 : SNESEffectPack
    {
        public MegaMan7([NotNull] Player player, [NotNull] Func<CrowdControlBlock, bool> responseHandler, [NotNull] Action<object> statusUpdateHandler) : base(player, responseHandler, statusUpdateHandler) { }

        private volatile bool _quitting = false;
        protected override void Dispose(bool disposing)
        {
            _quitting = true;
            base.Dispose(disposing);
        }

        private const uint ADDR_Brightness = 0x7E00AD;
        private const uint ADDR_Mosaic = 0x7E00C8;
        private const uint ADDR_indexL1 = 0x7E00DF;
        private const uint ADDR_indexL2 = 0x7E00E0;
        private const uint ADDR_DED = 0x7E05E8;
        private const uint ADDR_slide = 0x7E05F0;
        private const uint ADDR_moon = 0x7E05F1;
        private const uint ADDR_newlevel = 0x7E05F2;
        private const uint ADDR_newlevelF = 0x7E05F3;
        private const uint ADDR_immuneF = 0x7E05F4;
        private const uint ADDR_weaponL = 0x7E05F6;
        private const uint ADDR_NewWeaponF = 0x7E05F7;
        private const uint ADDR_NewWeapon = 0x7E05F8;
        private const uint ADDR_swap = 0x7E05F9;
        private const uint ADDR_invert = 0x7E05FB;
        private const uint ADDR_OHKO = 0x7E05FD;
        private const uint ADDR_Lives = 0x7E0B81;
        private const uint ADDR_ammoB = 0x7E0B83;
        private const uint ADDR_Etanks = 0x7E0BA0;
        private const uint ADDR_Wtanks = 0x7E0BA1;
        private const uint ADDR_Stanks = 0x7E0BA2;
        private const uint ADDR_bolts = 0x7E0BA6;
        private const uint ADDR_currentW = 0x7E0BC7;
        private const uint ADDR_BossIndex = 0x7E0BD4;
        private const uint ADDR_BossHP_old = 0x7E0BD3;
        private const uint ADDR_menu = 0x7E0BD9;
        private const uint ADDR_State = 0x7E0C02;
        private const uint ADDR_adapterF = 0x7E0C0A;
        private const uint ADDR_HP = 0x7E0C2E;
        private const uint ADDR_IFRAMES = 0x7E0C2F;
        private const uint ADDR_jmpH = 0x7E05E6;

        private volatile bool ohko = false;
        private volatile bool screenOff = false;
        private volatile bool mosaic = false;
        private volatile bool jumpH = false;
        private volatile bool jumpL = false;

        private Dictionary<string, (string weapon,byte index)> refillD = new Dictionary<string, (string weapon,byte index)>(StringComparer.OrdinalIgnoreCase)
        {
            {"buster",("Mega Buster",0) },
            {"freeze",("Freeze Cracker",2) },
            {"thunder",("Thunder Bolt",4) },
            {"junk",("Junk Shield",6) },
            {"burner",("Scorch Wheel",8) },
            {"slash",("Slash Claw",10) },
            {"noise",("Noise Crush",12) },
            {"danger",("Danger Wrap",14) },
            {"coil",("Wild Coil",16) },
            {"rushcoil",("Rush Coil",24) },
            {"rushjet",("Rush Jet",22) },
            {"rushsearch",("Rush Search",20) }
        };

        private Dictionary<string, (string boss, byte index)> robotD = new Dictionary<string, (string boss, byte index)>(StringComparer.OrdinalIgnoreCase)
        {
            {"freeze",("Freeze Man",2) },
            {"cloud",("Cloud Man",4) },
            {"junk",("Junk Man",6) },
            {"turbo",("Turbo Man",8) },
            {"slash",("Slash Man",10) },
            {"noise",("Shade Man",12) },
            {"burst",("Burst Man",14) },
            {"coil",("Spring Man",16) }
        };
        private Dictionary<string, (string stagename, byte id)> stageD = new Dictionary<string, (string stagename, byte id)>(StringComparer.OrdinalIgnoreCase)
        {
            {"intro",("Intro",0) },
            {"freeze",("Freeze Man's Stage",1) },
            {"cloud",("Cloud Man's Stage",2) },
            {"junk",("Junk Man's Stage",3) },
            {"turbo",("Turbo Man's Stage",4) },
            {"slash",("Slash Man's Stage",5) },
            {"noise",("Shade Man's Stage",6) },
            {"burst",("Burst Man's Stage",7) },
            {"coil",("Spring Man's Stage",8) },
            {"museum",("Robot Museum",9) },
            {"w1",("Wily Stage 1",10) },
            {"w2",("Wily Stage 2",11)},
            {"w3",("Wily Stage 3",12) },
            {"w4",("Wily Stage 4",13) }
        };

        private Dictionary<string, (string weapon, byte index)> weaponD = new Dictionary<string, (string weapon, byte index)>(StringComparer.OrdinalIgnoreCase)
        {
            {"buster",("Mega Buster",0) },
            {"freeze",("Freeze Cracker",2) },
            {"thunder",("Thunder Bolt",4) },
            {"junk",("Junk Shield",6) },
            {"burner",("Scorch Wheel",8) },
            {"slash",("Slash Claw",10) },
            {"noise",("Noise Crush",12) },
            {"danger",("Danger Wrap",14) },
            {"coil",("Wild Coil",16) },
            {"rushcoil",("Rush Coil",24) },
            {"rushjet",("Rush Jet",22) },
            {"rushsearch",("Rush Search",20) },
            {"sheild",("Proto Sheild",18) },
            {"adapter",("Super Adapter",28) }
        };

        /*Effect List*/
        public override List<Effect> Effects
        {
            get
            {
                List<Effect> effects = new List<Effect>
                {
                    new Effect("Give Lives","lives", new[] {"quantity9"}),
                    new Effect("Take Away Lives","lives2", new[] {"quantity9"}),
                    new Effect("Give bolts","bolt", new[] {"quantity65535"}),
                    new Effect("Take Away bolts","bolt2", new[] {"quantity65535"}),
                    new Effect("Give E-Tank","etank"),
                    new Effect("Give W-Tank","wtank"),
                    new Effect("Give S-Tank","stank"),
                    new Effect("Take Away E-Tank","etank2"),
                    new Effect("Take Away W-Tank","wtank2"),
                    new Effect("Take Away S-Tank","stank2"),
                    new Effect("Invert Buttons & D-Pad (25 seconds)","invert"),
                    new Effect("Swap Buttons & D-Pad (25 seconds)","swap"),
                    new Effect("Refill Weapon Energy","refill",ItemKind.Folder),
                    new Effect("Rebuild Robot Master","rebuild",ItemKind.Folder),
                    new Effect("Weapon Lock (45 seconds)","lock",ItemKind.Folder),
                    new Effect("Warp to Level","warp",ItemKind.Folder),
                    new Effect("Kill MegaMan","dead"),
                    new Effect("One-Hit KO (15 seconds)","ohko"),
                    new Effect("Refill Health","hpfull"),
                    new Effect("Boss E-Tank","bosshpfull"),
                    new Effect("Grant MegaMan Invulnerability (15 seconds)","iframes"),
                    new Effect("Grant Enemies Invulernability (30 seconds)","enemyiframes"),
                    new Effect("Low Gravity (15 seconds)","low"),
                    new Effect("High Gravity (15 second)","high"),
                    new Effect("No Slide (30 seconds)","slideoff"),
                    new Effect("Turn the Screen Off (15 seconds)","off"),
                    new Effect("Enable Mosaic Effect (15 seconds)","mosaic")
                };
                effects.AddRange(refillD.Skip(1).Select(t => new Effect($"Refill {t.Value.weapon}", $"refill_{t.Key}", "refill")));
                effects.AddRange(robotD.Take(8).Select(t => new Effect($"Rebuild {t.Value.boss}", $"rebuild_{t.Key}", "rebuild")));
                effects.AddRange(weaponD.Select(t => new Effect($"Force Weapon to {t.Value.weapon}", $"lock_{t.Key}", "lock")));
                effects.AddRange(stageD.Skip(1).Select(t => new Effect($"Warp to {t.Value.stagename}", $"warp_{t.Key}", "warp")));
                return effects;
            }
        }

        public override List<ItemType> ItemTypes => new List<ItemType>(new[]
        {
            new ItemType("Quantity", "quantity9", ItemType.Subtype.Slider, "{\"min\":1,\"max\":9}"),
            new ItemType("Quantity", "quantity65535", ItemType.Subtype.Slider, "{\"min\":1,\"max\":999}")
        });

        //Adding ROM INFO
        public override List<ROMInfo> ROMTable => new List<ROMInfo>(new[]
        {
           new ROMInfo("Rockman 7 - Shukumei no Taiketsu! (J)", "MM7_CC.bps", Patching.BPS,ROMStatus.NotSupported, s=> Patching.MD5(s,"E9C126CBD7C68C9E985DC501F625B030")),
           new ROMInfo("Mega Man 7 (U)", "MM7_CC.bps", Patching.BPS,ROMStatus.ValidPatched, s=> Patching.MD5(s,"301D8C4F1B5DE2CD10B68686B17B281A")),
           new ROMInfo("Mega Man 7 (U) (Headered)","MM7_CC",(stream,bytes)=>
           {var deheader = Patching.Truncate(stream,0x200,0x200000);
            return deheader.success ? Patching.BPS(stream, bytes) : deheader;
           }, ROMStatus.ValidPatched, s=> Patching.MD5(s,"301D8C4F1B5DE2CD10B68686B17B281A"))
        });

        public override List<(string, Action)> MenuActions => new List<(string, Action)>();

        public override Game Game { get; } = new Game(22, "Mega Man 7", "MegaMan7", "SNES", ConnectorType.SNESConnector);

        protected override bool IsReady(EffectRequest request) => Connector.Read8(ADDR_indexL1, out byte b) && Connector.Read8(ADDR_indexL2,out byte c) && b != 0 && b != 8 && b != 2;

        protected override void RequestData(DataRequest request) => Respond(request, request.Key, null, false, $"Variable name \"{request.Key}\" not known");

        protected override void StartEffect(EffectRequest request)
        {
            if (!IsReady(request))
            {
                DelayEffect(request);
                return;
            }
            string[] effectT = request.FinalCode.Split('_');
            switch (effectT[0])
            {
                case "hpfull":
                    {
                        TryEffect(request,
                            () => Connector.Read8(ADDR_HP, out byte hp) && (hp < 14) && !ohko,
                            () => Connector.Write8(ADDR_HP, 0x1C),
                            () =>
                            {
                                Connector.SendMessage($"{request.DisplayViewer} refilled your health.");
                            });
                        return;
                    }
                case "bolt":
                    {
                        ushort bolt = (ushort)request.AllItems[1].Reduce();
                        TryEffect(request,
                            () => Connector.RangeAdd16(ADDR_bolts, bolt, 0, 0x3E7, false),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} sent you {bolt} bolts."); }
                            );
                        return;
                    }
                case "bolt2":
                    {
                        short bolt = (short)request.AllItems[1].Reduce();
                        TryEffect(request,
                            () => Connector.RangeAdd16(ADDR_bolts, -bolt, 0, 0x3E7, false),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} toke away {bolt} bolts."); }
                            );
                        return;
                    }
                case "etank":
                    {
                        TryEffect(request,
                            () => Connector.RangeAdd8(ADDR_Etanks, 1, 0, 4, false),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} sent you an E-Tank"); });
                        return;
                    }
                case "wtank":
                    {
                        TryEffect(request,
                            () => Connector.RangeAdd8(ADDR_Wtanks, 1, 0, 4, false),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} sent you an W-Tank"); });
                        return;
                    }
                case "stank":
                    {
                        
                        TryEffect(request,
                            () => Connector.RangeAdd8(ADDR_Stanks, 1, 0, 1, false),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} sent you an W-Tank"); });
                        return;
                        }
                case "etank2":
                    {
                        TryEffect(request,
                            () =>  Connector.RangeDecrement8(ADDR_Etanks, 0),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} toke away a E-Tank"); });
                        return;
                    }
                case "wtank2":
                    {
                        TryEffect(request,
                            () => Connector.RangeDecrement8(ADDR_Wtanks, 0),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} toke away a W-Tank"); });
                        return;
                    }
                case "stank2":
                    {
                        TryEffect(request,
                            () => Connector.RangeDecrement8(ADDR_Stanks, 0),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} toke away a S-Tank"); });
                        return;
                    }
                case "swap":
                    {
                        var a = RepeatAction(request,
                            TimeSpan.FromSeconds(25),
                            () => Connector.Read16(ADDR_indexL1, out ushort l) && Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_State - 1, out ushort status) && l == 0x206 && (status&0xFF) == 2 && (status>>8) != 0x1C,
                            () => Connector.Write8(ADDR_swap, 1),
                            TimeSpan.FromMilliseconds(500), /*<= Start Retry Timer*/
                            () => Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x0206 && status == 2, /*Refresh Condition*/
                            TimeSpan.FromMilliseconds(500), /*Refresh Retry Timer*/
                            () => true,
                            TimeSpan.FromSeconds(1),
                            true, "swap"
                            );
                        a.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} deployed an swap buttons field."));
                        a.WhenCompleted.Then(t =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer}'s swap buttons field has dispered.");
                            Connector.Write8(ADDR_swap, 0);
                        });
                        return;
                    }
                case "invert":
                    {
                        var a = RepeatAction(request,
                            TimeSpan.FromSeconds(25),
                            () => Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read16(ADDR_State - 1, out ushort status) && Connector.IsZero8(ADDR_menu) && l == 0x206 && (status&0xFF) == 2 && (status>>8) != 0x1C,
                            () => Connector.Write8(ADDR_invert, 1),
                            TimeSpan.FromMilliseconds(500), /*<= Start Retry Timer*/
                            () => Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x0206 && status == 2, /*Refresh Condition*/
                            TimeSpan.FromMilliseconds(500), /*Refresh Retry Timer*/
                            () => true,
                            TimeSpan.FromSeconds(1),
                            true, "invert"
                            );
                        a.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} deployed an invert buttons field."));
                        a.WhenCompleted.Then(t =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer}'s swap invert field has dispered.");
                            Connector.Write8(ADDR_invert, 0);
                        });
                        return;
                    }
                case "refill":
                    {
                        var index = refillD[effectT[1]].index;
                        TryEffect(request,
                            () => Connector.Read8((ulong)(0x7E0000 + 0xB83 + index), out byte ammo) && (ammo&0x3F) <= 6 && (ammo & 0xC0) != 0,
                            () => Connector.Read8((ulong)(0x7E0000 + 0xB83 + index), out byte ammo) && Connector.Write8((ulong)0x7E0000 + 0xB83 + index, (byte)((ammo & 0xC0) + 28)),
                            () => { Connector.SendMessage($"{request.DisplayViewer} refilled {refillD[effectT[1]].weapon}."); });
                        return;
                    }
                case "lock":
                    {

                        byte orgWeapon = 0;
                        var d = RepeatAction(request,
                            TimeSpan.FromSeconds(45),
                            () => Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && Connector.IsZero8(ADDR_menu) && Connector.Read8(ADDR_State, out byte state) && l == 0x206 && status == 2 && state != 0x18 && state != 0x1C,
                            () => Connector.Write8(ADDR_weaponL,1)&&Connector.Read8(ADDR_currentW,out orgWeapon)&&Connector.Write8(ADDR_NewWeapon, (byte)(weaponD[effectT[1]].index / 2)) && Connector.Write8(ADDR_NewWeaponF,1) && Connector.Read8((ulong)(0x7E0B83 + weaponD[effectT[1]].index), out byte ammo) && Connector.Write8((ulong)(ADDR_ammoB + weaponD[effectT[1]].index), (byte)((ammo & 0xC0) + 28)),
                            TimeSpan.FromSeconds(5),
                            () => Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x0206 && status == 2,
                            TimeSpan.FromMilliseconds(100),
                            () => Connector.Read8((ulong)(0x7E0B83 + weaponD[effectT[1]].index), out byte ammo) && Connector.Write8((ulong)(ADDR_ammoB + weaponD[effectT[1]].index), (byte)((ammo & 0xC0) + 28)),
                            TimeSpan.FromSeconds(2),
                            true,"lock");
                        d.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} set your weapon to {weaponD[effectT[1]].weapon}."));
                        d.WhenCompleted.Then(t =>
                        {
                            Connector.Write8(ADDR_NewWeapon, orgWeapon);
                            Connector.Write8(ADDR_NewWeaponF, 1);
                            Connector.Write8(ADDR_weaponL, 0);
                        });
                        return;
                    }
                case "rebuild":
                    {
                        var index = robotD[effectT[1]].index;
                        TryEffect(request,
                            () => Connector.Read8((ulong)(ADDR_ammoB + index), out byte ammo) && (ammo & 0xC0) != 0,
                            () => Connector.Read8((ulong)(ADDR_ammoB + index), out byte ammo) && Connector.Write8((ulong)ADDR_ammoB + index, (byte)((ammo & 0x3F))),
                            () => { Connector.SendMessage($"{request.DisplayViewer} rebuilt {robotD[effectT[1]].boss}."); });
                        return;
                    }
                case "warp":
                    {
                        var id = stageD[effectT[1]].id;
                        TryEffect(request,
                            () => Connector.Read8(ADDR_newlevelF, out byte f) && Connector.Read16(ADDR_indexL1, out ushort l) && f == 0,
                            () => (Connector.Write8(ADDR_newlevel, id)) && Connector.Write8(ADDR_newlevelF, 1),
                            () => { Connector.SendMessage($"{request.DisplayViewer} warped MegaMan to {stageD[effectT[1]].stagename}."); });
                        return;
                    }
                case "bosshpfull":
                    {
                        Connector.Read8(ADDR_BossHP_old, out byte hp);
                        if (hp == 0){
                            Respond(request, EffectStatus.FailTemporary, "Not currently in a boss battle");
                            return;
                        }
                        /*In Boss Fight*/
                        TryEffect(request,
                            () => hp <= 10,
                            () => (Connector.Read16(ADDR_BossIndex, out ushort index)) && Connector.Write8((uint)(index + 0x7E002E), 28),
                            () => Connector.SendMessage($"{request.DisplayViewer} refilled the boss's health." /*<= TODO: Maybe add boss spefic name*/)
                            );      
                        return;
                    }
                case "dead":
                    {
                        TryEffect(request,
                            () => Connector.Read8(ADDR_State, out byte state) && Connector.Read8(ADDR_State-1,out byte altstate) && Connector.Read16(ADDR_indexL1,out ushort l) && Connector.IsZero8(ADDR_menu) && (state != 0x1C) && altstate == 2 && l == 0x0206,
                            () => Connector.Write8(ADDR_State, 0x1C) && Connector.Write8(ADDR_State + 1, 0) && Connector.Write8(ADDR_HP,0),
                            () => { Connector.SendMessage($"{request.DisplayViewer} killed MegaMan."); });
                        return;
                    }
                case "ohko":
                    {
                        byte orginHP = 0;
                        var s = RepeatAction(request,
                            TimeSpan.FromSeconds(15),
                            () => Connector.Read8(ADDR_State, out byte state) && Connector.Read8(ADDR_State - 1, out byte altstate) && Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && state != 0x1C && altstate == 2 && l == 0x0206,
                            () => 
                            {
                                if(Connector.SendMessage($"{request.DisplayViewer} disabled your structural shielding.") && (Connector.Read8(ADDR_HP, out orginHP)) && Connector.Write8(ADDR_OHKO,0x80))
                                {
                                    ohko = true;
                                    return true;
                                }
                                return false;
                            },
                            TimeSpan.FromMilliseconds(500),
                            () => Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x0206 && status == 2,
                            TimeSpan.FromSeconds(1),
                            () => Connector.Read8(ADDR_HP, out byte hp) && Connector.Write8(ADDR_HP, 1),
                            TimeSpan.FromMilliseconds(500),
                            true,"ohko");
                        s.WhenCompleted.Then(t =>
                        {
                            if(Connector.Read8(ADDR_DED,out byte d) && d == 0)
                            {
                                if (Connector.Write8(ADDR_OHKO, 0) && Connector.Write8(ADDR_HP, orginHP) && Connector.SendMessage("Your sheilding has been restored"))
                                    ohko = false;
                            }
                            else
                            {
                                if (Connector.Write8(ADDR_OHKO, 0) && Connector.Write8(ADDR_HP, 28) && Connector.SendMessage("Your sheilding has been restored"))
                                    ohko = false;
                            }


                        });
                        return;
                    }
                case "iframes":
                    {
                        var a = RepeatAction(request,
                            TimeSpan.FromSeconds(30),
                            () => Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read16(ADDR_State - 1, out ushort status) && Connector.IsZero8(ADDR_menu) && l == 0x206 && (status&0xFF) == 2 && (status >> 8) != 0x1C,
                            () => Connector.Write8(ADDR_IFRAMES,0xFF),
                            TimeSpan.FromMilliseconds(500), /*<= Start Retry Timer*/
                            () => Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x0206 && status == 2, /*Refresh Condition*/
                            TimeSpan.FromMilliseconds(500), /*Refresh Retry Timer*/
                            () => Connector.Write8(ADDR_IFRAMES,0xFF),
                            TimeSpan.FromSeconds(1),
                            true, "iframes");
                        a.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} deployed an invulnerability field."));
                        a.WhenCompleted.Then(t =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer}'s invulnerability field has dispered.");
                            Connector.Write8(ADDR_IFRAMES, 0);
                            Connector.Write8(0x7E0C0E, 0x80);

                        });
                        return;
                    }
                case "enemyiframes":
                    {
                        var c = RepeatAction(request,
                        TimeSpan.FromSeconds(30),
                        () => Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read16(ADDR_State - 1, out ushort status) && Connector.IsZero8(ADDR_menu) && l == 0x206 && (status&0xFF) == 2 && (status>>8) != 0x1C,
                        () => Connector.Write8(ADDR_immuneF, 1),
                        TimeSpan.FromSeconds(5),
                        () => Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x0206 && status == 2,
                        TimeSpan.FromMilliseconds(100),
                        () => true,
                        TimeSpan.FromMilliseconds(100),
                        true, "enemyiframes");
                        c.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} deployed a mass enemy shield."));
                        c.WhenCompleted.Then(t =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer}'s shield generator has run out.");
                            Connector.Write8(ADDR_immuneF, 0);
                        });
                        return;
                    }
                case "low":
                    {
                        var e = RepeatAction(request,
                            TimeSpan.FromSeconds(15),
                            () => Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read16(ADDR_State - 1, out ushort status) && Connector.IsZero8(ADDR_menu) && l == 0x206 && (status&0xFF) == 2 && (status>>8) != 0x1C && !jumpH,
                            () => {
                                if (Connector.Write16(ADDR_jmpH, 0x800))
                                {
                                    jumpL = true;
                                    return true;
                                }
                                return false;
                            },
                            TimeSpan.FromMilliseconds(500), /*<= Start Retry Timer*/
                            () => Connector.IsZero8(0x7E0BD9), /*Refresh Condition*/
                            TimeSpan.FromMilliseconds(500), /*Refresh Retry Timer*/
                            () => true,
                            TimeSpan.FromSeconds(1),
                            true,"low"
                            );
                        e.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} enabled an Low Gravity field."));
                        e.WhenCompleted.Then(t => Connector.SendMessage($"{request.DisplayViewer}'s Low Gravity field has dispered."));
                        return;
                    }
                case "high":
                    {
                        var f = RepeatAction(request,
                            TimeSpan.FromSeconds(15),
                            () => Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read16(ADDR_State - 1, out ushort status) && Connector.IsZero8(ADDR_menu) && l == 0x206 && (status&0xFF) == 2 && (status>>8) != 0x1C && !jumpL,
                            () => 
                            {
                                if(Connector.Write16(ADDR_jmpH, 0x400))
                                {
                                    jumpH = true;
                                    return true;
                                }
                                return false;
                            },
                            TimeSpan.FromMilliseconds(500), /*<= Start Retry Timer*/
                            () => Connector.IsZero8(0x7E0BD9), /*Refresh Condition*/
                            TimeSpan.FromMilliseconds(500), /*Refresh Retry Timer*/
                            () => true,
                            TimeSpan.FromSeconds(1),
                            true,"high"
                            );
                        f.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} enabled an High Gravity field."));
                        f.WhenCompleted.Then(t => Connector.SendMessage($"{request.DisplayViewer}'s High Gravity field has dispered."));
                        return;
                    }
                case "lives":
                    {
                        byte lives = (byte)request.AllItems[1].Reduce();
                        TryEffect(request,
                            () => Connector.RangeAdd8(ADDR_Lives, lives, 0, 9, false),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} sent you {lives} lives."); }
                            );
                        return;
                    }
                case "lives2":
                    {
                        byte lives = (byte)request.AllItems[1].Reduce();
                        TryEffect(request,
                            () => Connector.RangeAdd8(ADDR_Lives, -lives, 0, 9, false),
                            () => true,
                            () => { Connector.SendMessage($"{request.DisplayViewer} toke away {lives} lives."); }
                            );
                        return;
                    }
                case "slideoff":
                    {
                        var a = RepeatAction(request,
                        TimeSpan.FromSeconds(30),
                        () => Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x206 && status == 2,
                        () => Connector.Write8(ADDR_slide, 1),
                        TimeSpan.FromSeconds(5),
                        () => Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x0206 && status == 2,
                        TimeSpan.FromMilliseconds(100),
                        () => true,
                        TimeSpan.FromMilliseconds(100),
                        true,"slideoff");
                        a.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} disabled your slide module."));
                        a.WhenCompleted.Then(t =>
                        {
                            Connector.SendMessage($"{request.DisplayViewer}'s slide hack has ended.");
                            Connector.Write8(ADDR_slide, 0);
                        });

                        return;
                    }
                case "off":
                    {
                        var a = RepeatAction(request,
                        TimeSpan.FromSeconds(15),
                        () => Connector.Read8(ADDR_Brightness, out byte b) && Connector.Read16(ADDR_indexL1,out ushort l) && Connector.IsZero8(ADDR_menu) && Connector.Read8(ADDR_State-1,out byte status) && b == 0xF && l == 0x206 && status == 2 && !mosaic,
                        () => 
                        {
                            if(Connector.Write8(ADDR_Brightness, 0x0))
                            {
                                screenOff = true;
                                return true;
                            }
                            return false;
                        },
                        TimeSpan.FromSeconds(5),
                        () => Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x0206 && status == 2,
                        TimeSpan.FromMilliseconds(100),
                        () => Connector.Write8(ADDR_Brightness, 0x0),
                        TimeSpan.FromMilliseconds(100),
                        true,"off");
                        a.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} deployed an Screen Off field."));
                        a.WhenCompleted.Then(t => { Connector.SendMessage($"{request.DisplayViewer}'s Screen Off Effect has dispered.");screenOff = false; Connector.Write8(ADDR_Brightness, 0xF); });
                        return;
                    }
                case "mosaic":
                    {
                        var a = RepeatAction(request,
                        TimeSpan.FromSeconds(15),
                        () => Connector.Read8(ADDR_Mosaic,out byte  m) && Connector.IsZero8(0x7E0BD9) && Connector.IsZero8(ADDR_menu) && !screenOff, /*Effect Start Condition*/
                        () => Connector.Write8(ADDR_Mosaic, 0xFF), /*Start Action*/
                        TimeSpan.FromSeconds(1), /*Retry Timer*/
                        () => Connector.IsZero8(ADDR_menu) && Connector.Read16(ADDR_indexL1, out ushort l) && Connector.Read8(ADDR_State - 1, out byte status) && l == 0x0206 && status == 2, /*Refresh Condtion*/
                        TimeSpan.FromMilliseconds(500), /*Refresh Retry Timer*/
                        () => Connector.Write8(ADDR_Mosaic, 0xFF), /*Action*/
                        TimeSpan.FromSeconds(1),
                        true, "mosaic");
                        a.WhenStarted.Then(t => Connector.SendMessage($"{request.DisplayViewer} deployed an Mosaic Effect."));
                        a.WhenCompleted.Then(t => 
                        { 
                            Connector.SendMessage($"{request.DisplayViewer}'s Mosaic Effect has dispered.");
                        });
                        return;
                    }

            }
        }

        protected override bool StopEffect(EffectRequest request)
        {
            bool result = true;
            var effect = request.FinalCode.Split('_');
            switch (effect[0])
            {
                case "mosaic":
                    {
                        if(Connector.Write8(ADDR_Mosaic, 0))
                        {
                            mosaic = false;
                            return true;
                        }
                        return false;
                    }
                case "low":
                    {
                        result = Connector.Write16(ADDR_jmpH,0x5F4);
                        if (result)
                            jumpL = false;
                        return result;
                    }
                case "high":
                    {
                        result = Connector.Write16(ADDR_jmpH,0x5F4);
                        if (result)
                            jumpH = false;
                        return result;
                    }
            }
            return result;
        }

        public override bool StopAllEffects()
        {
            return false;
        }
    }
}
