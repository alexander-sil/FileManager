using FileManager.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Principal;
using System.Security.AccessControl;

namespace FileManager
{
    internal class Program
    {
        const int WINDOW_HEIGHT = 30;
        const int WINDOW_WIDTH = 120;
        private static string currentDir;


        static void Main(string[] args)
        {
			try {
			currentDir = Properties.Settings.Default.LastPath;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Title = "FileManager";

            Console.SetWindowSize(WINDOW_WIDTH, WINDOW_HEIGHT);
            Console.SetBufferSize(WINDOW_WIDTH, WINDOW_HEIGHT);
            
            DrawWindow(0, 0, WINDOW_WIDTH, 18);
            DrawWindow(0, 18, WINDOW_WIDTH, 8);
            UpdateConsole();

            Console.ReadLine();
			}
            catch (Exception e)
            {
                // Сериализатор ошибок
                DateTime date = DateTime.Now;
                Console.SetCursorPosition(1, 27);
                Console.WriteLine("Error: " + e.Message);
                Console.ReadKey(true);
                if (!File.Exists(@"random-name-exception.txt"))
                {
                    File.Create(@"random-name-exception.txt").Dispose();
                }
                File.AppendAllText(@"random-name-exception.txt", (date + " " + e.Message + "\n"));

            }


        }

        /// <summary>
        /// Вспомогательный метод, получить текущую позицию курсора
        /// </summary>
        /// <returns></returns>
        static (int left, int top) GetCursorPosition()
        {
            return (Console.CursorLeft, Console.CursorTop);
        }

        /// <summary>
        /// Обработка процесса ввода данных с консоли
        /// </summary>
        /// <param name="width">Длина строки ввода</param>

