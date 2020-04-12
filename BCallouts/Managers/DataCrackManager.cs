using RAGENativeUI.Elements;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Rage;
using System.Security.Cryptography;
using System;
using BCallouts.Common;

namespace BCallouts.Managers
{
    public static class DataCrackManager
    {
        private static bool minigameOn;

        public static bool GameWon { get; private set; }

        private static Sprite[] bg_sprites;
        private static Sprite[] bars_sprites;
        private static Sprite[] hl_bars_sprites;
        private static int[] bars_speed;
        private static int current;
        private static bool pinCentered;
        private static int soundId;

        private static GameFiber pFiber;
        private static GameFiber gFiber;

        public static void Initialize()
        {
            Size bg_size = Size.Empty;
            bg_size.Height = 1080;
            bg_size.Width = 1920;
            Size fen_size = Size.Empty;
            fen_size.Height = 1024;
            fen_size.Width = 1024;
            Point fen_point = Point.Empty;
            fen_point.X = 448;
            bg_sprites = new Sprite[2];
            bg_sprites[0] = new Sprite("hacking_pc_desktop_0", "hacking_pc_desktop_0", Point.Empty, bg_size);
            bg_sprites[0].LoadTextureDictionary();
            bg_sprites[1] = new Sprite("hackingNG", "DHMain", fen_point, fen_size);
            bg_sprites[1].LoadTextureDictionary();
            bars_sprites = new Sprite[8];
            hl_bars_sprites = new Sprite[8];
            bars_speed = new int[8];
            for (int i = 0; i < 8; i++)
            {
                Size bar_size = Size.Empty;
                bar_size.Height = 305;
                bar_size.Width = 512;
                Point bar_point = Point.Empty;
                bar_point.X = 532 + i*48;
                bar_point.Y = 567;
                bars_sprites[i] = new Sprite("hackingNG", "DHComp", bar_point, bar_size);
                hl_bars_sprites[i] = new Sprite("hackingNG", "DHCompHi", bar_point, bar_size);
            }
            Game.FrameRender += Render;
            Process();
        }

        private static void Render(object sender, GraphicsEventArgs e)
        {
            if (minigameOn)
            {
                //bg_sprites[0].Draw();
                bg_sprites[1].Draw();
                for (int i = 0; i < 8; i++)
                {
                    if(i == current)
                    {
                        hl_bars_sprites[i].Draw();
                    } 
                    else
                    {
                        bars_sprites[i].Draw();
                    }
                }
            }
        }

        private static void GameProcess()
        {
            gFiber = GameFiber.StartNew(delegate
            {
                int t = 0;
                while (true)
                {
                    
                    for (int i = 0; i < 8; i++)
                    {
                        if(current <= i)
                        {
                            bars_sprites[i].Position.Y = (int)(417 + 150 * Math.Cos(t * Math.PI / bars_speed[i]));
                            hl_bars_sprites[i].Position.Y = (int)(417 + 150 * Math.Cos(t * Math.PI / bars_speed[i]));
                        }
                        else
                        {
                            bars_sprites[i].Position.Y = 417;
                        }
                    }

                    if(current < 8 && 425 > bars_sprites[current].Position.Y && bars_sprites[current].Position.Y > 409)
                    {
                        if(!pinCentered)
                        {
                            pinCentered = true;
                            Natives.PlaySoundFrontend(-1, "Pin_Centred", "DLC_HEIST_BIOLAB_PREP_HACKING_SOUNDS");
                        }
                    } 
                    else
                    {
                        if(pinCentered)
                        {
                            pinCentered = false;
                        }
                    }
                    t++;
                    GameFiber.Wait(10);
                }
            });
        }

        private static void Process()
        {
            pFiber = GameFiber.StartNew(delegate
            {
                while (true)
                {

                    if (Game.IsControlJustPressed(0, GameControl.CellphoneSelect) && minigameOn)
                    {
                        if(pinCentered)
                        {
                            current++;
                            pinCentered = false;
                            if (current >= 8)
                            {
                                GameWon = true;
                                Natives.StopSound(soundId);
                                Natives.PlaySoundFrontend(-1, "Hack_Success", "DLC_HEIST_BIOLAB_PREP_HACKING_SOUNDS");
                                GameFiber.Wait(1000);
                                GameWon = false;
                                CloseMinigame();
                            } 
                            else
                            {
                                Natives.PlaySoundFrontend(-1, "Pin_Good", "DLC_HEIST_BIOLAB_PREP_HACKING_SOUNDS");
                            }
                        } 
                        else
                        {
                            Natives.PlaySoundFrontend(-1, "Pin_Bad", "DLC_HEIST_BIOLAB_PREP_HACKING_SOUNDS");
                            if (current > 0)
                            {
                                current--;
                            }
                        }
                    }
                    GameFiber.Yield();
                }
            });
        }

        public static void Finally()
        {
            minigameOn = false;
            if (pFiber != null) { pFiber.Abort(); }
            if (gFiber != null) { gFiber.Abort(); }
        }

        public static void OpenMinigame()
        {
            soundId = Natives.GetSoundId();
            Natives.PlaySoundFrontend(soundId, "Pin_Movement", "DLC_HEIST_BIOLAB_PREP_HACKING_SOUNDS");
            minigameOn = true;
            GameWon = false;
            pinCentered = false;
            current = 0;
            Random rdm = new Random(DateTime.UtcNow.Millisecond);
            for (int i = 0; i < 8; i++)
            {
                bars_speed[i] = rdm.Next(100, 151);
            }
            GameProcess();
        }

        public static void CloseMinigame()
        {
            if(minigameOn)
            {
                Natives.StopSound(soundId); ;
            }
            minigameOn = false;
            if (gFiber != null) { gFiber.Abort(); }
        }
    }
}
