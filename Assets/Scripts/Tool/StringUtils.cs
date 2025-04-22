using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class StringUtils
{
    /// <summary>
    /// ��ָ�����ַ�����Ϊ�ɶ��ַ�������
    /// </summary>
    /// <param name="camelCase"></param>
    /// <returns></returns>
    public static string DisplayCamelCaseString(string camelCase)
    {
        var chars = new List<char> { camelCase[0] };
        foreach (var c in camelCase.Skip(1))
        {
            if (char.IsUpper(c))
            {
                chars.Add(' ');
                chars.Add(char.ToLower(c));
            }
            else
            {
                chars.Add(c);
            }
        }

        return new string(chars.ToArray());
    }
}
