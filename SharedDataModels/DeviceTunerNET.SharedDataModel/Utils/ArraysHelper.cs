using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceTunerNET.SharedDataModel.Utils
{
    public static class ArraysHelper
    {
        public static byte[] CombineArrays(params byte[][] arrays)
        {
            var resultArray = new byte[arrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var item in arrays)
            {
                Buffer.BlockCopy(item, 0, resultArray, offset, item.Length);
                offset += item.Length;
            }
            return resultArray;
        }
    }
}
