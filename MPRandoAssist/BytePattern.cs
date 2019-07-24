using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPRandoAssist
{
    class BytePattern
    {
        internal static long Find(byte[] datas, String hexstring)
        {
            try
            {
                long i, j;
                bool found;
                if (hexstring.Length % 2 != 0)
                     return -2;
                for (i = 0; i < datas.Length; i++)
                {
                    found = true;
                    for (j = 0; j < hexstring.Length / 2; j++)
                    {
                        if (hexstring.Substring((int)(j*2), 2) == "??")
                            continue;
                        if (datas[j + i] != Convert.ToByte(hexstring.Substring((int)(j * 2), 2), 16))
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                        return i;
                }
                return -1;
            }
            catch
            {
                return -2;
            }
        }

        internal static long Find(byte[] datas, params String[] hexstring)
        {
            try
            {
                long i, j;
                bool found;
                for (i = 0; i < hexstring.Length; i++)
                    if (hexstring[i].Length != 2)
                        return -2;
                for (i = 0; i < datas.Length; i++)
                {
                    found = true;
                    for (j = 0; j < hexstring.Length; j++)
                    {
                        if (hexstring[j] == "??")
                            continue;
                        if (datas[j + i] != Convert.ToByte(hexstring[j], 16))
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                        return i;
                }
                return -1;
            } catch {
                return -2;
            }
        }
    }
}
