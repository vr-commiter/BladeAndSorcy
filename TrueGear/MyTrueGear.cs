using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using TrueGearSDK;
using ThunderRoad;
using System.Linq;
using TrueGear;


namespace MyTrueGear
{
    public class TrueGearMod : ThunderScript
    {
        private static TrueGearPlayer _player = null;

        private static ManualResetEvent heartbeatMRE = new ManualResetEvent(false);
        private static ManualResetEvent fastheartbeatMRE = new ManualResetEvent(false);


       

        public void HeartBeat()
        {
            while (true)
            {
                heartbeatMRE.WaitOne();
                _player.SendPlay("HeartBeat");
                Thread.Sleep(1000);
            }
            
        }

        public void FastHeartBeat()
        {
            while (true)
            {
                fastheartbeatMRE.WaitOne();
                _player.SendPlay("FastHeartBeat");
                Thread.Sleep(600);
            }            
        }

        public void Start() 
        {
            //_player = new TrueGearPlayer();
            //RegisterFilesFromDisk();
            _player = new TrueGearPlayer("629730", "Blade And Sorcery");

            _player.PreSeekEffect("BluntStoneLargeDamage");
            _player.PreSeekEffect("MeleeBladeMetalPierce");
            _player.PreSeekEffect("MeleeBladeWoodPierce");
            _player.PreSeekEffect("MeleeBladeFleshPierce");
            _player.PreSeekEffect("MeleeBladeFabricPierce");
            _player.PreSeekEffect("MeleeBladeStonePierce");
            _player.PreSeekEffect("MeleeWoodMetalBlunt");
            _player.PreSeekEffect("MeleeWoodWoodBlunt");
            _player.PreSeekEffect("MeleeWoodFleshBlunt");
            _player.PreSeekEffect("MeleeWoodFabricBlunt");
            _player.PreSeekEffect("MeleeWoodStoneBlunt");
            _player.PreSeekEffect("PunchMetal");
            _player.PreSeekEffect("PunchWood");
            _player.PreSeekEffect("PunchFlesh");
            _player.PreSeekEffect("PunchFabric");
            _player.PreSeekEffect("PunchStone");
            _player.PreSeekEffect("PunchOther");
            _player.PreSeekEffect("MeleeBladeMetalBlunt");
            _player.PreSeekEffect("MeleeStoneFleshBlunt");
            _player.PreSeekEffect("MeleeStoneFabricBlunt");
            _player.PreSeekEffect("MeleeStoneStoneBlunt");
            _player.PreSeekEffect("MeleeBladeMetalSlash");
            _player.PreSeekEffect("MeleeBladeWoodSlash");
            _player.PreSeekEffect("MeleeBladeFleshSlash");
            _player.PreSeekEffect("MeleeBladeFabricSlash");
            _player.PreSeekEffect("MeleeBladeStoneSlash");
            _player.PreSeekEffect("LightningDamage");
            _player.PreSeekEffect("DefaultDamage");
            _player.PreSeekEffect("FireDamage");
            _player.Start();
            new Thread(new ThreadStart(this.HeartBeat)).Start();
            new Thread(new ThreadStart(this.FastHeartBeat)).Start();
        }

        //private void RegisterFilesFromDisk()
        //{
        //    FileInfo[] files = new DirectoryInfo(".//BladeAndSorcery_Data//StreamingAssets//Mods//BNS_TrueGear//TrueGear").GetFiles("*.asset_json", SearchOption.AllDirectories);

        //    for (int i = 0; i < files.Length; i++)
        //    {
        //        string name = files[i].Name;
        //        string fullName = files[i].FullName;
        //        if (name == "." || name == "..")
        //        {
        //            continue;
        //        }
        //        string jsonStr = File.ReadAllText(fullName);
        //        JSONNode jSONNode = JSON.Parse(jsonStr);
        //        EffectObject _curAssetObj = EffectObject.ToObject(jSONNode.AsObject);
        //        string uuidName = Path.GetFileNameWithoutExtension(fullName);
        //        _curAssetObj.uuid = uuidName;
        //        _curAssetObj.name = uuidName;
        //        _player.SetupRegister(uuidName, jsonStr);
        //    }
        //}

        public void Play(string Event)
        {
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine(Event);
            _player.SendPlay(Event);
        }

