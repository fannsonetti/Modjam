using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ScheduleOne.NPCs;

class RunNpcProbe
{
    static void Main()
    {
        var managed = @"D:\Schedule 1 Dependencies\Mono\Schedule I_Data\Managed";
        AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
        {
            var name = new AssemblyName(e.Name).Name + ".dll";
            var path = Path.Combine(managed, name);
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };

        foreach (var m in typeof(NPC).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.Name.IndexOf("Impostor", StringComparison.OrdinalIgnoreCase) >= 0
                || x.Name.IndexOf("Body", StringComparison.OrdinalIgnoreCase) >= 0
                || x.Name.IndexOf("Avatar", StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(x => x.Name))
            Console.WriteLine("NPC " + m.MemberType + " " + m.Name);

        foreach (var m in typeof(NPCManager).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .Where(x => x.Name.IndexOf("NPC", StringComparison.OrdinalIgnoreCase) >= 0
                || x.Name.IndexOf("List", StringComparison.OrdinalIgnoreCase) >= 0)
            .OrderBy(x => x.Name))
            Console.WriteLine("MGR " + m.MemberType + " " + m.Name);
    }
}
