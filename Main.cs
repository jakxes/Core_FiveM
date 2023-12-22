using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Diagnostics;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Core_FiveM  {
    public class Main : BaseScript {
        /*
         * MAIN INSTANCE
         */

        public Main() {
            Tick += onTick;
            
            // KEYMAPPING REGISTRY
            API.RegisterKeyMapping("sp_st_veh", "Spawns your standard vehicle.", "keyboard", "n");
            API.RegisterKeyMapping("dont_fall", "Activates that you cannot exit the vehicle", "keyboard", "m");
            API.RegisterKeyMapping("repair", "Repairs vehicle.", "keyboard", "b");
            API.RegisterKeyMapping("mfh", "Move forward and up", "keyboard", "x");
            API.RegisterKeyMapping("+mfs", "Move forward", "keyboard", "c");
            API.RegisterKeyMapping("+rgvr", "roll left", "keyboard", "j");
            API.RegisterKeyMapping("+rgvl", "roll right", "keyboard", "k");
            
            // COMMAND REGISTRY
            
            // "/gaw" => Give {Player} all weapons
            API.RegisterCommand("gaw", new Action<int, List<object>, string>((src, args, raw) => { give_all_weapons();}), false);
            
            
            // "/invincible" => Make {Player} either invincible or vincible.
            API.RegisterCommand("invincible", new Action<int, List<object>, string>((src, args, raw) => {
                Game.PlayerPed.IsInvincible = !Game.PlayerPed.IsInvincible;
                send_message($"Invincibility set to {Game.PlayerPed.IsInvincible}");
            }), false);
            
            // "/refresh_core" => Refreshes Core resource
            API.RegisterCommand("refresh_core", new Action<int, List<object>, string>((src, args, raw) => {
                API.ExecuteCommand($"restart {API.GetCurrentResourceName()}");
            }), false);
            
            // "/cm" => Changes the ped model from {Player}.
            API.RegisterCommand("cm", new Action<int, List<object>, string>((src, args, raw) => { change_model(args[0].ToString());}), false);
            
            // "/sp_veh" => Spawns Vehicle.
            API.RegisterCommand("sp_veh", new Action<int, List<object>, string>((src, args, raw) => { spawn_vehicle(args[0].ToString()); }), false);
            
            // "/fast_drive" => Enables fast driving for {Player}.
            API.RegisterCommand("fast_drive", new Action<int, List<object>, string>((src, args, raw) => {
                fast_drive = !fast_drive;
                send_message($"Fast driving changed to {fast_drive}.");
            }), false);
            
            // "/repair" => Repairs vehicle. 
            API.RegisterCommand("repair", new Action<int, List<object>, string>((src, args, raw) => { Game.PlayerPed.CurrentVehicle.Repair();}), false);
            
            // "/wanted" => Changes wanted level.
            API.RegisterCommand("wanted", new Action<int, List<object>, string>((src, args, raw) => {
                if (args[0].ToString() == "change") {
                    wanted_type = !wanted_type;
                    send_message($"Changed if you can be wanted to: {wanted_type}");
                } else if (Int32.TryParse(args[0].ToString(), out _)) {
                    wanted_type = true;
                    Game.Player.WantedLevel = Int32.Parse(args[0].ToString());
                    send_message($"Changed wanted level to {Game.Player.WantedLevel}.");
                } else send_message("Argument must be either \"change\" or 0-5.", "error");
                
            }), false);
            
            // "/dont_fall" => Changes if {Player} is able to exit the vehicle.
            API.RegisterCommand("dont_fall", new Action<int, List<object>, string>((src, args, raw) => {
                able_to_fall = !able_to_fall;
                send_message($"Whether you can exit the vehicle changed to {able_to_fall}.");
            }), false);
            
            // "/st_veh" => Changes standard vehicle.
            API.RegisterCommand("st_veh", new Action<int, List<object>, string>((src, args, raw) => {
                if (new Model(args[0].ToString()).IsVehicle) {
                    standard_vehicle = args[0].ToString();
                    send_message($"Standard vehicle changed to {standard_vehicle}.");
                } else send_message($"Model \"{args[0].ToString()}\" is not valid.","error"); 
            }), false);
            
            // "/sp_st_veh" => Spawns standard vehicle.
            API.RegisterCommand("sp_st_veh", new Action<int, List<object>, string>((src, args, raw) => {
                spawn_vehicle(standard_vehicle);
            }), false);
            
            // "/cm" => Changes model of {Player}.
            API.RegisterCommand("cm", new Action<int, List<object>, string>((src, args, raw) => {
                change_model(args[0].ToString());
            }), false);
            
            
            // Rocket League Movement
            API.RegisterCommand("+rgvr", new Action<int, List<object>, string>(async (src, args, raw) => {
                rgvr = true;
            }), false);
            API.RegisterCommand("-rgvr", new Action<int, List<object>, string>(async (src, args, raw) => {
                rgvr = false;
            }), false);
            API.RegisterCommand("+rgvl", new Action<int, List<object>, string>(async (src, args, raw) => {
                rgvl = true;
            }), false);
            API.RegisterCommand("-rgvl", new Action<int, List<object>, string>(async (src, args, raw) => {
                rgvl = false;
            }), false);
            API.RegisterCommand("mfh", new Action<int, List<object>, string>(async (src, args, raw) => {
                Game.PlayerPed.CurrentVehicle.ForwardVector.Normalize();
                Game.PlayerPed.CurrentVehicle.Velocity += new Vector3(0, 0f, 10f);
            }), false);
            API.RegisterCommand("+mfs", new Action<int, List<object>, string>(async (src, args, raw) => {
                boost = true;
            }), false);
            API.RegisterCommand("-mfs", new Action<int, List<object>, string>(async (src, args, raw) => {
                boost = false;
            }), false);
        }



        private async Task onTick() {
            try {
                Game.Player.SetRunSpeedMultThisFrame(1.499f);
                Game.Player.SetSwimSpeedMultThisFrame(1.499f);
                Game.Player.SetSuperJumpThisFrame();
                if (Game.PlayerPed.IsInVehicle()) {
                    if (fast_drive) {
                        if (Game.PlayerPed.CurrentVehicle.IsInAir || Game.PlayerPed.CurrentVehicle.Model.IsBoat) {
                            Game.PlayerPed.CurrentVehicle.Gravity = 9.81f;
                        } else {
                            Game.PlayerPed.CurrentVehicle.Gravity = 35f;
                            Game.PlayerPed.CurrentVehicle.MaxSpeed = 500;
                            Game.PlayerPed.CurrentVehicle.EnginePowerMultiplier = 500;
                        }
                    } else {
                        Game.PlayerPed.CurrentVehicle.Gravity = 9.81f;
                        Game.PlayerPed.CurrentVehicle.MaxSpeed = 50;
                        Game.PlayerPed.CurrentVehicle.EnginePowerMultiplier = 50;
                    }
                
                    if (Game.PlayerPed.CurrentVehicle.IsInWater && !Game.PlayerPed.CurrentVehicle.Model.IsBoat) {
                        Game.PlayerPed.CurrentVehicle.Velocity += new Vector3(0, 0f, 6f) + Game.PlayerPed.CurrentVehicle.ForwardVector * 6;
                        Game.PlayerPed.CurrentVehicle.Repair();
                    }
                    
                    if((rgvr == true))
                        Game.PlayerPed.CurrentVehicle.Rotation += new Vector3(0f,4f,0f);
                

                    if ((rgvl == true))
                        Game.PlayerPed.CurrentVehicle.Rotation += new Vector3(0f, -4f, 0f);
                
                    if(boost == true)
                        Game.PlayerPed.CurrentVehicle.Speed = 50f;

                    
                }
               
                if (able_to_fall == false && !Game.PlayerPed.IsInVehicle() && Game.Player.LastVehicle != null && Game.PlayerPed.LastVehicle.IsAlive) 
                    Game.PlayerPed.SetIntoVehicle(Game.PlayerPed.LastVehicle, VehicleSeat.Driver);
                
                if (wanted_type == false) 
                    Game.Player.WantedLevel = 0;
                
               
               
                API.SetTextFont(4);  
                API.SetTextScale(0.5f, 0.5f);  
                API.SetTextColour(0, 255, 0, 255);
                API.SetTextEntry("TWOSTRINGS");
                string vehicle = Game.PlayerPed.IsInVehicle() ? Game.PlayerPed.CurrentVehicle.DisplayName : "null";
                API.AddTextComponentString($"Invincibility » {Game.Player.IsInvincible}\nAble to fall? » {able_to_fall}\nFast driving » {fast_drive}\n"); 
                API.AddTextComponentString2($"Can be wanted? » {wanted_type}\nCurrent Vehicle » {vehicle}\nGametime » {Game.GameTime/(1000*60)}min");
                
                API.DrawText(0f, 0.62f);
                
            }
            catch (Exception ex) {
                Debug.Print(ex.ToString());
            }
        }


        /*
         * COMMANDS
         */

        private static void give_all_weapons() {
            foreach (WeaponHash weapon_hash in Enum.GetValues(typeof(WeaponHash)))
            {
                var weapon = Game.PlayerPed.Weapons.Give(weapon_hash, 1, false, true);
                weapon.InfiniteAmmoClip = true;
                send_message($"{Game.Player.Name} received \"{weapon_hash}\".");
            }
        }

        public async Task change_model(string model_name) {
            Model model = new Model(model_name);
            if (model.IsValid) {
                if (model.IsPed) {
                    await Game.Player.ChangeModel(model);
                    send_message($"Model changed to {model_name}.");
                } else send_message($"Model \"{model_name}\" is not a ped model.","error");
            } else send_message($"Model \"{model_name}\" is not valid.","error");
        }

        public async Task spawn_vehicle(string model_name,bool set_into_vehicle = true, VehicleColor color=(VehicleColor)160, bool neonlights=true, bool invincibility=true, float x_off=0f, float y_off=5f, float z_off=0f) {
            Model model = new Model(model_name);
            if (model.IsValid) {
                if (model.IsVehicle)
                {
                    var vehicle = await World.CreateVehicle(API.GetHashKey(model_name), Game.PlayerPed.GetOffsetPosition(new Vector3(x_off,y_off,z_off)), Game.PlayerPed.Heading);
                    vehicle.IsInvincible = invincibility;
                    Random rnd = new Random();
                    int num = rnd.Next(160);
                    
                    vehicle.Mods.PrimaryColor = color == (VehicleColor)160 ? (VehicleColor) num : color;num = rnd.Next(160);
                    vehicle.Mods.SecondaryColor = color == (VehicleColor)160 ? (VehicleColor) num : color;num = rnd.Next(160);
                    vehicle.Mods.PearlescentColor = color == (VehicleColor)160 ? (VehicleColor) num : color;num = rnd.Next(160);
                    vehicle.Mods.TrimColor = color == (VehicleColor)160 ? (VehicleColor) num : color;num = rnd.Next(160);
                    vehicle.Mods.RimColor = color == (VehicleColor)160 ? (VehicleColor) num : color;num = rnd.Next(160);
                    vehicle.Mods.DashboardColor = color == (VehicleColor)160 ? (VehicleColor)num : color;
                    vehicle.Windows.RollDownAllWindows();
                    
                    if (neonlights) {
                        vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, true);
                        vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, true);
                        vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, true);
                        vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, true);
                    }
                    vehicle.Mods.InstallModKit();
                    if(set_into_vehicle) Game.PlayerPed.SetIntoVehicle(vehicle, VehicleSeat.Driver);
                    vehicle.Mods[VehicleModType.Spoilers].Index = 1;
                    vehicle.Mods[VehicleModType.Exhaust].Index = 2;
                    vehicle.Mods[VehicleModType.Armor].Index = 4;
                    vehicle.Mods[VehicleModType.Horns].Index = 7;
                    vehicle.Mods[VehicleModType.Brakes].Index = 2;
                    vehicle.Mods[VehicleModType.Engine].Index = 3;
                    vehicle.IsEngineRunning = true;
                    vehicle.IsBulletProof = true;
                    vehicle.CanTiresBurst = false;
                    vehicle.IsFireProof = true;
                    vehicle.IsCollisionProof = true;
                    vehicle.IsBurnoutForced = false;
                    vehicle.IsAxlesStrong = true;
                } else send_message($"Model \"{model_name}\" is not a vehicle model.","error"); 
            } else send_message($"Model \"{model_name}\" is not valid.","error");
        }
        
        
     
        /*
         * BASE FUNCTIONS
         */
        
        public static void send_message(string message, string color="default", bool multiline_p = true, int r=0, int g=0, int b=0) {
            r = color != "default"  && r==0 && g==0 && b==0 ? 255 : 0;
            g = color != "error" &&  g==0 && b==0 ? 255 : 0;
            TriggerEvent("chat:addMessage", new {
                color = new[] {r, g, b},
                args = new[] {"Core » ", message},
                multiline = multiline_p
            });
        }
        
        /*
         * GLOBAL VARIABLES
         */
        
        private bool fast_drive = true;
        private bool able_to_fall = false;
        private bool wanted_type = true;
        private int wanted_level = 0;
        private string standard_vehicle = "bf400";
        bool rgvr = false;
        bool rgvl = false;
        bool boost = false;
        
    }

}