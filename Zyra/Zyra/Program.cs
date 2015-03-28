using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace Zyra
{
    class Program
    {
         private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;

        private static Menu Menu;

        private static Vector3 Qpos, Epos;

        private static int  Qcount, Ecount;

        private static float Wcount;
     
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Zyra")
                return;

            Q = new Spell(SpellSlot.Q,950);
            W = new Spell(SpellSlot.W,850);
            E = new Spell(SpellSlot.E, 1100);
            //R = new Spell(SpellSlot.R,float.MaxValue,TargetSelector.DamageType.True);
            //Q.SetSkillshot(300, 50, 2000, false, SkillshotType.SkillshotLine);
            Q.SetSkillshot(600, 70, float.MaxValue, false,SkillshotType.SkillshotLine);
            E.SetSkillshot(250, 70, 1150, false, SkillshotType.SkillshotLine);

            Menu = new Menu(Player.ChampionName, Player.ChampionName, true);
            Menu orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu); 
            Menu.AddSubMenu(orbwalkerMenu);
            Menu ts = Menu.AddSubMenu(new Menu("Target Selector", "Target Selector")); ;
            TargetSelector.AddToMenu(ts);

            Menu spellMenu = Menu.AddSubMenu(new Menu("Spells", "Spells"));
            spellMenu.AddItem(new MenuItem("Use Q Harass", "Use Q Harass").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use E Harass", "Use E Harass").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use Q Combo", "Use Q Combo").SetValue(true));
            spellMenu.AddItem(new MenuItem("Use E Combo", "Use E Combo").SetValue(true));
            spellMenu.AddItem(new MenuItem("force focus selected", "force focus selected").SetValue(false));
            spellMenu.AddItem(new MenuItem("if selected in :", "if selected in :").SetValue(new Slider(1000, 1000, 1500)));
            //spellMenu.AddItem(new MenuItem("Use E", "Use E")).SetValue(false);
            //foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy))
            //{
            //    spellMenu.AddItem(new MenuItem("use R" + hero.SkinName, "use R" + hero.SkinName)).SetValue(true);
            //}

            //spellMenu.AddItem(new MenuItem("useR", "Use R to Farm").SetValue(true));
            //spellMenu.AddItem(new MenuItem("LaughButton", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            //spellMenu.AddItem(new MenuItem("ConsumeHealth", "Consume below HP").SetValue(new Slider(40, 1, 100)));

            Menu.AddToMainMenu();

            //Drawing.OnDraw += Drawing_OnDraw;

            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += OnCast;
            
            Game.PrintChat("Welcome to ZyraWorld");
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (Menu.Item("Use Q Combo").GetValue<bool>())
                {
                    useQ();
                }
                if (Menu.Item("Use E Combo").GetValue<bool>())
                {
                    useE();
                }
                //useR();
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (Menu.Item("Use Q Harass").GetValue<bool>())
                {
                    useQ();
                }
                if (Menu.Item("Use E Harass").GetValue<bool>())
                {
                    useE();
                }
                //useR();
            }
            castW();

        }

        public static bool Selected()
        {
            if (!Menu.Item("force focus selected").GetValue<bool>())
            {
                return false;
            }
            else
            {
                var target = TargetSelector.GetSelectedTarget();
                float a = Menu.Item("if selected in :").GetValue<Slider>().Value;
                if (target == null|| target.IsDead || target.IsZombie)
                {
                    return false;
                }
                else
                {
                    if (Player.Distance(target.Position) > a)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }
        public static void useQ()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget())
                {
                    castQ(target);
                }
            }
            else
            {
                var target = TargetSelector.GetTarget(800, TargetSelector.DamageType.Magical);
                if (target != null && target.IsValidTarget())
                {
                    castQ(target);
                }
            }

        }

        public static void useE()
        {
            if (Selected())
            {
                var target = TargetSelector.GetSelectedTarget();
                if (target != null && target.IsValidTarget())
                {
                    castE(target);
                }
            }
            else
            {
                var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                if (target != null && target.IsValidTarget())
                {
                    castE(target);
                }
            }
        }
        public static void castQ(Obj_AI_Base target)
        {
            if (!Q.IsReady())
                return;
            var t = Prediction.GetPrediction(target, 625).CastPosition;
            float x = target.MoveSpeed;
            float y = x * 600 / 1000;
            var pos = target.Position;
            if (target.Distance(t) <= y)
            {
                pos = t;
            }
            if (target.Distance(t) > y)
            {
                pos = target.Position.Extend(t, y -100);
            }
            if (Player.Distance(pos) <= 800 && target.Distance(pos) >= 100)
            {
                Q.Cast(pos);
            }
            if (Player.Distance(target.Position) <= Player.BoundingRadius + Player.AttackRange + target.BoundingRadius)
            {
                Q.Cast(pos);
            }
            if (Player.Distance(pos) > 800)
            {
                if (target.Distance(t) > y)
                {
                    var pos2 = target.Position.Extend(t, y);
                    if (Player.Distance(pos2) <= 800)
                    {
                        Q.Cast(pos2);
                    }


                    else
                    {
                        var prediction = Q.GetPrediction(target);
                        if (prediction.Hitchance >= HitChance.High)
                        {
                            var pos3 = prediction.CastPosition;
                            var pos4 = Player.Position.Extend(pos3, 800);
                            Q.Cast(pos4);
                        }
                    }
                }
            }
        }
        public static void castE(Obj_AI_Base target)
        {
            if (E.IsReady())
            {
                E.Cast(target);
            }
        }
        public static void castW()
        {
            if (!W.IsReady())
            {
                if(Environment.TickCount - Wcount >= 400)
                {
                    Ecount = 0;
                    Qcount = 0;
                }

            }
            if (W.IsReady())
            {
                if (Ecount == 1)
                {
                    W.Cast(Epos);
                }
                if (Qcount == 1)
                {
                    W.Cast(Qpos);
                }
            }
        }

        public static void OnCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var spell = args.SData;
            if (!sender.IsMe)
            {
                return;
            }
            if (spell.Name == "ZyraQFissure" && W.IsReady())
            {
                Qcount = 1;
                Qpos = args.End;
            }
            if (spell.Name == "ZyraGraspingRoots" && W.IsReady())
            {
                Ecount = 1;
                var pos = args.End;
                Obj_AI_Base target;
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                {
                    if (Selected())
                    {
                        target = TargetSelector.GetSelectedTarget();
                    }
                    else
                    {
                        target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                    }
                }
                else
                {
                    target = null; 
                }
                if (target != null)
                {
                    if(Player.Distance(target.Position) >850)
                    {
                        Epos = Player.Position.Extend(pos, 850);
                    }
                    if(Player.Distance(target.Position) <= 850)
                    {
                        Epos = Player.Position.Extend(pos, Player.Distance(target.Position));
                    }
                }
                if (target == null)
                {
                    if (Player.Distance(Game.CursorPos) >= 850)
                        Epos = Player.Position.Extend(Game.CursorPos, 850);
                    else
                        Epos = Game.CursorPos;
                }

            }
            if (spell.Name == "ZyraSeed")
            {
                Wcount = Environment.TickCount;
            }
        }

    }
}
