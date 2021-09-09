using System;

namespace ZNXHelpers
{
    public static class EnvHelper
    {
        public static string GetString(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }

        public static string GetString(string key, string defaultValue)
        {
            var envRes = GetString(key);
            if (string.IsNullOrEmpty(envRes))
            {
                envRes = defaultValue;
            }
            return envRes;
        }

        public static int GetInt(string key)
        {
            var envVar = GetString(key);
            return int.Parse(envVar);
        }

        public static int GetInt(string key, int defaultValue)
        {
            int envRes;
            try
            {
                envRes = GetInt(key);
            }
            catch (Exception)
            {
                envRes = defaultValue;
            }

            return envRes;
        }

        public static long GetLong(string key)
        {
            var envVar = GetString(key);
            return long.Parse(envVar);
        }

        public static long GetLong(string key, long defaultValue)
        {
            long envRes;
            try
            {
                envRes = GetLong(key);
            }
            catch (Exception)
            {
                envRes = defaultValue;
            }

            return envRes;
        }

        public static double GetDouble(string key)
        {
            var envVar = GetString(key);
            return double.Parse(envVar);
        }

        public static double GetDouble(string key, double defaultValue)
        {
            double envRes;
            try
            {
                envRes = GetDouble(key);
            }
            catch (Exception)
            {
                envRes = defaultValue;
            }

            return envRes;
        }

        public static bool GetBool(string key)
        {
            var envVar = GetString(key);
            return bool.Parse(envVar);
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            bool envRes;
            try
            {
                envRes = GetBool(key);
            }
            catch (Exception)
            {
                envRes = defaultValue;
            }

            return envRes;
        }
    }
}
