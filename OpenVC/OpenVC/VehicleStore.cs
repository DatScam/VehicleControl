using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenUtils
{
    class VehicleStore
    {
        public static List<Vehicle> tbovehicles = new List<Vehicle>();
        public static List<Vehicle> managedVehicles = new List<Vehicle>();
        public static List<Vehicle> calledVehicles = new List<Vehicle>();
        public static List<Vehicle> wasCalled = new List<Vehicle>();
        public static List<Vehicle> parkedVehicles = new List<Vehicle>();
        public static List<Vehicle> hornyVehicles = new List<Vehicle>();

        public static void AddVehicleToBeLeftOn(Vehicle veh)
        {
            if(tbovehicles == null) tbovehicles = new List<Vehicle>();
            tbovehicles.Add(veh);
        }

        public static void RemoveVehicleToBeLeftOn(Vehicle veh)
        {
            tbovehicles.Remove(veh);
        }

        public static void UpdateVehiclesToBeLeftOn()
        {
            
        }

        public static bool IsVehicleToBeLeftOn(Vehicle veh)
        {
            bool result = false;
            foreach(Vehicle v in tbovehicles.ToList())
            {
                if (veh == v) result = true;
            }
            return result;
        }

        public static void AddVehicleToBeManaged(Vehicle veh)
        {
            if (managedVehicles == null) managedVehicles = new List<Vehicle>();
            managedVehicles.Reverse();
            managedVehicles.Add(veh);
            managedVehicles.Reverse();
        }

        public static void RemoveVehicleToBeManaged(Vehicle veh)
        {
            managedVehicles.Remove(veh);
        }

        public static void UpdateVehiclesToBeManaged()
        {

            if(managedVehicles.Count > main.MaxManagedVehicles)
            {
                for (int i = main.MaxManagedVehicles; i < managedVehicles.Count; i++) managedVehicles.RemoveAt(i);
            }
        }

        public static bool IsVehicleToBeManaged(Vehicle veh)
        {
            bool result = false;
            foreach (Vehicle v in managedVehicles.ToList())
            {
                if (veh == v) result = true;
            }
            return result;
        }

        public static void AddVehicleToBeCalled(Vehicle veh)
        {
            if (calledVehicles == null) calledVehicles = new List<Vehicle>();
            calledVehicles.Add(veh);
        }

        public static void RemoveVehicleToBeCalled(Vehicle veh)
        {
            if(IsVehicleToBeCalled(veh)) calledVehicles.Remove(veh);
        }

        public static bool IsVehicleToBeCalled(Vehicle veh)
        {
            bool result = false;
            foreach (Vehicle v in calledVehicles.ToList())
            {
                if (veh == v) result = true;
            }
            return result;
        }

        public static void AddVehicleWasCalled(Vehicle veh)
        {
            if (wasCalled == null) wasCalled = new List<Vehicle>();
            wasCalled.Add(veh);
        }

        public static void RemoveVehicleWasCalled(Vehicle veh)
        {
            if (WasVehicleCalled(veh)) wasCalled.Remove(veh);
        }

        public static bool WasVehicleCalled (Vehicle veh)
        {
            bool result = false;
            foreach (Vehicle v in wasCalled.ToList())
            {
                if (veh == v) result = true;
            }
            return result;
        }

        public static void AddVehicleToBeParked(Vehicle veh)
        {
            if (parkedVehicles == null) parkedVehicles = new List<Vehicle>();
            parkedVehicles.Add(veh);
        }

        public static void RemoveVehicleToBeParked(Vehicle veh)
        {
            parkedVehicles.Remove(veh);
        }

        public static void UpdateVehiclesToBeParked()
        {
            foreach(Vehicle v in parkedVehicles.ToList())
            {
                v.HandbrakeOn = true;
            }
        }

        public static bool IsVehicleToBeParked(Vehicle veh)
        {
            bool result = false;
            foreach (Vehicle v in parkedVehicles.ToList())
            {
                if (veh == v) result = true;
            }
            return result;
        }

        public static void AddVehicleToBeHorny(Vehicle veh)
        {
            if (hornyVehicles == null) hornyVehicles = new List<Vehicle>();
            hornyVehicles.Add(veh);
        }

        public static void RemoveVehicleToBeHorny(Vehicle veh)
        {
            hornyVehicles.Remove(veh);
        }

        public static bool IsVehicleToBeHorny(Vehicle veh)
        {
            bool result = false;
            foreach (Vehicle v in hornyVehicles.ToList())
            {
                if (veh == v) result = true;
            }
            return result;
        }

        public static void UpdateVehiclesToBeHorny()
        {
            foreach (Vehicle v in hornyVehicles.ToList())
            {
                bool state = Function.Call<bool>(Hash.IS_HORN_ACTIVE, v);
                if (!state)
                {
                    Function.Call(Hash.START_VEHICLE_HORN, v, true);
                }
            }
        }


    }
}
