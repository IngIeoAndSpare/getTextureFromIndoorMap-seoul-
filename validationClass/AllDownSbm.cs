//참고 : http://inasie.tistory.com/5

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    class AllDownSbm
    {
        private static AllDownSbm allDownSbm; 
        private int threadPoint;
        private List<string> errorList;
        private AllDownSbm()
        {
            threadPoint = 0;
            errorList = new List<string>();
        }

        public static AllDownSbm GetInstance()
        {
            if (allDownSbm == null) allDownSbm = new AllDownSbm();
            return allDownSbm;
        }
        internal void upThreadPoint()
        {
            this.threadPoint++;
        }
        internal void downThreadPoint()
        {
            this.threadPoint--;
        }
        internal void setErrorList(string error)
        {
            this.errorList.Add(error);
        }
        internal List<string> getErrorList()
        {
            return this.errorList;
        }
        public bool checkThreadPoint()
        {
            if(this.threadPoint >= 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }




    }
}
