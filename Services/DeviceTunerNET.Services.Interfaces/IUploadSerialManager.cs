﻿using DeviceTunerNET.SharedDataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IUploadSerialManager: IDisposable
    {
        public string Protocol { get; set; }
        public string PortName { get; set; }
        public Action<int> UpdateProgressBar { get; set; }
        public bool QualityControl(IOrionDevice device);
        public bool Upload(IOrionDevice device, string serialNumb);
    }
}