        // Обработка (препроцессинг) введенной команды
        static void ProcessEnterCommand(int width)
        {
            //TODO: ...
            (int left, int top) = GetCursorPosition();
            StringBuilder command = new StringBuilder();
            ConsoleKeyInfo keyInfo;
            char key;

            do
            {
                keyInfo = Console.ReadKey();
                key = keyInfo.KeyChar;

                if (keyInfo.Key != ConsoleKey.Enter && keyInfo.Key != ConsoleKey.Backspace &&
                    keyInfo.Key != ConsoleKey.UpArrow)
                    command.Append(key);

                (int currentLeft, int currentTop) = GetCursorPosition();

                if (currentLeft == width - 2)
                {
                    Console.SetCursorPosition(currentLeft - 1, top);
                    Console.Write(" ");
                    Console.SetCursorPosition(currentLeft - 1, top);
                }
                
                if(keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (command.Length > 0)
                        command.Remove(command.Length - 1, 1);
                    if (currentLeft >= left)
                    {
                        Console.SetCursorPosition(currentLeft, top);
                        Console.Write(" ");
                        Console.SetCursorPosition(currentLeft, top);
                    }
                    else
                    {
                        command.Clear();
                        Console.SetCursorPosition(left, top);
                    }
                }

            }
            while (keyInfo.Key != ConsoleKey.Enter);
            ParseCommandString(command.ToString());
        }
        // Информация о файле
        static void Info(FileInfo file)
        {
            Console.SetCursorPosition(2, 19);
            Console.WriteLine(file.Attributes);
            Console.SetCursorPosition(2, 20);
            Console.WriteLine(file.CreationTime);
            Console.SetCursorPosition(2, 21);
            Console.WriteLine(file.LastAccessTime);
            Console.SetCursorPosition(2, 22);
            Console.WriteLine(file.Extension);
            Console.SetCursorPosition(2, 23);
            Console.WriteLine(file.Length + " bytes on disk");
        
        }
        // Информация о папке (перегрузка)
        static void DIRInfo(DirectoryInfo dir)
        {
            Console.SetCursorPosition(2, 19);
            Console.WriteLine(dir.Attributes);
            Console.SetCursorPosition(2, 20);
            Console.WriteLine(dir.CreationTime);
            Console.SetCursorPosition(2, 21);
            Console.WriteLine(dir.LastAccessTime);
            Console.SetCursorPosition(2, 22);
            Console.WriteLine("Exists: " + dir.Exists);
            Console.SetCursorPosition(2, 23);
            Console.WriteLine(dir.FullName);
        }
        // Интерпретатор команд
        static void ParseCommandString(string command)
        {
            string[] commandParams = command.ToLower().Split(' ');
            if (commandParams.Length > 0)
            {
                switch (commandParams[0])
                {
                    case "cd":
                        if (commandParams.Length > 1)
                            if (Directory.Exists(commandParams[1]))
                            {
                                currentDir = commandParams[1];
                                Properties.Settings.Default.LastPath = currentDir;
                                Properties.Settings.Default.Save();
                            }
                        break;
                    case "ls":
                        if (commandParams.Length > 1)
                        {
                            if (commandParams.Length > 2 && commandParams[1] == "-p" && int.TryParse(commandParams[2], out int n))
                            {
                                DrawTree(new DirectoryInfo(currentDir), n, false);
                            } else if (commandParams[1] == "-s")
                            {
                                DrawTree(new DirectoryInfo(currentDir), 1, true);
                            } else if (commandParams[1] == "-sp" && int.TryParse(commandParams[2], out int s))
                            {
                                DrawTree(new DirectoryInfo(currentDir), s, true);
                            }

                        }
                        else
                        {
                            DrawTree(new DirectoryInfo(currentDir), 1, false);
                        }
                        break;
                    case "cp":
                        if (commandParams.Length > 2)
                        {
                            if (File.Exists(currentDir + @"\" + commandParams[1]))
                            {
                                if (Directory.Exists(commandParams[2]))
                                {


                                    File.Create(commandParams[2] + @"\" + commandParams[1]).Dispose();
                                    File.Copy(currentDir + @"\" + commandParams[1], (commandParams[2] + @"\" + commandParams[1]), true);
                                    

                                }

                                else if (!Directory.Exists(currentDir + @"\" + commandParams[2]))
                                {
                                    Directory.CreateDirectory(commandParams[2]);
                                    File.Create(commandParams[2] + @"\" + commandParams[1]).Dispose();
                                    File.Copy(commandParams[1], (commandParams[2] + @"\" + commandParams[1]), true);
                                    
                                }
                            }
                            else if (Directory.Exists(currentDir + @"\" + commandParams[1]))
                            {
                                if (!Directory.Exists(commandParams[2]))
                                {
                                    Directory.CreateDirectory(commandParams[2]);
                                    CopyDirectory(currentDir + @"\" + commandParams[1], commandParams[2], true);
                                    
                                }
                                else
                                {
                                    CopyDirectory(currentDir + @"\" + commandParams[1], commandParams[2], true);
                                    
                                }


                            }
                        }
                        break;
                    case "info":
                        if (commandParams.Length > 1) {
                            if (File.Exists(currentDir + @"\" + commandParams[1]))
                            {
                                DrawWindow(0, 18, WINDOW_WIDTH, 8);
                                System.Threading.Thread.Sleep(100);
                                Info(new FileInfo(currentDir + @"\" + commandParams[1]));
                            } else if (Directory.Exists(currentDir + @"\" + commandParams[1])) 
                            {
                                DrawWindow(0, 18, WINDOW_WIDTH, 8);
                                System.Threading.Thread.Sleep(100);
                                DIRInfo(new DirectoryInfo(currentDir + @"\" + commandParams[1]));
                            } 
                        }
                        break;
                    case "del":
                        if (commandParams.Length > 1)
                        {
                            if (File.Exists(currentDir + @"\" + commandParams[1]))
                            {
                                File.Delete(currentDir + @"\" + commandParams[1]);
                            }
                            else if (Directory.Exists(currentDir + @"\" + commandParams[1]))
                            {
                                DelDirectory(currentDir + @"\" + commandParams[1]);
                            }
                        }
                        break;
                    case "mv":
                    if (commandParams.Length > 2) {
                        if (File.Exists(currentDir + @"\" + commandParams[1]))
                        {
                            if (Directory.Exists(commandParams[2]))
                            {


                                    File.Create(commandParams[2] + @"\" + commandParams[1]).Dispose();
                                    File.Copy(currentDir + @"\" + commandParams[1], (commandParams[2] + @"\" + commandParams[1]), true);
                                    File.Delete(currentDir + @"\" + commandParams[1]);

                            }
                            
                            else if (!Directory.Exists(currentDir + @"\" + commandParams[2]))
                            {
                                Directory.CreateDirectory(commandParams[2]);
                                File.Create(commandParams[2] + @"\" + commandParams[1]).Dispose();
                                File.Copy(commandParams[1], (commandParams[2] + @"\" + commandParams[1]), true);
                                File.Delete(commandParams[1]);
                            }
                        }
                        else if (Directory.Exists(currentDir + @"\" + commandParams[1]))
                        {
                            if (!Directory.Exists(commandParams[2]))
                            {
                                    Directory.CreateDirectory(commandParams[2]);
                                    CopyDirectory(currentDir + @"\" + commandParams[1], commandParams[2], true);
                                    DelDirectory(currentDir + @"\" + commandParams[1]);
                            } else
                            {
                                    CopyDirectory(currentDir + @"\" + commandParams[1], commandParams[2], true);
                                    DelDirectory(currentDir + @"\" + commandParams[1]);
                            }


                        }
                    }   
                    break;
                    case "mkdir":
                        if (commandParams.Length > 1)
                        {

                            if (!Directory.Exists(commandParams[1]))
                            {
                                Directory.CreateDirectory(currentDir + @"\" + commandParams[1]);
                            }
                        }
                        break;
                    case "touch":
                        if (commandParams.Length != 0) {
                            if (!File.Exists(commandParams[1]))
                            {
                                File.Create(currentDir + @"\" + commandParams[1]).Dispose();
                            }
                        }
                        break;
                    case "save":
                        Properties.Settings.Default.Save();
                        break;
                    case "load":
                        currentDir = Properties.Settings.Default.LastPath;
                        break;
                    case "exit":
                        Properties.Settings.Default.Save();
                        System.Environment.Exit(0);
                        break;
                    case "page-items":
                        if (commandParams.Length > 1)
                        {
                            if (uint.TryParse(commandParams[1], out uint pageElements))
                            {
                                Properties.Settings.Default.Elements = pageElements;
                                Properties.Settings.Default.Save();
                            }
                        }
                        break;
                    
                }
            }
            UpdateConsole();
        }
        // Копирование каталога и подкаталогов
        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            
            var dir = new DirectoryInfo(sourceDir);


            if (!dir.Exists)
            {
                return;
            }
            
            DirectoryInfo[] dirs = dir.GetDirectories();

            
            Directory.CreateDirectory(destinationDir);

        
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
        // Рекурсивное удаление каталога (моё)
        static void DelDirectory(string sourceDir)
        {

            var dir = new DirectoryInfo(sourceDir);


            if (!dir.Exists)
            {
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();


            


            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(sourceDir, file.Name);
                file.Delete();
            }


            
            foreach (DirectoryInfo subDir in dirs)
            {
                   
                    DelDirectory(subDir.FullName);
            }
            Directory.Delete(sourceDir);
            
        }
        // Получение краткой строки пути
        static string GetShortPath(string path)
        {
            StringBuilder shortPathName = new StringBuilder((int)API.MAX_PATH);
            API.GetShortPathName(path, shortPathName, API.MAX_PATH);
            return shortPathName.ToString();
        }



        /// <summary>
        /// Обновление ввода с консоли
        /// </summary>
        static void UpdateConsole()
        {
            DrawConsole(GetShortPath(currentDir), 0, 26, WINDOW_WIDTH, 3);
            ProcessEnterCommand(WINDOW_WIDTH);
        }

        
        /// <summary>
        /// Отрисовка консоли
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        static void DrawConsole(string dir, int x, int y, int width, int height)
        {
            DrawWindow(x, y, width, height);
            Console.SetCursorPosition(x + 1, y + height / 2);
            Console.Write($"{dir}>");
        }

        /// <summary>
        /// Отрисовка окна
        /// </summary>
        /// <param name="x">Начальная позиция по оси X</param>
        /// <param name="y">Начальная позиция по оси Y</param>
        /// <param name="width">Ширина окна</param>
        /// <param name="height">Высота окна</param>
        static void DrawWindow(int x, int y, int width, int height)
        {
            // header - шапка
            Console.SetCursorPosition(x, y);
            Console.Write("╔");
            for (int i = 0; i < width - 2; i++)
                Console.Write("═");
            Console.Write("╗");


            // window - окно
            Console.SetCursorPosition(x, y + 1);
            for (int i = 0; i < height - 2; i++)
            {
                Console.Write("║");

                for (int j = x + 1; j < x + width - 1; j++)
                    Console.Write(" ");

                Console.Write("║");
            }

            // footer - подвал
            Console.Write("╚");
            for (int i = 0; i < width - 2; i++)
                Console.Write("═");
            Console.Write("╝");
            Console.SetCursorPosition(x, y);

        }

        /// <summary>
        /// Отрисовать дерево каталогов
        /// </summary>
        /// <param name="dir">Директория</param>
        /// <param name="page">Страница</param>
        static void DrawTree(DirectoryInfo dir, int page, bool useSettingsFile)
        {
            StringBuilder tree = new StringBuilder();
            if (useSettingsFile)
            {
                GetTree(tree, dir, "", true, true);
            } else
            {
                GetTree(tree, dir, "", true, false);
            }
            DrawWindow(0, 0, WINDOW_WIDTH, 18);
            (int currentLeft, int currentTop) = GetCursorPosition();
            int pageLines = 16;
            string[] lines = tree.ToString().Split('\n');
            int pageTotal = (lines.Length + pageLines - 1) / pageLines;
            if (page > pageTotal)
                page = pageTotal;

            for (int i = (page - 1)*pageLines, counter = 0;  i < page*pageLines; i++, counter++)
            {
                if (lines.Length - 1 > i)
                {
                    Console.SetCursorPosition(currentLeft + 1, currentTop + 1 + counter);
                    Console.WriteLine(lines[i]);
                }
            }

            // Отрисуем footer
            string footer = $"╡ {page} of {pageTotal} ╞";
            Console.SetCursorPosition(WINDOW_WIDTH / 2 - footer.Length / 2, 17);
            Console.WriteLine(footer);

        }

        // Генерация дерева
        static void GetTree(StringBuilder tree, DirectoryInfo dir, string indent, bool lastDirectory, bool useSettingsFile)
        {
			try {
            tree.Append(indent);
            if (lastDirectory)
            {
                tree.Append("└─");
                indent += "  ";
            }
            else
            {
                tree.Append("├─");
                indent += "│ ";
            }

            tree.Append($"{dir.Name}\n");


            FileInfo[] subFiles = dir.GetFiles();
            if (useSettingsFile)
                Array.Resize(ref subFiles, (int)Properties.Settings.Default.Elements);
            if (!useSettingsFile)
            {
                for (int i = 0; i < subFiles.Length; i++)
                {
					
                    if (i == subFiles.Length - 1)
                    {
						
							tree.Append($"{indent}└─{subFiles[i].Name}\n");
						
                    }
                    else
                    {
						
							tree.Append($"{indent}├─{subFiles[i].Name}\n");
						
                    }
					
                }
            }
            else
            {

                int i = 0;
      
                    do
                    {
					
                    if (i == subFiles.Length - 1)
                    {
						if (subFiles[i] != null)
							tree.Append($"{indent}└─{subFiles[i].Name}\n");
						
                    }
                    else
                    {
						if (subFiles[i] != null)
							tree.Append($"{indent}├─{subFiles[i].Name}\n");
						
                    }
					
                    i++;
                    } while (subFiles != null && i < subFiles.Length);
                
            }

            DirectoryInfo[] subDirects = dir.GetDirectories();
			
            for (int i = 0; i < subDirects.Length; i++) {
				
			try {
				GetTree(tree, subDirects[i], indent, i == subDirects.Length - 1, useSettingsFile);
				
			} catch (UnauthorizedAccessException) {
				GetTree(tree, subDirects[i + 1], indent, i == subDirects.Length - 1, useSettingsFile);
			} catch (Exception) {
				Console.Write("XX");
			}
			}
			} catch {
				;
			}
        }
    }
}
