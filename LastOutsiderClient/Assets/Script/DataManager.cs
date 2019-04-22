using LastOutsiderShared.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DataManager
{
    private static DataManager instance;
    public static DataManager Instance {
        get {
            if(instance == null)
            {
                instance = new DataManager();
            }
            return instance;
        }
    }

    public Resource Resource;

    public void ReadFetchData(FetchData fetchData)
    {
        Resource = fetchData.resource;
    }
}
