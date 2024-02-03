using DeviceTunerNET.SharedDataModel.Devices;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.ElectricModules.Interfaces
{
    public interface IAccessController
    {
        /// <summary>
        /// Предоставление доступа
        /// </summary>
        public byte[] ProvidingAccess();

        /// <summary>
        /// Разрешение(восстановление) доступа
        /// </summary>
        public byte[] AccessPermission();

        /// <summary>
        /// Разрешение входа
        /// </summary>
        public byte[] PermissionEntrance();

        /// <summary>
        /// Разрешение выхода
        /// </summary>
        public byte[] PermissionOutput();

        /// <summary>
        /// Запрет доступа
        /// </summary>
        public byte[] AccessDenied();

        /// <summary>
        /// Запрет входа
        /// </summary>
        public byte[] EntranceDenied();

        /// <summary>
        /// Запрет выхода
        /// </summary>
        public byte[] OutputeDenied();

        /// <summary>
        /// Открытие доступа
        /// </summary>
        public byte[] AllowAccess();

    }
}
