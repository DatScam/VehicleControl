/* This Mod Is Made By Toxic Scams, Please Don't Republish It Without Premission
 * And Give Me Some Credits, You Can Use Modify It For Personal Use. Have Fun @06/07/2019 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using System.Drawing;
using GTA.Native;
using System.IO;
using System.Threading;
using NativeUI;
using OpenVC;

namespace OpenUtils
{
    public class main : Script
    {
        /* Engine Data */
        private bool EngineLoaded = false;
        private ScriptSettings _config;
        private MenuPool _menuPool;
        private UIMenu mainMenu;

        /*Default Engine Keys*/
        private Keys ExitCarEngineOn_KEY = Keys.G;
        private Keys ControlCarEngine_KEY = Keys.T;
        private Keys ControlCarHazards_KEY = Keys.K;
        private Keys ControlCarLeftIndicator_KEY = Keys.J;
        private Keys ControlCarRightIndicator_KEY = Keys.L;
        private Keys ControlCarInteriorLight_KEY = Keys.I;
        private Keys ControlCarHood_KEY = Keys.Y;
        private Keys ControlCarTrunk_KEY = Keys.U;
        private Keys ControlCarDoors_KEY = Keys.X;
        private Keys ControlCarWindows_KEY = Keys.Z;
        private Keys ControlCarParkingBrake_KEY = Keys.Oemcomma;

        private int ControlCarEngine_CONTROL = 201;
        private int ExitCarEngineOn_CONTROL = 204;
        private int ControlMenuKey1_CONTROL = 201;
        private int ControlMenuKey2_CONTROL = 205;

        private Keys RemoteMenu_KEY = Keys.B;

        /*Game Public Data*/
        bool ExitCarEngineOnHold = false;
        bool EnableBreakingLightsOnStop = true;
        bool HazardLightsEnabled = false;
        bool LeftLightSignalEnabled = false;
        bool RightLightSignalEnabled = false;
        bool InteriorLightEnabled = false;
        bool HoodOpened = false;
        bool TrunkOpened = false;

        int ExitCarKeyHeld = 0;

        public static int MaxManagedVehicles = 5;

        public static int CalledVehicleDrivingStyle = 1;

        public static int CalledVehicleSpeed = 50;

        public static bool ChangePreviousWindowStateOnUpdate = false;

        /* Server Data */
        public static string SERVER_FILE = "";
        public static string SERVER_ADDRESS = "";
        public static int SERVER_PORT = 7777;
        public static long SERVER_SPACE = 100;
        public object _server;

        /*Game Local Data*/
        bool VehicleEngineRunning = false;
        bool IsLeavingVehicle = false;
        Vehicle playerVehicle;
        Vehicle playerLastVehicle;

        public main()
        {
            _menuPool = new MenuPool();   
            mainMenu = new UIMenu("~g~Vehicle Control", "~b~By Toxic Scams ~r~V1.2.1");   
            _menuPool.Add(mainMenu);        

            Tick += GameLoop;
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
            Interval = 1;
        }


        private void GameLoop(object sender, EventArgs e)
        {

            _menuPool.ProcessMenus();

            playerVehicle = Game.Player.Character.CurrentVehicle;
            playerLastVehicle = Game.Player.Character.LastVehicle;
            if (!EngineLoaded)
            {
                EngineLoaded = true;
                LoadConfigFile();
            }
            else 
            {
                Function.Call(Hash.SET_AUDIO_FLAG, "LoadMPData", true);
                foreach (Vehicle v in VehicleStore.calledVehicles.ToList())
                {
                    if (v.Driver.IsInVehicle() && v.Driver != Game.Player.Character && v.Driver.Position.DistanceTo(Game.Player.Character.Position) < 5.0)
                    {
                        v.Driver.Task.LeaveVehicle();
                        v.Driver.Delete();
                        v.CloseDoor(VehicleDoor.FrontRightDoor, false);
                        v.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                        v.CloseDoor(VehicleDoor.BackLeftDoor, false);
                        v.CloseDoor(VehicleDoor.BackRightDoor, false);
                        v.CloseDoor(VehicleDoor.Hood, false);
                        v.CloseDoor(VehicleDoor.Trunk, false);
                        v.OpenDoor(VehicleDoor.FrontLeftDoor, false, false);
                        v.Speed = 0.00f;
                        VehicleStore.RemoveVehicleToBeCalled(v);
                        if (!VehicleStore.WasVehicleCalled(v)) VehicleStore.AddVehicleWasCalled(v);
                        UI.Notify("~b~The Vehicle ~g~Arrived~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", v, "PI_Menu_Sounds", 1, 0);
                        break;
                    }
                    else
                    {
                        v.Driver.DrivingSpeed = CalledVehicleSpeed;
                    }
                }

                CheckForGamepadKeyPress();

                VehicleStore.UpdateVehiclesToBeHorny();
                VehicleStore.UpdateVehiclesToBeLeftOn();
                VehicleStore.UpdateVehiclesToBeParked();
                VehicleStore.UpdateVehiclesToBeManaged();

                if (playerVehicle != null)
                {
                    if (VehicleStore.WasVehicleCalled(playerVehicle))
                    {
                        VehicleStore.RemoveVehicleWasCalled(playerVehicle);
                        playerVehicle.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                        playerVehicle.CurrentBlip.Alpha = 0;
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", playerVehicle, "PI_Menu_Sounds", 1, 0);
                        VehicleEngineRunning = true;
                        if (VehicleStore.IsVehicleToBeLeftOn(playerVehicle)) VehicleStore.RemoveVehicleToBeLeftOn(playerVehicle);

                    }

                    VehicleWindows.UpdateWindowState(playerVehicle);

                    if (!VehicleStore.IsVehicleToBeManaged(playerVehicle)) VehicleStore.AddVehicleToBeManaged(playerVehicle);
                    else if (VehicleStore.managedVehicles[0] != playerVehicle) { VehicleStore.RemoveVehicleToBeManaged(playerVehicle); VehicleStore.AddVehicleToBeManaged(playerVehicle); }

                    if (EnableBreakingLightsOnStop && playerVehicle.Speed == 0.00f)
                    {
                        Function.Call(Hash.SET_VEHICLE_BRAKE_LIGHTS, playerVehicle, true);
                    }

                    if (VehicleStore.IsVehicleToBeLeftOn(playerVehicle))
                    {
                        VehicleEngineRunning = true;
                        if(IsLeavingVehicle == false) VehicleStore.RemoveVehicleToBeLeftOn(playerVehicle);
                    }
                    if (!VehicleEngineRunning && !VehicleStore.IsVehicleToBeLeftOn(playerVehicle))
                    {
                        playerVehicle.EngineRunning = false;
                    }
                }
                else
                {
                    VehicleEngineRunning = false;
                    IsLeavingVehicle = false;
                    ExitCarKeyHeld = 0;
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == ExitCarEngineOn_KEY && ExitCarEngineOnHold)
            {
                ExitCarKeyHeld++;
                if (playerVehicle != null && ExitCarKeyHeld >= 10)
                {
                    Game.Player.Character.Task.LeaveVehicle();
                    Wait(3);
                    IsLeavingVehicle = true;
                    playerLastVehicle.EngineRunning = true;
                    VehicleStore.AddVehicleToBeLeftOn(playerLastVehicle);
                    UI.Notify("~b~You Left The Car Engine ~g~On~b~!");
                }
            }
        }

        private void CheckForGamepadKeyPress()
        {
            if (Game.IsControlPressed(2, (GTA.Control) (ControlCarEngine_CONTROL)) && Game.IsControlJustReleased(2, (GTA.Control)(ControlMenuKey2_CONTROL)))
            {
                OnKeyUp(null, new KeyEventArgs(ControlCarEngine_KEY));
            }
            if(Game.IsControlPressed(2, (GTA.Control)(ControlMenuKey1_CONTROL)) && Game.IsControlJustReleased(2, (GTA.Control)(ControlMenuKey2_CONTROL)))
            {
                OnKeyUp(null, new KeyEventArgs(RemoteMenu_KEY));
            }
            if(Game.IsControlPressed(2, (GTA.Control)(ExitCarEngineOn_CONTROL)))
            {
                ExitCarKeyHeld++;
                if (playerVehicle != null && ExitCarKeyHeld >= 10)
                {
                    Game.Player.Character.Task.LeaveVehicle();
                    Wait(3);
                    IsLeavingVehicle = true;
                    playerLastVehicle.EngineRunning = true;
                    VehicleStore.AddVehicleToBeLeftOn(playerLastVehicle);
                    UI.Notify("~b~You Left The Car Engine ~g~On~b~!");
                }
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            Ped player = Game.Player.Character;
            if(e.KeyCode == ControlCarParkingBrake_KEY)
            {
                if(playerVehicle != null && (playerVehicle.Model.IsCar || playerVehicle.Model.IsBike)){
                    if (VehicleStore.IsVehicleToBeParked(playerVehicle))
                    {
                        VehicleStore.RemoveVehicleToBeParked(playerVehicle);
                        playerVehicle.HandbrakeOn = false;
                        UI.Notify("~b~You ~g~Released~b~ The Parking Brake!");
                    }
                    else
                    {
                        VehicleStore.AddVehicleToBeParked(playerVehicle);
                        playerVehicle.HandbrakeOn = true;
                        UI.Notify("~b~You ~r~Pulled~b~ The Parking Brake!");
                    }
                }
            }
            if (e.KeyCode == ExitCarEngineOn_KEY)
            {
                if (playerVehicle != null && !ExitCarEngineOnHold)
                {
                    player.Task.LeaveVehicle();
                    Wait(3);
                    IsLeavingVehicle = true;
                    playerLastVehicle.EngineRunning = true;
                    VehicleStore.AddVehicleToBeLeftOn(playerLastVehicle);
                    UI.Notify("~b~You Left The Car Engine ~g~On~b~!");
                }else if(playerVehicle != null && !VehicleStore.IsVehicleToBeLeftOn(playerVehicle))
                {
                    player.Task.LeaveVehicle();
                }
            }
            if (e.KeyCode == ControlCarEngine_KEY)
            {
                if (playerVehicle != null)
                {
                    VehicleEngineRunning = !playerVehicle.EngineRunning;
                    if (!VehicleEngineRunning)UI.Notify("~b~Engine ~r~OFF~b~!");
                    else UI.Notify("~b~Engine ~g~ON~b~!");
                }
            }
            if(e.KeyCode == ControlCarHazards_KEY)
            {
                if(playerVehicle != null)
                {
                    if(!HazardLightsEnabled)
                    {
                        SetIndicatorLights(playerVehicle, true, true);
                        UI.Notify("~b~Hazard Lights ~g~ON~b~!");
                        HazardLightsEnabled = true;
                    }
                    else
                    {
                        SetIndicatorLights(playerVehicle, false, false);
                        UI.Notify("~b~Hazard Lights ~r~OFF~b~!");
                        HazardLightsEnabled = false;
                    }
                }
            }
            if (e.KeyCode == ControlCarLeftIndicator_KEY)
            {
                if (playerVehicle != null)
                {
                    if (!LeftLightSignalEnabled)
                    {
                        SetIndicatorLights(playerVehicle, true, false);
                        UI.Notify("~b~Left Indicator ~g~ON~b~!");
                        LeftLightSignalEnabled = true;
                    }
                    else
                    {
                        SetIndicatorLights(playerVehicle, false, false);
                        UI.Notify("~b~Left Indicator ~r~OFF~b~!");
                        LeftLightSignalEnabled = false;
                    }
                }
            }
            if (e.KeyCode == ControlCarRightIndicator_KEY)
            {
                if (playerVehicle != null)
                {
                    if (!RightLightSignalEnabled)
                    {
                        SetIndicatorLights(playerVehicle, false, true);
                        UI.Notify("~b~Right Indicator ~g~ON~b~!");
                        RightLightSignalEnabled = true;
                    }
                    else
                    {
                        SetIndicatorLights(playerVehicle, false, false);
                        UI.Notify("~b~Right Indicator ~r~OFF~b~!");
                        RightLightSignalEnabled = false;
                    }
                }
            }
            if (e.KeyCode == ControlCarInteriorLight_KEY)
            {
                if (playerVehicle != null)
                {
                    if (!InteriorLightEnabled)
                    {
                        Function.Call(Hash.SET_VEHICLE_INTERIORLIGHT, playerVehicle, true);
                        UI.Notify("~b~Interior Light ~g~ON~b~!");
                        InteriorLightEnabled = true;
                    }
                    else
                    {
                        Function.Call(Hash.SET_VEHICLE_INTERIORLIGHT, playerVehicle, false);
                        UI.Notify("~b~Interior Light ~r~OFF~b~!");
                        InteriorLightEnabled = false;
                    }
                }
            }
            if (e.KeyCode == ControlCarHood_KEY)
            {
                if (playerVehicle != null)
                {
                    if (!HoodOpened)
                    {
                        playerVehicle.OpenDoor(VehicleDoor.Hood, false, false);
                        UI.Notify("~b~Hood ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", playerVehicle, "PI_Menu_Sounds", 1, 0);
                        HoodOpened = true;
                    }
                    else
                    {
                        playerVehicle.CloseDoor(VehicleDoor.Hood, false);
                        UI.Notify("~b~Hood ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", playerVehicle, "PI_Menu_Sounds", 1, 0);
                        HoodOpened = false;
                    }
                }
            }
            if (e.KeyCode == ControlCarTrunk_KEY)
            {
                if (playerVehicle != null)
                {
                    if (!TrunkOpened)
                    {
                        playerVehicle.OpenDoor(VehicleDoor.Trunk, false, false);
                        UI.Notify("~b~Trunk ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", playerVehicle, "PI_Menu_Sounds", 1, 0);
                        TrunkOpened = true;
                    }
                    else
                    {
                        playerVehicle.CloseDoor(VehicleDoor.Trunk, false);
                        UI.Notify("~b~Trunk ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", playerVehicle, "PI_Menu_Sounds", 1, 0);
                        TrunkOpened = false;
                    }
                }
            }
            if(e.KeyCode == ControlCarDoors_KEY)
            {
                if (playerVehicle != null)
                {
                    if (playerVehicle.IsDoorOpen(VehicleDoor.FrontRightDoor))
                    {
                        playerVehicle.CloseDoor(VehicleDoor.FrontRightDoor, false);
                        playerVehicle.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                        playerVehicle.CloseDoor(VehicleDoor.BackLeftDoor, false);
                        playerVehicle.CloseDoor(VehicleDoor.BackRightDoor, false);
                        playerVehicle.CloseDoor(VehicleDoor.Hood, false);
                        playerVehicle.CloseDoor(VehicleDoor.Trunk, false);
                        UI.Notify("~b~All Doors ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", playerVehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        playerVehicle.OpenDoor(VehicleDoor.FrontLeftDoor, false, false);
                        playerVehicle.OpenDoor(VehicleDoor.FrontRightDoor, false, false);
                        playerVehicle.OpenDoor(VehicleDoor.BackLeftDoor, false, false);
                        playerVehicle.OpenDoor(VehicleDoor.BackRightDoor, false, false);
                        playerVehicle.OpenDoor(VehicleDoor.Hood, false, false);
                        playerVehicle.OpenDoor(VehicleDoor.Trunk, false, false);
                        UI.Notify("~b~All Doors ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", playerVehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
            }
            if (e.KeyCode == ControlCarWindows_KEY)
            {
                if(playerVehicle != null)
                {
                    if (!VehicleWindows.All)
                    {
                        playerVehicle.RollDownWindow(VehicleWindow.FrontLeftWindow);
                        playerVehicle.RollDownWindow(VehicleWindow.FrontRightWindow);
                        playerVehicle.RollDownWindow(VehicleWindow.BackRightWindow);
                        playerVehicle.RollDownWindow(VehicleWindow.BackLeftWindow);
                        UI.Notify("~b~All Windows ~g~Opened~b~!");
                        VehicleWindows.All = true;
                        if (ChangePreviousWindowStateOnUpdate) VehicleWindows.ResetWindowsState();
                    }
                    else
                    {
                        playerVehicle.RollUpWindow(VehicleWindow.BackRightWindow);
                        playerVehicle.RollUpWindow(VehicleWindow.BackLeftWindow);
                        playerVehicle.RollUpWindow(VehicleWindow.FrontRightWindow);
                        playerVehicle.RollUpWindow(VehicleWindow.FrontLeftWindow);
                        UI.Notify("~b~All Windows ~r~Closed~b~!");
                        VehicleWindows.All = false;
                    }
                }
            }
            if (e.KeyCode == RemoteMenu_KEY && !_menuPool.IsAnyMenuOpen())
            {
                mainMenu.Clear();
                VehicleStore.UpdateVehiclesToBeManaged();
                int index = 0;
                foreach(Vehicle v in VehicleStore.managedVehicles.ToList())
                {
                    if (v.Exists())
                    {
                        if (index == 0)
                        {
                            index++;
                            MENU_Vehicles(mainMenu, "~g~" + v.DisplayName + "(" + "~b~" + v.NumberPlate + "~g~" + ")" + "~g~ *Current*", v);
                        }
                        else
                        {
                            MENU_Vehicles(mainMenu, "~g~" + v.DisplayName + "(" + "~b~" + v.NumberPlate + "~g~" + ")", v);
                        }
                    }else
                    {
                        VehicleStore.RemoveVehicleToBeManaged(v);
                    }
                }
                MENU_Configurations(mainMenu, "~b~Configurations");
                mainMenu.RefreshIndex();
                mainMenu.Visible = !mainMenu.Visible;
            }
        }

        private void LoadConfigFile()
        {

            if (!FoundDataDir(@"scripts\OpenVC\")) CreateDataDir(@"scripts\OpenVC\");
            if (!FoundDataFile(@"scripts\OpenVC\Config.ini")) {
                CreateDataFile(@"scripts\OpenVC\Config.ini");
                _config = ScriptSettings.Load(@"scripts\OpenVC\Config.ini");
                _config.SetValue<Keys>("[KEYS]", "ExitCarEngineOn_KEY", Keys.G);
                _config.SetValue<Keys>("[KEYS]", "ControlCarEngine_KEY", Keys.T);
                _config.SetValue<Keys>("[KEYS]", "ControlCarHazards_KEY", Keys.K);
                _config.SetValue<Keys>("[KEYS]", "ControlCarLeftIndicator_KEY", Keys.J);
                _config.SetValue<Keys>("[KEYS]", "ControlCarRightIndicator_KEY", Keys.L); 
                _config.SetValue<Keys>("[KEYS]", "ControlCarInteriorLight_KEY", Keys.I);
                _config.SetValue<Keys>("[KEYS]", "ControlCarHood_KEY", Keys.Y);
                _config.SetValue<Keys>("[KEYS]", "ControlCarTrunk_KEY", Keys.U);
                _config.SetValue<Keys>("[KEYS]", "ControlCarDoors_KEY", Keys.X);
                _config.SetValue<Keys>("[KEYS]", "ControlCarWindows_KEY", Keys.Z);
                _config.SetValue<Keys>("[KEYS]", "ControlCarParkingBrake_KEY", Keys.Oemcomma);

                _config.SetValue("[KEYS_CONTROLLER]", "ControlCarEngine_CONTROL", 201);
                _config.SetValue("[KEYS_CONTROLLER]", "ControlMenuKey1_CONTROL", 201);
                _config.SetValue("[KEYS_CONTROLLER]", "ControlMenuKey2_CONTROL", 205);
                _config.SetValue("[KEYS_CONTROLLER]", "ExitCarEngineOn_CONTROL", 204);

                _config.SetValue<Keys>("[MENU]", "RemoteMenu_KEY", Keys.B);

                _config.SetValue("[CONFIGS]", "CalledVehicleDrivingStyle", 1);
                _config.SetValue("[CONFIGS]", "CalledVehicleSpeed", 50);
                _config.SetValue("[CONFIGS]", "MaxManagedVehicles", 5);
                _config.SetValue("[CONFIGS]", "EnableBreakingLightsOnStop", true);
                _config.SetValue("[CONFIGS]", "ChangePreviousWindowStateOnUpdate", false);
                _config.SetValue("[CONFIGS]", "ExitCarEngineOnHold", false);
                _config.Save();
            }
            else
            {
                _config = ScriptSettings.Load(@"scripts\OpenVC\Config.ini");
                ExitCarEngineOn_KEY = _config.GetValue<Keys>("[KEYS]", "ExitCarEngineOn_KEY", Keys.G);
                ControlCarEngine_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarEngine_KEY", Keys.T);
                ControlCarHazards_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarHazards_KEY", Keys.K);
                ControlCarLeftIndicator_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarLeftIndicator_KEY", Keys.J);
                ControlCarRightIndicator_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarRightIndicator_KEY", Keys.L);
                ControlCarInteriorLight_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarInteriorLight_KEY", Keys.I);
                ControlCarHood_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarHood_KEY", Keys.Y);
                ControlCarTrunk_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarTrunk_KEY", Keys.U);
                ControlCarWindows_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarWindows_KEY", Keys.Z);
                ControlCarDoors_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarDoors_KEY", Keys.X);
                ControlCarParkingBrake_KEY = _config.GetValue<Keys>("[KEYS]", "ControlCarParkingBrake_KEY", Keys.Oemcomma);

                ControlCarEngine_CONTROL = _config.GetValue("[KEYS_CONTROLLER]", "ControlCarEngine_CONTROL", 201);
                ControlMenuKey1_CONTROL = _config.GetValue("[KEYS_CONTROLLER]", "ControlMenuKey1_CONTROL", 201);
                ControlMenuKey2_CONTROL = _config.GetValue("[KEYS_CONTROLLER]", "ControlMenuKey2_CONTROL", 205);
                ExitCarEngineOn_CONTROL = _config.GetValue("[KEYS_CONTROLLER]", "ExitCarEngineOn_CONTROL", 204);

                RemoteMenu_KEY = _config.GetValue<Keys>("[MENU]", "RemoteMenu_KEY", Keys.B);

                CalledVehicleDrivingStyle = _config.GetValue("[CONFIGS]", "CalledVehicleDrivingStyle", 1);
                EnableBreakingLightsOnStop = _config.GetValue("[CONFIGS]", "EnableBreakingLightsOnStop", true);
                ExitCarEngineOnHold = _config.GetValue("[CONFIGS]", "ExitCarEngineOnHold", false);
                MaxManagedVehicles = _config.GetValue("[CONFIGS]", "MaxManagedVehicles", 5);
                CalledVehicleSpeed = _config.GetValue("[CONFIGS]", "CalledVehicleSpeed", 50);
                ChangePreviousWindowStateOnUpdate = _config.GetValue("[CONFIGS]", "ChangePreviousWindowStateOnUpdate", false);
                _config.Save();
                UpdateConfigForNewUsers();
            }
        }

        public void UpdateConfigForNewUsers()
        {
            _config = ScriptSettings.Load(@"scripts\OpenVC\Config.ini");
            string[] AllData = File.ReadAllLines(@"scripts\OpenVC\Config.ini");
            bool engineHold = false;

            foreach(string s in AllData)
            {
                if (s.Contains("EXITCARENGINEONHOLD")) engineHold = true;
            }

            if (!engineHold)
            {
                _config.SetValue("[CONFIGS]", "ExitCarEngineOnHold", ExitCarEngineOnHold);
            }
            if(_config.GetValue<Keys>("[KEYS]", "ControlCarParkingBrake_KEY", Keys.A) == Keys.A)
            {
                _config.SetValue<Keys>("[KEYS]", "ControlCarParkingBrake_KEY", Keys.Oemcomma);
            }
            if (_config.GetValue("[KEYS_CONTROLLER]", "ExitCarEngineOn_CONTROL", -1) == -1)
            {
                _config.SetValue("[KEYS_CONTROLLER]", "ExitCarEngineOn_CONTROL", 204);
            }
            _config.Save();
        }

        public void SetIndicatorLights(Vehicle veh, bool leftIndicatorState, bool rightIndicatorState)
        {
            Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, veh, true, leftIndicatorState);
            Function.Call(Hash.SET_VEHICLE_INDICATOR_LIGHTS, veh, false, rightIndicatorState);
        }

        public static bool FoundDataFile(string file)
        {
            bool result = true;
            if (!File.Exists(file)) result = false;

            return result;
        }

        public static void CreateDataFile(string file)
        {
            if (FoundDataFile(file)) return;
            File.Create(file).Dispose();
        }

        public static bool FoundDataDir(string dir)
        {
            bool result = true;
            if (!Directory.Exists(dir)) result = false;
            return result;
        }

        public static void CreateDataDir(string dir)
        {
            if (FoundDataDir(dir)) return;
            Directory.CreateDirectory(dir);
        }

        /* Menu Functions */

        public void MENU_Configurations(UIMenu menu, string menuName)
        {
            var sub1 = _menuPool.AddSubMenu(menu, menuName);

            var checkbox = new UIMenuCheckboxItem("~b~Breaking Light On Vehicle Stop", EnableBreakingLightsOnStop, "~b~Turn ~g~On~b~/~r~Off~b~ The Breaking Light On Vehicle Stop.");
            sub1.AddItem(checkbox);
            checkbox.Enabled = true;

            var checkbox3 = new UIMenuCheckboxItem("~b~Exit Vehicle Engine On Hold", ExitCarEngineOnHold, "~b~Turn ~g~On~b~/~r~Off~b~ The Need Of Holding The Exit Vehicle Engine On Key.");
            sub1.AddItem(checkbox3);
            checkbox3.Enabled = true;

            var checkbox2 = new UIMenuCheckboxItem("~b~Change Windows State On Update", ChangePreviousWindowStateOnUpdate, "~b~Turn ~g~On~b~/~r~Off~b~ The Change Of Previous Windows State When They Update.");
            sub1.AddItem(checkbox2);
            checkbox2.Enabled = true;

            var button1 = new UIMenuItem("~b~Max Managed Vehicles = " + "~w~" + MaxManagedVehicles, "~b~Change The Max Managed Vehicles Number.");
            button1.Enabled = true;
            sub1.AddItem(button1);

            var button2 = new UIMenuItem("~b~Called Vehicle Driving Style = " + "~w~" + CalledVehicleDrivingStyle, "~b~Change The Called Vehicle Driving Style, Check readme.txt To Find The Valid Styles.");
            button2.Enabled = true;
            sub1.AddItem(button2);

            var button3 = new UIMenuItem("~b~Called Vehicle Speed = " + "~w~" + CalledVehicleSpeed, "~b~Change The Called Vehicle Speed.");
            button3.Enabled = true;
            sub1.AddItem(button3);

            var button = new UIMenuItem("~b~Reload Configurations", "~g~Reloads ~b~ The Config File.");
            button.Enabled = true;
            sub1.AddItem(button);

            sub1.RefreshIndex();

            sub1.OnItemSelect += (sender, item, index) =>
            {
                if (item == button)
                {
                    LoadConfigFile();
                    UI.Notify("~b~Config File Has Been Reloaded.");
                }
                if (item == button1)
                {
                    int num = MaxManagedVehicles;
                    Int32.TryParse(Game.GetUserInput(2), out num);
                    if (num < 1) num = 1;
                    MaxManagedVehicles = num;
                    _config = ScriptSettings.Load(@"scripts\OpenVC\Config.ini");
                    _config.SetValue("[CONFIGS]", "MaxManagedVehicles", MaxManagedVehicles);
                    _config.Save();
                    UI.Notify("~b~Max Managed Vehicles Has Been Modified To " + MaxManagedVehicles + ".");
                    button1.Text = "~b~Max Managed Vehicles = " + "~w~" + MaxManagedVehicles;
                }
                if (item == button2)
                {
                    int num = CalledVehicleDrivingStyle;
                    Int32.TryParse(Game.GetUserInput(1), out num);
                    if (num > 5) num = 5;
                    if (num < 0) num = 0;
                    CalledVehicleDrivingStyle = num;
                    _config = ScriptSettings.Load(@"scripts\OpenVC\Config.ini");
                    _config.SetValue("[CONFIGS]", "CalledVehicleDrivingStyle", CalledVehicleDrivingStyle);
                    _config.Save();
                    UI.Notify("~b~Called Vehicle Driving Style Has Been Modified To " + CalledVehicleDrivingStyle + ".");
                    button2.Text = "~b~Called Vehicle Driving Style = " + "~w~" + CalledVehicleDrivingStyle;
                }
                if (item == button3)
                {
                    int num = CalledVehicleSpeed;
                    Int32.TryParse(Game.GetUserInput(3), out num);
                    if (num > 200) num = 200;
                    if (num < 15) num = 15;
                    CalledVehicleSpeed = num;
                    _config = ScriptSettings.Load(@"scripts\OpenVC\Config.ini");
                    _config.SetValue("[CONFIGS]", "CalledVehicleSpeed", CalledVehicleSpeed);
                    _config.Save();
                    UI.Notify("~b~Called Vehicle Speed Has Been Modified To " + CalledVehicleSpeed + ".");
                    button3.Text = "~b~Called Vehicle Speed = " + "~w~" + CalledVehicleSpeed;
                }

            };

            sub1.OnCheckboxChange += (sender, item, checked_) =>
            {
                if(item == checkbox)
                {
                    _config = ScriptSettings.Load(@"scripts\OpenVC\Config.ini");
                    _config.SetValue("[CONFIGS]", "EnableBreakingLightsOnStop", checked_);
                    EnableBreakingLightsOnStop = checked_;
                    _config.Save();
                    UI.Notify("~b~Car Breaking On Stop Has Been Modified To " + EnableBreakingLightsOnStop + ".");
                }
                if(item == checkbox2)
                {
                    _config = ScriptSettings.Load(@"scripts\OpenVC\Config.ini");
                    _config.SetValue("[CONFIGS]", "ChangePreviousWindowStateOnUpdate", checked_);
                    ChangePreviousWindowStateOnUpdate = checked_;
                    _config.Save();
                    UI.Notify("~b~Change Previous Window State On Update Has Been Modified To " + ChangePreviousWindowStateOnUpdate + ".");
                }
                if (item == checkbox3)
                {
                    _config = ScriptSettings.Load(@"scripts\OpenVC\Config.ini");
                    _config.SetValue("[CONFIGS]", "ExitCarEngineOnHold", checked_);
                    ExitCarEngineOnHold = checked_;
                    _config.Save();
                    UI.Notify("~b~Exit Vehicle Engine On Hold Has Been Modified To " + ExitCarEngineOnHold + ".");
                }
            };

        }

        public void MENU_Vehicles(UIMenu menu, string menuName, Vehicle vehicle)
        {
            var sub1 = _menuPool.AddSubMenu(menu, menuName);

            Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Fob", Game.Player.Character, "PI_Menu_Sounds", 1, 0);

            var engineCheckbox = new UIMenuCheckboxItem("~b~Engine", vehicle.EngineRunning, "~b~Turn ~g~On~b~/~r~Off~b~ The Vehicle Engine.");
            sub1.AddItem(engineCheckbox);
            engineCheckbox.Enabled = true;

            var button = new UIMenuItem("~b~Call Vehicle", "~b~Call The Vehicle To Your Position.");
            button.Enabled = true;

            var button2 = new UIMenuItem("~r~Remove Vehicle", "~r~Remove Vehicle From The Managed Vehicles.");
            button2.Enabled = true;
            int attempts = 0;

            sub1.OnItemSelect += (sender, item, index) =>
            {
                if(item == button2)
                {
                    if (VehicleStore.IsVehicleToBeManaged(vehicle) && attempts > 0)
                    {
                        attempts = 0;
                        if (VehicleStore.IsVehicleToBeCalled(vehicle)) VehicleStore.RemoveVehicleWasCalled(vehicle);
                        if (VehicleStore.IsVehicleToBeHorny(vehicle)) VehicleStore.RemoveVehicleToBeHorny(vehicle);
                        if (VehicleStore.IsVehicleToBeLeftOn(vehicle)) VehicleStore.RemoveVehicleToBeLeftOn(vehicle);
                        if (VehicleStore.IsVehicleToBeParked(vehicle)) VehicleStore.RemoveVehicleToBeParked(vehicle);
                        VehicleStore.RemoveVehicleToBeManaged(vehicle);
                        UI.Notify("~r~Vehicle Has Been Removed!");
                    }
                    if (attempts == 0)
                    {
                        attempts++;
                        UI.Notify("~r~Are You Sure You Want To Remove This Vehicle?");
                    }
                }
                if (item == button)
                {
                    attempts = 0;
                    if (playerVehicle == null)
                    {
                        bool exsistb = Function.Call<bool>(Hash.DOES_BLIP_EXIST, vehicle.CurrentBlip);
                        if (exsistb)
                        {
                            vehicle.CurrentBlip.Alpha = 255;
                            if (vehicle.Model.IsBike || vehicle.Model.IsBicycle) { if (vehicle.CurrentBlip.Sprite != BlipSprite.PersonalVehicleBike) vehicle.CurrentBlip.Sprite = BlipSprite.PersonalVehicleBike; }
                            else if (vehicle.CurrentBlip.Sprite != BlipSprite.PersonalVehicleCar) vehicle.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                        }
                        else
                        {
                            vehicle.AddBlip();
                            if (vehicle.Model.IsBike || vehicle.Model.IsBicycle) { if (vehicle.CurrentBlip.Sprite != BlipSprite.PersonalVehicleBike) vehicle.CurrentBlip.Sprite = BlipSprite.PersonalVehicleBike; }
                            else if (vehicle.CurrentBlip.Sprite != BlipSprite.PersonalVehicleCar) vehicle.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                            vehicle.CurrentBlip.Name = vehicle.FriendlyName;
                            vehicle.CurrentBlip.Color = BlipColor.Red;
                            vehicle.CurrentBlip.Alpha = 255;
                        }
                        if (!VehicleStore.IsVehicleToBeCalled(vehicle))
                        {
                            VehicleStore.AddVehicleToBeCalled(vehicle);
                            Ped driver = vehicle.CreatePedOnSeat(VehicleSeat.Driver, PedHash.Autoshop01SMM);
                            vehicle.EngineRunning = true;
                            DriveVehicleTo(driver, vehicle, Game.Player.Character.Position, 5, CalledVehicleSpeed, (DrivingStyle)CalledVehicleDrivingStyle, false);
                            if (VehicleStore.IsVehicleToBeParked(vehicle)) VehicleStore.RemoveVehicleToBeParked(vehicle);
                            vehicle.HandbrakeOn = false;
                            vehicle.CloseDoor(VehicleDoor.FrontRightDoor, false);
                            vehicle.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                            vehicle.CloseDoor(VehicleDoor.BackLeftDoor, false);
                            vehicle.CloseDoor(VehicleDoor.BackRightDoor, false);
                            vehicle.CloseDoor(VehicleDoor.Hood, false);
                            vehicle.CloseDoor(VehicleDoor.Trunk, false);
                            UI.Notify("~b~The Vehicle ~r~Coming~b~!");
                        }
                        else
                        {
                            VehicleStore.RemoveVehicleToBeCalled(vehicle);
                            vehicle.Driver.Task.LeaveVehicle();
                            vehicle.Driver.Delete();
                            if (VehicleStore.IsVehicleToBeParked(vehicle)) VehicleStore.RemoveVehicleToBeParked(vehicle);
                            vehicle.HandbrakeOn = false;
                            vehicle.CloseDoor(VehicleDoor.FrontRightDoor, false);
                            vehicle.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                            vehicle.CloseDoor(VehicleDoor.BackLeftDoor, false);
                            vehicle.CloseDoor(VehicleDoor.BackRightDoor, false);
                            vehicle.CloseDoor(VehicleDoor.Hood, false);
                            vehicle.CloseDoor(VehicleDoor.Trunk, false);
                            vehicle.OpenDoor(VehicleDoor.FrontLeftDoor, false, false);
                            vehicle.Position = World.GetNextPositionOnStreet(Game.Player.Character.Position);
                            vehicle.Speed = 0.00f;
                            if(!VehicleStore.WasVehicleCalled(vehicle)) VehicleStore.AddVehicleWasCalled(vehicle);
                            if (!VehicleStore.IsVehicleToBeLeftOn(vehicle)) VehicleStore.AddVehicleToBeLeftOn(vehicle);
                            Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                            UI.Notify("~b~The Vehicle ~g~Arrived~b~!");
                        }
                    }
                }

            };

            var lights = _menuPool.AddSubMenu(sub1, "~b~Lights");
            var lsub = new UIMenuCheckboxItem("~b~Low Light Beams", vehicle.LightsOn, "~g~On~b~/~r~Off~b~ The Vehicle Low Light Beams.");
            var lsub1 = new UIMenuCheckboxItem("~b~High Light Beams", vehicle.HighBeamsOn, "~g~On~b~/~r~Off~b~ The Vehicle High Light Beams.");
            if (!vehicle.LightsOn) lsub1.Enabled = false;
            lights.AddItem(lsub); lights.AddItem(lsub1);
            lights.OnCheckboxChange += (sender, item, checked_) =>
            {
                attempts = 0;
                if (item == lsub)
                {
                    if (checked_)
                    {
                        if (!vehicle.LightsOn)
                        {
                            vehicle.LightsOn = true;
                            lsub1.Enabled = true;
                            UI.Notify("~b~You Turned The Vehicle Low Light Beams ~g~On~b~!");
                        }
                    }else
                    {
                        if (vehicle.LightsOn)
                        {
                            vehicle.LightsOn = false;
                            lsub1.Checked = false;
                            lsub1.Enabled = false;
                            UI.Notify("~b~You Turned The Vehicle Low Light Beams ~r~Off~b~!");
                        }
                    }
                }
                if (item == lsub1)
                {
                    if (checked_)
                    {
                        if (!vehicle.HighBeamsOn)
                        {
                            vehicle.HighBeamsOn = true;
                            UI.Notify("~b~You Turned The Vehicle High Light Beams ~g~On~b~!");
                        }
                    }
                    else
                    {
                        if (vehicle.HighBeamsOn)
                        {
                            vehicle.HighBeamsOn = false;
                            UI.Notify("~b~You Turned The Vehicle High Light Beams ~r~Off~b~!");
                        }
                    }
                }
            };

            var nlights = _menuPool.AddSubMenu(lights, "~b~Neon Lights");
            var nsub = new UIMenuCheckboxItem("~b~Front Neon Lights", vehicle.IsNeonLightsOn(VehicleNeonLight.Front), "~g~On~b~/~r~Off~b~ The Vehicle Front Neon Lights.");
            var nsub1 = new UIMenuCheckboxItem("~b~Back Neon Lights", vehicle.IsNeonLightsOn(VehicleNeonLight.Back), "~g~On~b~/~r~Off~b~ The Vehicle Back Neon Lights.");
            var nsub2 = new UIMenuCheckboxItem("~b~Right Neon Lights", vehicle.IsNeonLightsOn(VehicleNeonLight.Right), "~g~On~b~/~r~Off~b~ The Vehicle Right Neon Lights.");
            var nsub3 = new UIMenuCheckboxItem("~b~Left Neon Lights", vehicle.IsNeonLightsOn(VehicleNeonLight.Left), "~g~On~b~/~r~Off~b~ The Vehicle Left Neon Lights.");
            bool neonState = false; if (vehicle.IsNeonLightsOn(VehicleNeonLight.Front) && vehicle.IsNeonLightsOn(VehicleNeonLight.Back) && vehicle.IsNeonLightsOn(VehicleNeonLight.Right) && vehicle.IsNeonLightsOn(VehicleNeonLight.Left)) neonState = true;
            var nsub4 = new UIMenuCheckboxItem("~b~All Neon Lights", neonState, "~g~On~b~/~r~Off~b~ The Vehicle All Neon Lights.");
            nlights.AddItem(nsub); nlights.AddItem(nsub1); nlights.AddItem(nsub2); nlights.AddItem(nsub3); nlights.AddItem(nsub4);
            lights.RefreshIndex();
            nlights.RefreshIndex();
            nlights.OnCheckboxChange += (sender, item, checked_) =>
            {
                attempts = 0;
                if (item == nsub)
                {
                    if (checked_)
                    {
                        if (!vehicle.IsNeonLightsOn(VehicleNeonLight.Front))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Front, true);
                            UI.Notify("~b~You Turned The Vehicle Front Neon Lights ~g~On~b~!");
                        }
                    }
                    else
                    {
                        if (vehicle.IsNeonLightsOn(VehicleNeonLight.Front))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Front, false);
                            nsub4.Checked = false;
                            UI.Notify("~b~You Turned The Vehicle Front Neon Lights ~r~Off~b~!");
                        }
                    }
                }
                if (item == nsub1)
                {
                    if (checked_)
                    {
                        if (!vehicle.IsNeonLightsOn(VehicleNeonLight.Back))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Back, true);
                            UI.Notify("~b~You Turned The Vehicle Back Neon Lights ~g~On~b~!");
                        }
                    }
                    else
                    {
                        if (vehicle.IsNeonLightsOn(VehicleNeonLight.Back))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Back, false);
                            nsub4.Checked = false;
                            UI.Notify("~b~You Turned The Vehicle Back Neon Lights ~r~Off~b~!");
                        }
                    }
                }
                if (item == nsub2)
                {
                    if (checked_)
                    {
                        if (!vehicle.IsNeonLightsOn(VehicleNeonLight.Right))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Right, true);
                            UI.Notify("~b~You Turned The Vehicle Right Neon Lights ~g~On~b~!");
                        }
                    }
                    else
                    {
                        if (vehicle.IsNeonLightsOn(VehicleNeonLight.Right))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Right, false);
                            nsub4.Checked = false;
                            UI.Notify("~b~You Turned The Vehicle Right Neon Lights ~r~Off~b~!");
                        }
                    }
                }
                if (item == nsub3)
                {
                    if (checked_)
                    {
                        if (!vehicle.IsNeonLightsOn(VehicleNeonLight.Left))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Left, true);
                            UI.Notify("~b~You Turned The Vehicle Left Neon Lights ~g~On~b~!");
                        }
                    }
                    else
                    {
                        if (vehicle.IsNeonLightsOn(VehicleNeonLight.Left))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Left, false);
                            nsub4.Checked = false;
                            UI.Notify("~b~You Turned The Vehicle Left Neon Lights ~r~Off~b~!");
                        }
                    }
                }
                if (item == nsub4)
                {
                    if (checked_)
                    {
                        if (!vehicle.IsNeonLightsOn(VehicleNeonLight.Front) && !vehicle.IsNeonLightsOn(VehicleNeonLight.Back) && !vehicle.IsNeonLightsOn(VehicleNeonLight.Right) && !vehicle.IsNeonLightsOn(VehicleNeonLight.Left))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Front, true);
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Back, true);
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Right, true);
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Left, true);
                            nsub.Checked = true; nsub1.Checked = true; nsub2.Checked = true; nsub3.Checked = true;
                            UI.Notify("~b~You Turned The Vehicle All Neon Lights ~g~On~b~!");
                        }
                    }
                    else
                    {
                        if (vehicle.IsNeonLightsOn(VehicleNeonLight.Front) && vehicle.IsNeonLightsOn(VehicleNeonLight.Back) && vehicle.IsNeonLightsOn(VehicleNeonLight.Right) && vehicle.IsNeonLightsOn(VehicleNeonLight.Left))
                        {
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Front, false);
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Back, false);
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Right, false);
                            vehicle.SetNeonLightsOn(VehicleNeonLight.Left, false);
                            nsub.Checked = false; nsub1.Checked = false; nsub2.Checked = false; nsub3.Checked = false;
                            UI.Notify("~b~You Turned The Vehicle All Neon Lights ~r~Off~b~!");
                        }
                    }
                }
            };

            var sub2 = _menuPool.AddSubMenu(sub1, "~b~Doors");
            var sub3 = _menuPool.AddSubMenu(sub1, "~b~Windows");
            var Dsubbtn = new UIMenuItem("~g~Open~b~/~r~Close~b~ Driver Door");
            var Dsubbtn2 = new UIMenuItem("~g~Open~b~/~r~Close~b~ Passenger Door");
            var Dsubbtn3 = new UIMenuItem("~g~Open~b~/~r~Close~b~ Back Seat Left Door");
            var Dsubbtn4 = new UIMenuItem("~g~Open~b~/~r~Close~b~ Back Seat Right Door");
            var Dsubbtn5 = new UIMenuItem("~g~Open~b~/~r~Close~b~ Vehicle Hood");
            var Dsubbtn6 = new UIMenuItem("~g~Open~b~/~r~Close~b~ Vehicle Trunk");
            var Dsubbtn7 = new UIMenuItem("~g~Open~b~/~r~Close~b~ All Vehicle Doors");
            sub2.AddItem(Dsubbtn); sub2.AddItem(Dsubbtn2); sub2.AddItem(Dsubbtn3); sub2.AddItem(Dsubbtn4); sub2.AddItem(Dsubbtn5); sub2.AddItem(Dsubbtn6);sub2.AddItem(Dsubbtn7);
            sub2.RefreshIndex();
            sub2.OnItemSelect += (sender, item, index) =>
            {
                attempts = 0;
                if (item == Dsubbtn)
                {
                    if(vehicle.IsDoorOpen(VehicleDoor.FrontLeftDoor))
                    {
                        vehicle.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                        UI.Notify("~b~Door ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        vehicle.OpenDoor(VehicleDoor.FrontLeftDoor, false, false);
                        UI.Notify("~b~Door ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
                if (item == Dsubbtn2)
                {
                    if (vehicle.IsDoorOpen(VehicleDoor.FrontRightDoor))
                    {
                        vehicle.CloseDoor(VehicleDoor.FrontRightDoor, false);
                        UI.Notify("~b~Door ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        vehicle.OpenDoor(VehicleDoor.FrontRightDoor, false, false);
                        UI.Notify("~b~Door ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
                if (item == Dsubbtn3)
                {
                    if (vehicle.IsDoorOpen(VehicleDoor.BackLeftDoor))
                    {
                        vehicle.CloseDoor(VehicleDoor.BackLeftDoor, false);
                        UI.Notify("~b~Door ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        vehicle.OpenDoor(VehicleDoor.BackLeftDoor, false, false);
                        UI.Notify("~b~Door ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
                if (item == Dsubbtn4)
                {
                    if (vehicle.IsDoorOpen(VehicleDoor.BackRightDoor))
                    {
                        vehicle.CloseDoor(VehicleDoor.BackRightDoor, false);
                        UI.Notify("~b~Door ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        vehicle.OpenDoor(VehicleDoor.BackRightDoor, false, false);
                        UI.Notify("~b~Door ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
                if (item == Dsubbtn5)
                {
                    if (vehicle.IsDoorOpen(VehicleDoor.Hood))
                    {
                        vehicle.CloseDoor(VehicleDoor.Hood, false);
                        UI.Notify("~b~Hood ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        vehicle.OpenDoor(VehicleDoor.Hood, false, false);
                        UI.Notify("~b~Hood ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
                if (item == Dsubbtn6)
                {
                    if (vehicle.IsDoorOpen(VehicleDoor.Trunk))
                    {
                        vehicle.CloseDoor(VehicleDoor.Trunk, false);
                        UI.Notify("~b~Trunk ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        vehicle.OpenDoor(VehicleDoor.Trunk, false, false);
                        UI.Notify("~b~Trunk ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
                if(item == Dsubbtn7)
                {
                    if (vehicle.IsDoorOpen(VehicleDoor.FrontRightDoor))
                    {
                        vehicle.CloseDoor(VehicleDoor.FrontRightDoor, false);
                        vehicle.CloseDoor(VehicleDoor.FrontLeftDoor, false);
                        vehicle.CloseDoor(VehicleDoor.BackLeftDoor, false);
                        vehicle.CloseDoor(VehicleDoor.BackRightDoor, false);
                        vehicle.CloseDoor(VehicleDoor.Hood, false);
                        vehicle.CloseDoor(VehicleDoor.Trunk, false);
                        UI.Notify("~b~All Doors ~r~Closed~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        vehicle.OpenDoor(VehicleDoor.FrontLeftDoor, false, false);
                        vehicle.OpenDoor(VehicleDoor.FrontRightDoor, false, false);
                        vehicle.OpenDoor(VehicleDoor.BackLeftDoor, false, false);
                        vehicle.OpenDoor(VehicleDoor.BackRightDoor, false, false);
                        vehicle.OpenDoor(VehicleDoor.Hood, false, false);
                        vehicle.OpenDoor(VehicleDoor.Trunk, false, false);
                        UI.Notify("~b~All Doors ~g~Opened~b~!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
            };

            var Wsubbbtn = new UIMenuItem("~g~Open~b~/~r~Close~b~ Driver Window");
            var Wsubbbtn2 = new UIMenuItem("~g~Open~b~/~r~Close~b~ Passenger Window");
            var Wsubbbtn3 = new UIMenuItem("~g~Open~b~/~r~Close~b~ Back Seat Left Window");
            var Wsubbbtn4 = new UIMenuItem("~g~Open~b~/~r~Close~b~ Back Seat Right Window");
            var Wsubbbtn5 = new UIMenuItem("~g~Open~b~/~r~Close~b~ All Vehicle Windows");
            sub3.AddItem(Wsubbbtn); sub3.AddItem(Wsubbbtn2); sub3.AddItem(Wsubbbtn3); sub3.AddItem(Wsubbbtn4); sub3.AddItem(Wsubbbtn5);
            sub3.RefreshIndex();
            sub3.OnItemSelect += (sender, item, index) =>
            {
                attempts = 0;
                if (item == Wsubbbtn)
                {
                    if (!VehicleWindows.FrontLeft)
                    {
                        vehicle.RollDownWindow(VehicleWindow.FrontLeftWindow);
                        UI.Notify("~b~Window ~g~Opened~b~!");
                        VehicleWindows.FrontLeft = true;
                    }
                    else
                    {
                        vehicle.RollUpWindow(VehicleWindow.FrontLeftWindow);
                        UI.Notify("~b~Window ~r~Closed~b~!");
                        VehicleWindows.FrontLeft = false;
                    }
                }
                if (item == Wsubbbtn2)
                {
                    if (!VehicleWindows.FrontRight)
                    {
                        vehicle.RollDownWindow(VehicleWindow.FrontRightWindow);
                        UI.Notify("~b~Window ~g~Opened~b~!");
                        VehicleWindows.FrontRight = true;
                    }
                    else
                    {
                        vehicle.RollUpWindow(VehicleWindow.FrontRightWindow);
                        UI.Notify("~b~Window ~r~Closed~b~!");
                        VehicleWindows.FrontRight = false;
                    }
                }
                if (item == Wsubbbtn3)
                {
                    if (!VehicleWindows.BackLeft)
                    {
                        vehicle.RollDownWindow(VehicleWindow.BackLeftWindow);
                        UI.Notify("~b~Window ~g~Opened~b~!");
                        VehicleWindows.BackLeft = true;
                    }
                    else
                    {
                        vehicle.RollUpWindow(VehicleWindow.BackLeftWindow);
                        UI.Notify("~b~Window ~r~Closed~b~!");
                        VehicleWindows.BackLeft = false;
                    }
                }
                if (item == Wsubbbtn4)
                {
                    if (!VehicleWindows.BackRight)
                    {
                        vehicle.RollDownWindow(VehicleWindow.BackRightWindow);
                        UI.Notify("~b~Window ~g~Opened~b~!");
                        VehicleWindows.BackRight = true;
                    }
                    else
                    {
                        vehicle.RollUpWindow(VehicleWindow.BackRightWindow);
                        UI.Notify("~b~Window ~r~Closed~b~!");
                        VehicleWindows.BackRight = false;
                    }
                }
                if (item == Wsubbbtn5)
                {
                    if (!VehicleWindows.All)
                    {
                        vehicle.RollDownWindow(VehicleWindow.FrontLeftWindow);
                        vehicle.RollDownWindow(VehicleWindow.FrontRightWindow);
                        vehicle.RollDownWindow(VehicleWindow.BackRightWindow);
                        vehicle.RollDownWindow(VehicleWindow.BackLeftWindow);
                        UI.Notify("~b~All Windows ~g~Opened~b~!");
                        if (ChangePreviousWindowStateOnUpdate) VehicleWindows.ResetWindowsState();
                        VehicleWindows.All = true;
                    }
                    else
                    {
                        vehicle.RollUpWindow(VehicleWindow.BackRightWindow);
                        vehicle.RollUpWindow(VehicleWindow.BackLeftWindow);
                        vehicle.RollUpWindow(VehicleWindow.FrontRightWindow);
                        vehicle.RollUpWindow(VehicleWindow.FrontLeftWindow);
                        UI.Notify("~b~All Windows ~r~Closed~b~!");
                        VehicleWindows.All = false;
                    }
                }
            };

            var checkbox = new UIMenuCheckboxItem("~b~Locked", false, "~g~Unlock~b~/~r~Lock~b~ The Vehicle.");
            sub1.AddItem(checkbox);
            checkbox.Enabled = true;

            var handbrake = new UIMenuCheckboxItem("~b~Parking Brakes", VehicleStore.IsVehicleToBeParked(vehicle), "~g~Release~b~/~r~Pull~b~ The Vehicle Parking Brake.");
            sub1.AddItem(handbrake);
            handbrake.Enabled = true;

            var checkbox1 = new UIMenuCheckboxItem("~b~Persistent", vehicle.IsPersistent, "~g~Enable~b~/~r~Disable~b~ The Vehicle From Being Removed From The Game.");
            sub1.AddItem(checkbox1);
            checkbox1.Enabled = true;

            var checkbox2 = new UIMenuCheckboxItem("~b~Show Blip", false, "~g~Show~b~/~r~Hide~b~ Vehicle Icon On The Map.");
            sub1.AddItem(checkbox2);
            checkbox2.Enabled = true;

            UIMenuCheckboxItem roofCheckbox = null;
            if (vehicle.IsConvertible)
            {
                bool state = false;
                if (vehicle.RoofState == VehicleRoofState.Opened || vehicle.RoofState == VehicleRoofState.Opening) state = true;
                roofCheckbox = new UIMenuCheckboxItem("~b~Roof Open", state, "~b~~g~Oped~b~/~r~Close~b~ The Vehicle Roof.");
                sub1.AddItem(roofCheckbox);
                roofCheckbox.Enabled = true;
            }

            var rstate = Function.Call<string>(Hash.GET_RADIO_STATION_NAME, vehicle);
            bool radioState = false;
            if (rstate.Equals(RadioStation.SelfRadio.ToString())) radioState = true;
            var radioCheckbox = new UIMenuCheckboxItem("~b~Self Radio", radioState, "~b~Turn ~g~On~b~/~r~Off~b~ The Vehicle Self Radio.");
            sub1.AddItem(radioCheckbox);
            radioCheckbox.Enabled = true;

            var hornCheckbox = new UIMenuCheckboxItem("~b~Horn", VehicleStore.IsVehicleToBeHorny(vehicle), "~b~Turn ~g~On~b~/~r~Off~b~ The Vehicle Horn.");
            sub1.AddItem(hornCheckbox);
            hornCheckbox.Enabled = true;

            var alarmCheckbox = new UIMenuCheckboxItem("~b~Alarm", vehicle.AlarmActive, "~b~Turn ~g~On~b~/~r~Off~b~ The Vehicle Alarm.");
            sub1.AddItem(alarmCheckbox);
            alarmCheckbox.Enabled = true;

            sub1.AddItem(button);
            sub1.AddItem(button2);

            if (vehicle.LockStatus == VehicleLockStatus.Locked) checkbox.Checked = true;
            else if (vehicle.LockStatus != VehicleLockStatus.Locked) checkbox.Checked = false;

            bool exsist = Function.Call<bool>(Hash.DOES_BLIP_EXIST, vehicle.CurrentBlip);
            if (exsist && vehicle.CurrentBlip.Alpha != 0) checkbox2.Checked = true;
            else checkbox2.Checked = false;

            sub1.RefreshIndex();

            sub1.OnCheckboxChange += (sender, item, checked_) =>
            {
                attempts = 0;
                if (item == radioCheckbox)
                {
                    if (checked_)
                    {
                        Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, vehicle, true);
                        Function.Call(Hash.SET_VEHICLE_RADIO_LOUD, vehicle, true);
                        vehicle.RadioStation = RadioStation.SelfRadio;
                    }
                    else
                    {
                        vehicle.RadioStation = RadioStation.RadioOff;
                    }
                }
                if(item == hornCheckbox)
                {
                    bool state = VehicleStore.IsVehicleToBeHorny(vehicle);
                    if (checked_)
                    {
                        if (!state) VehicleStore.AddVehicleToBeHorny(vehicle);
                        UI.Notify("~b~Vehicle Horn ~g~On~b~!");
                    }
                    else
                    {
                        if (state) VehicleStore.RemoveVehicleToBeHorny(vehicle);
                        Function.Call(Hash.START_VEHICLE_HORN, vehicle, false);
                        UI.Notify("~b~Vehicle Horn ~r~Off~b~!");
                        
                    }
                }
                if(item == handbrake)
                {
                    if (checked_)
                    {
                        if (vehicle.Model.IsCar || vehicle.Model.IsBike){
                            if (!VehicleStore.IsVehicleToBeParked(vehicle))
                            {
                                VehicleStore.AddVehicleToBeParked(vehicle);
                                vehicle.HandbrakeOn = true;
                                UI.Notify("~b~You ~r~Pulled~b~ The Vehicle Parking Brake!");
                            }
                        }
                    }
                    else
                    {
                        if (vehicle.Model.IsCar || vehicle.Model.IsBike)
                        {
                            if (VehicleStore.IsVehicleToBeParked(vehicle))
                            {
                                VehicleStore.RemoveVehicleToBeParked(vehicle);
                                vehicle.HandbrakeOn = false;
                                UI.Notify("~b~You ~g~Released~b~ The Vehicle Parking Brake!");
                            }
                        }
                    }
                }
                if (item == checkbox)
                {
                    if (checked_ == true)
                    {
                        vehicle.LockStatus = VehicleLockStatus.Locked;
                        UI.Notify("~b~You Have ~r~Locked~b~ The Vehicle Doors!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        vehicle.LockStatus = VehicleLockStatus.Unlocked;
                        UI.Notify("~b~You Have ~g~Unlocked~b~ The Vehicle Doors!");
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
                if (item == checkbox1)
                {
                    if (checked_ == true)
                    {
                        vehicle.IsPersistent = true;
                        UI.Notify("~b~The Vehicle Is Now ~r~Persistent~b~!");
                    }
                    else
                    {
                        vehicle.IsPersistent = false;
                        UI.Notify("~b~The Vehicle Is Not ~g~Persistent~b~ Anymore!");
                    }
                }
                if (item == checkbox2)
                {
                    if (checked_ == true)
                    {
                        bool exsistB = Function.Call<bool>(Hash.DOES_BLIP_EXIST, vehicle.CurrentBlip);
                        if (exsistB)
                        {
                            vehicle.CurrentBlip.Alpha = 255;
                            if (vehicle.Model.IsBike || vehicle.Model.IsBicycle) { if (vehicle.CurrentBlip.Sprite != BlipSprite.PersonalVehicleBike) vehicle.CurrentBlip.Sprite = BlipSprite.PersonalVehicleBike; }
                            else if (vehicle.CurrentBlip.Sprite != BlipSprite.PersonalVehicleCar) vehicle.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                        }else
                        {
                            vehicle.AddBlip();
                            if (vehicle.Model.IsBike || vehicle.Model.IsBicycle) { if (vehicle.CurrentBlip.Sprite != BlipSprite.PersonalVehicleBike) vehicle.CurrentBlip.Sprite = BlipSprite.PersonalVehicleBike; }
                            else if (vehicle.CurrentBlip.Sprite != BlipSprite.PersonalVehicleCar) vehicle.CurrentBlip.Sprite = BlipSprite.PersonalVehicleCar;
                            vehicle.CurrentBlip.Name = vehicle.FriendlyName;
                            vehicle.CurrentBlip.Color = BlipColor.Red;
                            vehicle.CurrentBlip.Alpha = 255;
                        }
                        UI.Notify("~b~You Turned The Vehicle Blip ~g~On~b~!");
                    }
                    else
                    {
                        bool exsistB = Function.Call<bool>(Hash.DOES_BLIP_EXIST, vehicle.CurrentBlip);
                        if (exsistB)
                        {
                            vehicle.CurrentBlip.Alpha = 0;
                        }
                        UI.Notify("~b~You Turned The Vehicle Blip ~r~Off~b~!");
                    }
                }
                if(item == engineCheckbox)
                {
                    if(checked_ == true)
                    {
                        if(!VehicleStore.IsVehicleToBeLeftOn(vehicle)) VehicleStore.AddVehicleToBeLeftOn(vehicle);
                        UI.Notify("~b~You Turned The Vehicle Engine ~g~On~b~!");
                        if (playerVehicle == vehicle) VehicleEngineRunning = true;
                        vehicle.EngineRunning = true;
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Open", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                    else
                    {
                        if (VehicleStore.IsVehicleToBeLeftOn(vehicle)) VehicleStore.RemoveVehicleToBeLeftOn(vehicle);
                        vehicle.EngineRunning = false;
                        UI.Notify("~b~You Turned The Vehicle Engine ~r~Off~b~!");
                        if (playerVehicle == vehicle) VehicleEngineRunning = false;
                        Function.Call(Hash.PLAY_SOUND_FROM_ENTITY, -1, "Remote_Control_Close", vehicle, "PI_Menu_Sounds", 1, 0);
                    }
                }
                if(item == alarmCheckbox)
                {
                    if (checked_)
                    {
                        if (vehicle.HasSiren)
                        {
                            if (!vehicle.SirenActive)
                            {
                                vehicle.SirenActive = true;
                                UI.Notify("~b~Vehicle Siren ~g~On~b~!");
                            }
                        }
                        else if (!vehicle.AlarmActive)
                        {
                            vehicle.HasAlarm = true;
                            vehicle.StartAlarm();
                            UI.Notify("~b~Vehicle Alarm ~g~On~b~!");
                        }
                    }else
                    {
                        if (vehicle.HasSiren)
                        {
                            if (vehicle.SirenActive)
                            {
                                vehicle.SirenActive = false;
                                UI.Notify("~b~Vehicle Siren ~r~Off~b~!");
                            }
                        }
                        if (vehicle.AlarmActive)
                        {
                            vehicle.HasAlarm = false;
                            UI.Notify("~b~Vehicle Alarm ~r~Off~b~!");
                        }
                    }
                }
                if(item == roofCheckbox)
                {
                    if (checked_)
                    {
                        if (vehicle.RoofState != VehicleRoofState.Opened || vehicle.RoofState != VehicleRoofState.Opening)
                        {
                            vehicle.RoofState = VehicleRoofState.Opening;
                            UI.Notify("~b~Vehicle Roof ~g~Opened~b~!");
                        }
                    }
                    else
                    {
                        if (vehicle.RoofState != VehicleRoofState.Closed || vehicle.RoofState != VehicleRoofState.Closing)
                        {
                            vehicle.RoofState = VehicleRoofState.Closing;
                            UI.Notify("~b~Vehicle Roof ~r~Closed~b~!");
                        }
                    }
                }
            };
        }

        public static void DriveVehicleTo(Ped ped, Vehicle vehicle, Vector3 target, float radius, float speed, DrivingStyle style, bool visible)
        {
            ped.Task.DriveTo(vehicle, target, radius, speed);
            ped.DrivingStyle = style;
            ped.IsVisible = visible;
        }

    }
    public static class VehicleWindows
    {
        public static bool FrontLeft = false;
        public static bool FrontRight = false;
        public static bool BackLeft = false;
        public static bool BackRight = false;
        public static bool All = false;

        private static bool LastVehicleRoofOpen = false;

        public static void ResetWindowsState()
        {
            FrontLeft = false;
            FrontRight = false;
            BackLeft = false;
            BackRight = false;
        }

        public static void UpdateWindowState(Vehicle v)
        {
            if(v.IsConvertible && v.RoofState == VehicleRoofState.Opened)
            {
                FrontLeft = true;
                FrontRight = true;
                BackLeft = true;
                BackRight = true;
                LastVehicleRoofOpen = true;
            }
            if (v.IsConvertible && v.RoofState == VehicleRoofState.Closing)
            {
                FrontLeft = false;
                FrontRight = false;
                BackLeft = false;
                BackRight = false;
                LastVehicleRoofOpen = false;
            }

            if (!v.IsConvertible && LastVehicleRoofOpen)
            {
                LastVehicleRoofOpen = false;
                FrontLeft = false;
                FrontRight = false;
                BackLeft = false;
                BackRight = false;
            }

            if (FrontLeft) v.RollDownWindow(VehicleWindow.FrontLeftWindow);
            else if (!FrontLeft)  v.RollUpWindow(VehicleWindow.FrontLeftWindow);

            if (FrontRight) v.RollDownWindow(VehicleWindow.FrontRightWindow);
            else if (!FrontRight) v.RollUpWindow(VehicleWindow.FrontRightWindow);

            if (BackLeft) v.RollDownWindow(VehicleWindow.BackLeftWindow);
            else if (!BackLeft) v.RollUpWindow(VehicleWindow.BackLeftWindow);

            if (BackRight) v.RollDownWindow(VehicleWindow.BackRightWindow);
            else if (!BackRight) v.RollUpWindow(VehicleWindow.BackRightWindow);

            if (All)
            {
                v.RollDownWindow(VehicleWindow.FrontLeftWindow);
                v.RollDownWindow(VehicleWindow.FrontRightWindow);
                v.RollDownWindow(VehicleWindow.BackRightWindow);
                v.RollDownWindow(VehicleWindow.BackLeftWindow);
            }

        }

    }
}
