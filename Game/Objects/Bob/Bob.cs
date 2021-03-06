﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

using CoreEngine;

using CloudberryKingdom.Levels;
using CloudberryKingdom.Blocks;

namespace CloudberryKingdom.Bobs
{
    public class Bob : ObjectBase
    {
        public float LightSourceFade = 1, LightSourceFadeVel = 0;
        public void ResetLightSourceFade()
        {
            LightSourceFade = 1;
            LightSourceFadeVel = 0;
        }
        public void SetLightSourceToFade()
        {
            LightSourceFadeVel = -.022f;
        }
        public void SetLightSourceToFadeIn()
        {
            LightSourceFadeVel = .022f;
            LightSourceFade = 0;
        }
        void DoLightSourceFade()
        {
            LightSourceFade += LightSourceFadeVel;
            CoreMath.Restrict(0, 1, ref LightSourceFade);
        }

        public bool Dopple = false;
        public Vector2 LastPlacedCoin;

        public static bool AllExplode = true;
        public static bool ShowCorpseAfterExplode = false;

        public BobPhsx MyHeroType;

        public bool FadingIn;
        public float Fade;

        public void SetToFadeIn()
        {
            FadingIn = true;
            Fade = 0;
        }

        public static int ImmortalLength = 55;
        public int ImmortalCountDown;
        public bool Moved;

        public ColorScheme MyColorScheme;

        public int HeldObjectIteration;

        public bool CanHaveCape, CanHaveHat = true;
        public BobPhsx MyObjectType;

        public float NewY, NewVel, Xvel;

        public override void Release()
        {
            base.Release();

            ControlFunc = null;
            OnLand = null;
            OnApexReached = null;
            OnAnimFinish = null;

            MyPiece = null;
            if (MyRecord != null) MyRecord.Release(); MyRecord = null;

            if (MyBobLinks != null)
                foreach (BobLink link in MyBobLinks)
                    link.Release();
            MyBobLinks = null;

            if (MyCape != null) MyCape.Release(); MyCape = null;

            if (MyPhsx != null) MyPhsx.Release(); MyPhsx = null;

            if (PlayerObject != null) PlayerObject.Release(); PlayerObject = null;

            if (temp != null) temp.Release(); temp = null;
        }

        public void SetObject(ObjectClass obj, bool boxesOnly)
        {
            if (PlayerObject != null) PlayerObject.Release();

            PlayerObject = new ObjectClass(obj, BoxesOnly, false);
            Vector2 size = PlayerObject.BoxList[0].Size();
            float ratio = size.Y / size.X;
            int width = Tools.TheGame.Resolution.Bob.X;

            //PlayerObject.FinishLoading();
            PlayerObject.FinishLoading(Tools.QDrawer, Tools.Device, Tools.TextureWad, Tools.EffectWad, Tools.Device.PresentationParameters, width, (int)(width * ratio), false);

            Head = null;
        }


        public uint StoredRecord_BL, StoredRecord_QuadSize;
        public int StoredRecordTexture = 0;

        Quad MainQuad;
        public void SetRecordingInfo()
        {
            if (MainQuad == null)
            {
                if (PlayerObject != null && PlayerObject.QuadList != null)
                {
                    if (MyPhsx is BobPhsxSpaceship)
                        MainQuad = PlayerObject.QuadList[1] as Quad;
                    else if (MyPhsx is BobPhsxMeat)
                        MainQuad = PlayerObject.QuadList[0] as Quad;
                    else
                        MainQuad = PlayerObject.FindQuad("MainQuad") as Quad;
                }
                else
                    MainQuad = null;
            }

            if (MainQuad == null)
            {
                StoredRecord_BL = 0;
                StoredRecord_QuadSize = 0;
                StoredRecordTexture = 0;
            }
            else
            {
                Vector2 _BL = MainQuad.BL();
                Vector2 _Size = MainQuad.TR() - _BL;

                if (PlayerObject.xFlip)
                {
                    _BL.X += _Size.X;
                    _Size.X *= -1;
                }

                StoredRecord_BL = PackVectorIntoInt_Pos(_BL);
                StoredRecord_QuadSize = PackVectorIntoInt_SizeAngle(_Size, PlayerObject.ContainedQuadAngle);

                //Vector2 BL = MainQuad.Corner[2].Pos;
                //Vector2 TR = MainQuad.Corner[1].Pos;
                //StoredRecord_BL = PackVectorIntoInt_Pos(BL);
                //StoredRecord_QuadSize = PackVectorIntoInt_Size(TR - BL);


                if (Game != null)
                {
                    if ((Dead || Dying) && !Game.MyGameFlags.IsTethered)
                    {
                        StoredRecordTexture = 0;
                    }
                    else
                    {
                        StoredRecordTexture = CoreMath.Restrict(0, Tools.TextureWad.TextureList.Count - 1,
                                                               Tools.TextureWad.TextureList.IndexOf(MainQuad.MyTexture));						
                    }
                }
            }
        }

        public static uint PackVectorIntoInt_Pos(Vector2 v)
        {
            v.X += 600;
            v.Y += 1000;

            uint x = (uint)(v.X * 4.0f) << 14;
            uint y = (uint)(v.Y * 4.0f);
            uint i = x + y;

            //Vector2 _v = UnpackIntIntoVector_Pos(i);

            return i;
        }

        public static Vector2 UnpackIntIntoVector_Pos(uint i)
        {
            uint _x = i >> 14;
            uint _y = i - (_x << 14);

            float x = (float)(_x) / 4.0f;
            float y = (float)(_y) / 4.0f;

            x -= 600;
            y -= 1000;
            
            return new Vector2(x, y);
        }

        public static uint PackVectorIntoInt_SizeAngle(Vector2 v, float angle)
        {
            float tau = (float)(2 * Math.PI);
            float revs = angle / tau;
            angle -= (int)revs * tau;
            if (angle < 0)
                angle += tau;

            uint x = (uint)(Math.Abs(v.X) * 0.7f) << 20;
            x += v.X > (uint)0 ? (uint)0 : (((uint)1) << 31);
            uint y = ((uint)(v.Y * 1.0f) << 20) >> 12;
            uint a = ((uint)(angle * 32.0f) << 24) >> 24;
            uint i = x + y + a;

            //Vector2 _v = UnpackIntIntoVector_Size(i);
            //float _a  = UnpackIntIntoVector_Angle(i);

            return i;
        }

        public static Vector2 UnpackIntIntoVector_Size(uint i)
        {
            bool sign = (i & (((uint)1) << 31)) == (((uint)1) << 31);
            if (sign)
                i -= (((uint)1) << 31);

            uint _x = i >> 20;
            uint _y = (i - (_x << 20)) >> 8;
            
            float x = (float)(_x) / 0.7f;
            float y = (float)(_y) / 1.0f;

            if (sign) x = -x;

            return new Vector2(x, y);
        }

        public static float UnpackIntIntoVector_Angle(uint i)
        {
            uint _x = i >> 20;
            uint _y = (i - (_x << 20)) >> 8;
            uint _a = (i - (_x << 20) - (_y << 8));

            float a = (float)(_a) / 32.0f;

            return a;
        }

