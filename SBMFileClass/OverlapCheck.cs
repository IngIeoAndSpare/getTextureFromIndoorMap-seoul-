using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
namespace WpfApp1
{
    class OverlapCheck
    {
        public List<string> CheckOverlap(List<string> sbmFileList)
        {
            HashSet<string> distince = new HashSet<string>(sbmFileList);
            List<string> result = new List<String>(distince);

            return result;
        }


    }
}
