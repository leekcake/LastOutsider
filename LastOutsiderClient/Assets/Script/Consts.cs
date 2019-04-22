using LastOutsiderShared.Data;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class Consts
{
    public static string ACCOUNT_FILE {
        get {
            return Path.Combine(Application.persistentDataPath, "accounts.bin");
        }
    }

    public static Account GetAccountFromFile()
    {
        if( !File.Exists(ACCOUNT_FILE) )
        {
            return null;
        }
        return MessagePackSerializer.Deserialize<Account>(File.ReadAllBytes(Consts.ACCOUNT_FILE));
    }
}
