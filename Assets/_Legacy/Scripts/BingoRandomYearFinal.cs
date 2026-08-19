using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BingoRandomYearFinal : MonoBehaviour
{
    private const string DefaultCentralText = "Ведущий в зале";
    
    [SerializeField] private GameObject bingoParent;
    
    private Text[] _bingoElems;
    private readonly int _centralCellNumber = 12;
    
    // We can take any count of elements from "none" group
    // and only one element from each of any other group
    private readonly Dictionary<string, string> _itemToGroupMap = new Dictionary<string, string>()
    {
        { "Вопрос со странными предметами", "SpecificThemeQuestion" },
        { "Вопрос о книге", "SpecificThemeQuestion" },
        { "Вопрос с цитатой", "SpecificThemeQuestion" },
        { "Вопрос про кино", "SpecificThemeQuestion" },
        { "Вопрос не из города", "SpecificThemeQuestion" },
        { "Вопрос про ЧГК", "SpecificThemeQuestion" },
        { "Вопрос с картиной", "SpecificThemeQuestion" },
        { "Восток - дело тонкое", "SpecificThemeQuestion" },
        { "Вопрос от мира науки", "SpecificThemeQuestion" },
        { "Вопрос, связанный с иностранным языком", "SpecificThemeQuestion" },
        { "Вопрос, связанный с музыкой", "SpecificThemeQuestion" },
        { "Вопрос с одеждой", "SpecificThemeQuestion" },
        { "Вопрос из Москвы", "SpecificRegionQuestion" },
        { "Вопрос из Питера", "SpecificRegionQuestion" },
        { "Вопрос из Ростова-На-Дону", "SpecificRegionQuestion" },
        { "Вопрос с Дальнего Востока", "SpecificRegionQuestion" },
        { "Вопрос из города в игорной зоне РФ", "SpecificRegionQuestion" },
        { "Вопрос не из России", "SpecificRegionQuestion" },
        { "Вопрос из Беларуси", "SpecificRegionQuestion" },
        { "Вопрос от детей", "SpecificSourceOfOfQuestion" },
        { "Видео вопрос от телезрителя", "SpecificSourceOfOfQuestion" },
        { "Видео вопрос от мужчины", "SpecificSourceOfOfQuestion" },
        { "Видео вопрос от женщины", "SpecificSourceOfOfQuestion" },
        { "Счет 1:1", "SpecificScore" },
        { "Счет 2:2", "SpecificScore" },
        { "Счет 3:3", "SpecificScore" },
        { "Счет 4:4", "SpecificScore" },
        { "Счет 5:5", "SpecificScore" },
        { "Знатоки выиграют 6:5", "SpecificScore" },
        { "Знатоки проиграют 5:6", "SpecificScore" },
        { "Знатоки проигрывают 0:3", "SpecificScore" },
        { "Знатоки выигрывают 3:0", "SpecificScore" },
        { "Обе команды набрали хотя бы 4 очка", "SpecificScore" },
        { "Очевидный хрустальный атом", "Obviously" },
        { "Очевидный лучший вопрос игры", "Obviously" },
        { "Несколько черных ящиков в одном вопросе", "BlackBox" },
        { "Несколько черных ящиков в течение игры", "BlackBox" },
        { "Начало ЧГК позже указанного на 5 мин", "StartTime" },
        { "Начало ЧГК позже указанного на 10 мин", "StartTime" },
        { "Начало ЧГК позже указанного на 15 мин", "StartTime" },
        { "Хотя бы 3 академика в зале", "Academicians" },
        { "Хотя бы 4 академика в зале", "Academicians" },
        { "Хотя бы 5 академиков в зале", "Academicians" },
        { "Берется минута в кредит", "None" },
        { "Знатоку делают замечание", "None" },
        { "Рофл над ведущим", "None" },
        { "Ведущий задает дополнительный вопрос", "None" },
        { "Волчок остановился на границе", "None" },
        { "Отвечает капитан", "None" },
        { "Показывают Поташева", "None" },
        { "Досрочный ответ", "None" },
        { "Берется помощь зала", "None" },
        { "Защитник телезрителей засчитывает вопрос", "None" },
        { "Ведущий недоволен выбором версий", "None" },
        { "Знатоки просят повторить вопрос", "None" },
        { "Сову получила женщина", "None" },
        { "Нет единогласного решения по сове", "None" },
        { "Не все телезрители смогли приехать", "None" },
        { "Ведущий спрашивает версии в зале", "None" },
    };

    void Awake()
    {
        _bingoElems = bingoParent.GetComponentsInChildren<Text>();

        // Bring all keys in list and shuffle it
        List<string> allItems = new List<string>(_itemToGroupMap.Keys);
        Shuffle(allItems);
        
        List<string> selected = new List<string>();
        
        HashSet<string> usedGroups = new HashSet<string>();
        
        int needCount = _bingoElems.Length - 1;

        // Pick elements
        for (int i = 0; i < allItems.Count && selected.Count < needCount; i++)
        {
            string candidate = allItems[i];
            string groupName = _itemToGroupMap[candidate];
            
            if (groupName != "None" && usedGroups.Contains(groupName))
                continue;
            
            selected.Add(candidate);
            
            if (groupName != "None")
                usedGroups.Add(groupName);
        }

        // Fill the cells
        int indexSelected = 0;
        for (int i = 0; i < _bingoElems.Length; i++)
        {
            if (i == _centralCellNumber)
            {
                // Central cell
                _bingoElems[i].text = GetDefaultCentralText();
            }
            else
            {
                _bingoElems[i].text = selected[indexSelected];
                indexSelected++;
            }
        }
    }

    private string GetDefaultCentralText() => DefaultCentralText;

    // Fisher–Yates Shuffle
    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
