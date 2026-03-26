using System.Collections.Generic;
using UnityEngine;
using Vams2.Data;
using Vams2.InGame.Player;

namespace Vams2.InGame.Skill
{
    // 스킬 선택지 타입
    public enum SkillChoiceType
    {
        NewSkill = 0,
        Upgrade,
        Evolution,
        Max
    }

    // 레벨업 시 제시되는 스킬 선택지
    public class SkillChoice
    {
        public SkillChoiceType mChoiceType;
        public SkillData mSkillData;
        public int mNextLevel; // 업그레이드 시 다음 레벨, 새 스킬은 1
    }

    // 스킬 슬롯 (장착된 스킬 1개)
    public class SkillSlot
    {
        public SkillData mData;
        public int mLevel;
        public SkillBase mSkillInstance;

        public SkillSlot(SkillData data, int level, SkillBase instance)
        {
            mData = data;
            mLevel = level;
            mSkillInstance = instance;
        }
    }

    // 인게임 스킬 슬롯 관리
    public class SkillManager : MonoBehaviour
    {
        public const int MAX_ACTIVE_SLOTS = 6;
        public const int MAX_PASSIVE_SLOTS = 6;

        [SerializeField] private SkillData[] mAllActiveSkills;  // 전체 액티브 스킬 목록
        [SerializeField] private SkillData[] mAllPassiveSkills;  // 전체 패시브 스킬 목록

        private List<SkillSlot> mActiveSlots;
        private List<SkillSlot> mPassiveSlots;
        private PlayerStats mPlayerStats;
        private Transform mPlayerTransform;

        public List<SkillSlot> ActiveSlots => mActiveSlots;
        public List<SkillSlot> PassiveSlots => mPassiveSlots;

        public void Initialize(PlayerStats stats, SkillData[] allActive, SkillData[] allPassive)
        {
            mPlayerStats = stats;
            mPlayerTransform = stats.transform;
            mAllActiveSkills = allActive;
            mAllPassiveSkills = allPassive;
            mActiveSlots = new List<SkillSlot>();
            mPassiveSlots = new List<SkillSlot>();
        }

        // 스킬 추가 또는 업그레이드
        public void AddOrUpgradeSkill(SkillChoice choice)
        {
            if (choice.mSkillData.mSkillType == SkillType.Active)
            {
                HandleActiveSkill(choice);
            }
            else if (choice.mSkillData.mSkillType == SkillType.Passive)
            {
                HandlePassiveSkill(choice);
            }
        }

        private void HandleActiveSkill(SkillChoice choice)
        {
            // 기존 슬롯에서 찾기
            for (int i = 0; i < mActiveSlots.Count; i++)
            {
                if (mActiveSlots[i].mData == choice.mSkillData)
                {
                    // 업그레이드
                    mActiveSlots[i].mLevel = choice.mNextLevel;
                    if (mActiveSlots[i].mSkillInstance != null)
                    {
                        mActiveSlots[i].mSkillInstance.OnLevelUp(choice.mNextLevel);
                    }
                    return;
                }
            }

            // 새 스킬 추가
            if (mActiveSlots.Count >= MAX_ACTIVE_SLOTS)
            {
                return;
            }

            SkillBase skillInstance = CreateSkillInstance(choice.mSkillData);
            if (skillInstance != null)
            {
                skillInstance.Initialize(choice.mSkillData, 1, mPlayerStats);
                SkillSlot slot = new SkillSlot(choice.mSkillData, 1, skillInstance);
                mActiveSlots.Add(slot);
            }
        }

        private void HandlePassiveSkill(SkillChoice choice)
        {
            // 기존 슬롯에서 찾기
            for (int i = 0; i < mPassiveSlots.Count; i++)
            {
                if (mPassiveSlots[i].mData == choice.mSkillData)
                {
                    // 업그레이드
                    mPassiveSlots[i].mLevel = choice.mNextLevel;
                    ApplyPassiveEffect(choice.mSkillData, choice.mNextLevel);
                    return;
                }
            }

            // 새 패시브 추가
            if (mPassiveSlots.Count >= MAX_PASSIVE_SLOTS)
            {
                return;
            }

            SkillSlot slot = new SkillSlot(choice.mSkillData, 1, null);
            mPassiveSlots.Add(slot);
            ApplyPassiveEffect(choice.mSkillData, 1);
        }

        private void ApplyPassiveEffect(SkillData data, int level)
        {
            if (mPlayerStats == null)
            {
                return;
            }

            float value = data.GetSpecialValue(level);

            switch (data.mSkillId)
            {
                case "ManaSurge":
                    mPlayerStats.SetAttackSpeedBonus(level * 0.10f);
                    break;
                case "ElementalCore":
                    mPlayerStats.SetElementalDmgBonus(level * 0.15f);
                    break;
                case "MagicArmor":
                    mPlayerStats.SetDefenseBonus(level * 5);
                    mPlayerStats.SetKnockbackResist(level * 0.20f);
                    break;
                case "SwiftBoots":
                    mPlayerStats.SetMoveSpeedBonus(level * 0.08f);
                    mPlayerStats.SetDodgeBonus(level * 0.03f);
                    break;
            }
        }

