using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;

public static class ExtensionMethods
{
    public static async Task<bool> WaitForStateAuthority(this NetworkObject o, float maxWaitTime = 8)
    {
        float waitStartTime = Time.time;
        o.RequestStateAuthority();
        while (!o.HasStateAuthority && (Time.time - waitStartTime) < maxWaitTime)
        {
            await System.Threading.Tasks.Task.Delay(1);
        }
        return o.HasStateAuthority;
    }
}
