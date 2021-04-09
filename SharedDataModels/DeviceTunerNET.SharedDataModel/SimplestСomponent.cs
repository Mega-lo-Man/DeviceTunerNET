using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.SharedDataModel
{
    public class SimplestСomponent
    {
        private int _id;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Модель компонента ("С2000-СП1 исп.01 АЦДР.425412.001-01")
        /// </summary>
        private string _model;
        public string Model
        {
            get { return _model; }
            set { _model = value; }
        }

        /// <summary>
        /// Тип прибора ("Блок сигнально-пусковой")
        /// </summary>
        private string _deviceType;
        public string DeviceType
        {
            get { return _deviceType; }
            set { _deviceType = value; }
        }

        /// <summary>
        /// Обозначение компонента на схеме ("SR1.3")
        /// </summary>
        private string _designation;
        public string Designation
        {
            get { return _designation; }
            set { _designation = value; }
        }

        /// <summary>
        /// Обозначение шкафа в котором находится этот дивайс
        /// </summary>
        private string _cabinetDesignation;
        public string ParentCabinetDesignation
        {
            get { return _cabinetDesignation; }
            set { _cabinetDesignation = value; }
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (this.GetType() != obj.GetType())
                return false;
            return this.Equals(obj as SimplestСomponent);
        }
        public bool Equals(SimplestСomponent obj)
        {
            if (obj == null)
                return false;
            if (object.ReferenceEquals(this, obj))
                return true;
            if (this.GetType() != obj.GetType())
                return false;
            if (string.Compare(this.Designation, obj.Designation, StringComparison.CurrentCulture) == 0 &&
                string.Compare(this.Model, obj.Model, StringComparison.CurrentCulture) == 0 &&
                string.Compare(this.DeviceType, obj.DeviceType, StringComparison.CurrentCulture) == 0)
                return true;
            else
                return false;
        }
    }
}
