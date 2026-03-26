using UnityEngine;

namespace Vams2.Data
{
    [CreateAssetMenu(fileName = "NewEvolutionData", menuName = "Vams2/EvolutionData")]
    public class EvolutionData : ScriptableObject
    {
        public SkillData mRequiredActiveSkill;
        public SkillData mRequiredPassiveSkill;
        public SkillData mResultSkill;
        public string mEvolutionName;
    }
}
