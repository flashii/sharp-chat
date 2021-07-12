using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharpChat.Reflection {
    public class ObjectConstructor<TObject, TAttribute, TDefault>
        where TAttribute : ObjectConstructorAttribute
        where TDefault : TObject {
        private Dictionary<string, Type> Types { get; } = new();
        private bool AllowDefault { get; }

        public ObjectConstructor(bool allowDefault = true) {
            AllowDefault = allowDefault;
            Reload();
        }

        public void Reload() {
            Types.Clear();
            IEnumerable<Assembly> asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly asm in asms) {
                IEnumerable<Type> types = asm.GetExportedTypes();
                foreach(Type type in types) {
                    Attribute attr = type.GetCustomAttribute(typeof(TAttribute));
                    if(attr is not null and ObjectConstructorAttribute oca)
                        Types.Add(oca.Name, type);
                }
            }
        }

        public TObject Construct(string name, params object[] args) {
            if(name == null)
                throw new ArgumentNullException(name);

            Type type = !Types.ContainsKey(name)
                ? (AllowDefault ? typeof(TDefault) : throw new ObjectConstructorObjectNotFoundException(name))
                : Types[name];

            IEnumerable<object> arguments = args;
            IEnumerable<Type> types = arguments.Select(a => a.GetType());
            ConstructorInfo[] cis = type.GetConstructors();
            ConstructorInfo constructor = null;

            for(;;) {
                foreach(ConstructorInfo ci in cis) {
                    IEnumerable<Type> constTypes = ci.GetParameters().Select(p => p.ParameterType);
                    if(constTypes.Count() != arguments.Count())
                        continue;

                    bool isMatch = true;
                    for(int i = 0; i < constTypes.Count(); ++i)
                        if(!types.ElementAt(i).IsAssignableTo(constTypes.ElementAt(i))) {
                            isMatch = false;
                            break;
                        }

                    if(isMatch) {
                        constructor = ci;
                        break;
                    }
                }

                if(constructor != null || !arguments.Any())
                    break;
                arguments = arguments.Take(arguments.Count() - 1);
                types = types.Take(arguments.Count());
            }

            if(constructor == null)
                throw new ObjectConstructorConstructorNotFoundException(name);

            return (TObject)constructor.Invoke(arguments.ToArray());
        }
    }
}
