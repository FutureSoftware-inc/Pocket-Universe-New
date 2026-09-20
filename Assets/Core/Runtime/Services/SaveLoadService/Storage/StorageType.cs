using UnityEngine;

namespace CrystalEngine.Services
{
    public enum StorageType : byte
    {
        LocalFile = 0,
        PlayerPrefs = 1,
        CloudeServer = 2
    }

}