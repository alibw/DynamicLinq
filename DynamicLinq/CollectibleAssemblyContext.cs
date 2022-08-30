using System.Reflection;
using System.Runtime.Loader;

namespace DynamicLinq;

public class CollectibleAssemblyContext : AssemblyLoadContext,IDisposable
{
    public CollectibleAssemblyContext() : base(true)
    {
        
    }
    
    protected override Assembly Load(AssemblyName assemblyName)
    {
        return null;
    }
    
    public void Dispose()
    {
        Unload();
    }
    
    
}