        public void SetColorScheme(ColorScheme scheme)
        {
            //scheme = ColorSchemeManager.ColorSchemes[2];
            Tools.Write(scheme.ToString());

            if (BoxesOnly || PlayerObject.QuadList == null) return;

            if (scheme.HatData == null) scheme.HatData = Hat.None;
            if (scheme.BeardData == null) scheme.BeardData = Hat.None;

            if (CanHaveHat)
            {
                var head = PlayerObject.FindQuad("Head");
                //if (null != head) head.Show = scheme.HatData.DrawHead;
                if (null != head) head.Show = false;

                foreach (BaseQuad quad in PlayerObject.QuadList)
                {
                    if (quad.Name.Contains("Hat_"))
                    {
                        Quad _Quad = quad as Quad;
                        if (string.Compare(quad.Name, scheme.HatData.QuadName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            quad.Show = scheme.HatData.DrawSelf;

                            if (null != _Quad) _Quad.ShowChildren();
                        }
                        else
                        {
                            quad.Show = false;
                            if (null != _Quad) _Quad.HideChildren();
                        }
                    }

                    if (quad.Name.Contains("Facial_"))
                    {
                        Quad _Quad = quad as Quad;
                        if (scheme.SkinColor.Clr.A != 0 && 
                            !(scheme.HatData != null && !scheme.HatData.DrawHead) &&
                            string.Compare(quad.Name, scheme.BeardData.QuadName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            quad.Show = scheme.BeardData.DrawSelf;

                            if (null != _Quad) _Quad.ShowChildren();
                        }
                        else
                        {
                            quad.Show = false;
                            if (null != _Quad) _Quad.HideChildren();
                        }
                    }
                }
            }
 

            var q = PlayerObject.FindQuad("MainQuad");
            if (q != null)
            {
                q.MyMatrix = scheme.SkinColor.M;

                if (scheme.SkinColor.Clr.A == 0)
                {
                    q.MyEffect = Tools.BasicEffect;
                    q.SetColor(Color.Transparent);
                }
                else
                {
                    q.MyEffect = Tools.HslGreenEffect;
                    q.SetColor(Color.White);
                }

                MyPhsx.ModColorScheme(q);

                var wf = PlayerObject.FindQuad("Wings_Front"); if (wf != null) wf.MyMatrix = scheme.SkinColor.M;
                var wb = PlayerObject.FindQuad("Wings_Back"); if (wb != null) wb.MyMatrix = scheme.SkinColor.M;
            }

            if (MyCape != null)
            {
                if (scheme.CapeColor.Clr.A == 0 || scheme.CapeOutlineColor.Clr.A == 0)
                    MyCape.MyColor = MyCape.MyOutlineColor = Color.Transparent;
                else
                {
                    MyCape.MyColor = scheme.CapeColor.Clr;
                    MyCape.MyOutlineColor = scheme.CapeOutlineColor.Clr;
                }

                MyCape.MyQuad.Quad.MyTexture = scheme.CapeColor.Texture;
                MyCape.MyQuad.Quad.MyEffect = scheme.CapeColor.Effect;

                if (scheme.CapeColor.ModObject != null)
                    scheme.CapeColor.ModObject(this);

                if (scheme.CapeColor.Clr.A == 0 || scheme.CapeOutlineColor.Clr.A == 0)
                    ShowCape = false;
            }

            MyColorScheme = scheme;
        }

        public struct BobMove
        {
            public float MaxTargetY, MinTargetY;

            public int Copy;

            /// <summary>
            /// If true the x acceleration is inverted
            /// </summary>
            public bool InvertDirX;

            public void Init()
            {
                MaxTargetY = 600;
                MinTargetY = -500;

                Copy = -1;
            }
        }

        public Vector2 CapeWind, Wind;

        public Hat MyHat;
        public Cape.CapeType MyCapeType;
        public Cape MyCape;
        public bool ShowCape;
        public Color InsideColor;
        
        ObjectVector temp = null;
        Quad Head = null;

        public List<BobLink> MyBobLinks;

        public int SideHitCount;

        public bool CanInteract = true;

        public BobMove MoveData;

        public int Count_ButtonA;
        public BobInput CurInput, PrevInput;
        public bool InputFromKeyboard = false;
        public BobPhsx MyPhsx;

        /// <summary>
        /// Whether the computer wants to land on a potential block (For PlayMode == 2)
        /// </summary>
        public bool WantsToLand;

        /// <summary>
        /// Whether the computer would be willing to land but prefers not to.
        /// </summary>
        public bool WantsToLand_Reluctant;

        public Vector2 TargetPosition = Vector2.Zero;

        public float GroundSpeed;

        public bool ComputerWaitAtStart;
        public int ComputerWaitAtStartLength = 0;

        public bool SaveNoBlock;
        public int PlaceDelay = 23;
        public int PlaceTimer;

        public bool Flying = false;
        public bool Immortal, DoNotTrackOffScreen = false;

        public bool TopCol, BottomCol;

        public FancyVector2 FancyPos;
        public bool CompControl, CharacterSelect, CharacterSelect2, Cinematic, DrawWithLevel = true, AffectsCamera = true;
        public int IndexOffset;
        
        public int ControlCount = 0;

        /// <summary>
        /// A callback called when the Bob lands on something.
        /// </summary>
        public Action OnLand;

        /// <summary>
        /// A callback called when the Bob reaches his jump apex.
        /// </summary>
        public Action OnApexReached;
        

        public bool CodeControl = false;
        public Action<int> ControlFunc;
        public Action<int> CinematicFunc;
        public Action OnAnimFinish;

        public void SetCodeControl()
        {
            CodeControl = true;
            ControlCount = 0;
            ControlFunc = null;
        }

        public LevelPiece MyPiece;

        /// <summary>
        /// Which start position in the current level piece this Bob belongs to
        /// </summary>
        public int MyPieceIndex;

        /// <summary>
        /// If more than one Bob belongs to the same start position, this is the Bobs' ordering
        /// </summary>
        public int MyPieceIndexOffset;
        public ComputerRecording MyRecord;


        public bool Dying, Dead, FlamingCorpse;

        public int DeadCount;

        public bool BoxesOnly;

        public bool ScreenWrap, ScreenWrapToCenter, CollideWithCamera = true;

        public CoreSound JumpSound, DieSound;
        public static CoreSound JumpSound_Default, DieSound_Default;

        public PlayerIndex MyPlayerIndex;
        public PlayerData MyPlayerData
        {
            get 
            {
                return PlayerManager.Get(MyPlayerIndex);
            }
        }


        public bool TryPastTop;

        
        public ObjectClass PlayerObject;

        public BlockBase LastCeiling = null;
        Vector2 LastCoinPos;

        public int MinFall, MinDrop;

        public bool MakingLava, MakingCeiling;

        public AABox Box, Box2;

        public Vector2 Feet()
        {
            Box.CalcBounds();

            return new Vector2(Box.Current.Center.X, Box.BL.Y);
        }

        /// <summary>
        /// A list of boxes to allow for different difficulty levels for different obstacles.
        /// </summary>
        List<AABox> Boxes;
        int NumBoxes = 10;

        public AABox GetBox(int DifficultyLevel)
        {
            int index = CoreMath.Restrict(0, Boxes.Count - 1, DifficultyLevel);
            return Boxes[index];
        }

        /// <summary>
        /// A collision box corresponding to the normal size of Box2 during actual gameplay.
        /// </summary>
        public AABox RegularBox2;

        public static List<BobPhsx> HeroTypes = GetPlayableHeroTypes();

        static List<BobPhsx> GetPlayableHeroTypes()
        {
            if (CloudberryKingdomGame.CodersEdition)
            {
                return new List<BobPhsx>(new BobPhsx[] {
                    BobPhsxNormal.Instance, BobPhsxJetman.Instance, BobPhsxDouble.Instance, BobPhsxSmall.Instance, BobPhsxWheel.Instance, BobPhsxSpaceship.Instance, BobPhsxBox.Instance,
                    BobPhsxBouncy.Instance, BobPhsxRocketbox.Instance, BobPhsxBig.Instance, BobPhsxScale.Instance, BobPhsxInvert.Instance,
                    BobPhsxBlobby.Instance, BobPhsxMeat.Instance, BobPhsxTimeship.Instance
                });
            }
            else
            {
                return new List<BobPhsx>(new BobPhsx[] {
                    BobPhsxNormal.Instance, BobPhsxJetman.Instance, BobPhsxDouble.Instance, BobPhsxSmall.Instance, BobPhsxWheel.Instance, BobPhsxSpaceship.Instance, BobPhsxBox.Instance,
                    BobPhsxBouncy.Instance, BobPhsxRocketbox.Instance, BobPhsxBig.Instance, BobPhsxScale.Instance, BobPhsxInvert.Instance,
                });
            }
        }

        /// <summary>
        /// How many time the bob has popped something without hitting the ground.
        /// </summary>
        public int PopModifier = 1;

        public Bob(BobPhsx type, bool boxesOnly)
        {
            MyHeroType = type;
            Bob bob = type.Prototype;

            CanHaveCape = bob.CanHaveCape;
            CanHaveHat = bob.CanHaveHat;
            MyObjectType = bob.MyObjectType;

            Core.DrawLayer = 6;
            Core.Show = true;

            BoxesOnly = boxesOnly;

            SetObject(bob.PlayerObject, BoxesOnly);

            Core.Data.Position = bob.Core.Data.Position;
            Core.Data.Velocity = bob.Core.Data.Velocity;
            PlayerObject.ParentQuad.Update();
            PlayerObject.Update(null);
            PlayerObject.PlayUpdate(0);

            Box = new AABox(Core.Data.Position, PlayerObject.BoxList[1].Size() / 2);
            Box2 = new AABox(Core.Data.Position, PlayerObject.BoxList[2].Size() / 2);

            SetHeroPhsx(MyHeroType);

            SetColorScheme(bob.MyColorScheme);
        }

        public bool IsSpriteBased = true;
        public Bob(string file, CoreEffectWad EffectWad, CoreTextureWad TextureWad)
        {
            LoadFromFile(file, EffectWad, TextureWad, BobPhsxNormal.Instance);
        }
        public Bob(string file, CoreEffectWad EffectWad, CoreTextureWad TextureWad, BobPhsx MyHeroType, bool AllowHats)
        {
            CanHaveHat = AllowHats;
            LoadFromFile(file, EffectWad, TextureWad, MyHeroType);
        }
        public Bob(ObjectClass obj, CoreEffectWad EffectWad, CoreTextureWad TextureWad, BobPhsx MyHeroType, bool AllowHats)
        {
            CanHaveHat = AllowHats;
            _Load(obj, EffectWad, TextureWad, MyHeroType);
        }
        void LoadFromFile(string file, CoreEffectWad EffectWad, CoreTextureWad TextureWad, BobPhsx HeroType)
        {
            Tools.UseInvariantCulture();
            FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
            BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);

            Vector2 size = new Vector2(1, 2);
            float ratio = size.Y / size.X;
            int width = Tools.TheGame.Resolution.Bob.X;
            int height = (int)(width * ratio);

            var obj = new ObjectClass(Tools.QDrawer, Tools.Device, Tools.Device.PresentationParameters, width, height, EffectWad.FindByName("BasicEffect"), TextureWad.FindByName("White"));
            obj.ReadFile(reader, EffectWad, TextureWad);
            reader.Close();
            stream.Close();

            obj.ParentQuad.Scale(new Vector2(260, 260));

            _Load(obj, EffectWad, TextureWad, MyHeroType);
        }
        void _Load(ObjectClass obj, CoreEffectWad EffectWad, CoreTextureWad TextureWad, BobPhsx HeroType)
        {
            this.PlayerObject = obj;

            this.MyHeroType = HeroType;

            CoreData = new ObjectData();
            Core.Show = true;

            JumpSound = JumpSound_Default = Tools.SoundWad.FindByName("Jump5");
            JumpSound.DefaultVolume = .1f;
            JumpSound.DelayTillNextSoundCanPlay = 10;

            DieSound = DieSound_Default = Tools.Sound("Death_Chime");

            PlayerObject.Read(0, 0);
            PlayerObject.Play = true;

            Core.Data.Position = new Vector2(100, 50);
            Core.Data.Velocity = new Vector2(0, 0);
            PlayerObject.ParentQuad.Update();
            PlayerObject.Update(null);
            PlayerObject.PlayUpdate(0);
            
            Box = new AABox(Core.Data.Position, PlayerObject.BoxList[1].Size() / 2);
            Box2 = new AABox(Core.Data.Position, PlayerObject.BoxList[2].Size() / 2);

            MyPhsx = new BobPhsx();
            MyPhsx.Init(this);

            SetColorScheme(ColorSchemeManager.ColorSchemes[0]);
        }

        public void SwitchHero(BobPhsx hero)
        {
            Vector2 HoldVel = MyPhsx.Vel;

            if (MyCape != null) MyCape.Release(); MyCape = null;

            SetObject(hero.Prototype.PlayerObject, false);
            SetHeroPhsx(hero);

            if (MyCape != null) MyCape.Move(Pos);

            //MakeCape();

            SetColorScheme(PlayerManager.Get(this).ColorScheme);

            MyPhsx.Vel = HoldVel;
            
            //PhsxStep();
            AnimAndUpdate();
            //PhsxStep2();
        }

        /// <summary>
        /// When true the player can not move.
        /// </summary>
        public bool Immobile = false;

        public void SetHeroPhsx(BobPhsx type)
        {
            MyCapeType = Cape.CapeType.Normal;

            MyPhsx = type.Clone();

            MyPhsx.Init(this);
            MakeCape(MyCapeType);
        }

        public void MakeCape(Cape.CapeType CapeType)
        {
            if (MyCape == null && !BoxesOnly && CanHaveCape)
            {
                MyCape = new Cape(this, CapeType, MyPhsx);
                MyCape.Reset();
            }
        }

        public void Init(bool BoxesOnly, PhsxData StartData, GameData game)
        {
            Core.Show = true;

            HeldObjectIteration = 0;

            BobPhsx type = game.DefaultHeroType;
            if (Core.MyLevel != null) type = Core.MyLevel.DefaultHeroType;

            if (Dopple)
            {
                if (game.DefaultHeroType2 != null) type = game.DefaultHeroType2;
                if (Core.MyLevel != null && Core.MyLevel.DefaultHeroType2 != null) type = Core.MyLevel.DefaultHeroType2;
            }

            MyHeroType = type;

            Core.DrawLayer = 6;

            if (CharacterSelect2)
            {
                MyPhsx = new BobPhsxCharSelect();
                MyPhsx.Init(this);
                MakeCape(Cape.CapeType.Normal);
            }
            else
                SetHeroPhsx(type);

            ImmortalCountDown = ImmortalLength;
            Moved = false;

            PlaceTimer = 0;

            GroundSpeed = 0;

            Dead = Dying = false;
            DeadCount = 0;
                        
            Move(StartData.Position - Core.Data.Position);
            Core.StartData = Core.Data = StartData;


            if (PlayerObject == null)
            {
                PlayerObject = new ObjectClass(type.Prototype.PlayerObject, BoxesOnly, false);

                PlayerObject.FinishLoading();
                Vector2 size = PlayerObject.BoxList[0].Size();
                float ratio = size.Y / size.X;
                int width = Tools.TheGame.Resolution.Bob.X;
                int height = (int)(width * ratio);
                PlayerObject.FinishLoading(Tools.QDrawer, Tools.Device, Tools.TextureWad, Tools.EffectWad, Tools.Device.PresentationParameters, width, height);
            }

            PlayerObject.Read(0, 0);
            PlayerObject.Play = true;

            PlayerObject.ParentQuad.Update();
            PlayerObject.Update(null);
            PlayerObject.PlayUpdate(0);

            Move(StartData.Position - Core.Data.Position);
            Core.Data = StartData;            
            Box.SetTarget(Core.Data.Position, Box.Current.Size);
            Box2.SetTarget(Core.Data.Position + MyPhsx.TranscendentOffset, Box2.Current.Size);
            Box.SwapToCurrent();
            Box2.SwapToCurrent();
            UpdateObject();

            Box.CalcBounds();
            Box2.CalcBounds();

            LastCoinPos = Core.Data.Position;


            if (MyCape != null)
            {
                MyCape.AnchorPoint[0] = Core.Data.Position;
                MyCape.Reset();
            }

            SetColorScheme(MyColorScheme);
        }

        /// <summary>
        /// Whether this Bob is a player.
        /// </summary>
        public bool IsPlayer = true;

        /// <summary>
        /// Get the player data associated with this Bob.
        /// If the Bob isn't controlled by a player return null.
        /// </summary>
        /// <returns></returns>
        public PlayerData GetPlayerData()
        {
            if (!IsPlayer) return null;

            return PlayerManager.Get((int)MyPlayerIndex);
        }

        public bool GiveStats()
        {
            return Core.MyLevel.PlayMode == 0 && !CompControl && !Core.MyLevel.Watching && !Dead && !Dying;
        }

        public PlayerStats MyStats
        {
            get { return PlayerManager.Get((int)MyPlayerIndex).Stats; }
        }

        public PlayerStats MyTempStats
        {
            get { return PlayerManager.Get((int)MyPlayerIndex).TempStats; }
        }

        /// <summary>
        /// The number of frames since the player has died.
        /// </summary>
        int DeathCount = 0;

        public ObjectBase KillingObject = null;
        public enum BobDeathType                { None,   Fireball,   Firesnake,    FireSpinner,    Boulder,      SpikeyGuy,   Spike,   Fall,      Lava,   Blob,   Laser,   LavaFlow,    FallingSpike,
            Serpent, unnamed2, unnamed3, unnamed4, unnamed5, unnamed6, unnamed7, unnamed8, unnamed9, unnamed10, unnamed11, unnamed12, unnamed13, unnamed14, unnamed15, unnamed16, unnamed17, unnamed18, unnamed19, unnamed20, unnamed21, unnamed22, unnamed23, unnamed24, unnamed25, unnamed26, unnamed27, unnamed28, unnamed29, unnamed30, 
            Time,         LeftBehind,    Other,   Total };

        public static Dictionary<BobDeathType, Localization.Words> BobDeathNames = new Dictionary<BobDeathType, Localization.Words>
        {
            { BobDeathType.None, Localization.Words.None },
            { BobDeathType.Fireball, Localization.Words.Fireball },
            { BobDeathType.FireSpinner, Localization.Words.Firespinner },
            { BobDeathType.Boulder, Localization.Words.Boulder },
            { BobDeathType.SpikeyGuy, Localization.Words.SpikeyGuy },
            { BobDeathType.Spike, Localization.Words.Spike },
            { BobDeathType.Fall, Localization.Words.Falling },
            { BobDeathType.Lava, Localization.Words.Lava },
            { BobDeathType.Blob, Localization.Words.FlyingBlobs },
            { BobDeathType.Laser, Localization.Words.Laser },
            { BobDeathType.LavaFlow, Localization.Words.Sludge },
            { BobDeathType.FallingSpike, Localization.Words.FallingSpikey },
            { BobDeathType.Serpent, Localization.Words.Serpent },
            
            { BobDeathType.Time, Localization.Words.TimeLimit },
            { BobDeathType.LeftBehind, Localization.Words.LeftBehind },
            { BobDeathType.Other, Localization.Words.Other },
            { BobDeathType.Total, Localization.Words.Total },
        };
        
        /// <summary>
        /// Kill the player.
        /// </summary>
        /// <param name="DeathType">The type of death.</param>
        /// <param name="ForceDeath">Whether to force the players death, ignoring immortality.</param>
        /// <param name="DoAnim">Whether to do the death animation.</param>
        public void Die(BobDeathType DeathType, bool ForceDeath, bool DoAnim)
        {
            Die(DeathType, null, ForceDeath, DoAnim);
        }
        public void Die(BobDeathType DeathType)
        {
            Die(DeathType, null, false, true);
        }
        public void Die(BobDeathType DeathType, ObjectBase KillingObject)
        {
            Die(DeathType, KillingObject, false, true);
        }
        public void Die(BobDeathType DeathType, ObjectBase KillingObject, bool ForceDeath, bool DoAnim)
        {
            if (Dying) return;

            if (!ForceDeath)
            {
                if (Immortal ||
                    (!Core.MyLevel.Watching && Core.MyLevel.PlayMode == 0 && ImmortalCountDown > 0)) return;

                if (CompControl) return;
            }

            DeathCount = 0;

            //Core.DrawLayer = 9;

            FlamingCorpse = false;

            this.KillingObject = KillingObject;

#if XBOX
            if (!ForceDeath && !DoAnim)
            {
                // No vibration
            }
            else
            {
                Tools.SetVibration(MyPlayerIndex, .5f, .5f, 45);
            }
#endif

            // Update stats
            if (DeathType != BobDeathType.None)
                MyStats.DeathsBy[(int)BobDeathType.Total]++;

            MyStats.DeathsBy[(int)DeathType]++;

            Dying = true;

            if (DoAnim)
                MyPhsx.Die(DeathType);
            
            Tools.CurGameData.BobDie(Core.MyLevel, this);
        }

        /// <summary>
        /// Whether we can kill the current player.
        /// The player must be player controlled and not already dead.
        /// </summary>
        public bool CanDie
        {
            get
            {
                return !Immortal && !Dead && !Dying && Core.MyLevel.PlayMode == 0 && !Core.MyLevel.Watching;
            }
        }

        /// <summary>
        /// Whether we can finish a current level.
        /// The player must be player controlled and not already dead.
        /// </summary>
        public bool CanFinish
        {
            get
            {
                return !Dead && !Dying && Core.MyLevel.PlayMode == 0 && !Core.MyLevel.Watching;
            }
        }

        public void DyingPhsxStep()
        {
            DeathCount++;

            if (Core.Data.Velocity.Y > -30)
                Core.Data.Velocity += Core.Data.Acceleration;
            
            Core.Data.Position += Core.Data.Velocity;

            PlayerObject.PlayUpdate(1000f / 60f / 150f);

            // Check to see if any other players are alive
            /*
            if (PlayerManager.AllDead())
            {
                // Check to see if we should give a hint about quickspawning
                if (Hints.CurrentGiver != null)
                {
                    Hints.CurrentGiver.Check_QuickSpawn();
                }
            }*/

            // Check to see if we've fallen past the edge of the screen,
            // if so, officially declare the player dead.
            if (!Dead && (
                (IsVisible() && Core.Show && Core.Data.Position.Y < Core.MyLevel.MainCamera.BL.Y - Game.DoneDyingDistance)
                ||
                (!IsVisible() && DeathCount > Game.DoneDyingCount)))
            {
                Tools.CurGameData.BobDoneDying(Core.MyLevel, this);
                if (!Dead && !Dying) DeadCount = 0;
                Dead = true;
            }

            Box.Current.Size = PlayerObject.BoxList[1].Size() / 2;
            Box.SetTarget(Core.Data.Position, Box.Current.Size);
        }

        public void CheckForScreenWrap()
        {
            if (ScreenWrap)
            {
                if (ScreenWrapToCenter)
                {
                    bool OffScreen = false;
                    if (Core.Data.Position.X < Core.MyLevel.MainCamera.BL.X - 100)
                        OffScreen = true;
                    if (Core.Data.Position.X > Core.MyLevel.MainCamera.TR.X + 100)
                        OffScreen = true;
                    if (Core.Data.Position.Y < Core.MyLevel.MainCamera.BL.Y - 600)
                        OffScreen = true;
                    if (Core.Data.Position.Y > Core.MyLevel.MainCamera.TR.Y + 600)
                        OffScreen = true;

                    if (OffScreen)
                    {
                        // Find highest bob
                        Vector2 Destination = Core.MyLevel.MainCamera.Data.Position;
                        if (Core.MyLevel.Bobs.Count > 1)
                        {
                            Bob HighestBob = null;
                            foreach (Bob bob in Core.MyLevel.Bobs)
                            {
                                if (bob != this && bob.AffectsCamera && (HighestBob == null || bob.Core.Data.Position.Y > HighestBob.Core.Data.Position.Y))
                                {
                                    HighestBob = bob;
                                }
                            }
                            Destination = HighestBob.Core.Data.Position;
                        }
                        Move(Destination - Core.Data.Position);
                        ParticleEffects.AddPop(Core.MyLevel, Core.Data.Position);
                    }
                }
                else
                {
                    // Do the screen wrap
                    //bool Moved = false;
                    Vector2 w = Core.MyLevel.MainCamera.TR - Core.MyLevel.MainCamera.BL + new Vector2(1200, 1600);
                    if (Core.Data.Position.X < Core.MyLevel.MainCamera.BL.X - 100)
                    {
                        //Moved = true;
                        Move(new Vector2(w.X, 0));
                    }
                    if (Core.Data.Position.X > Core.MyLevel.MainCamera.TR.X + 100)
                    {
                        //Moved = true;
                        Move(new Vector2(-w.X, 0));
                    }
                    if (Core.Data.Position.Y < Core.MyLevel.MainCamera.BL.Y - 600)
                    {
                        //Moved = true;
                        Move(new Vector2(0, w.Y));
                    }
                    if (Core.Data.Position.Y > Core.MyLevel.MainCamera.TR.Y + 600)
                    {
                        //Moved = true;
                        Move(new Vector2(0, -w.Y));
                    }

                    // If multiplayer, decrease the bob's camera weight
                    //if (Moved && PlayerManager.GetNumPlayers() > 1)
                    //{
                    //    CameraWeight = 0;
                    //    CameraWeightSpeed = .01f;
                    //}
                }
            }
        }
        public float CameraWeight = 1, CameraWeightSpeed;

        public bool Prevent_A_Button = false;
        public void GetPlayerInput()
        {
            CurInput.Clean();

            if (Immobile) return;

#if WINDOWS
            bool GamepadUsed = false;
#endif

            if (CoreGamepad.IsConnected(MyPlayerIndex))
            {
                if (CoreGamepad.IsPressed(MyPlayerIndex, ControllerButtons.A))
                {
                    CurInput.A_Button = true;
                }
                else
                {
                    CurInput.A_Button = false;
                }

                CurInput.xVec.X = CurInput.xVec.Y = 0;
                
                Vector2 LDir = CoreGamepad.LeftJoystick(MyPlayerIndex);
                if (Math.Abs(LDir.X) > .15f)
                    CurInput.xVec.X = LDir.X;
                if (Math.Abs(LDir.Y) > .15f)
                    CurInput.xVec.Y = LDir.Y;

                Vector2 DPad = CoreGamepad.DPad(MyPlayerIndex);
                if (Math.Abs(DPad.X) > .15f)
                    CurInput.xVec.X = DPad.X;
                if (Math.Abs(DPad.Y) > .15f)
                    CurInput.xVec.Y = DPad.Y;

                CurInput.B_Button = (CoreGamepad.IsPressed(MyPlayerIndex, ControllerButtons.LS) ||
                                     CoreGamepad.IsPressed(MyPlayerIndex, ControllerButtons.RS));

#if WINDOWS
                if (CurInput.xVec != Vector2.Zero || CurInput.A_Button || CurInput.B_Button)
                {
                    GamepadUsed = true;
                    InputFromKeyboard = false;
                }
#endif

                if (Prevent_A_Button)
                {
                    if (CurInput.A_Button)
                        CurInput.A_Button = false;
                    else
                        Prevent_A_Button = false;
                }
            }

#if WINDOWS
            if (CoreKeyboard.KeyboardPlayerIndex == MyPlayerIndex)
            {
                Vector2 KeyboardDir = Vector2.Zero;

                CurInput.A_Button |= Tools.Keyboard.IsKeyDownCustom(Keys.Up);
                CurInput.A_Button |= Tools.Keyboard.IsKeyDownCustom(ButtonCheck.Up_Secondary);
                KeyboardDir.X = KeyboardDir.Y = 0;
                if (Tools.Keyboard.IsKeyDownCustom(Keys.Up)) KeyboardDir.Y = 1;
                if (Tools.Keyboard.IsKeyDownCustom(Keys.Down)) KeyboardDir.Y = -1;
                if (Tools.Keyboard.IsKeyDownCustom(Keys.Right)) KeyboardDir.X = 1;
                if (Tools.Keyboard.IsKeyDownCustom(Keys.Left)) KeyboardDir.X = -1;
                if (Tools.Keyboard.IsKeyDownCustom(ButtonCheck.Left_Secondary)) KeyboardDir.X = -1;
                if (Tools.Keyboard.IsKeyDownCustom(ButtonCheck.Right_Secondary)) KeyboardDir.X = 1;
                if (Tools.Keyboard.IsKeyDownCustom(ButtonCheck.Up_Secondary)) KeyboardDir.Y = 1;
                if (Tools.Keyboard.IsKeyDownCustom(ButtonCheck.Down_Secondary)) KeyboardDir.Y = -1;

                CurInput.B_Button |= Tools.Keyboard.IsKeyDownCustom(ButtonCheck.Back_Secondary);

                if (KeyboardDir.LengthSquared() > CurInput.xVec.LengthSquared())
                    CurInput.xVec = KeyboardDir;
            }
#if WINDOWS
            if (!GamepadUsed && (CurInput.xVec != Vector2.Zero || CurInput.A_Button || CurInput.B_Button))
            {
                InputFromKeyboard = true;
            }
#endif

#endif        

            // Invert left-right for inverted levels
            if (Core.MyLevel != null && Core.MyLevel.ModZoom.X < 0)
                CurInput.xVec.X *= -1;
        }
        
        public void GetRecordedInput(int Step)
        {
            if (Core.MyLevel.Replay)
            {
                if (Step < MyRecord.Input.Length)
                    CurInput = MyRecord.Input[Step];
                
                return;
            }

            if (MyPiece != null && Step < MyPiece.PieceLength)
            {
                CurInput = MyRecord.Input[Step];
            }
            else
            {
                CurInput.xVec = new Vector2(1, 0);
                CurInput.A_Button = true;
                CurInput.B_Button = false;
            }
        }

        void RecordInput(int Step)
        {
            MyRecord.Input[Step] = CurInput;
        }

        public void AnimStep()
        {
            if (Dying) return;

            MyPhsx.AnimStep();
        }

        /// <summary>
        ///// When true the call to AnimAndUpdate must be done manually.
        /// </summary>
        public bool ManualAnimAndUpdate = false;
        public void AnimAndUpdate()
        {
            AnimStep();
            UpdateObject();
        }

        /// <summary>
        /// The position of the stickman object
        /// </summary>
        public Vector2 ObjectPos
        {
            get { return PlayerObject.ParentQuad.Center.Pos; }
        }

        public Vector2 ExtraShift = Vector2.Zero;
        public void UpdateObject()
        {
            Vector2 NewCenter = Core.Data.Position - (PlayerObject.BoxList[1].TR.Pos - PlayerObject.ParentQuad.Center.Pos - Box.Current.Size);
            //Vector2 NewCenter = Core.Data.Position - (PlayerObject.ParentQuad.Center.Pos - PlayerObject.BoxList[1].BL.Pos + new Vector2(69.09941f, 104.1724f));
            //NewCenter += ExtraShift;

            PlayerObject.ParentQuad.Center.Move(NewCenter);
            PlayerObject.ParentQuad.Update();

            PlayerObject.Update(null);
            //Core.Data.Position = PlayerObject.BoxList[1].Center();
        }

        public void UpdateColors()
        {
            if (MyObjectType is BobPhsxBlobby && PlayerObject.QuadList != null)
            {
                var ql = PlayerObject.QuadList;
                if (ql.Count >= 1) PlayerObject.QuadList[1].SetColor(Color.White);
                if (ql.Count >= 1) PlayerObject.QuadList[1].MyMatrix = ColorHelper.HsvTransform(1, 1, 170) * MyColorScheme.SkinColor.M;
                if (ql.Count >= 1) PlayerObject.QuadList[1].MyEffect = Tools.HslEffect;
                if (ql.Count >= 0) PlayerObject.QuadList[0].Show = false;
                if (ql.Count >= 2) PlayerObject.QuadList[2].Show = false;
            }
            else if (MyObjectType is BobPhsxSpaceship && PlayerObject.QuadList != null)
            {
                var ql = PlayerObject.QuadList;
                if (ql.Count >= 1) PlayerObject.QuadList[1].SetColor(Color.White);
                if (ql.Count >= 1) PlayerObject.QuadList[1].MyMatrix = MyColorScheme.SkinColor.M;
                if (ql.Count >= 1) PlayerObject.QuadList[1].MyEffect = Tools.HslGreenEffect;
                if (ql.Count >= 0) PlayerObject.QuadList[0].Show = false;
                if (ql.Count >= 2) PlayerObject.QuadList[2].Show = false;
            }
        }

        public static bool GuideActivated = false;
        static QuadClass GuideQuad;
        void InitGuideQuad()
        {
            if (GuideQuad != null) return;

            GuideQuad = new QuadClass();
            GuideQuad.EffectName = "Circle";
            GuideQuad.Size = new Vector2(100, 100);
        }

        static int GuideLength = 8;
        static float Guide_h = 1f / GuideLength;
        void DrawGuidePiece(int Step, Vector2[] Loc, int i)
        {
            if (Loc.Length > Step)
            {
                InitGuideQuad();

                Vector2 Size = new Vector2(100 - 50 * Guide_h * i);
                //Vector2 Size = new Vector2(40);

                GuideQuad.Quad.SetColor(new Color(0f, 0f, 0f, 1f - Guide_h * i));
                GuideQuad.Size = Size * 1.15f;

                GuideQuad.Pos = Loc[Step];
                GuideQuad.Draw();


                Color c = MyColorScheme.SkinColor.Clr; c.A = (byte)(255 * (1f - Guide_h * i));
                //GuideQuad.Quad.SetColor(new Color(0f, 1f, 0f, 1f - Guide_h * i));
                GuideQuad.Quad.SetColor(c);
                GuideQuad.Size = Size;

                GuideQuad.Pos = Loc[Step];
                GuideQuad.Draw();
            }
        }

        void InitSectionDraw()
        {
                Vector2 Size = new Vector2(15);

                GuideQuad.Quad.SetColor(Color.PowderBlue);
                //GuideQuad.Quad.SetColor(Color.Black);
                //GuideQuad.Quad.SetColor(new Color(0,255,0,150));
                GuideQuad.Size = Size;
        }
        void DrawSection(int Step, Vector2[] Loc)
        {
                GuideQuad.Pos = Loc[Step];
                GuideQuad.Draw();
        }

        void DrawGuide()
        {            
            if (MyPiece != null && MyPiece.Recording != null && MyPiece.Recording.Length > MyPieceIndex)
            {
                int Step = Core.MyLevel.GetPhsxStep();
                Step = Math.Max(0, Step - 2);

                Vector2[] Loc = MyPiece.Recording[MyPieceIndex].AutoLocs;

                InitGuideQuad();
                int N = Math.Min(1000, Loc.Length);
                N = Math.Min(N, MyPiece.PieceLength);
                InitSectionDraw();

                for (int i = 1; i < N; i += 2)
                    DrawSection(i, Loc);
                
                if (Loc != null && N > Step)
                    DrawGuidePiece(Step, Loc, 2);
                Tools.QDrawer.Flush();
            }
        }

        /// <summary>
        /// Whether the player is visible on the screen.
        /// </summary>
        public bool IsVisible()
        {
            if (!Core.Show) return false;

            if (Bob.AllExplode)
            {
                if (Dying || Dead) return false;
            }

            return true;
        }

        QuadClass Rocket;
        public Vector2 RocketOffset = new Vector2(-60, 0);
        void DrawTheRocket()
        {
            if (Rocket == null)
            {
                Rocket = new QuadClass("Castle_Jet_Pack");
                //Rocket = new QuadClass("RocketPack");
                Rocket.FancyAngle = new FancyVector2();
                Rocket.Quad.MyEffect = Tools.HslEffect;
                Rocket.Degrees = -20;
            }
            else
            {
                float scale = .3675f;

                Rocket.Degrees = -33;
                Rocket.ScaleYToMatchRatio(PlayerObject.ParentQuad.Size.X * scale);
                Rocket.Pos = Pos + new Vector2(-88, 20) * GetScale();
                Rocket.Draw();

                Rocket.Degrees = 33;
                Rocket.Quad.MirrorUV_Horizontal();
                Rocket.ScaleYToMatchRatio(PlayerObject.ParentQuad.Size.X * scale);
                Rocket.Pos = Pos + new Vector2(93, 20) * GetScale();
                Rocket.Draw();
                Rocket.Quad.MirrorUV_Horizontal();
            }
        }

        public Vector2 GetScale()
        {
            return PlayerObject.ParentQuad.Size / new Vector2(260);
        }

        public override void Draw()
        {
            bool SkipDraw = false;

            // Draw guide
            if (GuideActivated && Core.MyLevel != null && !Core.MyLevel.Watching && !Core.MyLevel.Replay)
                DrawGuide();

            if (!Core.Show)
                return;

            if (Dying || Dead)
            {
                if (Bob.AllExplode && !Bob.ShowCorpseAfterExplode) return;

                if (MyObjectType is BobPhsxSpaceship)
                {
                    return;
                }
            }

            if ((Dying || Dead) && FlamingCorpse)                
                ParticleEffects.Flame(Core.MyLevel, Core.Data.Position + 1.5f * Core.Data.Velocity, Core.MyLevel.GetPhsxStep(), 1f, 10, false);

            UpdateColors();

            // Draw guide
            //if (GuideActivated && Core.MyLevel != null && !Core.MyLevel.Watching && !Core.MyLevel.Replay)
            //    DrawGuide();

            if (FadingIn)
            {
                Fade += .033f;
                if (Fade >= 1)
                {
                    FadingIn = false;
                    Fade = 1;
                    PlayerObject.ContainedQuad.SetColor(new Color(1, 1, 1, 1));
                }

                if (MyCape != null)
                    MyCape._MyColor.A = (byte)(255 * Fade);
            }


            if (MyCape != null && CanHaveCape && Core.Show && ShowCape && !SkipDraw && Tools.DrawGraphics)
            {                
                MyCape.Draw();
                Tools.QDrawer.Flush();
                //return;
            }

            if (Tools.DrawGraphics && !BoxesOnly && Core.Show)
            {
                if (!SkipDraw)
                {
                    if (MyPhsx.ThrustType == BobPhsx.RocketThrustType.Double)
                        DrawTheRocket();

                    Tools.QDrawer.SetAddressMode(false, false);
                    if (MyPhsx != null)
                    {
                        MyPhsx.PreObjectDraw();
                        MyPhsx.ObjectDraw();
                    }
                }
            }

            if (Tools.DrawBoxes)
            {
                //Box.DrawFilled(Tools.QDrawer, Color.HotPink);
                Box2.DrawT(Tools.QDrawer, Color.HotPink, 12);

                //Box.Draw(Tools.QDrawer, Color.HotPink, 12);
                //Box.DrawT(Tools.QDrawer, Color.HotPink, 6);
                //Box2.Draw(Tools.QDrawer, Color.HotPink, 12);
                //Box2.DrawT(Tools.QDrawer, Color.HotPink, 12);

                if (Boxes != null)
                {
                    Boxes[0].Draw(Tools.QDrawer, Color.Red, 8);
                    Boxes[3].Draw(Tools.QDrawer, Color.Green, 8);
                    Boxes[8].Draw(Tools.QDrawer, Color.Blue, 8);
                }
            }
        }

        public override void Move(Vector2 shift)
        {
            Core.Data.Position += shift;

            Box.Move(shift);
            Box2.Move(shift);

            if (PlayerObject == null)
                return;

            PlayerObject.ParentQuad.Center.Move(PlayerObject.ParentQuad.Center.Pos + shift);
            PlayerObject.ParentQuad.Update();
            PlayerObject.Update(null);

            if (MyCape != null)
                MyCape.Move(shift);
        }

        public void InteractWithBlock(AABox box, BlockBase block, ColType Col)
        {            
            if (block != null && !block.IsActive) return;

            if (block != null && Col != ColType.NoCol) block.Hit(this);

            if (block != null && Col != ColType.NoCol)
                if (Col != ColType.Top)
                    block.BlockCore.NonTopUsed = true;


            ColType OriginalColType = Col;

            if (MyPhsx.IsTopCollision(Col, box, block))
            {
                //if (Col != ColType.Top) Tools.Write(0);

                Col = ColType.Top;

                NewY = box.Target.TR.Y + Box.Current.Size.Y + .01f;

                if (Core.Data.Position.Y <= NewY)
                {
                    NewVel = Math.Max(-1000, MyPhsx.ForceDown + box.Target.TR.Y - box.Current.TR.Y);

                    if (block != null)
                    {
                        block.BlockCore.StoodOn = true;
                        block.LandedOn(this);
                    }

                    if (!TopCol)
                    {
                        BottomCol = true;

                        if (OriginalColType == ColType.Top)
                        {
                            Core.Data.Position.Y = NewY;

                            if (block == null || block.BlockCore.GivesVelocity)
                            {
                                if (MyPhsx.Sticky)
                                    Core.Data.Velocity.Y = NewVel;
                                else
                                    Core.Data.Velocity.Y = Math.Max(NewVel, Core.Data.Velocity.Y);
                            }

                            UpdateGroundSpeed(box, block);
                        }
                        else
                        {
                            // We hit the block from the side, just at the top edge
                            // Keep bigger Y values
                            Core.Data.Position.Y = Math.Max(Core.Data.Position.Y, NewY);
                            if (block == null || block.BlockCore.GivesVelocity)
                                Core.Data.Velocity.Y = Math.Max(Core.Data.Velocity.Y, NewVel);
                        }                        

                        //MyPhsx.ObjectLandedOn = block;
                        MyPhsx.LandOnSomething(false, block);
                        
                        if (OnLand != null) OnLand(); OnLand = null;
                    }
                }
            }

            if (Col != ColType.NoCol && (block == null || block.BlockCore.MyType != ObjectType.LavaBlock))
            {
                if (!box.TopOnly)
                {
                    if (Col == ColType.Bottom && !(Col == ColType.Left || Col == ColType.Right))
                    {
                        TopCol = true;
                    }

                    //if (Col == ColType.Right) Tools.Write(0);
                    //if (Col == ColType.Left) Tools.Write(0);

                    if (MyPhsx.IsBottomCollision(Col, box, block))
                        Col = ColType.Bottom;

                    NewY = box.Target.BL.Y - Box.Current.Size.Y - .01f;
                    if (Core.Data.Position.Y > NewY && Col == ColType.Bottom)
                    {
                        if (MyPhsx.OnGround && block.BlockCore.DoNotPushHard)
                        {
                            block.Smash(this);
                            return;
                        }

                        MyPhsx.HitHeadOnSomething(block);

                        if (block != null)
                            block.HitHeadOn(this);

                        if (OriginalColType == ColType.Bottom)
                        {
                            Core.Data.Position.Y = NewY;

                            if (block == null || block.BlockCore.GivesVelocity)
                            {
                                NewVel = Math.Min(Math.Min(0, NewVel), box.Target.BL.Y - box.Current.BL.Y) + 10;
                                Core.Data.Velocity.Y = Math.Min(NewVel, Core.Data.Velocity.Y);
                            }

                            // If we are inverted, then we can take on the speed of the block we have landed on upside-down.
                            if (MyPhsx.Gravity < 0)
                                UpdateGroundSpeed(box, block);
                        }
                        else
                        {
                            // We hit the block from the side, just at the bottom edge
                            // Keep smaller Y values
                            Core.Data.Position.Y = Math.Min(Core.Data.Position.Y, NewY);
                            if (block == null || block.BlockCore.GivesVelocity)
                            {
                                NewVel = Math.Min(0, box.Target.BL.Y - box.Current.BL.Y) + 10;
                                Core.Data.Velocity.Y = Math.Min(NewVel, Core.Data.Velocity.Y);
                            }

                        }                        
                    }
                    else
                    {
                        Xvel = box.Target.TR.X - box.Current.TR.X;

                        if (Col == ColType.Left)
                        {
                            if (block != null)
                                block.SideHit(this);

                            MyPhsx.SideHit(Col, block);

                            Core.Data.Position.X = box.Target.BL.X - Box.Current.Size.X - .01f;

                            SideHitCount += 2;
                            if (SideHitCount > 5) Core.Data.Velocity.X *= .4f;

                            if (block == null || block.BlockCore.GivesVelocity)
                            if (Xvel < Core.Data.Velocity.X)
                                if (Box.Current.BL.Y < box.Current.TR.Y - 35 &&
                                    Box.Current.TR.Y > box.Current.BL.Y + 35)
                                    Core.Data.Velocity.X = Xvel;
                        }

                        if (Col == ColType.Right)
                        {
                            if (block != null)
                                block.SideHit(this);

                            MyPhsx.SideHit(Col, block);

                            Core.Data.Position.X = box.Target.TR.X + Box.Current.Size.X + .01f;

                            SideHitCount += 2;
                            if (SideHitCount > 5) Core.Data.Velocity.X *= .4f;

                            if (block == null || block.BlockCore.GivesVelocity)
                            if (Xvel > Core.Data.Velocity.X)
                                if (Box.Current.BL.Y < box.Current.TR.Y - 35 &&
                                    Box.Current.TR.Y > box.Current.BL.Y + 35)
                                    Core.Data.Velocity.X = Xvel;
                        }
                    }
                }
            }
        }

        private void UpdateGroundSpeed(AABox box, BlockBase block)
        {
            GroundSpeed = box.xSpeed();

            if (block != null)
                GroundSpeed += block.BlockCore.GroundSpeed;
        }

        public void InitBoxesForCollisionDetect()
        {
            Box.Current.Size = PlayerObject.BoxList[1].Size() / 2;
            Box2.Current.Size = PlayerObject.BoxList[2].Size() / 2;

            if (Core.MyLevel.PlayMode != 0 && Core.MyLevel.DefaultHeroType is BobPhsxSpaceship)
            {
                Box.Current.Size *= 1.2f;
                Box2.Current.Size *= 1.2f;
            }

            if (MyPhsx.Gravity > 0)
            {
                Box.SetTarget(Core.Data.Position, Box.Current.Size + new Vector2(.0f, .02f));
                Box.Target.TR.Y += 5;
            }
            else
            {
                Box.SetTarget(Core.Data.Position, Box.Current.Size + new Vector2(.0f, -.02f));
                Box.Target.BL.Y -= 5;
            }

            Box2.SetTarget(Core.Data.Position + MyPhsx.TranscendentOffset, Box2.Current.Size);

            MyPhsx.OnInitBoxes();
        }

        public bool UseCustomCapePos = false;
        public Vector2 CustomCapePos;
        public void UpdateCape()
        {
            if (!CanHaveCape) // || !ShowCape)
                return;

            MyCape.Wind = CapeWind;
            Vector2 AdditionalWind = Vector2.Zero;
            if (Core.MyLevel != null && Core.MyLevel.MyBackground != null)
            {
                AdditionalWind += Core.MyLevel.MyBackground.Wind;
                MyCape.Wind += AdditionalWind;
            }
            //MyCape.Wind.X -= .2f * Core.Data.Velocity.X;

            if (MyPhsx.Ducking && MyObjectType != BobPhsxBox.Instance)
                MyCape.GravityScale = .4f;
            else
                MyCape.GravityScale = 1f;

            // Set the anchor point
            if (temp == null) temp = new ObjectVector();            
            if (Head == null) Head = (Quad)PlayerObject.FindQuad("Head");

            if (UseCustomCapePos)
                temp.Pos = Pos + CustomCapePos;
            else
                temp.Pos = Head.Center.Pos;
            temp.Pos += MyPhsx.TranscendentOffset;

            if (Dead)
                temp.Pos += new Vector2(60, -50) * MyPhsx.ModCapeSize + new Vector2(0, 3 * (1 / MyPhsx.ModCapeSize.Y - 1));
            else if (MyPhsx.Ducking)
                temp.Pos += MyPhsx.CapeOffset_Ducking * MyPhsx.ModCapeSize * new Vector2(PlayerObject.xFlip ? -1 : 1, 1) +
                    new Vector2(0, 3 * (1 / MyPhsx.ModCapeSize.Y - 1));
            else
                temp.Pos += MyPhsx.CapeOffset * MyPhsx.ModCapeSize;


            MyCape.Gravity = MyPhsx.CapeGravity;

            Vector2 vel = MyPhsx.ApparentVelocity;
            MyCape.AnchorPoint[0] = temp.Pos + (vel);

            //Tools.Write("{0} {1} {2}", Core.Data.Position, MyCape.AnchorPoint[0], MyCape.AnchorPoint[1]);

            if (Core.MyLevel != null)
            {
                float t = Core.MyLevel.GetPhsxStep() / 2.5f;
                if (CharacterSelect2)
                    t = Tools.DrawCount / 2.5f;
                float AmplitudeX = Math.Min(2.5f, Math.Abs(vel.X - AdditionalWind.X) / 20);
                MyCape.AnchorPoint[0].Y += 15 * (float)(Math.Cos(t) * AmplitudeX);
                float Amp = 2;
                if (vel.Y < 0)
                    Amp = 8;
                float AmplitudeY = Math.Min(2.5f, Math.Abs(vel.Y - AdditionalWind.Y) / 45);
                MyCape.AnchorPoint[0].X += Amp * (float)(Math.Sin(t) * AmplitudeY);
            }
            //MyCape.AnchorPoint[0].X += .1f * (Core.Data.Velocity).X;
            Vector2 CheatShift = Vector2.Zero;//new Vector2(.15f, .35f) * Core.Data.Velocity;
            float l = (vel - 2*AdditionalWind).Length();
            if (l > 15)
            {
                CheatShift = (vel - 1*AdditionalWind);
                CheatShift.Normalize();
                CheatShift = (l - 15) * CheatShift;
            }
            MyCape.Move(CheatShift);
            //for (int i = 0; i < 1; i++)
            MyCape.PhsxStep();
            //MyCape.MyColor = Color.Gray;
        }

        public void CorePhsxStep()
        {
        }

        public void DollPhsxStep()
        {
            CurInput.A_Button = false;
            CurInput.B_Button = false;
            CurInput.xVec = Vector2.Zero;

            // Phsyics update
            MyPhsx.PhsxStep();

            // Integrate velocity
            Core.Data.Position += Core.Data.Velocity;
            if (MyPhsx.UseGroundSpeed)
                Core.Data.Position += new Vector2(GroundSpeed, 0);

            // Cape
            if (Core.MyLevel.PlayMode == 0 && MyCape != null)
                UpdateCape();

            MyPhsx.OnGround = true;
        }

        /// <summary>
        /// Whether to do object interactions.
        /// </summary>
        public bool DoObjectInteractions = true;

        void FlyingPhsx()
        {
            if (MyPhsx.OnGround && CurInput.xVec.Y > 0) MyPhsx.yVel += .7f;
            MyPhsx.Vel *= .985f;

            MyPhsx.Vel += CurInput.xVec;
        }

        static BobInput FirstBobInput;
        public override void PhsxStep()
        {
            DoLightSourceFade();

            if (!Core.Show)
            {
                SetRecordingInfo();
                return;
            }

            if (CharacterSelect2)
            {
                DollPhsxStep();
                SetRecordingInfo();
                return;
            }

            if (ImmortalCountDown > 0)
            {
                ImmortalCountDown--;
                if (ImmortalCountDown < ImmortalLength - 15)
                if (Math.Abs(CurInput.xVec.X) > .5f || CurInput.A_Button)
                    ImmortalCountDown = 0;
            }

            SaveNoBlock = false;


            int CurPhsxStep = Core.MyLevel.CurPhsxStep;



            // Bob connections
            if (MyBobLinks != null)
                foreach (BobLink link in MyBobLinks)
                    link.PhsxStep(this);

            if (Dead || Dying) DeadCount++;

            if (Dying)
            {
                DyingPhsxStep();

                // Cape
                if (Core.MyLevel.PlayMode == 0 && MyCape != null)
                    UpdateCape();

                SetRecordingInfo();
                return;
            }

            // Track Star bonus book keeping
            if (GiveStats() && Core.MyLevel.CurPhsxStep > 45)
            {
                MyTempStats.FinalTimeSpent++;

                if (Math.Abs(CurInput.xVec.X) < .75f)
                    MyTempStats.FinalTimeSpentNotMoving++;
            }

            // Increment life counter
            if (Core.MyLevel.PlayMode == 0 && !CompControl && !Core.MyLevel.Watching)
                MyStats.TimeAlive++;

            // Screen wrap
            CheckForScreenWrap();

            if (!CharacterSelect)
            {
                int Mode = Core.MyLevel.PlayMode;
                if (Core.MyLevel.NumModes == 1)
                    if (Mode == 1) Mode = 2;

                switch (Mode)
                {
                    case 0:
                        if (!CompControl)
                        {
                            if (Cinematic)
                                Tools.Nothing();//AnimAndUpdate();
                            else
                            {
                                if (CodeControl)
                                {
                                    CurInput.Clean();
                                    if (ControlFunc != null)
                                    {
                                        ControlCount++;
                                        ControlFunc(ControlCount);
                                    }
                                }
                                else
                                    GetPlayerInput();
                            }
                        }
                        else
                            GetRecordedInput(CurPhsxStep - IndexOffset);

                        break;

                    case 1:
                        GetRecordedInput(CurPhsxStep - IndexOffset);

                        break;

                    case 2:
                        MyPhsx.GenerateInput(CurPhsxStep);
                        RecordInput(CurPhsxStep - IndexOffset);

                        break;
                }
            }
            else
            {
                CurInput.A_Button = false;
                CurInput.B_Button = false;
                CurInput.xVec = Vector2.Zero;
            }

            if (Dopple)
            {
                CurInput = FirstBobInput;
            }
            else
            {
                FirstBobInput = CurInput;
            }

            // Phsyics update
            if (Dopple)
            {
                if (MoveData.InvertDirX || MyLevel.MySourceGame != null && MyLevel.MySourceGame.MyGameFlags.IsDopplegangerInvert && !MyLevel.IsHorizontal())
                {
                    CurInput.xVec.X *= -1;
                }
            }
            float Windx = Wind.X;
            if (MyPhsx.OnGround) Windx /= 2;
            Core.Data.Velocity.X -= Windx;
            if (Flying)
                FlyingPhsx();
            else
                MyPhsx.PhsxStep();
            Core.Data.Velocity.X += Windx;
            MyPhsx.CopyPrev();

            // Collision with screen boundary
            if (CollideWithCamera && !Cinematic)
            {
                Box.CalcBounds();
                if (Box.TR.X > Core.MyLevel.MainCamera.TR.X - 40 && Core.Data.Velocity.X > 0)
                {
                    Core.Data.Velocity.X = 0;
                    MyPhsx.SideHit(ColType.Right, null);
                    if (Box.TR.X > Core.MyLevel.MainCamera.TR.X - 20 && Core.Data.Velocity.X > 0)
                    {
                        Move(new Vector2(Core.MyLevel.MainCamera.TR.X - 20 - Box.TR.X, 0));
                    }
                }
                if (Box.BL.X < Core.MyLevel.MainCamera.BL.X + 40 && Core.Data.Velocity.X < 0)
                {
                    Core.Data.Velocity.X = 0;
                    MyPhsx.SideHit(ColType.Left, null);
                    if (Box.BL.X < Core.MyLevel.MainCamera.BL.X + 20 && Core.Data.Velocity.X < 0)
                    {
                        Move(new Vector2(Core.MyLevel.MainCamera.BL.X + 20 - Box.BL.X, 0));
                    }
                }
            }


            // Integrate velocity
            if (!Cinematic)
                //Core.Data.Position += Core.Data.Velocity + new Vector2(GroundSpeed, 0);
                MyPhsx.Integrate();

            // Cape
            if (Core.MyLevel.PlayMode == 0 && MyCape != null)
                UpdateCape();
            Wind /= 2;
            CapeWind /= 2;

            // If cinematic, don't do any death or object interactions
            if (Cinematic)
            {
                AnimAndUpdate();
                ControlCount++;
                if (CinematicFunc != null) CinematicFunc(ControlCount);

                SetRecordingInfo();
                return;
            }

            // If too high, knock Bob down a bit
            if (Core.Data.Position.Y > Core.MyLevel.MainCamera.TR.Y + 900 && Core.Data.Velocity.Y > 0)
                Core.Data.Velocity.Y = 0;

            // Check for death by falling or by off screen
            if (Core.MyLevel.PlayMode == 0)
            {
                float DeathDist = 650;
                if (Core.MyLevel.MyGame.MyGameFlags.IsTethered) DeathDist = 900;
                if (Core.Data.Position.Y < Core.MyLevel.MainCamera.BL.Y - DeathDist)
                    Die(BobDeathType.Fall);
                else if (MyPhsx.Gravity < 0 && Core.Data.Position.Y > Core.MyLevel.MainCamera.TR.Y + DeathDist)
                    Die(BobDeathType.Fall);
                else
                    if (Core.Data.Position.Y > Core.MyLevel.MainCamera.TR.Y + 1500 ||
                        Core.Data.Position.X < Core.MyLevel.MainCamera.BL.X - 550 ||
                        Core.Data.Position.X > Core.MyLevel.MainCamera.TR.X + 550)
                    {
                        Die(BobDeathType.LeftBehind);
                    }
            }

            // Check for death by time out
            if (Core.MyLevel.PlayMode == 0 && Core.MyLevel.CurPhsxStep > Core.MyLevel.TimeLimit && Core.MyLevel.TimeLimit > 0)
                Die(BobDeathType.Time);

            // Initialize boxes for collision detection
            InitBoxesForCollisionDetect();

            /////////////////////////////////////////////////////////////////////////////////////////////
            //                 Block Interactions                                                      //
            /////////////////////////////////////////////////////////////////////////////////////////////            
            NewVel = 0;
            BlockInteractions();

            /////////////////////////////////////////////////////////////////////////////////////////////
            //                 Object Interactions                                                     //
            /////////////////////////////////////////////////////////////////////////////////////////////            
            if (DoObjectInteractions)
                ObjectInteractions();

            // Reset boxes to normal
            Box.SetCurrent(Core.Data.Position, Box.Current.Size);
            Box2.SetCurrent(Core.Data.Position + MyPhsx.TranscendentOffset, Box2.Current.Size);

//if (Core.MyLevel.PlayMode != 0)
//    for (int i = 0; i <= NumBoxes; i++)
//    {
//        AABox box = Boxes[i];
//        box.SetCurrent(Core.Data.Position, box.Current.Size);
//    }

            // Closing phsx
            MyPhsx.PhsxStep2();

            PrevInput = CurInput;
            SetRecordingInfo();
        }

        /// <summary>
        /// Calculate all interactions between the player and every IObject in the level.
        /// </summary>
        void ObjectInteractions()
        {
            if (Core.MyLevel.PlayMode != 0)
            {
                // Create list of boxes
                if (Boxes == null)
                {
                    Boxes = new List<AABox>();
                    for (int i = 0; i <= NumBoxes; i++)
                        Boxes.Add(new AABox(Vector2.Zero, Vector2.One));
                }

                // Update box list
                UpdateBoxList();
            }
            else
                Box2.SetTarget(Core.Data.Position + MyPhsx.TranscendentOffset, Box2.Current.Size);
            Box.SetTarget(Core.Data.Position, Box.Current.Size + new Vector2(.0f, .2f));


            foreach (ObjectBase obj in Core.MyLevel.ActiveObjectList)
            {
                if (!obj.Core.MarkedForDeletion && obj.Core.Real && obj.Core.Active && obj.Core.Show)
                    obj.Interact(this);
            }
        }

        /// <summary>
        /// Update the list of AABox boxes used by the computer when creating the level.
        /// </summary>
        void UpdateBoxList()
        {
            float extra = 0;
            for (int i = 0; i <= NumBoxes; i++)
            {
                AABox box = Boxes[i];

                box.Current.Size = Box2.Current.Size;
                
                box.Current.Size.X += extra + .7f * Upgrades.MaxBobWidth * ((NumBoxes - i) * .1f);
                box.Current.Size.Y += extra + .23f * Upgrades.MaxBobWidth * ((NumBoxes - i) * .1f);

                box.SetCurrent(Box2.Current.Center, box.Current.Size);
                box.SetTarget(Core.Data.Position + MyPhsx.TranscendentOffset, box.Current.Size);
            }
            RegularBox2 = Boxes[Boxes.Count - 1];

            Box2.Current.Size.X += extra + .7f * Upgrades.MaxBobWidth;
            Box2.Current.Size.Y += extra + .23f * Upgrades.MaxBobWidth;
            Box2.SetTarget(Core.Data.Position + MyPhsx.TranscendentOffset, Box2.Current.Size);
        }

        public void DeleteObj(ObjectBase obj)
        {
            obj.Core.DeletedByBob = true;
            Core.Recycle.CollectObject(obj);
        }

        public Ceiling_Parameters CeilingParams;
        /// <summary>
        /// Calculate all interactions between the player and every Block in the level.
        /// </summary>
        void BlockInteractions()
        {
            //OldBlockInteractions();
            NewBlockInteractions();
        }

        void OldBlockInteractions()
        {
            int CurPhsxStep = Core.MyLevel.CurPhsxStep;

            GroundSpeed = 0;

            SideHitCount--;
            if (SideHitCount < 0) SideHitCount = 0;

            MyPhsx.ResetJumpModifiers();

            BottomCol = TopCol = false;
            if (CanInteract)
                if (Core.MyLevel.PlayMode != 2)
                {
                    if (Core.MyLevel.DefaultHeroType is BobPhsxSpaceship && Core.MyLevel.PlayMode == 0)
                    {
                        foreach (BlockBase block in Core.MyLevel.Blocks)
                        {
                            if (!block.Core.MarkedForDeletion && block.IsActive && Phsx.BoxBoxOverlap(Box2, block.Box))
                            {
                                if (!Immortal)
                                    Die(BobDeathType.Other);
                                else
                                    block.Hit(this);
                            }
                        }
                    }
                    else
                    {
                        foreach (BlockBase block in Core.MyLevel.Blocks)
                        {
                            if (block.Core.MarkedForDeletion || !block.IsActive || !block.Core.Real) continue;
                            if (block.BlockCore.OnlyCollidesWithLowerLayers && block.Core.DrawLayer <= Core.DrawLayer)
                                continue;

                            ColType Col = Phsx.CollisionTest(Box, block.Box);
                            if (Col != ColType.NoCol)
                            {
                                InteractWithBlock(block.Box, block, Col);
                            }
                        }
                    }
                }
                else
                {
                    Ceiling_Parameters CeilingParams = (Ceiling_Parameters)Core.MyLevel.CurPiece.MyData.Style.FindParams(Ceiling_AutoGen.Instance);

                    foreach (BlockBase block in Core.MyLevel.Blocks)
                    {
                        if (block.Core.MarkedForDeletion || !block.IsActive || !block.Core.Real) continue;
                        if (block.BlockCore.OnlyCollidesWithLowerLayers && block.Core.DrawLayer <= Core.DrawLayer)
                            continue;

                        if (block.BlockCore.Ceiling)
                        {
                            if (Core.Data.Position.X > block.Box.Current.BL.X - 100 &&
                                Core.Data.Position.X < block.Box.Current.TR.X + 100)
                            {
                                float NewBottom = block.Box.Current.BL.Y;
                                // If ceiling has a left neighbor make sure we aren't too close to it
                                if (block.BlockCore.TopLeftNeighbor != null)
                                {
                                    if (NewBottom > block.BlockCore.TopLeftNeighbor.Box.Current.BL.Y - 100)
                                        NewBottom = Math.Max(NewBottom, block.BlockCore.TopLeftNeighbor.Box.Current.BL.Y + 120);
                                }
                                block.Extend(Side.Bottom, Math.Max(NewBottom, Math.Max(Box.Target.TR.Y, Box.Current.TR.Y) + CeilingParams.BufferSize.GetVal(Core.Data.Position)));
                                if (block.Box.Current.Size.Y < 170 ||
                                    block.Box.Current.BL.Y > Core.MyLevel.MainCamera.TR.Y - 75)
                                {
                                    DeleteObj(block);
                                }
                            }
                            continue;
                        }

                        // For lava blocks...
                        if (block is LavaBlock)
                        {
                            // If the computer gets close, move the lava block down
                            if (Box.Current.TR.X > block.Box.Current.BL.X &&
                                Box.Current.BL.X < block.Box.Current.TR.X)
                            {
                                Core.MyLevel.PushLava(Box.Target.BL.Y - 60, block as LavaBlock);
                            }
                            continue;
                        }

                        if (!block.IsActive) continue;

                        ColType Col = Phsx.CollisionTest(Box, block.Box);
                        bool Overlap;
                        if (!block.Box.TopOnly || block.Core.GenData.RemoveIfOverlap)
                            Overlap = Phsx.BoxBoxOverlap(Box, block.Box);
                        else
                            Overlap = false;
                        if (Col != ColType.NoCol || Overlap)
                        {
                            if (block.BlockCore.Ceiling)
                            {
                                block.Extend(Side.Bottom, Math.Max(block.Box.Current.BL.Y, Math.Max(Box.Target.TR.Y, Box.Current.TR.Y) + CeilingParams.BufferSize.GetVal(Core.Data.Position)));
                                continue;
                            }

                            //if (Col != ColType.Top) Tools.Write("");

                            bool Delete = false;
                            bool MakeTopOnly = false;
                            if (SaveNoBlock) Delete = true;
                            if (BottomCol && Col == ColType.Top) Delete = true;
                            //if (Col == ColType.Top && Core.Data.Position.Y > TargetPosition.Y) Delete = true;
                            if (Col == ColType.Top && WantsToLand == false) Delete = true;
                            if (Col == ColType.Bottom && Core.Data.Position.Y < TargetPosition.Y) Delete = true;
                            if (Col == ColType.Left || Col == ColType.Right) MakeTopOnly = true;// Delete = true;
                            if (TopCol && Col == ColType.Bottom) Delete = true;
                            //if (block is MovingBlock2 && Col == ColType.Bottom) Delete = true;
                            if (Col == ColType.Bottom) Delete = true;
                            //if (CurPhsxStep < 2) Delete = true;
                            if (Overlap && Col == ColType.NoCol && !block.Box.TopOnly && !(block is NormalBlock && !block.BlockCore.NonTopUsed)) Delete = true;
                            if ((Col == ColType.Bottom || Overlap) && Col != ColType.Top) MakeTopOnly = true;
                            if ((Col == ColType.Left || Col == ColType.Right) && Col != ColType.Top)
                            {
                                if (Box.Current.TR.Y < block.Box.Current.TR.Y)
                                    MakeTopOnly = true;
                                else
                                    MakeTopOnly = true;
                                //MakeTopOnly = false;
                                //Delete = true;
                            }
                            if (block.BlockCore.NonTopUsed || !(block is NormalBlock))
                                if (MakeTopOnly)
                                {
                                    MakeTopOnly = false;
                                    Delete = true;
                                }

                            // Do not allow for TopOnly blocks
                            // NOTE: Wanted to have this uncommented, but needed to comment it
                            // out in order for final door blocks to work right
                            // (otherwise a block might be used and made not-toponly and block the computer)
                            //if (MakeTopOnly) { MakeTopOnly = false; Delete = true; }

                            if (MakeTopOnly && block.BlockCore.DeleteIfTopOnly)
                            {
                                if (block.Core.GenData.Used)
                                    MakeTopOnly = Delete = false;
                                else
                                    Delete = true;
                            }

                            if (MakeTopOnly)
                            {
                                block.Extend(Side.Bottom, Math.Max(block.Box.Current.BL.Y, Math.Max(Box.Target.TR.Y, Box.Current.TR.Y) + CeilingParams.BufferSize.GetVal(Core.Data.Position)));
                                ((NormalBlock)block).CheckHeight();
                                if (Col != ColType.Top)
                                    Col = ColType.NoCol;
                            }

                            // Don't land on the very edge of the block
                            if (!Delete && !MyPhsx.OnGround)
                            {
                                float Safety = block.BlockCore.GenData.EdgeSafety;
                                if (Box.BL.X > block.Box.TR.X - Safety ||
                                    Box.TR.X < block.Box.BL.X + Safety)
                                {
                                    Delete = true;
                                }
                            }

                            // Don't land on a block that says not to
                            bool DesiresDeletion = false;
                            {
                                if (block.Core.GenData.TemporaryNoLandZone ||
                                    !block.Core.GenData.Used && !block.PermissionToUse())
                                    DesiresDeletion = Delete = true;
                            }


                            if (block.Core.GenData.Used) Delete = false;
                            if (!DesiresDeletion && block.Core.GenData.AlwaysLandOn && !block.Core.MarkedForDeletion && Col == ColType.Top) Delete = false;
                            if (!DesiresDeletion && block.Core.GenData.AlwaysLandOn_Reluctantly && WantsToLand_Reluctant && !block.Core.MarkedForDeletion && Col == ColType.Top) Delete = false;
                            if (Overlap && block.Core.GenData.RemoveIfOverlap) Delete = true;
                            if (!DesiresDeletion && block.Core.GenData.AlwaysUse && !block.Core.MarkedForDeletion) Delete = false;

                            // Shift bottom of block if necessary
                            if (!Delete && !block.BlockCore.DeleteIfTopOnly)
                            {
                                float NewBottom = Math.Max(block.Box.Current.BL.Y,
                                                           Math.Max(Box.Target.TR.Y, Box.Current.TR.Y) + CeilingParams.BufferSize.GetVal(Core.Data.Position));

                                if (block is NormalBlock &&
                                    (Col == ColType.Bottom || Overlap) && Col != ColType.Top &&
                                    !block.BlockCore.NonTopUsed)
                                {
                                    block.Extend(Side.Bottom, NewBottom);
                                    ((NormalBlock)block).CheckHeight();
                                }

                                // Delete the box if it was made TopOnly but TopOnly is not allowed for this block
                                if (block.Box.TopOnly && block.BlockCore.DeleteIfTopOnly)
                                    Delete = true;
                            }

                            // We're done deciding if we should delete the block or not.
                            // If we should delete it, delete.
                            if (Delete)
                            {
                                DeleteObj(block);
                                block.IsActive = false;
                            }
                            // Otherwise keep it and interact with it
                            else
                            {
                                Delete = false;

                                if (Col != ColType.NoCol)
                                {
                                    // We changed the blocks property, so Bob may no longer be on a collision course with it. Check to see if he is before marking block as used.
                                    if (!block.Box.TopOnly || Col == ColType.Top)
                                    {
                                        if (block.Core.GenData.RemoveIfUsed)
                                            Delete = true;

                                        if (!Delete)
                                        {
                                            InteractWithBlock(block.Box, block, Col);
                                            block.StampAsUsed(CurPhsxStep);

                                            // Normal blocks delete surrounding blocks when stamped as used
                                            if (block.Core.GenData.DeleteSurroundingOnUse && block is NormalBlock)
                                                foreach (BlockBase nblock in Core.MyLevel.Blocks)
                                                {
                                                    NormalBlock Normal = nblock as NormalBlock;
                                                    if (null != Normal && !Normal.Core.MarkedForDeletion && !Normal.Core.GenData.AlwaysUse)
                                                        if (!Normal.Core.GenData.Used &&
                                                            Math.Abs(Normal.Box.Current.TR.Y - block.Box.TR.Y) < 15 &&
                                                            !(Normal.Box.Current.TR.X < block.Box.Current.BL.X - 350 || Normal.Box.Current.BL.X > block.Box.Current.TR.X + 350))
                                                        {
                                                            DeleteObj(Normal);
                                                            Normal.IsActive = false;
                                                        }
                                                }

                                            // Ghost blocks delete surrounding blocks when stamped as used
                                            if (block is GhostBlock)
                                                foreach (BlockBase gblock in Core.MyLevel.Blocks)
                                                {
                                                    GhostBlock ghost = gblock as GhostBlock;
                                                    if (null != ghost && !ghost.Core.MarkedForDeletion)
                                                        if (!ghost.Core.GenData.Used &&
                                                            (ghost.Core.Data.Position - block.Core.Data.Position).Length() < 200)
                                                        {
                                                            DeleteObj(ghost);
                                                            ghost.IsActive = false;
                                                        }
                                                }
                                        }
                                    }
                                }

                                Delete = false;
                                if (block.Core.GenData.RemoveIfOverlap)
                                {
                                    if (Phsx.BoxBoxOverlap(Box, block.Box))
                                        Delete = true;
                                }
                            }
                        }
                    }
                }
        }

        void NewBlockInteractions()
        {
            int CurPhsxStep = Core.MyLevel.CurPhsxStep;

            GroundSpeed = 0;

            SideHitCount--;
            if (SideHitCount < 0) SideHitCount = 0;

            MyPhsx.ResetJumpModifiers();

            BottomCol = TopCol = false;
            if (CanInteract)
                if (Core.MyLevel.PlayMode != 2)
                    MyPhsx.BlockInteractions();
                else
                {
                    CeilingParams = (Ceiling_Parameters)Core.GetParams(Ceiling_AutoGen.Instance);

                    foreach (BlockBase block in Core.MyLevel.Blocks)
                    {
                        if (MyPhsx.SkipInteraction(block)) continue;
                        //if (block.Core.MarkedForDeletion || !block.IsActive || !block.Core.Real) continue;
                        //if (block.BlockCore.OnlyCollidesWithLowerLayers && block.Core.DrawLayer <= Core.DrawLayer)
                        //    continue;

                        if (block.PreDecision(this)) continue;
                        if (!block.IsActive) continue;

                        // Collision check
                        ColType Col = Phsx.CollisionTest(Box, block.Box);
                        bool Overlap = false;
                        if (!block.Box.TopOnly || block.Core.GenData.RemoveIfOverlap)
                            Overlap = Phsx.BoxBoxOverlap(Box, block.Box);

                        if (Col != ColType.NoCol || Overlap)
                        {
                            if (block.PostCollidePreDecision(this)) continue;

                            bool Delete = false;
                            block.PostCollideDecision(this, ref Col, ref Overlap, ref Delete);

                            // We're done deciding if we should delete the block or not.
                            // If we should delete it, delete.
                            if (Delete)
                            {
                                DeleteObj(block);
                                block.IsActive = false;
                            }
                            // Otherwise keep it and interact with it
                            else
                            {
                                Delete = false;

                                block.PostKeep(this, ref Col, ref Overlap);

                                if (Col != ColType.NoCol)
                                {
                                    // We changed the blocks property, so Bob may no longer be on a collision course with it. Check to see if he is before marking block as used.
                                    if (!block.Box.TopOnly || Col == ColType.Top)
                                    {
                                        if (block.Core.GenData.RemoveIfUsed)
                                            Delete = true;

                                        if (!Delete)
                                        {
                                            //if (Col == ColType.Bottom && !block.Core.GenData.Used)
                                            //    Tools.Write("");

                                            //if (!MyPhsx.SkipInteraction(block))
                                                InteractWithBlock(block.Box, block, Col);
                                            block.StampAsUsed(CurPhsxStep);
                                            MyPhsx.LastUsedStamp = CurPhsxStep;

                                            block.PostInteractWith(this, ref Col, ref Overlap);
                                            //block.PostCollidePreDecision(this);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
        }
    }
}