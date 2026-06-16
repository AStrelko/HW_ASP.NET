namespace HW_05.Async.Models;

// Загальний стан виконання задачі.
//Використовується для відстеження прогресу та статистики.

public class TaskState
{
    public string Adress; //адреса корня
    public string SearchWord;// шукаєье слово
    public  string NewWord; //слово для зміни
    public int TotalFiles; // Загальна кількість файлів для обробки
    public int ProcessedFiles; // Скільки файлів вже оброблено

    //  Пошук слів 
    public int FoundFiles;   // файли, де знайдено слово
    public int FoundWords;   // кількість входжень слова

    //  Заміна слів 
    public int ReplacedWords; // скільки слів замінено
    public int CopiedFiles;   // скільки файлів скопійовано

    //  Аналіз C# коду 
    public int ClassesFound;      // знайдені класи
    public int InterfacesFound;   // знайдені інтерфейси

    // Чи завершена задача
    public bool IsCompleted;
    
    public List<string> Classes = new();
    public List<string> Interfaces = new();
}