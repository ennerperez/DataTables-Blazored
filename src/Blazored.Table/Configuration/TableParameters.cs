using System.Collections.Generic;

namespace Blazored.Table
{
    public class TableParameters
    {
        internal Dictionary<string, object> Parameters;

        public TableParameters()
        {
            Parameters = new Dictionary<string, object>();
        }

        public void Add(string parameterName, object value)
        {
            Parameters[parameterName] = value;
        }

        public T Get<T>(string parameterName)
        {
            if (Parameters.TryGetValue(parameterName, out var value))
            {
                return (T)value;
            }
            
            throw new KeyNotFoundException($"{parameterName} does not exist in table parameters");
        }

        public T TryGet<T>(string parameterName)
        {
            if (Parameters.TryGetValue(parameterName, out var value))
            {
                return (T)value;
            }

            return default;
        }
    }
}
