using System.Collections.Generic;
using UnityEngine;

namespace Vams2.Data
{
    [System.Serializable]
    public class WaveEntry
    {
        public float mStartTime;
        public float mEndTime;
        public EnemyData mEnemyData;
        public float mSpawnInterval = 1f;
        public int mSpawnCount = 1;
        public bool mIsBoss;
    }

    [CreateAssetMenu(fileName = "NewWaveData", menuName = "Vams2/WaveData")]
    public class WaveData : ScriptableObject
    {
        public string mWaveName;
        public float mSessionDuration = 300f;
        public List<WaveEntry> mEntries = new List<WaveEntry>();
    }
}
