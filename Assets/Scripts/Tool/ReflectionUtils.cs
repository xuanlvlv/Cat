using System.Linq;

public static class ReflectionUtils
{
    /// <summary>
    /// ���ش�ָ��������������������
    /// </summary>
    /// <param name="aAppDomain"></param>
    /// <param name="aType"></param>
    /// <returns></returns>
    public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
    {
        var assemblies = aAppDomain.GetAssemblies();
        return (from assembly in assemblies
                from type in assembly.GetTypes()
                where type.IsSubclassOf(aType)
                select type).ToArray();
    }
}