        // 레벨업 시 랜덤 선택지 3개 생성
        public List<SkillChoice> GetRandomChoices(int count)
        {
            List<SkillChoice> candidates = new List<SkillChoice>();

            // 1. 기존 스킬 업그레이드 (Lv < 5)
            for (int i = 0; i < mActiveSlots.Count; i++)
            {
                if (mActiveSlots[i].mLevel < 5)
                {
                    SkillChoice choice = new SkillChoice();
                    choice.mChoiceType = SkillChoiceType.Upgrade;
                    choice.mSkillData = mActiveSlots[i].mData;
                    choice.mNextLevel = mActiveSlots[i].mLevel + 1;
                    candidates.Add(choice);
                }
            }
            for (int i = 0; i < mPassiveSlots.Count; i++)
            {
                if (mPassiveSlots[i].mLevel < 5)
                {
                    SkillChoice choice = new SkillChoice();
                    choice.mChoiceType = SkillChoiceType.Upgrade;
                    choice.mSkillData = mPassiveSlots[i].mData;
                    choice.mNextLevel = mPassiveSlots[i].mLevel + 1;
                    candidates.Add(choice);
                }
            }

            // 2. 새 액티브 스킬 (빈 슬롯 있을 때)
            if (mActiveSlots.Count < MAX_ACTIVE_SLOTS && mAllActiveSkills != null)
            {
                for (int i = 0; i < mAllActiveSkills.Length; i++)
                {
                    if (!HasSkill(mAllActiveSkills[i]))
                    {
                        SkillChoice choice = new SkillChoice();
                        choice.mChoiceType = SkillChoiceType.NewSkill;
                        choice.mSkillData = mAllActiveSkills[i];
                        choice.mNextLevel = 1;
                        candidates.Add(choice);
                    }
                }
            }

            // 3. 새 패시브 스킬 (빈 슬롯 있을 때)
            if (mPassiveSlots.Count < MAX_PASSIVE_SLOTS && mAllPassiveSkills != null)
            {
                for (int i = 0; i < mAllPassiveSkills.Length; i++)
                {
                    if (!HasSkill(mAllPassiveSkills[i]))
                    {
                        SkillChoice choice = new SkillChoice();
                        choice.mChoiceType = SkillChoiceType.NewSkill;
                        choice.mSkillData = mAllPassiveSkills[i];
                        choice.mNextLevel = 1;
                        candidates.Add(choice);
                    }
                }
            }

            // 셔플 후 count개 반환
            ShuffleList(candidates);

            // 규칙: 최소 1개는 미보유 스킬 보장 (가능할 때)
            List<SkillChoice> result = new List<SkillChoice>();
            bool hasNewSkill = false;
            SkillChoice reservedNew = null;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].mChoiceType == SkillChoiceType.NewSkill && reservedNew == null)
                {
                    reservedNew = candidates[i];
                    continue;
                }
                if (result.Count < count)
                {
                    result.Add(candidates[i]);
                    if (candidates[i].mChoiceType == SkillChoiceType.NewSkill)
                    {
                        hasNewSkill = true;
                    }
                }
            }

            // 미보유 스킬 1개 보장
            if (!hasNewSkill && reservedNew != null && result.Count > 0)
            {
                result[result.Count - 1] = reservedNew;
            }
            else if (reservedNew != null && result.Count < count)
            {
                result.Add(reservedNew);
            }

            // 부족하면 남은 후보로 채우기
            for (int i = 0; i < candidates.Count && result.Count < count; i++)
            {
                if (!result.Contains(candidates[i]))
                {
                    result.Add(candidates[i]);
                }
            }

            return result;
        }

        private bool HasSkill(SkillData data)
        {
            for (int i = 0; i < mActiveSlots.Count; i++)
            {
                if (mActiveSlots[i].mData == data) return true;
            }
            for (int i = 0; i < mPassiveSlots.Count; i++)
            {
                if (mPassiveSlots[i].mData == data) return true;
            }
            return false;
        }

        private SkillBase CreateSkillInstance(SkillData data)
        {
            GameObject skillGo = new GameObject("Skill_" + data.mSkillId);
            skillGo.transform.SetParent(mPlayerTransform);

            SkillBase skill = null;

            switch (data.mSkillCategory)
            {
                case SkillCategory.Projectile:
                    MagicBoltSkill bolt = skillGo.AddComponent<MagicBoltSkill>();
                    if (data.mProjectilePrefab != null)
                    {
                        bolt.SetProjectilePrefab(data.mProjectilePrefab);
                    }
                    skill = bolt;
                    break;
                // 나머지 스킬은 Phase 4-2에서 추가
                default:
                    skill = skillGo.AddComponent<MagicBoltSkill>();
                    break;
            }

            return skill;
        }

        private void ShuffleList(List<SkillChoice> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                SkillChoice temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
