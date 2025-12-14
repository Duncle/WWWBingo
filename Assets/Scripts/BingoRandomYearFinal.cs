using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BingoRandomYearFinal : MonoBehaviour
{
    public GameObject bingoParent;
    private Text[] _bingoElems;
    private int _centralCellNumber = 12;

    // Словарь "элемент -> группа"
    // Из группы "None" можно брать неограниченно,
    // а из любой другой (например, "SpecificScore") — только 1 элемент.
    private Dictionary<string, string> itemToGroupMap = new Dictionary<string, string>()
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
        { "Вопрос из casino zone", "SpecificRegionQuestion" },
        { "Вопрос не из России", "SpecificRegionQuestion" },
        { "Вопрос из Беларуси", "SpecificRegionQuestion" },
        { "Вопрос от детей", "SpecificSourceOfOfQuestion" },
        { "Видео вопрос от телезрителя", "SpecificSourceOfOfQuestion" },
        { "Спецсектор в первом раунде", "SpecialSector" },
        { "Спецсектор в последнем раунде", "SpecialSector" },
        { "Счет 1:1", "SpecificScore" },
        { "Счет 2:2", "SpecificScore" },
        { "Счет 3:3", "SpecificScore" },
        { "Счет 4:4", "SpecificScore" },
        { "Счет 5:5", "SpecificScore" },
        { "Знатоки выиграют 6:5", "SpecificScore" },
        { "Знатоки проиграют 5:6", "SpecificScore" },
        { "Знатоки проигрывают 0:3", "SpecificScore" },
        { "Знатоки выигрывают 3:0", "SpecificScore" },
        { "Очевидный хрустальный атом", "Obviously" },
        { "Очевидный лучший вопрос игры", "Obviously" },
        { "Несколько черных ящиков в одном вопросе", "BlackBox" },
        { "Несколько черных ящиков в течение игры", "BlackBox" },
        { "Начало ЧГК позже указанного на 5 мин", "StartTime" },
        { "Начало ЧГК позже указанного на 10 мин", "StartTime" },
        { "Начало ЧГК позже указанного на 15 мин", "StartTime" },
        { "Берется минута в кредит", "None" },
        { "Знатоку делают замечание", "None" },
        { "Рофл над ведущим", "None" },
        { "Крюк задает дополнительный вопрос", "None" },
        { "Волчок остановился на границе", "None" },
        { "Отвечает капитан", "None" },
        { "В 13 секторе тупой вопрос", "None" },
        { "Показывают Поташева", "None" },
        { "Досрочный ответ", "None" },
        { "Берется помощь зала", "None" },
        { "Защитник телезрителей засчитывает вопрос", "None" },
        { "Крюк недоволен выбором версий", "None" },
        { "Знатоки просят повторить вопрос", "None" },
        { "Хотя бы 3 академика в зале", "None" },
        { "Сову получила женщина", "None" },
        { "Нет единогласного решения по сове", "None" },
        { "Не все телезрители смогли приехать", "None" },
    };

    void Awake()
    {
        _bingoElems = bingoParent.GetComponentsInChildren<Text>();

        // Собираем все ключи в список и перемешиваем их
        List<string> allItems = new List<string>(itemToGroupMap.Keys);
        Shuffle(allItems);

        // Сюда кладём выбранные варианты
        List<string> selected = new List<string>();

        // Множество занятых групп (кроме "None")
        HashSet<string> usedGroups = new HashSet<string>();

        // Нужно заполнить 24 ячейки (если всего 25, а одна — центральная).
        int needCount = _bingoElems.Length - 1;

        // Идём по перемешанному списку и отбираем элементы
        for (int i = 0; i < allItems.Count && selected.Count < needCount; i++)
        {
            string candidate = allItems[i];
            string groupName = itemToGroupMap[candidate];

            // Если группа не "None" и уже занята — пропускаем
            if (groupName != "None" && usedGroups.Contains(groupName))
            {
                continue;
            }

            // Иначе берём
            selected.Add(candidate);

            // Если группа не "None", помечаем её как занятую
            if (groupName != "None")
            {
                usedGroups.Add(groupName);
            }
        }

        // Если по каким-то причинам элементов не хватает (слишком мало было в словаре),
        // то selected.Count может быть < needCount.
        // Можно проверить и как-то обработать ситуацию, но в примере пропустим.

        // Разводим по ячейкам
        int indexSelected = 0;
        for (int i = 0; i < _bingoElems.Length; i++)
        {
            if (i == _centralCellNumber)
            {
                // Центральная клетка
                _bingoElems[i].text = GetDefaultCentralText();
            }
            else
            {
                _bingoElems[i].text = selected[indexSelected];
                indexSelected++;
            }
        }
    }

    private string GetDefaultCentralText()
    {
        return "Ведущий в зале";
    }

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
