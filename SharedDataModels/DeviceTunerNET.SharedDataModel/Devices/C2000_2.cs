﻿using DeviceTunerNET.SharedDataModel.ElectricModules;
using DeviceTunerNET.SharedDataModel.ElectricModules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel.Devices
{
    public class C2000_2 : OrionDevice, IShleifs
    {
        private readonly int inputsCount = 2;

        #region Properties
        
        public IEnumerable<Shleif> Shleifs { get; set; }

        #endregion Properties


        public C2000_2(IPort port) : base (port)
        {
            ModelCode = 16;
            SupportedModels = new List<string>
            {
                "С2000-2",
            };

            var inputs = new List<Shleif>();
            for (byte i = 0; i < inputsCount; i++)
            {
                inputs.Add(new Shleif(this, i));
            }
            Shleifs = inputs;
        }


    }
}
