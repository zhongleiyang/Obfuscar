using System;
using System.Collections.Generic;
using System.Text;

namespace Obfuscar
{
    class UnityUtil
    {
        static public List<string> skipMethods = new List<string>{"Update", 
                                                                    "LateUpdate", 
                                                                    "FixedUpdate",
                                                                    "Awake", 
                                                                    "Start",	
                                                                    "Reset",
                                                                    "Main"};

        static public bool isSkipMethod(string methodString)
        {
            if (methodString.Contains("::On"))
                return true;

            foreach (string unityMethod in skipMethods)
                if (methodString.Contains(unityMethod))
                    return true;

            return false;
        }

    }
}