        //public void PlayAngle(string tmpEvent, float tmpAngle, float tmpVertical)
        //{
        //    try
        //    {
        //        int num = (int)(((tmpAngle - 22.5f > 0f) ? (tmpAngle - 22.5f) : (360f - tmpAngle)) / 45f) + 1;
        //        int num2 = (tmpVertical > 0.1f) ? -4 : ((tmpVertical < 0f) ? 8 : 0);
        //        EffectObject effectObject = EffectObject.ToObject(JSON.Parse(File.ReadAllText("BladeAndSorcery_Data\\StreamingAssets\\Mods\\BNS_TrueGear\\TrueGear\\" + tmpEvent + ".asset_json")).AsObject);
        //        foreach (TrackObject trackObject in effectObject.trackList)
        //        {
        //            if (trackObject.action_type == ActionType.Shake)
        //            {
        //                for (int j = 0; j < trackObject.index.Length; j++)
        //                {
        //                    if (num2 != 0)
        //                    {
        //                        trackObject.index[j] += num2;
        //                    }
        //                    if (num < 8)
        //                    {
        //                        if (trackObject.index[j] < 50)
        //                        {
        //                            int num3 = trackObject.index[j] % 4;
        //                            if (num <= num3)
        //                            {
        //                                trackObject.index[j] = trackObject.index[j] - num;
        //                            }
        //                            else if (num <= num3 + 4)
        //                            {
        //                                int num4 = num - num3;
        //                                trackObject.index[j] = trackObject.index[j] - num3 + 99 + num4;
        //                            }
        //                            else
        //                            {
        //                                trackObject.index[j] = trackObject.index[j] + 1;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            int num5 = 3 - trackObject.index[j] % 4;
        //                            if (num <= num5)
        //                            {
        //                                trackObject.index[j] = trackObject.index[j] + num;
        //                            }
        //                            else if (num <= num5 + 4)
        //                            {
        //                                int num6 = num - num5;
        //                                trackObject.index[j] = trackObject.index[j] + num5 - 99 - num6;
        //                            }
        //                            else
        //                            {
        //                                trackObject.index[j] = trackObject.index[j] - 1;
        //                            }
        //                        }
        //                    }
        //                }
        //                if (trackObject.index != null)
        //                {
        //                    trackObject.index = (from i in trackObject.index
        //                                         where i >= 0 && (i <= 19 || i >= 100) && i <= 119
        //                                         select i).ToArray<int>();
        //                }
        //            }
        //            else if (trackObject.action_type == ActionType.Electrical)
        //            {
        //                for (int k = 0; k < trackObject.index.Length; k++)
        //                {
        //                    if (num <= 4)
        //                    {
        //                        trackObject.index[k] = 0;
        //                    }
        //                    else
        //                    {
        //                        trackObject.index[k] = 100;
        //                    }
        //                    if (num == 1 || num == 8 || num == 4 || num == 5)
        //                    {
        //                        trackObject.index = new int[]
        //                        {
        //                    0,
        //                    100
        //                        };
        //                    }
        //                }
        //            }
        //        }
        //        TrueGearMod._player.SendPlayNoRegistered(effectObject.ToJsonObject().ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("TrueGear Mod PlayAngle Error :" + ex.Message);
        //        TrueGearMod._player.SendPlay(tmpEvent);
        //    }
        //}

        public void PlayAngle(string tmpEvent, float tmpAngle, float tmpVertical)
        {
            try
            {
                float angle = (tmpAngle - 22.5f) > 0f ? tmpAngle - 22.5f : 360f - tmpAngle;
                int horCount = (int)(angle / 45) + 1;

                int verCount = tmpVertical > 0.1f ? -4 : tmpVertical < -0.5f ? 8 : 0;


                EffectObject oriObject = _player.FindEffectByUuid(tmpEvent);

                EffectObject rootObject = EffectObject.Copy(oriObject);


                foreach (TrackObject track in rootObject.trackList)
                {
                    if (track.action_type == ActionType.Shake)
                    {
                        for (int i = 0; i < track.index.Length; i++)
                        {
                            if (verCount != 0)
                            {
                                track.index[i] += verCount;
                            }
                            if (horCount < 8)
                            {
                                if (track.index[i] < 50)
                                {
                                    int remainder = track.index[i] % 4;
                                    if (horCount <= remainder)
                                    {
                                        track.index[i] = track.index[i] - horCount;
                                    }
                                    else if (horCount <= (remainder + 4))
                                    {
                                        var num1 = horCount - remainder;
                                        track.index[i] = track.index[i] - remainder + 99 + num1;
                                    }
                                    else
                                    {
                                        track.index[i] = track.index[i] + 2;
                                    }
                                }
                                else
                                {
                                    int remainder = 3 - (track.index[i] % 4);
                                    if (horCount <= remainder)
                                    {
                                        track.index[i] = track.index[i] + horCount;
                                    }
                                    else if (horCount <= (remainder + 4))
                                    {
                                        var num1 = horCount - remainder;
                                        track.index[i] = track.index[i] + remainder - 99 - num1;
                                    }
                                    else
                                    {
                                        track.index[i] = track.index[i] - 2;
                                    }
                                }
                            }
                        }
                        if (track.index != null)
                        {
                            track.index = track.index.Where(i => !(i < 0 || (i > 19 && i < 100) || i > 119)).ToArray();
                        }
                    }
                    else if (track.action_type == ActionType.Electrical)
                    {
                        for (int i = 0; i < track.index.Length; i++)
                        {
                            if (horCount <= 4)
                            {
                                track.index[i] = 0;
                            }
                            else
                            {
                                track.index[i] = 100;
                            }
                            if (horCount == 1 || horCount == 8 || horCount == 4 || horCount == 5)
                            {
                                track.index = new int[2] { 0, 100 };
                            }

                        }
                    }
                }
                _player.SendPlayEffectByContent(rootObject);

            }
            catch (System.Exception ex)
            {
                Console.WriteLine("TrueGear Mod PlayAngle Error :" + ex.Message);
                _player.SendPlay(tmpEvent);
            }
        }

