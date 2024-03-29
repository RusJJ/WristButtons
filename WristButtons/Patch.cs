﻿using HarmonyLib;
using System.Reflection;

namespace HarmonyPatcher
{
    class Patch
    {
        private static bool m_bIsPatched = false;
        private static Harmony m_hMyInstance = null;
        private static string m_szInstanceId = null;
        public static bool IsPatched()
        {
            return m_bIsPatched;
        }
        internal static void Apply(string instanceId)
        {
            if (m_szInstanceId == null) m_szInstanceId = instanceId;
            if (m_hMyInstance == null)
            {
                m_hMyInstance = new Harmony(m_szInstanceId);
                if (!m_bIsPatched)
                {
                    m_hMyInstance.PatchAll(Assembly.GetExecutingAssembly());
                    m_bIsPatched = true;
                }
            }
        }
        internal static void Remove()
        {
            if (m_hMyInstance != null)
            {
                m_hMyInstance.UnpatchAll(m_szInstanceId);
            }
            m_bIsPatched = false;
        }
    }
}
