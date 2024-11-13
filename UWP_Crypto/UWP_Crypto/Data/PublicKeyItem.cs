using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWP_Crypto.Utils;

namespace UWP_Crypto.Data;
public class PublicKeyItem
{
    public readonly string Name, Info;
    public readonly KeyHolder Holder;

    public PublicKeyItem(KeyHolder holder)
    {
        Holder = holder;
        Name = Holder.Name;
        Info = Holder.CreateTime.ToString("yy/MM/dd HH:mm:ss");
        if (!Holder.HasPrivate)
        {
            Info += "  不包含私钥";
        }
        else
        {
            if (Holder.IsEncrypted)
            {
                Info += "  包含加密的私钥";
            }
            else
            {
                Info += "  包含未加密的私钥";
            }
        }
    }
}