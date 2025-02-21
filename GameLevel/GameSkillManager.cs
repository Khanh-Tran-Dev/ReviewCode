
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SurZom.Core
{
    public class GameSkillManager : MonoBehaviour
    {
        [SerializeField] GameObject skillInfo;
        [SerializeField] Transform skillIconPlace;
        [SerializeField] TextMeshProUGUI skillDetail;

        [SerializeField] List<Transform> skillSlots;
        [SerializeField] List<Transform> skillHandlePlaces;

        private List<SkillPiece> equippedSkills = new List<SkillPiece>();
        private List<GameObject> skillIcons = new List<GameObject>();
        private List<GameObject> skillHandles = new List<GameObject>();

        public void EquipSkillProcess(SkillPiece skill)
        {
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                if (equippedSkills[i] == null)
                {
                    equippedSkills[i] = skill;
                    skill.Equipped();
                    skillIcons[i] = Instantiate(skill.skillIcon, skillSlots[i]) as GameObject;
                    skillHandles[i] = Instantiate(skill.skillPrefab, skillHandlePlaces[i]) as GameObject;
                    return;
                }
            }

            if (equippedSkills.Count < 3)
            {
                equippedSkills.Add(skill);
                skill.Equipped();
                skillIcons.Add(Instantiate(skill.skillIcon, skillSlots[equippedSkills.Count - 1]) as GameObject);
                skillHandles.Add(Instantiate(skill.skillPrefab, skillHandlePlaces[equippedSkills.Count - 1]) as GameObject);
            }
        }

        public void UnEquipSkill(SkillPiece skill)
        {
            int index = equippedSkills.IndexOf(skill);
            if (index >= 0)
            {
                Destroy(skillIcons[index]);
                Destroy(skillHandles[index]);
                skillIcons[index] = null;
                skillHandles[index] = null;
                skill.UnEquipped();
                equippedSkills[index] = null;
            }
        }

        public void UnEquipAllSkills()
        {
            for (int i = 0; i < equippedSkills.Count; i++)
            {
                if (equippedSkills[i] != null)
                {
                    Destroy(skillIcons[i]);
                    Destroy(skillHandles[i]);
                    skillIcons[i] = null;
                    skillHandles[i] = null;
                    equippedSkills[i].UnEquipped();
                    equippedSkills[i] = null;
                }
            }
        }

        public void OnSkillInfo(SkillPiece skill)
        {
            skillInfo.SetActive(true);
            GameObject skillIcon = Instantiate(skill.skillIcon, skillIconPlace) as GameObject;
            skillDetail.text = skill.skillDetail;
        }

        public void OffSkillInfo()
        {
            skillInfo.SetActive(false);
            foreach (var icon in skillIcons)
            {
                Destroy(icon);
            }
            skillIcons.Clear();
        }
    }
}
