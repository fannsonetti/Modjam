using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ScheduleOne.PlayerScripts;

class P
{
    static void Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
        {
            var n = new AssemblyName(e.Name).Name + ".dll";
            var p = Path.Combine(@"D:\Schedule 1 Dependencies\Mono\Schedule I_Data\Managed", n);
            return File.Exists(p) ? Assembly.LoadFrom(p) : null;
        };

        foreach (var m in typeof(Player).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.Name.IndexOf("Third", StringComparison.OrdinalIgnoreCase) >= 0
                || x.Name.IndexOf("Avatar", StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(x => x.Name))
            Console.WriteLine(m.MemberType + " " + m.Name);
    }
}
