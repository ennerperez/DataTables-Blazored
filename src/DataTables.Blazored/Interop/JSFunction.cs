using System;

namespace DataTables.Blazored.Interop
{
    public class JSFunction
    {
        public string Namespace { get; }

        public string Function { get; }

        public JSFunction(string nameSpace, string function)
        {
            if (string.IsNullOrWhiteSpace(nameSpace))
                throw new ArgumentException("Namespace for function must be provided!", nameof(nameSpace));
            else if (string.IsNullOrWhiteSpace(function))
                throw new ArgumentException("A function name must be provided!", nameof(function));

            Namespace = nameSpace;
            Function = function;
        }

        public static implicit operator JSFunction(string javaScriptFunction)
        {
            var split = javaScriptFunction.Split('.');
            if (split.Length != 2)
                throw new ArgumentException("Javascript function must be composed of a namespace and a function separated by a '.'", nameof(javaScriptFunction));
            
            return new JSFunction(split[0], split[1]);
        }
    }
}
