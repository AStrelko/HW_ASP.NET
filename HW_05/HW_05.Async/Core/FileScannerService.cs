using System.Text.RegularExpressions;
using HW_05.Async.Models;

namespace HW_05.Async.Core;

// Сервіс для роботи з файловою системою:
//- пошук слів
// - заміна слів
// - аналіз C# файлів

public class FileScannerService
{
    string destinationFolder = @"C:\TestLess05";
    
    // Отримати всі файли за маскою (*.txt або *.cs)
    private List<string> GetFiles(string folderPath, string pattern)
    {
        var files = new List<string>();

        try
        {
            files.AddRange(Directory.GetFiles(folderPath, pattern));
        }
        catch (UnauthorizedAccessException)
        {
            // Немає доступу до папки
        }
        catch (Exception)
        {
            // Інші помилки читання
        }

        try
        {
            foreach (var directory in Directory.GetDirectories(folderPath))
            {
                files.AddRange(GetFiles(directory, pattern));
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Немає доступу до підпапки
        }
        catch (Exception)
        {
            // Інші помилки
        }

        return files;
    }

    // TASK1. ПОШУК СЛОВА  + TASK2. ЗНАЙТИ ФАЙЛИ З ЗАДОНИМ СЛОВОМ, СКОПІЮВАТИ, ЗМІНИТИ СЛОВО +
    // + TASK3 ПОШУК ТА РОХУВАННЯ ІНТЕРФЕЙСІВ ТА КЛАСІВ
    public async Task SearchWordAsync(TaskState state, CancellationToken token, string taskN)
    {
        
        Console.Write("Шлях до папки: ");
        state.Adress = Console.ReadLine()!;

        if (taskN != "task3")
        {
            Console.Write("Слово: ");
            state.SearchWord = Console.ReadLine()!;
        }

        if (taskN == "task2")
        {
            Console.Write("Нове слово: ");
            state.NewWord = Console.ReadLine()!;
        }
        if (!Directory.Exists(state.Adress))
        {
            Console.WriteLine("Папка не знайдена!");
            return;
        }
        // отримую список файлів
        var files = taskN == "task3"
            ? GetFiles(state.Adress, "*.cs")
            : GetFiles(state.Adress, "*.txt");

        // отримую кількість файлів
        state.TotalFiles = files.Count;

        foreach (var file in files)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                string text = await File.ReadAllTextAsync(file, token);

                int count = Regex.Matches(
                    text,
                    Regex.Escape(state.SearchWord),
                    RegexOptions.IgnoreCase
                ).Count;

                if (taskN == "task1")
                {
                    if (count > 0)
                    {
                        state.FoundFiles++;
                        state.FoundWords += count;
                    }
                }
                else if (taskN == "task2")
                {
                    if (count > 0)
                    {
                        string newText = text.Replace(
                            state.SearchWord,
                            state.NewWord);

                        string fileName =
                            $"{Guid.NewGuid()}_{Path.GetFileName(file)}";

                        string newFilePath = Path.Combine(
                            destinationFolder,
                            fileName);

                        await File.WriteAllTextAsync(
                            newFilePath,
                            newText,
                            token);

                        state.CopiedFiles++;
                        state.ReplacedWords += count;
                    }
                }
                else if (taskN == "task3")
                {
                    var classes = Regex.Matches(
                        text,
                        @"\bclass\s+(\w+)"
                    );

                    foreach (Match match in classes)
                    {
                        state.ClassesFound++;
                        state.Classes.Add(match.Groups[1].Value);
                    }
                    var interfaces = Regex.Matches(
                        text,
                        @"\binterface\s+(\w+)"
                    );

                    foreach (Match match in interfaces)
                    {
                        state.InterfacesFound++;
                        state.Interfaces.Add(match.Groups[1].Value);
                    }
                }
            }
            catch (Exception)
            {
                // Пропускаємо файл
            }

            state.ProcessedFiles++;
        }

        state.IsCompleted = true;
    }
    
}