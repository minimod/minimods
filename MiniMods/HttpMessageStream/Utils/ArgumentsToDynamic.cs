using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;

namespace Minimod.HttpMessageStream.Utils
{
    public class ArgumentsToDynamic : DynamicObject
    {
        private readonly IDictionary<string, string> _arguments;

        public ArgumentsToDynamic(NameValueCollection nameValueCollection)
        {
            _arguments = nameValueCollection.Keys.OfType<string>().ToDictionary(k => k.ToString(), k => nameValueCollection[k], StringComparer.InvariantCultureIgnoreCase);
        }

        public ArgumentsToDynamic(IDictionary<string, string> arguments)
        {
            _arguments = arguments;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string value;
            result = !_arguments.TryGetValue(binder.Name.ToLower(), out value) ? null : value;
            return true;
        }
    }
}