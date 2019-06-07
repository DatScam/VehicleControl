using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVC
{
    class IntPV
    {
        private static List<object> _data = new List<object>();
        private static List<object> _vars = new List<object>();

        public static void SetVariable(object variable, object data)
        {
            if (variable != null && data != null)
            {
                bool foundSet = false;
                foreach (object s in _vars)
                {
                    if (s.Equals(variable)) foundSet = true;
                }
                if (!foundSet)
                {
                    _vars.Add(variable);
                    _data.Add(data);
                }
                else
                {
                    int index = GetIndexAtVar(variable);
                    if (index > -1)
                    {
                        _data.RemoveAt(index);
                        _vars.RemoveAt(index);
                        _data.Add(data);
                        _vars.Add(variable);
                    }
                }
            }
        }

        public static void RemoveVariable(object variable)
        {
            if (variable != null)
            {
                bool foundSet = false;
                foreach (object s in _vars)
                {
                    if (s.Equals(variable)) foundSet = true;
                }
                if (foundSet)
                {
                    int index = GetIndexAtVar(variable);
                    if (index > -1)
                    {
                        _data.RemoveAt(index);
                        _vars.RemoveAt(index);
                    }
                }
            }
        }

        public static object GetVariableData(object variable)
        {
            object result = null;
            if (variable != null)
            {
                bool foundSet = false;
                foreach (object s in _vars)
                {
                    if (s.Equals(variable)) foundSet = true;
                }
                if (foundSet)
                {
                    int index = GetIndexAtVar(variable);
                    if (index > -1)
                    {
                        result = _data[index];
                    }
                }
            }
            return result;
        }

        private static int GetIndexAtVar(object obj)
        {
            int result = -1;
            for (int i = 0; i < _vars.Count; i++)
            {
                if (_vars[i].Equals(obj)) result = i;
            }
            return result;
        }

    }
}