        public void PlayRandom(string tmpEvent, int[,] tmpEletricals, int[] tmpRandomCounts)
        {
            try
            {
                Random random = new Random();
                int num = 0;
                foreach (int num2 in tmpRandomCounts)
                {
                    num += num2;
                }
                //EffectObject rootObject = EffectObject.ToObject(JSON.Parse(File.ReadAllText("BladeAndSorcery_Data\\StreamingAssets\\Mods\\BNS_TrueGear\\TrueGear\\" + tmpEvent + ".asset_json")).AsObject);

                EffectObject oriObject = _player.FindEffectByUuid(tmpEvent);

                EffectObject rootObject = EffectObject.Copy(oriObject);

                int length = tmpEletricals.GetLength(1);
                foreach (TrackObject trackObject in rootObject.trackList)
                {
                    if (trackObject.action_type == ActionType.Shake)
                    {
                        if (trackObject.index.Length < num)
                        {
                            trackObject.index = new int[num];
                        }
                        int num3 = 0;
                        for (int j = 0; j < trackObject.index.Length; j++)
                        {
                            if (tmpRandomCounts[num3] > 1)
                            {
                                int num4 = random.Next(length);
                                while (tmpEletricals[num3, num4] == -1)
                                {
                                    num4 = random.Next(length);
                                }
                                trackObject.index[j] = tmpEletricals[num3, num4];
                                tmpEletricals[num3, num4] = -1;
                                tmpRandomCounts[num3]--;
                            }
                            else
                            {
                                int num5 = random.Next(length);
                                while (tmpEletricals[num3, num5] == -1)
                                {
                                    num5 = random.Next(length);
                                }
                                trackObject.index[j] = tmpEletricals[num3, num5];
                                tmpEletricals[num3, num5] = -1;
                                tmpRandomCounts[num3]--;
                                num3++;
                            }
                        }
                    }
                    else if (trackObject.action_type == ActionType.Electrical)
                    {
                        for (int k = 0; k < trackObject.index.Length; k++)
                        {
                            int num6 = random.Next(0, 2);
                            if (num6 == 0)
                            {
                                trackObject.index[k] = num6;
                            }
                            else
                            {
                                trackObject.index[k] = 100;
                            }
                        }
                    }
                }
                //TrueGearMod._player.SendPlayNoRegistered(rootObject.ToJsonObject().ToString());
                _player.SendPlayEffectByContent(rootObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("TrueGear Mod PlayRandom Error :" + ex.Message);
                TrueGearMod._player.SendPlay(tmpEvent);
            }
        }

        public void StartHeartBeat()
        {
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("StartHeartBeat");
            heartbeatMRE.Set();
        }

        public void StopHeartBeat()
        {
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("StopHeartBeat");
            heartbeatMRE.Reset();
        }

        public void StartFastHeartBeat()
        {
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("StartFastHeartBeat");
            fastheartbeatMRE.Set();
        }

        public void StopFastHeartBeat()
        {
            Console.WriteLine("------------------------------------------------------------------------------------------");
            Console.WriteLine("StopFastHeartBeat");
            fastheartbeatMRE.Reset();
        }

    }
}
